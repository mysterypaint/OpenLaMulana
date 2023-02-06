using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GreatAnkh : ParentInteractableWorldEntity
    {
        enum AnkhStates : int
        {
            VISIBLE,
            USABLE,
            ACTIVATED,
            MAX
        }
        private SpriteAnimation _sparklingParticles = new SpriteAnimation();
        private AnkhStates _state = AnkhStates.VISIBLE;
        private IGameEntity _activatedGuardian = null;
        private Global.BossIDs _bossID = Global.BossIDs.MOTHER;
        private int _bossBGM = -1;
        private int _alwaysEight = 8;       // No idea what this does... The 8 seems hardcoded-yet-not-actually-hardcoded...?
        private int _activationTimer = 0;
        private AnkhParticle _ankhActivatedParticle = null;

        public GreatAnkh(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _sprIndex = new Sprite(_tex, 0, 64, 16, 16);
            _sparklingParticles = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(0, 32), 16, 16, new Point(16, 0), 3, 0.05f);

            _alwaysEight = op1;
            _bossBGM = op3;

            _state = AnkhStates.USABLE;

            Depth = (int)Global.DrawOrder.AboveEntitiesGraphicDisplay;
            if (_state == AnkhStates.USABLE)
            {
                _sparklingParticles.Play();
            }
        }

        ~GreatAnkh()
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
                    if (ViewCoords.X == _world.CurrViewX && ViewCoords.Y == _world.CurrViewY && _parentView.GetParentField().ID == Global.World.CurrField)
                    {
                        if (Global.AudioManager.IsPlaying() != 20)
                            Global.AudioManager.ChangeSongs(20);

                        if (CollidesWithPlayer()) {
                            if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.MAIN_WEAPON))
                            {
                                _state = AnkhStates.ACTIVATED;
                                _activationTimer = 30;

                                if (Global.AudioManager.IsPlaying() >= 0)
                                    Global.AudioManager.StopMusic();
                                Global.AudioManager.PlaySFX(SFX.ANKH_ACTIVATED);
                                _ankhActivatedParticle = (AnkhParticle)InstanceCreate(new AnkhParticle((int)Position.X, (int)Position.Y - 16, 0, 0, 0, 0, true, null, null));
                            }
                        }
                    }
                    break;
                case AnkhStates.ACTIVATED:
                    if (_activationTimer > 0)
                        _activationTimer--;
                    else
                    {
                        _activationTimer = 0;

                        View[] bossViews = Global.World.GetCurrField().GetBossViews();
                        //FieldTransitionCardinalBoss
                        _bossID = (Global.BossIDs)Global.World.GetCurrField().GetBossID();

                        Global.Camera.SetState((int)System.Camera.CamStates.STANDBY);
                        Global.AudioManager.ChangeSongs(_bossBGM);
                        Global.World.FieldTransitionPixelate(1, -1, 0, 0);
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