using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Diagnostics.Metrics;
using System.Linq;
using System.Security.Cryptography;
using System.Text.RegularExpressions;
using static OpenLaMulana.Entities.Protag;
using static OpenLaMulana.Entities.World;

namespace OpenLaMulana.Entities
{

    public class Protag : IGameEntity
    {
        public enum Facing
        {
            LEFT,
            UP,
            RIGHT,
            DOWN
        };

        private Sprite _idleSprite;

        public PlayerState State { get; set; } = PlayerState.IDLE;
        private PlayerState _prevState = PlayerState.IDLE;
        public Vector2 Position { get; set; }
        public Facing FacingX = Facing.RIGHT;
        public Facing FacingY = Facing.DOWN;
        public short BBoxOriginX { get; set; } = 5;
        public short BBoxOriginY { get; set; } = 12;
        public int Depth { get; set; } = (int)Global.DrawOrder.Abstract;
        public Effect ActiveShader { get; set; } = null;

        private short _moveX = 0;
        private short _moveY = 0;
        private short _jumpCount = 0;
        private short _jumpsMax = 2;
        private float _jumpSpeed = 5.0f;
        private float _gravInc = 0.4f;
        private float _hsp = 0;
        private float _vsp = 0;
        private float _acc = 0.3f;
        private float _drag = 0.1f;
        private float _chipWidth, _chipHeight;
        private float _moveSpeed = 1f;
        private float _gravMax = 9.81f;

        private bool _isGrounded = false;
        private bool _inLiquid = false;
        private bool _fellOffLedge = false;
        private bool _onIce = false;
        private bool _wasOnIce = false;
        private bool _hasBoots = true;
        private bool _hasFeather = true;

        public struct InventoryStruct
        {
            public Global.Weapons EquippedMainWeapon { get; set; }
            public Global.SubWeapons EquippedSubWeapon { get; set; }
            public int[] EquippedRoms;
            public int Coins;
            public int Weights;

            public int HP { get; set; }
            public int HPMax { get; set; }
            public int CurrExp { get; set; }
            public int ExpMax { get; set; }
            public int TrueHPMax { get; set; }
        }

        public InventoryStruct Inventory;


        /*
         * Weapon attack powerThe attack power of each weapon is as follows. This number also applies to breaking wall events, etc.

Whip: 2 Knife: 3 Chain Whip: 4 Ax: 5 Sword: 5 Key Sword: 1 Mace: 6
Shuriken: 2 Throwing Sword: 3 (Penetrating) Cannon: 4 Spear: 3 Bomb: 2x4 Gun: 20
rom Strengthening by cartridge combination is as follows.
Video Hustler + Break Shot: Knife and key sword attack power +2
Castlevania Dracula + Tile Magician: Whip attack power +2
        */

        public Rectangle BBox
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
            State = PlayerState.MAIN_MENU;

            Position = position;

            _chipWidth = World.CHIP_SIZE;
            _chipHeight = World.CHIP_SIZE;

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
            Inventory.EquippedRoms = new int[] { (int)Global.ObtainableSoftware.RUINS_RAM_8K, (int)Global.ObtainableSoftware.GLYPH_READER };
            Inventory.EquippedMainWeapon = Global.Weapons.FLAIL_WHIP;
            Inventory.EquippedSubWeapon = Global.SubWeapons.HANDY_SCANNER;
            Inventory.Coins = 999;
            Inventory.Weights = 999;
            Inventory.HP = 32;
            Inventory.HPMax = 32; // A life orb will increase this value by 32. True max is 352.
            Inventory.TrueHPMax = 352;

            // Damage Table:
            // Divine Lightning = 16;
            // Skeleton = 1;
            // Slime thing = 2;
            Inventory.CurrExp = 0;
            Inventory.ExpMax = 88; // When this is 88, trigger and reset to 0

            MoveToWorldSpawnPoint();
        }

        private void MoveToWorldSpawnPoint()
        {
            int[] spawnParams = Regex.Matches(Global.TextManager.GetText((int)Global.HardCodedText.SPAWN_POINT, Global.CurrLang), "(-?[0-9]+)").OfType<Match>().Select(m => int.Parse(m.Value)).ToArray();
            int destFieldID = spawnParams[0];
            View destView = Global.World.GetField(destFieldID).GetView(spawnParams[1]);
            int spawnX = spawnParams[2];
            int spawnY = spawnParams[3];

            Global.World.FieldTransitionImmediate(Global.World.GetCurrentView(), destView);

            Position = new Vector2(spawnX * CHIP_SIZE, spawnY * CHIP_SIZE);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _idleSprite.Draw(spriteBatch, Position + new Vector2(0, World.HUD_HEIGHT));

            Rectangle dRect = BBox;
            dRect.Y += World.HUD_HEIGHT;
            RectangleSprite.DrawRectangle(spriteBatch, dRect, Color.Red, 1);
        }

        public void Update(GameTime gameTime)
        {
            if (Global.Camera.GetState() != Camera.CamStates.NONE)
                return;
            
            if (!Global.AnimationTimer.OneFrameElapsed())
                return;

            switch(Global.ProtagPhysics)
            {
                case Global.PlatformingPhysics.CLASSIC:
                    ClassicStateMachine();
                    break;
                case Global.PlatformingPhysics.REVAMPED:
                    RevampedStateMachine();
                    break;
            }
        }

        #region RevampedMovementCode
        private void RevampedStateMachine()
        {
            View currRoom = Global.World.GetActiveViews()[(int)AViews.DEST].GetView();// (Global.World.CurrField).GetMapData()[Global.World.CurrViewX, Global.World.CurrViewY]; // TODO: Update this variable only whenever a map change occurs

            switch (State)
            {
                case PlayerState.MAIN_MENU:
                case PlayerState.PAUSED:
                case PlayerState.CUTSCENE:
                case PlayerState.SCREEN_TRANSITION:
                    _hsp = 0;
                    _vsp = 0;
                    break;
                case PlayerState.IDLE:
                case PlayerState.WALKING:
                case PlayerState.JUMPING:
                case PlayerState.FALLING:
                case PlayerState.WHIPPING:
                    RevampedCollideAndMove();
                    break;
                case PlayerState.CLIMBING:
                    _moveY = InputManager.DirHeldY;
                    _vsp = _moveSpeed * _moveY;

                    ChipTypes collidingTile = TilePlaceMeeting(currRoom, BBox, Position, Position.X, Position.Y + _vsp);

                    if (TileGetCellAtPixel(currRoom, BBox.Left, BBox.Top) == ChipTypes.BACKGROUND)
                    {
                        Position = new Vector2(Position.X, ((float)Math.Floor(Position.Y / World.CHIP_SIZE) * World.CHIP_SIZE) - BBox.Height + 3);
                        //Position += new Vector2(0, Math.Sign(_vsp) * World.CHIP_SIZE);
                        _hsp = 0;
                        _vsp = 0;
                        State = PlayerState.IDLE;
                    } else if (
                        TileGetCellAtPixel(currRoom, BBox.Left, BBox.Bottom) == ChipTypes.BACKGROUND ||
                        TileGetCellAtPixel(currRoom, Position.X, BBox.Bottom) == ChipTypes.SOLID ||
                        TileGetCellAtPixel(currRoom, Position.X, BBox.Bottom) == ChipTypes.ICE ||
                        TileGetCellAtPixel(currRoom, Position.X, BBox.Bottom) == ChipTypes.UNCLINGABLE_WALL ||
                        TileGetCellAtPixel(currRoom, Position.X, BBox.Bottom) == ChipTypes.CLINGABLE_WALL
                        )
                    {
                        Position = new Vector2(Position.X, (float)Math.Floor(Position.Y / World.CHIP_SIZE) * World.CHIP_SIZE);
                        //Position += new Vector2(0, Math.Sign(_vsp) * World.CHIP_SIZE);
                        _hsp = 0;
                        _vsp = 0;

                        if (Math.Floor((Position.Y + _vsp) / World.CHIP_SIZE) < ROOM_HEIGHT)
                            State = PlayerState.IDLE;
                        else
                        {
                            Global.World.FieldTransitionCardinal(VIEW_DIR.DOWN);
                            _hsp = 0;
                            _vsp = 0;
                            State = PlayerState.SCREEN_TRANSITION;
                        }

                    }

                    if (TilePlaceMeeting(currRoom, BBox, Position, Position.X, BBox.Top + _vsp, ChipTypes.SOLID) != ChipTypes.SOLID && TilePlaceMeeting(currRoom, BBox, Position, Position.X, Position.Y + _vsp, ChipTypes.SOLID) != ChipTypes.SOLID)
                    {
                        if (InputManager.PressedKeys[(int)Global.ControllerKeys.JUMP])
                        {
                            State = PlayerState.JUMPING;
                            Global.AudioManager.PlaySFX(SFX.P_JUMP);
                            _jumpCount--;
                            _vsp = -_jumpSpeed;
                            FacingY = Facing.UP;
                            _hsp = _moveX * _moveSpeed;
                        }

                        if ((_vsp < 0) && !InputManager.HeldKeys[(int)Global.ControllerKeys.JUMP])
                        {
                            Math.Max(_vsp, -_jumpSpeed / 2);
                        }
                    }

                    Position += new Vector2(0, _vsp);
                    break;
            }

            if (Global.Camera.GetState() == Camera.CamStates.NONE)
            {
                // Handle screen transitions when the player touches the border of the screen
                if (BBox.Right + _hsp > ROOM_WIDTH * World.CHIP_SIZE)
                {
                    Global.World.FieldTransitionCardinal(VIEW_DIR.RIGHT);
                    _hsp = 0;
                    _vsp = 0;
                    State = PlayerState.SCREEN_TRANSITION;
                }
                else if (BBox.Left + _hsp < 0)
                {
                    Global.World.FieldTransitionCardinal(VIEW_DIR.LEFT);
                    _hsp = 0;
                    _vsp = 0;
                    State = PlayerState.SCREEN_TRANSITION;
                }
                else if (BBox.Bottom + _vsp +1> (ROOM_HEIGHT * World.CHIP_SIZE) + HUD_HEIGHT)
                {
                    Global.World.FieldTransitionCardinal(VIEW_DIR.DOWN);
                    _hsp = 0;
                    _vsp = 0;
                    State = PlayerState.SCREEN_TRANSITION;
                }
                else if (BBox.Top <= 0 &&
                    (_isGrounded || State == PlayerState.CLIMBING || State == PlayerState.JUMPING) &&
                    State != PlayerState.FALLING)
                {
                    if (State == PlayerState.JUMPING)
                    {
                        var currTile = TileGetCellAtPixel(currRoom, BBox.Left + 1, 0);
                        if (currTile != ChipTypes.LAVA &&
                        (currTile != ChipTypes.WATER) &&
                        (currTile != ChipTypes.WATER_STREAM_DOWN) &&
                        (currTile != ChipTypes.WATER_STREAM_LEFT) &&
                        (currTile != ChipTypes.WATER_STREAM_RIGHT) &&
                        (currTile != ChipTypes.WATER_STREAM_UP))
                            return;
                    }
                    Global.World.FieldTransitionCardinal(VIEW_DIR.UP);
                    _hsp = 0;
                    _vsp = 0;
                    State = PlayerState.SCREEN_TRANSITION;
                }
            }

            if (State != PlayerState.SCREEN_TRANSITION)
                _prevState = State;
        }

        private void RevampedCollideAndMove()
        {
            _moveSpeed = 1.5f;
            _moveX = InputManager.DirHeldX;
            //_moveY = InputManager.DirHeldY;

            View currRoom = Global.World.GetActiveViews()[(int)AViews.CURR].GetView();// (Global.World.CurrField).GetMapData()[Global.World.CurrViewX, Global.World.CurrViewY]; // TODO: Update this variable only whenever a map change occurs

            //_vsp = _moveY * _moveSpeed;


            // Gravity handling

            if (_vsp < _gravMax)
                _vsp += _gravInc;
            else
                _vsp = _gravMax;

            if (_vsp != 0) {
                if (Math.Sign(_vsp) > 0)
                    FacingY = Facing.DOWN;
                else
                    FacingY = Facing.UP;
            }

            if (_isGrounded)
            {
                _jumpCount = _jumpsMax;
                _fellOffLedge = false;

                if (_onIce)
                {
                    if (_moveX != 0)
                    {
                        if (_moveX > 0)
                            FacingX = Facing.RIGHT;
                        else
                            FacingX = Facing.LEFT;

                        _hsp += _acc * _moveX;
                    }
                    _hsp = HelperFunctions.Lerp(_hsp, 0, _drag);
                }
                else
                {
                    _hsp = _moveX * _moveSpeed;
                }
            }
            else
            {
                if (!_inLiquid)
                {
                    /*
                    if (_jumpCount == _jumpsMax)
                        _jumpCount--;
                    */

                    if (_wasOnIce)
                    {
                        //_wasOnIce = false;
                    }
                    else
                    {
                        if (_vsp >= 0)
                        {
                            if (_jumpCount < _jumpsMax && !_fellOffLedge)
                                _hsp = _moveX * _moveSpeed; // Allow horizontal movement if we're falling
                            else {
                                // We walked off an edge: Instead, we should fall straight down until we touch ground
                                _hsp = 0;
                                _jumpCount = 0;
                                _fellOffLedge = true;
                            }
                        }
                    }
                } else
                {
                    if (_vsp < 0)
                        _hsp = _moveX * _moveSpeed;
                }
            }

            if (InputManager.PressedKeys[(int)Global.ControllerKeys.JUMP] && _jumpCount > 0)
            {
                State = PlayerState.JUMPING;
                Global.AudioManager.PlaySFX(SFX.P_JUMP);
                _jumpCount--;
                _vsp = -_jumpSpeed;
                FacingY = Facing.UP;

                if (_onIce) {
                    _onIce = false;
                    _wasOnIce = true;
                }
            }

            if ((_vsp < 0) && !InputManager.HeldKeys[(int)Global.ControllerKeys.JUMP])
            {
                Math.Max(_vsp, -_jumpSpeed / 2);
            }




            // Horizontal movement
            ChipTypes collidingTile = TilePlaceMeeting(currRoom, BBox, Position, Position.X + _hsp, Position.Y);


            switch (collidingTile)
            {
                default:
                    break;
                case ChipTypes.LADDER:
                    if (BBox.Left % World.CHIP_SIZE > 4)
                    {
                        if (TilePlaceMeeting(currRoom, BBox, Position, BBox.Left, Position.Y, ChipTypes.LADDER) == ChipTypes.LADDER)
                        {

                            if (InputManager.PressedKeys[(int)Global.ControllerKeys.UP])
                            {
                                _jumpCount = _jumpsMax;
                                State = PlayerState.CLIMBING;
                                Position = new Vector2((float)Math.Floor(Position.X / World.CHIP_SIZE) * World.CHIP_SIZE, Position.Y);
                                if (TileGetCellAtPixel(currRoom, BBox.Left + 1, BBox.Top) != ChipTypes.LADDER)
                                    Position += new Vector2(World.CHIP_SIZE, 0);
                                _vsp = 0;
                                _hsp = 0;
                            }
                        }
                    }
                    break;
                case ChipTypes.WATER:
                case ChipTypes.WATER_STREAM_UP:
                case ChipTypes.WATER_STREAM_RIGHT:
                case ChipTypes.WATER_STREAM_DOWN:
                case ChipTypes.WATER_STREAM_LEFT:
                case ChipTypes.LAVA:
                case ChipTypes.WATERFALL:
                    break;
                case ChipTypes.ASCENDING_SLOPE:
                case ChipTypes.ASCENDING_SLOPE_LEFT:
                case ChipTypes.ASCENDING_STAIRS_RIGHT:
                case ChipTypes.ASCENDING_STAIRS_LEFT:
                case ChipTypes.ASCENDING_RIGHT_BEHIND_STAIRS:
                case ChipTypes.ASCENDING_BACK_LEFT:
                case ChipTypes.ICE_SLOPE_RIGHT:
                case ChipTypes.ICE_SLOPE_LEFT:
                case ChipTypes.CLINGABLE_WALL:
                case ChipTypes.ICE:
                case ChipTypes.UNCLINGABLE_WALL:
                case ChipTypes.SOLID:
                    while (TilePlaceMeeting(currRoom, BBox, Position, Position.X + Math.Sign(_hsp), Position.Y) != collidingTile)
                    {
                        Position += new Vector2(Math.Sign(_hsp), 0);
                    }
                    _hsp = 0;
                    break;
            }

            Position += new Vector2(_hsp, 0);

            // Vertical movement
            collidingTile = TilePlaceMeeting(currRoom, BBox, Position, Position.X, Position.Y + _vsp);
            bool noMoveY = false;

            switch (collidingTile)
            {
                default:
                    break;
                case ChipTypes.WATER:
                case ChipTypes.WATER_STREAM_UP:
                case ChipTypes.WATER_STREAM_RIGHT:
                case ChipTypes.WATER_STREAM_DOWN:
                case ChipTypes.WATER_STREAM_LEFT:
                case ChipTypes.LAVA:
                    _onIce = false;
                    _isGrounded = false;
                    _jumpCount = 1;

                    // Reset vertical speed if we just entered the water/lava
                    if (!_inLiquid)
                        _vsp = 0;

                    if (_vsp > 0)
                        _vsp *= 0.7f;

                    if (!_inLiquid)
                    {
                        _inLiquid = true;
                        Global.AudioManager.PlaySFX(SFX.WATER_SUBMERGE);
                    }

                    // Account for solid floors
                    /// TODO: Refactor
                    RevampedAccountForSolidFloors(currRoom); // This is EXTREMELY laggy... 
                    
                    break;
                case ChipTypes.BACKGROUND:
                    _inLiquid = false;
                    _onIce = false;
                    _isGrounded = false;
                    break;
                case ChipTypes.ICE_SLOPE_RIGHT:
                case ChipTypes.ICE_SLOPE_LEFT:
                case ChipTypes.ICE:
                    _onIce = true;
                    while (TilePlaceMeeting(currRoom, BBox, Position, Position.X, Position.Y + Math.Sign(_vsp)) != collidingTile)
                    {
                        if (Position.Y <= CHIP_SIZE)
                            break;
                        Position += new Vector2(0, Math.Sign(_vsp));
                    }
                    _vsp = 0;

                    if (!_isGrounded)
                    {
                        if (FacingY == Facing.DOWN) {
                            _isGrounded = true;
                            Global.AudioManager.PlaySFX(SFX.P_GROUNDED);
                        }
                    }


                    break;
                case ChipTypes.LADDER:
                    _onIce = false;
                    _inLiquid = false;

                    // Account for solid floors
                    RevampedAccountForSolidFloors(currRoom);
                    break;
                case ChipTypes.ASCENDING_SLOPE:
                case ChipTypes.ASCENDING_SLOPE_LEFT:
                case ChipTypes.ASCENDING_STAIRS_RIGHT:
                case ChipTypes.ASCENDING_STAIRS_LEFT:
                case ChipTypes.ASCENDING_RIGHT_BEHIND_STAIRS:
                case ChipTypes.ASCENDING_BACK_LEFT:
                case ChipTypes.WATERFALL:
                case ChipTypes.CLINGABLE_WALL:
                case ChipTypes.UNCLINGABLE_WALL:
                case ChipTypes.SOLID:
                    _onIce = false;
                    while (TilePlaceMeeting(currRoom, BBox, Position, Position.X, Position.Y + Math.Sign(_vsp)) != collidingTile)
                    {
                        Position += new Vector2(0, Math.Sign(_vsp));
                    }
                    _vsp = 0;

                    if (!_isGrounded)
                    {
                        if (FacingY == Facing.DOWN)
                        {
                            _isGrounded = true;
                            Global.AudioManager.PlaySFX(SFX.P_GROUNDED);
                        }
                    }

                    if (BBox.Left % World.CHIP_SIZE <= 4)
                    {
                        if (TilePlaceMeeting(currRoom, BBox, Position, BBox.Left, BBox.Bottom + World.CHIP_SIZE * 2, ChipTypes.LADDER) == ChipTypes.LADDER)
                        {
                            if (InputManager.PressedKeys[(int)Global.ControllerKeys.DOWN] && _jumpCount == _jumpsMax)
                            {
                                _jumpCount = _jumpsMax;
                                State = PlayerState.CLIMBING;
                                Position = new Vector2((float)Math.Floor((double)BBox.Left / World.CHIP_SIZE) * World.CHIP_SIZE, Position.Y + World.CHIP_SIZE * 2);

                                if (BBox.Left % CHIP_SIZE <= 4)
                                {
                                    if (TileGetCellAtPixel(currRoom, BBox.Left, Position.Y + 1 + CHIP_SIZE) != ChipTypes.LADDER)
                                        Position += new Vector2(World.CHIP_SIZE, 0);
                                }
                                _vsp = 0;
                                _hsp = 0;
                            }
                        }
                    }
                    break;
            }

            // Account for jumping beyond the HUD
            if (Position.Y + _vsp <= CHIP_SIZE)
            {

                if (_vsp < 0)
                {
                    Position = new Vector2(Position.X, CHIP_SIZE);
                    noMoveY = true;
                }
            }

            if (!noMoveY)
                Position += new Vector2(0, _vsp);
        }

        private void RevampedAccountForSolidFloors(View currRoom)
        {
            if (FacingY == Facing.DOWN)
            {
                if (TilePlaceMeeting(currRoom, BBox, Position, Position.X, Position.Y + _vsp, ChipTypes.SOLID) == ChipTypes.SOLID)
                {
                    //Position += new Vector2(0, -1);
                    while (TilePlaceMeeting(currRoom, BBox, Position, Position.X, Position.Y + Math.Sign(_vsp), ChipTypes.SOLID) != ChipTypes.SOLID)
                    {
                        Position += new Vector2(0, Math.Sign(_vsp));
                    }
                    _vsp = 0;

                    if (!_isGrounded)
                    {
                        _isGrounded = true;
                        Global.AudioManager.PlaySFX(SFX.P_GROUNDED);
                    }
                }
                if (TilePlaceMeeting(currRoom, BBox, Position, Position.X, Position.Y + _vsp, ChipTypes.UNCLINGABLE_WALL) == ChipTypes.UNCLINGABLE_WALL)
                {
                    //Position += new Vector2(0, -1);
                    while (TilePlaceMeeting(currRoom, BBox, Position, Position.X, Position.Y + Math.Sign(_vsp), ChipTypes.UNCLINGABLE_WALL) != ChipTypes.UNCLINGABLE_WALL)
                    {
                        Position += new Vector2(0, Math.Sign(_vsp));
                    }
                    _vsp = 0;
                    if (!_isGrounded)
                    {
                        _isGrounded = true;
                        Global.AudioManager.PlaySFX(SFX.P_GROUNDED);
                    }
                }
                if (TilePlaceMeeting(currRoom, BBox, Position, Position.X, Position.Y + _vsp, ChipTypes.CLINGABLE_WALL) == ChipTypes.CLINGABLE_WALL)
                {
                    //Position += new Vector2(0, -1);
                    while (TilePlaceMeeting(currRoom, BBox, Position, Position.X, Position.Y + Math.Sign(_vsp), ChipTypes.CLINGABLE_WALL) != ChipTypes.CLINGABLE_WALL)
                    {
                        Position += new Vector2(0, Math.Sign(_vsp));
                    }
                    _vsp = 0;
                    if (!_isGrounded)
                    {
                        _isGrounded = true;
                        Global.AudioManager.PlaySFX(SFX.P_GROUNDED);
                    }
                }
            }
        }
        #endregion

        #region ClassicMovementCode
        private void ClassicStateMachine()
        {
            switch (State)
            {
                case PlayerState.MAIN_MENU:
                case PlayerState.PAUSED:
                case PlayerState.CUTSCENE:
                    break;
                case PlayerState.IDLE:
                case PlayerState.WALKING:
                case PlayerState.JUMPING:
                case PlayerState.FALLING:
                case PlayerState.WHIPPING:
                    var chipline = Global.World.GetField(Global.World.CurrField).GetChipline();

                    ClassicCollideAndMove(_moveSpeed, _moveSpeed, chipline);
                    break;
            }

            _prevState = State;
        }

        private void ClassicCollideAndMove(float dx, float dy, int[] chipline)
        {
            var posX = Position.X;
            var posY = Position.Y;

            int bboxTileXMin, bboxTileXMax, bboxTileYMin, bboxTileYMax;
            var bboxLeft = BBox.Left;
            var bboxRight = BBox.Right;
            var bboxTop = BBox.Top;
            var bboxBottom = BBox.Bottom;
            /*
            Decompose movement into X and Y axes, step one at a time. If you’re planning on implementing slopes afterwards, step X first, then Y.
            Otherwise, the order shouldn’t matter much. Then, for each axis:
            */

                    // Step X

                    // Get the coordinate of the forward-facing edge, e.g. : If walking left, the x coordinate of left of bounding box.
                    //  If walking right, x coordinate of right side.If up, y coordinate of top, etc.

                    _moveX = InputManager.DirHeldX;
            if (_moveX == 1)
                FacingX = Facing.RIGHT;
            else if (_moveX == -1)
                FacingX = Facing.LEFT;

            View currRoom = Global.World.GetActiveViews()[(int)AViews.CURR].GetView();// (Global.World.CurrField).GetMapData()[Global.World.CurrViewX, Global.World.CurrViewY]; // TODO: Update this variable only whenever a map change occurs

            /*
            ChipTypes collidingTile = TilePlaceMeeting(currRoom, BBox, Position, Position.X + _hsp, Position.Y);

            switch (collidingTile)
            {
                default:
                    break;
                case ChipTypes.LEFT_SIDE_OF_STAIRS:
                case ChipTypes.ASCENDING_SLOPE:
                case ChipTypes.ASCENDING_SLOPE_LEFT:
                case ChipTypes.ASCENDING_STAIRS_RIGHT:
                case ChipTypes.ASCENDING_STAIRS_LEFT:
                case ChipTypes.ASCENDING_RIGHT_BEHIND_STAIRS:
                case ChipTypes.ASCENDING_BACK_LEFT:
                case ChipTypes.ICE_SLOPE_RIGHT:
                case ChipTypes.ICE_SLOPE_LEFT:
                case ChipTypes.WATER:
                case ChipTypes.WATER_STREAM_UP:
                case ChipTypes.WATER_STREAM_RIGHT:
                case ChipTypes.WATER_STREAM_DOWN:
                case ChipTypes.WATER_STREAM_LEFT:
                case ChipTypes.LAVA:
                case ChipTypes.WATERFALL:
                case ChipTypes.CLINGABLE_WALL:
                case ChipTypes.ICE:
                case ChipTypes.UNCLINGABLE_WALL:
                case ChipTypes.SOLID:
                    while (TilePlaceMeeting(currRoom, BBox, Position, Position.X + Math.Sign(_hsp), Position.Y) != collidingTile)
                    {
                        Position += new Vector2(Math.Sign(_hsp), 0);
                    }
                    _hsp = 0;
                    break;
            }*/


            if (_moveX != 0)
            {
                if (FacingX == Facing.RIGHT)
                {

                    // Figure which lines of tiles the bounding box intersects with – this will give you a minimum and maximum tile value on the OPPOSITE axis.
                    // For example, if we’re walking left, perhaps the player intersects with horizontal rows 32, 33 and 34(that is, tiles with y = 32 * TS, y = 33 * TS, and y = 34 * TS, where TS = tile size).

                    dx = _moveSpeed;

                    bboxTileYMin = (int)Math.Floor(bboxTop / _chipHeight);
                    bboxTileYMax = (int)Math.Floor(bboxBottom / _chipHeight);

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

                    bboxTileYMin = (int)Math.Floor(bboxTop / _chipHeight);
                    bboxTileYMax = (int)Math.Floor(bboxBottom / _chipHeight);

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
            if (InputManager.PressedKeys[(int)Global.ControllerKeys.JUMP])
                Global.AudioManager.PlaySFX(SFX.P_JUMP);

            if (InputManager.HeldKeys[(int)Global.ControllerKeys.JUMP] && dy <= 0 && _isGrounded)
            {
                State = PlayerState.JUMPING;
            }

            _moveY = InputManager.DirHeldY;
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
                    bboxTileYMin = (int)Math.Floor(bboxBottom / _chipHeight);
                    bboxTileYMax = (int)Math.Floor((bboxBottom + dy) / _chipHeight);

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

                    int ty = closestTile * World.CHIP_SIZE;

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
                            if (x >= 0 && y >= 0 && x < World.ROOM_WIDTH && y < World.ROOM_HEIGHT)
                            {
                                if (currRoom.Chips[x, y].TileID >= chipline[0] && currRoom.Chips[x, y].TileID < chipline[1])
                                {
                                    if (y > closestTile)
                                        closestTile = y;
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

                    int ty = (closestTile * World.CHIP_SIZE) + World.CHIP_SIZE - 1;
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
        #endregion

        public string PrintState()
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
                case PlayerState.CLIMBING:
                    return "Climbing";
                case PlayerState.SCREEN_TRANSITION:
                    return "Screen Transition";
                case PlayerState.PAUSED:
                    return "Paused";
                case PlayerState.WALKING:
                    return "Walking";
                case PlayerState.WHIPPING:
                    return "Whipping";
                case PlayerState.NPC_DIALOGUE:
                    return "NPC Dialogue";
                case PlayerState.MAX:
                    return "Max";
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
            return _isGrounded;
        }

        internal PlayerState GetPrevState()
        {
            return _prevState;
        }
    }
}
