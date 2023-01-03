using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class RuinsTablet : IRoomWorldEntity
    {
        public RuinsTablet(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.MAPG00);
            _sprIndex = new Sprite(_tex, 48, 16, 16, 16);
            Position += new Vector2(0, -8);
        }
    }
}