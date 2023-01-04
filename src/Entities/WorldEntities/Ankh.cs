using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class Ankh : IRoomWorldEntity
    {
        enum AnkhStates
        {
            VISIBLE,
            USABLE,
            ACTIVATED,
            MAX
        };

        SpriteAnimation _sparklingParticles = new SpriteAnimation();
        AnkhStates state = AnkhStates.VISIBLE;

        public Ankh(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _sprIndex = new Sprite(_tex, 0, 64, 16, 16);
            _sparklingParticles = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(0, 32), 16, 16, new Point(16, 0), 3, 0.05f);
            
            state = AnkhStates.USABLE;

            if (state == AnkhStates.USABLE)
            {
                _sparklingParticles.Play();
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (state)
            {
                default:
                case AnkhStates.VISIBLE:
                    break;
                case AnkhStates.USABLE:
                    _sparklingParticles.Update(gameTime);
                    break;
                case AnkhStates.ACTIVATED:
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (viewCoords.X == _world.CurrViewX && viewCoords.Y == _world.CurrViewY)
            {
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);

                switch (state)
                {
                    default:
                    case AnkhStates.VISIBLE:
                        break;
                    case AnkhStates.USABLE:
                        if (Global.AudioManager.IsPlaying() != 20)
                            Global.AudioManager.ChangeSongs(20);
                        if (_sparklingParticles.IsPlaying)
                            _sparklingParticles.Draw(spriteBatch, Position + new Vector2(0, World.CHIP_SIZE));
                        break;
                    case AnkhStates.ACTIVATED:
                        if (Global.AudioManager.IsPlaying() >= 0)
                            Global.AudioManager.StopMusic();
                        break;
                }
            }
        }
    }
}