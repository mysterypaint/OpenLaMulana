using Microsoft.VisualBasic.FileIO;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenLaMulana.Entities;
using OpenLaMulana.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenLaMulana.System.Menu;

namespace OpenLaMulana.System
{
    internal partial class Menu
    {
        public enum OptionsMenuPages : int
        {
            NOTHING,
            FRONT_PAGE,
            LOAD_SAVE,
            GENERAL_SETTINGS,
            CONFIGURE_CONTROLS,
            CONFIGURE_KEYBOARD,
            CONFIGURE_GAMEPAD,
            AUDIO_SETTINGS,
            VIDEO_SETTINGS,
            MAX
        };

        public enum MenuTypes : int
        {
            SCRIPT_RUNNER,
            PAGE_TRANSFER,
            SLIDER,
            SHIFT,
            TOGGLE,
            INPUT_KB,
            INPUT_GP,
            YES_NO_PROMPT,
            MAX
        };

        private MenuPage[] _menuPages = new MenuPage[(int)OptionsMenuPages.MAX];
        private Sprite _menuCursor;
        private int _currOptionPosition = (int)OptionsMenuPages.FRONT_PAGE;
        private const int BUFFER_X = 16 * World.CHIP_SIZE;
        private const int BUFFER_Y = 16;
        private const int PADDING_X = 40;
        private const int PADDING_Y = 32;
        private const int WRAP_LIMIT = 4;
        private const int ELEMENT_DISPLAY_LIMIT = 9;
        private bool _inputting = false;
        private Color _colorYellow = new Color(204, 204, 51, 255);
        private Color _colorGrey = new Color(187, 187, 187, 255);

        private MenuPage _currPage = null;
        private int _inputCooldownTimer = 0;
        private int _inputTimerReset = 8;
        private int _stickInputCooldownTimer = 0;
        private int _stickInputCooldownTimerReset = 8;

        private int LoadFile(int[] args)
        {
            int fileSlot = args[0];
            return 0;
        }

        private int ResetAllControls(int[] args)
        {
            switch (args[0])
            {
                default:
                case 0: // Reset all keyboard controls
                    // TODO: Reset all keyboard controls
                    break;
                case 1: // Reset all gamepad controls
                    // TODO: Reset all gamepad controls
                    break;
            }
            return 0;
        }

        private int ApplyControls(int[] args)
        {
            switch (args[0])
            {
                case 0:     // Apply all Keyboard controls
                    for (var i = 0; i < _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Count - 3; i++)
                    {
                        int key = _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions[i].Args[0];
                        int value = (int)_menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions[i].Value;
                        InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)key] = value;
                    }
                    break;
                case 1:     // Apply all Gamepad controls
                    InputManager.SetJoysticksEnabled(Convert.ToBoolean(_menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions[0].Value));

                    for (var i = 1; i < _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Count - 3; i++)
                    {
                        Global.ControllerKeys key = (Global.ControllerKeys) _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions[i].Args[0];
                        Buttons value = (Buttons)_menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions[i].Value;
                        InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)key] = (int)value;
                    }
                    break;
                case 2:     // Apply all Graphics settings

                    int newZoomFactor = (int)_menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions[1].Value + 1;
                    Global.Camera.UpdateWindowSize(Main.WINDOW_WIDTH * newZoomFactor, Main.WINDOW_HEIGHT * newZoomFactor, newZoomFactor);
                    Global.Main.SetDisplayZoomFactor(newZoomFactor);
                    Global.Main.ToggleDisplayMode();
                    //Global.Main.GetDisplayZoomFactor()


                    bool setFullscreen = Convert.ToBoolean(_menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions[0].Value);
                    bool currFullscreen = Global.GraphicsDeviceManager.IsFullScreen;

                    if (setFullscreen != currFullscreen) {
                        Global.GraphicsDeviceManager.ToggleFullScreen();
                        Global.GraphicsDeviceManager.ApplyChanges();
                    }
                    break;
                case 3:     // Apply General settings
                    Global.CurrLang = (Global.Languages)_menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions[0].Value;
                    Global.ProtagPhysics = (Global.PlatformingPhysics)_menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions[1].Value;
                    Global.QoLChanges = Convert.ToBoolean(_menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions[2].Value);


                    if (Global.QoLChanges)
                    {
                        Global.Inventory.ExpMax = Global.Inventory.HPMax; // Remake behavior
                    }
                    else
                        Global.Inventory.ExpMax = Global.InventoryStruct.ExpMaxClassic; // When this is 88, trigger and reset to 0
                    break;
            }
            return 0;
        }

        private int AdjustVolume(int[] args)
        {
            switch (args[2])
            {
                case 0:
                    Global.AudioManager.SetMasterVolume(_menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions[0].Value);
                    break;
                case 1:
                    Global.AudioManager.SetBGMVolume(_menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions[1].Value);
                    break;
                case 2:
                    Global.AudioManager.SetSFXVolume(_menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions[2].Value);
                    break;
            }
            return 0;
        }

        private int Dummy(int dummy) { return 0; }

        public Menu()
        {
            _menuPages[(int)OptionsMenuPages.FRONT_PAGE] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.FRONT_PAGE].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.FRONT_PAGE].MenuOptions.Add(new MenuOption("Load Save", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.LOAD_SAVE, -1, new int[] { -1 }, new string[] {""}));
            _menuPages[(int)OptionsMenuPages.FRONT_PAGE].MenuOptions.Add(new MenuOption("General Settings", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.GENERAL_SETTINGS, -1, new int[] { -1 }, new string[] {""}));
            _menuPages[(int)OptionsMenuPages.FRONT_PAGE].MenuOptions.Add(new MenuOption("Configure Controls", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.CONFIGURE_CONTROLS, -1, new int[] { -1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.FRONT_PAGE].MenuOptions.Add(new MenuOption("Audio Settings", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.AUDIO_SETTINGS, -1, new int[] { -1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.FRONT_PAGE].MenuOptions.Add(new MenuOption("Video Settings", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.VIDEO_SETTINGS, -1, new int[] { -1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.FRONT_PAGE].MenuOptions.Add(new MenuOption("Return to Title", MenuTypes.YES_NO_PROMPT, null, OptionsMenuPages.NOTHING, -1, new int[] { -1 }, new string[] { "Return to the title screen?" }));

            _menuPages[(int)OptionsMenuPages.LOAD_SAVE] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.LOAD_SAVE].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.LOAD_SAVE].MenuOptions.Add(new MenuOption("File 1", MenuTypes.SCRIPT_RUNNER, LoadFile, OptionsMenuPages.LOAD_SAVE, -1, new int[] { 0 }, new string[] {""}));
            _menuPages[(int)OptionsMenuPages.LOAD_SAVE].MenuOptions.Add(new MenuOption("File 2", MenuTypes.SCRIPT_RUNNER, LoadFile, OptionsMenuPages.GENERAL_SETTINGS, -1, new int[] { 1 }, new string[] {""}));
            _menuPages[(int)OptionsMenuPages.LOAD_SAVE].MenuOptions.Add(new MenuOption("File 3", MenuTypes.SCRIPT_RUNNER, LoadFile, OptionsMenuPages.CONFIGURE_CONTROLS, -1, new int[] { 2 }, new string[] {""}));
            _menuPages[(int)OptionsMenuPages.LOAD_SAVE].MenuOptions.Add(new MenuOption("File 4", MenuTypes.SCRIPT_RUNNER, LoadFile, OptionsMenuPages.CONFIGURE_CONTROLS, -1, new int[] { 3 }, new string[] {""}));
            _menuPages[(int)OptionsMenuPages.LOAD_SAVE].MenuOptions.Add(new MenuOption("File 5", MenuTypes.SCRIPT_RUNNER, LoadFile, OptionsMenuPages.CONFIGURE_CONTROLS, -1, new int[] { 4 }, new string[] {""}));
            _menuPages[(int)OptionsMenuPages.LOAD_SAVE].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.FRONT_PAGE, -1, new int[] { -1 }, new string[] {""}));

            _menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions.Add(new MenuOption("Language", MenuTypes.SHIFT, null, OptionsMenuPages.NOTHING, (int)Global.CurrLang, new int[] { -1 }, new string[] { "English", "Japanese" }));
            _menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions.Add(new MenuOption("Player Physics", MenuTypes.SHIFT, null, OptionsMenuPages.NOTHING, 0, new int[] { -1 }, new string[] { "Classic", "Revamped" }));
            _menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions.Add(new MenuOption("QoL Changes", MenuTypes.SHIFT, null, OptionsMenuPages.NOTHING, 0, new int[] { -1 }, new string[] { "OFF", "ON" }));
            _menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions.Add(new MenuOption("Apply Settings", MenuTypes.SCRIPT_RUNNER, ApplyControls, OptionsMenuPages.NOTHING, 3, new int[] { 3 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.FRONT_PAGE, -1, new int[] { -1 }, new string[] { "" }));

            _menuPages[(int)OptionsMenuPages.CONFIGURE_CONTROLS] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_CONTROLS].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_CONTROLS].MenuOptions.Add(new MenuOption("Configure Keyboard", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.CONFIGURE_KEYBOARD, -1, new int[] { -1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_CONTROLS].MenuOptions.Add(new MenuOption("Configure Joypad", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.CONFIGURE_GAMEPAD, -1, new int[] { -1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_CONTROLS].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.FRONT_PAGE, -1, new int[] { -1 }, new string[] { "" }));

            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Up", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.UP], new int[] { (int)Global.ControllerKeys.UP }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.UP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Down", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.DOWN], new int[] { (int)Global.ControllerKeys.DOWN }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.DOWN].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Left", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.LEFT], new int[] { (int)Global.ControllerKeys.LEFT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Right", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.RIGHT], new int[] { (int)Global.ControllerKeys.RIGHT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Jump", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.JUMP], new int[] { (int)Global.ControllerKeys.JUMP }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.JUMP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Whip", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MAIN_WEAPON], new int[] { (int)Global.ControllerKeys.MAIN_WEAPON }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MAIN_WEAPON].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Sub Weapon", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_WEAPON], new int[] { (int)Global.ControllerKeys.SUB_WEAPON }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_WEAPON].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Item", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.ITEM], new int[] { (int)Global.ControllerKeys.ITEM }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.ITEM].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Pause", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.PAUSE], new int[] { (int)Global.ControllerKeys.PAUSE }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.PAUSE].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Open Inventory", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_INVENTORY], new int[] { (int)Global.ControllerKeys.MENU_OPEN_INVENTORY }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_INVENTORY].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Main Weapon Left", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_LEFT], new int[] { (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_LEFT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Main Weapon Right", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT], new int[] { (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Sub Weapon Left", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_SHIFT_LEFT], new int[] { (int)Global.ControllerKeys.SUB_SHIFT_LEFT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_SHIFT_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Sub Weapon Right", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_SHIFT_RIGHT], new int[] { (int)Global.ControllerKeys.SUB_SHIFT_RIGHT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_SHIFT_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Menu Confirm", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_CONFIRM], new int[] { (int)Global.ControllerKeys.MENU_CONFIRM }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_CONFIRM].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Menu Cancel", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_CANCEL], new int[] { (int)Global.ControllerKeys.MENU_CANCEL }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_CANCEL].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Open Rom Window", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_MSX_ROM_SELECTION], new int[] { (int)Global.ControllerKeys.MENU_OPEN_MSX_ROM_SELECTION }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_MSX_ROM_SELECTION].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Open Emulator", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_MSX_EMULATOR], new int[] { (int)Global.ControllerKeys.MENU_OPEN_MSX_EMULATOR }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_MSX_EMULATOR].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Open Config Window", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_CONFIG], new int[] { (int)Global.ControllerKeys.MENU_OPEN_CONFIG }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_CONFIG].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Reset to Default", MenuTypes.SCRIPT_RUNNER, ResetAllControls, OptionsMenuPages.NOTHING, 0, new int[] { 0 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Apply Settings", MenuTypes.SCRIPT_RUNNER, ApplyControls, OptionsMenuPages.NOTHING, 0, new int[] { 0 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.CONFIGURE_CONTROLS, -1, new int[] { -1 }, new string[] { "" }));

            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Joystick Movement", MenuTypes.SHIFT, null, OptionsMenuPages.NOTHING, Convert.ToSingle(InputManager.GetJoysticksEnabled()), new int[] { -1 }, new string[] { "OFF", "ON" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Up", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.UP], new int[] { (int)Global.ControllerKeys.UP }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.UP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Down", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.DOWN], new int[] { (int)Global.ControllerKeys.DOWN }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.DOWN].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Left", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.LEFT], new int[] { (int)Global.ControllerKeys.LEFT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Right", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.RIGHT], new int[] { (int)Global.ControllerKeys.RIGHT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Joy Up", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_UP], new int[] { (int)Global.ControllerKeys.JOY_UP }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_UP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Joy Down", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_DOWN], new int[] { (int)Global.ControllerKeys.JOY_DOWN }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_DOWN].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Joy Left", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_LEFT], new int[] { (int)Global.ControllerKeys.JOY_LEFT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Joy Right", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_RIGHT], new int[] { (int)Global.ControllerKeys.JOY_RIGHT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Jump", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JUMP], new int[] { (int)Global.ControllerKeys.JUMP }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JUMP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Whip", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MAIN_WEAPON], new int[] { (int)Global.ControllerKeys.MAIN_WEAPON }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MAIN_WEAPON].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Sub Weapon", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_WEAPON], new int[] { (int)Global.ControllerKeys.SUB_WEAPON }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_WEAPON].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Item", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.ITEM], new int[] { (int)Global.ControllerKeys.ITEM }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.ITEM].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Pause", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.PAUSE], new int[] { (int)Global.ControllerKeys.PAUSE }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.PAUSE].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Open Inventory", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_OPEN_INVENTORY], new int[] { (int)Global.ControllerKeys.MENU_OPEN_INVENTORY }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_OPEN_INVENTORY].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Main Weapon Left", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_LEFT], new int[] { (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_LEFT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Main Weapon Right", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT], new int[] { (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Sub Weapon Left", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_SHIFT_LEFT], new int[] { (int)Global.ControllerKeys.SUB_SHIFT_LEFT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_SHIFT_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Sub Weapon Right", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_SHIFT_RIGHT], new int[] { (int)Global.ControllerKeys.SUB_SHIFT_RIGHT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_SHIFT_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Menu Confirm", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_CONFIRM], new int[] { (int)Global.ControllerKeys.MENU_CONFIRM }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_CONFIRM].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Menu Cancel", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_CANCEL], new int[] { (int)Global.ControllerKeys.MENU_CANCEL }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_CANCEL].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Menu Move Left", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_MOVE_LEFT], new int[] { (int)Global.ControllerKeys.MENU_MOVE_LEFT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_MOVE_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Menu Move Right", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_MOVE_RIGHT], new int[] { (int)Global.ControllerKeys.MENU_MOVE_RIGHT }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_MOVE_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Reset to Default", MenuTypes.SCRIPT_RUNNER, ResetAllControls, OptionsMenuPages.NOTHING, 1, new int[] { 1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Apply Settings", MenuTypes.SCRIPT_RUNNER, ApplyControls, OptionsMenuPages.NOTHING, 1, new int[] { 1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.CONFIGURE_CONTROLS, -1, new int[] { -1 }, new string[] { "" }));

            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions.Add(new MenuOption("Master", MenuTypes.SLIDER, AdjustVolume, OptionsMenuPages.NOTHING, Global.AudioManager.GetUserMasterVolume(), new int[] { 0, 1,0 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions.Add(new MenuOption("BGM", MenuTypes.SLIDER, AdjustVolume, OptionsMenuPages.NOTHING, Global.AudioManager.GetUserBGMVolume(), new int[] { 0, 1,1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions.Add(new MenuOption("SFX", MenuTypes.SLIDER, AdjustVolume, OptionsMenuPages.NOTHING, Global.AudioManager.GetUserSFXVolume(), new int[] { 0, 1,2 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.FRONT_PAGE, -1, new int[] { -1 }, new string[] { "" }));
            
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions.Add(new MenuOption("Fullscreen", MenuTypes.SHIFT, null, OptionsMenuPages.NOTHING, Convert.ToSingle(Global.GraphicsDeviceManager.IsFullScreen), new int[] { -1 }, new string[] { "Off", "On" }));
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions.Add(new MenuOption("Window Size", MenuTypes.SHIFT, null, OptionsMenuPages.NOTHING, Global.Main.GetDisplayZoomFactor(), new int[] { -1 }, new string[] { "WindowSize" }));
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions.Add(new MenuOption("Apply Settings", MenuTypes.SCRIPT_RUNNER, ApplyControls, OptionsMenuPages.NOTHING, 2, new int[] { 2 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.FRONT_PAGE, -1, new int[] { -1 }, new string[] { "" }));

            Texture2D _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _menuCursor = Global.TextureManager.Get8x8Tile(_tex, 0, 1, Vector2.Zero);
            _currPage = _menuPages[_currOptionPosition];
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Global.TextManager.QueueText(PADDING_X - 2 * 8, 2 * 8, "- Options -");

            int y = 0;
            foreach (MenuOption option in _currPage.MenuOptions)
            {
                Color highlightCol = _colorGrey;
                Color navigationCol = _colorGrey;

                float currVal;
                int _rightSidePadding;
                string currStr = String.Empty;

                if (y == _currOptionPosition) {
                    if (_inputting)
                        highlightCol = new Color(255, 51, 51, 255);// _colorYellow;
                    navigationCol = Color.White;
                }

                if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT) {
                    if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT)
                        Global.TextManager.QueueText(PADDING_X, PADDING_Y + ((WRAP_LIMIT + y - _currOptionPosition) * BUFFER_Y), option.Name, 32, navigationCol);
                    _menuCursor.Draw(spriteBatch, new Vector2(PADDING_X - 16, PADDING_Y + (WRAP_LIMIT * BUFFER_Y)));
                }
                else if (y < ELEMENT_DISPLAY_LIMIT) {
                        Global.TextManager.QueueText(PADDING_X, PADDING_Y + (y * BUFFER_Y), option.Name, 32, navigationCol);

                    _menuCursor.Draw(spriteBatch, new Vector2(PADDING_X - 16, PADDING_Y + (_currOptionPosition * BUFFER_Y)));
                }

                switch (option.ElementType)
                {
                    default:
                    case MenuTypes.SCRIPT_RUNNER:
                    case MenuTypes.PAGE_TRANSFER:
                        break;
                    case MenuTypes.SHIFT:
                        string lShift = "   ";
                        string rShift = "   ";
                        if (_inputting && y == _currOptionPosition)
                        {
                            lShift = "<< ";
                            rShift = " >>";
                        }

                        if (option.Strings[0].Equals("WindowSize"))
                        {
                            int zoomMax = Global.Main.GetDisplayZoomMax();
                            List<string> winSizes = new List<string>();
                            for (int i = 1; i <= zoomMax; i++)
                            {
                                winSizes.Add(String.Format("{0}x", i));
                            }

                            option.Strings = winSizes.ToArray();
                        }

                        currVal = Math.Clamp(option.Value, 0, option.Strings.Length - 1);
                        if ((int)currVal == 0)
                            lShift = "";
                        if ((int)currVal == option.Strings.Length - 1)
                            rShift = "";

                        string finalStr = lShift + option.Strings[(int)currVal] + rShift;
                        int hAlignRight = Main.WINDOW_WIDTH - World.CHIP_SIZE - (finalStr.Length * World.CHIP_SIZE);

                        if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT)
                        {
                            if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT)
                                Global.TextManager.QueueText(hAlignRight, PADDING_Y + ((WRAP_LIMIT + y - _currOptionPosition) * BUFFER_Y), finalStr, 32, highlightCol);
                        }
                        else if (y < ELEMENT_DISPLAY_LIMIT)
                        {
                            Global.TextManager.QueueText(hAlignRight, PADDING_Y + (y * BUFFER_Y), finalStr, 32, highlightCol);
                        }
                        break;
                    case MenuTypes.SLIDER:
                        int toggleLen = 64;
                        currVal = option.Value;
                        float togglePos = (currVal - option.Args[0]) / (option.Args[1] - option.Args[0]);

                        if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT)
                        {
                            if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT) {

                            }
                        }
                        else if (y < ELEMENT_DISPLAY_LIMIT)
                        {
                            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle(toggleLen + PADDING_X, PADDING_Y + (y * BUFFER_Y) + 4, toggleLen, 2), Color.White);
                            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle(toggleLen + PADDING_X + (int)(togglePos * toggleLen), PADDING_Y + (y * BUFFER_Y) + 1, 2, 8), highlightCol);

                            string percentage = Math.Floor(togglePos * 100).ToString();
                            Global.TextManager.QueueText(toggleLen + PADDING_X + (int)(toggleLen * 1.2) + 8, PADDING_Y + (y * BUFFER_Y), percentage, 32, highlightCol);
                        }
                        break;
                    case MenuTypes.TOGGLE:
                        currVal = option.Value;
                        highlightCol = Color.White;
                        Color c1, c2;

                        if (currVal == 0) {
                            c1 = highlightCol;
                            c2 = _colorGrey;
                        } else
                        {
                            c1 = _colorGrey;
                            c2 = highlightCol;
                        }

                        _rightSidePadding = (option.Name.Length + 2) * World.CHIP_SIZE;

                        if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT)
                        {
                            if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT) {

                            }
                        }
                        else if (y < ELEMENT_DISPLAY_LIMIT)
                        {
                            Global.TextManager.QueueText(PADDING_X + _rightSidePadding, PADDING_Y + (y * BUFFER_Y), option.Strings[0], 32, highlightCol);
                            Global.TextManager.QueueText(((option.Strings[0].Length + 1) * World.CHIP_SIZE) + PADDING_X + _rightSidePadding, PADDING_Y + (y * BUFFER_Y), option.Strings[1], 32, highlightCol);
                        }
                        break;
                    case MenuTypes.INPUT_KB:
                        currVal = option.Value;

                        switch((Keys)currVal)
                        {
                            case Keys.F1:
                                currStr = "F1";
                                break;
                            case Keys.F2:
                                currStr = "F2";
                                break;
                            case Keys.F3:
                                currStr = "F3";
                                break;
                            case Keys.F4:
                                currStr = "F4";
                                break;
                            case Keys.F5:
                                currStr = "F5";
                                break;
                            case Keys.F6:
                                currStr = "F6";
                                break;
                            case Keys.F7:
                                currStr = "F7";
                                break;
                            case Keys.F8:
                                currStr = "F8";
                                break;
                            case Keys.F9:
                                currStr = "F9";
                                break;
                            case Keys.F10:
                                currStr = "F10";
                                break;
                            case Keys.F11:
                                currStr = "F11";
                                break;
                            case Keys.F12:
                                currStr = "F12";
                                break;
                            default:
                                currStr += HelperFunctions.KeyToString((Keys)currVal, false);
                                switch (currStr)
                                {
                                    case "\0":
                                        currStr = "[None]";
                                        break;
                                    case " ":
                                        currStr = "[Space]";
                                        break;
                                }
                                break;
                        }

                        if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT)
                        {
                            if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT)
                                Global.TextManager.QueueText(PADDING_X + (19 * 8), PADDING_Y + ((WRAP_LIMIT + y - _currOptionPosition) * BUFFER_Y), currStr, 32, highlightCol);
                        }
                        else if (y < ELEMENT_DISPLAY_LIMIT) {
                            Global.TextManager.QueueText(PADDING_X + (19 * 8), PADDING_Y + (y * BUFFER_Y), currStr, 32, highlightCol);
                        }
                        break;
                    case MenuTypes.INPUT_GP:
                        currVal = option.Value;
                        currStr = HelperFunctions.ButtonToString((Buttons)currVal);

                        if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT)
                        {
                            if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT)
                                Global.TextManager.QueueText(PADDING_X + (19 * 8), PADDING_Y + ((WRAP_LIMIT + y - _currOptionPosition) * BUFFER_Y), currStr, 32, highlightCol);
                        }
                        else if (y < ELEMENT_DISPLAY_LIMIT)
                        {
                            Global.TextManager.QueueText(PADDING_X + (19 * 8), PADDING_Y + (y * BUFFER_Y), currStr, 32, highlightCol);
                        }

                        break;
                    case MenuTypes.YES_NO_PROMPT:

                        break;
                }

                y++;
            }
        }

        public void Update(GameTime gameTime)
        {
            bool inputModified = false;
            if (_inputting)
            {
                switch (_currPage.MenuOptions[_currOptionPosition].ElementType)
                {
                    case MenuTypes.SHIFT:
                        if (InputManager.DirConfPressedX != 0)
                        {
                            _currPage.MenuOptions[_currOptionPosition].Value += InputManager.DirConfPressedX;
                            if (_currPage.MenuOptions[_currOptionPosition].Value < 0)
                            {
                                _currPage.MenuOptions[_currOptionPosition].Value = _currPage.MenuOptions[_currOptionPosition].Strings.Length - 1;
                            } else if (_currPage.MenuOptions[_currOptionPosition].Value > _currPage.MenuOptions[_currOptionPosition].Strings.Length - 1)
                            {
                                _currPage.MenuOptions[_currOptionPosition].Value = 0;
                            }
                            Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);
                        }
                        break;
                    case MenuTypes.SLIDER:
                        if (InputManager.DirHeldX != 0)
                        {
                            _currPage.MenuOptions[_currOptionPosition].Value += InputManager.DirHeldX * 0.01f;
                            if (_currPage.MenuOptions[_currOptionPosition].Value < _currPage.MenuOptions[_currOptionPosition].Args[0])
                            {
                                _currPage.MenuOptions[_currOptionPosition].Value = _currPage.MenuOptions[_currOptionPosition].Args[0];
                            }
                            else if (_currPage.MenuOptions[_currOptionPosition].Value > _currPage.MenuOptions[_currOptionPosition].Args[1])
                            {
                                _currPage.MenuOptions[_currOptionPosition].Value = _currPage.MenuOptions[_currOptionPosition].Args[1];
                            } else
                            {
                                Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);
                                Func<int[], int> script = _currPage.MenuOptions[_currOptionPosition].Function;
                                int[] args = _currPage.MenuOptions[_currOptionPosition].Args;
                                script(args);
                            }
                        }
                        break;
                    case MenuTypes.TOGGLE:
                        break;
                    case MenuTypes.INPUT_KB:
                        if (_inputCooldownTimer <= 0) {
                            Keys[] pressedKeys = Global.Input.GetPressedKeys();

                            if (pressedKeys.Length > 0)
                            {
                                Keys lastKey = pressedKeys[0];

                                switch (lastKey)
                                {
                                    default:
                                        if ((float)lastKey != _currPage.MenuOptions[_currOptionPosition].Value)
                                        {
                                            Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);
                                            _currPage.MenuOptions[_currOptionPosition].Value = (float)lastKey;
                                            inputModified = true;
                                        }
                                        break;
                                    //case Keys.Enter: // We can block keys by adding blank entries to this switch
                                    //    break;
                                }

                            }
                        } else
                        {
                            if (Global.AnimationTimer.OneFrameElapsed())
                                _inputCooldownTimer--;
                        }
                        break;
                    case MenuTypes.INPUT_GP:
                        if (_inputCooldownTimer <= 0)
                        {
                            List<Buttons> pressedButtons = Global.Input.GetPressedButtons();

                            if (pressedButtons.Count > 0)
                            {
                                Buttons lastKey = pressedButtons[0];

                                switch (lastKey)
                                {
                                    default:
                                        if ((float)lastKey != _currPage.MenuOptions[_currOptionPosition].Value)
                                        {
                                            Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);

                                            // Menu Confirm is 19
                                            if (_currOptionPosition == 19 && (_currPage.MenuOptions[20].Value == (float)lastKey))
                                                _currPage.MenuOptions[20].Value = _currPage.MenuOptions[19].Value;
                                            else if (_currOptionPosition == 20 && (_currPage.MenuOptions[19].Value == (float)lastKey))
                                                _currPage.MenuOptions[19].Value = _currPage.MenuOptions[20].Value;

                                            _currPage.MenuOptions[_currOptionPosition].Value = (float)lastKey;
                                            inputModified = true;
                                        }
                                        break;
                                }

                                switch (lastKey)
                                {
                                    case Buttons.LeftThumbstickLeft:
                                    case Buttons.LeftThumbstickRight:
                                    case Buttons.LeftThumbstickUp:
                                    case Buttons.LeftThumbstickDown:
                                    case Buttons.LeftStick:
                                    case Buttons.RightThumbstickLeft:
                                    case Buttons.RightThumbstickRight:
                                    case Buttons.RightThumbstickUp:
                                    case Buttons.RightThumbstickDown:
                                    case Buttons.RightStick:
                                        _stickInputCooldownTimer = _stickInputCooldownTimerReset;
                                        break;
                                }
                            }
                        }
                        else
                        {
                            if (Global.AnimationTimer.OneFrameElapsed())
                                _inputCooldownTimer--;
                        }
                        break;
                    case MenuTypes.YES_NO_PROMPT:

                        break;
                }
            }
            else
            {
                if (InputManager.DirConfPressedY != 0 && _stickInputCooldownTimer <= 0)
                {
                    _currOptionPosition += InputManager.DirConfPressedY;

                    if (_currOptionPosition >= _currPage.MenuOptions.Count)
                        _currOptionPosition = 0;

                    if (_currOptionPosition < 0)
                        _currOptionPosition = _currPage.MenuOptions.Count - 1;

                    Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);
                } else
                {
                    if (Global.AnimationTimer.OneFrameElapsed())
                        _stickInputCooldownTimer--;
                }
            }

            if (InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.MENU_CONFIRM) || inputModified)
            {
                switch (_currPage.MenuOptions[_currOptionPosition].ElementType)
                {
                    case MenuTypes.SCRIPT_RUNNER:
                        Func<int[], int> script = _currPage.MenuOptions[_currOptionPosition].Function;
                        int[] args = _currPage.MenuOptions[_currOptionPosition].Args;
                        script(args);

                        if (script == ApplyControls)
                        {
                            Global.AudioManager.PlaySFX(SFX.P_ITEM_TAKEN);
                        } else
                            Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                        break;
                    case MenuTypes.PAGE_TRANSFER:
                        if (_currOptionPosition == _currPage.MenuOptions.Count - 1)
                        {
                            // Do not play a sound if we pressed Menu Confirm on a Back button
                        }
                        else
                            Global.AudioManager.PlaySFX(SFX.MSX_OPEN);

                        _currPage = _menuPages[(int)_currPage.MenuOptions[_currOptionPosition].Page];
                        _currOptionPosition = 0;
                        break;
                    case MenuTypes.INPUT_KB:
                    case MenuTypes.INPUT_GP:
                        _inputCooldownTimer = _inputTimerReset;
                        _inputting = !_inputting;

                        if (_inputting)
                            Global.AudioManager.PlaySFX(SFX.GRAIL_WARP);
                        break;
                    case MenuTypes.SHIFT:
                    case MenuTypes.SLIDER:
                    case MenuTypes.TOGGLE:
                    case MenuTypes.YES_NO_PROMPT:
                        _inputting = !_inputting;
                        Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);
                        break;
                }
            }

            if (InputManager.ButtonCheckPressed60FPS(Global.ControllerKeys.MENU_CANCEL) && ! _inputting && !inputModified)
            {
                if (_currPage == _menuPages[(int)OptionsMenuPages.FRONT_PAGE])
                {
                    // We backed out of the front page, so just close the MSX window
                    Global.Main.SetState(Global.GameState.PLAYING);
                    Global.MobileSuperX.SetState(Global.MSXStates.INACTIVE);
                    _currOptionPosition = 0;
                } else
                {
                    _currPage = _menuPages[(int)_currPage.MenuOptions[_currPage.MenuOptions.Count - 1].Page]; // The last element is always "Back": Grab the page that it leads to, and make that the current page
                    _currOptionPosition = 0;
                }
            }

        }

        public MenuPage GetCurrPage()
        {
            return _currPage;
        }

        public bool IsInputting() { return _inputting;}

    }
}
