using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLaMulana.System
{
    class HelperFunctions
    {
        public static IEnumerable<int> GetDigits(int source)
        {
            int individualFactor = 0;
            int tennerFactor = Convert.ToInt32(Math.Pow(10, source.ToString().Length));
            do
            {
                source -= tennerFactor * individualFactor;
                tennerFactor /= 10;
                individualFactor = source / tennerFactor;

                yield return individualFactor;
            } while (tennerFactor > 1);
        }

        public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle rect, Color color)
        {
            spriteBatch.Draw(Global.TextureManager.GetWhitePixel(), rect, color);
        }

        public static void DrawRectangle(SpriteBatch spriteBatch, Vector2 position, Color color, Vector2 widthHeight)
        {
            spriteBatch.Draw(Global.TextureManager.GetWhitePixel(), position, null,
                    color, 0f, Vector2.Zero, widthHeight,
                    SpriteEffects.None, 0f);
        }

        internal static void DrawBlackSplashscreen(SpriteBatch spriteBatch, bool wholeScreen = true)
        {
            if (wholeScreen)
                DrawRectangle(spriteBatch, new Rectangle(0, 0, Main.WINDOW_WIDTH, Main.WINDOW_HEIGHT), Color.Black);
            else
                DrawRectangle(spriteBatch, new Rectangle(0, Main.HUD_HEIGHT, Main.WINDOW_WIDTH, Main.WINDOW_HEIGHT - Main.HUD_HEIGHT), Color.Black);
        }

        internal static void DrawSkyBlueSplashscreen(SpriteBatch spriteBatch, bool wholeScreen = true)
        {
            if (wholeScreen)
                DrawRectangle(spriteBatch, new Rectangle(0, 0, Main.WINDOW_WIDTH, Main.WINDOW_HEIGHT), new Color(51, 204, 255, 255));
            else
            {
                DrawRectangle(spriteBatch, new Rectangle(9, 9, 238, 171), new Color(51, 204, 255, 255));
                DrawRectangle(spriteBatch, new Rectangle(8, 181, 240, 1), new Color(255, 255, 255, 255));
                DrawRectangle(spriteBatch, new Rectangle(8, 183, 240, 1), new Color(51, 51, 255, 255));
                DrawRectangle(spriteBatch, new Rectangle(13, 16, 230, 157), new Color(51, 102, 255, 255));
            }
        }
    }
}
