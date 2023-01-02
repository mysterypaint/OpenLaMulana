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

        private const string SAVE_FILE_NAME = "Save.dat";
        private const string musPath = "Content/music/";

        private int _displayZoomFactor = 3;
        private int _displayZoomMax = 10;
        private int _pauseToggleTimer = 0;
        private int _shaderMode = 0;
        private float _shdHueShiftTime = 0.0f;

        private Protag _protag;
        private Jukebox _jukebox;
        private KeyboardState _previousKeyboardState;
        private Effect activeShader = null;

        public Main()
        {
            Global.GraphicsDeviceManager = new GraphicsDeviceManager(this);
            Global.GraphicsDeviceManager.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";

            // 30fps game
            IsFixedTimeStep = true;//false;
            TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d); //60);

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

            Global.GraphicsDevice = GraphicsDevice;
            Global.SpriteBatch = new SpriteBatch(Global.GraphicsDevice);
            Global.ShdTransition = Content.Load<Effect>("shaders/shdTransition");
            Global.ShdHueShift = Content.Load<Effect>("shaders/shdHueShift");

            _shaderMode = (int)Global.Shaders.NONE;

            Global.TextureManager = new TextureManager();
            Global.TextureManager.InitTextures();

            long val = Global.GameRNG.RollDice(9);

            Global.World = new World();
            _jukebox = new Jukebox();
            Global.Camera = new Camera();
            _protag = new Protag(new Vector2(0, 0));
            _protag.DrawOrder = 100;
            Global.Camera.SetProtag(_protag);

            Global.InputController = new InputController(_protag, _jukebox);

            Global.EntityManager.AddEntity(_protag);
            Global.EntityManager.AddEntity(Global.World);
            Global.EntityManager.AddEntity(Global.AudioManager);

            Global.TextManager = Global.World.GetTextManager();

            Global.GameMenu = new GameMenu(Global.ScreenOverlayState.INVISIBLE, Global.TextManager);
            Global.EntityManager.AddEntity(Global.TextManager);
            Global.EntityManager.AddEntity(Global.GameMenu);

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
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            Global.Camera.Update(gameTime);
            Global.AudioManager.Update(gameTime);
            base.Update(gameTime);

            KeyboardState keyboardState = Keyboard.GetState();
            bool isStartKeyPressed, wasStartKeyPressed, isAltKeyDown;

            isStartKeyPressed = keyboardState.IsKeyDown(Keys.Enter);
            wasStartKeyPressed = _previousKeyboardState.IsKeyDown(Keys.Enter);
            isAltKeyDown = (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt));
            if (isAltKeyDown && (isStartKeyPressed && !wasStartKeyPressed))
            {
                Global.GraphicsDeviceManager.ToggleFullScreen();
                Global.GraphicsDeviceManager.ApplyChanges();
            }

            switch (State)
            {
                case Global.GameState.PLAYING:
                    Global.InputController.ProcessControls(gameTime);

                    if (!isAltKeyDown && isStartKeyPressed && !wasStartKeyPressed)
                    {
                        ToggleGamePause();
                    }
                    break;
                case Global.GameState.PAUSED:
                    JukeboxRoutine();
                    Global.AudioManager.PauseMusic();
                    if (!isAltKeyDown && isStartKeyPressed && !wasStartKeyPressed)
                    {
                        ToggleGamePause();
                    }
                    break;
                case Global.GameState.TRANSITION:
                    State = Global.GameState.PLAYING;
                    _protag.Initialize();
                    break;
                case Global.GameState.MSX_INVENTORY:
                    break;
                case Global.GameState.MSX_EMULATOR:
                    JukeboxRoutine();
                    break;
                case Global.GameState.INITIAL:
                    if (isStartKeyPressed && !wasStartKeyPressed)
                    {
                        StartGame();
                    }
                    break;
            }

            DecrementCounters();

            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
                _protag.Position = new Vector2(mouseState.X / _displayZoomFactor, mouseState.Y / _displayZoomFactor);

            if (keyboardState.IsKeyDown(Keys.F8) && !_previousKeyboardState.IsKeyDown(Keys.F8))
            {
                //ResetSaveState();
                _shaderMode++;
                if (_shaderMode >= (int)Global.Shaders.MAX)
                    _shaderMode = 0;
            }

            if (keyboardState.IsKeyDown(Keys.F7) && !_previousKeyboardState.IsKeyDown(Keys.F7) && !Global.GraphicsDeviceManager.IsFullScreen)
            {
                _displayZoomMax = CalcDisplayZoomMax();
                _displayZoomFactor += 1;
                if (_displayZoomFactor > _displayZoomMax)
                    _displayZoomFactor = 1;
                ToggleDisplayMode();
            }

            _previousKeyboardState = keyboardState;

            Global.EntityManager.Update(gameTime);
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

        private void DecrementCounters()
        {
            if (_pauseToggleTimer > 0)
                _pauseToggleTimer--;
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


            //_shdHueShift.CurrentTechnique.Passes[0].Apply();
            Global.EntityManager.Draw(Global.SpriteBatch, gameTime, GraphicsDevice);

            Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
            samplerState: SamplerState.PointClamp,
            transformMatrix: Global.Camera.GetTransformation(GraphicsDevice),
            effect: activeShader);

            switch (State)
            {
                case Global.GameState.INITIAL:
                case Global.GameState.TRANSITION:
                    //_spriteBatch.Draw(_fadeInTexture, new Rectangle((int)Math.Round(_fadeInTexturePosX), 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                    //Globals.TextManager.DrawText(0, 0, "Press Enter to Begin\\10WASD to move camera\\10J/K to switch maps");
                    Global.TextManager.DrawText(0, 0, "Press Enter to Begin");
                    break;
                default:
                case Global.GameState.MSX_INVENTORY:
                    break;
                case Global.GameState.MSX_EMULATOR:
                    break;
                case Global.GameState.PAUSED:
                    //_jukebox.Draw(_spriteBatch, gameTime);
                    break;
                case Global.GameState.PLAYING:
                    //_jukebox.Draw(_spriteBatch, gameTime);

                    View[,] thisFieldMapData = Global.World.GetField(Global.World.CurrField).GetMapData();
                    View thisView = thisFieldMapData[Global.World.CurrViewX, Global.World.CurrViewY];

                    /*
                    int[] viewDest = thisView.GetDestinationView(movingDirection);

                    //currRoomX = viewDest[(int)VIEW_DEST.WORLD];
                    int destField = viewDest[(int)VIEW_DEST.FIELD];
                    int destRoomX = viewDest[(int)VIEW_DEST.X];
                    int destRoomY = viewDest[(int)VIEW_DEST.Y];*/

                    List<IGameEntity> fieldEntities = Global.World.GetField(Global.World.CurrField).GetFieldEntities();
                    List<IGameEntity> roomEntities = Global.World.GetField(Global.World.CurrField).GetRoomEntities();
                    Global.TextManager.DrawText(0, 0, String.Format("RoomEntities: {0}\\10FieldEntities:{1}", roomEntities.Count, fieldEntities.Count));
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

            Global.SpriteBatch.End();

            base.Draw(gameTime);
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
            Global.InputController.BlockInputTemporarily();

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

        private void ToggleDisplayMode()
        {
            Global.GraphicsDeviceManager.PreferredBackBufferHeight = WINDOW_HEIGHT * _displayZoomFactor;
            Global.GraphicsDeviceManager.PreferredBackBufferWidth = WINDOW_WIDTH * _displayZoomFactor;
            Global.Camera.UpdateWindowSize(WINDOW_WIDTH * _displayZoomFactor, WINDOW_HEIGHT * _displayZoomFactor, _displayZoomFactor);
            Global.GraphicsDeviceManager.ApplyChanges();
            Window.Position = new Point((GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Width / 2) - (Global.GraphicsDeviceManager.PreferredBackBufferWidth / 2), (GraphicsAdapter.DefaultAdapter.CurrentDisplayMode.Height / 2) - (Global.GraphicsDeviceManager.PreferredBackBufferHeight / 2));
        }
    }
}