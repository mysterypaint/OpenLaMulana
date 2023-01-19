﻿using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using OpenLaMulana.System;

namespace OpenLaMulana.Entities.WorldEntities.Parents
{
    abstract class InteractableWorldEntity : IRoomWorldEntity
    {
        public virtual int HP { get; set; } = 2;
        private Protag _protag = Global.Protag;
        public virtual int HitboxWidth { get; set; } = 8;
        public virtual int HitboxHeight { get; set; } = 8;

        protected InteractableWorldEntity(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
        }

        public Rectangle BBox
        {
            get
            {
                Rectangle box = new Rectangle(
                    (int)Math.Round(Position.X - BBoxOriginX),
                    (int)Math.Round(Position.Y - BBoxOriginY),
                    HitboxWidth,
                    HitboxHeight
                );
                //box.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);
                return box;
            }
        }

        public bool CollidesWithPlayer()
        {
            return BBox.Intersects(_protag.BBox);
        }
    }
}