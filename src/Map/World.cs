using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using static OpenLaMulana.Global;
using static OpenLaMulana.System.Camera;

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


        public enum ShaderDrawingState
        {
            NO_SHADER = 0,
            CURR_VIEW,
            DEST_VIEW,
            TRANSITION_LAYER,
            OUTPUT_1X,
            OUTPUT,
            MAX
        }

        public int DrawOrder { get; set; }
        public Effect ActiveShader { get; set; } = null;
        public const int CHIP_SIZE = 8;
        public const int FIELD_WIDTH = 4;
        public const int FIELD_HEIGHT = 5;
        public const int ROOM_WIDTH = 32;  // How many 8x8 tiles a room is wide
        public const int ROOM_HEIGHT = 22; // How many 8x8 tiles a room is tall
        public const int HUD_HEIGHT = CHIP_SIZE * 2;
        public const int ANIME_TILES_BEGIN = 1160;
        private View _transitionView = new View(ROOM_WIDTH, ROOM_HEIGHT, null, 0, 0);
        private ShaderDrawingState _shdState = ShaderDrawingState.NO_SHADER;

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
        public int CurrViewX = 3, CurrViewY = 1, FieldCount = 0;
        private int[] _currChipLine;
        private List<Field> _fields;
        private ActiveView[] _activeViews;
        private RenderTarget2D _bkgRenderTarget, _transitionRenderTarget, _destRenderTarget, _outputRenderTarget;
        private bool _abortDrawing = false;
        private bool _disposedRenderTargetsFlag = false;
        private bool _drawBossRoom = false;

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
            DEST = 1,
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

        public World()
        {
            _fields = new List<Field>();

            Texture2D gameFontTex = Global.TextureManager.GetTexture(Global.Textures.FONT_EN);
            Global.TextManager = new TextManager(gameFontTex);

            _activeViews = new ActiveView[(int)AViews.MAX];
            for (var i = 0; i < _activeViews.Length; i++)
            {
                _activeViews[i] = new ActiveView();
            }

            // Define the font table for the game
            Dictionary<int, string> s_charSet = Global.TextManager.GetCharSet();
            s_charSet = PseudoXML.DefineCharSet(s_charSet);

            string jpTxtFile = "Content/data/script_JPN_UTF8.txt";
            string engTxtFile = "Content/data/script_ENG_UTF8.txt";

            // Decrypt "Content/data/script.dat" and the English-Translated counterpart file
            PseudoXML.DecodeScriptDat("Content/data/script.dat", jpTxtFile, s_charSet, this, Global.Languages.Japanese);
            PseudoXML.DecodeScriptDat("Content/data/script_EN.dat", engTxtFile, s_charSet, this, Global.Languages.English);

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
                f.InitializeBossRoom();
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
            _activeViews[(int)AViews.DEST].SetView(thisView);
            _activeViews[(int)AViews.DEST].SetField(thisField);
            var correctedTexID = Global.TextureManager.GetMappedWorldTexID(thisField.MapGraphics);
            var nextFieldTex = Global.TextureManager.GetTexture(correctedTexID);
            _activeViews[(int)AViews.DEST].SetFieldTex(nextFieldTex);
            UpdateCurrActiveView();
            UpdateEntities(CurrField, thisField, thisView, CurrViewX, CurrViewY, Vector2.Zero);
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
                        _fields.Add(f);
                        numFields++;
                        currView = null;
                        currentObject = f;
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
                        if (currView.GetParentField().HasEnglishField()) {
                            View enView = currView.GetParentField().GetMapData()[currView.X, currView.Y];
                            enView.DefineViewDestination(VIEW_DIR.UP, args[0], args[1], args[2], args[3]);
                        }
                        currentLine++;
                        break;
                    case "RIGHT":
                        args = parseArgs(line);
                        currView.DefineViewDestination(VIEW_DIR.RIGHT, args[0], args[1], args[2], args[3]);
                        if (currView.GetParentField().HasEnglishField())
                        {
                            View enView = currView.GetParentField().GetMapData()[currView.X, currView.Y];
                            enView.DefineViewDestination(VIEW_DIR.RIGHT, args[0], args[1], args[2], args[3]);
                        }
                        currentLine++;
                        break;
                    case "DOWN":
                        args = parseArgs(line);
                        currView.DefineViewDestination(VIEW_DIR.DOWN, args[0], args[1], args[2], args[3]);
                        if (currView.GetParentField().HasEnglishField())
                        {
                            View enView = currView.GetParentField().GetMapData()[currView.X, currView.Y];
                            enView.DefineViewDestination(VIEW_DIR.DOWN, args[0], args[1], args[2], args[3]);
                        }
                        currentLine++;
                        break;
                    case "LEFT":
                        args = parseArgs(line);
                        currView.DefineViewDestination(VIEW_DIR.LEFT, args[0], args[1], args[2], args[3]);
                        if (currView.GetParentField().HasEnglishField())
                        {
                            View enView = currView.GetParentField().GetMapData()[currView.X, currView.Y];
                            enView.DefineViewDestination(VIEW_DIR.LEFT, args[0], args[1], args[2], args[3]);
                        }
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
            Global.GraphicsDevice.SetRenderTarget(null);
            foreach (ActiveView a in _activeViews)
            {
                // Do not attempt to draw null
                if (a == null)
                    continue;

                // Only draw the current active view if the Camera is not moving
                if (a != _activeViews[(int)AViews.CURR])
                {
                    if (Global.Camera.GetState() == CamStates.NONE)
                        continue;
                }

                // Draw the current view
                a.DrawView(spriteBatch, gameTime);
            }
        }

        public void DrawPixelate(SpriteBatch spriteBatch, GameTime gameTime, ShaderDrawingState shaderState)
        {
            if (_abortDrawing)
            {
                // Make sure we draw normally for this frame
                if (shaderState == ShaderDrawingState.OUTPUT)
                    Draw(spriteBatch, gameTime);

                if (_disposedRenderTargetsFlag)
                    return;
                Global.GraphicsDevice.SetRenderTarget(null);
                Global.Camera.SetState((int)CamStates.NONE);
                ActiveShader = null;
                _bkgRenderTarget.Dispose();
                _transitionRenderTarget.Dispose();
                _destRenderTarget.Dispose();
                _outputRenderTarget.Dispose();
                _bkgRenderTarget = null;
                _transitionRenderTarget = null;
                _destRenderTarget = null;
                _outputRenderTarget = null;
                _disposedRenderTargetsFlag = true;
                shaderState = ShaderDrawingState.NO_SHADER;
                return;
            }

            switch (shaderState)
            {
                default:
                case ShaderDrawingState.CURR_VIEW:
                    // Draw the current View without any shaders, if it's not null
                    Global.GraphicsDevice.SetRenderTarget(_bkgRenderTarget);
                    Global.GraphicsDevice.Clear(Color.Black);
                    ActiveView currView = _activeViews[(int)AViews.CURR];
                    if (currView != null)
                        currView.DrawView(spriteBatch, gameTime);
                    break;
                case ShaderDrawingState.TRANSITION_LAYER:
                    // Draw all of the raw transition tiles to a render target
                    Global.GraphicsDevice.SetRenderTarget(_transitionRenderTarget);

                    Global.GraphicsDevice.Clear(Color.Transparent);


                    //GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
                    //spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.PointClamp, null, null, effect);



                    // Loop through every single Room[_x][_y] Chip to draw every single Chip in a given room
                    Texture2D tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);

                    for (int y = 0; y < World.ROOM_HEIGHT; y++)
                    {
                        for (int x = 0; x < World.ROOM_WIDTH; x++)
                        {
                            // Grab the current Chip (tile) we're looking at
                            Chip thisChip = _transitionView.Chips[x, y];

                            // Handle animated Chips here
                            var animeSpeed = thisChip.AnimeSpeed;
                            var animeFrames = thisChip.GetAnimeFrames();
                            var maxFrames = animeFrames.Length;

                            if (animeSpeed > 0)
                            {
                                if (gameTime.TotalGameTime.Ticks % (animeSpeed * 6) == 0)
                                {
                                    thisChip.CurrFrame++;

                                    // Play the animation once, then stop and turn off the shader
                                    if (thisChip.CurrFrame >= maxFrames)
                                    {
                                        thisChip.CurrFrame = maxFrames - 1;
                                        thisChip.AnimeSpeed = 0;
                                        _abortDrawing = true;
                                        UpdateCurrActiveView();
                                        return;
                                    }
                                }
                            }

                            var drawingTileID = animeFrames[(int)thisChip.CurrFrame];

                            //spriteBatch.Draw(_fieldTextures[currField], new Vector2(0, 0), new Rectangle(16, 16, tileWidth, tileHeight), Color.White);
                            var posX = (x * 8);
                            var posY = (y * 8);
                            var texX = (drawingTileID % 40) * 8;
                            var texY = (drawingTileID / 40) * 8;

                            spriteBatch.Draw(tex, new Vector2(posX, posY + HUD_HEIGHT), new Rectangle(texX, texY, World.CHIP_SIZE, World.CHIP_SIZE), Color.White);
                        }
                    }
                    break;
                case ShaderDrawingState.DEST_VIEW:
                    Global.GraphicsDevice.SetRenderTarget(_destRenderTarget);
                    Global.GraphicsDevice.Clear(Color.Transparent);

                    // Draw the destination View with the shader enabled, if it's not null
                    ActiveView destView = _activeViews[(int)AViews.DEST];
                    if (destView != null)
                        destView.DrawView(spriteBatch, gameTime);
                    break;

                case ShaderDrawingState.OUTPUT_1X:
                    // Set the render target to draw everything at a 1x resolution
                    Global.GraphicsDevice.SetRenderTarget(_outputRenderTarget);
                    // Force everything to be pixel-perfect
                    Global.GraphicsDevice.SamplerStates[0] = SamplerState.PointClamp;
                    Global.GraphicsDevice.SamplerStates[1] = SamplerState.PointClamp;
                    Global.GraphicsDevice.SamplerStates[2] = SamplerState.PointClamp;

                    // Pass the other render targets into this temporary 1x output target to the shader
                    ActiveShader.Parameters["PrevViewTexture"].SetValue(_bkgRenderTarget);       // Snapshot of the Previous View
                    ActiveShader.Parameters["MaskTexture"].SetValue(_transitionRenderTarget);    // Masking window (the entire [relevant] screen), animated
                    spriteBatch.Draw(_destRenderTarget, _destRenderTarget.Bounds, Color.White);             // The Destination View, which is also the current Sampling Texture because of the order we are drawing in
                    break;
                case ShaderDrawingState.OUTPUT:
                    // Finally, output the 1x resolution to the final screen output, but blow it up to actual size
                    Global.GraphicsDevice.SetRenderTarget(null);
                    spriteBatch.Draw(_outputRenderTarget, _outputRenderTarget.Bounds, Color.White);
                    break;
            }
        }

            public void CreateRenderTargets()
        {
            _bkgRenderTarget = new RenderTarget2D(
                Global.GraphicsDevice,
                Main.WINDOW_WIDTH,
                Main.WINDOW_HEIGHT,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
            _transitionRenderTarget = new RenderTarget2D(
                Global.GraphicsDevice,
                Main.WINDOW_WIDTH,
                Main.WINDOW_HEIGHT,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
            _destRenderTarget = new RenderTarget2D(
                Global.GraphicsDevice,
                Main.WINDOW_WIDTH,
                Main.WINDOW_HEIGHT,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
            _outputRenderTarget = new RenderTarget2D(
                Global.GraphicsDevice,
                Main.WINDOW_WIDTH,
                Main.WINDOW_HEIGHT,
                false,
                SurfaceFormat.Color,
                DepthFormat.None);
        }

        public void FieldTransitionCardinal(VIEW_DIR movingDirection)
        {
            _drawBossRoom = false;

            // Camera is busy; Do not transition.
            if (Global.Camera.GetState() != CamStates.NONE)
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

            ActiveView nextAV = _activeViews[(int)AViews.DEST];

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
                // Give permission back to all the views to allow them to spawn entities
                thisField.DeleteAllFieldAndRoomEntities();
                thisField.UnlockAllViewSpawning();
                thisField.ClearVisitedViews(); 
                nextField.DeleteAllFieldAndRoomEntities();
                nextField.UnlockAllViewSpawning();
                nextField.ClearVisitedViews();
            }
            else // Otherwise, if moving to a new View within the same Field, delete the older spawns, but only if they do not share the same RoomNumber/Region as the last View we were in
                thisField.DeleteOldRoomEntities(thisView, thisField.GetView(destViewX, destViewY));

            // Old entities have been removed (if applicable). Now, our current (and next) Field+View are the destination Field+View
            CurrField = destField;
            CurrViewX = destViewX;
            CurrViewY = destViewY;

            // Change the BGM, if applicable
            Global.AudioManager.ChangeSongs(_fields[destField].MusicNumber);

            UpdateEntities(destField, thisField, thisView, destViewX, destViewY, nextAV.Position);
        }

        public void FieldTransitionCardinalBoss(VIEW_DIR movingDirection, View srcView, View destView, Texture2D bossTexture = null)
        {
            // Camera is busy; Do not transition.
            if (Global.Camera.GetState() != CamStates.NONE)
                return;

            // Grab the View we are transitioning to
            Field srcField = srcView.GetParentField();
            Global.Textures correctedTexID = Global.TextureManager.GetMappedWorldTexID(srcField.MapGraphics);
            var srcFieldTex = Global.TextureManager.GetTexture(correctedTexID);
            
            // Determine the next field and its texture
            
            Field destField = destView.GetParentField();
            Texture2D destFieldTex;
            if (destField != null)
            {
                correctedTexID = Global.TextureManager.GetMappedWorldTexID(destField.MapGraphics);
                destFieldTex = Global.TextureManager.GetTexture(correctedTexID);
            }
            else {
                destFieldTex = bossTexture;
            }

            // Set the transitioning Active View to the next field + its texture
            ActiveView nextAV = _activeViews[(int)AViews.DEST];

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

            nextAV.SetField(destField);
            nextAV.SetFieldTex(destFieldTex);
            nextAV.SetView(destView);

            // Slide the camera toward the new field
            Global.Camera.UpdateMoveTarget(movingDirection);
        }

        private void UpdateEntities(int destField, Field thisField, View thisView, int destViewX, int destViewY, Vector2 offsetVector)
        {

            Field nextField = _fields[destField];
            var nextFieldMapData = nextField.GetMapData();
            View nextView = nextFieldMapData[destViewX, destViewY];

            // Finally, spawn the new entities for the destination View, but let the destination Field keep track of ALL of the entities (Field Entities, View Entities)
            nextField.SpawnEntities(nextView, nextField, thisView, thisField, offsetVector); // "thisField" was the previous Field we were on, regardless if we moved Fields or not
        }

        internal TextManager GetTextManager()
        {
            return Global.TextManager;
        }

        internal void UpdateCurrActiveView()
        {
            _activeViews[(int)AViews.CURR].SetView(_activeViews[(int)AViews.DEST].GetView());
            _activeViews[(int)AViews.CURR].SetField(_activeViews[(int)AViews.DEST].GetField());
            _activeViews[(int)AViews.CURR].SetFieldTex(_activeViews[(int)AViews.DEST].GetFieldTex());
        }

        internal void FieldTransitionPixelate(int warpType, int destField, int destViewX, int destViewY)
        {
            _drawBossRoom = false;

            // Camera is busy; Do not transition.
            Camera.CamStates camState = Global.Camera.GetState();
            if (camState != CamStates.NONE)
                return;

            ActiveShader = Global.ShdTransition;

            // Grab the View we are transitioning to
            Field thisField = _fields[CurrField];
            Global.Textures correctedTexID = Global.TextureManager.GetMappedWorldTexID(thisField.MapGraphics);
            var thisFieldTex = Global.TextureManager.GetTexture(correctedTexID);
            var thisFieldMapData = thisField.GetMapData();
            View thisView = thisFieldMapData[CurrViewX, CurrViewY];

            int drawingTileID;
            int frame0;
            int animeSpeed;
            int[] animatedTileInfo;

            ActiveView nextAV;

            if (destField < 0)
            {
                switch (destField)
                {
                    default:
                        destField = 0;
                        // Do not transition if the destination field goes out of the map bounds (4x5)
                        if (destViewX < 0 || destViewY < 0 || destViewX > FIELD_WIDTH - 1 || destViewY > FIELD_HEIGHT - 1)
                            return;
                        break;
                    case -1:
                        // Check and go to a boss room
                        View[] bossViews = thisField.GetBossViews();

                        // Set the transitioning Active View to the next field + its texture

                        nextAV = _activeViews[(int)AViews.DEST];
                        nextAV.Position = new Vector2(0, 0);

                        nextAV.SetField(null);
                        nextAV.SetFieldTex(thisFieldTex);
                        nextAV.SetView(bossViews[destViewX]);

                        // Update all the transition tiles to create the transition effect
                        drawingTileID = (20 * 40) + (40 * warpType);
                        frame0 = -ANIME_TILES_BEGIN + drawingTileID;
                        animeSpeed = 3;
                        animatedTileInfo = new int[17];

                        animatedTileInfo[0] = frame0;
                        animatedTileInfo[1] = animeSpeed;
                        for (int i = 1; i <= 15; i++)
                        {
                            animatedTileInfo[1 + i] = frame0 + ANIME_TILES_BEGIN + i;
                        }

                        for (var y = 0; y < ROOM_HEIGHT; y++)
                        {
                            for (var x = 0; x < ROOM_WIDTH; x++)
                            {
                                _transitionView.Chips[x, y] = new Chip((short)drawingTileID, animatedTileInfo);
                            }
                        }

                        // Prepare the camera to transition to the new field
                        Global.Camera.UpdateMoveTarget(World.VIEW_DIR.SELF);
                        Global.Camera.SetState((int)CamStates.TRANSITION_PIXELATE);

                        // Create the Shader Effect's Render Targets
                        CreateRenderTargets();

                        // Reset the abort drawing state
                        _abortDrawing = false;
                        _disposedRenderTargetsFlag = false;

                        // If we're moving to a new Field, get rid of all the entities from the previous Field and allow spawning in every view
                        thisField.DeleteAllFieldAndRoomEntities();
                        thisField.UnlockAllViewSpawning();
                        thisField.ClearVisitedViews();

                        // Old entities have been removed (if applicable). Now, our current (and next) Field+View are the destination Field+View

                        _drawBossRoom = true;
                        /*
                        CurrField = destField;
                        CurrViewX = destViewX;
                        CurrViewY = destViewY;
                        */
                        return;
                }
            }
            else if (destViewX < 0 || destViewY < 0 || destViewX > FIELD_WIDTH - 1 || destViewY > FIELD_HEIGHT - 1)
            {
                // Do not transition if the destination field goes out of the map bounds (4x5)
                return;
            }

            // Determine the next field and its texture
            Field nextField = _fields[destField];
            correctedTexID = Global.TextureManager.GetMappedWorldTexID(nextField.MapGraphics);
            var nextFieldTex = Global.TextureManager.GetTexture(correctedTexID);

            // Set the transitioning Active View to the next field + its texture

            nextAV = _activeViews[(int)AViews.DEST];
            nextAV.Position = new Vector2(0, 0);

            nextAV.SetField(nextField);
            nextAV.SetFieldTex(nextFieldTex);

            var nextFieldMapData = nextField.GetMapData();
            View nextView = nextFieldMapData[destViewX, destViewY];
            nextAV.SetView(nextView);

            // Update all the transition tiles to create the transition effect
            drawingTileID = (20 * 40) + (40 * warpType);
            frame0 = -ANIME_TILES_BEGIN + drawingTileID;
            animeSpeed = 3;
            animatedTileInfo = new int[17];
            animatedTileInfo[0] = frame0;
            animatedTileInfo[1] = animeSpeed;
            for (int i = 1; i <= 15; i++)
            {
                animatedTileInfo[1 + i] = frame0 + ANIME_TILES_BEGIN + i;
            }

            for (var y = 0; y < ROOM_HEIGHT; y++)
            {
                for (var x = 0; x < ROOM_WIDTH; x++)
                {
                    _transitionView.Chips[x, y] = new Chip((short)drawingTileID, animatedTileInfo);
                }
            }

            // Prepare the camera to transition to the new field
            Global.Camera.UpdateMoveTarget(World.VIEW_DIR.SELF);
            Global.Camera.SetState((int)CamStates.TRANSITION_PIXELATE);

            // Create the Shader Effect's Render Targets
            CreateRenderTargets();
            // Reset the abort drawing state
            _abortDrawing = false;
            _disposedRenderTargetsFlag = false;

            // If we're moving to a new Field, get rid of all the entities from the previous Field and allow spawning in every view
            thisField.DeleteAllFieldAndRoomEntities();
            thisField.UnlockAllViewSpawning();
            thisField.ClearVisitedViews();
            nextField.DeleteAllFieldAndRoomEntities();
            nextField.UnlockAllViewSpawning();
            nextField.ClearVisitedViews();

            // Old entities have been removed (if applicable). Now, our current (and next) Field+View are the destination Field+View
            CurrField = destField;
            CurrViewX = destViewX;
            CurrViewY = destViewY;

            // Change the BGM, if applicable
            //Global.AudioManager.ChangeSongs(_fields[destField].MusicNumber);

            UpdateEntities(destField, thisField, thisView, destViewX, destViewY, Vector2.Zero);
        }

        internal Textures GetCurrMapTexture()
        {
            Field thisField = _fields[CurrField];
            Global.Textures correctedTexID = Global.TextureManager.GetMappedWorldTexID(thisField.MapGraphics);
            return correctedTexID;
        }

        internal Textures GetCurrEveTexture()
        {
            Field thisField = _fields[CurrField];
            Global.Textures correctedTexID = Global.TextureManager.GetMappedEventTexID(thisField.EventGraphics);
            return correctedTexID;
        }

        internal View GetCurrentView()
        {
            if (!_drawBossRoom)
                return _fields[CurrField].GetView(Global.World.CurrViewX, Global.World.CurrViewY);
            else
            {
                return _fields[CurrField].GetBossViews()[0];
            }
        }

        internal View GetBackupView()
        {
            return _activeViews[(int)AViews.CURR].GetView();
        }

        internal Field GetCurrField()
        {
            return _fields[CurrField];
        }
    }
}