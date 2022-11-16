using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;

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
        PlayerState prev_state = PlayerState.IDLE;

        private World _world;
        private AudioManager _audioManager;

        public Vector2 Position { get; set; }

        public short moveX = 0;
        public short moveY = 0;
        public float hsp = 0;
        public float vsp = 0;

        float grav = 0.34f;
        double grav_max = 0.8;
        bool grounded = false;
        int jump_speed = 5;

        private bool hasBoots = true;
        private bool hasFeather = true;
        private float _tileWidth, _tileHeight;
        float move_speed = 1f;

        public Facing facingX = Facing.RIGHT;
        public Facing facingY = Facing.DOWN;

        private InputController _inputController = null;
        private int HUD_TILE_HEIGHT = 2;

        public short bBoxOriginX { get; set; }
        public short bBoxOriginY { get; set; }

        public int DrawOrder { get; set; }

        public Rectangle CollisionBox
        {
            get
            {
                Rectangle box = new Rectangle(
                    (int)Math.Round(Position.X - bBoxOriginX),
                    (int)Math.Round(Position.Y - bBoxOriginY),
                    9,
                    11
                );
                //box.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);
                return box;
            }
        }

        public Protag(Texture2D spriteSheet, World world, Vector2 position, AudioManager audioManager)
        {
            _world = world;
            Position = position;
            _audioManager = audioManager;

            State = PlayerState.IDLE;

            _tileWidth = World.tileWidth;
            _tileHeight = World.tileHeight;
            bBoxOriginX = 5;
            bBoxOriginY = 12;

            _idleSprite = new Sprite(spriteSheet, 0, 0, 16, 16, 8, 16);
        }

        public void Initialize()
        {
            State = PlayerState.IDLE;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _idleSprite.Draw(spriteBatch, Position);

            RectangleSprite.DrawRectangle(spriteBatch, CollisionBox, Color.Red, 1);
        }

        public void Update(GameTime gameTime)
        {
            var chipline = _world.GetField(_world.currField).GetChipline();

            CollideAndMove(move_speed, move_speed, chipline);
            prev_state = State;
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

            moveX = _inputController.DirMoveX;
            if (moveX == 1)
                facingX = Facing.RIGHT;
            else if (moveX == -1)
                facingX = Facing.LEFT;

            if (moveX != 0)
            {
                View currRoom = _world.GetField(_world.currField).GetMapData()[_world.currRoomX, _world.currRoomY]; // TODO: Update this variable only whenever a map change occurs


                if (facingX == Facing.RIGHT)
                {

                    // Figure which lines of tiles the bounding box intersects with – this will give you a minimum and maximum tile value on the OPPOSITE axis.
                    // For example, if we’re walking left, perhaps the player intersects with horizontal rows 32, 33 and 34(that is, tiles with y = 32 * TS, y = 33 * TS, and y = 34 * TS, where TS = tile size).

                    dx = move_speed;

                    bboxTileYMin = (int)Math.Floor(bboxTop / _tileHeight) - HUD_TILE_HEIGHT;
                    bboxTileYMax = (int)Math.Floor(bboxBottom / _tileHeight) - HUD_TILE_HEIGHT;

                    bboxTileXMin = (int)Math.Floor(bboxRight / _tileWidth);
                    bboxTileXMax = (int)Math.Floor((bboxRight + dx) / _tileWidth) + 1;


                    // Scan along those lines of tiles and towards the direction of movement until you find the closest static obstacle. Then loop through every moving obstacle,
                    // and determine which is the closest obstacle that is actually on your path.
                    var closestTile = 255;
                    for (var y = bboxTileYMin; y <= bboxTileYMax; y++)
                    {
                        for (var x = bboxTileXMax; x >= bboxTileXMin; x--)
                        {
                            if (x >= 0 && y >= 0 && x < Field.RoomWidth && y < Field.RoomHeight)
                            {
                                if (currRoom.Tiles[x, y].tileID >= chipline[0] && currRoom.Tiles[x, y].tileID < chipline[1])
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
                                    closestTile = Field.RoomWidth;
                                */
                            }
                        }
                    }

                    // The total movement of the player along that direction is then the minimum between the distance to closest obstacle,
                    // and the amount that you wanted to move in the first place.
                    int tx = closestTile * World.tileWidth;
                    dx = Math.Min(dx, tx - bboxRight + 1);

                    // Move player to the new position.

                    if (bboxRight + dx >= tx)
                    {
                        posX = tx - bBoxOriginX;
                        dx = 0;
                    }
                    posX += dx;

                }
                else if (facingX == Facing.LEFT)
                {

                    // Figure which lines of tiles the bounding box intersects with – this will give you a minimum and maximum tile value on the OPPOSITE axis.
                    // For example, if we’re walking left, perhaps the player intersects with horizontal rows 32, 33 and 34(that is, tiles with y = 32 * TS, y = 33 * TS, and y = 34 * TS, where TS = tile size).

                    dx = -move_speed;

                    bboxTileYMin = (int)Math.Floor(bboxTop / _tileHeight) - HUD_TILE_HEIGHT;
                    bboxTileYMax = (int)Math.Floor(bboxBottom / _tileHeight) - HUD_TILE_HEIGHT;

                    bboxTileXMin = (int)Math.Floor(bboxLeft / _tileWidth);
                    bboxTileXMax = (int)Math.Floor((bboxLeft + dx) / _tileWidth) - 1;


                    // Scan along those lines of tiles and towards the direction of movement until you find the closest static obstacle. Then loop through every moving obstacle,
                    // and determine which is the closest obstacle that is actually on your path.
                    var closestTile = -255;
                    for (var y = bboxTileYMin; y <= bboxTileYMax; y++)
                    {
                        for (var x = bboxTileXMax; x <= bboxTileXMin; x++)
                        {
                            if (x >= 0 && y >= 0 && x < Field.RoomWidth && y < Field.RoomHeight)
                            {
                                if (currRoom.Tiles[x, y].tileID >= chipline[0] && currRoom.Tiles[x, y].tileID < chipline[1])
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
                                    closestTile = Field.RoomWidth;
                                */
                            }
                        }
                    }
                    //closestTile = 1;
                    // The total movement of the player along that direction is then the minimum between the distance to closest obstacle,
                    // and the amount that you wanted to move in the first place.
                    int tx = (closestTile * World.tileWidth) + World.tileWidth - 1;

                    //movingDistance = Math.Sign(movingDistance) * Math.Min(Math.Abs(movingDistance), Math.Abs(bboxLeft - tx - 1));

                    // Move player to the new position.

                    if (bboxLeft + dx <= tx)
                    {
                        posX = tx + bBoxOriginX + 1;
                        dx = 0;
                    }
                    posX += dx;
                }
            }

            // With this new position, step the other coordinate, if still not done.
            Position = new Vector2(posX, posY);

            // Step Y
            if (_inputController.KeyJumpPressed)
                _audioManager.PlaySFX(SFX.P_JUMP);

            if (_inputController.KeyJumpHeld && dy <= 0 && grounded)
            {
                State = PlayerState.JUMPING;
            }

            moveY = _inputController.DirMoveY;
            if (moveY < 0)
            {
                facingY = Facing.UP;
            }
            else
            {
                facingY = Facing.DOWN;
            }


            if (moveY != 0)
            {
                View currRoom = _world.GetField(_world.currField).GetMapData()[_world.currRoomX, _world.currRoomY]; // TODO: Update this variable only whenever a map change occurs

                if (facingY == Facing.DOWN)
                {
                    bboxTileYMin = (int)Math.Floor(bboxBottom / _tileHeight) - HUD_TILE_HEIGHT;
                    bboxTileYMax = (int)Math.Floor((bboxBottom + dy) / _tileHeight) - HUD_TILE_HEIGHT;

                    bboxTileXMin = (int)Math.Floor(bboxLeft / _tileWidth);
                    bboxTileXMax = (int)Math.Floor(bboxRight / _tileWidth);

                    var closestTile = 255;
                    for (var x = bboxTileXMin; x <= bboxTileXMax; x++)
                    {
                        for (var y = bboxTileYMin; y <= bboxTileYMax; y++)
                        {
                            if (x >= 0 && y >= 0 && x < Field.RoomWidth && y < Field.RoomHeight)
                            {
                                if (currRoom.Tiles[x, y].tileID >= chipline[0] && currRoom.Tiles[x, y].tileID < chipline[1])
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
                                    closestTile = Field.RoomHeight;
                                */
                            }
                        }
                    }

                    int ty = (closestTile + HUD_TILE_HEIGHT) * World.tileHeight;

                    if (bboxBottom + dy >= ty)
                    {
                        posY = ty;
                        dy = 0;
                    }
                    posY += dy;

                }
                else if (facingY == Facing.UP)
                {
                    dy = -move_speed;

                    bboxTileYMin = (int)Math.Floor(bboxTop / _tileHeight);
                    bboxTileYMax = (int)Math.Floor((bboxTop + dy) / _tileHeight) - 1;

                    bboxTileXMin = (int)Math.Floor(bboxLeft / _tileWidth);
                    bboxTileXMax = (int)Math.Floor(bboxRight / _tileWidth);

                    var closestTile = -255;
                    for (var x = bboxTileXMin; x <= bboxTileXMax; x++)
                    {
                        for (var y = bboxTileYMax; y < bboxTileYMin; y++)
                        {
                            if (x >= 0 && y >= HUD_TILE_HEIGHT && x < Field.RoomWidth && y < Field.RoomHeight + HUD_TILE_HEIGHT)
                            {
                                if (currRoom.Tiles[x, y - HUD_TILE_HEIGHT].tileID >= chipline[0] && currRoom.Tiles[x, y - HUD_TILE_HEIGHT].tileID < chipline[1])
                                {
                                    if (y - HUD_TILE_HEIGHT > closestTile)
                                        closestTile = y - HUD_TILE_HEIGHT;
                                }
                            }
                            else
                            {
                                /*
                                if (y < 0)
                                    closestTile = -1 + HUD_TILE_HEIGHT;
                                else
                                    closestTile = Field.RoomHeight - HUD_TILE_HEIGHT;
                                */
                            }
                        }
                    }

                    int ty = ((closestTile + HUD_TILE_HEIGHT) * World.tileWidth) + World.tileWidth - 1;
                    if (bboxTop + dy <= ty)
                    {
                        posY = ty + bBoxOriginY + 1;
                        dy = 0;
                    }
                    posY += dy + vsp;
                }
            }

            // Update the actual position
            Position = new Vector2(posX, posY);
            /*
            if (_world == null)
                return false;
            var currField = _world.GetField(_world.currField);
            var rx = _world.currRoomX;
            var ry = _world.currRoomY;
            var currRoom = currField.GetMapData()[rx, ry];

            var rtx = Math.Floor(xCheck / World.tileWidth);
            var rty = Math.Floor(yCheck / World.tileHeight);

            var chipline = currField.GetChipline();

            if (!(rtx >= 0 && rtx <= Field.RoomWidth - 1 &&
                rty >= 0 && rty <= Field.RoomHeight - 1))
                return true;

            var checkingTileID = currRoom.Tiles[(int)rtx, (int)rty]._tileID;

            if (checkingTileID >= chipline[0] && checkingTileID < chipline[1])
                return true;
            else
                return false;
            */
        }

        public void SetInputController(InputController inputController)
        {
            _inputController = inputController;
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
    }
}
