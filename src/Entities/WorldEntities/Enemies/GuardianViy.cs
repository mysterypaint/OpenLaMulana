using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Entities.World;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GuardianViy : IGlobalWorldEntity
    {
        private int spritesMax = 24;
        private Sprite[] _sprites = new Sprite[24];
        private int _sprNum = 0;
        private View _bossRoom = null;
        private View _bossRoomUnmodified = null;
        private View _currActiveView = null;
        private int _moveTimerMax = 20;
        private int _moveTimer = 0;
        private int _shiftYSides = World.ROOM_HEIGHT - 1;
        private int _shiftYMiddle = World.ROOM_HEIGHT;
        private Protag _protag = null;
        private int _ts = World.CHIP_SIZE;
        private SpriteAnimation anim;

        enum ViySprites
        {
            MainBody,
            Tentacle1_1,
            Tentacle1_2,
            Tentacle1_3,

            Tentacle2_1,
            Tentacle2_2,
            Tentacle2_3,

            Tentacle3_1,
            Tentacle3_2,
            Tentacle3_3,

            Tentacle4_1,
            Tentacle4_2,
            Tentacle4_3,

            TentacleBullet,

            EyeBullet,

            ClosingEye,
            ClosedEye,
            OpenEye,
            GiantLaser,
            ExtendedEye,
            ImpRight,
            ImpRight_2,
            ImpLeft
        };

        private GuardianViyTentacle[] _tentacles = new GuardianViyTentacle[4];
        private GuardianViyEye _eye = null;


        public GuardianViy(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.SpriteDefManager.GetTexture(Global.SpriteDefs.BOSS04);
            for (var i = 0; i < spritesMax; i++)
            {
                _sprites[i] = Global.SpriteDefManager.GetSprite(Global.SpriteDefs.BOSS04, i);
            }
            _sprIndex = _sprites[_sprNum];

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

                Sprite[] tentSprites = new Sprite[] { _sprites[(int)ViySprites.Tentacle1_1 + (i * 3)],
                _sprites[(int)ViySprites.Tentacle1_2 + (i * 3)],
                _sprites[(int)ViySprites.Tentacle1_3 + (i * 3)]
                };

                anim = SpriteAnimation.CreateSimpleAnimation(tentSprites, 0.25f);
                _tentacles[i].Init(anim, new Vector2(
                    xPos[i] * _ts,
                    yPos[i] * _ts
                    ));
            }

            Sprite[] eyeSprites = new Sprite[] { _sprites[(int)ViySprites.ClosedEye],
            _sprites[(int)ViySprites.ClosingEye],
            _sprites[(int)ViySprites.OpenEye],
            _sprites[(int)ViySprites.ExtendedEye]};
            _eye = (GuardianViyEye)InstanceCreatePersistent(new GuardianViyEye(x, y, -1, -1, -1, -1, true, null, null));
            _eye.Init(eyeSprites, new Vector2(11 * _ts, 18 * _ts), this);

            Position = new Vector2(_ts, 16 * _ts);
            _protag = Global.World.GetProtag();
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
            _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            if (InputManager.PressedKeys[(int)Global.ControllerKeys.WHIP])
            {
                _sprNum++;
                if (_sprNum >= spritesMax)
                    _sprNum = 0;
            }
            _sprIndex = _sprites[_sprNum];

            if (Global.Camera.GetState() == System.Camera.CamStates.NONE) {
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
            Chip[] bottomMostRow = new Chip[World.ROOM_WIDTH];
            for (int x = 1; x < World.ROOM_WIDTH - 1; x++)
            {
                if (_shiftYMiddle < World.ROOM_HEIGHT)
                    bottomMostRow[x] = _bossRoomUnmodified.Chips[x, _shiftYMiddle];
                else // Grab the top-most row if we just finished looping through the whole room
                    bottomMostRow[x] = _bossRoomUnmodified.Chips[x, 0];


            }
            bottomMostRow[0] = _bossRoomUnmodified.Chips[0, _shiftYSides];
            bottomMostRow[World.ROOM_WIDTH - 1] = _bossRoomUnmodified.Chips[World.ROOM_WIDTH - 1, _shiftYSides];

            // Shift every single tile in the room toward the top; The bottom-most column will be written at the top of the room, effectively wrapping the screen
            _bossRoom.ShiftTiles(World.VIEW_DIR.DOWN, bottomMostRow);
            
            // Update the current room's collision tiles to reflect the updated visual tiles
            _currActiveView.Chips = _bossRoom.Chips;

            _shiftYSides--;
            _shiftYMiddle--;
            if (_shiftYSides < 0)
                _shiftYSides = World.ROOM_HEIGHT - 1;
            if (_shiftYMiddle < 0)
                _shiftYMiddle = World.ROOM_HEIGHT;

            if (_protag.IsGrounded()) {
                _protag.ApplyVector(new Vector2(0, 8));
            }
        }
    }
}