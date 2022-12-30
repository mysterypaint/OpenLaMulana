using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;

namespace OpenLaMulana.Entities
{
    public partial class World : IGameEntity
    {
        /*
         * In-game flags can be used from 0 to 7999. There are three types of flags, each with different behavior.

Numbers 0 to 39: Even if it is turned on, it will be reset if you move the room. It is suitable for the type of device that switches screens and starts over again. It is also used as a spare flag when constructing a complicated device.

Numbers 40-6999: Normal flags. On/off information is memorized when saving. It is used for devices that become clear once solved.

Numbers 7000 to 7999: The state is recorded during the game, but it is not reflected in the save data. Even if it is on, it will be turned off when loading. Mainly used for pots that respawn when loaded.

There are also flags monitored by the system in-game. They are deeply involved in the mystery of the game, so please do not change the usage of the following items. See game specs, flags for details.

Please refer to the LA-MULANA Flag List for the list of flags used in the actual game.
　
        */

        public int DrawOrder { get; set; }
        public const int CHIP_SIZE = 8;
        public const int FIELD_WIDTH = 4;
        public const int FIELD_HEIGHT = 5;
        public const int ROOM_WIDTH = 32;  // How many 8x8 tiles a room is wide
        public const int ROOM_HEIGHT = 22; // How many 8x8 tiles a room is tall
        public const int HUD_HEIGHT = CHIP_SIZE * 2;
        public const int ANIME_TILES_BEGIN = 1160;

        public enum SpecialChipTypes
        {
            BACKGROUND = 0,
            LEFT_SIDE_OF_STAIRS = 1,
            ASCENDING_SLOPE = 2,
            ASCENDING_SLOPE_LEFT = 3,
            ASCENDING_STAIRS_RIGHT = 4,
            ASCENDING_STAIRS_LEFT = 5,
            ASCENDING_RIGHT_BEHIND_STAIRS = 6,
            ASCENDING_BACK_LEFT = 7,
            ICE_SLOPE_RIGHT = 12,
            ICE_SLOPE_LEFT = 13,
            WATER = 64,
            WATER_STREAM_UP = 65,
            WATER_STREAM_RIGHT = 66,
            WATER_STREAM_DOWN = 67,
            WATER_STREAM_LEFT = 68,
            LAVA = 69,
            WATERFALL = 76,
            CLINGABLE_WALL = 128,
            ICE = 129,
            UNCLINGABLE_WALL = 255,
            MAX
        };

        public int CurrField { get; set; } = 1;
        public int CurrViewX, CurrViewY, FieldCount = 0;
        private int[] _currChipLine;
        private List<Field> _fields;
        private List<Texture2D> _fieldTextures, _bossTextures, _otherTextures;
        internal static Texture2D _genericEntityTex;
        private ActiveView[] activeViews;

        public enum VIEW_DEST
        {
            WORLD,
            FIELD,
            X,
            Y,
            MAX
        };


        public enum AViews
        {
            CURR = 0,
            NEXT = 1,
            MAX
        };

        public enum VIEW_DIR
        {
            UP,
            RIGHT,
            DOWN,
            LEFT,
            SELF,
            MAX
        };

        public World(Texture2D _gameFontTex, Texture2D genericEntityTex)
        {
            _fields = new List<Field>();

            Global.TextManager = new TextManager(_gameFontTex);

            _genericEntityTex = genericEntityTex;

            activeViews = new ActiveView[(int)AViews.MAX];
            for (var i = 0; i < activeViews.Length; i++)
            {
                activeViews[i] = new ActiveView();
            }

            // Define the font table for the game
            Dictionary<int, string> s_charSet = Global.TextManager.GetCharSet();
            s_charSet = PseudoXML.DefineCharSet(s_charSet);

            string jpTxtFile = "Content/data/script_JPN_UTF8.txt";
            string engTxtFile = "Content/data/script_ENG_UTF8.txt";

            // Decrypt "Content/data/script.dat" and the English-Translated counterpart file
            PseudoXML.DecodeScriptDat("Content/data/script.dat", jpTxtFile, s_charSet, this, Global.Languages.Japanese);
            PseudoXML.DecodeScriptDat("Content/data/script_ENG.dat", engTxtFile, s_charSet, this, Global.Languages.English);

            string[] data = File.ReadAllLines(jpTxtFile);

            ParseXmlRecursive(this, data, 0);

            FieldCount = _fields.Count;

            if (File.Exists(engTxtFile))
            {
                File.Delete(engTxtFile);
            }
            if (File.Exists(jpTxtFile))
            {
                File.Delete(jpTxtFile);
            }

            foreach (Field f in _fields)
            {
                f.InitializeArea();
            }

            _currChipLine = _fields[CurrField].GetChipline();
        }

        public Field GetField(int index)
        {
            return _fields[index];
        }

        public void InitWorldEntities()
        {
            Field thisField = _fields[CurrField];
            var thisFieldMapData = thisField.GetMapData();
            View thisView = thisFieldMapData[CurrViewX, CurrViewY];
            activeViews[(int)AViews.CURR].SetView(thisView);
            activeViews[(int)AViews.NEXT].SetView(thisView);

            //UpdateEntities(CurrField, thisField, thisView, CurrViewX, CurrViewY);
        }

        // Returns new currentLine; end when it returns -1
        public int ParseXmlRecursive(IGameEntity currentObject, string[] xml, int currentLine)
        {
            if (currentLine >= xml.Length - 1)
                return -1;

            string line, type;
            int[] args;
            Field currField;
            ObjectSpawnData currObjSpawnData = null;
            View currView = null;
            View[,] fieldViews = null;
            int numFields = 0;
            int numWorlds = 0;

            do
            {
                // "<MAP 1,3,13>"

                line = xml[currentLine].Trim();
                // "<MAP"
                string splitTrimmedLine = line.Split(" ")[0].Trim();

                if (splitTrimmedLine.Length <= 0)
                {
                    //ParseXmlRecursive(currentObject, xml, currentLine + 1);
                    type = "Undefined";
                    currentLine++;
                    continue;
                }
                else
                {
                    // "MAP"
                    type = splitTrimmedLine.Substring(1, splitTrimmedLine.Length - 1);
                }

                switch (type)
                {
                    case "FIELD":
                        args = parseArgs(line);
                        Field f = new Field(args[0], args[1], args[3], args[4], Global.EntityManager, Global.TextManager, this, args[2], numFields, numWorlds);
                        numFields++;
                        _fields.Add(f);
                        fieldViews = f.GetMapData();
                        currView = null;
                        currentObject = f;
                        //currentLine = ParseXmlRecursive(f, xml, currentLine + 1); // Until we start defining maps for this field, every single object spawn data will be considered a Field object. So, "currentView" is null by default
                        currentLine++;
                        break;
                    case "CHIPLINE":
                        args = parseArgs(line);
                        currField = (Field)currentObject;
                        currField.SetChipline(args[0], args[1]);
                        currentLine++;
                        break;
                    case "HIT":
                        args = parseArgs(line);
                        currField = (Field)currentObject;
                        currField.AddHit(args[0], args[1]);
                        currentLine++;
                        break;
                    case "ANIME":
                        args = parseArgs(line);
                        currField = (Field)currentObject;
                        currField.DefineAnimatedTile(args);
                        currentLine++;
                        break;
                    case "OBJECT":
                        args = parseArgs(line);
                        currField = (Field)currentObject;
                        // Spawn a Field-global entity or spawn a View-specific entity, depending on if currView == null
                        currObjSpawnData = currField.DefineObjectSpawnData(args[0], args[1], args[2], args[3], args[4], args[5], args[6], currView);
                        currentLine++;
                        break;
                    case "START":
                        args = parseArgs(line);
                        currField = (Field)currentObject;

                        // Regardless this object was global to the Field or specific to a View, we still remembered which object we're referring to
                        currObjSpawnData.AddStartFlag(args[0], Convert.ToBoolean(args[1]));
                        currentLine++;
                        break;
                    case "MAP":
                        // A map has been defined. We are no longer writing objects global to the Field, but rather this specific "View" (aka Screen/"Map")
                        args = parseArgs(line);
                        int roomX = args[0];
                        int roomY = args[1];
                        int roomNumber = args[2];
                        currField = (Field)currentObject;
                        currView = currField.GetMapData()[roomX, roomY];
                        currView.RoomNumber = roomNumber;
                        currentLine++;
                        break;
                    case "UP":
                        args = parseArgs(line);
                        currView.DefineViewDestination(VIEW_DIR.UP, args[0], args[1], args[2], args[3]);
                        currentLine++;
                        break;
                    case "RIGHT":
                        args = parseArgs(line);
                        currView.DefineViewDestination(VIEW_DIR.RIGHT, args[0], args[1], args[2], args[3]);
                        currentLine++;
                        break;
                    case "DOWN":
                        args = parseArgs(line);
                        currView.DefineViewDestination(VIEW_DIR.DOWN, args[0], args[1], args[2], args[3]);
                        currentLine++;
                        break;
                    case "LEFT":
                        args = parseArgs(line);
                        currView.DefineViewDestination(VIEW_DIR.LEFT, args[0], args[1], args[2], args[3]);
                        currentLine++;
                        break;
                    //case "TALK": // We don't care about the dialogue because it is already stored by this point, in all supported languages
                    //case "WORLD": // Don't need more than one world...
                    //    numWorlds++;
                    //    break;
                    //default:
                    //    currentLine = ParseXmlRecursive(currentObject, xml, currentLine + 1);
                    //    break;
                    default:
                        currentLine++;
                        break;
                }
            }
            while (currentLine < xml.Length - 1);
            //while (!line.StartsWith("</"));
            return currentLine;
        }

        private int[] parseArgs(string line)
        {
            string[] strArgs = line.Split(" ")[1].Split(",");
            int[] outArgs = new int[strArgs.Length];
            strArgs[strArgs.Length - 1] = strArgs[strArgs.Length - 1].Substring(0, strArgs[strArgs.Length - 1].Length - 1);

            int i = 0;
            foreach (string s in strArgs)
            {
                int.TryParse(s, out outArgs[i]);
                i++;
            }
            return outArgs;
        }

        internal void InitGameText(Global.Languages lang, List<string> data)
        {
            Global.TextManager.SetDialogue(lang, data);
        }

        public void Update(GameTime gameTime)
        {
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            foreach (ActiveView a in activeViews)
            {
                // Do not attempt to draw null
                if (a == null)
                    continue;

                // Only draw the current active view if the Camera is not moving
                if (a != activeViews[(int)AViews.CURR]) {
                    if (Global.Camera.GetState() == Camera.CamStates.NONE)
                        continue;
                }

                // Draw the current view
                a.DrawView(spriteBatch, gameTime);
            }
        }

        public void FieldTransitionCardinal(VIEW_DIR movingDirection)
        {
            // Camera is busy; Do not transition.
            if (Global.Camera.GetState() != Camera.CamStates.NONE)
                return;

            // Grab the View we are transitioning to
            Field thisField = _fields[CurrField];
            Global.Textures correctedTexID = Global.TextureManager.GetMappedWorldTexID(thisField.MapGraphics);
            var thisFieldTex = Global.TextureManager.GetTexture(correctedTexID);
            var thisFieldMapData = thisField.GetMapData();
            View thisView = thisFieldMapData[CurrViewX, CurrViewY];
            int[] viewDest = thisView.GetDestinationView(movingDirection);


            // Remember the parameters that our destination View contains
            int destField = viewDest[(int)VIEW_DEST.FIELD];
            int destViewX = viewDest[(int)VIEW_DEST.X];
            int destViewY = viewDest[(int)VIEW_DEST.Y];


            // Do not transition if the destination field goes out of the map bounds (4x5)
            if (destField < 0 || destViewX < 0 || destViewY < 0 || destViewX > FIELD_WIDTH - 1 || destViewY > FIELD_HEIGHT - 1)
                return;

            // Determine the next field and its texture
            Field nextField = _fields[destField];
            correctedTexID = Global.TextureManager.GetMappedWorldTexID(nextField.MapGraphics);
           var nextFieldTex = Global.TextureManager.GetTexture(correctedTexID);

            // Set the transitioning Active View to the next field + its texture

            ActiveView nextAV = activeViews[(int)AViews.NEXT];

            switch (movingDirection)
            {
                case World.VIEW_DIR.LEFT:
                    nextAV.Position = new Vector2(-(World.ROOM_WIDTH * World.CHIP_SIZE), 0);
                    break;
                case World.VIEW_DIR.DOWN:
                    nextAV.Position = new Vector2(0, (World.ROOM_HEIGHT * World.CHIP_SIZE) * 1);
                    break;
                case World.VIEW_DIR.RIGHT:
                    nextAV.Position = new Vector2((World.ROOM_WIDTH * World.CHIP_SIZE), 0);
                    break;
                case World.VIEW_DIR.UP:
                    nextAV.Position = new Vector2(0, -(World.ROOM_HEIGHT * World.CHIP_SIZE));
                    break;
            }

            nextAV.SetField(nextField);
            nextAV.SetFieldTex(nextFieldTex);

            var nextFieldMapData = nextField.GetMapData();
            View nextView = nextFieldMapData[destViewX, destViewY];
            nextAV.SetView(nextView);

            // Slide the camera toward the new field
            Global.Camera.UpdateMoveTarget(movingDirection);


            // If we're moving to a new Field, get rid of all the entities from the previous Field
            if (CurrField != destField)
            {
                thisField.DeleteAllFieldAndRoomEntities();
                thisField.UnlockAllViewSpawning(); // Give permission back to all the views to allow them to spawn entities
            }
            else // Otherwise, if moving to a new View within the same Field, delete the older spawns, but only if they do not share the same RoomNumber/Region as the last View we were in
                thisField.DeleteOldRoomEntities(thisView, thisField.GetView(destViewX, destViewY));

            // Old entities have been removed (if applicable). Now, our current (and next) Field+View are the destination Field+View
            CurrField = destField;
            CurrViewX = destViewX;
            CurrViewY = destViewY;

            // Change the BGM, if applicable
            Global.AudioManager.ChangeSongs(_fields[destField].MusicNumber);
            
            //UpdateEntities(destField, thisField, thisView, destViewX, destViewY);
        }

        private void UpdateEntities(int destField, Field thisField, View thisView, int destViewX, int destViewY)
        {

            Field nextField = _fields[destField];
            var nextFieldMapData = nextField.GetMapData();
            View nextView = nextFieldMapData[destViewX, destViewY];

            // Finally, spawn the new entities for the destination View, but let the destination Field keep track of ALL of the entities (Field Entities, View Entities)
            nextField.SpawnEntities(nextView, nextField, thisView, thisField); // "thisField" was the previous Field we were on, regardless if we moved Fields or not
        }

        internal TextManager GetTextManager()
        {
            return Global.TextManager;
        }

        internal void UpdateCurrActiveView()
        {
            activeViews[(int)AViews.CURR].SetView(activeViews[(int)AViews.NEXT].GetView());
            activeViews[(int)AViews.CURR].SetField(activeViews[(int)AViews.NEXT].GetField());
            activeViews[(int)AViews.CURR].SetFieldTex(activeViews[(int)AViews.NEXT].GetFieldTex());
        }

        internal void FieldTransitionPixelate(int warpType, int destField, int destViewX, int destViewY)
        {
            // Camera is busy; Do not transition.
            if (Global.Camera.GetState() != Camera.CamStates.NONE)
                return;

            // Grab the View we are transitioning to
            Field thisField = _fields[CurrField];
            var thisFieldTex = _fieldTextures[thisField.MapGraphics];
            var thisFieldMapData = thisField.GetMapData();
            View thisView = thisFieldMapData[CurrViewX, CurrViewY];


            // Do not transition if the destination field goes out of the map bounds (4x5)
            if (destField < 0 || destViewX < 0 || destViewY < 0 || destViewX > FIELD_WIDTH - 1 || destViewY > FIELD_HEIGHT - 1)
                return;

            // Determine the next field and its texture
            Field nextField = _fields[destField];
            var nextFieldTex = _fieldTextures[nextField.MapGraphics];

            // Set the transitioning Active View to the next field + its texture

            ActiveView nextAV = activeViews[(int)AViews.NEXT];

            switch (World.VIEW_DIR.RIGHT)
            {
                case World.VIEW_DIR.LEFT:
                    nextAV.Position = new Vector2(-(World.ROOM_WIDTH * World.CHIP_SIZE), 0);
                    break;
                case World.VIEW_DIR.DOWN:
                    nextAV.Position = new Vector2(0, (World.ROOM_HEIGHT * World.CHIP_SIZE) * 1);
                    break;
                case World.VIEW_DIR.RIGHT:
                    nextAV.Position = new Vector2((World.ROOM_WIDTH * World.CHIP_SIZE), 0);
                    break;
                case World.VIEW_DIR.UP:
                    nextAV.Position = new Vector2(0, -(World.ROOM_HEIGHT * World.CHIP_SIZE));
                    break;
            }

            nextAV.SetField(nextField);
            nextAV.SetFieldTex(nextFieldTex);

            var nextFieldMapData = nextField.GetMapData();
            View nextView = nextFieldMapData[destViewX, destViewY];
            nextAV.SetView(nextView);

            // Slide the camera toward the new field
            Global.Camera.UpdateMoveTarget(World.VIEW_DIR.RIGHT);


            // If we're moving to a new Field, get rid of all the entities from the previous Field
            if (CurrField != destField)
            {
                thisField.DeleteAllFieldAndRoomEntities();
                thisField.UnlockAllViewSpawning(); // Give permission back to all the views to allow them to spawn entities
            }
            else // Otherwise, if moving to a new View within the same Field, delete the older spawns, but only if they do not share the same RoomNumber/Region as the last View we were in
                thisField.DeleteOldRoomEntities(thisView, thisField.GetView(destViewX, destViewY));

            // Old entities have been removed (if applicable). Now, our current (and next) Field+View are the destination Field+View
            CurrField = destField;
            CurrViewX = destViewX;
            CurrViewY = destViewY;

            // Change the BGM, if applicable
            Global.AudioManager.ChangeSongs(_fields[destField].MusicNumber);

            //UpdateEntities(destField, thisField, thisView, destViewX, destViewY);
        }
    }
}