﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenLaMulana.Audio;
using OpenLaMulana.Entities;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;

namespace OpenLaMulana
{
    public partial class Main : Game
    {
        public Global.GameState State { get; private set; }
        public Global.DisplayMode WindowDisplayMode { get; set; } = Global.DisplayMode.Default;

        public const string GAME_TITLE = "La.MuLANA";
        public const int WINDOW_WIDTH = 256;
        public const int WINDOW_HEIGHT = 192;
        public const int HUD_WIDTH = 256;
        public const int HUD_HEIGHT = 16;
        public float ZoomFactor => WindowDisplayMode == Global.DisplayMode.Default ? 1 : _displayZoomFactor;

        public static float FPS { get; internal set; } = 60f;

        private const string SAVE_FILE_NAME = "Save.dat";
        private const string musPath = "Content/music/";

        private int _displayZoomFactor = 3;
        private int _displayZoomMax = 10;
        private int _pauseToggleTimer = 0;
        private int _pauseToggleTimerReset = 60;
        private int _shaderMode = 0;
        private int _drawnCurrHp = 0;
        private int _drawnCurrExp = 0;
        private int _drawnDestHp = 0;
        private int _drawnDestExp = 0;
        private int _drawnMaxHPLine = 0;
        private Color _hpColorRed = new Color(255, 51, 51, 255);
        private Color _hpColorBlue = new Color(51, 102, 255, 255);
        private Color _hpColorYellow = new Color(204, 204, 51, 255);
        public Color _hpColor { get; private set; }
        private int _maxExpPixels = 0;
        private int _currExpPixels = 0;
        private float _shdHueShiftTime = 0.0f;

        private Protag _protag;
        private Jukebox _jukebox;
        private Effect activeShader = null;
        private Sprite[] _hudSprites = null;
        private Texture2D _itemTex = null;

        public Main()
        {
            Global.GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Global.GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            Global.Main = this;
            Content.RootDirectory = "Content";

            // 60fps game
            IsFixedTimeStep = true;//false;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / FPS); //60);

            IsMouseVisible = true;
            //this.Window.AllowUserResizing = true;

            Global.GameRNG = new GameRNG();
            Global.GameFlags = new GameFlags();
            Global.InGameTimer = new InGameTimer();

            //SaveData reencryptedSave = HelperFunctions.EncryptSaveFile(decryptedSave);
            //HelperFunctions.WriteSaveToFile(decryptedSave, "lamulana_dec.sa0", false);
            //HelperFunctions.WriteSaveToFile(reencryptedSave, "lamulana_reenc.sa0", true);
            //Global.SaveData = new SaveData();



            //Globals.SaveData.ReadEncryptedSave("lamulana.sa0");
            //Globals.SaveData.WriteDecryptedSave("lamulana_dec.sa0");
            //gameRNG.Advance();

            DateTime bootTime = DateTime.Now;
            long sRNG = new DateTimeOffset(bootTime).ToUnixTimeMilliseconds();

            //Globals.SaveData.WriteEncryptedSave("lamulana_enc.sa0", sRNG);

            Global.EntityManager = new EntityManager();
            State = Global.GameState.INITIAL;

            //_fadeInTexturePosX = Protag.DEFAULT_SPRITE_WIDTH;
        }

        enum HudSprites : int
        {
            COINS,
            WEIGHTS,
            WEAPONSBRACKETLEFT,
            WEAPONSBRACKETMIDDLE,
            WEAPONSBRACKETRIGHT,
            SIGILS_BLANK,
            SIGILS_ORIGIN,
            SIGILS_BIRTH,
            SIGILS_LIFE,
            SIGILS_DEATH,
            MAX
        };

        private void InitHUDSprites()
        {
            _itemTex = Global.TextureManager.GetTexture(Global.Textures.ITEM);

            Sprite hudCoinsSprite = new Sprite(_itemTex, 10, 16, 8, 8);
            Sprite hudWeightsSprite = new Sprite(_itemTex, 1 * 8, 3 * 8, 8, 8);
            Sprite hudWeaponsBracketLeft = new Sprite(_itemTex, 2 * 8, 0, 8, 16);
            Sprite hudWeaponsBracketMiddle = new Sprite(_itemTex, 3 * 8, 0, 8, 16);
            Sprite hudWeaponsBracketRight = new Sprite(_itemTex, 4 * 8, 0, 8, 16);

            Sprite hudSigilsBlank = Global.TextureManager.Get8x8Tile(_itemTex, 8, 3, Vector2.Zero);
            Sprite hudSigilsOrigin = Global.TextureManager.Get8x8Tile(_itemTex, 18, 16, Vector2.Zero);
            Sprite hudSigilsBirth = Global.TextureManager.Get8x8Tile(_itemTex, 19, 16, Vector2.Zero);
            Sprite hudSigilsLife = Global.TextureManager.Get8x8Tile(_itemTex, 18, 17, Vector2.Zero);
            Sprite hudSigilsDeath = Global.TextureManager.Get8x8Tile(_itemTex, 19, 17, Vector2.Zero);

            Sprite[] sprites = new Sprite[] {
                hudCoinsSprite, hudWeightsSprite,
                hudWeaponsBracketLeft, hudWeaponsBracketMiddle, hudWeaponsBracketRight,
                hudSigilsBlank, hudSigilsOrigin, hudSigilsBirth, hudSigilsLife, hudSigilsDeath};

            _hudSprites = sprites;
        }

        protected override void Initialize()
        {
            base.Initialize();

            Window.Title = GAME_TITLE;

            Global.GraphicsDeviceManager.PreferredBackBufferHeight = WINDOW_HEIGHT;
            Global.GraphicsDeviceManager.PreferredBackBufferWidth = WINDOW_WIDTH;
            Global.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
            Global.GraphicsDeviceManager.ApplyChanges();

            _displayZoomFactor = 3;
            CalcDisplayZoomMax();
            if (_displayZoomFactor > 4)
                _displayZoomFactor = 1;
            ToggleDisplayMode();

            Global.Camera.UpdateWindowSize(WINDOW_WIDTH * _displayZoomFactor, WINDOW_HEIGHT * _displayZoomFactor, _displayZoomFactor);
        }

        protected override void LoadContent()
        {
            Global.AudioManager = new AudioManager();
            Global.AudioManager.LoadContent(musPath, Content);
            Global.AnimationTimer = new AnimationTimer();

            Global.GraphicsDevice = GraphicsDevice;
            Global.SpriteBatch = new SpriteBatch(Global.GraphicsDevice);
            Global.ShdTransition = Content.Load<Effect>("shaders/shdTransition");
            Global.ShdHueShift = Content.Load<Effect>("shaders/shdHueShift");
            Global.ShdMaskingBlack = Content.Load<Effect>("shaders/shdMaskingBlack");

            _shaderMode = (int)Global.Shaders.NONE;

            Global.TextureManager = new TextureManager();
            Global.TextureManager.InitTextures();

            long val = Global.GameRNG.RollDice(9);

            _protag = new Protag(0, 0, -1, -1, -1, -1, true, null, null);
            Global.Protag = _protag;

            Global.TextManager = new TextManager();
            Global.World = new World(_protag);
            _jukebox = new Jukebox();
            Global.Jukebox = _jukebox;
            Global.Camera = new Camera();
            _protag.Depth = (int)Global.DrawOrder.Protag;
            Global.Camera.SetProtag(_protag);

            Global.Input = new InputManager();
            Global.Input.Init();

            Global.MobileSuperX = new MobileSuperX();

            Global.SpriteDefManager = new SpriteDefManager();

            Global.EntityManager.AddEntity(_protag);
            Global.EntityManager.AddEntity(Global.World);
            Global.EntityManager.AddEntity(Global.SpriteDefManager);
            Global.EntityManager.AddEntity(Global.MobileSuperX);
            
            Global.GameMenu = new GameMenu(Global.ScreenOverlayState.INVISIBLE, Global.TextManager);
            Global.EntityManager.AddEntity(Global.GameMenu);
            Global.InitMantras();
            InitHUDSprites();
            LoadSaveState();
        }

        protected override void UnloadContent()
        {
            Global.AudioManager.UnloadContent();
        }

        public World GetWorld()
        {
            return Global.World;
        }

        protected override void Update(GameTime gameTime)
        {
            Global.GameTime = gameTime;
            Global.Input.Update(gameTime);

            if (InputManager.DirectKeyboardCheckPressed(Keys.Escape))
                Exit();

            Global.Camera.Update(gameTime);
            Global.AudioManager.Update(gameTime);

            Global.AnimationTimer.Update(gameTime);
            base.Update(gameTime);
            bool isAltKeyDown = (InputManager.DirectKeyboardCheckDown(Keys.LeftAlt) || InputManager.DirectKeyboardCheckDown(Keys.RightAlt));

                if (isAltKeyDown && InputManager.DirectKeyboardCheckPressed(Keys.Enter))
                {
                    Global.GraphicsDeviceManager.ToggleFullScreen();
                    Global.GraphicsDeviceManager.ApplyChanges();
                }

                if (Global.DevModeEnabled)
                {
                    MouseState mouseState = Mouse.GetState();

                    if (mouseState.RightButton == ButtonState.Pressed)
                    {
                        int centerX = _protag.BBox.Width / 2;
                        _protag.BBox.X = (int)Math.Floor((float)mouseState.X / _displayZoomFactor) - centerX;
                        _protag.BBox.Y = (int)Math.Floor((float)mouseState.Y / _displayZoomFactor) - World.HUD_HEIGHT - _protag.BBox.Height;
                    }
                    if (InputManager.DirectKeyboardCheckPressed(Keys.F6))
                    {
                        //ResetSaveState();
                        _shaderMode++;
                        if (_shaderMode >= (int)Global.Shaders.MAX)
                            _shaderMode = 0;
                    }
                }

                if (InputManager.DirectKeyboardCheckPressed(Keys.F7) && !Global.GraphicsDeviceManager.IsFullScreen)
                {
                    CalcDisplayZoomMax();
                    _displayZoomFactor += 1;
                    if (_displayZoomFactor > _displayZoomMax)
                        _displayZoomFactor = 1;
                    ToggleDisplayMode();
                }

            switch (State)
            {
                case Global.GameState.PLAYING:
                    Global.MobileSuperX.Update(gameTime);
                    if (Global.Camera.GetState() == Camera.CamStates.NONE)
                    {
                        switch (Global.MobileSuperX.GetState())
                        {
                            case Global.MSXStates.INACTIVE:
                                Global.InGameTimer.Update(gameTime);
                                if (InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.MENU_OPEN_INVENTORY))
                                {
                                    State = Global.GameState.MSX_OPEN;
                                    Global.MobileSuperX.SetState(Global.MSXStates.INVENTORY);
                                    Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                                    Global.MobileSuperX.VerifyThatPlayerHasAtLeastOneSubweapon();
                                }

                                if (InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.MENU_OPEN_MSX_EMULATOR))
                                {
                                    State = Global.GameState.MSX_OPEN;
                                    Global.MobileSuperX.SetState(Global.MSXStates.EMULATOR);
                                    Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                                    Global.MobileSuperX.VerifyThatPlayerHasAtLeastOneSubweapon();
                                }

                                if (InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.MENU_OPEN_MSX_SOFTWARE_SELECTION))
                                {
                                    State = Global.GameState.MSX_OPEN;
                                    Global.MobileSuperX.SetState(Global.MSXStates.SOFTWARE_SELECTION);
                                    Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                                    Global.MobileSuperX.VerifyThatPlayerHasAtLeastOneSubweapon();
                                }

                                if (InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.MENU_OPEN_CONFIG))
                                {
                                    State = Global.GameState.MSX_OPEN;
                                    Global.MobileSuperX.SetState(Global.MSXStates.CONFIG_SCREEN);
                                    Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                                    Global.MobileSuperX.VerifyThatPlayerHasAtLeastOneSubweapon();
                                }
                                break;
                            case Global.MSXStates.INVENTORY:
                            case Global.MSXStates.SOFTWARE_SELECTION:
                            case Global.MSXStates.EMULATOR:
                            case Global.MSXStates.SCANNING:
                                Global.InGameTimer.Update(gameTime);
                                break;
                        }

                        if (Global.DevModeEnabled && Global.DevModeRunWhileInactive)
                        {
                            if (!isAltKeyDown && InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.PAUSE) && State == Global.GameState.PLAYING)
                                ToggleGamePause();
                        }
                        else if ((!isAltKeyDown && InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.PAUSE) && State == Global.GameState.PLAYING) || !IsActive)
                        {
                            ToggleGamePause();
                        }
                    }

                    Global.EntityManager.Update(gameTime);
                    UpdateExpAndHP();
                    break;
                case Global.GameState.CUTSCENE:
                    Global.EntityManager.Update(gameTime);
                    UpdateExpAndHP();
                    break;
                case Global.GameState.ITEM_ACQUIRED:
                    Global.NineSliceBox.Update(gameTime);
                    UpdateExpAndHP();
                    break;
                case Global.GameState.PAUSED:
                    Global.AudioManager.PauseMusic();
                    if (!isAltKeyDown && InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.PAUSE))
                    {
                        ToggleGamePause();
                    }
                    break;
                case Global.GameState.TRANSITION:
                    State = Global.GameState.PLAYING;
                    _protag.Initialize();
                    UpdateExpAndHP();
                    break;
                case Global.GameState.MSX_OPEN:
                    Global.MobileSuperX.Update(gameTime);
                    Global.InGameTimer.Update(gameTime);
                    break;
                case Global.GameState.MSX_LOADING_FILE:
                    Global.MobileSuperX.Update(gameTime);
                    break;
                case Global.GameState.INITIAL:
                    if (InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.PAUSE))
                    {
                        StartGame();
                    }
                    break;
            }

            if (Global.AnimationTimer.OneFrameElapsed(true))
            {
                if (_pauseToggleTimer > 0)
                    _pauseToggleTimer--;
            }
        }

        private void UpdateExpAndHP()
        {
            if (Global.Inventory.CurrExp >= Global.Inventory.ExpMax)
            {
                Global.Inventory.HP = Global.Inventory.HPMax;

                if (Global.Inventory.ExpMax > 0)
                    Global.Inventory.CurrExp = Global.Inventory.CurrExp % Global.Inventory.ExpMax;
                else
                    Global.Inventory.CurrExp = 0;

                if (State != Global.GameState.INITIAL)
                    Global.AudioManager.PlaySFX(SFX.P_EXP_MAX);
            }


            int trueHPMax = Global.Inventory.TrueHPMax;
            int maxHP = Global.Inventory.HPMax;
            // Calculate the white line representing max HP
            float maxHPRatio = maxHP / (float)trueHPMax;
            _drawnMaxHPLine = Math.Clamp((int)Math.Round(maxHPRatio * 88), 1, 88);

            if (maxHP > 0)
                _drawnDestHp = (int)Math.Round(88 * ((float)Global.Inventory.HP / trueHPMax));
            else
                _drawnDestHp = 0;

            if (Global.Inventory.HPMax <= 0)
                Global.Inventory.HPMax = 32;
            float healthColorRatio = (float)_drawnCurrHp / _drawnMaxHPLine;
            if (healthColorRatio <= 0.25f)
            {
                if (_hpColor != _hpColorRed && Global.Inventory.HP != Global.Inventory.HPMax)
                    Global.AudioManager.PlaySFX(SFX.P_LOW_HP);
                _hpColor = _hpColorRed; // Change health color to red
            }
            else if (healthColorRatio < 0.5f)
                _hpColor = _hpColorYellow; // Change health color to yellow
            else
                _hpColor = _hpColorBlue; // Health color is blue by default

            if (_drawnCurrHp != _drawnDestHp)
                _drawnCurrHp++;
            if (_drawnCurrHp > _drawnMaxHPLine)
                _drawnCurrHp = 0;

            // Calculate the Current and Max EXP positions for drawing
            _drawnDestExp = Global.Inventory.CurrExp;
            float currEXPRatio;
            if (Global.QoLChanges)
            {
                _maxExpPixels = Math.Clamp((int)Math.Round(maxHPRatio * 88), 1, 88);
                currEXPRatio = _drawnDestExp / (float)trueHPMax;
                _currExpPixels = (int)Math.Round(currEXPRatio * 88);
                if (_maxExpPixels < 1)
                    _maxExpPixels = 1;
            }
            else
            {
                _maxExpPixels = Global.Inventory.ExpMax;
                _currExpPixels = Global.Inventory.CurrExp;
                if (_maxExpPixels < 1)
                    _maxExpPixels = 1;
            }

            if (_drawnCurrExp != _drawnDestExp)
                _drawnCurrExp++;

            if (_drawnCurrExp > Global.InventoryStruct.ExpMaxClassic)
                _drawnCurrExp = 0;
        }

        private void CalcDisplayZoomMax()
        {
            int idealWidth = 0;
            int idealHeight = WINDOW_HEIGHT;

            int monitorWidth = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width;
            int monitorHeight = GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height;

            double aspect_ratio = (double)monitorWidth / monitorHeight;

            idealWidth = (int)Math.Round(idealHeight * aspect_ratio);

            // Check for odd numbers
            if (idealWidth % 2 == 1)
                idealWidth++;

            int maxZoom = (int)Math.Floor((double)monitorWidth / idealWidth);
            _displayZoomMax = maxZoom;
        }

        private void ToggleGamePause()
        {
            if (_pauseToggleTimer > 0)
                return;

            Global.AudioManager.PlaySFX(SFX.PAUSE);

            if (State == Global.GameState.PLAYING)
            {
                _pauseToggleTimer = _pauseToggleTimerReset;
                State = Global.GameState.PAUSED;
                Global.AudioManager.PauseMusic();
            }
            else if (State == Global.GameState.PAUSED)
            {
                State = Global.GameState.PLAYING;
                _pauseToggleTimer = _pauseToggleTimerReset;
                Global.AudioManager.ResumeMusic();
            }
        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            //  Globals.Camera.GetTransformation(GraphicsDevice)
            //            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: _transformMatrix);

            switch (_shaderMode)
            {
                default:
                case (int)Global.Shaders.NONE:
                    if (activeShader != null)
                        activeShader = null;
                    break;
                case (int)Global.Shaders.HUE_SHIFT:
                    if (activeShader != Global.ShdHueShift)
                        activeShader = Global.ShdHueShift;
                    Global.ShdHueShift.Parameters["time"].SetValue(_shdHueShiftTime);

                    _shdHueShiftTime = Math.Clamp((2.6f * (float)gameTime.TotalGameTime.TotalSeconds) % 0.9f, 0.01f, 0.9f);
                    break;
                case (int)Global.Shaders.TRANSITION:
                    if (activeShader != Global.ShdTransition)
                        activeShader = Global.ShdTransition;
                    break;
            }

            if (Global.MobileSuperX.GetState() == Global.MSXStates.INACTIVE)
                Global.EntityManager.Draw(Global.SpriteBatch, gameTime, GraphicsDevice);

            Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: Global.Camera.GetTransformation(GraphicsDevice),
            effect: activeShader);
            //_shdHueShift.CurrentTechnique.Passes[0].Apply();
            switch (State)
            {
                case Global.GameState.INITIAL:
                case Global.GameState.TRANSITION:
                    //_spriteBatch.Draw(_fadeInTexture, new Rectangle((int)Math.Round(_fadeInTexturePosX), 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                    //Globals.TextManager.DrawText(0, 0, "Press Enter to Begin\\10WASD to move camera\\10J/K to switch maps");
                    Global.TextManager.QueueText(0, 0, "Press Select, or F1, to Begin");
                    break;
                default:
                case Global.GameState.MSX_OPEN:
                    Global.MobileSuperX.Draw(Global.SpriteBatch, gameTime);
                    break;
                case Global.GameState.MSX_LOADING_FILE:
                    break;
                case Global.GameState.ITEM_ACQUIRED:
                case Global.GameState.CUTSCENE:
                    DrawHud(Global.SpriteBatch, gameTime);
                    break;
                case Global.GameState.PAUSED:
                    DrawHud(Global.SpriteBatch, gameTime);
                    HelperFunctions.DrawRectangle(Global.SpriteBatch, new Rectangle(13 * 8, 12 * 8, 5 * 8, 8), Color.Black);
                    Global.TextManager.QueueText(13 * 8, 12 * 8, "PAUSE");
                    break;
                case Global.GameState.PLAYING:
                    //View[,] thisFieldMapData = Global.World.GetField(Global.World.CurrField).GetMapData();
                    //View thisView = thisFieldMapData[Global.World.CurrViewX, Global.World.CurrViewY];

                    /*
                    int[] viewDest = thisView.GetDestinationView(movingDirection);

                    //currRoomX = viewDest[(int)VIEW_DEST.WORLD];
                    int destField = viewDest[(int)VIEW_DEST.FIELD];
                    int destRoomX = viewDest[(int)VIEW_DEST.X];
                    int destRoomY = viewDest[(int)VIEW_DEST.Y];*/


                    if (Global.DevModeEnabled)
                    {
                        Vector2 _camPos = Global.Camera.Position;
                        Rectangle rect = new Rectangle((int)_camPos.X, (int)_camPos.Y, World.VIEW_WIDTH * World.CHIP_SIZE, World.HUD_HEIGHT);
                        HelperFunctions.DrawRectangle(Global.SpriteBatch, rect, Color.Black);

                        switch (Global.DebugStatsState)
                        {
                            case Global.DebugStats.GAME_HUD:
                                DrawHud(Global.SpriteBatch, gameTime);
                                break;
                            case Global.DebugStats.ENTITY_COUNT:
                                List<IGameEntity> fieldEntities = Global.World.GetField(Global.World.CurrField).GetFieldEntities();
                                List<IGameEntity> viewEntities = Global.World.GetField(Global.World.CurrField).GetViewEntities();

                                int staticEntityCount = Global.EntityManager.GetStaticEntityCount();
                                int entityCount = Global.EntityManager.GetCount();

                                Global.TextManager.DrawText(_camPos, String.Format("View Entities: {0}   Static: {1}\\10Field Entities:{2}  Total: {3}", viewEntities.Count, staticEntityCount, fieldEntities.Count, entityCount));
                                break;
                            case Global.DebugStats.ROOM_COORDS_INFO:
                                ActiveView[] activeViews = Global.World.GetActiveViews();
                                View currView = activeViews[(int)World.AViews.CURR].GetView();
                                View destView = activeViews[(int)World.AViews.DEST].GetView();
                                Global.TextManager.DrawText(_camPos, String.Format("CurrView: [{0},{1}]\\10DestView: [{2},{3}]", currView.X, currView.Y, destView.X, destView.Y));
                                break;
                            case Global.DebugStats.PLAYER_HITBOX_INFO:
                                string playerCoordsStr = "\\10bboxLeft: " + Math.Floor(_protag.BBox.Left / (float)World.CHIP_SIZE).ToString()
                                                            + "\\10bboxBottom: " + Math.Floor(_protag.BBox.Bottom / (float)World.CHIP_SIZE).ToString()
                                                            + "\\10bboxRight: " + Math.Floor(_protag.BBox.Right / (float)World.CHIP_SIZE).ToString()
                                                            + "\\10bboxTop: " + Math.Floor(_protag.BBox.Top / (float)World.CHIP_SIZE).ToString();
                                Global.TextManager.DrawText(_camPos + new Vector2(0 * 8), playerCoordsStr);
                                break;
                            case Global.DebugStats.IN_GAME_TIMER_VIEWER:
                                string inGameTimeStr = Global.InGameTimer.ToString();
                                Global.TextManager.DrawText(new Vector2(31 * World.CHIP_SIZE, 1 * World.CHIP_SIZE), inGameTimeStr, 32, Color.White, World.VIEW_DIR.RIGHT);
                                break;
                            case Global.DebugStats.FPS_VIEWER:
                                double frameRate = (1 / gameTime.ElapsedGameTime.TotalSeconds);
                                string fmt = "00.00";
                                string fpsStr = String.Format("FPS: {0}",frameRate.ToString(fmt));
                                Global.TextManager.DrawText(_camPos + new Vector2(31 * World.CHIP_SIZE, 0), fpsStr, 32, Color.White, World.VIEW_DIR.RIGHT);
                                break;
                        }
                    } else
                        DrawHud(Global.SpriteBatch, gameTime);
                    break;

            }

            Global.TextManager.Draw(Global.SpriteBatch, gameTime);
            Global.SpriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHud(SpriteBatch spriteBatch, GameTime gameTime)
        {
            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)Global.Camera.Position.X, (int)Global.Camera.Position.Y, HUD_WIDTH, HUD_HEIGHT), new Color(0, 0, 0, 255));

            Vector2 _camPos = Global.Camera.Position;

            _hudSprites[(int)HudSprites.COINS].Draw(spriteBatch, _camPos + new Vector2(27 * 8, 0 * 8));
            _hudSprites[(int)HudSprites.WEIGHTS].Draw(spriteBatch, _camPos + new Vector2(27 * 8, 1 * 8));
            _hudSprites[(int)HudSprites.WEAPONSBRACKETLEFT].Draw(spriteBatch, _camPos + new Vector2(15 * 8, 0 * 8));
            _hudSprites[(int)HudSprites.WEAPONSBRACKETMIDDLE].Draw(spriteBatch, _camPos + new Vector2(18 * 8, 0 * 8));
            _hudSprites[(int)HudSprites.WEAPONSBRACKETRIGHT].Draw(spriteBatch, _camPos + new Vector2(24 * 8, 0 * 8));


            if (Global.Inventory.ObtainedTreasures[Global.ObtainableTreasures.ORIGIN_SIGIL])
                _hudSprites[(int)HudSprites.SIGILS_ORIGIN].Draw(spriteBatch, _camPos + new Vector2(25 * 8, 0 * 8));
            else
                _hudSprites[(int)HudSprites.SIGILS_BLANK].Draw(spriteBatch, _camPos + new Vector2(25 * 8, 0 * 8));

            if (Global.Inventory.ObtainedTreasures[Global.ObtainableTreasures.BIRTH_SIGIL])
                _hudSprites[(int)HudSprites.SIGILS_BIRTH].Draw(spriteBatch, _camPos + new Vector2(25 * 8, 1 * 8));
            else
                _hudSprites[(int)HudSprites.SIGILS_BLANK].Draw(spriteBatch, _camPos + new Vector2(25 * 8, 1 * 8));

            if (Global.Inventory.ObtainedTreasures[Global.ObtainableTreasures.LIFE_SIGIL])
                _hudSprites[(int)HudSprites.SIGILS_LIFE].Draw(spriteBatch, _camPos + new Vector2(26 * 8, 0 * 8));
            else
                _hudSprites[(int)HudSprites.SIGILS_BLANK].Draw(spriteBatch, _camPos + new Vector2(26 * 8, 0 * 8));

            if (Global.Inventory.ObtainedTreasures[Global.ObtainableTreasures.DEATH_SIGIL])
                _hudSprites[(int)HudSprites.SIGILS_DEATH].Draw(spriteBatch, _camPos + new Vector2(26 * 8, 1 * 8));
            else
                _hudSprites[(int)HudSprites.SIGILS_BLANK].Draw(spriteBatch, _camPos + new Vector2(26 * 8, 1 * 8));

            Global.TextManager.DrawTextImmediate(Global.Camera.Position, "VIT\\10EXP");
            for (int y = 0; y < 15; y++)
            {
                switch (y)
                {
                    case 0:
                    case 6:
                    case 8:
                    case 14:
                        HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 24, (int)_camPos.Y + y, 88, 1), new Color(255, 255, 255, 255));
                        break;
                }
            }


            // Draw the HP bar and its max position

            if (_drawnDestHp > 0)
                HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 24, (int)_camPos.Y + 1, _drawnCurrHp, 5), _hpColor);

            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 24 + _drawnMaxHPLine, (int)_camPos.Y + 0, 1, 7), new Color(255, 255, 255, 255));

            // Draw the white line representing max EXP
            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 24 + _maxExpPixels, (int)_camPos.Y + 8, 1, 7), new Color(255, 255, 255, 255));

            // Draw the current Exp
            if (_drawnCurrExp > 0)
                HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 24, (int)_camPos.Y + 9, _drawnCurrExp, 5), new Color(51, 204, 51, 255));

            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 23, (int)_camPos.Y + 0, 1, 7), new Color(255, 255, 255, 255));
            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 23, (int)_camPos.Y + 8, 1, 7), new Color(255, 255, 255, 255));
            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 112, (int)_camPos.Y + 0, 1, 7), new Color(255, 255, 255, 255));
            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 112, (int)_camPos.Y + 8, 1, 7), new Color(255, 255, 255, 255));

            // Draw the Weapon Icon
            int equippedMainWeapon = (int)Global.Inventory.EquippedMainWeapon;
            if (equippedMainWeapon >= 0 && equippedMainWeapon < 255)
                spriteBatch.Draw(_itemTex,
                    new Rectangle((int)_camPos.X + (16 * World.CHIP_SIZE), (int)_camPos.Y, 16, 16),
                    new Rectangle(256 + (equippedMainWeapon % 4) * World.CHIP_SIZE * 2, 0 + (equippedMainWeapon / 4) * World.CHIP_SIZE * 2, 16, 16),
                    Color.White);

            // Draw the Subweapon Icon
            int equippedSubWeapon = (int)Global.Inventory.EquippedSubWeapon;
            if (equippedSubWeapon >= 0 && equippedSubWeapon < 255)
                spriteBatch.Draw(_itemTex,
                    new Rectangle((int)_camPos.X + (19 * World.CHIP_SIZE), (int)_camPos.Y, 16, 16),
                    new Rectangle(256 + (equippedSubWeapon % 4) * World.CHIP_SIZE * 2, 96 + (equippedSubWeapon / 4) * World.CHIP_SIZE * 2, 16, 16),
                    Color.White);

            int subweaponCount = -1;
            switch (Global.Inventory.EquippedSubWeapon)
            {
                case Global.SubWeapons.SHURIKEN:
                    subweaponCount = Global.Inventory.ShurikenCount;
                    break;
                case Global.SubWeapons.THROWING_KNIFE:
                    subweaponCount = Global.Inventory.ThrowingKnifeCount;
                    break;
                case Global.SubWeapons.FLARES:
                    subweaponCount = Global.Inventory.FlaresCount;
                    break;
                case Global.SubWeapons.SPEAR:
                    subweaponCount = Global.Inventory.SpearsCount;
                    break;
                case Global.SubWeapons.BOMB:
                    subweaponCount = Global.Inventory.BombCount;
                    break;
                case Global.SubWeapons.ANKH_JEWEL:
                    subweaponCount = Global.Inventory.AnkhJewelCount;
                    break;
            }

            string fmt = "000";
            if (subweaponCount >= 0)
                Global.TextManager.DrawText(_camPos + new Vector2(21 * 8, 1 * 8), subweaponCount.ToString(fmt));
            else
                Global.TextManager.DrawText(_camPos + new Vector2(21 * 8, 1 * 8), "---");
            int numCoins = Global.Inventory.CoinCount;
            int numWeights = Global.Inventory.WeightCount;

            Global.TextManager.DrawText(_camPos + new Vector2(28 * 8, 0 * 8), String.Format("={0}\\10={1}", numCoins.ToString(fmt), numWeights.ToString(fmt)));
        }

        private bool StartGame()
        {
            if (State != Global.GameState.INITIAL)
                return false;

            UpdateExpAndHP();
            Global.World.InitWorldEntities();

            State = Global.GameState.TRANSITION;
            return true;
        }

        public bool Replay()
        {
            if (State != Global.GameState.GAME_OVER)
                return false;

            State = Global.GameState.PLAYING;
            _protag.Initialize();
            InputManager.BlockInputTemporarily();

            return true;

        }

        public void SaveGame()
        {
            /*
            SaveState saveState = new SaveState
            {
                //Highscore = _scoreBoard.HighScore,
                //HighscoreDate = _highscoreDate
            };

            try
            {
                using (FileStream fileStream = new FileStream(SAVE_FILE_NAME, FileMode.Create))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    binaryFormatter.Serialize(fileStream, saveState);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occurred while saving the game: " + ex.Message);
            }
            */
        }

        public void LoadSaveState()
        {

            /*
             * 
            try
            {
                using (FileStream fileStream = new FileStream(SAVE_FILE_NAME, FileMode.OpenOrCreate))
                {
                    BinaryFormatter binaryFormatter = new BinaryFormatter();
                    SaveState saveState = binaryFormatter.Deserialize(fileStream) as SaveState;

                    if (saveState != null)
                    {
                        //if(_scoreBoard != null)
                        //    _scoreBoard.HighScore = saveState.Highscore;

                        //_highscoreDate = saveState.HighscoreDate;

                    }

                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine("An error occurred while loading the game: " + ex.Message);
            }*/
        }

        private void ResetSaveState()
        {
            //_scoreBoard.HighScore = 0;
            //_highscoreDate = default(DateTime);

            SaveGame();

        }

        public int GetDisplayZoomFactor()
        {
            return _displayZoomFactor;
        }

        public void SetDisplayZoomFactor(int value)
        {
            _displayZoomFactor = value;
        }

        public int GetDisplayZoomMax()
        {
            return _displayZoomMax;
        }

        public void ToggleDisplayMode()
        {
            Global.GraphicsDeviceManager.PreferredBackBufferHeight = WINDOW_HEIGHT * _displayZoomFactor;
            Global.GraphicsDeviceManager.PreferredBackBufferWidth = WINDOW_WIDTH * _displayZoomFactor;
            Global.Camera.UpdateWindowSize(WINDOW_WIDTH * _displayZoomFactor, WINDOW_HEIGHT * _displayZoomFactor, _displayZoomFactor);
            Global.GraphicsDeviceManager.ApplyChanges();
            Window.Position = new Point((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2) - (Global.GraphicsDeviceManager.PreferredBackBufferWidth / 2), (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2) - (Global.GraphicsDeviceManager.PreferredBackBufferHeight / 2));
        }

        internal void SetState(Global.GameState state)
        {
            State = state;
        }
    }
}