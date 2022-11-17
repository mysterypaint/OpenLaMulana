﻿using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;

namespace OpenLaMulana
{
    internal class ObjectSpawnData
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

        public ObjectSpawnData(int eventNumber, int x, int y, int OP1, int OP2, int OP3, int OP4)
        {
            EventNumber = eventNumber;
            X = x;
            Y = y;
            this.OP1 = OP1;
            this.OP2 = OP2;
            this.OP3 = OP3;
            this.OP4 = OP4;

            StartFlags = new List<ObjectStartFlag>();
        }

        internal void AddStartFlag(int value, bool initiallyDisabled)
        {
            ObjectStartFlag newStartFlag = new ObjectStartFlag(value, initiallyDisabled);
            StartFlags.Add(newStartFlag);
        }

        internal void CoordsAreRelativeToView(int roomWidth, int roomHeight)
        {
            X %= roomWidth;
            Y %= roomHeight;
            SpawnIsGlobal = false;
        }

        internal Point GetExactChip() {
            return new Point(X / 2048, Y / 2048);
        }
    }
}