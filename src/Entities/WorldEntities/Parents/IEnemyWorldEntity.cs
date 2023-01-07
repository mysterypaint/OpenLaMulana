using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;

namespace OpenLaMulana.Entities.WorldEntities.Parents
{
    abstract class IEnemyWorldEntity : IRoomWorldEntity
    {
        internal int _hp = 2;
        protected IEnemyWorldEntity(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView)
        {
        }
    }
}