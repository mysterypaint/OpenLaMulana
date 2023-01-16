using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class AnkhParticle : IRoomWorldEntity
    {
        SpriteAnimation _myAnimation;
        public AnkhParticle(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            Position = new Vector2(x, y);
            _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _myAnimation = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(17 * World.CHIP_SIZE, 22 * World.CHIP_SIZE), 16, 16, new Point(16, 0), 4, 0.02f);
            _myAnimation.Play();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            //_sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
            _myAnimation.Draw(spriteBatch, Position);
        }

        public override void Update(GameTime gameTime)
        {
            _myAnimation.Update(gameTime);
            _sprIndex = _myAnimation.CurrentFrame.Sprite;
            Position += new Vector2(0, -2);
        }
    }
}