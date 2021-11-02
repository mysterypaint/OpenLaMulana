using System;
using System.IO;

namespace OpenLaMulana
{
    public class Field
    {
        public const int FieldWidth = 4;
        public const int FieldHeight = 5;
        public const int RoomWidth = 32;  // How many 8x8 tiles a room is wide
        public const int RoomHeight = 22; // How many 8x8 tiles a room is tall

        private View[,] mapData = new View[FieldWidth, FieldHeight];
        private string _mapName { get; set; }
        private string _mapGraphics { get; set; }
        private string _eventGraphics { get; set; }
        private int _musicNumber = 0;

        public Field(string mapName, string mapGraphics, string eventGraphics)
        {
            _mapName = mapName;
            _mapGraphics = mapGraphics;
            _eventGraphics = eventGraphics;

            for (var y = 0; y < FieldHeight; y++)
            {
                for (var x = 0; x < FieldWidth; x++)
                {
                    mapData[x, y] = new View(RoomWidth, RoomHeight);
                }
            }

            InitializeArea(mapName);
        }

        private void InitializeArea(string mapName)
        {
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

                        mapData[roomX, roomY].Tiles[_rtx, _rty] = tileID;

                        i++;
                    }
                }
            }
        }

        public View[,] GetMapData()
        {
            return mapData;
        }
    }
}