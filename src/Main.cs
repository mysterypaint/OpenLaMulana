using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using OpenLaMulana.Entities;
using OpenLaMulana.System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Collections.Generic;
using MeltySynth;
using OpenLaMulana.Audio;

namespace OpenLaMulana
{
    public class Main : Game
    {
        public enum DisplayMode
        {
            Default,
            Zoomed
        }

        public enum Languages
        {
            English,
            Japanese,
            Max
        };

        public const string GAME_TITLE = "La.MuLANA";

        public Languages currLang = Languages.English;

        public const int WINDOW_WIDTH = 256;
        public const int WINDOW_HEIGHT = 192;

        public const int TREX_START_POS_Y = 8*12;
        public const int TREX_START_POS_X = WINDOW_WIDTH/2;

        public const int HUD_WIDTH = 256;
        public const int HUD_HEIGHT = 16;

        //private const int SCORE_BOARD_POS_X = WINDOW_WIDTH - 130;
        //private const int SCORE_BOARD_POS_Y = 10;

        private const float FADE_IN_ANIMATION_SPEED = 820f;

        private const string SAVE_FILE_NAME = "Save.dat";
        private const string musPath = "Content/music/";
        private const string gfxPath = "Content/graphics/";

        public int DISPLAY_ZOOM_FACTOR = 3;
        private int DISPLAY_ZOOM_MAX = 10;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        private SoundEffect _sfxPause;
        private SoundEffectInstance _sfxPauseInstance;
        private SoundEffect _sfxJump;

        private Texture2D _txProt1;
        private Texture2D _gameFontTex;

        private float _fadeInTexturePosX;

        private Protag _protag;
        //private ScoreBoard _scoreBoard;

        private InputController _inputController;
        private Jukebox _jukebox;
        private World _world;

        //private TileManager _tileManager;
        //private ObstacleManager _obstacleManager;
        //private SkyManager _skyManager;

        private EntityManager _entityManager;
        private AudioManager _audioManager;
        private TextManager _textManager;
        private GameMenu _gameMenu;
        private SaveData _saveData;

        public static GameRNG gameRNG;

        private KeyboardState _previousKeyboardState;

        //private DateTime _highscoreDate;

        private Matrix _transformMatrix = Matrix.Identity;
        private int pauseToggleTimer = 0;


        public GameState State { get; private set; }

        public DisplayMode WindowDisplayMode { get; set; } = DisplayMode.Default;

        public float ZoomFactor => WindowDisplayMode == DisplayMode.Default ? 1 : DISPLAY_ZOOM_FACTOR;

        public Main()
        {
            _graphics = new GraphicsDeviceManager(this);
            //_graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";

            // 30fps game
            this.IsFixedTimeStep = true;//false;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d); //60);

            this.IsMouseVisible = true;
            //this.Window.AllowUserResizing = true;

            gameRNG = new GameRNG();
            _saveData = new SaveData();
            //_saveData.ReadEncryptedSave("lamulana.sa0");
            //_saveData.WriteDecryptedSave("lamulana_dec.sa0");
            //gameRNG.Advance();


            DateTime bootTime = DateTime.Now;
            long sRNG = new DateTimeOffset(bootTime).ToUnixTimeMilliseconds();

            //_saveData.WriteEncryptedSave("lamulana_enc.sa0", sRNG);

            _entityManager = new EntityManager();
            State = GameState.Initial;
            //_fadeInTexturePosX = Protag.DEFAULT_SPRITE_WIDTH;
        }

        protected override void Initialize()
        {
            // TODO: Add your initialization logic here

            base.Initialize();

            Window.Title = GAME_TITLE;

            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT;
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH;
            _graphics.SynchronizeWithVerticalRetrace = true;
            _graphics.ApplyChanges();

            DISPLAY_ZOOM_FACTOR = 3;
            if (DISPLAY_ZOOM_FACTOR > 4)
                DISPLAY_ZOOM_FACTOR = 1;
            ToggleDisplayMode();

        }

        List<Texture2D> tempTexList = new List<Texture2D>();
        private int textIndex;

        protected override void LoadContent()
        {
            _audioManager = new AudioManager();
            _audioManager.LoadContent(musPath);

            _spriteBatch = new SpriteBatch(GraphicsDevice);

            _sfxPause = Content.Load<SoundEffect>("sound/se00");
            _sfxPauseInstance = _sfxPause.CreateInstance();
            _sfxJump = Content.Load<SoundEffect>("sound/se01");

            _txProt1 = LoadTexture(gfxPath + "prot1.png");

            _gameFontTex = LoadTexture(gfxPath + "font_EN.png");

            long val = gameRNG.RollDice(9);

            _world = new World(_entityManager, _gameFontTex, _audioManager);
            _jukebox = new Jukebox(_audioManager, _world.GetTextManager(), currLang);
            _protag = new Protag(_txProt1, _world, new Vector2(0, 0), _sfxJump);
            _protag.DrawOrder = 100;

            _inputController = new InputController(_protag, _world, _jukebox);
            _protag.SetInputController(_inputController);

            for (int i = 0; i <= 32; i++)
            {
                string numStr;
                if (i <= 9)
                {
                    numStr = "0" + i.ToString();
                }
                else
                {
                    numStr = i.ToString();
                }

                if (i > 22 && i < 31)
                {
                    Texture2D dummy = new Texture2D(GraphicsDevice, 1, 1);
                    dummy.SetData(new Color[] { Color.White });
                    tempTexList.Add(dummy);
                } else
                {
                    var tex = LoadTexture(gfxPath + "mapg" + numStr + ".png");
                    tempTexList.Add(tex);
                }
            }

            _world.SetTexturesList(tempTexList);

            _entityManager.AddEntity(_protag);
            _entityManager.AddEntity(_world);
            _entityManager.AddEntity(_audioManager);

            _textManager = _world.GetTextManager();

            _gameMenu = new GameMenu(ScreenOverlayState.INVISIBLE, _textManager);
            _entityManager.AddEntity(_textManager);
            _entityManager.AddEntity(_gameMenu);

            LoadSaveState();

        }

        protected override void UnloadContent()
        {
            _audioManager.UnloadContent();
            //base.UnloadContent();
        }
        private Texture2D LoadTexture(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            Texture2D tex = Texture2D.FromStream(GraphicsDevice, fileStream);
            fileStream.Dispose();
            Color[] buffer = new Color[tex.Width * tex.Height];
            tex.GetData(buffer);
            for (int i = 0; i < buffer.Length; i++)
            {
                if (buffer[i].Equals(new Color(68, 68, 68)))
                    buffer[i] = Color.FromNonPremultiplied(255, 255, 255, 0);
            }
            tex.SetData(buffer);

            return tex;
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            _audioManager.Update(gameTime);
            base.Update(gameTime);

            KeyboardState keyboardState = Keyboard.GetState();
            bool isStartKeyPressed, wasStartKeyPressed, isAltKeyDown;


            isStartKeyPressed = keyboardState.IsKeyDown(Keys.Enter);
            wasStartKeyPressed = _previousKeyboardState.IsKeyDown(Keys.Enter);
            isAltKeyDown = (keyboardState.IsKeyDown(Keys.LeftAlt) || keyboardState.IsKeyDown(Keys.RightAlt));
            if (isAltKeyDown && (isStartKeyPressed && !wasStartKeyPressed))
            {
                _graphics.ToggleFullScreen();
                _graphics.ApplyChanges();
            }

            switch (State)
            {
                case GameState.Playing:
                    _inputController.ProcessControls(gameTime);

                    if (!isAltKeyDown && isStartKeyPressed && !wasStartKeyPressed)
                    {
                        ToggleGamePause();
                    }

                    if (gameTime.TotalGameTime.Ticks % 5 == 0)
                    {
                        textIndex++;
                        if (textIndex >= _textManager.GetTextCount(currLang))
                            textIndex = 0;
                    }

                    break;
                case GameState.Paused:
                    JukeboxRoutine();
                    _audioManager.PauseMusic();
                    if (!isAltKeyDown && isStartKeyPressed && !wasStartKeyPressed)
                    {
                        ToggleGamePause();
                    }
                    break;
                case GameState.Transition:
                    State = GameState.Playing;
                    _protag.Initialize();
                    break;
                case GameState.MSXInventory:
                    break;
                case GameState.MSXEmulator:
                    JukeboxRoutine();
                    break;
                case GameState.Initial:

                    if (isStartKeyPressed && !wasStartKeyPressed)
                    {
                        StartGame();

                    }
                    break;
            }

            DecrementCounters();


            MouseState mouseState = Mouse.GetState();

            if (mouseState.LeftButton == ButtonState.Pressed)
                _protag.Position = new Vector2(mouseState.X / DISPLAY_ZOOM_FACTOR, mouseState.Y / DISPLAY_ZOOM_FACTOR);

            if(keyboardState.IsKeyDown(Keys.F8) && !_previousKeyboardState.IsKeyDown(Keys.F8)) {

                ResetSaveState();

            }

            if (keyboardState.IsKeyDown(Keys.F7) && !_previousKeyboardState.IsKeyDown(Keys.F7) && !_graphics.IsFullScreen)
            {
                DISPLAY_ZOOM_FACTOR += 1;
                if (DISPLAY_ZOOM_FACTOR > DISPLAY_ZOOM_MAX)
                    DISPLAY_ZOOM_FACTOR = 1;
                ToggleDisplayMode();

            }

            _previousKeyboardState = keyboardState;


            _entityManager.Update(gameTime);


        }

        private void DecrementCounters()
        {
            if (pauseToggleTimer > 0)
                pauseToggleTimer--;
        }

        private void ToggleGamePause()
        {
            if (pauseToggleTimer > 0)
                return;
            _sfxPauseInstance.Stop();
            _sfxPauseInstance.Play();

            if (State == GameState.Playing)
            {
                pauseToggleTimer = 30;
                State = GameState.Paused;
                _audioManager.PauseMusic();
            }
            else if (State == GameState.Paused)
            {
                State = GameState.Playing;
                pauseToggleTimer = 30;
                _audioManager.ResumeMusic();
            }


        }

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Black);
            /*
            if (_skyManager == null)
                GraphicsDevice.Clear(Color.White);
            else
                GraphicsDevice.Clear(_skyManager.ClearColor);
            */


            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, samplerState: SamplerState.PointClamp, transformMatrix: _transformMatrix);

            _entityManager.Draw(_spriteBatch, gameTime);

            switch(State)
            {
                case GameState.Initial:
                case GameState.Transition:
                    //_spriteBatch.Draw(_fadeInTexture, new Rectangle((int)Math.Round(_fadeInTexturePosX), 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);
                    //_textManager.DrawText(0, 0, "Press Enter to Begin\\10WASD to move camera\\10J/K to switch maps");
                    _textManager.DrawText(0, 0, "Press Enter to Begin");
                    break;
                default:
                case GameState.MSXInventory:
                    break;
                case GameState.MSXEmulator:
                    break;
                case GameState.Paused:
                    _jukebox.Draw(_spriteBatch, gameTime);
                    break;
                case GameState.Playing:
                    _jukebox.Draw(_spriteBatch, gameTime);
                    /*
                    _textManager.DrawText(0, 0, "Player State: " + _protag.PrintState()
                        + "\nWASD, JK: Move between rooms\nF2: Open MSX [Jukebox]");*/

                    /*"\\10bboxLeft: " + Math.Floor(_protag.CollisionBox.Left / (float)World.tileHeight).ToString()
                    + "\\10bboxBottom: " + Math.Floor(_protag.CollisionBox.Bottom / (float)World.tileHeight).ToString()
                + "\\10bboxRight: " + Math.Floor(_protag.CollisionBox.Right / (float)World.tileHeight).ToString()
                + "\\10bboxTop: " + Math.Floor(_protag.CollisionBox.Top / (float)World.tileHeight).ToString());
                    (/
                    */
                    break;

            }


            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private void JukeboxRoutine()
        {

        }

        private bool StartGame()
        {
            if (State != GameState.Initial)
                return false;

            //_scoreBoard.Score = 0;
            State = GameState.Transition;
            //_protag.BeginJump();

            return true;
        }

        public bool Replay()
        {
            if (State != GameState.GameOver)
                return false;

            State = GameState.Playing;
            _protag.Initialize();

            //_obstacleManager.Reset();
            //_obstacleManager.IsEnabled = true;

            //_gameOverScreen.IsEnabled = false;
            //_scoreBoard.Score = 0;

            //_tileManager.Initialize();

            _inputController.BlockInputTemporarily();

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
            catch(Exception ex)
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

                    if(saveState != null)
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
            _graphics.PreferredBackBufferHeight = WINDOW_HEIGHT * DISPLAY_ZOOM_FACTOR;
            _graphics.PreferredBackBufferWidth = WINDOW_WIDTH * DISPLAY_ZOOM_FACTOR;
            _transformMatrix = Matrix.Identity * Matrix.CreateScale(DISPLAY_ZOOM_FACTOR, DISPLAY_ZOOM_FACTOR, 1); //_transformMatrix = Matrix.Identity;
            _graphics.ApplyChanges();

        }

    }
}
