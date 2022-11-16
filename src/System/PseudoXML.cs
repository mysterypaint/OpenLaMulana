using OpenLaMulana.Entities;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Text.RegularExpressions;

namespace OpenLaMulana.System
{
    public class PseudoXML
    {
        private const int DECRYPTION_KEY = 0x61;

        internal static void GetGameDialogue(string v1, string v2, World world)
        {
            throw new NotImplementedException();
        }

        public static void DecodeScriptDat(string fileIn, string fileOut, Dictionary<int, string> charSet, World world, Main.Languages lang)
        {
            if (File.Exists(fileOut))
            {
                File.Delete(fileOut);
            }

            byte[] fDat = File.ReadAllBytes(fileIn);

            FileStream converted = new FileStream(fileOut, FileMode.CreateNew);

            using (StreamWriter writer = new StreamWriter(converted, Encoding.UTF8))
            {
                var i = 0;
                bool writingCommand = false;
                byte nextByte;
                bool talkingCommand = true;
                bool justLineBroke = false;
                foreach (byte b in fDat)
                {
                    byte currByte = (byte)(b ^ DECRYPTION_KEY);
                    bool alreadyWrote = false;
                    fDat[i] = currByte;

                    if (currByte == 0x3E) // >
                    {
                        if (fDat[i - 1] == 0x4B) // K
                        {
                            if (fDat[i - 2] == 0x4C) // L
                                if (fDat[i - 3] == 0x41) // A
                                    if (fDat[i - 4] == 0x54) // T
                                    {
                                        talkingCommand = true;
                                        if (fDat[i - 5] == 0x2F)
                                        { // /
                                            talkingCommand = false;
                                            writer.Write(charSet[currByte]);
                                        }
                                        else
                                            writer.Write(charSet[currByte] + "\n");
                                        alreadyWrote = true;
                                        justLineBroke = true;
                                    }
                        }
                    }
                    else if (currByte == 0x3C) // <
                    {
                        if (i > 0)
                        {
                            //if (!writingCommand)
                            writer.Write("\n");
                        }
                    }


                    if (!writingCommand && (currByte == 0x5C || (currByte <= 0xF && currByte != 0xA)))          // We ran into a '¥' char or special command. This is a special control code that must be written explicitly
                    {
                        writingCommand = true;
                    }
                    else if (ValueCapitalLowerCaseHiraganaOrKatakana(currByte) || currByte == 0x3C)     // If we hit a capital letter or '<'...
                    {
                        if (i < fDat.Length - 1)
                        { // Are we already at the end of the file?
                            nextByte = (byte)(fDat[i + 1] ^ DECRYPTION_KEY);
                            if (currByte == 0x3C)                           // We should check if the next char is a '<' char
                            {
                                if ((int)nextByte == 0x2F
                                    && (fDat[i + 2] ^ DECRYPTION_KEY) == 0x54
                                    && (fDat[i + 3] ^ DECRYPTION_KEY) == 0x41
                                    && (fDat[i + 4] ^ DECRYPTION_KEY) == 0x4C
                                    && (fDat[i + 5] ^ DECRYPTION_KEY) == 0x4B
                                    && (fDat[i + 6] ^ DECRYPTION_KEY) == 0x3E)                             // We should check if the next few chars are "/TALK>"
                                    writingCommand = false;
                            }
                            if (nextByte == 0x5C                     // another control code,
                                || nextByte == 0x20                     // a space,
                                || nextByte == 0x27                     // or an apostrophe
                                || ValueCapitalLowerCaseHiraganaOrKatakana(nextByte))// A capital/lowercase letter, and all of the non-kanji Hiragana/Katakana chars
                                writingCommand = false;                     // ...before we stop explicitly writing values as-is, instead of actual text characters

                        }
                        else
                        {
                            writingCommand = false;
                        }
                    }

                    // Perform a linebreak properly if we hit a \10 while we're not explicitly writing out <TALK> command data
                    if (currByte == 0xA)
                    {
                        if (talkingCommand)
                        {
                            /*
                            if (fDat[i - 1] != 0x3E &&      // >
                                fDat[i - 2] == 0x4B &&      // K
                                fDat[i - 3] == 0x4C &&      // L
                                fDat[i - 4] == 0x41 &&      // A
                                fDat[i - 5] == 0x54 &&      // T
                                fDat[i - 6] == 0x2F         // /
                                ) // Only write a new linebreak as "\10" if the previous text wasn't "</TALK>"
                            {
                                writer.Write("\\" + currByte.ToString());
                            }
                            */
                            if (fDat[i + 1] != 0x3E)
                            {
                                if (justLineBroke)
                                    justLineBroke = false;
                                else
                                    writer.Write("\\" + currByte.ToString());
                            }
                            else
                                writer.Write("\n");
                        }
                        else
                        {
                            //writer.Write("\n");
                        }
                    }
                    else
                    {
                        if (!alreadyWrote)
                        {
                            if (!writingCommand)
                                writer.Write(charSet[currByte]);
                            else
                                writer.Write("\\" + currByte.ToString());
                        }
                    }

                    i++;
                }
                writer.Write("\n");
            }

            PseudoXML.ParseGameDialogue(fileOut, world, lang);
        }

        public static void ParseGameDialogue(string inFile, World world, Main.Languages lang)
        {
            String inStr = File.ReadAllText(inFile, Encoding.UTF8);
            List<String> aResult = new List<String>();
            string regexPattern;

            regexPattern = "<TALK>[\r\n]*((.*?)[\r\n]*)*</TALK>+";

            foreach (Match match in Regex.Matches(inStr, regexPattern))
            {
                string groupStr = match.Groups[0].ToString();
                string filteredStr = groupStr.Substring(7, groupStr.Length - 15);
                aResult.Add(filteredStr);
            }

            switch (lang)
            {
                default:
                case Main.Languages.English:
                    world.InitGameText(Main.Languages.English, aResult);
                    break;
                case Main.Languages.Japanese:
                    world.InitGameText(Main.Languages.Japanese, aResult);
                    break;
            }
        }

        private static bool ValueCapitalLowerCaseHiraganaOrKatakana(byte testByte)
        {
            bool result = ((testByte >= 0x61 && testByte <= 0x7A)       // Lowercase letters
                            || (testByte >= 0x41 && testByte <= 0x5A)   // Uppercase letters
                            || (testByte >= 0x86 && testByte <= 0x8F)   // Hiragana part 1
                            || (testByte >= 0x91 && testByte <= 0x9F)
                            || (testByte >= 0xA7 && testByte <= 0xAF)
                            || (testByte >= 0xB1 && testByte <= 0xBF)
                            || (testByte >= 0xC0 && testByte <= 0xCF)
                            || (testByte >= 0xD0 && testByte <= 0xDD)
                            || (testByte >= 0xE0 && testByte <= 0xFD));
            return result;
        }

        public static Dictionary<int, string> DefineCharSet(Dictionary<int, string> charSet)
        {
            // Customized Shift-JIS; Manually defined for readability+portability (there's only a handful of characters & loaded on boot only once; Could probably do this iteratively but eh)

            Dictionary<int, string> s_charSet = charSet;

            s_charSet.Add(0x0, "\\0"); //␀
            s_charSet.Add(0x1, "\\1"); //␁
            s_charSet.Add(0x2, "\\2"); //␂
            s_charSet.Add(0x3, "\\3"); //␃
            s_charSet.Add(0x4, "\\4"); //␄
            s_charSet.Add(0x5, "\\5"); //␅
            s_charSet.Add(0x6, "\\6"); //␆
            s_charSet.Add(0x7, "\\7"); //␇
            s_charSet.Add(0x8, "\\8"); //␈
            s_charSet.Add(0x9, "\\9"); //␉
            //charSet.Add(0xA, "\\10"); //␊
            s_charSet.Add(0xA, "\n\r");
            s_charSet.Add(0xB, "\\11"); //␋
            s_charSet.Add(0xC, "\\12"); //␌
            s_charSet.Add(0xD, "\\13"); //␌
            s_charSet.Add(0xE, "\\14"); //␎
            s_charSet.Add(0xF, "\\15"); //␏

            s_charSet.Add(0x10, "\\16"); //Ｓ
            s_charSet.Add(0x11, "\\17"); //ｄ
            s_charSet.Add(0x12, "\\18"); //Ｏ
            s_charSet.Add(0x13, "\\19"); //新
            s_charSet.Add(0x14, "\\20"); //⑩
            s_charSet.Add(0x15, "\\21"); //倍
            s_charSet.Add(0x16, "\\22"); //母
            s_charSet.Add(0x17, "\\23"); //天
            s_charSet.Add(0x18, "\\24"); //道
            s_charSet.Add(0x19, "\\25"); //書
            s_charSet.Add(0x1A, "\\26"); //者
            s_charSet.Add(0x1B, "\\27"); //間
            s_charSet.Add(0x1C, "\\28"); //死
            s_charSet.Add(0x1D, "\\29"); //地
            s_charSet.Add(0x1E, "\\30"); //古
            s_charSet.Add(0x1F, "\\31"); //文

            s_charSet.Add(0x20, " ");
            s_charSet.Add(0x21, "!");
            s_charSet.Add(0x22, "\"");
            s_charSet.Add(0x23, "\\35"); //#
            s_charSet.Add(0x24, "\\36"); //$
            s_charSet.Add(0x25, "\\37"); //%
            s_charSet.Add(0x26, "&");
            s_charSet.Add(0x27, "\'");
            s_charSet.Add(0x28, "(");
            s_charSet.Add(0x29, ")");
            s_charSet.Add(0x2A, "*");
            s_charSet.Add(0x2B, "+");
            s_charSet.Add(0x2C, ",");
            s_charSet.Add(0x2D, "-");
            s_charSet.Add(0x2E, ".");
            s_charSet.Add(0x2F, "/");

            s_charSet.Add(0x30, "0");
            s_charSet.Add(0x31, "1");
            s_charSet.Add(0x32, "2");
            s_charSet.Add(0x33, "3");
            s_charSet.Add(0x34, "4");
            s_charSet.Add(0x35, "5");
            s_charSet.Add(0x36, "6");
            s_charSet.Add(0x37, "7");
            s_charSet.Add(0x38, "8");
            s_charSet.Add(0x39, "9");
            s_charSet.Add(0x3A, ":");
            s_charSet.Add(0x3B, ";");
            s_charSet.Add(0x3C, "<");
            s_charSet.Add(0x3D, "=");
            s_charSet.Add(0x3E, ">");
            s_charSet.Add(0x3F, "?");

            s_charSet.Add(0x40, "\\64"); //@
            s_charSet.Add(0x41, "A");
            s_charSet.Add(0x42, "B");
            s_charSet.Add(0x43, "C");
            s_charSet.Add(0x44, "D");
            s_charSet.Add(0x45, "E");
            s_charSet.Add(0x46, "F");
            s_charSet.Add(0x47, "G");
            s_charSet.Add(0x48, "H");
            s_charSet.Add(0x49, "I");
            s_charSet.Add(0x4A, "J");
            s_charSet.Add(0x4B, "K");
            s_charSet.Add(0x4C, "L");
            s_charSet.Add(0x4D, "M");
            s_charSet.Add(0x4E, "N");
            s_charSet.Add(0x4F, "O");

            s_charSet.Add(0x50, "P");
            s_charSet.Add(0x51, "Q");
            s_charSet.Add(0x52, "R");
            s_charSet.Add(0x53, "S");
            s_charSet.Add(0x54, "T");
            s_charSet.Add(0x55, "U");
            s_charSet.Add(0x56, "V");
            s_charSet.Add(0x57, "W");
            s_charSet.Add(0x58, "X");
            s_charSet.Add(0x59, "Y");
            s_charSet.Add(0x5A, "Z");
            s_charSet.Add(0x5B, "[");
            s_charSet.Add(0x5C, "¥");
            s_charSet.Add(0x5D, "]");
            s_charSet.Add(0x5E, "^");
            s_charSet.Add(0x5F, "_");

            s_charSet.Add(0x60, "`");
            s_charSet.Add(0x61, "a");
            s_charSet.Add(0x62, "b");
            s_charSet.Add(0x63, "c");
            s_charSet.Add(0x64, "d");
            s_charSet.Add(0x65, "e");
            s_charSet.Add(0x66, "f");
            s_charSet.Add(0x67, "g");
            s_charSet.Add(0x68, "h");
            s_charSet.Add(0x69, "i");
            s_charSet.Add(0x6A, "j");
            s_charSet.Add(0x6B, "k");
            s_charSet.Add(0x6C, "l");
            s_charSet.Add(0x6D, "m");
            s_charSet.Add(0x6E, "n");
            s_charSet.Add(0x6F, "o");

            s_charSet.Add(0x70, "p");
            s_charSet.Add(0x71, "q");
            s_charSet.Add(0x72, "r");
            s_charSet.Add(0x73, "s");
            s_charSet.Add(0x74, "t");
            s_charSet.Add(0x75, "u");
            s_charSet.Add(0x76, "v");
            s_charSet.Add(0x77, "w");
            s_charSet.Add(0x78, "x");
            s_charSet.Add(0x79, "y");
            s_charSet.Add(0x7A, "z");
            s_charSet.Add(0x7B, "{");
            s_charSet.Add(0x7C, "|");
            s_charSet.Add(0x7D, "}");
            s_charSet.Add(0x7E, "‾");
            s_charSet.Add(0x7F, "代");

            s_charSet.Add(0x80, "形");
            s_charSet.Add(0x81, "勇");
            s_charSet.Add(0x82, "気");
            s_charSet.Add(0x83, "年");
            s_charSet.Add(0x84, "杯");
            s_charSet.Add(0x85, "体");
            s_charSet.Add(0x86, "を");
            s_charSet.Add(0x87, "ぁ");
            s_charSet.Add(0x88, "ぃ");
            s_charSet.Add(0x89, "ぅ");
            s_charSet.Add(0x8A, "ぇ");
            s_charSet.Add(0x8B, "ぉ");
            s_charSet.Add(0x8C, "ゃ");
            s_charSet.Add(0x8D, "ゅ");
            s_charSet.Add(0x8E, "ょ");
            s_charSet.Add(0x8F, "っ");

            s_charSet.Add(0x90, "真");
            s_charSet.Add(0x91, "あ");
            s_charSet.Add(0x92, "い");
            s_charSet.Add(0x93, "う");
            s_charSet.Add(0x94, "え");
            s_charSet.Add(0x95, "お");
            s_charSet.Add(0x96, "か");
            s_charSet.Add(0x97, "き");
            s_charSet.Add(0x98, "く");
            s_charSet.Add(0x99, "け");
            s_charSet.Add(0x9A, "こ");
            s_charSet.Add(0x9B, "さ");
            s_charSet.Add(0x9C, "し");
            s_charSet.Add(0x9D, "す");
            s_charSet.Add(0x9E, "せ");
            s_charSet.Add(0x9F, "そ");

            s_charSet.Add(0xA0, "実");
            s_charSet.Add(0xA1, "｡");
            s_charSet.Add(0xA2, "｢");
            s_charSet.Add(0xA3, "｣");
            s_charSet.Add(0xA4, "､");
            s_charSet.Add(0xA5, "･");
            s_charSet.Add(0xA6, "ｦ");
            s_charSet.Add(0xA7, "ｧ");
            s_charSet.Add(0xA8, "ｨ");
            s_charSet.Add(0xA9, "ｩ");
            s_charSet.Add(0xAA, "ｪ");
            s_charSet.Add(0xAB, "ｫ");
            s_charSet.Add(0xAC, "ｬ");
            s_charSet.Add(0xAD, "ｭ");
            s_charSet.Add(0xAE, "ｮ");
            s_charSet.Add(0xAF, "ｯ");

            s_charSet.Add(0xB0, "ｰ");
            s_charSet.Add(0xB1, "ｱ");
            s_charSet.Add(0xB2, "ｲ");
            s_charSet.Add(0xB3, "ｳ");
            s_charSet.Add(0xB4, "ｴ");
            s_charSet.Add(0xB5, "ｵ");
            s_charSet.Add(0xB6, "ｶ");
            s_charSet.Add(0xB7, "ｷ");
            s_charSet.Add(0xB8, "ｸ");
            s_charSet.Add(0xB9, "ｹ");
            s_charSet.Add(0xBA, "ｺ");
            s_charSet.Add(0xBB, "ｻ");
            s_charSet.Add(0xBC, "ｼ");
            s_charSet.Add(0xBD, "ｽ");
            s_charSet.Add(0xBE, "ｾ");
            s_charSet.Add(0xBF, "ｿ");

            s_charSet.Add(0xC0, "ﾀ");
            s_charSet.Add(0xC1, "ﾁ");
            s_charSet.Add(0xC2, "ﾂ");
            s_charSet.Add(0xC3, "ﾃ");
            s_charSet.Add(0xC4, "ﾄ");
            s_charSet.Add(0xC5, "ﾅ");
            s_charSet.Add(0xC6, "ﾆ");
            s_charSet.Add(0xC7, "ﾇ");
            s_charSet.Add(0xC8, "ﾈ");
            s_charSet.Add(0xC9, "ﾉ");
            s_charSet.Add(0xCA, "ﾊ");
            s_charSet.Add(0xCB, "ﾋ");
            s_charSet.Add(0xCC, "ﾌ");
            s_charSet.Add(0xCD, "ﾍ");
            s_charSet.Add(0xCE, "ﾎ");
            s_charSet.Add(0xCF, "ﾏ");

            s_charSet.Add(0xD0, "ﾐ");
            s_charSet.Add(0xD1, "ﾑ");
            s_charSet.Add(0xD2, "ﾒ");
            s_charSet.Add(0xD3, "ﾓ");
            s_charSet.Add(0xD4, "ﾔ");
            s_charSet.Add(0xD5, "ﾕ");
            s_charSet.Add(0xD6, "ﾖ");
            s_charSet.Add(0xD7, "ﾗ");
            s_charSet.Add(0xD8, "ﾘ");
            s_charSet.Add(0xD9, "ﾙ");
            s_charSet.Add(0xDA, "ﾚ");
            s_charSet.Add(0xDB, "ﾛ");
            s_charSet.Add(0xDC, "ﾜ");
            s_charSet.Add(0xDD, "ﾝ");
            s_charSet.Add(0xDE, "ﾞ");
            s_charSet.Add(0xDF, "ﾟ");

            s_charSet.Add(0xE0, "た");
            s_charSet.Add(0xE1, "ち");
            s_charSet.Add(0xE2, "つ");
            s_charSet.Add(0xE3, "て");
            s_charSet.Add(0xE4, "と");
            s_charSet.Add(0xE5, "な");
            s_charSet.Add(0xE6, "に");
            s_charSet.Add(0xE7, "ぬ");
            s_charSet.Add(0xE8, "ね");
            s_charSet.Add(0xE9, "の");
            s_charSet.Add(0xEA, "は");
            s_charSet.Add(0xEB, "ひ");
            s_charSet.Add(0xEC, "ふ");
            s_charSet.Add(0xED, "へ");
            s_charSet.Add(0xEE, "ほ");
            s_charSet.Add(0xEF, "ま");

            s_charSet.Add(0xF0, "み");
            s_charSet.Add(0xF1, "む");
            s_charSet.Add(0xF2, "め");
            s_charSet.Add(0xF3, "も");
            s_charSet.Add(0xF4, "や");
            s_charSet.Add(0xF5, "ゆ");
            s_charSet.Add(0xF6, "よ");
            s_charSet.Add(0xF7, "ら");
            s_charSet.Add(0xF8, "り");
            s_charSet.Add(0xF9, "る");
            s_charSet.Add(0xFA, "れ");
            s_charSet.Add(0xFB, "ろ");
            s_charSet.Add(0xFC, "わ");
            s_charSet.Add(0xFD, "ん");
            s_charSet.Add(0xFE, "我");
            s_charSet.Add(0xFF, "▼");

            return s_charSet;
        }
    }
}