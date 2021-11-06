using System;
using System.Collections.Generic;
using System.IO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;

namespace OpenLaMulana
{
    public class Field : IGameEntity
    {
        public const int FieldWidth = 4;
        public const int FieldHeight = 5;
        public const int RoomWidth = 32;  // How many 8x8 tiles a room is wide
        public const int RoomHeight = 22; // How many 8x8 tiles a room is tall
        public const int ANIME_TILES_BEGIN = 1160;
        private Dictionary<int, int> hitList;   // lol (also, I have no idea what this does... probably hit-detection-related?

        public List<int[]> animeList;
        private List<ObjectSpawnData> objectSpawnData;
        private List<int[]> objectStartFlagData;

        private View[,] mapData = new View[FieldWidth, FieldHeight];

        public int DrawOrder => throw new NotImplementedException();

        public int _musicNumber = 0;
        public int _mapIndex;
        private int _mapData;
        public int _mapGraphics;
        public int _eventGraphics;

        private int[] _chipline = { 0, 0 };

        public Field(int mapIndex, int mapData, int mapGraphics, int eventGraphics, int musicNumber)
        {
            this._mapIndex = mapIndex;
            this._mapData = mapData;
            this._mapGraphics = mapGraphics;
            this._eventGraphics = eventGraphics;
            this._musicNumber = musicNumber;


            for (var y = 0; y < FieldHeight; y++)
            {
                for (var x = 0; x < FieldWidth; x++)
                {
                    this.mapData[x, y] = new View(RoomWidth, RoomHeight);
                }
            }

            hitList = new Dictionary<int, int>();
            animeList = new List<int[]>();
            objectSpawnData = new List<ObjectSpawnData>();
        }

        public void InitializeArea()
        {
            string mapName = "map";
            if (_mapIndex <= 9)
                mapName += "0";
            mapName += _mapIndex.ToString();

            string fileName = "Content/data/" + mapName + ".dat";

            if (File.Exists(fileName))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    int i = 0;
                    var roomX = 0;      // The Room X coordinate we're writing to
                    var roomY = 0;      // The Room Y coordinate we're writing to

                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        Int16 tileID = reader.ReadInt16();

                        if (i % RoomWidth == 0 && i > 0)
                        { // Besides the first tile, for every <RoomWidth> tiles, increment roomX (make sure it never goes beyond the bounds of the map)
                            roomX++;
                        }

                        if (roomX > FieldWidth - 1)
                        {
                            roomX = 0;

                            if (i % (RoomWidth * RoomHeight * FieldWidth) == 0)
                                roomY++;
                        }

                        int _rtx = i % RoomWidth; // Relative Room Tile X
                        int _rty = (i / (RoomWidth * FieldWidth)) % RoomHeight; // Relative Room Tile Y

                        int[] animatedTileInfo = null;

                        foreach (int[] j in animeList)
                        {
                            int aniIndex = j[0];
                            if (aniIndex + ANIME_TILES_BEGIN == tileID)
                                animatedTileInfo = j;
                        }

                        mapData[roomX, roomY].Tiles[_rtx, _rty] = new Tile(tileID, animatedTileInfo);

                        i++;
                    }
                }
            }
        }

        internal int[] GetChipline()
        {
            return _chipline;
        }

        public View[,] GetMapData()
        {
            return mapData;
        }

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        internal void SetChipline(int chiplineOne, int chiplineTwo)
        {
            _chipline[0] = chiplineOne;
            _chipline[1] = chiplineTwo;
        }

        internal void AddHit(int index, int value)
        {
            hitList.Add(index, value);
        }

        internal void DefineAnimatedTile(int[] animationInfo)
        {
            animeList.Add(animationInfo);
        }

        internal ObjectSpawnData DefineObjectSpawnData(int eventNumber, int tileX, int tileY, int OP1, int OP2, int OP3, int OP4, bool isAFieldObject)
        {
            ObjectSpawnData newObj = new ObjectSpawnData(eventNumber, tileX / 2048, tileY / 2048, OP1, OP2, OP3, OP4, isAFieldObject);
            objectSpawnData.Add(newObj);

            return newObj;
        }
    }
}