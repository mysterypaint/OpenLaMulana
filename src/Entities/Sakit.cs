using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class Sakit : IRoomWorldEntity
    {
        private int spritesMax = 7;//7;
        Sprite[] sprites = new Sprite[7];
        private int _sprNum = 0;

        public Sakit(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.SpriteDefManager.GetTexture(Global.SpriteDefs.BOSS01);
            for (var i = 0; i < spritesMax; i++)
            {
                sprites[i] = Global.SpriteDefManager.GetSprite(Global.SpriteDefs.BOSS01, i);
            }
            _sprIndex = sprites[_sprNum];
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (viewCoords.X == _world.CurrViewX && viewCoords.Y == _world.CurrViewY)
            {

                /*
                 * Rectangle srcRect;

                srcRect = new Rectangle(0, 0, 16, 16);
                Rectangle destRect = new Rectangle(
                    (int)Position.X,
                    (int)Position.Y + Main.HUD_HEIGHT,
                    8, 8);
                spriteBatch.Draw(_tex, destRect, srcRect, Color.White);
*/

                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                //_textManager.DrawText((int)Position.X, (int)Position.Y, "ok");
            }
        }

        public override void Update(GameTime gameTime)
        {
            Position += new Vector2(1, 0);

            int animeSpeed = 6;
            if (gameTime.TotalGameTime.Ticks % (animeSpeed * 6) == 0) {
                _sprNum++;
                if (_sprNum >= spritesMax)
                    _sprNum= 0;
            }
            _sprIndex = sprites[_sprNum];
        }
    }
}