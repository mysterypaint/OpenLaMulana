using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class TreasureChest : IRoomWorldEntity
    {
        private Sprite _sprOpen, _sprClosed;

        public TreasureChest(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.MAPG00);
            _sprOpen = new Sprite(_tex, 304, 16, 16, 16);
            _sprClosed = new Sprite(_tex, 304, 0, 16, 16);
            _sprIndex = _sprClosed;
        }
    }
}