using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenLaMulana.Audio;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.Intrinsics.X86;
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

        public static float FPS { get; internal set; } = 30f;

        private const string SAVE_FILE_NAME = "Save.dat";
        private const string musPath = "Content/music/";

        private int _displayZoomFactor = 3;
        private int _displayZoomMax = 10;
        private int _pauseToggleTimer = 0;
        private int _shaderMode = 0;
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

            // 30fps game
            IsFixedTimeStep = true;//false;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / FPS); //60);

            IsMouseVisible = true;
            //this.Window.AllowUserResizing = true;

            Global.GameRNG = new GameRNG();
            Global.SaveData = new SaveData();
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

        enum HudSprites
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
            Sprite hudSigilsBirth = Global.TextureManager.Get8x8Tile(_itemTex, 18, 17, Vector2.Zero);
            Sprite hudSigilsLife = Global.TextureManager.Get8x8Tile(_itemTex, 19, 16, Vector2.Zero);
            Sprite hudSigilsDeath = Global.TextureManager.Get8x8Tile(_itemTex, 19, 17, Vector2.Zero);

            Sprite[] sprites = new Sprite[] {
                hudCoinsSprite, hudWeightsSprite,
                hudWeaponsBracketLeft, hudWeaponsBracketMiddle, hudWeaponsBracketRight,
                hudSigilsBlank, hudSigilsOrigin, hudSigilsBirth, hudSigilsLife, hudSigilsDeath};

            _hudSprites = sprites;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            Window.Title = GAME_TITLE;

            Global.GraphicsDeviceManager.PreferredBackBufferHeight = WINDOW_HEIGHT;
            Global.GraphicsDeviceManager.PreferredBackBufferWidth = WINDOW_WIDTH;
            Global.GraphicsDeviceManager.SynchronizeWithVerticalRetrace = true;
            Global.GraphicsDeviceManager.ApplyChanges();

            _displayZoomFactor = 3;
            _displayZoomMax = CalcDisplayZoomMax();
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

            _protag = new Protag(new Vector2(0, 0));
            Global.Protag = _protag;
            Global.TextManager = new TextManager();
            Global.World = new World(_protag);
            _jukebox = new Jukebox();
            Global.Jukebox = _jukebox;
            Global.Camera = new Camera();
            _protag.Depth = (int)Global.DrawOrder.Protag;
            Global.Camera.SetProtag(_protag);

            Global.GlobalInput = new InputManager();
            Global.GlobalInput.Init();

            Global.MobileSuperX = new MobileSuperX();

            Global.SpriteDefManager = new SpriteDefManager();

            Global.EntityManager.AddEntity(_protag);
            Global.EntityManager.AddEntity(Global.World);
            Global.EntityManager.AddEntity(Global.SpriteDefManager);
            Global.EntityManager.AddEntity(Global.MobileSuperX);
            
            Global.GameMenu = new GameMenu(Global.ScreenOverlayState.INVISIBLE, Global.TextManager);
            Global.EntityManager.AddEntity(Global.GameMenu);
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
            Global.GlobalInput.Update(gameTime);

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

            switch (State)
            {
                case Global.GameState.PLAYING:
                    Global.MobileSuperX.Update(gameTime);

                    if (Global.Camera.GetState() == Camera.CamStates.NONE)
                    {
                        switch (Global.MobileSuperX.GetState())
                        {
                            case Global.MSXStates.INACTIVE:
                                if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_INVENTORY])
                                {
                                    State = Global.GameState.MSX_OPEN;
                                    Global.MobileSuperX.SetState(Global.MSXStates.INVENTORY);
                                    Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                                }

                                if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_MSX_EMULATOR])
                                {
                                    State = Global.GameState.MSX_OPEN;
                                    Global.MobileSuperX.SetState(Global.MSXStates.EMULATOR);
                                    Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                                }

                                if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_MSX_ROM_SELECTION])
                                {
                                    State = Global.GameState.MSX_OPEN;
                                    Global.MobileSuperX.SetState(Global.MSXStates.ROM_SELECTION);
                                    Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                                }

                                if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_CONFIG])
                                {
                                    State = Global.GameState.MSX_OPEN;
                                    Global.MobileSuperX.SetState(Global.MSXStates.CONFIG_SCREEN);
                                    Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                                }
                                break;
                        }

                        if (!isAltKeyDown && InputManager.PressedKeys[(int)Global.ControllerKeys.PAUSE] && State == Global.GameState.PLAYING)
                        {
                            ToggleGamePause();
                        }
                    }

                    Global.EntityManager.Update(gameTime);
                    break;
                case Global.GameState.PAUSED:
                    JukeboxRoutine();
                    Global.AudioManager.PauseMusic();
                    if (!isAltKeyDown && InputManager.PressedKeys[(int)Global.ControllerKeys.PAUSE])
                    {
                        ToggleGamePause();
                    }
                    break;
                case Global.GameState.TRANSITION:
                    State = Global.GameState.PLAYING;
                    _protag.Initialize();
                    break;
                case Global.GameState.MSX_OPEN:
                    Global.MobileSuperX.Update(gameTime);
                    break;
                case Global.GameState.MSX_LOADING_FILE:
                    Global.MobileSuperX.Update(gameTime);
                    JukeboxRoutine();
                    break;
                case Global.GameState.INITIAL:
                    if (InputManager.PressedKeys[(int)Global.ControllerKeys.PAUSE])
                    {
                        StartGame();
                    }
                    break;
            }

            if (Global.AnimationTimer.OneFrameElapsed())
            {
                if (_pauseToggleTimer > 0)
                    _pauseToggleTimer--;
            }

            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
                _protag.Position = new Vector2(mouseState.X / _displayZoomFactor, mouseState.Y / _displayZoomFactor);

            if (InputManager.DirectKeyboardCheckPressed(Keys.F6))
            {
                //ResetSaveState();
                _shaderMode++;
                if (_shaderMode >= (int)Global.Shaders.MAX)
                    _shaderMode = 0;
            }

            if (InputManager.DirectKeyboardCheckPressed(Keys.F7) && !Global.GraphicsDeviceManager.IsFullScreen)
            {
                _displayZoomMax = CalcDisplayZoomMax();
                _displayZoomFactor += 1;
                if (_displayZoomFactor > _displayZoomMax)
                    _displayZoomFactor = 1;
                ToggleDisplayMode();
            }
        }

        private int CalcDisplayZoomMax()
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
            return maxZoom;
        }

        private void ToggleGamePause()
        {
            if (_pauseToggleTimer > 0)
                return;

            Global.AudioManager.PlaySFX(SFX.PAUSE);

            if (State == Global.GameState.PLAYING)
            {
                _pauseToggleTimer = 30;
                State = Global.GameState.PAUSED;
                Global.AudioManager.PauseMusic();
            }
            else if (State == Global.GameState.PAUSED)
            {
                State = Global.GameState.PLAYING;
                _pauseToggleTimer = 30;
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
                    Global.TextManager.DrawText(0, 0, "Press Select, or F1, to Begin");
                    break;
                default:
                case Global.GameState.MSX_OPEN:
                    Global.MobileSuperX.Draw(Global.SpriteBatch, gameTime);
                    break;
                case Global.GameState.MSX_LOADING_FILE:
                    break;
                case Global.GameState.PAUSED:
                    DrawHud(Global.SpriteBatch, gameTime);
                    Global.TextManager.DrawText(13 * 8, 12 * 8, "PAUSE");
                    //_jukebox.Draw(_spriteBatch, gameTime);
                    break;
                case Global.GameState.PLAYING:
                    DrawHud(Global.SpriteBatch, gameTime);
                    //_jukebox.Draw(_spriteBatch, gameTime);

                    View[,] thisFieldMapData = Global.World.GetField(Global.World.CurrField).GetMapData();
                    View thisView = thisFieldMapData[Global.World.CurrViewX, Global.World.CurrViewY];

                    /*
                    int[] viewDest = thisView.GetDestinationView(movingDirection);

                    //currRoomX = viewDest[(int)VIEW_DEST.WORLD];
                    int destField = viewDest[(int)VIEW_DEST.FIELD];
                    int destRoomX = viewDest[(int)VIEW_DEST.X];
                    int destRoomY = viewDest[(int)VIEW_DEST.Y];*/


                    /*
                     * 
                    fieldEntities = Global.World.GetField(Global.World.CurrField).GetFieldEntities();
                    roomEntities = Global.World.GetField(Global.World.CurrField).GetRoomEntities();
                    entityCount = Global.EntityManager.GetCount();
                    Global.TextManager.DrawText(0, 0, String.Format("RoomEntities: {0}    Static: {1}\\10FieldEntities:{2}   Total: {3}", roomEntities.Count, 4, fieldEntities.Count, entityCount));
                    */

                    //Global.TextManager.DrawOwnString();

                    /*
                    Globals.TextManager.DrawText(0, 0, "Player State: " + _protag.PrintState()
                        + "\nWASD, JK: Move between rooms\nF2: Open MSX [Jukebox]");*/

                    /*"\\10bboxLeft: " + Math.Floor(_protag.CollisionBox.Left / (float)World.tileHeight).ToString()
                    + "\\10bboxBottom: " + Math.Floor(_protag.CollisionBox.Bottom / (float)World.tileHeight).ToString()
                + "\\10bboxRight: " + Math.Floor(_protag.CollisionBox.Right / (float)World.tileHeight).ToString()
                + "\\10bboxTop: " + Math.Floor(_protag.CollisionBox.Top / (float)World.tileHeight).ToString());
                    (/
                    */
                    break;

            }

            Global.TextManager.Draw(Global.SpriteBatch, gameTime);
            Global.SpriteBatch.End();

            base.Draw(gameTime);
        }

        private void DrawHud(SpriteBatch spriteBatch, GameTime gameTime)
        {
            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)Global.Camera.Position.X, (int)Global.Camera.Position.Y, HUD_WIDTH, HUD_HEIGHT), new Color(0, 0, 0, 255));

            /*
             * 
            List<IGameEntity> fieldEntities, roomEntities;
            int entityCount;
            fieldEntities = Global.World.GetField(Global.World.CurrField).GetFieldEntities();//Global.World.GetActiveViews()[(int)World.AViews.DEST].GetView().GetParentField().GetFieldEntities();
            roomEntities = Global.World.GetField(Global.World.CurrField).GetRoomEntities();
            entityCount = Global.EntityManager.GetCount();
            Global.TextManager.DrawText(Global.Camera.Position, String.Format("RoomEntities: {0}    Static: {1}\\10FieldEntities:{2}   Total: {3}", roomEntities.Count, 4, fieldEntities.Count, entityCount));
            
             */
            Vector2 _camPos = Global.Camera.Position;

            _hudSprites[(int)HudSprites.COINS].Draw(spriteBatch, _camPos + new Vector2(27 * 8, 0 * 8));
            _hudSprites[(int)HudSprites.WEIGHTS].Draw(spriteBatch, _camPos + new Vector2(27 * 8, 1 * 8));
            _hudSprites[(int)HudSprites.WEAPONSBRACKETLEFT].Draw(spriteBatch, _camPos + new Vector2(15 * 8, 0 * 8));
            _hudSprites[(int)HudSprites.WEAPONSBRACKETMIDDLE].Draw(spriteBatch, _camPos + new Vector2(18 * 8, 0 * 8));
            _hudSprites[(int)HudSprites.WEAPONSBRACKETRIGHT].Draw(spriteBatch, _camPos + new Vector2(24 * 8, 0 * 8));


            _hudSprites[(int)HudSprites.SIGILS_BLANK].Draw(spriteBatch, _camPos + new Vector2(25 * 8, 0 * 8));
            _hudSprites[(int)HudSprites.SIGILS_ORIGIN].Draw(spriteBatch, _camPos + new Vector2(25 * 8, 0 * 8));

            _hudSprites[(int)HudSprites.SIGILS_BLANK].Draw(spriteBatch, _camPos + new Vector2(26 * 8, 0 * 8));
            _hudSprites[(int)HudSprites.SIGILS_BIRTH].Draw(spriteBatch, _camPos + new Vector2(26 * 8, 0 * 8));

            _hudSprites[(int)HudSprites.SIGILS_BLANK].Draw(spriteBatch, _camPos + new Vector2(25 * 8, 1 * 8));
            _hudSprites[(int)HudSprites.SIGILS_LIFE].Draw(spriteBatch, _camPos + new Vector2(25 * 8, 1 * 8));

            _hudSprites[(int)HudSprites.SIGILS_BLANK].Draw(spriteBatch, _camPos + new Vector2(26 * 8, 1 * 8));
            _hudSprites[(int)HudSprites.SIGILS_DEATH].Draw(spriteBatch, _camPos + new Vector2(26 * 8, 1 * 8));

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


            // Calculate and draw the blue HP bar
            int trueHPMax = Global.Protag.GetInventory().TrueHPMax;
            int maxHP = Global.Protag.GetInventory().HPMax;
            int currHP = Global.Protag.GetInventory().HP;
            float currHPRatio = currHP / 352.0f;
            float healthColorRatio = currHP / (float)maxHP;
            Color HPColor = new Color(51, 102, 255, 255); // Health color is blue by default
            if (healthColorRatio < 0.25f)
                HPColor = new Color(255, 51, 51, 255); // Change health color to red
            else if (healthColorRatio < 0.5f)
                HPColor = new Color(204, 204, 51, 255); // Change health color to yellow

            int currHPPixels = (int)Math.Round(currHPRatio * 88);
            if (currHPPixels < 1)
                currHPPixels = 1;
            if (currHP > 0)
                HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 24, (int)_camPos.Y + 1, currHPPixels, 5), HPColor);

            // Draw the white line representing max HP
            float maxHPRatio = maxHP / (float)trueHPMax;
            int hpPixels = Math.Clamp((int)Math.Round(maxHPRatio * 88), 1, 88);
            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 24 + hpPixels, (int)_camPos.Y + 0, 1, 7), new Color(255, 255, 255, 255));

            // Draw the white line representing max EXP
            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 24 + hpPixels, (int)_camPos.Y + 8, 1, 7), new Color(255, 255, 255, 255));


            int currExp = Global.Protag.GetInventory().CurrExp;
            float currEXPRatio = currExp / (float)trueHPMax; // Remake behavior
            int expPixels = (int)Math.Round(currEXPRatio * 88);
            if (expPixels < 1)
                expPixels = 1;
            if (currExp > 0)
                HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 24, (int)_camPos.Y + 9, expPixels, 5), new Color(51, 204, 51, 255));

            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 23, (int)_camPos.Y + 0, 1, 7), new Color(255, 255, 255, 255));
            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 23, (int)_camPos.Y + 8, 1, 7), new Color(255, 255, 255, 255));
            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 112, (int)_camPos.Y + 0, 1, 7), new Color(255, 255, 255, 255));
            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle((int)_camPos.X + 112, (int)_camPos.Y + 8, 1, 7), new Color(255, 255, 255, 255));


            Global.TextManager.DrawText(_camPos + new Vector2(21 * 8, 1 * 8), "---");
            string fmt = "000";
            int numCoins = Global.Protag.GetInventory().Coins;
            int numWeights = Global.Protag.GetInventory().Weights;
            string strWeights = numCoins.ToString(fmt);
            string strCoins = numWeights.ToString(fmt);

            Global.TextManager.DrawText(_camPos + new Vector2(28 * 8, 0 * 8), String.Format("={0}\\10={1}", strCoins, strWeights));
        }

        private void JukeboxRoutine()
        {

        }

        private bool StartGame()
        {
            if (State != Global.GameState.INITIAL)
                return false;

            Global.World.InitWorldEntities();

            Global.AudioManager.ChangeSongs(0);

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

        }

        public void LoadSaveState()
        {
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
            }
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

        public int GetDisplayZoomMax()
        {
            return _displayZoomMax;
        }

        private void ToggleDisplayMode()
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