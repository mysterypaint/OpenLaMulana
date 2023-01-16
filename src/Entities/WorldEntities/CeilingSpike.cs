using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class CeilingSpike : IRoomWorldEntity
    {
        public CeilingSpike(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            UInt32[] pixels = new UInt32[16 * 16];
            pixels[0] = 0x000000FF;
            _tex = new Texture2D(Global.GraphicsDevice, 16, 16, false, SurfaceFormat.Color);
            _tex.SetData<UInt32>(pixels, 0, 16 * 16);

            _sprIndex = new Sprite(_tex, 48, 16, 16, 16);
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