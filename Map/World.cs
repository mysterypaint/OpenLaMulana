using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.Collections;
using OpenLaMulana.System;
using System.Text.RegularExpressions;
using System;

namespace OpenLaMulana.Entities
{
    public partial class World : IGameEntity
    {
        public int DrawOrder { get; set; }
        public const int tileWidth = 8;
        public const int tileHeight = 8;

        public int currField { get; set; }  = 1;

        private List<Field> _fields;
        private List<Texture2D> _Textures = null;
        private int currRoomX = 0;
        private int currRoomY = 0;

        public int fieldCount = 0;

        public static EntityManager s_entityManager;

        private TextManager _textManager;

        private int[] currChipLine;

        public enum VIEW_DEST
        {
            WORLD,
            FIELD,
            X,
            Y
        };

        public enum VIEW_DIR
        {
            UP,
            RIGHT,
            DOWN,
            LEFT
        };

        public World(EntityManager entityManager, Texture2D _gameFontTex)
        {
            s_entityManager = entityManager;
            
            _fields = new List<Field>();

            _textManager = new TextManager(_gameFontTex);


            // Define the font table for the game
            Dictionary<int, string>  s_charSet = _textManager.GetCharSet();
            s_charSet = PseudoXML.DefineCharSet(s_charSet);

            string jpTxtFile = "Content/data/script_JPN_UTF8.txt";
            string engTxtFile = "Content/data/script_ENG_UTF8.txt";

            // Decrypt "Content/data/script.dat" and the English-Translated counterpart file
            PseudoXML.DecodeScriptDat("Content/data/script.dat", jpTxtFile, s_charSet, this, OpenLaMulanaGame.Languages.Japanese);
            PseudoXML.DecodeScriptDat("Content/data/script_ENG.dat", engTxtFile, s_charSet, this, OpenLaMulanaGame.Languages.English);

            string[] data = File.ReadAllLines(jpTxtFile);

            ParseXmlRecursive(this, data, 0);

            fieldCount = _fields.Count;

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

            currChipLine = _fields[currField].GetChipline();
        }

        public Field GetField(int index)
        {
            return _fields[index];
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
                } else
                {
                    // "MAP"
                    type = splitTrimmedLine.Substring(1, splitTrimmedLine.Length - 1);
                }

                switch (type)
                {
                    case "FIELD":
                        args = parseArgs(line);
                        Field f = new Field(args[0], args[1], args[2], args[3], args[4]);
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
                        if (currView == null) // Spawn a Field-global entity
                            currObjSpawnData = currField.DefineObjectSpawnData(args[0], args[1], args[2], args[3], args[4], args[5], args[6], currView == null);
                        else // Spawn a View-specific entity
                            currObjSpawnData = currField.DefineObjectSpawnData(args[0], args[1], args[2], args[3], args[4], args[5], args[6], currView == null);
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
                        currView._roomNumber = roomNumber;
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

        internal void InitGameText(OpenLaMulanaGame.Languages lang, List<string> data)
        {
            _textManager.SetDialogue(lang, data);
        }

        public void SetTexturesList(List<Texture2D> inTexList)
        {
            _Textures = inTexList;
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var _thisField = _fields[currField];
            var _thisTex = _Textures[_thisField._mapGraphics];
            var currFieldRoomData = _thisField.GetMapData();
            var _thisRoom = currFieldRoomData[currRoomX, currRoomY];
            var animations = _thisField.animeList;

            //mapData[roomX, roomY].Tiles[_rtx, _rty] = tileID;

            // Loop through every single Room[_x][_y] tile to draw every single tile in a given room
            for (int _y = 0; _y < Field.RoomHeight; _y++)
            {
                for (int _x = 0; _x < Field.RoomWidth; _x++)
                {
                    Tile _thisTile = _thisRoom.Tiles[_x, _y];

                    if (!_thisTile.isAnime)
                    {
                        //spriteBatch.Draw(_fieldTextures[currField], new Vector2(0, 0), new Rectangle(16, 16, tileWidth, tileHeight), Color.White);
                        var _posx = (_x * 8);
                        var _posy = (_y * 8);
                        var _texX = (_thisTile._tileID % 40) * 8;
                        var _texY = (_thisTile._tileID / 40) * 8;

                        spriteBatch.Draw(_Textures[_thisField._mapGraphics], new Vector2(_posx, OpenLaMulanaGame.HUD_HEIGHT + _posy), new Rectangle(_texX, _texY, tileWidth, tileHeight), Color.White);
                    } else
                    {
                        // Handle animated tiles here
                        var animeSpeed = _thisTile.animeSpeed;
                        var _animeFrames = _thisTile.GetAnimeFrames();
                        var maxFrames = _animeFrames.Length;

                        if (animeSpeed > 0) {
                            if (gameTime.TotalGameTime.Ticks % (animeSpeed * 6) == 0)
                            {
                                _thisTile._currFrame++;

                                if (_thisTile._currFrame >= maxFrames)
                                    _thisTile._currFrame = 0;
                            }
                        }

                        var drawingTileID = _animeFrames[_thisTile._currFrame];

                        //spriteBatch.Draw(_fieldTextures[currField], new Vector2(0, 0), new Rectangle(16, 16, tileWidth, tileHeight), Color.White);
                        var _posx = (_x * 8);
                        var _posy = (_y * 8);
                        var _texX = (drawingTileID % 40) * 8;
                        var _texY = (drawingTileID / 40) * 8;

                        spriteBatch.Draw(_Textures[_thisField._mapGraphics], new Vector2(_posx, OpenLaMulanaGame.HUD_HEIGHT + _posy), new Rectangle(_texX, _texY, tileWidth, tileHeight), Color.White);
                    }
                }
            }

        }

        public void FieldTransition(VIEW_DIR movingDirection)
        {
            var thisFieldMapData = _fields[currField].GetMapData();
            View thisView = thisFieldMapData[currRoomX, currRoomY];
            int[] viewDest = thisView.GetDestinationView(movingDirection);

            //currRoomX = viewDest[(int)VIEW_DEST.WORLD];
            int destField = viewDest[(int)VIEW_DEST.FIELD];
            int destRoomX = viewDest[(int)VIEW_DEST.X];
            int destRoomY = viewDest[(int)VIEW_DEST.Y];

            if (destField <= -1 || destRoomX <= -1 || destRoomY <= -1 || destRoomX > Field.FieldWidth - 1 || destRoomY > Field.FieldHeight - 1)
                return;

            currField = destField;
            currRoomX = destRoomX;
            currRoomY = destRoomY;
        }

        internal TextManager GetTextManager()
        {
            return _textManager;
        }
    }
}