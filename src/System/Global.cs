﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using static OpenLaMulana.Main;

namespace OpenLaMulana
{
    public static class Global
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

        public enum Shaders
        {
            NONE,
            TRANSITION,
            HUE_SHIFT,
            BINARY,
            MAX
        };
        public enum GameState
        {
            INITIAL,
            TRANSITION,
            PLAYING,
            GAME_OVER,
            PAUSED,
            MSX_INVENTORY,
            MSX_ROMS,
            MSX_EMULATOR,
            MSX_MANTRAS,
            MAX
        };
        public enum ScreenOverlayState
        {
            INVISIBLE,
            PAUSE,
            INVENTORY,
            ROM_SELECTION,
            MSX_BIOS,
            OPTIONS,
            SHOP,
            MAX
        };

        public enum Textures
        {
            BOSS00,
            BOSS01,
            BOSS02,
            BOSS03,
            BOSS04,
            BOSS05,
            BOSS06,
            BOSS07,
            BOSS08,
            ENDDEMO,
            ENEMY1,
            ENEMY2,
            EVEG00,
            EVEG00_EN,
            EVEG01,
            EVEG02,
            EVEG02_EN,
            EVEG03,
            EVEG03_EN,
            EVEG04,
            EVEG04_EN,
            EVEG05,
            EVEG05_EN,
            EVEG06,
            EVEG06_EN,
            EVEG07,
            EVEG07_EN,
            EVEG08,
            EVEG08_EN,
            EVEG09,
            EVEG09_EN,
            EVEG10,
            EVEG10_EN,
            EVEG11,
            EVEG11_EN,
            EVEG12,
            EVEG12_EN,
            EVEG13,
            EVEG13_EN,
            EVEG14,
            EVEG14_EN,
            EVEG15,
            EVEG15_EN,
            EVEG16,
            EVEG16_EN,
            EVEG17,
            EVEG17_EN,
            EVEG18,
            EVEG18_EN,
            EVEG19,
            EVEG20,
            EVEG21,
            EVEG22,
            FONT_EN,
            FONT_JP,
            ITEM,
            LOGO,
            LOGO_EN,
            MAPG00,
            MAPG01,
            MAPG02,
            MAPG03,
            MAPG04,
            MAPG05,
            MAPG06,
            MAPG07,
            MAPG08,
            MAPG09,
            MAPG10,
            MAPG11,
            MAPG12,
            MAPG13,
            MAPG14,
            MAPG15,
            MAPG16,
            MAPG17,
            MAPG18,
            MAPG18_EN,
            MAPG19,
            MAPG20,
            MAPG20_EN,
            MAPG21,
            MAPG22,
            MAPG22_EN,
            MAPG31,
            MAPG31_EN,
            MAPG32,
            MAPG32_EN,
            MSXLOGO,
            MSXLOGO_EN,
            OPDEMO,
            PROT1,
            PROT_DEBUG,
            STDEMO1,
            TITLE,
            DEBUG_ENTITY_TEMPLATE,
            MAX
        };

        public enum SpriteDefs
        {
            BOSS01,
            BOSS03,
            BOSS04,
            BOSS05,
            MAX
        };

        public static Effect ShdTransition, ShdHueShift, ShdBinary;

        public static GraphicsDevice GraphicsDevice;
        public static GraphicsDeviceManager GraphicsDeviceManager;
        public static Camera Camera;
        public static InputController InputController;
        public static World World;
        public static EntityManager EntityManager;
        public static AudioManager AudioManager;
        public static TextManager TextManager;
        public static GameMenu GameMenu;
        public static SaveData SaveData;
        public static GameRNG GameRNG;
        public static SpriteBatch SpriteBatch;
        public static TextureManager TextureManager;

        public static Languages CurrLang = Languages.English;

        public static SpriteDefManager SpriteDefManager { get; set; }
    }
}
