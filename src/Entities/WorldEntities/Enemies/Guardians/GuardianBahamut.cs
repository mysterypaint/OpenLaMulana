using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities.Enemies.Guardians
{
    internal class GuardianBahamut : ParentGuardianEntity
    {
        private Global.WEStates _state = Global.WEStates.INIT;
        private View _bossRoom = null;
        private int _beginningWaitTimer = 0;
        private int speedUpTimer = 300;
        private int[] _movementFrames = new int[] { 0, 2, 5, 7, 9, 15, 23, 50, 80, 120, 180, 220, 260 };
        private View[] _bossViews = Global.World.GetField(4).GetBossViews();
        private View srcView, finalView = null;
        private View[] backupView = new View[3];
        private int _timesShifted = 0;

        public GuardianBahamut(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags, Global.SpriteDefs sprSheetIndex) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            InitAssembly(sprSheetIndex);

            Position += new Vector2(200, -3);
            Visible = false;

            srcView = _bossViews[0];
            finalView = _bossViews[1];

            //_currActiveView = Global.World.GetCurrentView();

            //finalView.InitChipData(0, null);
            for (int ty = World.ROOM_HEIGHT - 1 - 3; ty < World.ROOM_HEIGHT; ty++)
            {
                for (int tx = 0; tx < World.ROOM_WIDTH; tx++)
                {
                    Chip destChip = srcView.Chips[tx % 4, ty];
                    finalView.Chips[tx, ty].CloneTile(destChip);
                }
            }

            _state = Global.WEStates.INIT;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!Visible)
                return;
            if (Global.DevModeEnabled)
            {
                HelperFunctions.DrawSplashscreen(spriteBatch, true, Color.Gray);
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
                    if (Global.Camera.GetState() == System.Camera.CamStates.NONE)
                    {
                        _bossRoom = srcView;

                        _beginningWaitTimer = 30;
                        _state = Global.WEStates.ACTIVATING;

                    }
                    break;
                case Global.WEStates.ACTIVATING:
                    if (_beginningWaitTimer <= 0)
                    {
                        _beginningWaitTimer = 0;
                        _state = Global.WEStates.SPEEDING_UP;
                    }
                    else
                    {
                        if (Global.AnimationTimer.OneFrameElapsed())
                            _beginningWaitTimer--;
                    }
                    break;
                case Global.WEStates.SPEEDING_UP:
                    if (Global.AnimationTimer.OneFrameElapsed())
                    {
                        if (speedUpTimer > 0)
                            speedUpTimer--;
                    }

                    foreach (int i in _movementFrames)
                    {
                        if (i == speedUpTimer)
                            ShiftScreenRight();
                    }
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

        private void ShiftScreenRight()
        {
            // Grab the left-most column of the boss arena

            Chip[] rightMostColumn = new Chip[World.ROOM_HEIGHT];

            // Check if we've already shifted out of the first room

            for (int y = 0; y < World.ROOM_HEIGHT; y++)
            {
                rightMostColumn[y] = finalView.Chips[World.ROOM_WIDTH - 1 - _timesShifted % 2, y];
            }

            // Shift every single tile in the room toward the left; The left-most column will be written on the far right of the room, effectively wrapping the screen
            _bossRoom.ShiftTiles(World.VIEW_DIR.RIGHT, rightMostColumn);

            _timesShifted++;
        }
    }
}