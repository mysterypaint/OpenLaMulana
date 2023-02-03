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
        public enum ControllerNames : int
        {
            GENERIC,
            XBOX_360,
            XBOX_ONE,
            PS3,
            PS4,
            PS5,
            NINTENDO_SWITCH,
            MAX
        };

        private static Protag _protag;

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
        public static bool[] PressedKeysPrev = new bool[(int)ControllerKeys.MAX];
        public static bool[] HeldKeysPrev = new bool[(int)ControllerKeys.MAX];
        public static bool[] ReleasedKeysPrev = new bool[(int)ControllerKeys.MAX];

        public static short DirHeldX, DirHeldY, DirPressedX, DirPressedY, DirConfPressedX, DirConfPressedY,
            DirHeldXPrev, DirHeldYPrev, DirPressedXPrev, DirPressedYPrev, DirConfPressedXPrev, DirConfPressedYPrev = 0;
        private static bool _joysticksEnabled = true;
        private static ControllerNames? _identifiedController = null;
        private static ControllerNames? _previouslyIdentifiedController = null;
        private Keys[] _lastKeyStroke = null;
        private static Keys[] _lastEightKeyStrokes;
        private int _keystrokeCooldownTimer = 0;
        private int _keystrokeCooldownTimerReset = 8;

        public void Init()
        {
            _protag = Global.Protag;

            // The actual default controls
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.LEFT] = (int)Keys.Left;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.RIGHT] = (int)Keys.Right;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.UP] = (int)Keys.Up;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.DOWN] = (int)Keys.Down;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.PAUSE] = (int)Keys.F1;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MAIN_WEAPON] = (int)Keys.Space;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.JUMP] = (int)Keys.Up;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.SUB_WEAPON] = (int)Keys.M;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.ITEM] = (int)Keys.N;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.PAUSE] = (int)Keys.F1;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_INVENTORY] = (int)Keys.F2;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_MSX_SOFTWARE_SELECTION] = (int)Keys.F3;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_MSX_EMULATOR] = (int)Keys.F4;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_CONFIG] = (int)Keys.F5;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_CONFIRM] = (int)Keys.Z;
            ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_CANCEL] = (int)Keys.X;

            // For testing: Replace default controls
            if (Global.DevModeEnabled)
            {
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.LEFT] = (int)Keys.A;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.RIGHT] = (int)Keys.D;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.UP] = (int)Keys.W;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.DOWN] = (int)Keys.S;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.PAUSE] = (int)Keys.F1;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MAIN_WEAPON] = (int)Keys.J;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.JUMP] = (int)Keys.K;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.SUB_WEAPON] = (int)Keys.L;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.ITEM] = (int)Keys.I;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.PAUSE] = (int)Keys.F1;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_INVENTORY] = (int)Keys.F2;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_MSX_SOFTWARE_SELECTION] = (int)Keys.F3;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_MSX_EMULATOR] = (int)Keys.F4;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_OPEN_CONFIG] = (int)Keys.F5;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_CONFIRM] = (int)Keys.L;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.MENU_CANCEL] = (int)Keys.K;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.CONFIG_MENU_LEFT] = (int)Keys.A;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.CONFIG_MENU_RIGHT] = (int)Keys.D;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.CONFIG_MENU_UP] = (int)Keys.W;
                ConfigKeys[(int)ControllerTypes.Keyboard, (int)ControllerKeys.CONFIG_MENU_DOWN] = (int)Keys.S;
            }

            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.LEFT] = (int)Buttons.DPadLeft;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.RIGHT] = (int)Buttons.DPadRight;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.UP] = (int)Buttons.DPadUp;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.DOWN] = (int)Buttons.DPadDown;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.PAUSE] = (int)Buttons.Back;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_OPEN_INVENTORY] = (int)Buttons.Start;

            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.CONFIG_MENU_UP] = (int)Buttons.DPadUp;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.CONFIG_MENU_DOWN] = (int)Buttons.DPadDown;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.CONFIG_MENU_LEFT] = (int)Buttons.DPadLeft;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.CONFIG_MENU_RIGHT] = (int)Buttons.DPadRight;

            GetGamePadState();
            if (_identifiedController == ControllerNames.NINTENDO_SWITCH)
            {
                ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_CONFIRM] = (int)Buttons.B;
                ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_CANCEL] = (int)Buttons.A;
                ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.SUB_WEAPON] = (int)Buttons.A;
                ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.JUMP] = (int)Buttons.B;
            } else {
                ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_CONFIRM] = (int)Buttons.A;
                ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_CANCEL] = (int)Buttons.B;
                ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.SUB_WEAPON] = (int)Buttons.B;
                ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.JUMP] = (int)Buttons.A;
            }
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.ITEM] = (int)Buttons.Y;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MAIN_WEAPON] = (int)Buttons.X;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.SUB_SHIFT_RIGHT] = (int)Buttons.RightShoulder;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.SUB_SHIFT_LEFT] = (int)Buttons.LeftShoulder;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MAIN_WEAPON_SHIFT_LEFT] = (int)Buttons.LeftTrigger;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT] = (int)Buttons.RightTrigger;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_MOVE_LEFT] = (int)Buttons.LeftTrigger;
            ConfigKeys[(int)ControllerTypes.Gamepad, (int)ControllerKeys.MENU_MOVE_RIGHT] = (int)Buttons.RightTrigger;

            _lastEightKeyStrokes = new Keys[8];
        }

        public void Update(GameTime gameTime)
        {
            GetKeyboardState();
            _lastKeyStroke = t_keyboardState.GetPressedKeys();

            if (_keystrokeCooldownTimer <= 0)
            {
                if (_lastKeyStroke.Length > 0)
                {
                    for (var i = 7; i > 0; i--)
                    {
                        _lastEightKeyStrokes[i] = _lastEightKeyStrokes[i - 1];
                    }
                    _lastEightKeyStrokes[0] = _lastKeyStroke[0];
                    _keystrokeCooldownTimer = _keystrokeCooldownTimerReset;

                }
            }
            else
            {
                _keystrokeCooldownTimer--;
            }

            if (_identifiedController != null) {
                _previouslyIdentifiedController = _identifiedController;
            }
            GetGamePadState();
            UpdateAllButtonChecks();

            if (_identifiedController == ControllerNames.NINTENDO_SWITCH)
            {
                if (_identifiedController != _previouslyIdentifiedController)
                    SwapABButtons();
            }
            else if (_previouslyIdentifiedController == ControllerNames.NINTENDO_SWITCH)
            {
                SwapABButtons();
                _previouslyIdentifiedController = _identifiedController;
            }

            UpdateAllDirectionalInputs();

            if (Global.DevModeEnabled)
            {
                DebugControls();
            }
        }

        public bool CheckChantedMantra(string checkingString, bool ignoreKeystroke = false)
        {
            if (_lastKeyStroke.Length > 0 || ignoreKeystroke)
            {
                string chant = String.Empty;
                for (var i = 7; i >= 0; i--)
                {
                    if (_lastEightKeyStrokes[i] != Keys.None)
                        chant += _lastEightKeyStrokes[i].ToString();
                }

                int startPos = chant.Length - checkingString.Length;

                if (startPos >= 0)
                {
                    return chant.Substring(startPos, checkingString.Length).Equals(checkingString);
                }
            }
            return false;
        }

        private void UpdateAllDirectionalInputs()
        {
            DirHeldXPrev = DirHeldX;
            DirHeldYPrev = DirHeldY;

            DirPressedXPrev = DirPressedX;
            DirPressedYPrev = DirPressedY;

            DirConfPressedXPrev = DirConfPressedX;
            DirConfPressedYPrev = DirConfPressedY;

            DirHeldX = (short)(Convert.ToInt16(HeldKeys[(int)ControllerKeys.RIGHT]) - Convert.ToInt16(HeldKeys[(int)ControllerKeys.LEFT]));
            DirHeldY = (short)(Convert.ToInt16(HeldKeys[(int)ControllerKeys.DOWN]) - Convert.ToInt16(HeldKeys[(int)ControllerKeys.UP]));

            DirPressedX = (short)(Convert.ToInt16(PressedKeys[(int)ControllerKeys.RIGHT]) - Convert.ToInt16(PressedKeys[(int)ControllerKeys.LEFT]));
            DirPressedY = (short)(Convert.ToInt16(PressedKeys[(int)ControllerKeys.DOWN]) - Convert.ToInt16(PressedKeys[(int)ControllerKeys.UP]));

            DirConfPressedX = (short)(Convert.ToInt16(PressedKeys[(int)ControllerKeys.CONFIG_MENU_RIGHT]) - Convert.ToInt16(PressedKeys[(int)ControllerKeys.CONFIG_MENU_LEFT]));
            DirConfPressedY = (short)(Convert.ToInt16(PressedKeys[(int)ControllerKeys.CONFIG_MENU_DOWN]) - Convert.ToInt16(PressedKeys[(int)ControllerKeys.CONFIG_MENU_UP]));
        }

        private void SwapABButtons()
        {
            for (ControllerKeys k = (ControllerKeys)0; k < ControllerKeys.MAX; k++)
            {
                if (ConfigKeys[(int)ControllerTypes.Gamepad, (int)k] == (int)Buttons.A)
                    ConfigKeys[(int)ControllerTypes.Gamepad, (int)k] = (int)Buttons.B;
                else if (ConfigKeys[(int)ControllerTypes.Gamepad, (int)k] == (int)Buttons.B)
                    ConfigKeys[(int)ControllerTypes.Gamepad, (int)k] = (int)Buttons.A;
            }
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
                PressedKeysPrev[(int)cK] = PressedKeys[(int)cK];
                HeldKeysPrev[(int)cK] = HeldKeys[(int)cK];
                ReleasedKeysPrev[(int)cK] = ReleasedKeys[(int)cK];
                PressedKeys[(int)cK] = CheckAllInputControllers(_allPressedKeys, cK);
                HeldKeys[(int)cK] = CheckAllInputControllers(_allHeldKeys, cK);
                ReleasedKeys[(int)cK] = CheckAllInputControllers(_allReleasedKeys, cK);
            }
        }

        public static bool ButtonCheckPressed60FPS(ControllerKeys cK)
        {
            return PressedKeys[(int)cK];
        }

        public static bool ButtonCheckHeld60FPS(ControllerKeys cK)
        {
            return HeldKeys[(int)cK];
        }

        public static bool ButtonCheckReleased60FPS(ControllerKeys cK)
        {
            return ReleasedKeys[(int)cK];
        }

        public static bool ButtonCheckPressed30FPS(ControllerKeys cK)
        {
            return PressedKeys[(int)cK] || PressedKeysPrev[(int)cK];
        }

        public static bool ButtonCheckHeld30FPS(ControllerKeys cK)
        {
            return HeldKeys[(int)cK] || HeldKeysPrev[(int)cK];
        }

        public static bool ButtonCheckReleased30FPS(ControllerKeys cK)
        {
            return ReleasedKeys[(int)cK] || ReleasedKeysPrev[(int)cK];
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

        public static void BlockInputTemporarily()
        {
            _isBlocked = true;
        }

        private void DebugControls()
        {
            int camMoveX = Convert.ToInt32(DirectKeyboardCheckPressed(Keys.Right)) - Convert.ToInt32(DirectKeyboardCheckPressed(Keys.Left));
            int camMoveY = Convert.ToInt32(DirectKeyboardCheckPressed(Keys.Down)) - Convert.ToInt32(DirectKeyboardCheckPressed(Keys.Up));

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

            if (DirectKeyboardCheckPressed(Keys.E))
            {
/*
 *                 HelperFunctions.UpdateInventory(ItemTypes.TREASURE, (int)ObtainableTreasures.POCHETTE_KEY, true);
                Global.GameFlags.InGameFlags[(int)GameFlags.Flags.POCHETTE_KEY_TAKEN] = true;
                HelperFunctions.UpdateInventory(ItemTypes.TREASURE, (int)ObtainableTreasures.PEPPER, true);
                Global.GameFlags.InGameFlags[(int)GameFlags.Flags.PEPPER_TAKEN] = true;
                HelperFunctions.UpdateInventory(ItemTypes.TREASURE, (int)ObtainableTreasures.DETECTOR, true);
                Global.GameFlags.InGameFlags[(int)GameFlags.Flags.DETECTOR_TAKEN] = true;
                HelperFunctions.UpdateInventory(ItemTypes.TREASURE, (int)ObtainableTreasures.CRYSTAL_SKULL, true);
                Global.GameFlags.InGameFlags[(int)GameFlags.Flags.CRYSTAL_SKULL_TAKEN] = true;
                HelperFunctions.UpdateInventory(ItemTypes.TREASURE, (int)ObtainableTreasures.TWIN_STATUE, true);
                Global.GameFlags.InGameFlags[(int)GameFlags.Flags.TWIN_STATUE_TAKEN] = true;
                HelperFunctions.UpdateInventory(ItemTypes.TREASURE, (int)ObtainableTreasures.MSX2, true);
                Global.GameFlags.InGameFlags[(int)GameFlags.Flags.MSX2_TAKEN] = true;


                Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.GRADIUS] = true;
                //Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.GLYPH_READER] = true;
                Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.HYPER_RALLY] = true;
                Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.KONAMI_TENNIS] = true;
                Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.GAME_MASTER] = true;
                Global.Inventory.ObtainedSoftware[Global.ObtainableSoftware.A1_SPIRIT] = true;

                Global.AudioManager.PlaySFX(SFX.P_ITEM_TAKEN);
*/

                Global.Inventory.HP = 3;
                Global.Inventory.CurrExp += 41;//Global.Inventory.ExpMax;

            }

            if (DirectKeyboardCheckPressed(Keys.P))
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
            else if (DirectKeyboardCheckPressed(Keys.O))
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
            else if (DirectKeyboardCheckPressed(Keys.Q))
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
                switch(capabilities.DisplayName)
                {
                    default:
                        _identifiedController = ControllerNames.GENERIC;
                        break;
                    case "Xbox 360 Controller (XInput STANDARD GAMEPAD)":
                        _identifiedController = ControllerNames.XBOX_360;
                        break;
                    case "PLAYSTATION(R)3 Controller":
                        _identifiedController = ControllerNames.PS3;
                        break;
                    case "Nintendo Switch Pro Controller":
                        _identifiedController = ControllerNames.NINTENDO_SWITCH;
                        break;
                    case "PS4 Controller":
                        _identifiedController = ControllerNames.PS4;
                        break;
                    case "PS5 Controller":
                        _identifiedController = ControllerNames.PS5;
                        break;
                }

                if (capabilities.GamePadType == GamePadType.GamePad) //  && capabilities.HasLeftXThumbStick
                    t_gamePadState = GamePad.GetState(PlayerIndex.One); // Get the current state of Controller1
            } else
            {
                _identifiedController = null;
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
            if (_isBlocked)
            {
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

        internal static bool GetJoysticksEnabled()
        {
            return _joysticksEnabled;
        }

        internal static void SetJoysticksEnabled(bool value)
        {
            _joysticksEnabled = value;
        }

        public static ControllerNames? GetIdentifiedController()
        {
            return _identifiedController;
        }

        public List<Buttons> GetPressedButtons()
        {
            List<Buttons> pressedButtons = new List<Buttons>();

            foreach (Buttons btn in Enum.GetValues(typeof(Buttons)))
            {
                if (t_gamePadState.IsButtonDown(btn))
                {
                    switch (btn)
                    {
                        default:
                            pressedButtons.Add(btn);
                            break;
                        case Buttons.A:
                            if (_identifiedController == ControllerNames.NINTENDO_SWITCH)
                                pressedButtons.Add(Buttons.B);
                            else
                                pressedButtons.Add(btn);
                            break;
                        case Buttons.B:
                            if (_identifiedController == ControllerNames.NINTENDO_SWITCH)
                                pressedButtons.Add(Buttons.A);
                            else
                                pressedButtons.Add(btn);
                            break;
                        case Buttons.Y:
                            if (_identifiedController == ControllerNames.NINTENDO_SWITCH)
                                pressedButtons.Add(Buttons.X);
                            else
                                pressedButtons.Add(btn);
                            break;
                        case Buttons.X:
                            if (_identifiedController == ControllerNames.NINTENDO_SWITCH)
                                pressedButtons.Add(Buttons.Y);
                            else
                                pressedButtons.Add(btn);
                            break;
                    }
                }
            }

            return pressedButtons;
        }

        public Keys[] GetPressedKeys()
        {
            return _lastKeyStroke;
        }
    }
}
