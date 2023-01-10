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
        public enum OptionsMenuPages
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

        public enum MenuTypes
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

        private MenuPage _currPage = null;

        private int LoadFile(int[] args)
        {
            int fileSlot = args[0];
            return 0;
        }

        private int ChangeLanguage(int[] args)
        {
            Global.Languages lang = (Global.Languages) args[0];
            Global.CurrLang = lang;
            return 0;
        }

        private int ToggleFullscreen(int[] args)
        {
            return 0;
        }

        private int ToggleJoystick(int[] args)
        {
            return 0;
        }

        private int SetAllControls(int[] args)
        {
            return 0;
        }

        private int ApplyControls(int[] args)
        {
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
            _menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions.Add(new MenuOption("Language", MenuTypes.SHIFT, ChangeLanguage, OptionsMenuPages.NOTHING, (int)Global.CurrLang, new int[] { -1 }, new string[] { "English", "Japanese" }));
            _menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions.Add(new MenuOption("Player Physics", MenuTypes.SHIFT, ChangeLanguage, OptionsMenuPages.NOTHING, 0, new int[] { -1 }, new string[] { "Classic", "Revamped" }));
            _menuPages[(int)OptionsMenuPages.GENERAL_SETTINGS].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.FRONT_PAGE, -1, new int[] { -1 }, new string[] { "" }));

            _menuPages[(int)OptionsMenuPages.CONFIGURE_CONTROLS] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_CONTROLS].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_CONTROLS].MenuOptions.Add(new MenuOption("Configure Keyboard", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.CONFIGURE_KEYBOARD, -1, new int[] { -1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_CONTROLS].MenuOptions.Add(new MenuOption("Configure Joypad", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.CONFIGURE_GAMEPAD, -1, new int[] { -1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_CONTROLS].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.FRONT_PAGE, -1, new int[] { -1 }, new string[] { "" }));

            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Up", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.UP], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.UP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Down", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.DOWN], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.DOWN].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Left", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.LEFT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Right", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.RIGHT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Jump", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.JUMP], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.JUMP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Whip", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.WHIP], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.WHIP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Sub Weapon", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_WEAPON], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_WEAPON].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Item", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.ITEM], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.ITEM].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Pause", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.PAUSE], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.PAUSE].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Open Inventory", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_INVENTORY], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_INVENTORY].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Main Weapon Left", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_LEFT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Main Weapon Right", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Sub Weapon Left", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_SHIFT_LEFT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_SHIFT_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Sub Weapon Right", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_SHIFT_RIGHT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.SUB_SHIFT_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Menu Confirm", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_CONFIRM], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_CONFIRM].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Menu Cancel", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_CANCEL], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_CANCEL].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Open Rom Window", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_MSX_ROM_SELECTION], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_MSX_ROM_SELECTION].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Open Emulator", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_MSX_EMULATOR], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_MSX_EMULATOR].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Open Config Window", MenuTypes.INPUT_KB, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_CONFIG], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Keyboard, (int)Global.ControllerKeys.MENU_OPEN_CONFIG].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Set All", MenuTypes.SCRIPT_RUNNER, SetAllControls, OptionsMenuPages.NOTHING, 0, new int[] { 0 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Apply Settings", MenuTypes.SCRIPT_RUNNER, ApplyControls, OptionsMenuPages.NOTHING, 0, new int[] { 0 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_KEYBOARD].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.CONFIGURE_CONTROLS, -1, new int[] { -1 }, new string[] { "" }));

            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Joystick Movement", MenuTypes.SHIFT, ToggleJoystick, OptionsMenuPages.NOTHING, Convert.ToSingle(InputManager.GetJoysticksEnabled()), new int[] { -1 }, new string[] { "On", "Off" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Up", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.UP], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.UP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Down", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.DOWN], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.DOWN].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Left", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.LEFT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Right", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.RIGHT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Joy Up", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_UP], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_UP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Joy Down", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_DOWN], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_DOWN].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Joy Left", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_LEFT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Joy Right", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_RIGHT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JOY_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Jump", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JUMP], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.JUMP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Whip", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.WHIP], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.WHIP].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Sub Weapon", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_WEAPON], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_WEAPON].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Item", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.ITEM], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.ITEM].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Pause", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.PAUSE], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.PAUSE].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Open Inventory", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_OPEN_INVENTORY], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_OPEN_INVENTORY].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Main Weapon Left", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_LEFT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Main Weapon Right", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MAIN_WEAPON_SHIFT_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Sub Weapon Left", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_SHIFT_LEFT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_SHIFT_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Sub Weapon Right", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_SHIFT_RIGHT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.SUB_SHIFT_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Menu Confirm", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_CONFIRM], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_CONFIRM].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Menu Cancel", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_CANCEL], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_CANCEL].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Menu Move Left", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_MOVE_LEFT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_MOVE_LEFT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Menu Move Right", MenuTypes.INPUT_GP, null, OptionsMenuPages.NOTHING, InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_MOVE_RIGHT], new int[] { -1 }, new string[] { InputManager.ConfigKeys[(int)Global.ControllerTypes.Gamepad, (int)Global.ControllerKeys.MENU_MOVE_RIGHT].ToString() }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Set All", MenuTypes.SCRIPT_RUNNER, SetAllControls, OptionsMenuPages.NOTHING, 1, new int[] { 0 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Apply Settings", MenuTypes.SCRIPT_RUNNER, ApplyControls, OptionsMenuPages.NOTHING, 1, new int[] { 0 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.CONFIGURE_GAMEPAD].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.CONFIGURE_CONTROLS, -1, new int[] { -1 }, new string[] { "" }));

            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions.Add(new MenuOption("Master", MenuTypes.SLIDER, null, OptionsMenuPages.NOTHING, Global.AudioManager.GetUserMasterVolume(), new int[] { 0, 1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions.Add(new MenuOption("BGM", MenuTypes.SLIDER, null, OptionsMenuPages.NOTHING, Global.AudioManager.GetUserBGMVolume(), new int[] { 0, 1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions.Add(new MenuOption("SFX", MenuTypes.SLIDER, null, OptionsMenuPages.NOTHING, Global.AudioManager.GetUserSFXVolume(), new int[] { 0, 1 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.AUDIO_SETTINGS].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.FRONT_PAGE, -1, new int[] { -1 }, new string[] { "" }));
            
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS] = new MenuPage();
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions = new List<MenuOption>();
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions.Add(new MenuOption("Fullscreen", MenuTypes.SHIFT, null, OptionsMenuPages.NOTHING, Convert.ToSingle(Global.GraphicsDeviceManager.IsFullScreen), new int[] { -1 }, new string[] { "On", "Off" }));
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions.Add(new MenuOption("Window Size", MenuTypes.SHIFT, null, OptionsMenuPages.NOTHING, Global.Main.GetDisplayZoomFactor(), new int[] { -1 }, new string[] { "WindowSize" }));
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions.Add(new MenuOption("Apply Settings", MenuTypes.SCRIPT_RUNNER, ApplyControls, OptionsMenuPages.NOTHING, 2, new int[] { 0 }, new string[] { "" }));
            _menuPages[(int)OptionsMenuPages.VIDEO_SETTINGS].MenuOptions.Add(new MenuOption("Back", MenuTypes.PAGE_TRANSFER, null, OptionsMenuPages.FRONT_PAGE, -1, new int[] { -1 }, new string[] { "" }));

            Texture2D _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _menuCursor = Global.TextureManager.Get8x8Tile(_tex, 0, 1, Vector2.Zero);
            _currPage = _menuPages[_currOptionPosition];
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Global.TextManager.DrawText(PADDING_X - 2 * 8, 2 * 8, "- Options -");

            int y = 0;
            foreach (MenuOption option in _currPage.MenuOptions)
            {

                Color col = Color.White;
                float currVal;
                int _rightSidePadding;
                string currStr = String.Empty;

                if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT) {
                    if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT)
                        Global.TextManager.DrawText(PADDING_X, PADDING_Y + ((WRAP_LIMIT + y - _currOptionPosition) * BUFFER_Y), option.Name);
                    _menuCursor.Draw(spriteBatch, new Vector2(PADDING_X - 16, PADDING_Y + (WRAP_LIMIT * BUFFER_Y)));
                }
                else if (y < ELEMENT_DISPLAY_LIMIT) {
                        Global.TextManager.DrawText(PADDING_X, PADDING_Y + (y * BUFFER_Y), option.Name);

                    _menuCursor.Draw(spriteBatch, new Vector2(PADDING_X - 16, PADDING_Y + (_currOptionPosition * BUFFER_Y)));
                }

                switch (option.ElementType)
                {
                    default:
                    case MenuTypes.SCRIPT_RUNNER:
                    case MenuTypes.PAGE_TRANSFER:
                        break;
                    case MenuTypes.SHIFT:
                        string lShift = "<< ";
                        string rShift = " >>";

                        if ((int)Global.CurrLang == 0)
                            lShift = "";
                        if ((int)Global.CurrLang == (int)Global.Languages.Max - 1)
                            rShift = "";

                        _rightSidePadding = (option.Name.Length + 2) * World.CHIP_SIZE;

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

                        if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT)
                        {
                            if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT)
                                Global.TextManager.DrawText(PADDING_X + _rightSidePadding, PADDING_Y + ((WRAP_LIMIT + y - _currOptionPosition) * BUFFER_Y), lShift + option.Strings[(int)Global.CurrLang] + rShift);
                        }
                        else if (y < ELEMENT_DISPLAY_LIMIT)
                        {
                            Global.TextManager.DrawText(PADDING_X + _rightSidePadding, PADDING_Y + (y * BUFFER_Y), lShift + option.Strings[(int)Global.CurrLang] + rShift);
                        }
                        break;
                    case MenuTypes.SLIDER:
                        int toggleLen = 64;
                        currVal = option.SliderDefaultValue;
                        col = Color.White;
                        float togglePos = (currVal - option.Args[0]) / (option.Args[1] - option.Args[0]);

                        if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT)
                        {
                            if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT) {

                            }
                        }
                        else if (y < ELEMENT_DISPLAY_LIMIT)
                        {
                            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle(toggleLen + PADDING_X, PADDING_Y + (y * BUFFER_Y) + 4, toggleLen, 2), col);
                            HelperFunctions.DrawRectangle(spriteBatch, new Rectangle(toggleLen + PADDING_X + (int)(togglePos * toggleLen), PADDING_Y + (y * BUFFER_Y) + 1, 2, 8), col);

                            string percentage = Math.Floor(togglePos * 100).ToString();
                            Global.TextManager.DrawText(toggleLen + PADDING_X + (int)(toggleLen * 1.2) + 8, PADDING_Y + (y * BUFFER_Y), percentage);
                        }
                        break;
                    case MenuTypes.TOGGLE:
                        currVal = option.SliderDefaultValue;
                        col = Color.White;
                        Color c1, c2;

                        if (currVal == 0) {
                            c1 = col;
                            c2 = new Color(187, 187, 187, 255);
                        } else
                        {
                            c1 = new Color(187, 187, 187, 255);
                            c2 = col;
                        }

                        _rightSidePadding = (option.Name.Length + 2) * World.CHIP_SIZE;

                        if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT)
                        {
                            if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT) {

                            }
                        }
                        else if (y < ELEMENT_DISPLAY_LIMIT)
                        {
                            Global.TextManager.DrawText(PADDING_X + _rightSidePadding, PADDING_Y + (y * BUFFER_Y), option.Strings[0]);
                            Global.TextManager.DrawText(((option.Strings[0].Length + 1) * World.CHIP_SIZE) + PADDING_X + _rightSidePadding, PADDING_Y + (y * BUFFER_Y), option.Strings[1]);
                        }
                        break;
                    case MenuTypes.INPUT_KB:
                        currVal = option.SliderDefaultValue;

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
                                currStr += Char.ToUpper(HelperFunctions.KeyToChar((Keys)currVal, false));
                                if (currStr.Equals("\0"))
                                    currStr = "[None]";
                                break;
                        }

                        if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT)
                        {
                            if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT)
                                Global.TextManager.DrawText(PADDING_X + (19 * 8), PADDING_Y + ((WRAP_LIMIT + y - _currOptionPosition) * BUFFER_Y), currStr);
                        }
                        else if (y < ELEMENT_DISPLAY_LIMIT) {
                            Global.TextManager.DrawText(PADDING_X + (19 * 8), PADDING_Y + (y * BUFFER_Y), currStr);
                        }
                        break;
                    case MenuTypes.INPUT_GP:
                        currVal = option.SliderDefaultValue;
                        currStr = HelperFunctions.ButtonToString((Buttons)currVal);

                        if (_currOptionPosition > WRAP_LIMIT - 1 && _currPage.MenuOptions.Count > ELEMENT_DISPLAY_LIMIT)
                        {
                            if ((WRAP_LIMIT + y - _currOptionPosition) >= 0 && (WRAP_LIMIT + y - _currOptionPosition) < ELEMENT_DISPLAY_LIMIT)
                                Global.TextManager.DrawText(PADDING_X + (19 * 8), PADDING_Y + ((WRAP_LIMIT + y - _currOptionPosition) * BUFFER_Y), currStr);
                        }
                        else if (y < ELEMENT_DISPLAY_LIMIT)
                        {
                            Global.TextManager.DrawText(PADDING_X + (19 * 8), PADDING_Y + (y * BUFFER_Y), currStr);
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
            if (InputManager.DirPressedY != 0) {
                _currOptionPosition += InputManager.DirPressedY;

                if (_currOptionPosition >= _currPage.MenuOptions.Count)
                    _currOptionPosition = 0;

                if (_currOptionPosition < 0)
                    _currOptionPosition = _currPage.MenuOptions.Count - 1;

                Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);
            }

            if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_CONFIRM])
            {
                switch (_currPage.MenuOptions[_currOptionPosition].ElementType)
                {
                    case MenuTypes.SCRIPT_RUNNER:
                        Func<int[], int> script = _currPage.MenuOptions[_currOptionPosition].Function;
                        int[] args = _currPage.MenuOptions[_currOptionPosition].Args;
                        script(args);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                        break;
                    case MenuTypes.PAGE_TRANSFER:
                        _currPage = _menuPages[(int)_currPage.MenuOptions[_currOptionPosition].Page];
                        _currOptionPosition = 0;
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                        break;
                    case MenuTypes.SLIDER:
                        /*
                        Global.AudioManager.GetUserMasterVolume();
                        Global.AudioManager.GetUserBGMVolume();
                        Global.AudioManager.GetUserSFXVolume();
                        Global.AudioManager.SetMasterVolume();
                        Global.AudioManager.SetBGMVolume();
                        Global.AudioManager.SetSFXVolume();
                        */

            break;
                }
            }

            if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_CANCEL])
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

    }
}
