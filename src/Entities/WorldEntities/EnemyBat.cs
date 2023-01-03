using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class EnemyBat : IEnemyWorldEntity
    {
        public EnemyBat(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ENEMY1);
            _sprIndex = new Sprite(_tex, 80, 0, 16, 16);
            _hp = 1;
        }

    }
}