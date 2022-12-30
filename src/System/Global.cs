using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
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
            MAX
        };

        public static Effect ShdTransition, ShdHueShift;

        public static Camera Camera;
        public static InputController InputController;
        public static World World;
        public static EntityManager EntityManager;
        public static AudioManager AudioManager;
        public static TextManager TextManager;
        public static GameMenu GameMenu;
        public static SaveData SaveData;
        public static GameRNG GameRNG;
        public static GraphicsDeviceManager GraphicsDeviceManager;
        public static SpriteBatch SpriteBatch;

        public static Languages CurrLang = Languages.English;
    };
}
