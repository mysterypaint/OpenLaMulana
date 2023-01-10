using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenLaMulana.System.InputManager;
using static System.Net.Mime.MediaTypeNames;

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







        /// <summary>
        /// Inputs a key to the textbox.
        /// </summary>
        /// <param name="Key">The key to input.</param>
        
        /*
        protected void InputKey(Keys Key)
        {
            if (Key == Keys.Back)
            {
                if (Text.Length > 0)
                    Text = Text.Substring(0, Text.Length - 1);

                return;
            }

            var Char = KeyToChar(Key, Shift);

            if (Char != 0)
                Text += Char;
        }*/

        /// <summary>
        /// Converts a key to a char.
        /// </summary>
        /// <param name="Key">They key to convert.</param>
        /// <param name="Shift">Whether or not shift is pressed.</param>
        /// <returns>The key in a char.</returns>
        public static Char KeyToChar(Keys Key, bool Shift = false)
        {
            /* It's the space key. */
            if (Key == Keys.Space)
            {
                return ' ';
            }
            else
            {
                string String = Key.ToString();

                /* It's a letter. */
                if (String.Length == 1)
                {
                    Char Character = Char.Parse(String);
                    byte Byte = Convert.ToByte(Character);

                    if (
                        (Byte >= 65 && Byte <= 90) ||
                        (Byte >= 97 && Byte <= 122)
                        )
                    {
                        return (!Shift ? Character.ToString().ToLower() : Character.ToString())[0];
                    }
                }

                /* 
                 * 
                 * The only issue is, if it's a symbol, how do I know which one to take if the user isn't using United States international?
                 * Anyways, thank you, for saving my time
                 * down here:
                 */

                #region Credits :  http://roy-t.nl/2010/02/11/code-snippet-converting-keyboard-input-to-text-in-xna.html for saving my time.
                switch (Key)
                {
                    case Keys.D0:
                        if (Shift) { return ')'; } else { return '0'; }
                    case Keys.D1:
                        if (Shift) { return '!'; } else { return '1'; }
                    case Keys.D2:
                        if (Shift) { return '@'; } else { return '2'; }
                    case Keys.D3:
                        if (Shift) { return '#'; } else { return '3'; }
                    case Keys.D4:
                        if (Shift) { return '$'; } else { return '4'; }
                    case Keys.D5:
                        if (Shift) { return '%'; } else { return '5'; }
                    case Keys.D6:
                        if (Shift) { return '^'; } else { return '6'; }
                    case Keys.D7:
                        if (Shift) { return '&'; } else { return '7'; }
                    case Keys.D8:
                        if (Shift) { return '*'; } else { return '8'; }
                    case Keys.D9:
                        if (Shift) { return '('; } else { return '9'; }

                    case Keys.NumPad0: return '0';
                    case Keys.NumPad1: return '1';
                    case Keys.NumPad2: return '2';
                    case Keys.NumPad3: return '3';
                    case Keys.NumPad4: return '4';
                    case Keys.NumPad5: return '5';
                    case Keys.NumPad6: return '6';
                    case Keys.NumPad7: return '7'; ;
                    case Keys.NumPad8: return '8';
                    case Keys.NumPad9: return '9';

                    case Keys.OemTilde:
                        if (Shift) { return '~'; } else { return '`'; }
                    case Keys.OemSemicolon:
                        if (Shift) { return ':'; } else { return ';'; }
                    case Keys.OemQuotes:
                        if (Shift) { return '"'; } else { return '\''; }
                    case Keys.OemQuestion:
                        if (Shift) { return '?'; } else { return '/'; }
                    case Keys.OemPlus:
                        if (Shift) { return '+'; } else { return '='; }
                    case Keys.OemPipe:
                        if (Shift) { return '|'; } else { return '\\'; }
                    case Keys.OemPeriod:
                        if (Shift) { return '>'; } else { return '.'; }
                    case Keys.OemOpenBrackets:
                        if (Shift) { return '{'; } else { return '['; }
                    case Keys.OemCloseBrackets:
                        if (Shift) { return '}'; } else { return ']'; }
                    case Keys.OemMinus:
                        if (Shift) { return '_'; } else { return '-'; }
                    case Keys.OemComma:
                        if (Shift) { return '<'; } else { return ','; }
                }
                #endregion

                return (Char)0;

            }
        }

        internal static string ButtonToString(Buttons input)
        {
            ControllerNames? idenfitiedController = InputManager.GetIdentifiedController();

            switch (input)
            {
                case Buttons.A:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Face Down";
                        case ControllerNames.PS4:
                        case ControllerNames.PS5:
                            return "Cross";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "B";
                    }
                case Buttons.B:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Face Right";
                        case ControllerNames.PS4:
                        case ControllerNames.PS5:
                            return "Circle";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "A";
                    }
                case Buttons.X:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Face Left";
                        case ControllerNames.PS4:
                        case ControllerNames.PS5:
                            return "Square";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "Y";
                    }
                case Buttons.Y:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Face Up";
                        case ControllerNames.PS4:
                        case ControllerNames.PS5:
                            return "Triangle";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "X";
                    }
                case Buttons.DPadLeft:
                    return "D-Pad Left";
                case Buttons.DPadRight:
                    return "D-Pad Right";
                case Buttons.DPadUp:
                    return "D-Pad Up";
                case Buttons.DPadDown:
                    return "D-Pad Down";
                case Buttons.LeftTrigger:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Left Trigger";
                        case ControllerNames.PS4:
                        case ControllerNames.PS5:
                            return "L2";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "ZL";
                    }
                case Buttons.RightTrigger:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Right Trigger";
                        case ControllerNames.PS4:
                        case ControllerNames.PS5:
                            return "R2";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "ZR";
                    }
                case Buttons.LeftShoulder:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Left Shoulder";
                        case ControllerNames.PS4:
                        case ControllerNames.PS5:
                            return "L1";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "L";
                    }
                case Buttons.RightShoulder:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Right Shoulder";
                        case ControllerNames.PS4:
                        case ControllerNames.PS5:
                            return "R1";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "R";
                    }
                case Buttons.Start:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Start";
                        case ControllerNames.PS4:
                        case ControllerNames.PS5:
                            return "Options";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "Plus";
                    }
                case Buttons.Back:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Select";
                        case ControllerNames.XBOX_360:
                        case ControllerNames.XBOX_ONE:
                            return "Back";
                        case ControllerNames.PS4:
                            return "Share";
                        case ControllerNames.PS5:
                            return "Create";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "Minus";
                    }
            }

            return "[None]";
        }
    }
}
