﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using static OpenLaMulana.Entities.World;

namespace OpenLaMulana.Entities
{

    public class Protag : IGameEntity, ICollidable
    {
        public enum Facing
        {
            LEFT,
            UP,
            RIGHT,
            DOWN
        };

        private Sprite _idleSprite;

        public PlayerState State { get; private set; }
        private PlayerState _prevState = PlayerState.IDLE;
        public Vector2 Position { get; set; }
        public Facing FacingX = Facing.RIGHT;
        public Facing FacingY = Facing.DOWN;
        public short BBoxOriginX { get; set; }
        public short BBoxOriginY { get; set; }
        public int DrawOrder { get; set; }
        public Effect ActiveShader { get; set; } = null;

        private const int HudTileHeight = 2;
        private int _jumpSpeed = 5;
        private short _moveX = 0;
        private short _moveY = 0;
        private float _grav = 0.34f;
        private float _hsp = 0;
        private float _vsp = 0;
        private float _chipWidth, _chipHeight;
        private float _moveSpeed = 1f;
        private double _gravMax = 0.8;

        private bool _grounded = false;
        private bool _hasBoots = true;
        private bool _hasFeather = true;


        /*
         * Weapon attack powerThe attack power of each weapon is as follows. This number also applies to breaking wall events, etc.

Whip: 2 Knife: 3 Chain Whip: 4 Ax: 5 Sword: 5 Key Sword: 1 Mace: 6
Shuriken: 2 Throwing Sword: 3 (Penetrating) Cannon: 4 Spear: 3 Bomb: 2x4 Gun: 20
rom Strengthening by cartridge combination is as follows.
Video Hustler + Break Shot: Knife and key sword attack power +2
Castlevania Dracula + Tile Magician: Whip attack power +2
        */

        public Rectangle CollisionBox
        {
            get
            {
                Rectangle box = new Rectangle(
                    (int)Math.Round(Position.X - BBoxOriginX),
                    (int)Math.Round(Position.Y - BBoxOriginY),
                    9,
                    11
                );
                //box.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);
                return box;
            }
        }

        public const int SPRITE_WIDTH = 16;
        public const int SPRITE_HEIGHT = 16;

        public Protag(Vector2 position)
        {
            Position = position;

            Initialize();

            Texture2D spriteSheet = Global.TextureManager.GetTexture(Global.Textures.PROT1);
            _idleSprite = new Sprite(spriteSheet, 0, 0, 16, 16, 8, 16);
        }

        public void Initialize()
        {
            State = PlayerState.IDLE;
            _chipWidth = World.CHIP_SIZE;
            _chipHeight = World.CHIP_SIZE;
            BBoxOriginX = 5;
            BBoxOriginY = 12;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _idleSprite.Draw(spriteBatch, Position);

            RectangleSprite.DrawRectangle(spriteBatch, CollisionBox, Color.Red, 1);
        }

        public void Update(GameTime gameTime)
        {
            var chipline = Global.World.GetField(Global.World.CurrField).GetChipline();

            CollideAndMove(_moveSpeed, _moveSpeed, chipline);
            _prevState = State;
        }

        private void CollideAndMove(float dx, float dy, int[] chipline)
        {
            var posX = Position.X;
            var posY = Position.Y;

            int bboxTileXMin, bboxTileXMax, bboxTileYMin, bboxTileYMax;
            var bboxLeft = CollisionBox.Left;
            var bboxRight = CollisionBox.Right;
            var bboxTop = CollisionBox.Top;
            var bboxBottom = CollisionBox.Bottom;
            /*
            Decompose movement into X and Y axes, step one at a time. If you’re planning on implementing slopes afterwards, step X first, then Y.
            Otherwise, the order shouldn’t matter much. Then, for each axis:
            */

            // Step X

            // Get the coordinate of the forward-facing edge, e.g. : If walking left, the x coordinate of left of bounding box.
            //  If walking right, x coordinate of right side.If up, y coordinate of top, etc.

            _moveX = Global.InputController.DirMoveX;
            if (_moveX == 1)
                FacingX = Facing.RIGHT;
            else if (_moveX == -1)
                FacingX = Facing.LEFT;

            View currRoom = Global.World.GetActiveViews()[(int)AViews.CURR].GetView();// (Global.World.CurrField).GetMapData()[Global.World.CurrViewX, Global.World.CurrViewY]; // TODO: Update this variable only whenever a map change occurs

            if (_moveX != 0)
            {
                if (FacingX == Facing.RIGHT)
                {

                    // Figure which lines of tiles the bounding box intersects with – this will give you a minimum and maximum tile value on the OPPOSITE axis.
                    // For example, if we’re walking left, perhaps the player intersects with horizontal rows 32, 33 and 34(that is, tiles with y = 32 * TS, y = 33 * TS, and y = 34 * TS, where TS = tile size).

                    dx = _moveSpeed;

                    bboxTileYMin = (int)Math.Floor(bboxTop / _chipHeight) - HudTileHeight;
                    bboxTileYMax = (int)Math.Floor(bboxBottom / _chipHeight) - HudTileHeight;

                    bboxTileXMin = (int)Math.Floor(bboxRight / _chipWidth);
                    bboxTileXMax = (int)Math.Floor((bboxRight + dx) / _chipWidth) + 1;


                    // Scan along those lines of tiles and towards the direction of movement until you find the closest static obstacle. Then loop through every moving obstacle,
                    // and determine which is the closest obstacle that is actually on your path.
                    var closestTile = 255;
                    for (var y = bboxTileYMin; y <= bboxTileYMax; y++)
                    {
                        for (var x = bboxTileXMax; x >= bboxTileXMin; x--)
                        {
                            if (x >= 0 && y >= 0 && x < World.ROOM_WIDTH && y < World.ROOM_HEIGHT)
                            {
                                if (currRoom.Chips[x, y].TileID >= chipline[0] && currRoom.Chips[x, y].TileID < chipline[1])
                                {
                                    if (closestTile > x)
                                        closestTile = x;
                                }
                            }
                            else
                            {
                                /*
                                if (x < 0)
                                    closestTile = 0;
                                else
                                    closestTile = World.ROOM_WIDTH;
                                */
                            }
                        }
                    }

                    // The total movement of the player along that direction is then the minimum between the distance to closest obstacle,
                    // and the amount that you wanted to move in the first place.
                    int tx = closestTile * World.CHIP_SIZE;
                    dx = Math.Min(dx, tx - bboxRight + 1);

                    // Move player to the new position.

                    if (bboxRight + dx >= tx)
                    {
                        posX = tx - BBoxOriginX;
                        dx = 0;
                    }
                    posX += dx;

                }
                else if (FacingX == Facing.LEFT)
                {

                    // Figure which lines of tiles the bounding box intersects with – this will give you a minimum and maximum tile value on the OPPOSITE axis.
                    // For example, if we’re walking left, perhaps the player intersects with horizontal rows 32, 33 and 34(that is, tiles with y = 32 * TS, y = 33 * TS, and y = 34 * TS, where TS = tile size).

                    dx = -_moveSpeed;

                    bboxTileYMin = (int)Math.Floor(bboxTop / _chipHeight) - HudTileHeight;
                    bboxTileYMax = (int)Math.Floor(bboxBottom / _chipHeight) - HudTileHeight;

                    bboxTileXMin = (int)Math.Floor(bboxLeft / _chipWidth);
                    bboxTileXMax = (int)Math.Floor((bboxLeft + dx) / _chipWidth) - 1;


                    // Scan along those lines of tiles and towards the direction of movement until you find the closest static obstacle. Then loop through every moving obstacle,
                    // and determine which is the closest obstacle that is actually on your path.
                    var closestTile = -255;
                    for (var y = bboxTileYMin; y <= bboxTileYMax; y++)
                    {
                        for (var x = bboxTileXMax; x <= bboxTileXMin; x++)
                        {
                            if (x >= 0 && y >= 0 && x < World.ROOM_WIDTH && y < World.ROOM_HEIGHT)
                            {
                                if (currRoom.Chips[x, y].TileID >= chipline[0] && currRoom.Chips[x, y].TileID < chipline[1])
                                {
                                    if (closestTile < x)
                                        closestTile = x;
                                }
                            }
                            else
                            {
                                /*
                                if (x < 0)
                                    closestTile = -1;
                                else
                                    closestTile = World.ROOM_WIDTH;
                                */
                            }
                        }
                    }
                    //closestTile = 1;
                    // The total movement of the player along that direction is then the minimum between the distance to closest obstacle,
                    // and the amount that you wanted to move in the first place.
                    int tx = (closestTile * World.CHIP_SIZE) + World.CHIP_SIZE - 1;

                    //movingDistance = Math.Sign(movingDistance) * Math.Min(Math.Abs(movingDistance), Math.Abs(bboxLeft - tx - 1));

                    // Move player to the new position.

                    if (bboxLeft + dx <= tx)
                    {
                        posX = tx + BBoxOriginX + 1;
                        dx = 0;
                    }
                    posX += dx;
                }
            }

            // With this new position, step the other coordinate, if still not done.
            Position = new Vector2(posX, posY);

            // Step Y
            if (Global.InputController.KeyJumpPressed)
                Global.AudioManager.PlaySFX(SFX.P_JUMP);

            if (Global.InputController.KeyJumpHeld && dy <= 0 && _grounded)
            {
                State = PlayerState.JUMPING;
            }

            _moveY = Global.InputController.DirMoveY;
            if (_moveY < 0)
            {
                FacingY = Facing.UP;
            }
            else
            {
                FacingY = Facing.DOWN;
            }


            if (_moveY != 0)
            {
                if (FacingY == Facing.DOWN)
                {
                    bboxTileYMin = (int)Math.Floor(bboxBottom / _chipHeight) - HudTileHeight;
                    bboxTileYMax = (int)Math.Floor((bboxBottom + dy) / _chipHeight) - HudTileHeight;

                    bboxTileXMin = (int)Math.Floor(bboxLeft / _chipWidth);
                    bboxTileXMax = (int)Math.Floor(bboxRight / _chipWidth);

                    var closestTile = 255;
                    for (var x = bboxTileXMin; x <= bboxTileXMax; x++)
                    {
                        for (var y = bboxTileYMin; y <= bboxTileYMax; y++)
                        {
                            if (x >= 0 && y >= 0 && x < World.ROOM_WIDTH && y < World.ROOM_HEIGHT)
                            {
                                if (currRoom.Chips[x, y].TileID >= chipline[0] && currRoom.Chips[x, y].TileID < chipline[1])
                                {
                                    if (closestTile > y)
                                        closestTile = y;
                                }
                            }
                            else
                            {
                                // TODO: Treat screen edges like walls if the View doesn't transition to another screen in all 4 directions. Is most likely how the original game is coded, too (do not treat it like a floor, though: let the player keep falling
                                /*
                                if (y < 0)
                                    closestTile = -1;
                                else
                                    closestTile = World.ROOM_HEIGHT;
                                */
                            }
                        }
                    }

                    int ty = (closestTile + HudTileHeight) * World.CHIP_SIZE;

                    if (bboxBottom + dy >= ty)
                    {
                        posY = ty;
                        dy = 0;
                    }
                    posY += dy;

                }
                else if (FacingY == Facing.UP)
                {
                    dy = -_moveSpeed;

                    bboxTileYMin = (int)Math.Floor(bboxTop / _chipHeight);
                    bboxTileYMax = (int)Math.Floor((bboxTop + dy) / _chipHeight) - 1;

                    bboxTileXMin = (int)Math.Floor(bboxLeft / _chipWidth);
                    bboxTileXMax = (int)Math.Floor(bboxRight / _chipWidth);

                    var closestTile = -255;
                    for (var x = bboxTileXMin; x <= bboxTileXMax; x++)
                    {
                        for (var y = bboxTileYMax; y < bboxTileYMin; y++)
                        {
                            if (x >= 0 && y >= HudTileHeight && x < World.ROOM_WIDTH && y < World.ROOM_HEIGHT + HudTileHeight)
                            {
                                if (currRoom.Chips[x, y - HudTileHeight].TileID >= chipline[0] && currRoom.Chips[x, y - HudTileHeight].TileID < chipline[1])
                                {
                                    if (y - HudTileHeight > closestTile)
                                        closestTile = y - HudTileHeight;
                                }
                            }
                            else
                            {
                                /*
                                if (y < 0)
                                    closestTile = -1 + HUD_TILE_HEIGHT;
                                else
                                    closestTile = World.ROOM_HEIGHT - HUD_TILE_HEIGHT;
                                */
                            }
                        }
                    }

                    int ty = ((closestTile + HudTileHeight) * World.CHIP_SIZE) + World.CHIP_SIZE - 1;
                    if (bboxTop + dy <= ty)
                    {
                        posY = ty + BBoxOriginY + 1;
                        dy = 0;
                    }
                    posY += dy + _vsp;
                }
            }

            // Update the actual position
            Position = new Vector2(posX, posY);
            /*
            if (Global.World == null)
                return false;
            var currField = Global.World.GetField(Global.World.currField);
            var rx = Global.World.currRoomX;
            var ry = Global.World.currRoomY;
            var currRoom = currField.GetMapData()[rx, ry];

            var rtx = Math.Floor(xCheck / World.tileWidth);
            var rty = Math.Floor(yCheck / World.tileHeight);

            var chipline = currField.GetChipline();

            if (!(rtx >= 0 && rtx <= World.ROOM_WIDTH - 1 &&
                rty >= 0 && rty <= World.ROOM_HEIGHT - 1))
                return true;

            var checkingTileID = currRoom.Tiles[(int)rtx, (int)rty]._tileID;

            if (checkingTileID >= chipline[0] && checkingTileID < chipline[1])
                return true;
            else
                return false;
            */
        }

        internal string PrintState()
        {
            switch (State)
            {
                case PlayerState.IDLE:
                    return "Idle";
                case PlayerState.CUTSCENE:
                    return "Cutscene";
                case PlayerState.FALLING:
                    return "Falling";
                case PlayerState.JUMPING:
                    return "Jumping";
                case PlayerState.MAX:
                    return "Max";
                case PlayerState.PAUSED:
                    return "Paused";
                case PlayerState.WALKING:
                    return "Walking";
                case PlayerState.WHIPPING:
                    return "Whipping";
            }
            return "Undefined";
        }

        public void SetHsp(int hsp)
        {
            _hsp = hsp;
        }

        public void SetVsp(int vsp)
        {
            _vsp = vsp;
        }

        public void ApplyVector(Vector2 offset)
        {
            Position += offset;
        }

        internal bool IsGrounded()
        {
            return _grounded;
        }
    }
}
