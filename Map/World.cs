using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using System.Collections;
using OpenLaMulana.System;

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
        private List<string> _dialogue;

        public int fieldCount = 0;

        public static EntityManager s_entityManager;

        public static Dictionary<int, string> s_charSet;

        public World(EntityManager entityManager)
        {
            s_entityManager = entityManager;
            
            _fields = new List<Field>();


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
            s_charSet = new Dictionary<int, string>();
            s_charSet = PseudoXML.DefineCharSet(s_charSet);

            // Decrypt "Content/data/script.dat" and the English-Translated counterpart file
            byte[] scriptData = PseudoXML.DecodeScriptDat("Content/data/", this, s_charSet);

            // Parse the decrypted data as a data tree
            PseudoXML.ParseDataScriptTree("Content/data/script_JPN_UTF8.txt", this);

        }

        public void SetDialogue(List<string> dialogue)
        {
            _dialogue = dialogue;
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
    }
}