using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;

namespace OpenLaMulana.Entities.WorldEntities.Parents
{
    abstract class IEnemyWorldEntity : IRoomWorldEntity
    {
        internal int _hp = 2;
        Protag _protag = Global.Protag;

        protected IEnemyWorldEntity(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView)
        {
        }

        public Rectangle BBox
        {
            get
            {
                Rectangle box = new Rectangle(
                    (int)Math.Round(Position.X - BBoxOriginX),
                    (int)Math.Round(Position.Y - BBoxOriginY),
                    9,
                    11
                );
                //box.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);
                return box;
            }
        }

        public bool IntersectsWithPlayer()
        {
            return BBox.Intersects(_protag.BBox);
        }
    }
}