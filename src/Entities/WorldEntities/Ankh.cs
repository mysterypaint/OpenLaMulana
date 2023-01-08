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
        }
        private SpriteAnimation _sparklingParticles = new SpriteAnimation();
        private AnkhStates _state = AnkhStates.VISIBLE;
        private IGameEntity _activatedGuardian = null;
        private Global.BossIDs _bossID = Global.BossIDs.AMPHISBAENA;
        private float _activationTimer = 0.0f;
        private AnkhParticle _ankhActivatedParticle = null;

        public Ankh(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _sprIndex = new Sprite(_tex, 0, 64, 16, 16);
            _sparklingParticles = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(0, 32), 16, 16, new Point(16, 0), 3, 0.05f);

            _state = AnkhStates.USABLE;

            if (_state == AnkhStates.USABLE)
            {
                _sparklingParticles.Play();
            }
        }

        ~Ankh()
        {
            if (_ankhActivatedParticle != null)
            {
                _ankhActivatedParticle = (AnkhParticle)InstanceDestroy(_ankhActivatedParticle);
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
                    if (viewCoords.X == _world.CurrViewX && viewCoords.Y == _world.CurrViewY && _parentView.GetParentField().ID == Global.World.CurrField)
                    {
                        if (Global.AudioManager.IsPlaying() != 20)
                            Global.AudioManager.ChangeSongs(20);

                        if (Global.InputManager.GetPressedKeyState(Global.ControllerKeys.SUBWEAPON))
                        {
                            _state = AnkhStates.ACTIVATED;
                            _activationTimer = 35.0f;

                            if (Global.AudioManager.IsPlaying() >= 0)
                                Global.AudioManager.StopMusic();
                            Global.AudioManager.PlaySFX(SFX.ANKH_ACTIVATED);
                            _ankhActivatedParticle = (AnkhParticle)InstanceCreate(new AnkhParticle((int)Position.X, (int)Position.Y - 16, 0, 0, 0, 0, true, null));
                        }
                    }
                    break;
                case AnkhStates.ACTIVATED:
                    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (_activationTimer > 0)
                        _activationTimer -= 13 * dt;
                    else
                    {
                        _activationTimer = 0.0f;

                        View[] bossViews = Global.World.GetCurrField().GetBossViews();
                        //FieldTransitionCardinalBoss
                        _bossID = (Global.BossIDs)Global.World.GetCurrField().GetBossID();

                        Global.Camera.SetState((int)System.Camera.CamStates.STANDBY);
                        switch (_bossID)
                        {
                            default:
                                break;
                            case Global.BossIDs.AMPHISBAENA:
                                Global.AudioManager.ChangeSongs(23);
                                Global.World.FieldTransitionPixelate(1, -1, 0, 0);
                                break;
                            case Global.BossIDs.SAKIT:
                                Global.AudioManager.ChangeSongs(24);
                                Global.World.FieldTransitionPixelate(1, -1, 0, 0);
                                _activatedGuardian = InstanceCreatePersistent(new GuardianSakit(-0xFFFF, -0xFFFF, 0, 0, 0, 0, true, null));
                                break;
                            case Global.BossIDs.ELLMAC:
                                Global.AudioManager.ChangeSongs(25);
                                Global.World.FieldTransitionPixelate(1, -1, 0, 0);
                                _activatedGuardian = InstanceCreatePersistent(new GuardianEllmac(-0xFFFF, -0xFFFF, 0, 0, 0, 0, true, null));
                                break;
                            case Global.BossIDs.BAHAMUT:
                                Global.AudioManager.ChangeSongs(26);
                                Global.World.FieldTransitionPixelate(1, -1, 0, 0);
                                _activatedGuardian = InstanceCreatePersistent(new GuardianBahamut(-0xFFFF, -0xFFFF, 0, 0, 0, 0, true, null));
                                break;
                            case Global.BossIDs.VIY:
                                Global.AudioManager.ChangeSongs(27);
                                Global.World.FieldTransitionPixelate(1, -1, 0, 0);
                                _activatedGuardian = InstanceCreatePersistent(new GuardianViy(-0xFFFF, -0xFFFF, 0, 0, 0, 0, true, null));
                                break;
                            case Global.BossIDs.PALENQUE:
                                Global.AudioManager.ChangeSongs(28);
                                Global.World.FieldTransitionPixelate(1, -1, 0, 0);
                                _activatedGuardian = InstanceCreatePersistent(new GuardianPalenque(-0xFFFF, -0xFFFF, 0, 0, 0, 0, true, null));
                                break;
                            case Global.BossIDs.BAPHOMET:
                                Global.AudioManager.ChangeSongs(29);
                                Global.World.FieldTransitionPixelate(1, -1, 0, 0);
                                break;
                            case Global.BossIDs.TIAMAT:
                                Global.AudioManager.ChangeSongs(30);
                                Global.World.FieldTransitionPixelate(1, -1, 0, 0);
                                break;
                            case Global.BossIDs.MOTHER:
                                Global.AudioManager.ChangeSongs(31);
                                Global.World.FieldTransitionPixelate(1, -1, 0, 0);
                                break;
                        }
                    }
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (_state)
            {
                default:
                case AnkhStates.VISIBLE:
                    _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                    break;
                case AnkhStates.USABLE:
                    _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                    if (_sparklingParticles.IsPlaying)
                        _sparklingParticles.Draw(spriteBatch, Position + new Vector2(0, World.CHIP_SIZE));
                    break;
                case AnkhStates.ACTIVATED:
                    break;
            }
        }
    }
}