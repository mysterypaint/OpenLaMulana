using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Entities.World;

namespace OpenLaMulana.Entities.WorldEntities.Enemies.Guardians
{
    internal class GuardianViy : ParentGuardianEntity
    {
        private View _bossRoom = null;
        private View _bossRoomUnmodified = null;
        private View _currActiveView = null;
        private int _moveTimerMax = 20;
        private int _moveTimer = 0;
        private int _shiftYSides = ROOM_HEIGHT - 1;
        private int _shiftYMiddle = ROOM_HEIGHT;
        private int _ts = CHIP_SIZE;
        private SpriteAnimation anim;

        enum ViySprites
        {
            MAIN_BODY,
            TENTACLE1_1,
            TENTACLE1_2,
            TENTACLE1_3,

            TENTACLE2_1,
            TENTACLE2_2,
            TENTACLE2_3,

            TENTACLE3_1,
            TENTACLE3_2,
            TENTACLE3_3,

            TENTACLE4_1,
            TENTACLE4_2,
            TENTACLE4_3,

            TENTACLE_BULLET,

            EYE_BULLET,

            CLOSING_EYE,
            CLOSED_EYE,
            OPEN_EYE,
            GIANT_LASER,
            EXTENDED_EYE,
            IMP_RIGHT,
            IMP_RIGHT_2,
            IMP_LEFT
        };

        private GuardianViyTentacle[] _tentacles = new GuardianViyTentacle[4];
        private GuardianViyEye _eye = null;


        public GuardianViy(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags, Global.SpriteDefs sprSheetIndex) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            InitAssembly(sprSheetIndex);

            _bossRoom = Global.World.GetField(Global.World.CurrField).GetBossViews()[1].CloneView();
            _bossRoomUnmodified = _bossRoom.CloneView();
            _currActiveView = Global.World.GetCurrentView();
            _moveTimer = _moveTimerMax;
            for (int ty = 0; ty <= 4; ty++)
            {
                for (int tx = 7; tx <= 8; tx++)
                {
                    _bossRoom.Chips[tx, ty].CloneTile(_bossRoom.Chips[6, 0]);
                }
            }

            int[] xPos = new int[] { 3, 7, 22, 26 };
            int[] yPos = new int[] { 17, 16, 16, 17 };
            for (int i = 0; i < 4; i++)
            {
                _tentacles[i] = (GuardianViyTentacle)InstanceCreatePersistent(new GuardianViyTentacle(x, y, -1, -1, -1, -1, true, null, null));

                Sprite[] tentSprites = new Sprite[] { _mySprites[(int)ViySprites.TENTACLE1_1 + i * 3],
                _mySprites[(int)ViySprites.TENTACLE1_2 + i * 3],
                _mySprites[(int)ViySprites.TENTACLE1_3 + i * 3]
                };

                anim = SpriteAnimation.CreateSimpleAnimation(tentSprites, 0.25f);
                _tentacles[i].Init(anim, new Vector2(
                    xPos[i] * _ts,
                    yPos[i] * _ts
                    ));
            }

            Sprite[] eyeSprites = new Sprite[] { _mySprites[(int)ViySprites.CLOSED_EYE],
            _mySprites[(int)ViySprites.CLOSING_EYE],
            _mySprites[(int)ViySprites.OPEN_EYE],
            _mySprites[(int)ViySprites.EXTENDED_EYE]};
            _eye = (GuardianViyEye)InstanceCreatePersistent(new GuardianViyEye(x, y, -1, -1, -1, -1, true, null, null));
            _eye.Init(eyeSprites, new Vector2(11 * _ts, 18 * _ts), this);

            Position = new Vector2(_ts, 16 * _ts);
        }

        ~GuardianViy()
        {
            foreach (GuardianViyTentacle t in _tentacles)
            {
                InstanceDestroy(t);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
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
            if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.JUMP))
            {
                _sprDefIndex++;
                if (_sprDefIndex >= _spritesMax)
                    _sprDefIndex = 0;

                UpdateSpriteIndex();
                UpdateMaskIndex();
            }

            if (Global.Camera.GetState() == Camera.CamStates.NONE)
            {
                if (Global.AnimationTimer.OneFrameElapsed())
                {
                    if (_moveTimer <= 0)
                    {
                        Global.World.FieldTransitionImmediate(_bossRoom, _bossRoom, false, false);
                        ShiftScreenDown();
                        _moveTimer = _moveTimerMax;
                    }
                    else
                        _moveTimer--;
                }
            }
        }


        private void ShiftScreenDown()
        {
            // Grab the bottom-most column of the boss arena
            Chip[] bottomMostRow = new Chip[ROOM_WIDTH];
            for (int x = 1; x < ROOM_WIDTH - 1; x++)
            {
                if (_shiftYMiddle < ROOM_HEIGHT)
                    bottomMostRow[x] = _bossRoomUnmodified.Chips[x, _shiftYMiddle];
                else // Grab the top-most row if we just finished looping through the whole room
                    bottomMostRow[x] = _bossRoomUnmodified.Chips[x, 0];


            }
            bottomMostRow[0] = _bossRoomUnmodified.Chips[0, _shiftYSides];
            bottomMostRow[ROOM_WIDTH - 1] = _bossRoomUnmodified.Chips[ROOM_WIDTH - 1, _shiftYSides];

            // Shift every single tile in the room toward the top; The bottom-most column will be written at the top of the room, effectively wrapping the screen
            _bossRoom.ShiftTiles(VIEW_DIR.DOWN, bottomMostRow);

            // Update the current room's collision tiles to reflect the updated visual tiles
            _currActiveView.Chips = _bossRoom.Chips;

            _shiftYSides--;
            _shiftYMiddle--;
            if (_shiftYSides < 0)
                _shiftYSides = ROOM_HEIGHT - 1;
            if (_shiftYMiddle < 0)
                _shiftYMiddle = ROOM_HEIGHT;

            if (_protag.IsGrounded())
            {
                _protag.ApplyVector(new Vector2(0, 8));
            }
        }
    }
}