using static OpenLaMulana.Entities.World;

namespace OpenLaMulana
{
    public class View
    {
        private int[] _destWorld = { 0, 0, 0, 0 };   // World may never go below 0
        private int[] _destField = { 0, 0, 0, 0 };   // Field may never go below 0
        private int[] _destX = { 0, 0, 0, 0 };       // If X is negative, forbid moving toward this direction
        private int[] _destY = { 0, 0, 0, 0 };       // Y may never go below 0

        public Tile[,] Tiles { get; set; }
        public int _roomNumber { get; set; } = 0; // Unsure what this does... Maybe has to do with sharing room numbers -> how enemies persist/de-spawn?

        public View(int roomWidth, int roomHeight)
        {
            Tiles = new Tile[roomWidth, roomHeight];

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

        internal void DefineViewDestination(VIEW_DIR direction, int destWorld, int destField, int destX, int destY)
        {
            _destWorld[(int)direction] = destWorld;
            _destField[(int)direction] = destField;
            _destX[(int)direction] = destX;
            _destY[(int)direction] = destY;
        }

        internal int[] GetDestinationView(VIEW_DIR direction)
        {
            int[] destination = { _destWorld[(int)direction], _destField[(int)direction], _destX[(int)direction], _destY[(int)direction] };
            return destination;
        }
    }
}