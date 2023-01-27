using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenLaMulana
{
    public class ObjectSpawnData
    {
        public int EventNumber;
        public int X;
        public int Y;
        public int OP1;
        public int OP2;
        public int OP3;
        public int OP4;
        public List<ObjectStartFlag> StartFlags;
        public bool SpawnIsGlobal = true; // Sanity checking
        public bool IsHardModeChange { get; set; } = false; // For quick researching
        public bool XYAreCoords { get; set; } = true;

        public ObjectSpawnData(int eventNumber, int x, int y, int OP1, int OP2, int OP3, int OP4, bool xYAreCoords)
        {
            EventNumber = eventNumber;
            X = x;
            Y = y;
            this.OP1 = OP1;
            this.OP2 = OP2;
            this.OP3 = OP3;
            this.OP4 = OP4;
            this.XYAreCoords = xYAreCoords;

            StartFlags = new List<ObjectStartFlag>();
        }


        internal void AddStartFlag(int value, bool conditionMetIfFlagIsOn)
        {
            ObjectStartFlag newStartFlag = new ObjectStartFlag(value, conditionMetIfFlagIsOn);
            StartFlags.Add(newStartFlag);
        }

        internal void CoordsAreRelativeToView(int roomWidth, int roomHeight)
        {
            if (XYAreCoords)
            {
                X %= roomWidth;
                Y %= roomHeight;
            }
            SpawnIsGlobal = false;
        }

        internal Point GetExactChip() {
            return new Point(X / 2048, Y / 2048);
        }
    }
}