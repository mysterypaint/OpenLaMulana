using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;
using static OpenLaMulana.Global;

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

        private SpriteAnimation _sparklingParticles = new SpriteAnimation();
        private AnkhStates _state = AnkhStates.VISIBLE;
        private IGameEntity _activatedGuardian = null;

        public Ankh(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _sprIndex = new Sprite(_tex, 0, 64, 16, 16);
            _sparklingParticles = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(0, 32), 16, 16, new Point(16, 0), 3, 0.05f);

            _state = AnkhStates.USABLE;

            if (_state == AnkhStates.USABLE)
            {
                _sparklingParticles.Play();
            }

            _activatedGuardian = InstanceCreate(new GuardianSakit(-0xFFFF, -0xFFFF, 0, 0, 0, 0, destView));
        }

        ~Ankh()
        {
            if (_activatedGuardian != null)
            {
                _activatedGuardian = InstanceDestroy(_activatedGuardian);
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (_state)
            {
                default:
                case AnkhStates.VISIBLE:
                    break;
                case AnkhStates.USABLE:
                    _sparklingParticles.Update(gameTime);

                    if (Global.InputController.KeyJumpPressed)
                    {

                        _state = AnkhStates.ACTIVATED;

                        if (Global.AudioManager.IsPlaying() >= 0)
                            Global.AudioManager.StopMusic();

                        View[] bossViews = Global.World.GetCurrField().GetBossViews();
                        switch (Global.World.GetCurrField().GetBossID())
                        {
                            default:
                                break;
                            case (int)Global.BossIDs.AMPHISBAENA:
                                Global.AudioManager.ChangeSongs(23);
                                break;
                            case (int)Global.BossIDs.SAKIT:
                                Global.AudioManager.ChangeSongs(24);
                                break;
                            case (int)Global.BossIDs.ELLMAC:
                                Global.AudioManager.ChangeSongs(25);
                                break;
                            case (int)Global.BossIDs.BAHAMUT:
                                Global.AudioManager.ChangeSongs(26);
                                break;
                            case (int)Global.BossIDs.VIY:
                                Global.AudioManager.ChangeSongs(27);
                                break;
                            case (int)Global.BossIDs.PALENQUE:
                                Global.AudioManager.ChangeSongs(28);
                                break;
                            case (int)Global.BossIDs.BAPHOMET:
                                Global.AudioManager.ChangeSongs(29);
                                break;
                            case (int)Global.BossIDs.TIAMAT:
                                Global.AudioManager.ChangeSongs(30);
                                break;
                            case (int)Global.BossIDs.MOTHER:
                                Global.AudioManager.ChangeSongs(31);
                                break;
                        }
                        Global.World.FieldTransitionPixelate(2, -1, 0, 0);
                        _activatedGuardian = InstanceCreate(new GuardianSakit(-0xFFFF, -0xFFFF, 0, 0, 0, 0, null));
                    }
                    break;
                case AnkhStates.ACTIVATED:
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (viewCoords.X == _world.CurrViewX && viewCoords.Y == _world.CurrViewY)
            {
                switch (_state)
                {
                    default:
                    case AnkhStates.VISIBLE:
                        _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                        break;
                    case AnkhStates.USABLE:
                        _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                        if (Global.AudioManager.IsPlaying() != 20)
                            Global.AudioManager.ChangeSongs(20);
                        if (_sparklingParticles.IsPlaying)
                            _sparklingParticles.Draw(spriteBatch, Position + new Vector2(0, World.CHIP_SIZE));
                        break;
                    case AnkhStates.ACTIVATED:
                        break;
                }
            }
        }
    }
}