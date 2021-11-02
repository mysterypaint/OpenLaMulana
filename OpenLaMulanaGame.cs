using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Diagnostics;
using OpenLaMulana.Entities;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using OpenLaMulana.Extensions;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Collections.Generic;

namespace OpenLaMulana
{

    public class OpenLaMulanaGame : Game
    {
        public enum DisplayMode
        {
            Default,
            Zoomed
        }

        public const string GAME_TITLE = "La.MuLANA";

        private const string ASSET_NAME_SPRITESHEET = "TrexSpritesheet";
        private const string ASSET_NAME_SFX_HIT = "hit";
        private const string ASSET_NAME_SFX_SCORE_REACHED = "score-reached";
        private const string ASSET_NAME_SFX_BUTTON_PRESS = "button-press";

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
        
        public int DISPLAY_ZOOM_FACTOR = 3;
        private int DISPLAY_ZOOM_MAX = 10;

        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;

        //private SoundEffect _sfxHit;
        //private SoundEffect _sfxButtonPress;
        //private SoundEffect _sfxScoreReached;

        private SoundEffect _sfxPause;
        private SoundEffectInstance _sfxPauseInstance;
        private SoundEffect _sfxJump;

        private Texture2D _spriteSheetTexture;
        private Texture2D _fadeInTexture;
        private Texture2D _invertedSpriteSheet;
        private Texture2D _txProt1;
        private Texture2D _gameFontTex;

        private float _fadeInTexturePosX;

        private Protag _protag;
        //private ScoreBoard _scoreBoard;

        private InputController _inputController;
        private World _world;

        //private TileManager _tileManager;
        //private ObstacleManager _obstacleManager;
        //private SkyManager _skyManager;
        private GameOverScreen _gameOverScreen;

        private EntityManager _entityManager;
        private SongManager _songManager;
        private TextManager _textManager;
        private GameMenu _gameMenu;

        private KeyboardState _previousKeyboardState;

        //private DateTime _highscoreDate;

        private Matrix _transformMatrix = Matrix.Identity;
        private int pauseToggleTimer = 0;

        public GameState State { get; private set; }

        public DisplayMode WindowDisplayMode { get; set; } = DisplayMode.Default;

        public float ZoomFactor => WindowDisplayMode == DisplayMode.Default ? 1 : DISPLAY_ZOOM_FACTOR;

        public OpenLaMulanaGame()
        {
            _graphics = new GraphicsDeviceManager(this);
            //_graphics.GraphicsProfile = GraphicsProfile.HiDef;
            Content.RootDirectory = "Content";
            
            // 30fps game
            this.IsFixedTimeStep = true;//false;
            this.TargetElapsedTime = TimeSpan.FromSeconds(1d / 30d); //60);

            this.IsMouseVisible = true;
            //this.Window.AllowUserResizing = true;

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
        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            //_sfxButtonPress = Content.Load<SoundEffect>(ASSET_NAME_SFX_BUTTON_PRESS);
            //_sfxHit = Content.Load<SoundEffect>(ASSET_NAME_SFX_HIT);
            //_sfxScoreReached = Content.Load<SoundEffect>(ASSET_NAME_SFX_SCORE_REACHED);
            
            _spriteSheetTexture = Content.Load<Texture2D>(ASSET_NAME_SPRITESHEET);

            _sfxPause = Content.Load<SoundEffect>("sound/se00");
            _sfxPauseInstance = _sfxPause.CreateInstance();
            _sfxJump = Content.Load<SoundEffect>("sound/se01");

            _txProt1 = Content.Load<Texture2D>("graphics/prot1");

            _invertedSpriteSheet = _spriteSheetTexture.InvertColors(Color.Transparent);

            //_fadeInTexture = new Texture2D(GraphicsDevice, 1, 1);
            //_fadeInTexture.SetData(new Color[] { Color.White });

            _protag = new Protag(_txProt1, new Vector2(TREX_START_POS_X, TREX_START_POS_Y - Protag.DEFAULT_SPRITE_HEIGHT), _sfxJump);
            _protag.DrawOrder = 100;
            _protag.JumpComplete += trex_JumpComplete;
            _protag.Died += trex_Died;

            //_scoreBoard = new ScoreBoard(_spriteSheetTexture, new Vector2(SCORE_BOARD_POS_X, SCORE_BOARD_POS_Y), _protag, _sfxScoreReached);
            //_scoreBoard.Score = 498;
            //_scoreBoard.HighScore = 12345;

            _world = new World(_entityManager);
            _inputController = new InputController(_protag, _world);


            //_gameFontTex = Content.Load<Texture2D>("graphics/font_JP");
            _gameFontTex = Content.Load<Texture2D>("graphics/font_EN");

            
            for (int i = 0; i <= 22; i++)
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
                tempTexList.Add(Content.Load<Texture2D>("graphics/mapg" + numStr));
            }

            _world.SetTexturesList(tempTexList);


            /*
            _songManager = new SongManager();
            string path = "music/m00.it";// "C:/Users/User/Desktop/ConvertedOST/IT/m00.it";//Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location) + "/music/m00";

            Debug.WriteLine(path);
            Console.WriteLine(path);

            _songManager.InitPlayer();
            _songManager.LoadSong("test.mp3");
            //Debug.WriteLine(_songManager.PrintSongName());
            */


            //_tileManager = new TileManager(_spriteSheetTexture, _entityManager, _protag);
            //_obstacleManager = new ObstacleManager(_entityManager, _protag, _scoreBoard, _spriteSheetTexture);
            //_skyManager = new SkyManager(_protag, _spriteSheetTexture, _invertedSpriteSheet, _entityManager, _scoreBoard);

            _gameOverScreen = new GameOverScreen(_spriteSheetTexture, this);
            _gameOverScreen.Position = new Vector2(WINDOW_WIDTH / 2 - GameOverScreen.GAME_OVER_SPRITE_WIDTH / 2, WINDOW_HEIGHT / 2 - 30);


            _entityManager.AddEntity(_protag);
            _entityManager.AddEntity(_world);

            _textManager = new TextManager(_gameFontTex);
            _gameMenu = new GameMenu(ScreenOverlayState.INVISIBLE, _textManager); 
            _entityManager.AddEntity(_textManager);
            _entityManager.AddEntity(_gameMenu);

            //_entityManager.AddEntity(_tileManager);
            //_entityManager.AddEntity(_scoreBoard);
            //_entityManager.AddEntity(_obstacleManager);
            _entityManager.AddEntity(_gameOverScreen);
            //_entityManager.AddEntity(_skyManager);

            //_tileManager.Initialize();

            LoadSaveState();

        }

        private void trex_Died(object sender, EventArgs e)
        {
            State = GameState.GameOver;
            //_obstacleManager.IsEnabled = false;
            _gameOverScreen.IsEnabled = true;

            //_sfxHit.Play();

            Debug.WriteLine("Game Over: " +  DateTime.Now);

            /*
            if(_scoreBoard.DisplayScore > _scoreBoard.HighScore)
            {
                Debug.WriteLine("New highscore set: " + _scoreBoard.DisplayScore);
                _scoreBoard.HighScore = _scoreBoard.DisplayScore;
                _highscoreDate = DateTime.Now;

                SaveGame();

            }
            */

        }

        private void trex_JumpComplete(object sender, EventArgs e)
        {
            
            if(State == GameState.Transition)
            {
                State = GameState.Playing;
                _protag.Initialize();

                //_obstacleManager.IsEnabled = true;
            }

        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            // TODO: Add your update logic here

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
                    break;
                case GameState.Paused:
                    if (!isAltKeyDown && isStartKeyPressed && !wasStartKeyPressed)
                    {
                        ToggleGamePause();
                    }
                    break;
                case GameState.Transition:
                    //_fadeInTexturePosX += (float)gameTime.ElapsedGameTime.TotalSeconds * FADE_IN_ANIMATION_SPEED;
                    break;
                case GameState.Initial:

                    if (isStartKeyPressed && !wasStartKeyPressed)
                    {
                        StartGame();

                    }
                    break;
            }

            DecrementCounters();

            _entityManager.Update(gameTime);

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
            }
            else if (State == GameState.Paused)
            {
                State = GameState.Playing;
                pauseToggleTimer = 30;
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


            _spriteBatch.Begin(samplerState: SamplerState.PointClamp, transformMatrix: _transformMatrix);

            _entityManager.Draw(_spriteBatch, gameTime);

            if(State == GameState.Initial || State == GameState.Transition)
            {

                //_spriteBatch.Draw(_fadeInTexture, new Rectangle((int)Math.Round(_fadeInTexturePosX), 0, WINDOW_WIDTH, WINDOW_HEIGHT), Color.White);

            }

            _spriteBatch.End();

            base.Draw(gameTime);
        }

        private bool StartGame()
        {
            if (State != GameState.Initial)
                return false;

            //_scoreBoard.Score = 0;
            State = GameState.Transition;
            _protag.BeginJump();

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

            _gameOverScreen.IsEnabled = false;
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
