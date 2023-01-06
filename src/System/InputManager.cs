using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenLaMulana.Audio;
using OpenLaMulana.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using static OpenLaMulana.Entities.World;
using static OpenLaMulana.Global;
using static OpenLaMulana.System.Camera;

namespace OpenLaMulana.System
{
    public class InputManager
    {
        private static Protag _protag;
        private static Jukebox _jukebox;

        static KeyboardState t_keyboardState;
        static KeyboardState t_previousKeyboardState;
        static GamePadState t_gamePadState;
        static GamePadState t_previousGamePadState;
        static bool _isBlocked = false;

        // The actual key configurations
        public static int[,] ConfigKeys = new int[(int)ControllerTypes.Max, (int)ControllerKeys.MAX];

        // Checks if the key in question was pressed or not
        private static bool[,] _allPressedKeys = new bool[(int)ControllerTypes.Max, (int)ControllerKeys.MAX];
        private static bool[,] _allHeldKeys = new bool[(int)ControllerTypes.Max, (int)ControllerKeys.MAX];
        private static bool[,] _allReleasedKeys = new bool[(int)ControllerTypes.Max, (int)ControllerKeys.MAX];
        public static bool[,] _joyInputs { get; private set; } = new bool[(int)ControllerTypes.Max, 4]; // 4 = Up/Down/Left/Right
        public static bool[] PressedKeys = new bool[(int)ControllerKeys.MAX];
        public static bool[] HeldKeys = new bool[(int)ControllerKeys.MAX];
        public static bool[] ReleasedKeys = new bool[(int)ControllerKeys.MAX];

        public static short DirMoveX, DirMoveY = 0;


        public void Init()
        {
            _protag = Global.Protag;
            _jukebox = Global.Jukebox;

            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.LEFT] = (int)Keys.Left;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.RIGHT] = (int)Keys.Right;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.UP] = (int)Keys.Up;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.DOWN] = (int)Keys.Down;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.PAUSE] = (int)Keys.F1;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.WHIP] = (int)Keys.Z;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.JUMP] = (int)Keys.Up;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.SUBWEAPON] = (int)Keys.X;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.ITEM] = (int)Keys.C;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.PAUSE] = (int)Keys.F1;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_INVENTORY] = (int)Keys.F2;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_MSX_ROM_SELECTION] = (int)Keys.F3;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_MSX_EMULATOR] = (int)Keys.F4;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_CONFIG] = (int)Keys.F5;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_CONFIRM] = (int)Keys.Z;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_CANCEL] = (int)Keys.X;

            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.LEFT] = (int)Buttons.DPadLeft;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.RIGHT] = (int)Buttons.DPadRight;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.UP] = (int)Buttons.DPadUp;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.DOWN] = (int)Buttons.DPadDown;

            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.PAUSE] = (int)Buttons.Back;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_OPEN_INVENTORY] = (int)Buttons.Start;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_CONFIRM] = (int)Buttons.B;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_CANCEL] = (int)Buttons.A;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.ITEM] = (int)Buttons.Y;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.WHIP] = (int)Buttons.X;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.SUBWEAPON] = (int)Buttons.B;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.JUMP] = (int)Buttons.A;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.SUB_SHIFT_RIGHT] = (int)Buttons.RightShoulder;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.SUB_SHIFT_LEFT] = (int)Buttons.LeftShoulder;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MAIN_WEAPON_SHIFT_LEFT] = (int)Buttons.LeftTrigger;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT] = (int)Buttons.RightTrigger;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_MOVE_LEFT] = (int)Buttons.LeftTrigger;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_MOVE_RIGHT] = (int)Buttons.RightTrigger;
        }

        public void Update(GameTime gameTime)
        {
            GetKeyboardState();
            GetGamePadState();
            UpdateAllButtonChecks();

            DirMoveX = (short)(Convert.ToInt16(HeldKeys[(int)ControllerKeys.RIGHT]) - Convert.ToInt16(HeldKeys[(int)ControllerKeys.LEFT]));
            DirMoveY = (short)(Convert.ToInt16(HeldKeys[(int)ControllerKeys.DOWN]) - Convert.ToInt16(HeldKeys[(int)ControllerKeys.UP]));

            WorldTransitionTesting();
            //JukeboxControls();
        }

        private void UpdateAllButtonChecks()
        {
            for (ControllerKeys cK = (ControllerKeys)0; cK < ControllerKeys.MAX; cK++)
            {
                KeyboardCheckPressed(cK);
                KeyboardCheckReleased(cK);
                KeyboardDown(cK);
                GamePadCheckPressed(cK);
                GamePadCheckReleased(cK);
                GamePadDown(cK);

                // Check if ANYTHING in our input arrays are true, then return true if we found anything (otherwise, return false)
                PressedKeys[(int)cK] = CheckAllInputControllers(_allPressedKeys, cK);
                HeldKeys[(int)cK] = CheckAllInputControllers(_allHeldKeys, cK);
                ReleasedKeys[(int)cK] = CheckAllInputControllers(_allReleasedKeys, cK);
            }
        }

        private bool CheckAllInputControllers(bool[,] allKeysOnController, ControllerKeys cK)
        {
            switch (cK)
            {
                default:
                    for (ControllerTypes type = (ControllerTypes)0; type < ControllerTypes.Max; type++)
                    {
                        if (allKeysOnController[(int)type, (int)cK] == true)
                            return true;
                    }
                    break;
                case ControllerKeys.UP:
                case ControllerKeys.DOWN:
                case ControllerKeys.LEFT:
                case ControllerKeys.RIGHT:
                    for (ControllerTypes type = (ControllerTypes)0; type < ControllerTypes.Max; type++)
                    {
                        if (allKeysOnController[(int)type, (int)cK] || _joyInputs[(int)type, (int)(cK)])
                            return true;
                    }
                    break;
                case ControllerKeys.JOY_UP:
                case ControllerKeys.JOY_DOWN:
                case ControllerKeys.JOY_LEFT:
                case ControllerKeys.JOY_RIGHT:
                    for (ControllerTypes type = (ControllerTypes)0; type < ControllerTypes.Max; type++)
                    {
                        if (allKeysOnController[(int)type, (int)cK] || _joyInputs[(int)type, (int)(cK - ControllerKeys.JOY_UP)])
                            return true;
                    }
                    break;
            }
            return false;
        }

        private void JukeboxControls()
        {
            _jukebox.Control(DirMoveX, PressedKeys[(int)ControllerKeys.MENU_CANCEL], PressedKeys[(int)ControllerKeys.MENU_CONFIRM]);
        }

        public static void BlockInputTemporarily()
        {
            _isBlocked = true;
        }

        private void WorldTransitionTesting()
        {
            int camMoveX = Convert.ToInt32(DirectKeyboardCheckPressed(Keys.D)) - Convert.ToInt32(DirectKeyboardCheckPressed(Keys.A));
            int camMoveY = Convert.ToInt32(DirectKeyboardCheckPressed(Keys.S)) - Convert.ToInt32(DirectKeyboardCheckPressed(Keys.W));

            if (camMoveX == 1)
            {
                Global.World.FieldTransitionCardinal(VIEW_DIR.RIGHT);
            }
            else if (camMoveX == -1)
            {
                Global.World.FieldTransitionCardinal(VIEW_DIR.LEFT);
            }
            if (camMoveY == 1)
            {
                Global.World.FieldTransitionCardinal(VIEW_DIR.DOWN);
            }
            else if (camMoveY == -1)
            {
                Global.World.FieldTransitionCardinal(VIEW_DIR.UP);
            }

            if (DirectKeyboardCheckPressed(Keys.K))
            {
                Field currField = Global.World.GetCurrField();
                var newVal = Global.World.CurrField + 1;
                if (newVal > Global.World.FieldCount - 1)
                    newVal = 0;

                int mX = Global.World.CurrViewX;
                int mY = Global.World.CurrViewY;
                Field destField = Global.World.GetField(newVal);
                Global.World.FieldTransitionImmediate(currField.GetView(mX, mY), destField.GetView(mX, mY));
            }
            else if (DirectKeyboardCheckPressed(Keys.J))
            {
                Field currField = Global.World.GetCurrField();
                var newVal = Global.World.CurrField - 1;
                if (newVal < 0)
                    newVal = Global.World.FieldCount - 1;

                int mX = Global.World.CurrViewX;
                int mY = Global.World.CurrViewY;
                Field destField = Global.World.GetField(newVal);
                Global.World.FieldTransitionImmediate(currField.GetView(mX, mY), destField.GetView(mX, mY));
            }
            else if (DirectKeyboardCheckPressed(Keys.T))
            {
                //Global.Camera.SetState((int)CamStates.TRANSITION_PIXELATE_1);
                Camera.CamStates camState = Global.Camera.GetState();
                if (camState == CamStates.NONE) {
                    Global.AudioManager.ChangeSongs(22);
                    //Global.AudioManager.ChangeSongs(25);
                    Global.World.FieldTransitionPixelate(0, 3, 0, 0);

                    //Global.World.FieldTransitionPixelate(0, -1, 0, 0);
                    //Global.World.FieldTransitionPixelate(1, 9, 3, 2);
                }
            }
        }

        public static KeyboardState GetKeyboardState()
        {
            t_previousKeyboardState = t_keyboardState;
            t_keyboardState = Keyboard.GetState();
            return t_keyboardState;
        }

        public static GamePadState GetGamePadState()
        {
            t_previousGamePadState = t_gamePadState;

            // Check the device for Player One
            GamePadCapabilities capabilities = GamePad.GetCapabilities(PlayerIndex.One);

            // If there a controller attached, handle it
            if (capabilities.IsConnected)
            {
                if (capabilities.GamePadType == GamePadType.GamePad) //  && capabilities.HasLeftXThumbStick
                    t_gamePadState = GamePad.GetState(PlayerIndex.One); // Get the current state of Controller1
            }
            return t_gamePadState;
        }

        public static bool DirectKeyboardCheckPressed(Keys checkingKey)
        {
            return t_keyboardState.IsKeyDown(checkingKey) && !t_previousKeyboardState.IsKeyDown(checkingKey);
        }

        public static bool DirectKeyboardCheckDown(Keys checkingKey)
        {
            return t_keyboardState.IsKeyDown((Keys)checkingKey);
        }

        private static void KeyboardCheckPressed(ControllerKeys key)
        {
            if (_isBlocked)
                _allPressedKeys[(int)ControllerTypes.Keyboard, (int)key] = false;
            Keys checkingKey = (Keys)ConfigKeys[(int)ControllerTypes.Keyboard, (int)key];
            _allPressedKeys[(int)ControllerTypes.Keyboard, (int)key] = t_keyboardState.IsKeyDown((Keys)checkingKey) && !t_previousKeyboardState.IsKeyDown((Keys)checkingKey);
        }

        private static void KeyboardCheckReleased(ControllerKeys key)
        {
            if (_isBlocked)
                _allReleasedKeys[(int)ControllerTypes.Keyboard, (int)key] = false;
            Keys checkingKey = (Keys)ConfigKeys[(int)ControllerTypes.Keyboard, (int)key];
            _allReleasedKeys[(int)ControllerTypes.Keyboard, (int)key] = t_previousKeyboardState.IsKeyDown((Keys)checkingKey);
        }

        private static void KeyboardDown(ControllerKeys key)
        {
            if (_isBlocked)
                _allHeldKeys[(int)ControllerTypes.Keyboard, (int)key] = false;

            Keys checkingKey = (Keys)ConfigKeys[(int)ControllerTypes.Keyboard, (int)key];

            _allHeldKeys[(int)ControllerTypes.Keyboard, (int)key] = t_keyboardState.IsKeyDown((Keys)checkingKey);
        }

        private static void GamePadCheckPressed(ControllerKeys key)
        {
            if (_isBlocked) {
                _allPressedKeys[(int)ControllerTypes.Gamepad, (int)key] = false;
                return;
            }

            Buttons checkingKey = (Buttons)ConfigKeys[(int)ControllerTypes.Gamepad, (int)key];

            switch (key)
            {
                default:
                    _allPressedKeys[(int)ControllerTypes.Gamepad, (int)key] = t_gamePadState.IsButtonDown(checkingKey) && !t_previousGamePadState.IsButtonDown(checkingKey);
                    break;
                case ControllerKeys.JOY_LEFT:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_gamePadState.ThumbSticks.Left.X < -0.15f) && !(t_previousGamePadState.ThumbSticks.Left.X < -0.15f);
                    break;
                case ControllerKeys.JOY_RIGHT:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_gamePadState.ThumbSticks.Left.X > 0.15f) && !(t_previousGamePadState.ThumbSticks.Left.X > 0.15f);
                    break;
                case ControllerKeys.JOY_DOWN:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_gamePadState.ThumbSticks.Left.Y < -0.15f) && !(t_previousGamePadState.ThumbSticks.Left.Y < -0.15f);
                    break;
                case ControllerKeys.JOY_UP:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_gamePadState.ThumbSticks.Left.Y > 0.15f) && !(t_previousGamePadState.ThumbSticks.Left.Y > 0.15f);
                    break;
            }
        }

        private static void GamePadCheckReleased(ControllerKeys key)
        {
            if (_isBlocked)
            {
                _allReleasedKeys[(int)ControllerTypes.Gamepad, (int)key] = false;
                return;
            }
            Buttons checkingKey = (Buttons)ConfigKeys[(int)ControllerTypes.Gamepad, (int)key];

            switch (key)
            {
                default:
                    _allReleasedKeys[(int)ControllerTypes.Gamepad, (int)key] = t_previousGamePadState.IsButtonDown((Buttons)checkingKey);
                    break;
                case ControllerKeys.JOY_LEFT:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_previousGamePadState.ThumbSticks.Left.X < -0.15f);
                    break;
                case ControllerKeys.JOY_RIGHT:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_previousGamePadState.ThumbSticks.Left.X > 0.15f);
                    break;
                case ControllerKeys.JOY_DOWN:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_previousGamePadState.ThumbSticks.Left.Y < -0.15f);
                    break;
                case ControllerKeys.JOY_UP:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_previousGamePadState.ThumbSticks.Left.Y > 0.15f);
                    break;
            }
        }

        private static void GamePadDown(ControllerKeys key)
        {
            if (_isBlocked)
            {
                _allHeldKeys[(int)ControllerTypes.Gamepad, (int)key] = false;
                return;
            }
            Buttons checkingKey = (Buttons)ConfigKeys[(int)ControllerTypes.Gamepad, (int)key];
            switch (key)
            {
                default:
                    _allHeldKeys[(int)ControllerTypes.Gamepad, (int)key] = t_gamePadState.IsButtonDown((Buttons)checkingKey);
                    break;
                case ControllerKeys.JOY_LEFT:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_gamePadState.ThumbSticks.Left.X < -0.15f);
                    break;
                case ControllerKeys.JOY_RIGHT:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_gamePadState.ThumbSticks.Left.X > 0.15f);
                    break;
                case ControllerKeys.JOY_DOWN:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_gamePadState.ThumbSticks.Left.Y < -0.15f);
                    break;
                case ControllerKeys.JOY_UP:
                    _joyInputs[(int)ControllerTypes.Gamepad, (int)(key - ControllerKeys.JOY_UP)] = (t_gamePadState.ThumbSticks.Left.Y > 0.15f);
                    break;
            }
        }

        internal bool GetPressedKeyState(ControllerKeys key)
        {
            if (_isBlocked)
            {
                return false;
            }
            return PressedKeys[(int)key];
        }
    }
}
