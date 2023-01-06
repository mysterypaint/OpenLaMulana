using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class FloorSwitch : IRoomWorldEntity
    {
        public FloorSwitch(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            _sprIndex = new Sprite(_tex, 288, 128, 16, 8);
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