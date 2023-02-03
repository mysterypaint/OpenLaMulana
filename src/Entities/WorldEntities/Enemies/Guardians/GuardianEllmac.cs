using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities.Enemies.Guardians
{
    internal class GuardianEllmac : ParentGuardianEntity
    {
        private int _framesBeforeDrop = 0;
        private Global.WEStates _state = Global.WEStates.INIT;
        private View _bossRoom = null;
        private int _speedUpTimer = 30;

        public GuardianEllmac(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags, Global.SpriteDefs sprSheetIndex) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            InitAssembly(sprSheetIndex);

            Position = new Vector2(-9999, -9999);
            Visible = false;

            _framesBeforeDrop = 15;
            _state = Global.WEStates.INIT;
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
                    if (Global.Camera.GetState() == Camera.CamStates.NONE)
                    {
                        if (_framesBeforeDrop > 0)
                        {
                            _framesBeforeDrop--;
                        }
                        else
                        {
                            _framesBeforeDrop = 0;
                            View srcView = Global.World.GetField(3).GetBossViews()[0];//new View(World.ROOM_WIDTH, World.ROOM_HEIGHT, Global.World.GetField(3), 0, 0);
                            View destView = new View(World.ROOM_WIDTH, World.ROOM_HEIGHT, null, 0, 0);
                            destView.InitChipData(0, null);

                            for (int y = 0; y < 4; y++)
                            {
                                for (int x = 0; x < World.ROOM_WIDTH; x++)
                                {
                                    destView.Chips[x, y].TileID = 36 + x % 4 + y * 40;
                                }
                            }
                            for (int x = 0; x < World.ROOM_WIDTH; x++)
                            {
                                destView.Chips[x, World.ROOM_HEIGHT - 1].TileID = 26 + x % 2;
                            }

                            _bossRoom = destView;

                            Global.World.FieldTransitionCardinalBoss(World.VIEW_DIR.DOWN, srcView, destView, Global.TextureManager.GetTexture(Global.Textures.BOSS02), this);

                            Visible = true;
                            Position = new Vector2(-32, World.ROOM_HEIGHT * 2 * World.CHIP_SIZE - (1 * World.CHIP_SIZE));
                            _state = Global.WEStates.ACTIVATING;
                        }
                    }
                    break;
                case Global.WEStates.ACTIVATING:
                    if (Global.Camera.GetState() == Camera.CamStates.NONE)
                    {
                        _state = Global.WEStates.SPEEDING_UP;
                        Global.Main.SetState(Global.GameState.PLAYING);
                    }
                    break;
                case Global.WEStates.SPEEDING_UP:
                    if (_speedUpTimer > 0)
                        _speedUpTimer--;

                    if (_speedUpTimer >= 23)
                        Position = Vector2.Zero;

                    if (_speedUpTimer <= 0 || _speedUpTimer == 2 || _speedUpTimer == 5 || _speedUpTimer == 7 || _speedUpTimer == 9 || _speedUpTimer == 15 || _speedUpTimer == 23)
                        ShiftScreenLeft();
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

        private void ShiftScreenLeft()
        {
            // Grab the left-most column of the boss arena
            Chip[] leftMostColumn = new Chip[World.ROOM_HEIGHT];
            for (int y = 0; y < World.ROOM_HEIGHT; y++)
            {
                leftMostColumn[y] = _bossRoom.Chips[0, y];
            }

            // Shift every single tile in the room toward the left; The left-most column will be written on the far right of the room, effectively wrapping the screen
            _bossRoom.ShiftTiles(World.VIEW_DIR.LEFT, leftMostColumn);
        }
    }
}