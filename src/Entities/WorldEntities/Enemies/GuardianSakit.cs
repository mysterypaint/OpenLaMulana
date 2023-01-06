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
        Sprite[] sprites = new Sprite[7];
        private int _sprNum = 0;
        private int _ts = World.CHIP_SIZE;
        private int _initTimer = 0;
        private Global.EnemyStates _state = Global.EnemyStates.INIT;
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

        public GuardianSakit(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.SpriteDefManager.GetTexture(Global.SpriteDefs.BOSS01);
            for (int i = 0; i < (int)SakitSprites.Max; i++)
            {
                sprites[i] = Global.SpriteDefManager.GetSprite(Global.SpriteDefs.BOSS01, i);
            }
            _sprIndex = sprites[_sprNum];
            Position = new Vector2(5 * _ts, 4 * _ts);
            Visible = false;

            _bossViews = Global.World.GetCurrField().GetBossViews();
            _initTimer = 50;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!Visible)
                return;
            _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            switch (_state)
            {
                default:
                    break;
                case Global.EnemyStates.INIT:
                    if (Global.AnimationTimer.OneFrameElapsed()) {
                        if (_initTimer <= 0) {
                            _initTimer = 10;
                            _state = Global.EnemyStates.ACTIVATING;
                            Visible = true;
                            Global.World.FieldTransitionImmediate(_bossViews[0], _bossViews[1], false, false);
                        } else
                        {
                            _initTimer--;
                        }
                    }
                    break;
                case Global.EnemyStates.ACTIVATING:
                    if (Global.AnimationTimer.OneFrameElapsed())
                    {
                        if (_initTimer <= 0)
                        {
                            _initTimer = 10;
                            _state = Global.EnemyStates.IDLE;
                        }
                        else
                        {
                            _initTimer--;
                        }
                    }
                    break;
                case Global.EnemyStates.IDLE:

                    break;
            }

            /*
            int animeSpeed = 6;

             * 
                         if (Global.InputManager.KeyWhipPressed) {
                _sprNum++;
                if (_sprNum >= (int)SakitSprites.Max)
                    _sprNum = 0;
            }
            */
            _sprIndex = sprites[_sprNum];
        }
    }
}