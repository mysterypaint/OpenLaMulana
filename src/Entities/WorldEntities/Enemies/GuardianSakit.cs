using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GuardianSakit : IGlobalWorldEntity
    {
        private int spritesMax = 7;
        Sprite[] sprites = new Sprite[7];
        private int _sprNum = 0;

        public GuardianSakit(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.SpriteDefManager.GetTexture(Global.SpriteDefs.BOSS01);
            for (var i = 0; i < spritesMax; i++)
            {
                sprites[i] = Global.SpriteDefManager.GetSprite(Global.SpriteDefs.BOSS01, i);
            }
            _sprIndex = sprites[_sprNum];
            Position += new Vector2(200, -3);
            Visible = false;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!Visible)
                return;


            if (viewCoords.X == _world.CurrViewX && viewCoords.Y == _world.CurrViewY)
            {
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
            }
        }

        public override void Update(GameTime gameTime)
        {
            Position = new Vector2(1, 0);

            int animeSpeed = 6;
            if (gameTime.TotalGameTime.Ticks % (animeSpeed * 6) == 0) {
                //_sprNum++;
                if (_sprNum >= spritesMax)
                    _sprNum= 0;
            }
            _sprIndex = sprites[_sprNum];
        }
    }
}