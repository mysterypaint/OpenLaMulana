using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenLaMulana.Entities;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenLaMulana.Global;
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
        public static String KeyToString(Keys Key, bool Shift = false)
        {
            /* It's the space key. */
            if (Key == Keys.Space)
            {
                return "[Space]";
            }
            else if (Key == Keys.LeftControl)
            {
                return "Ctrl Left";
            }
            else if (Key == Keys.RightControl)
            {
                return "Ctrl Right";
            }
            else if (Key == Keys.LeftAlt)
            {
                return "Left Alt";
            }
            else if (Key == Keys.RightAlt)
            {
                return "Right Alt";
            }
            else if (Key == Keys.RightShift)
            {
                return "Shift Right";
            }
            else if (Key == Keys.LeftShift)
            {
                return "Shift Left";
            }
            else if (Key == Keys.Up)
            {
                return "Up";
            }
            else if (Key == Keys.Down)
            {
                return "Down";
            }
            else if (Key == Keys.Left)
            {
                return "Left";
            }
            else if (Key == Keys.Right)
            {
                return "Right";
            }
            else if (Key == Keys.Enter)
            {
                return "Enter";
            }
            else if (Key == Keys.Tab)
            {
                return "Tab";
            }
            else if (Key == Keys.CapsLock)
            {
                return "CapsLock";
            }
            else if (Key == Keys.LeftWindows)
            {
                return "Win Left";
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
                        return "" + (!Shift ? Character.ToString().ToUpper() : Character.ToString())[0];
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
                        if (Shift) { return ")"; } else { return "0"; }
                    case Keys.D1:
                        if (Shift) { return "!"; } else { return "1"; }
                    case Keys.D2:
                        if (Shift) { return "@"; } else { return "2"; }
                    case Keys.D3:
                        if (Shift) { return "#"; } else { return "3"; }
                    case Keys.D4:
                        if (Shift) { return "$"; } else { return "4"; }
                    case Keys.D5:
                        if (Shift) { return "%"; } else { return "5"; }
                    case Keys.D6:
                        if (Shift) { return "^"; } else { return "6"; }
                    case Keys.D7:
                        if (Shift) { return "&"; } else { return "7"; }
                    case Keys.D8:
                        if (Shift) { return "*"; } else { return "8"; }
                    case Keys.D9:
                        if (Shift) { return "("; } else { return "9"; }

                    case Keys.NumPad0: return "0";
                    case Keys.NumPad1: return "1";
                    case Keys.NumPad2: return "2";
                    case Keys.NumPad3: return "3";
                    case Keys.NumPad4: return "4";
                    case Keys.NumPad5: return "5";
                    case Keys.NumPad6: return "6";
                    case Keys.NumPad7: return "7";
                    case Keys.NumPad8: return "8";
                    case Keys.NumPad9: return "9";

                    case Keys.OemTilde:
                        if (Shift) { return "~"; } else { return "`"; }
                    case Keys.OemSemicolon:
                        if (Shift) { return ":"; } else { return ";"; }
                    case Keys.OemQuotes:
                        if (Shift)
                        {
                            return "\"";
                        } else { return "\'"; }
                    case Keys.OemQuestion:
                        if (Shift) { return "?"; } else { return "/"; }
                    case Keys.OemPlus:
                        if (Shift) { return "+"; } else { return "="; }
                    case Keys.OemPipe:
                        if (Shift) { return "|"; } else { return "\\"; }
                    case Keys.OemPeriod:
                        if (Shift) { return ">"; } else { return "."; }
                    case Keys.OemOpenBrackets:
                        if (Shift) { return "{"; } else { return "["; }
                    case Keys.OemCloseBrackets:
                        if (Shift) { return "}"; } else { return "]"; }
                    case Keys.OemMinus:
                        if (Shift) { return "_"; } else { return "-"; }
                    case Keys.OemComma:
                        if (Shift) { return "<"; } else { return ","; }
                }
                #endregion

                return "[None]";
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
                case Buttons.LeftThumbstickLeft:
                    return "LX-";
                case Buttons.LeftThumbstickRight:
                    return "LX+";
                case Buttons.LeftThumbstickUp:
                    return "LY-";
                case Buttons.LeftThumbstickDown:
                    return "LY+";
                case Buttons.RightThumbstickLeft:
                    return "RX-";
                case Buttons.RightThumbstickRight:
                    return "RX+";
                case Buttons.RightThumbstickUp:
                    return "RY-";
                case Buttons.RightThumbstickDown:
                    return "RY+";
                case Buttons.LeftStick:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Left Stick Button";
                        case ControllerNames.PS4:
                        case ControllerNames.PS5:
                            return "L3";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "SwitchStickButtonIconLeft";
                    }
                case Buttons.RightStick:
                    switch (idenfitiedController)
                    {
                        default:
                            return "Right Stick Button";
                        case ControllerNames.PS4:
                        case ControllerNames.PS5:
                            return "R3";
                        case ControllerNames.NINTENDO_SWITCH:
                            return "SwitchStickButtonIconRight";
                    }
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

        public static float Lerp(float start_value, float end_value, float pct)
        {
            return (start_value + (end_value - start_value) * pct);
        }

        public static Vector2 GetOPCoords(int opValue)
        {
            int[] digits = GetDigits(opValue).ToArray();
            int numDigits = digits.Count();
            int yVal;
            if (numDigits < 2)
            {
                yVal = digits[numDigits - 1] * 1;
                return new Vector2(0, yVal);
            }
            else if (numDigits < 3)
            {
                yVal = digits[numDigits - 1] * 1 + digits[numDigits - 2] * 10;
                return new Vector2(0, yVal);
            }
            else
                yVal = digits[numDigits - 1] * 1 + digits[numDigits - 2] * 10 + digits[numDigits - 3] * 100;

            if (numDigits < 4)
                return new Vector2(0, yVal);

            int xVal;
            if (numDigits < 5)
                xVal = digits[0] * 1;
            else if (numDigits < 6)
                xVal = digits[0] * 10 + digits[1] * 1;
            else
                xVal = digits[0] * 100 + digits[1] * 10 + digits[2] * 1;

            return new Vector2(xVal, yVal);
        }

        internal static bool EntityMaySpawn(List<ObjectStartFlag> startFlags)
        {
            foreach (ObjectStartFlag flag in startFlags)
            {
                int flagIndex = flag.GetIndex();
                if (flagIndex == -1 || flagIndex >= (int)GameFlags.Flags.MAX)
                    continue;

                bool currFlagValue = Global.GameFlags.InGameFlags[flagIndex];

                bool conditionMetIfFlagIsOn = flag.GetFlagCondition();
                if (conditionMetIfFlagIsOn)
                {
                    currFlagValue = !currFlagValue;
                }

                if (currFlagValue)
                    return false;

            }

            return true;
        }


        #region SaveDataFunctions
        public static SaveData LoadSaveFromFile(string fileName)
        {
            // All EncryptionBlocks are Encrypted, then decrypted in place, with the exception of the RomBlock.

            EncryptionBlock[] allBlocks = new EncryptionBlock[(int)SaveRegions.MAX];
            int[] blockSizes = { 870, 60, 40, 24, 5, 24, 10, 20, 1, 2, 2, 4, 4, 336, 4, 20 };
            byte globalChecksum;

            using (BinaryReader reader = new BinaryReader(new FileStream(fileName, FileMode.Open)))
            {
                int bufferOffset = 0;
                for (SaveRegions eB = (SaveRegions)0; eB < SaveRegions.MAX; eB++)
                {
                    byte key = reader.ReadByte();
                    bufferOffset++;

                    byte[] data = new byte[blockSizes[(int)eB]];
                    reader.Read(data, 0, blockSizes[(int)eB]);
                    bufferOffset += blockSizes[(int)eB];
                    reader.BaseStream.Seek(bufferOffset, SeekOrigin.Begin);

                    byte checksum = reader.ReadByte();
                    bufferOffset++;


                    allBlocks[(int)eB] = new EncryptionBlock(key, data, checksum);
                }

                globalChecksum = reader.ReadByte();
            }
            SaveData encryptedSave = new SaveData(false, allBlocks, globalChecksum);

            return encryptedSave;
        }

        public static SaveData DecryptSaveFile(SaveData encryptedSave)
        {
            EncryptionBlock[] allDecryptedBlocks = new EncryptionBlock[(int)SaveRegions.MAX];

            for (SaveRegions eB = (SaveRegions)0; eB < SaveRegions.MAX; eB++)
            {
                // All EncryptionBlocks are Encrypted, then decrypted in place
                allDecryptedBlocks[(int)eB] = DecryptBlock(encryptedSave.SaveBlocks[(int)eB]);
            }

            byte globalChecksum = GetFinalChecksum(allDecryptedBlocks);

            SaveData decryptedSave = new SaveData(true, allDecryptedBlocks, globalChecksum);

            return decryptedSave;
        }


        /// TODO: Implement EncryptSaveFile (Further research may be required...)
        // worsety: "0x4801A0 creates the save file, with multiple calls to 0x47FF50 for the obfuscation"
        // Research in a disasm

        public static SaveData EncryptSaveFile(SaveData decryptedSave)
        {

            EncryptionBlock[] allEncryptedBlocks = new EncryptionBlock[(int)SaveRegions.MAX];
            /*
             * 
            for (EncryptionBlocks eB = (EncryptionBlocks)0; eB < EncryptionBlocks.MAX; eB++)
            {
                // All EncryptionBlocks are Encrypted, then decrypted in place
                allEncryptedBlocks[(int)eB] = EncryptBlock(decryptedSave.SaveBlocks[(int)eB]);
            }
*/

            byte globalChecksum = GetFinalChecksum(allEncryptedBlocks);

            SaveData encryptedSave = new SaveData(true, allEncryptedBlocks, globalChecksum);

            return encryptedSave;
        }

        private static EncryptionBlock EncryptBlock(EncryptionBlock block)
        {
            byte state = block.Key;
            byte checksum = 0;

            for (int i = 0; i < block.Data.Length; i++)
            {
                state = (byte)(109 * state + 1021); // Implicit (& 255) performed on this too
                block.Data[i] ^= state;
                checksum += (byte)(i + block.Data[i]);
            }
            
            /*
            if (checksum != block.Checksum)
                throw new Exception("Bad checksum error while encrypting this save block!");*/

            return new EncryptionBlock(state, block.Data, checksum);
        }

        private static EncryptionBlock DecryptBlock(EncryptionBlock block)
        {
            byte state = block.Key;
            byte checksum = 0;

            for (int i = 0; i < block.Data.Length; i++)
            {
                state = (byte)(109 * state + 1021); // Implicit (& 255) performed on this too
                block.Data[i] ^= state;
                checksum += (byte)(i + block.Data[i]);
            }

            if (checksum != block.Checksum)
                throw new Exception("Bad checksum error while decrypting this save block!");

            return new EncryptionBlock(state, block.Data, checksum);
        }

        public static byte GetFinalChecksum(EncryptionBlock[] encBlkArray)
        {
            byte finalValue = 0;

            for (int i = 0; i < encBlkArray.Length; i++)
            {
                finalValue += (byte)(encBlkArray[i].Checksum + encBlkArray[i].Key);
            }

            return finalValue;
        }

        public static bool GetBit(byte b, int bitNumber)
        {
            return (b & (1 << bitNumber - 1)) != 0;
        }

        public static int GetWordAsInt(byte[] bytes)
        {
            return bytes[0] | bytes[1] << 8;
        }

        internal static void WriteSaveToFile(SaveData decryptedSave, string fileName, bool encryptThisSave = true)
        {
            if (encryptThisSave)
            {
                if (decryptedSave.IsDecrypted)
                {
                    SaveData encryptedSave = EncryptSaveFile(decryptedSave);
                    decryptedSave = encryptedSave;
                }
            }

            byte globalChecksum = decryptedSave.GlobalChecksum;
            EncryptionBlock[] saveBlocks = decryptedSave.SaveBlocks;

            try
            {
                using (var fs = new FileStream(fileName, FileMode.Create, FileAccess.Write))
                {
                    foreach(EncryptionBlock b in saveBlocks)
                    {
                        fs.WriteByte(b.Key);
                        fs.Write(b.Data, 0, b.Data.Length);
                        fs.WriteByte(b.Checksum);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception("Exception caught in process: {0}", ex);
            }
        }

        internal static void ParseSaveData(SaveData decryptedSave)
        {
            byte _globalChecksum = decryptedSave.GlobalChecksum;
            EncryptionBlock[] saveBlocks = decryptedSave.SaveBlocks;

            byte[] flags = saveBlocks[(int)SaveRegions.Flags].Data;
            byte[] treasures = saveBlocks[(int)SaveRegions.Treasures].Data;
            byte[] treasuresMenu = saveBlocks[(int)SaveRegions.TreasuresMenu].Data;
            byte[] blockD = saveBlocks[(int)SaveRegions.BlockD].Data;
            byte[] mainWeapons = saveBlocks[(int)SaveRegions.MainWeapons].Data;
            byte[] blockF = saveBlocks[(int)SaveRegions.BlockF].Data;
            byte[] subWeapons = saveBlocks[(int)SaveRegions.SubWeapons].Data;
            byte[] ammo = saveBlocks[(int)SaveRegions.Ammo].Data;
            byte[] maxHP32 = saveBlocks[(int)SaveRegions.MaxHP32].Data;
            byte[] coins = saveBlocks[(int)SaveRegions.Coins].Data;
            byte[] weights = saveBlocks[(int)SaveRegions.Weights].Data;
            byte[] gameTime = saveBlocks[(int)SaveRegions.GameTime].Data;
            byte[] blockM = saveBlocks[(int)SaveRegions.BlockM].Data;
            byte[] roms = saveBlocks[(int)SaveRegions.Roms].Data;
            byte[] blockO = saveBlocks[(int)SaveRegions.BlockO].Data;
            byte[] blockP = saveBlocks[(int)SaveRegions.BlockP].Data;

            Global.Inventory.HPMax = maxHP32[0] * 32;
            Global.Inventory.HP = maxHP32[0] * 32;
            Global.Inventory.CoinCount = GetWordAsInt(coins);
            Global.Inventory.WeightCount = GetWordAsInt(weights);
            Global.Inventory.EquippedRoms = new Global.ObtainableSoftware[] { Global.ObtainableSoftware.NONE, Global.ObtainableSoftware.NONE };
            Global.Inventory.EquippedMainWeapon = Global.MainWeapons.NONE;
            Global.Inventory.EquippedSubWeapon = Global.SubWeapons.NONE;
            foreach (var treasure in Global.Inventory.ObtainedTreasures.Keys.ToList())
            {
                Global.Inventory.ObtainedTreasures[treasure] = false;
            }

            foreach (var software in Global.Inventory.ObtainedSoftware.Keys.ToList())
            {
                Global.Inventory.ObtainedSoftware[software] = false;
            }

            // Turn on any relevant SFLAGs
            int flagOffset = (int)GameFlags.Flags.TALKED_TO_XELPUD_FOR_THE_FIRST_TIME;
            foreach (byte b in flags)
            {
                for (var i = 0; i < 8; i++)
                {
                    Global.GameFlags.InGameFlags[flagOffset + i] = GetBit(b, i + 1);        // Could potentially be GetBit(b, 8 - i)...
                }

                flagOffset += 8;
            }

            // Give the player any items that they should have
            int j = 0;
            foreach (byte b in treasures)
            {
                Global.Inventory.ObtainedTreasures[(Global.ObtainableTreasures)j] = Convert.ToBoolean(b);
                j++;
            }

            j = 0;
            foreach (byte b in treasuresMenu)
            {
                Global.Inventory.TreasureIcons[j] = (Global.ObtainableTreasures)b;
                j++;
            }

            j = 0;
            foreach (byte b in mainWeapons)
            {
                if (b < 0x255)
                {
                    Global.Inventory.ObtainedMainWeapons[j] = (Global.MainWeapons)b;
                }
                else
                    Global.Inventory.ObtainedMainWeapons[j] = Global.MainWeapons.NONE;
                j++;
            }

            j = 0;
            foreach (byte b in subWeapons)
            {
                if (b < 0x255)
                {
                    Global.Inventory.ObtainedSubWeapons[j] = (Global.SubWeapons)b;
                }
                else
                    Global.Inventory.ObtainedSubWeapons[j] = Global.SubWeapons.NONE;
                j++;
            }


            for (int i = 0; i < ammo.Length; i += 2)
            {
                byte[] bytes = new byte[] { ammo[i], ammo[i + 1] };
                switch (i / 2)
                {
                    default:
                        break;
                    case 0:
                        Global.Inventory.ShurikenCount = GetWordAsInt(bytes);
                        break;
                    case 1:
                        Global.Inventory.ThrowingKnivesCount = GetWordAsInt(bytes);
                        break;
                    case 2:
                        Global.Inventory.SpearsCount = GetWordAsInt(bytes);
                        break;
                    case 3:
                        Global.Inventory.FlaresCount = GetWordAsInt(bytes);
                        break;
                    case 4:
                        Global.Inventory.BombsCount = GetWordAsInt(bytes);
                        break;
                    case 5:
                        Global.Inventory.BulletCount = GetWordAsInt(bytes);
                        break;
                    case 6:
                        Global.Inventory.AmmunitionRefills = GetWordAsInt(bytes);
                        break;
                    case 7:
                        Global.Inventory.AnkhJewelCount = GetWordAsInt(bytes);
                        break;
                    case 8:
                        Global.Inventory.ShieldValue = GetWordAsInt(bytes);
                        break;
                    case 9:
                        Global.Inventory.HandyScannerValue = GetWordAsInt(bytes);
                        break;
                }
            }

            for (Global.ObtainableSoftware software = 0; software < Global.ObtainableSoftware.MAX; software++)
            {
                Global.Inventory.ObtainedSoftware[software] = Convert.ToBoolean(roms[(int)software * 4]);
            }

            //(Int32)(BitConverter.ToInt16(array, 0))

            if (Global.QoLChanges)
            {
                Global.Inventory.ExpMax = Global.Inventory.HPMax;
            }
            else
                Global.Inventory.ExpMax = 88; // When this is 88, trigger and reset to 0

        }

        // Define where each icon should be displayed on the Inventory Screen
        private static Point[] _inventoryOrderTreasures = new Point[] {
            new Point(0,0),     // MSX1
            new Point(7,1),     // Shell Horn
            new Point(1,0),     // Waterproof Case
            new Point(2,0),     // Heatproof Case
            new Point(7,0),     // Detector
            new Point(1,2),     // Grail
            new Point(6,0),     // Lamp of Time (Active)
            new Point(4,0),     // Body Armor
            new Point(8,3),     // Talisman
            new Point(0,3),     // Scripture
            new Point(0,1),     // Gauntlet
            new Point(9,1),     // Ring
            new Point(5,3),     // Glove
            new Point(3,1),     // Endless Key
            new Point(4,3),     // Twin Statue
            new Point(7,3),     // Bronze Mirror
            new Point(3,0),     // Boots
            new Point(1,3),     // Feather
            new Point(2,3),     // Bracelet
            new Point(8,2),     // Dragon Bone
            new Point(0,2),     // Grapple Claw
            new Point(6,1),     // Magatama
            new Point(4,1),     // Cross
            new Point(3,2),     // Book of the Dead
            new Point(5,0),     // Perfume
            new Point(4,2),     // Ocarina
            new Point(6,2),     // Anchor
            new Point(0,9),     // Woman Statue (Infertile)
            new Point(2,1),     // Mini Doll
            new Point(3,3),     // Eye of Truth
            new Point(7,2),     // Serpent Staff
            new Point(3,2),     // Ice Cape
            new Point(5,1),     // Helmet
            new Point(1,1),     // Scalesphere
            new Point(6,3),     // Crystal Skull
            new Point(9,2),     // Wedge
            new Point(5,2),     // Plane Model
            new Point(8,1),     // Flywheel
            new Point(8,0),     // Pochette Key
            new Point(9,3),     // Container (Empty)
            new Point(0,0),     // MSX2
            new Point(8,3),     // Diary
            new Point(8,3),     // Mulana Talisman
            new Point(6,0),     // Lamp of Time (Empty)
            new Point(0,9),     // Woman Statue (Fertile)
            new Point(-1,-1),   // Handy Scanner
            new Point(6,2),     // Pepper
            new Point(6,2),     // Treasure
            new Point(9,3),     // Container (Medicine of Life)
            new Point(9,3),     // Container (Green Medicine)
            new Point(9,3),     // Container (Red Medicine)
            new Point(-1,-1),   // Silver Shield
            new Point(-1,-1),   // Treasure of La-Mulana
            new Point(-1,-1),   // Life Orb
            new Point(-1,-1),   // Map
            new Point(-1,-1),   // Origin Sigil
            new Point(-1,-1),   // Birth Sigil
            new Point(-1,-1),   // Life Sigil
            new Point(-1,-1),   // Death Sigil
            new Point(-1,-1)    // Skimpy Swimsuit
        };

        internal static void UpdateInventory(Global.ItemTypes itemType, int itemID, bool grantToPlayer = true, SFX sfx = SFX.P_ITEM_TAKEN)
        {
            switch(itemType)
            {
                default:
                case ItemTypes.UNKNOWN:
                    break;
                case ItemTypes.TREASURE:
                    Global.Inventory.ObtainedTreasures[(ObtainableTreasures)itemID] = grantToPlayer;
                    ObtainableTreasures thisTreasure = (ObtainableTreasures)itemID;

                    Point inventoryCoords = _inventoryOrderTreasures[itemID];

                    if (!inventoryCoords.Equals(new Point(-1, -1))) {
                        int coordsAsIndex = Math.Clamp(inventoryCoords.Y * 10 + inventoryCoords.X, 0, 39);
                        Global.Inventory.TreasureIcons[coordsAsIndex] = thisTreasure;
                    }

                    switch ((ObtainableTreasures)itemID)
                    {
                        default:
                            Global.AudioManager.PlaySFX(SFX.P_ITEM_TAKEN);
                            break;
                        case ObtainableTreasures.MAP:
                            Global.AudioManager.PlaySFX(SFX.P_ITEM_TAKEN);
                            break;
                        case ObtainableTreasures.LIFE_JEWEL:
                            Global.AudioManager.PlaySFX(SFX.P_MAJOR_ITEM_TAKEN);
                            Global.Inventory.HPMax += 32;
                            Global.Inventory.HP = Global.Inventory.HPMax;
                            break;
                        case ObtainableTreasures.FEATHER:
                            Global.AudioManager.PlaySFX(SFX.P_ITEM_TAKEN);
                            Global.Protag.SetJumpsMax(2);
                            break;
                    }
                    break;
                case ItemTypes.SUBWEAPON:
                    short subWeaponSlot = (short)itemID;
                    switch ((SubWeapons)itemID)
                    {
                        default:
                            Global.Inventory.ObtainedSubWeapons[subWeaponSlot] = (SubWeapons)itemID;
                            break;
                        case SubWeapons.BUCKLER:
                        case SubWeapons.SILVER_SHIELD:
                        case SubWeapons.ANGEL_SHIELD:
                            subWeaponSlot = 8;
                            if (Global.Inventory.ObtainedSubWeapons[subWeaponSlot] < (SubWeapons)itemID || Global.Inventory.ObtainedSubWeapons[subWeaponSlot] == SubWeapons.NONE)
                            {
                                Global.Inventory.ObtainedSubWeapons[subWeaponSlot] = (SubWeapons)itemID;
                                if (Global.Inventory.EquippedSubWeapon == SubWeapons.BUCKLER || Global.Inventory.EquippedSubWeapon == SubWeapons.SILVER_SHIELD || Global.Inventory.EquippedSubWeapon == SubWeapons.ANGEL_SHIELD)
                                {
                                    Global.Inventory.EquippedSubWeapon = (SubWeapons)itemID;
                                }
                            }
                            break;
                    }
                    Global.AudioManager.PlaySFX(sfx);
                    break;
                case ItemTypes.MAIN_WEAPON:
                    short mainWeaponSlot = 0;
                    switch ((MainWeapons)itemID)
                    {
                        default:
                            Global.Inventory.ObtainedMainWeapons[mainWeaponSlot] = (MainWeapons)itemID;
                            break;
                        case MainWeapons.WHIP:
                        case MainWeapons.FLAIL_WHIP:
                        case MainWeapons.CHAIN_WHIP:
                            mainWeaponSlot = 0;
                            if (Global.Inventory.ObtainedMainWeapons[mainWeaponSlot] < (MainWeapons)itemID || Global.Inventory.ObtainedMainWeapons[mainWeaponSlot] == MainWeapons.NONE)
                            {
                                Global.Inventory.ObtainedMainWeapons[mainWeaponSlot] = (MainWeapons)itemID;

                                if (Global.Inventory.EquippedMainWeapon == MainWeapons.WHIP || Global.Inventory.EquippedMainWeapon == MainWeapons.CHAIN_WHIP || Global.Inventory.EquippedMainWeapon == MainWeapons.FLAIL_WHIP)
                                    Global.Inventory.EquippedMainWeapon = (MainWeapons)itemID;
                            }
                            break;
                        case MainWeapons.KNIFE:
                            mainWeaponSlot = 1;
                            Global.Inventory.ObtainedMainWeapons[mainWeaponSlot] = (MainWeapons)itemID;
                            break;
                        case MainWeapons.KEYBLADE:
                        case MainWeapons.KEYBLADE_BETA:
                            mainWeaponSlot = 2;
                            if (Global.Inventory.ObtainedMainWeapons[mainWeaponSlot] < (MainWeapons)itemID || Global.Inventory.ObtainedMainWeapons[mainWeaponSlot] == MainWeapons.NONE)
                            {
                                Global.Inventory.ObtainedMainWeapons[mainWeaponSlot] = (MainWeapons)itemID;

                                if (Global.Inventory.EquippedMainWeapon == MainWeapons.KEYBLADE || Global.Inventory.EquippedMainWeapon == MainWeapons.KEYBLADE_BETA)
                                    Global.Inventory.EquippedMainWeapon = (MainWeapons)itemID;
                            }
                            break;
                        case MainWeapons.AXE:
                            mainWeaponSlot = 3;
                            Global.Inventory.ObtainedMainWeapons[mainWeaponSlot] = (MainWeapons)itemID;
                            break;
                        case MainWeapons.KATANA:
                            mainWeaponSlot = 4;
                            Global.Inventory.ObtainedMainWeapons[mainWeaponSlot] = (MainWeapons)itemID;
                            break;
                    }
                    Global.AudioManager.PlaySFX(sfx);
                    break;
                case ItemTypes.SOFTWARE:
                    Global.Inventory.ObtainedSoftware[(ObtainableSoftware)itemID] = grantToPlayer;
                    Global.AudioManager.PlaySFX(sfx);
                    break;
            }
        }

        internal static int[] SeparateDigits(int inputDigit, int[] args)
        {
            int[] returnArgs = new int[args.Length];

            int[] digits = GetDigits(inputDigit).ToArray();

            int digitsOffset = digits.Length - 1;
            int argIndex = 0;
            foreach (int arg in args)
            {
                int numArgDigits = arg;
                digitsOffset -= (numArgDigits - 1);
                int finalArgValue = 0;
                int baseFactor = (int)Math.Pow(10, numArgDigits - 1);


                for (int i = 0; i < numArgDigits; i++)
                {
                    if (digitsOffset < 0)
                    {
                        digitsOffset++;
                        baseFactor /= 10;
                        continue;
                    }
                    if (digitsOffset >= digits.Length)
                    {
                        break;
                    }
                    finalArgValue += digits[digitsOffset] * baseFactor;

                    baseFactor /= 10;
                    digitsOffset++;
                }
                digitsOffset -= (numArgDigits + 1);

                returnArgs[args.Length - 1 - argIndex] = finalArgValue;
                argIndex++;
            }

            return returnArgs;
        }
        #endregion

    }
}
