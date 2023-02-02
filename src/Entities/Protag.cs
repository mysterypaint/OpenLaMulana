using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
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

        private Sprite _sprIndex;
        private Sprite _idleSprite;

        public PlayerState State { get; set; } = PlayerState.IDLE;
        private PlayerState _prevState = PlayerState.IDLE;
        public Facing FacingX = Facing.RIGHT;
        public Facing FacingY = Facing.DOWN;
        public short BBoxOriginX { get; set; } = 5;
        public short BBoxOriginY { get; set; } = 11;
        public short BBoxCenterX { get; set; } = 5;
        public short BBoxCenterY { get; set; } = 6;

        public int Depth { get; set; } = (int)Global.DrawOrder.Protag;
        public Effect ActiveShader { get; set; } = null;
        public bool LockTo30FPS { get; set; } = true;

        private short _moveX = 0;
        private short _moveY = 0;
        private short _jumpCount = 0;
        private short _jumpsMax = 1;
        private double _jumpSpeed = 5.0f;
        private double _gravInc = 0.5f;
        private double _hsp = 0;
        private double _vsp = 0;
        private double _hspCarry = 0;
        private double _vspCarry = 0;
        private double _hsp_final = 0;
        private double _hsp_fract = 0;
        private double _vsp_final = 0;
        private double _vsp_fract = 0;
        private double _acc = 0.3f;
        private double _drag = 0.1f;
        private double _chipWidth, _chipHeight;
        private double _moveSpeed = 1.5f;
        private double _gravMax = 9.81f;

        private bool _isGrounded = false;
        private bool _inLiquid = false;
        private bool _fellOffLedge = false;
        private bool _onIce = false;
        private bool _wasOnIce = false;
        private bool _hasBoots = false;
        private bool _hasFeather = false;



        /*
         * Weapon attack powerThe attack power of each weapon is as follows. This number also applies to breaking wall events, etc.

Whip: 2 Knife: 3 Chain Whip: 4 Ax: 5 Sword: 5 Key Sword: 1 Mace: 6
Shuriken: 2 Throwing Sword: 3 (Penetrating) Cannon: 4 Spear: 3 Bomb: 2x4 Gun: 20
rom Strengthening by cartridge combination is as follows.
Video Hustler + Break Shot: Knife and key sword attack power +2
Castlevania Dracula + Tile Magician: Whip attack power +2
        */

        public Rectangle BBox;

        public int ContactWarpCooldownTimer { get; set; } = 0;

        public const int SPRITE_WIDTH = 16;
        public const int SPRITE_HEIGHT = 16;
        public const int CHIP_EDGE = CHIP_SIZE - 1;
        private const double SPD_GRAVITY = 0.4;
        private double _spdWalk = 1;
        private bool _enemyGrounded;
        private ParentInteractableWorldEntity _myGroundedEntity;
        private const double SPD_JUMP = 7.0;
        private const double SPD_GRAV_MAX = 8f;

        public Protag(Vector2 position)
        {
            InitWeaponDamageTables();

            State = PlayerState.MAIN_MENU;

            BBox = new Rectangle(
                    (int)Math.Round(position.X),
                    (int)Math.Round(position.Y),
                    9,
                    11
                );

            BBox.Offset(new Point(-BBoxOriginX, -BBoxOriginY));

            //box.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);

            _chipWidth = World.CHIP_SIZE;
            _chipHeight = World.CHIP_SIZE;

            Texture2D spriteSheet = Global.TextureManager.GetTexture(Global.Textures.PROT1);
            _idleSprite = new Sprite(spriteSheet, 0, 0, 16, 16, 8, 16);
            _sprIndex = _idleSprite;

            Global.Inventory.HP = 32;
            Global.Inventory.HPMax = 32;
        }

        private void InitWeaponDamageTables()
        {
            Global.WeaponsDamageTable[Global.MainWeapons.WHIP] = 2;
            Global.WeaponsDamageTable[Global.MainWeapons.KNIFE] = 3;
            Global.WeaponsDamageTable[Global.MainWeapons.CHAIN_WHIP] = 4;
            Global.WeaponsDamageTable[Global.MainWeapons.AXE] = 5;
            Global.WeaponsDamageTable[Global.MainWeapons.KATANA] = 5;
            Global.WeaponsDamageTable[Global.MainWeapons.KEYBLADE] = 1;
            Global.WeaponsDamageTable[Global.MainWeapons.KEYBLADE_BETA] = 1;
            Global.WeaponsDamageTable[Global.MainWeapons.FLAIL_WHIP] = 6;
            Global.SubWeaponsDamageTable[Global.SubWeapons.SHURIKEN] = 2;
            Global.SubWeaponsDamageTable[Global.SubWeapons.THROWING_KNIFE] = 3;
            Global.SubWeaponsDamageTable[Global.SubWeapons.FLARES] = 4;
            Global.SubWeaponsDamageTable[Global.SubWeapons.SPEAR] = 3;
            Global.SubWeaponsDamageTable[Global.SubWeapons.BOMB] = 2 * 4;
            Global.SubWeaponsDamageTable[Global.SubWeapons.PISTOL] = 20;

            Global.RomDamageMultipliers[Global.SoftwareCombos.VID_HUST_BREAKSHOT] = 2;      // Knife and Keyblade Attack Power +2
            Global.RomDamageMultipliers[Global.SoftwareCombos.CASTLV_MAHJONGWIZ] = 2;       // Whip Attack Power +2
        }

        public void ResetState()
        {
            State = PlayerState.IDLE;
            _jumpCount = _jumpsMax;
            _hsp = 0;
            _vsp = 0;
            FacingX = Protag.Facing.LEFT;
            _isGrounded = false;
        }

        public void Initialize()
        {
            State = PlayerState.IDLE;
            _chipWidth = World.CHIP_SIZE;
            _chipHeight = World.CHIP_SIZE;

            Global.Inventory.ObtainedTreasures = new Dictionary<Global.ObtainableTreasures, bool>();//bool[(int)Global.ObtainableTreasures.MAX];
            for (Global.ObtainableTreasures treasures = (Global.ObtainableTreasures)0; treasures < Global.ObtainableTreasures.MAX; treasures++)
            {
                Global.Inventory.ObtainedTreasures.Add(treasures, false);
            }
            Global.Inventory.TreasureIcons = new Global.ObtainableTreasures[40];
            Array.Fill((Global.ObtainableTreasures[])Global.Inventory.TreasureIcons, (Global.ObtainableTreasures)255);

            Global.Inventory.ObtainedSoftware = new Dictionary<Global.ObtainableSoftware, bool>();//bool[(int)Global.ObtainableSoftware.MAX];
            for (Global.ObtainableSoftware software = (Global.ObtainableSoftware)0; software < Global.ObtainableSoftware.MAX; software++)
            {
                Global.Inventory.ObtainedSoftware.Add(software, false);
            }
            Global.Inventory.ObtainedSubWeapons = new Global.SubWeapons[10];
            Array.Fill((Global.SubWeapons[])Global.Inventory.ObtainedSubWeapons, (Global.SubWeapons)Global.SubWeapons.NONE);
            Global.Inventory.ObtainedMainWeapons = new Global.MainWeapons[5];
            Array.Fill((Global.MainWeapons[])Global.Inventory.ObtainedMainWeapons, (Global.MainWeapons)Global.MainWeapons.NONE);

            Global.Inventory.EquippedRoms = new Global.ObtainableSoftware[] { Global.ObtainableSoftware.NONE, Global.ObtainableSoftware.NONE };
            Global.Inventory.ObtainedMainWeapons[0] = Global.MainWeapons.WHIP;
            Global.Inventory.ObtainedTreasures[Global.ObtainableTreasures.MSX] = true;
            Global.Inventory.TreasureIcons[(int)Global.ObtainableTreasures.MSX] = Global.ObtainableTreasures.MSX;

            Global.Inventory.EquippedMainWeapon = Global.MainWeapons.WHIP;
            Global.Inventory.EquippedSubWeapon = Global.SubWeapons.HANDY_SCANNER;
            Global.Inventory.CoinCount = 0;
            Global.Inventory.WeightCount = 500;
            Global.Inventory.HP = 32;
            Global.Inventory.HPMax = 32; // A life orb will increase this value by 32. True max is 352.
            Global.Inventory.TrueHPMax = 352;
            Global.Inventory.CurrExp = 0;

            Global.Inventory.BulletCount = 0;
            Global.Inventory.AmmunitionRefills = 0;
            Global.Inventory.AnkhJewelCount = 0;
            Global.Inventory.ShieldValue = 0;
            Global.Inventory.HandyScannerValue = 0;

            if (Global.DevModeEnabled)
            {
                GetDebugEquipment();
                SaveData encryptedSave = null;// HelperFunctions.LoadSaveFromFile("lamulana.sa0");

                if (encryptedSave != null)
                {
                    SaveData decryptedSave = HelperFunctions.DecryptSaveFile(encryptedSave);
                    HelperFunctions.ParseSaveData(decryptedSave);
                }
            }

            // Damage Table:
            // Divine Lightning = 16;
            // Skeleton = 1;
            // Slime thing = 2;

            if (Global.QoLChanges)
            {
                Global.Inventory.ExpMax = Global.Inventory.HPMax;
            }
            else
                Global.Inventory.ExpMax = Global.InventoryStruct.ExpMaxClassic; // When this is 88, trigger and reset to 0

            if (Global.Inventory.ObtainedTreasures[Global.ObtainableTreasures.FEATHER])
                _jumpsMax = 2;

            Global.MobileSuperX.VerifyThatPlayerHasAtLeastOneSubweapon();

            if (Global.DevModeEnabled)
            {
                View destView = Global.World.GetField(6).GetView(2, 0);
                Global.World.FieldTransitionImmediate(Global.World.GetCurrentView(), destView);

                SetBBoxToTile(6, 6);
                //MoveToWorldSpawnPoint();
            }
            else
                MoveToWorldSpawnPoint();
        }

        private void GetDebugEquipment()
        {
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.GRADIUS] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.GLYPH_READER] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.HYPER_RALLY] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.KONAMI_TENNIS] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.GAME_MASTER] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.A1_SPIRIT] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.SKY_JAGUAR] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.F1_SPIRIT] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.YIE_AR_KUNG_FU_2] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.COMIC_BAKERY] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.ANTARCTIC_ADVENTURE] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.SHIN_SYNTHESIZER] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.SNATCHER] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.SD_SNATCHER] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.A1_SPIRIT] = true;
            Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.BADLANDS] = true;
            HelperFunctions.UpdateInventorySilent(Global.ObtainableTreasures.SHELL_HORN, true);
            HelperFunctions.UpdateInventorySilent(Global.ObtainableTreasures.BOOTS, true);
            HelperFunctions.UpdateInventorySilent(Global.ObtainableTreasures.FEATHER, true);
            HelperFunctions.UpdateInventorySilent(Global.ObtainableTreasures.MSX2, true);
            HelperFunctions.UpdateInventorySilent(Global.ObtainableTreasures.GRAIL, true);
            HelperFunctions.UpdateInventorySilent(Global.ObtainableTreasures.MAGATAMA_JEWEL, true);
            HelperFunctions.UpdateInventorySilent(Global.SubWeapons.SHURIKEN, true);
            HelperFunctions.UpdateInventorySilent(Global.SubWeapons.HANDY_SCANNER, true);

            Global.Inventory.ObtainedMainWeapons[4] = Global.MainWeapons.KATANA;
            Global.Inventory.EquippedRoms = new Global.ObtainableSoftware[] { Global.ObtainableSoftware.GLYPH_READER, Global.ObtainableSoftware.ANTARCTIC_ADVENTURE };
        }

        private void MoveToWorldSpawnPoint()
        {
            int[] spawnParams = Regex.Matches(Global.TextManager.GetText((int)Global.HardCodedText.SPAWN_POINT, Global.CurrLang), "(-?[0-9]+)").OfType<Match>().Select(m => int.Parse(m.Value)).ToArray();
            int destFieldID = spawnParams[0];
            View destView = Global.World.GetField(destFieldID).GetView(spawnParams[1]);
            int spawnX = spawnParams[2];
            int spawnY = spawnParams[3];

            Global.World.FieldTransitionImmediate(Global.World.GetCurrentView(), destView);

            SetBBoxToTile(spawnX, spawnY);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _sprIndex.Draw(spriteBatch, new Vector2(BBox.Center.X + 1, BBox.Y + BBoxOriginY + 1 + World.HUD_HEIGHT));
            Rectangle dRect = BBox;
            dRect.Y += World.HUD_HEIGHT;
            RectangleSprite.DrawRectangle(spriteBatch, dRect, new Color(255, 0, 0, 10), 1);

            /*
            int offY = World.HUD_HEIGHT;
            RectangleSprite.DrawRectangle(spriteBatch, new Rectangle(BBox.Right, BBox.Top + offY, 0, 0), Color.White, 1);
            RectangleSprite.DrawRectangle(spriteBatch, new Rectangle(BBox.Right, BBox.Bottom + offY, 0, 0), Color.White, 1);
            RectangleSprite.DrawRectangle(spriteBatch, new Rectangle(BBox.Right, BBox.Center.Y + offY, 0, 0), Color.White, 1);
            RectangleSprite.DrawRectangle(spriteBatch, new Rectangle(BBox.Left, BBox.Bottom + offY, 0, 0), Color.White, 1);
            RectangleSprite.DrawRectangle(spriteBatch, new Rectangle(BBox.Right, BBox.Bottom + offY, 0, 0), Color.White, 1);
            RectangleSprite.DrawRectangle(spriteBatch, new Rectangle(BBox.Center.X, BBox.Bottom + offY, 0, 0), Color.White, 1);
            */
        }

        public void Update(GameTime gameTime)
        {
            if (Global.Camera.GetState() != Camera.CamStates.NONE)
                return;

            View currRoom = Global.World.GetActiveViews()[(int)AViews.DEST].GetView();// (Global.World.CurrField).GetMapData()[Global.World.CurrViewX, Global.World.CurrViewY]; // TODO: Update this variable only whenever a map change occurs

            switch (Global.ProtagPhysics)
            {
                case Global.PlatformingPhysics.CLASSIC:
                    ClassicStateMachine(currRoom);
                    break;
                case Global.PlatformingPhysics.REVAMPED:
                    RevampedStateMachine(currRoom);
                    break;
            }

            if (Global.AnimationTimer.OneFrameElapsed())
            {
                if (ContactWarpCooldownTimer > 0)
                    ContactWarpCooldownTimer--;
            }
        }

        #region RevampedMovementCode
        private void RevampedStateMachine(View currRoom)
        {

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
                    RevampedCollideAndMove(currRoom);
                    HandleScreenTransitions(currRoom);
                    break;
                case PlayerState.CLIMBING:
                    /*
                    _moveY = InputManager.DirHeldY;
                    _vsp = _moveSpeed * _moveY;

                    HandleScreenTransitions(currRoom);

                    ChipTypes collidingTile = TilePlaceMeeting(currRoom, BBox, BBox, BBox.X, BBox.Y + _vsp);

                    
                    if (TileGetCellAtPixel(currRoom, BBox.Left, BBox.Top) == ChipTypes.VOID)
                    {
                        BBox.X = new Vector2(BBox.X, ((float)Math.Floor((double)BBox.Y / World.CHIP_SIZE) * World.CHIP_SIZE) - BBox.Height + 3);
                        //BBox += new Vector2(0, Math.Sign(_vsp) * World.CHIP_SIZE);
                        _hsp = 0;
                        _vsp = 0;
                        State = PlayerState.IDLE;
                    } else if (
                        TileGetCellAtPixel(currRoom, BBox.Left, BBox.Bottom) == ChipTypes.VOID ||
                        TileGetCellAtPixel(currRoom, BBox.X, BBox.Bottom) == ChipTypes.SOLID ||
                        TileGetCellAtPixel(currRoom, BBox.X, BBox.Bottom) == ChipTypes.ICE ||
                        TileGetCellAtPixel(currRoom, BBox.X, BBox.Bottom) == ChipTypes.UNCLINGABLE_WALL ||
                        TileGetCellAtPixel(currRoom, BBox.X, BBox.Bottom) == ChipTypes.CLINGABLE_WALL
                        )
                    {
                        BBox = new Vector2(BBox.X, (float)Math.Floor((double)BBox.Y / World.CHIP_SIZE) * World.CHIP_SIZE);
                        //BBox += new Vector2(0, Math.Sign(_vsp) * World.CHIP_SIZE);
                        _hsp = 0;
                        _vsp = 0;

                        if (Math.Floor((BBox.Y + _vsp) / World.CHIP_SIZE) < ROOM_HEIGHT)
                            State = PlayerState.IDLE;
                        else
                        {
                            Global.World.FieldTransitionCardinal(VIEW_DIR.DOWN);
                            _hsp = 0;
                            _vsp = 0;
                            State = PlayerState.SCREEN_TRANSITION;
                        }

                    }
                    
                    if (TilePlaceMeeting(currRoom, BBox, BBox, BBox.X, BBox.Top + _vsp, ChipTypes.SOLID) != ChipTypes.SOLID && TilePlaceMeeting(currRoom, BBox, BBox, BBox.X, BBox.Y + _vsp, ChipTypes.SOLID) != ChipTypes.SOLID)
                    {
                        if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.JUMP)
                        {
                            State = PlayerState.JUMPING;
                            Global.AudioManager.PlaySFX(SFX.P_JUMP);
                            _jumpCount--;
                            _vsp = -_jumpSpeed;
                            FacingY = Facing.UP;
                            _hsp = _moveX * _moveSpeed;
                        }

                        if ((_vsp < 0) && !InputManager.ButtonCheckHeld30FPS(Global.ControllerKeys.JUMP)
                        {
                            Math.Max(_vsp, -_jumpSpeed / 2);
                        }
                    }

                    BBox += new Vector2(0, (float)_vsp);
                    */
                    break;
            }

            if (State != PlayerState.SCREEN_TRANSITION)
                _prevState = State;
        }

        private void RevampedCollideAndMove(View currRoom)
        {

            _moveX = InputManager.DirHeldX;

            if (Global.DevModeEnabled && Global.DevModeDebugPhysics)
            {
                _moveY = InputManager.DirHeldY;
                _vsp = _moveY * _spdWalk;
                _hsp = _moveX * _spdWalk;
                BBox.X += (int)_hsp;
                BBox.Y += (int)_vsp;
            }
            else
            {

                double bboxSide;

                if (_vsp < SPD_GRAV_MAX)
                    _vsp += SPD_GRAVITY;// _moveY * _moveSpeed;
                else
                {
                    _vsp = SPD_GRAV_MAX;
                    _vsp_fract = 0;

                }

                // Check if the player's middle center is touching the floor at the start of this frame
                _isGrounded = (InFloor(currRoom, BBox.Center.X, BBox.Bottom + 1) >= 0);

                // Check if the player's middle center is touching any solid entities at the start of this frame

                ParentInteractableWorldEntity entGroundCheckBL = Global.EntityManager.InstanceCollisionAtPosition(new Point(BBox.Left, BBox.Bottom + 1));
                ParentInteractableWorldEntity entGroundCheckBM = Global.EntityManager.InstanceCollisionAtPosition(new Point(BBox.Center.X, BBox.Bottom + 1));
                ParentInteractableWorldEntity entGroundCheckBR = Global.EntityManager.InstanceCollisionAtPosition(new Point(BBox.Right, BBox.Bottom + 1));

                _myGroundedEntity = null;

                if (entGroundCheckBL != null)
                    _myGroundedEntity = entGroundCheckBL;
                if (entGroundCheckBR != null)
                    _myGroundedEntity = entGroundCheckBR;
                if (entGroundCheckBM != null)
                    _myGroundedEntity = entGroundCheckBM;

                _isGrounded |= (_myGroundedEntity != null);

                // Jump
                if ((_isGrounded || InFloor(currRoom, BBox.Left, BBox.Bottom + 1) >= 0) ||
                    (_isGrounded || InFloor(currRoom, BBox.Right, BBox.Bottom + 1) >= 0) ||
                    (_isGrounded || InFloor(currRoom, BBox.Center.X, BBox.Bottom + 1) >= 0)
                    )
                {
                    if (_jumpCount < _jumpsMax || _fellOffLedge)
                        Global.AudioManager.PlaySFX(SFX.P_GROUNDED);
                    _jumpCount = _jumpsMax;
                    _fellOffLedge = false;
                    //_vsp = 0;
                    //_vsp_fract = 0;
                    State = PlayerState.IDLE;


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
                        _hsp = HelperFunctions.Lerp((float)_hsp, 0, (float)_drag);
                    }
                    else
                    {
                        _hsp = _moveX * _spdWalk;
                    }

                    if (_myGroundedEntity != null)
                    {
                        _hsp += _myGroundedEntity.Hsp;
                        _vsp = _myGroundedEntity.Vsp;
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
                            _wasOnIce = false;
                        }
                        else
                        {
                            if (_vsp >= 0)
                            {
                                if (_jumpCount < _jumpsMax && !_fellOffLedge)
                                    _hsp = _moveX * _spdWalk; // Allow horizontal movement if we're falling
                                else
                                {
                                    // We walked off an edge: Instead, we should fall straight down until we touch ground
                                    _hsp = 0;
                                    _jumpCount = 0;
                                    _fellOffLedge = true;
                                }
                            }
                        }
                    }
                    else
                    {
                        if (_vsp < 0)
                            _hsp = _moveX * _spdWalk;
                    }
                }

                if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.JUMP) && _jumpCount > 0)
                {
                    State = PlayerState.JUMPING;
                    _jumpCount--;
                    _vsp = -SPD_JUMP;
                    _vsp_fract = 0;
                    _isGrounded = false;
                    _hsp = _moveX * _spdWalk;
                    Global.AudioManager.PlaySFX(SFX.P_JUMP);
                }

                if ((_vsp < 0) && !InputManager.ButtonCheckHeld30FPS(Global.ControllerKeys.JUMP))
                {
                    Math.Max(_vsp, -SPD_JUMP / 2);
                }

                // Reapply fractional movement
                _hsp += _hsp_fract;
                _vsp += _vsp_fract;

                // Store and Remove fractions
                _hsp_fract = _hsp - (Math.Floor(Math.Abs(_hsp)) * Math.Sign(_hsp));
                _hsp -= _hsp_fract;
                _vsp_fract = _vsp - (Math.Floor(Math.Abs(_vsp)) * Math.Sign(_vsp));
                _vsp -= _vsp_fract;

                bboxSide = 0;

                // Horizontal Collision

                if (_hsp > 0)
                {
                    bboxSide = BBox.Right;
                }
                else
                    bboxSide = BBox.Left;

                int pxT = TileIsSolid(TileGetAtPixel(currRoom, bboxSide + _hsp, BBox.Top));
                int pxB = TileIsSolid(TileGetAtPixel(currRoom, bboxSide + _hsp, BBox.Bottom));
                int pxSC = TileIsSolid(TileGetAtPixel(currRoom, bboxSide + _hsp, BBox.Center.Y));
                ChipTypes tileBC = TileGetAtPixel(currRoom, BBox.Center.X, BBox.Bottom);
                if (TileIsASlope(tileBC))
                {                               // Did we find a slope tile at the bottom-center of the player's hitbox?
                    pxB = 0;                    // If we did, pretend that we didn't find any solid tiles on the bottom-left/bottom-right and top-left/top-right edges of our hitbox, even if we did
                }

                if (pxT > 0 || pxSC > 0 || pxB > 0)
                {
                    if (_hsp > 0)
                    {
                        BBox.X = BBox.Center.X - (BBox.Center.X % CHIP_SIZE) + CHIP_SIZE - 1 - (BBox.Right - BBox.X);
                    }
                    else
                        BBox.X = BBox.Center.X - (BBox.Center.X % CHIP_SIZE) - (BBox.Left - BBox.X);

                    _hsp = 0;
                }

                /// Horizontal Entity Collision
                
                // Grab all of the entities that are marked as Solid, and collide at the point in question
                List<ParentInteractableWorldEntity> collidedEntities = Global.EntityManager.GetInstancesAtPosition(BBox, new Point((int)(_hsp), 0));
                
                // Iterate through all of those entities, and snap the player next to their X edge
                var entityCount = collidedEntities.Count;
                int _snapX;

                foreach(ParentInteractableWorldEntity checkingEntity in collidedEntities)
                {
                    // Move as close as we can to the checkingEntity
                    if (Math.Sign(_hsp) < 0)
                    {
                        // Moving left; Snap the player one pixel to the right of the checkingEntity's right collision edge
                        _snapX = checkingEntity.BBox.Right;
                        BBox.X = _snapX;
                    }
                    else
                    {
                        // Moving Right; Snap the player one pixel to the left of the checkingEntity's left collision edge
                        _snapX = checkingEntity.BBox.Left - (BBox.Right - BBox.X) - 1;
                        BBox.X = _snapX;
                    }

                    _hsp = 0;
                    _hsp_fract = 0;
                    _hspCarry = checkingEntity.Hsp;
                    //_collision = true;
                    break;
                }

                // Commit Horizontal Movement Adjustments
                BBox.X += (int)_hsp;
                _hspCarry = 0;


                // Vertical Collision
                if (_vsp > 0)
                {
                    bboxSide = BBox.Bottom;
                }
                else
                    bboxSide = BBox.Top;

                // First, consider if we are on a slope or not, using the bottom center pixel + vsp
                ChipTypes tilePXC = TileGetAtPixel(currRoom, BBox.Center.X, BBox.Bottom + _vsp);

                if (!TileIsASlope(tilePXC))
                {
                    int pxL = TileIsSolid(TileGetAtPixel(currRoom, BBox.Left, bboxSide + _vsp));
                    int pxR = TileIsSolid(TileGetAtPixel(currRoom, BBox.Right, bboxSide + _vsp));
                    pxSC = TileIsSolid(TileGetAtPixel(currRoom, BBox.Center.X, bboxSide + _vsp));
                    if (pxL > 0 || pxSC > 0 || pxR > 0)
                    {
                        if (_vsp > 0)
                        {
                            BBox.Y = BBox.Bottom - (BBox.Bottom % CHIP_SIZE) + CHIP_SIZE + 1 - (BBox.Bottom - BBox.Y);
                        }
                        else
                            BBox.Y = BBox.Bottom - (BBox.Bottom % CHIP_SIZE) - CHIP_SIZE - (BBox.Top - BBox.Y);

                        _vsp = 0;
                    }
                }

                // Regardless we hit a slope or not, correct our position, juuust in case we might still have any remaining vertical speed
                int floorDist = InFloor(currRoom, BBox.Center.X, BBox.Bottom + _vsp);
                if (floorDist >= 0)
                {
                    // We are inside the floor. Correct the Y position so we are exactly one pixel above the ground
                    BBox.Y += (int)_vsp;
                    BBox.Y -= (floorDist + 1);

                    _vsp = 0;
                    //_vsp_final = 0;
                    _vsp_fract = 0;
                    floorDist = -1;
                }




                // Walk down slopes
                if (_isGrounded)
                {
                    BBox.Y += Math.Abs(floorDist) - 1;

                    if (Global.AnimationTimer.OneFrameElapsed())
                    {
                        switch (tilePXC)
                        {
                            case ChipTypes.ASCENDING_SLOPE_LEFT:
                                // Drag the player along the slope if they're standing on it
                                BBox.X += 1;
                                BBox.Y += 1;
                                break;
                            case ChipTypes.ASCENDING_SLOPE_RIGHT:
                                // Drag the player along the slope if they're standing on it
                                BBox.X -= 1;
                                BBox.Y -= 1;
                                break;
                            case ChipTypes.ICE_SLOPE_RIGHT:
                            case ChipTypes.ICE_SLOPE_LEFT:          // Enable slippery physics, because we've landed on ice
                                _onIce = true;
                                break;
                        }
                    }

                    var yDiff = (int)(BBox.Bottom - Math.Floor((double)BBox.Bottom / CHIP_SIZE) * CHIP_SIZE);
                    if (yDiff == CHIP_SIZE - 1)
                    {
                        // If the slope continues
                        ChipTypes secondCheckingTile = TileGetAtPixel(currRoom, BBox.Center.X, BBox.Bottom + 1);
                        if (TileIsASlope(secondCheckingTile))
                        {
                            // Move there
                            BBox.Y += Math.Abs(InFloor(currRoom, BBox.Center.X, BBox.Bottom + 1));


                            // If applicable, drag the player along the slope if they're standing on it
                            if (Global.AnimationTimer.OneFrameElapsed())
                            {
                                switch (secondCheckingTile)
                                {
                                    case ChipTypes.ASCENDING_SLOPE_LEFT:
                                        BBox.X += 1;
                                        //BBox.Y += 1;
                                        break;
                                    case ChipTypes.ASCENDING_SLOPE_RIGHT:
                                        BBox.X -= 1;
                                        //BBox.Y -= 1;
                                        break;
                                }
                            }

                        }

                    }
                }

                /// Vertical Entity Collision

                // Grab all of the entities that are marked as Solid, and collide at the point in question
                collidedEntities = Global.EntityManager.GetInstancesAtPosition(BBox, new Point(0, (int)(_vsp)));

                // Iterate through all of those entities, and snap the player next to their X edge
                entityCount = collidedEntities.Count;
                int _snapY;

                foreach (ParentInteractableWorldEntity checkingEntity in collidedEntities)
                {
                    // Move as close as we can to the checkingEntity
                    if (Math.Sign(_vsp) < 0)
                    {
                        // Moving left; Snap the player one pixel to the bottom of the checkingEntity's bottom collision edge
                        _snapY = checkingEntity.BBox.Bottom;
                        BBox.Y = _snapY;
                    }
                    else
                    {
                        // Moving Right; Snap the player one pixel to the top of the checkingEntity's top collision edge
                        _snapY = checkingEntity.BBox.Top - (BBox.Bottom - BBox.Y) - 1;
                        BBox.Y = _snapY;
                    }

                    _vsp = 0;
                    _vsp_fract = 0;
                    _vspCarry = checkingEntity.Vsp;
                    //_collision = true;
                    break;
                }

                // Commit Vertical Movement Adjustments
                BBox.Y += (int)_vsp;
                _vspCarry = 0;

                if (BBox.Y < -8)
                    BBox.Y = -8;
            }
        }

        private void HandleScreenTransitions(View currRoom)
        {
            if (Global.DevModeDisableAutoScreenTransitions)
                return;
            if (Global.Camera.GetState() == Camera.CamStates.NONE)
            {
                // Handle screen transitions when the player touches the border of the screen
                if (BBox.Right + _hsp > ROOM_WIDTH * World.CHIP_SIZE)
                {
                    Global.World.FieldTransitionCardinal(VIEW_DIR.RIGHT);
                    BBox.X = ROOM_WIDTH * World.CHIP_SIZE - (BBox.Width / 2);
                }
                else if (BBox.Left + _hsp < 0)
                {
                    Global.World.FieldTransitionCardinal(VIEW_DIR.LEFT);
                    BBox.X = BBox.Width / 2;
                }
                else if (HUD_HEIGHT + BBox.Bottom + _vsp > (ROOM_HEIGHT * World.CHIP_SIZE) + HUD_HEIGHT)
                {
                    Global.World.FieldTransitionCardinal(VIEW_DIR.DOWN);
                }
                else if (BBox.Top <= 0 &&
                    (_isGrounded || State == PlayerState.CLIMBING || State == PlayerState.JUMPING) &&
                    State != PlayerState.FALLING)
                {
                    if (State == PlayerState.JUMPING)
                    {
                        var currTile = TileGetAtPixel(currRoom, BBox.Left + 1, 0);
                        if (currTile != ChipTypes.LAVA &&
                        (currTile != ChipTypes.WATER) &&
                        (currTile != ChipTypes.WATER_STREAM_DOWN) &&
                        (currTile != ChipTypes.WATER_STREAM_LEFT) &&
                        (currTile != ChipTypes.WATER_STREAM_RIGHT) &&
                        (currTile != ChipTypes.WATER_STREAM_UP))
                            return;
                    }
                    Global.World.FieldTransitionCardinal(VIEW_DIR.UP);
                }
            }
        }

        private void RevampedAccountForSolidFloors(View currRoom)
        {
            if (FacingY == Facing.DOWN)
            {
                if (TilePlaceMeeting(currRoom, BBox, BBox.X, BBox.Y + _vsp, ChipTypes.SOLID) == ChipTypes.SOLID)
                {
                    //BBox += new Vector2(0, -1);
                    while (TilePlaceMeeting(currRoom, BBox, BBox.X, BBox.Y + Math.Sign(_vsp), ChipTypes.SOLID) != ChipTypes.SOLID)
                    {
                        BBox.Y += Math.Sign(_vsp);
                    }
                    _vsp = 0;

                    if (!_isGrounded)
                    {
                        _isGrounded = true;
                        Global.AudioManager.PlaySFX(SFX.P_GROUNDED);
                    }
                }
                if (TilePlaceMeeting(currRoom, BBox, BBox.X, BBox.Y + _vsp, ChipTypes.UNCLINGABLE_WALL) == ChipTypes.UNCLINGABLE_WALL)
                {
                    //BBox += new Vector2(0, -1);
                    while (TilePlaceMeeting(currRoom, BBox, BBox.X, BBox.Y + Math.Sign(_vsp), ChipTypes.UNCLINGABLE_WALL) != ChipTypes.UNCLINGABLE_WALL)
                    {
                        BBox.Y += Math.Sign(_vsp);
                    }
                    _vsp = 0;
                    if (!_isGrounded)
                    {
                        _isGrounded = true;
                        Global.AudioManager.PlaySFX(SFX.P_GROUNDED);
                    }
                }
                if (TilePlaceMeeting(currRoom, BBox, BBox.X, BBox.Y + _vsp, ChipTypes.CLINGABLE_WALL) == ChipTypes.CLINGABLE_WALL)
                {
                    //BBox += new Vector2(0, -1);
                    while (TilePlaceMeeting(currRoom, BBox, BBox.X, BBox.Y + Math.Sign(_vsp), ChipTypes.CLINGABLE_WALL) != ChipTypes.CLINGABLE_WALL)
                    {
                        BBox.Y += Math.Sign(_vsp);
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
        private void ClassicStateMachine(View currRoom)
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

                    ClassicCollideAndMove(_moveSpeed, _moveSpeed, chipline, currRoom);
                    break;
            }

            _prevState = State;
        }

        private void ClassicCollideAndMove(double dx, double dy, int[] chipline, View currRoom)
        {
            double posX = BBox.X;
            double posY = BBox.Y;

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

            /*
            ChipTypes collidingTile = TilePlaceMeeting(currRoom, BBox, BBox, BBox.X + _hsp, BBox.Y);

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
                    while (TilePlaceMeeting(currRoom, BBox, BBox, BBox.X + Math.Sign(_hsp), BBox.Y) != collidingTile)
                    {
                        BBox += new Vector2(Math.Sign(_hsp), 0);
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

                    bboxTileYMin = (int)Math.Floor((double)BBox.Top / _chipHeight);
                    bboxTileYMax = (int)Math.Floor((double)BBox.Bottom / _chipHeight);

                    bboxTileXMin = (int)Math.Floor((double)BBox.Right / _chipWidth);
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

                    // Move player to the new BBox.

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

                    bboxTileYMin = (int)Math.Floor((double)BBox.Top / _chipHeight);
                    bboxTileYMax = (int)Math.Floor((double)BBox.Bottom / _chipHeight);

                    bboxTileXMin = (int)Math.Floor((double)BBox.Left / _chipWidth);
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
                    int tx = (closestTile * World.CHIP_SIZE) + CHIP_EDGE;

                    //movingDistance = Math.Sign(movingDistance) * Math.Min(Math.Abs(movingDistance), Math.Abs(bboxLeft - tx - 1));

                    // Move player to the new BBox.

                    if (bboxLeft + dx <= tx)
                    {
                        posX = tx + BBoxOriginX + 1;
                        dx = 0;
                    }
                    posX += dx;
                }
            }

            // With this new BBox, step the other coordinate, if still not done.
            BBox.X = (int)posX;
            BBox.Y = (int)posY;

            // Step Y
            if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.JUMP))
                Global.AudioManager.PlaySFX(SFX.P_JUMP);

            if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.JUMP) && dy <= 0 && _isGrounded)
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
                    bboxTileYMin = (int)Math.Floor((double)BBox.Bottom / _chipHeight);
                    bboxTileYMax = (int)Math.Floor((bboxBottom + dy) / _chipHeight);

                    bboxTileXMin = (int)Math.Floor((double)BBox.Left / _chipWidth);
                    bboxTileXMax = (int)Math.Floor((double)BBox.Right / _chipWidth);

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

                    bboxTileYMin = (int)Math.Floor((double)BBox.Top / _chipHeight);
                    bboxTileYMax = (int)Math.Floor((bboxTop + dy) / _chipHeight) - 1;

                    bboxTileXMin = (int)Math.Floor((double)BBox.Left / _chipWidth);
                    bboxTileXMax = (int)Math.Floor((double)BBox.Right / _chipWidth);

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

                    int ty = (closestTile * World.CHIP_SIZE) + CHIP_EDGE;
                    if (bboxTop + dy <= ty)
                    {
                        posY = ty + BBoxOriginY + 1;
                        dy = 0;
                    }
                    posY += dy + _vsp;
                }
            }

            // Update the actual BBox
            BBox.X = (int)posX;
            BBox.Y = (int)posY;
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
                case PlayerState.DYING:
                    return "Dying";
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
            BBox.X += (int)offset.X;
            BBox.Y += (int)offset.Y;
        }

        internal bool IsGrounded()
        {
            return _isGrounded;
        }

        internal PlayerState GetPrevState()
        {
            return _prevState;
        }

        internal void SetBBoxToTile(int rTX, int rTY)
        {
            BBox.X = rTX * CHIP_SIZE + BBox.Width - 1;
            BBox.Y = rTY * CHIP_SIZE + BBox.Height + 5;
        }

        public void SetHasBoots(bool value)
        {
            _hasBoots = value;

            if (_hasBoots)
                _spdWalk *= 2;
            else
                _spdWalk = 1.5;
        }

        public void SetHasFeather(bool value)
        {
            _hasFeather = value;

            if (_hasFeather)
                _jumpsMax = 2;
            else
                _jumpsMax = 1;
        }

        internal void SetPositionToTile(Point tileCoords)
        {
            BBox.X = tileCoords.X * CHIP_SIZE;
            BBox.Y = tileCoords.Y * CHIP_SIZE;
        }

        internal void SetPosition(Point coords)
        {
            BBox.X = coords.X;
            BBox.Y = coords.Y;
        }

        internal void Hurt(int damageValue)
        {
            /// TODO: Implement
        }
    }
}
