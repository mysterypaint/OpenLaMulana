using System.Collections.Generic;
using System.Text;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Xml;
using static OpenLaMulana.Entities.PseudoXML;
using System.Collections;

namespace OpenLaMulana.Entities
{

    public class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }

    public class World : IGameEntity
    {
        public int DrawOrder { get; set; }
        public const int tileWidth = 8;
        public const int tileHeight = 8;
        public int currField = 3;

        private List<Field> _fields;
        private List<Texture2D> _fieldTextures = null;
        private int currRoomX = 0;
        private int currRoomY = 0;
        private List<string> _dialogue;


        public World()
        {
            _fields = new List<Field>();
            _fields.Add(new Field("map00", "mapg00", "eveg00"));
            _fields.Add(new Field("map01", "mapg01", "eveg01"));
            _fields.Add(new Field("map02", "mapg02", "eveg02"));
            _fields.Add(new Field("map03", "mapg03", "eveg03"));
            _fields.Add(new Field("map04", "mapg04", "eveg04"));

            PseudoXML.ParseDataScriptTree("Content/data/script_ENG.txt", this);
        }

        public void SetDialogue(List<string> dialogue)
        {
            _dialogue = dialogue;
        }
        

        public void SetAreaTexturesList(List<Texture2D> inTexList)
        {
            _fieldTextures = inTexList;
        }

        public void Update(GameTime gameTime)
        {
            
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var _thisField = _fields[currField];
            var _thisTex = _fieldTextures[currField];
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

                    spriteBatch.Draw(_fieldTextures[currField], new Vector2(_posx, OpenLaMulanaGame.HUD_HEIGHT + _posy), new Rectangle(_texX, _texY, tileWidth, tileHeight), Color.White);
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