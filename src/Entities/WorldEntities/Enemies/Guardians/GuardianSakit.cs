using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities.Enemies.Guardians
{
    internal class GuardianSakit : ParentAssembledInteractiveWorldEntity
    {
        private int _ts = World.CHIP_SIZE;
        private int _initTimer = 0;
        private Global.WEStates _state = Global.WEStates.INIT;
        private View[] _bossViews = null;

        enum SakitSprites
        {
            Idle,
            Awake,
            Walking,
            WalkingFeet,
            WalkingFeet2,
            ExtendedHand,
            HandFiring,
            Max
        };

        public GuardianSakit(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags, Global.SpriteDefs sprSheetIndex) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            InitAssembly(sprSheetIndex);

            Position = new Vector2(5 * _ts, 4 * _ts);
            Visible = false;

            _bossViews = Global.World.GetCurrField().GetBossViews();
            _initTimer = 50;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!Visible)
                return;
            if (Global.DevModeEnabled)
            {
                if (CollidesWithPlayer(Position))
                {
                    if (_sprIndex.TintColor != Color.Red)
                        Global.AudioManager.PlaySFX(SFX.SHIELD_BLOCK);
                    _sprIndex.TintColor = Color.Red;
                }
                else
                {
                    _sprIndex.TintColor = Color.White;
                }
            }
            _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            switch (_state)
            {
                default:
                    break;
                case Global.WEStates.INIT:
                    if (Global.AnimationTimer.OneFrameElapsed())
                    {
                        if (_initTimer <= 0)
                        {
                            _initTimer = 10;
                            _state = Global.WEStates.ACTIVATING;
                            Visible = true;
                            Global.World.FieldTransitionImmediate(_bossViews[0], _bossViews[1], false, false);
                        }
                        else
                        {
                            _initTimer--;
                        }
                    }
                    break;
                case Global.WEStates.ACTIVATING:
                    if (Global.AnimationTimer.OneFrameElapsed())
                    {
                        if (_initTimer <= 0)
                        {
                            _initTimer = 10;
                            _state = Global.WEStates.IDLE;
                        }
                        else
                        {
                            _initTimer--;
                        }
                    }
                    break;
                case Global.WEStates.IDLE:

                    break;
            }

            if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.JUMP))
            {
                _sprDefIndex++;
                if (_sprDefIndex >= _spritesMax)
                    _sprDefIndex = 0;

                UpdateSpriteIndex();
                UpdateMaskIndex();
            }
        }
    }
}