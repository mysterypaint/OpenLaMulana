using System.Collections.Generic;

namespace OpenLaMulana
{
    public class Room
    {
        enum WORLD_DESTINATION_DIRECTIONS
        {
            UP,
            RIGHT,
            DOWN,
            LEFT
        };
        enum WORLD_DESTINATION_PARAMS
        {
            WORLD,      // World may never go below 0
            FIELD,      // Field may never go below 0
            X,          // If X is negative, forbid moving toward this direction
            Y           // Y may never go below 0
        };

        public int[,] Tiles { get; set; }
        private int[] RoomDirections = { 0, 0, 0, 0 };
        private int[] RoomParams = { 0, 0, 0, 0 };
        private int RoomNumber = 0; // Unsure what this does... Maybe has to do with sharing room numbers -> how enemies persist/de-spawn?

        public Room(int roomWidth, int roomHeight)
        {
            Tiles = new int[roomWidth, roomHeight];

            /*
            for (var y = 0; y < roomHeight; y++)
            {
                for (var x = 0; x < roomWidth; x++)
                {
                    Tiles[x, y] = -1;
                }
            }
            */
        }
    }
}