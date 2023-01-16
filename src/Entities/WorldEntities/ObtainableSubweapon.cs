using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class ObtainableSubweapon : InteractableWorldEntity
    {
        private int _itemID;

        public ObtainableSubweapon(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _itemID = op1;
            int texOffX = (_itemID % 4) * 16;
            int texOffY = (int)Math.Floor((double)_itemID / 4) * 16;
            _sprIndex = new Sprite(_tex, 256 + texOffX, 96 + texOffY, 16, 16);
            HP = 1;
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
        }
    }
}