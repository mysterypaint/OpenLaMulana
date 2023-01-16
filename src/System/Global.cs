using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Audio;
using OpenLaMulana.Entities;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;
using static OpenLaMulana.Main;

namespace OpenLaMulana
{
    public static class Global
    {
        public enum DrawOrder {
            Abstract = 0,
            Background = 100,
            Tileset = 200,
            AboveTilesetGraphicDisplay = 201,
            Entities = 300,
            AboveEntitiesGraphicDisplay = 301,
            Characters = 400,
            Protag = 500,
            Foreground = 600,
            UI = 700,
            Overlay = 800,
            Text = 900,
            Max = 10000,
        }

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
            MSX_OPEN,
            MSX_ROMS,
            MSX_LOADING_FILE,
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

        // Ordered by the asset values in Content/data/
        public enum SpriteDefs
        {
            BOSS01,
            BOSS03,
            BOSS04,
            BOSS05,
            MAX
        };

        // Ordered by the asset values in Content/data/
        public enum BossIDs {
            AMPHISBAENA = 0,
            SAKIT = 1,
            ELLMAC = 2,
            BAHAMUT = 3,
            VIY = 4,
            PALENQUE = 5,
            BAPHOMET = 6,
            TIAMAT = 7,
            MOTHER = 8,
            MAX
        };

        public enum WEStates {
            UNSPAWNED,
            INIT,
            ACTIVATING,
            IDLE,
            ATTACKING,
            DYING,
            SPEEDING_UP,
            MAX
        }

        public enum Weapons
        {
            NONE,
            WHIP,
            CHAIN_WHIP,
            FLAIL_WHIP,
            KNIFE,
            KEYBLADE_BETA,
            AXE,
            KATANA,
            KEYBLADE,
            MAX
        };

        public enum SubWeapons
        {
            NONE,
            SHURIKEN,
            THROWING_KNIFE,
            SPEAR,
            FLARES,
            BOMB,
            PISTOL,
            WEIGHT,
            ANKH_JEWEL,
            BUCKLER,
            HANDY_SCANNER,
            SILVER_SHIELD,
            ANGEL_SHIELD,
            PISTOL_AMMUNITION,
            MAX
        };

        enum ObtainableTreasures
        {
            MSX,
            SHELL_HORN,
            WATERPROOF_CASE,
            HEATPROOF_CASE,
            DETECTOR,
            GRAIL,
            LAMP_OF_TIME,
            BODY_ARMOR,
            TALISMAN,
            SCRIPTURE,
            GAUNTLET,
            RING,
            GLOVE,
            ENDLESS_KEY,
            TWIN_STATUE,
            BRONZE_MIRROR,
            BOOTS,
            FEATHER,
            BRACELET,
            DRAGON_BONE,
            GRAPPLE_CLAW,
            MAGATAMA_JEWEL,
            CROSS,
            BOOK_OF_THE_DEAD,
            PERFUME,
            OCARINA,
            ANCHOR,
            WOMAN_STATUE,
            MINI_DOLL,
            EYE_OF_TRUTH,
            SERPENT_STAFF,
            ICE_CAPE,
            HELMET,
            SCALESPHERE,
            CRYSTAL_SKULL,
            WEDGE,
            PLANE_MODEL,
            FLYWHEEL,
            POCHETTE_KEY,
            CONTAINER,
            MSX2,
            DIARY,
            MULANA_TALISMAN,
            LAMP_OF_TIME_EMPTY,
            WOMAN_STATUE_PREGNANT,
            HANDY_SCANNER,
            PEPPER,
            TREASURE,
            CONTAINER_YELLOW,
            CONTAINER_GREEN,
            CONTAINER_RED,
            FAKE_SHIELD,
            LAMULANA_TREASURE,
            LIFE_JEWEL,
            MAP,
            ORIGIN_SIGIL,
            BIRTH_SIGIL,
            LIFE_SIGIL,
            DEATH_SIGIL,
            HELL_TREASURE
        };

        public enum ObtainableSoftware
        {
            GAME_MASTER,
            GAME_MASTER_2,
            GLYPH_READER,
            RUINS_RAM_8K,
            RUINS_RAM_16K,
            UNRELEASED_ROM,
            PR3,
            GR3,
            ATHLETIC_LAND,
            ANTARCTIC_ADVENTURE,
            MONKEY_ACADEMY,
            TIME_PILOT,
            FROGGER,
            SUPER_COBRA,
            VIDEO_HUSTLER,
            MAHJONG_DOJO,
            HYPER_OLYMPIC_1,
            HYPER_OLYMPIC_2,
            HYPER_OLYMPIC_3,
            CIRCUS_CHARLIE,
            MAGICAL_TREE,
            COMIC_BAKERY,
            HYPER_SPORTS_1,
            HYPER_SPORTS_2,
            HYPER_SPORTS_3,
            CABBAGE_PATCH_KIDS,
            HYPER_RALLY,
            KONAMI_TENNIS,
            SKY_JAGUAR,
            KONAMI_PINBALL,
            KONAMI_GOLF,
            KONAMI_BASEBALL,
            YIE_AR_KUNG_FU,
            KINGS_VALLEY,
            MOPI_RANGER,
            PIPPOLS,
            ROAD_FIGHTER,
            KONAMI_PINGPONG,
            KONAMI_SOCCER,
            GOONIES,
            KONAMI_BOXING,
            YIE_AR_KUNG_FU_2,
            KNIGHTMARE,
            TWINBEE,
            SHIN_SYNTHESIZER,
            GRADIUS,
            PENGUIN_ADVENTURE,
            CASTLEVANIA,
            KING_KONG_2,
            QBERT,
            FIREBIRD,
            GANBARE_GOEMON,
            MAZE_OF_GALIOUS,
            METAL_GEAR,
            GRADIUS_2,
            F1_SPIRIT,
            USAS,
            SHALOM,
            BREAK_SHOT,
            PENNANT_RACE,
            SALAMANDER,
            PARODIUS,
            SEAL_OF_EL_GIZA,
            CONTRA,
            HEAVEN_AND_EARTH,
            NEMESIS_3,
            MAHJONG_WIZARD,
            PENNANT_RACE_2,
            METAL_GEAR_2,
            SPACE_MANBOW,
            QUARTH,
            KINGS_VALLEY_DISK,
            DIVINER_SENSATION,
            SNATCHER,
            F1_SPIRIT_3D,
            GAME_COLLECTION_1,
            GAME_COLLECTION_2,
            GAME_COLLECTION_3,
            GAME_COLLECTION_4,
            GAME_COLLECTION_EX,
            SD_SNATCHER,
            BADLANDS,
            GRADIUS_2_BETA,
            A1_SPIRIT,
            MAX
        };

        public enum RomCombos
        {
            RUINS8K_16K,        // Detailed map display in MSX Emulator
            UNREL_GR3,          // Mukimuki SD: Memorial Minigame in MSX Emulator
            PR3_GR3,            // PR3 Minigame in MSX Emulator
            ATHL_CABB,          // Player iframes dramatically increased
            ANTA_COMIC,         // Warp to backside fields
            VID_HUST_BREAKSHOT, // Knife and Keyblade Attack +2
            HYPERRAL_ROADF,     // Communicate with hidden Laptops
            YIEARKUNG_1AND2,    // Turns weights into Oolong Tea
            KNIGHT_MAZE,        // Small VIT rewarded immediately before death. Occurs only once.
            TWINB_GR2,          // Turns coins into bells
            SHINS_SNATC,        // Unlocks Music Mode Plus
            SHINS_SDSNATC,      // Unlocks Music Mode Full
            PENG_GR2,           // Coins become fish
            CASTLV_MAHJONGWIZ,  // Whip Attack +2
            KKONG2_FBIRD,       // Faster cooldown time for fairy spawn point and the Lamp of Time
            QB_DIVINER,         // Guaranteed key fairy
            MAZE_ELGIZ,         // Coins reward +1 than normal
            METALG_1AND2,       // Exclamation over protag whenever puzzle is solved
            GR2_SALAM,          // Slightly-altered credits
            SHALO_DIVIN,        // Guaranteed blue fairy (healing)
            CONTR_F1_SPIR,      // VIT rapidly decreases
            F1_SPIR_CONT,       // OHKO
            BADL_A1SPR,         // Displays Jukebox password [Or allows for in-game jukebox]
            MAX
        };

        public enum ControllerTypes
        {
            Keyboard,
            Gamepad,
            Max
        };

        public enum ControllerKeys
        {
            UP,
            DOWN,
            LEFT,
            RIGHT,
            JOY_UP,
            JOY_DOWN,
            JOY_LEFT,
            JOY_RIGHT,
            CONFIG_MENU_UP,
            CONFIG_MENU_DOWN,
            CONFIG_MENU_LEFT,
            CONFIG_MENU_RIGHT,
            JUMP,
            WHIP,
            SUB_WEAPON,
            ITEM,
            MENU_OPEN_INVENTORY,
            PAUSE,
            SUB_SHIFT_LEFT,
            SUB_SHIFT_RIGHT,
            MAIN_WEAPON_SHIFT_LEFT,
            MAIN_WEAPON_SHIFT_RIGHT,
            MENU_CONFIRM,
            MENU_CANCEL,
            MENU_MOVE_LEFT,
            MENU_MOVE_RIGHT,
            MENU_OPEN_MSX_ROM_SELECTION,
            MENU_OPEN_MSX_EMULATOR,
            MENU_OPEN_CONFIG,
            MAX
        };

        public enum MSXStates {
            INACTIVE,
            INVENTORY,
            ROM_SELECTION,
            EMULATOR,
            CONFIG_SCREEN,
            SCANNING,
            MAX
        }
        public enum HardCodedText
        {
            ROM_NAMES_BEGIN = 0,
            ROM_NAMES_END = 83,
            SAVE_LOAD_DIALOGUE_BEGINS = 84,
            SAVE_LOAD_DIALOGUE_ENDS = 93,
            ITEM_ACQUISITION_MESSAGE = 94,
            SPAWN_POINT = 99,
            JUKEBOX_NAMES_BEGIN = 100,
            JUKEBOX_NAMES_END = 155,
            ITEM_NAMES_BEGIN = 500,
            ITEM_NAMES_END = 559,
        };

        public enum PlatformingPhysics
        {
            CLASSIC,
            REVAMPED,
            MAX
        };

        public static Effect ShdTransition, ShdHueShift, ShdBinary, ShdMaskingBlack;

        public static GraphicsDevice GraphicsDevice;
        public static GraphicsDeviceManager GraphicsDeviceManager;
        public static Camera Camera;
        public static InputManager GlobalInput;
        public static World World;
        public static EntityManager EntityManager;
        public static AudioManager AudioManager;
        public static TextManager TextManager;
        public static GameMenu GameMenu;
        public static SaveData SaveData;
        public static GameRNG GameRNG;
        public static SpriteBatch SpriteBatch;
        public static TextureManager TextureManager;
        public static Protag Protag;
        public static Jukebox Jukebox;
        public static GameFlags GameFlags;

        public static Languages CurrLang = Languages.English;

        public static SpriteDefManager SpriteDefManager { get; set; }
        public static AnimationTimer AnimationTimer { get; internal set; }
        public static Main Main { get; internal set; }
        public static MobileSuperX MobileSuperX { get; set; }
        public static PlatformingPhysics ProtagPhysics { get; internal set; } = PlatformingPhysics.REVAMPED;
        public static bool QoLChanges { get; internal set; } = false;


        public static Dictionary<Weapons, int> WeaponsDamageTable = new Dictionary<Weapons, int>();
        public static Dictionary<SubWeapons, int> SubWeaponsDamageTable = new Dictionary<SubWeapons, int>();
        public static Dictionary<RomCombos, int> RomDamageMultipliers = new Dictionary<RomCombos, int>();
    }
}
