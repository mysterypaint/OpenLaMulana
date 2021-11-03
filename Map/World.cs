using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.Collections;
using OpenLaMulana.System;
using System.Text.RegularExpressions;

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

        public World(EntityManager entityManager, Texture2D _gameFontTex)
        {
            s_entityManager = entityManager;
            
            _fields = new List<Field>();

            _textManager = new TextManager(_gameFontTex);
            
            // Testing: Loads the first 22 maps into memory when the game world is initialized on bootup
            for (int i = 0; i <= 22; i++)
            {
                string numStr;
                if (i <= 9)
                {
                    numStr = "0" + i.ToString();
                }
                else
                {
                    numStr = i.ToString();
                }
                _fields.Add(new Field("map" + numStr, "mapg" + numStr, "eveg" + numStr));
            }

            fieldCount = _fields.Count;

            // Define the font table for the game
            Dictionary<int, string>  s_charSet = _textManager.GetCharSet();
            s_charSet = PseudoXML.DefineCharSet(s_charSet);

            string jpTxtFile = "Content/data/script_JPN_UTF8.txt";
            string engTxtFile = "Content/data/script_ENG_UTF8.txt";
            // Decrypt "Content/data/script.dat" and the English-Translated counterpart file
            PseudoXML.DecodeScriptDat("Content/data/script.dat", jpTxtFile, s_charSet, this, OpenLaMulanaGame.Languages.Japanese);
            PseudoXML.DecodeScriptDat("Content/data/script_ENG.dat", engTxtFile, s_charSet, this, OpenLaMulanaGame.Languages.English);

            string[] data = File.ReadAllLines(jpTxtFile);

            /*
            foreach(string s in data)
            {
                string regexPattern = "<TALK>[\r\n]*((.*?)[\r\n]*)*</TALK>+";


                if (Regex.Match(s, regexPattern))
                    break;
                foreach (Match match in Regex.Matches(inStr, regexPattern))
                {
                    string groupStr = match.Groups[0].ToString();
                    string filteredStr = groupStr.Substring(7, groupStr.Length - 15);
                    aResult.Add(filteredStr);
                }
            }*/

            //ParseXmlRecursive(world, data, 0);

            if (File.Exists(engTxtFile))
            {
                File.Delete(engTxtFile);
            }
            if (File.Exists(jpTxtFile))
            {
                File.Delete(jpTxtFile);
            }

        }

        /*
        // Returns new currentLine; end when it returns -1
        public int ParseXmlRecursive(IGameEntity currentObject, string[] xml, int currentLine)
        {
            if (currentLine >= xml.Length - 1)
                return -1;

            string line;

            do
            {
                // "<MAP 1,3,13>"
                line = xml[currentLine];

                // "<MAP"
                string type = line.Split(" ")[0];
                // "MAP"
                type = type.Substring(1, type.Length);

                switch (type)
                {
                    case "WORLD":
                        break;
                    case "FIELD":
                        string[] params = line.Split(" ")[1].Split(",");
                        // Process paramters
                        //...
                        Field f = new Field(params[0], params[1], params[2], params[3], params[4]);
                        currentLine = ParseXmlRecursive(f, xml, currentLine + 1);
                        currentObject.AddChild(f);
                        break;
                    case "MAP":
                        break;
                }
            }
            while (!line.StartsWith("</"));
            return currentLine;
        }
        */


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
            var _thisTex = _Textures[(int)currField];
            var currFieldRoomData = _thisField.GetMapData();
            var _thisRoom = currFieldRoomData[currRoomX, currRoomY];

            //mapData[roomX, roomY].Tiles[_rtx, _rty] = tileID;

            // Loop through every single Room[_x][_y] tile to draw every single tile in a given room
            for (int _y = 0; _y < Field.RoomHeight; _y++)
            {
                for (int _x = 0; _x < Field.RoomWidth; _x++)
                {
                    int _thisTile = _thisRoom.Tiles[_x, _y];

                    //spriteBatch.Draw(_fieldTextures[currField], new Vector2(0, 0), new Rectangle(16, 16, tileWidth, tileHeight), Color.White);
                    var _posx = (_x * 8);
                    var _posy = (_y * 8);
                    var _texX = (_thisTile % 40) * 8;
                    var _texY = (_thisTile / 40) * 8;

                    spriteBatch.Draw(_Textures[(int)currField], new Vector2(_posx, OpenLaMulanaGame.HUD_HEIGHT + _posy), new Rectangle(_texX, _texY, tileWidth, tileHeight), Color.White);
                }
            }

        }

        public void FieldTransitionRight()
        {
            currRoomX++;
            if (currRoomX > Field.FieldWidth - 1)
                currRoomX = 0;
        }
        public void FieldTransitionLeft()
        {
            currRoomX--;
            if (currRoomX < 0)
                currRoomX = Field.FieldWidth - 1;
        }
        public void FieldTransitionUp()
        {
            currRoomY--;
            if (currRoomY < 0)
                currRoomY = Field.FieldHeight - 1;
        }
        public void FieldTransitionDown()
        {
            currRoomY++;
            if (currRoomY > Field.FieldHeight - 1)
                currRoomY = 0;
        }

        internal TextManager GetTextManager()
        {
            return _textManager;
        }
    }
}