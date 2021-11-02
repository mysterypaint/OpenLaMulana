using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using OpenLaMulana.Entities;

namespace OpenLaMulana.System
{
    public class PseudoXML
    {
        private const int DECRYPTION_KEY = 0x61;
        private static World _world = null;

        private enum TokenTypes
        {
            Talk,
            World,
            Field,
            Chipline,
            Hit,
            Anime,
            Object,
            Start,
            Map,
            Up,
            Down,
            Left,
            Right,
            Max
        }

        public static void ParseDataScriptTree(string inFile, World world)
        {
            _world = world;

            /*

            String str = File.ReadAllText(inFile, Encoding.UTF8);
            GetMatches(str, TokenTypes.Talk);

            List<String> aResult = new List<String>();

            var doc = File.ReadLines(inFile, Encoding.UTF8);

            foreach (var line in doc)
            {

                string regexPattern = "<TALK>[\r\n]+(.*?)[\r\n]+</TALK>+";
                //string regexPattern = "<WORLD>[\r\n]+(.*?)[\r\n]+</TALK>+";

                MatchCollection match = Regex.Matches(line, regexPattern);
                //aResult.Add(match[1].ToString());
            }

            _world.SetDialogue(aResult);
            */
        }

        private static void GetMatches(string inStr, TokenTypes token)
        {
            List<String> aResult = new List<String>();
            string regexPattern;

            switch (token)
            {
                case TokenTypes.Talk:
                    regexPattern = "<TALK>[\r\n]+(.*?)[\r\n]+</TALK>+";

                    foreach (Match match in Regex.Matches(inStr, regexPattern))
                    {
                        aResult.Add(match.Groups[1].ToString());
                    }

                    _world.SetDialogue(aResult);
                    break;
                case TokenTypes.World:
                    regexPattern = "";
                    break;
                default:
                    break;
            }
        }

        public static byte[] DecodeScriptDat(string dirPath, World world, Dictionary<int, string> charSet)
        {
            if (File.Exists(dirPath + "script_ENG_Decrypted_UTF8.txt"))
            {
                File.Delete(dirPath + "script_ENG_Decrypted_UTF8.txt");
            }

            byte[] fDat = File.ReadAllBytes(dirPath + "script_ENG.dat");

            FileStream converted = new FileStream(dirPath + "script_ENG_Decrypted_UTF8.txt", FileMode.CreateNew);

            using (StreamWriter writer = new StreamWriter(converted, Encoding.UTF8))
            {
                var i = 0;
                bool writingCommand = false;
                byte nextByte;
                bool talkingCommand = true;
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
                                        if (fDat[i - 5] == 0x2F) // /
                                            talkingCommand = false;

                                        writer.Write(charSet[currByte]);
                                        alreadyWrote = true;
                                    }
                        }
                    } else if (currByte == 0x3C) // <
                    {
                        if (i > 0)
                            writer.Write("\n");
                    }


                    if (!writingCommand && (currByte == 0x5C || (currByte <= 0xF && currByte != 0xA)))          // We ran into a '¥' char or special command. This is a special control code that must be written explicitly
                    {
                        writingCommand = true;
                    }
                    else if ((currByte >= 0x41 && currByte <= 0x5A) || currByte == 0x3C)     // If we hit a capital letter or '<'...
                    {
                        if (i < fDat.Length - 1) { // Are we already at the end of the file?
                            nextByte = (byte)(fDat[i + 1] ^ DECRYPTION_KEY);
                            if (currByte == 0x3C)
                                writingCommand = false;
                            if ((nextByte >= 0x61 && nextByte <= 0x7A) || nextByte == 0x20 || nextByte == 0x27)// We should check if the next char is a '<' char, another control code, a lowercase letter, a space, or an apostrophe
                                writingCommand = false;                                                         // ...before we stop explicitly writing values as-is, instead of actual text characters

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
                            if (fDat[i - 1] != 0x3E) // Only write a new linebreak as "\10" if the previous text wasn't "</TALK>"
                                writer.Write("\\" + currByte.ToString());
                            else
                                writer.Write("\n");
                        }
                        else {
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

            }

            return fDat;
        }

        public static Dictionary<int, string> DefineCharSet(Dictionary<int, string> charSet)
        {
            // Customized Shift-JIS; Manually defined for readability+portability (there's only a handful of characters & loaded on boot only once; Could probably do this iteratively but eh)

            Dictionary<int, string> _charSet = charSet;
            
            charSet.Add(0x0, "\\0"); //␀
            charSet.Add(0x1, "\\1"); //␁
            charSet.Add(0x2, "\\2"); //␂
            charSet.Add(0x3, "\\3"); //␃
            charSet.Add(0x4, "\\4"); //␄
            charSet.Add(0x5, "\\5"); //␅
            charSet.Add(0x6, "\\6"); //␆
            charSet.Add(0x7, "\\7"); //␇
            charSet.Add(0x8, "\\8"); //␈
            charSet.Add(0x9, "\\9"); //␉
            //charSet.Add(0xA, "\\10"); //␊
            charSet.Add(0xA, "\n\r");
            charSet.Add(0xB, "\\11"); //␋
            charSet.Add(0xC, "\\12"); //␌
            charSet.Add(0xD, "\\13"); //␌
            charSet.Add(0xE, "\\14"); //␎
            charSet.Add(0xF, "\\15"); //␏

            charSet.Add(0x10, "\\16"); //Ｓ
            charSet.Add(0x11, "\\17"); //ｄ
            charSet.Add(0x12, "\\18"); //Ｏ
            charSet.Add(0x13, "\\19"); //新
            charSet.Add(0x14, "\\20"); //⑩
            charSet.Add(0x15, "\\21"); //倍
            charSet.Add(0x16, "\\22"); //母
            charSet.Add(0x17, "\\23"); //天
            charSet.Add(0x18, "\\24"); //道
            charSet.Add(0x19, "\\25"); //書
            charSet.Add(0x1A, "\\26"); //者
            charSet.Add(0x1B, "\\27"); //間
            charSet.Add(0x1C, "\\28"); //死
            charSet.Add(0x1D, "\\29"); //地
            charSet.Add(0x1E, "\\30"); //古
            charSet.Add(0x1F, "\\31"); //文

            _charSet.Add(0x20, " ");
            _charSet.Add(0x21, "!");
            _charSet.Add(0x22, "\"");
            _charSet.Add(0x23, "\\35"); //#
            _charSet.Add(0x24, "\\36"); //$
            _charSet.Add(0x25, "\\37"); //%
            _charSet.Add(0x26, "&");
            _charSet.Add(0x27, "\'");
            _charSet.Add(0x28, "(");
            _charSet.Add(0x29, ")");
            _charSet.Add(0x2A, "*");
            _charSet.Add(0x2B, "+");
            _charSet.Add(0x2C, ",");
            _charSet.Add(0x2D, "-");
            _charSet.Add(0x2E, ".");
            _charSet.Add(0x2F, "/");

            _charSet.Add(0x30, "0");
            _charSet.Add(0x31, "1");
            _charSet.Add(0x32, "2");
            _charSet.Add(0x33, "3");
            _charSet.Add(0x34, "4");
            _charSet.Add(0x35, "5");
            _charSet.Add(0x36, "6");
            _charSet.Add(0x37, "7");
            _charSet.Add(0x38, "8");
            _charSet.Add(0x39, "9");
            _charSet.Add(0x3A, ":");
            _charSet.Add(0x3B, ";");
            _charSet.Add(0x3C, "<");
            _charSet.Add(0x3D, "=");
            _charSet.Add(0x3E, ">");
            _charSet.Add(0x3F, "?");

            _charSet.Add(0x40, "\\64"); //@
            _charSet.Add(0x41, "A");
            _charSet.Add(0x42, "B");
            _charSet.Add(0x43, "C");
            _charSet.Add(0x44, "D");
            _charSet.Add(0x45, "E");
            _charSet.Add(0x46, "F");
            _charSet.Add(0x47, "G");
            _charSet.Add(0x48, "H");
            _charSet.Add(0x49, "I");
            _charSet.Add(0x4A, "J");
            _charSet.Add(0x4B, "K");
            _charSet.Add(0x4C, "L");
            _charSet.Add(0x4D, "M");
            _charSet.Add(0x4E, "N");
            _charSet.Add(0x4F, "O");

            _charSet.Add(0x50, "P");
            _charSet.Add(0x51, "Q");
            _charSet.Add(0x52, "R");
            _charSet.Add(0x53, "S");
            _charSet.Add(0x54, "T");
            _charSet.Add(0x55, "U");
            _charSet.Add(0x56, "V");
            _charSet.Add(0x57, "W");
            _charSet.Add(0x58, "X");
            _charSet.Add(0x59, "Y");
            _charSet.Add(0x5A, "Z");
            _charSet.Add(0x5B, "[");
            _charSet.Add(0x5C, "¥");
            _charSet.Add(0x5D, "]");
            _charSet.Add(0x5E, "^");
            _charSet.Add(0x5F, "_");

            _charSet.Add(0x60, "`");
            _charSet.Add(0x61, "a");
            _charSet.Add(0x62, "b");
            _charSet.Add(0x63, "c");
            _charSet.Add(0x64, "d");
            _charSet.Add(0x65, "e");
            _charSet.Add(0x66, "f");
            _charSet.Add(0x67, "g");
            _charSet.Add(0x68, "h");
            _charSet.Add(0x69, "i");
            _charSet.Add(0x6A, "j");
            _charSet.Add(0x6B, "k");
            _charSet.Add(0x6C, "l");
            _charSet.Add(0x6D, "m");
            _charSet.Add(0x6E, "n");
            _charSet.Add(0x6F, "o");

            _charSet.Add(0x70, "p");
            _charSet.Add(0x71, "q");
            _charSet.Add(0x72, "r");
            _charSet.Add(0x73, "s");
            _charSet.Add(0x74, "t");
            _charSet.Add(0x75, "u");
            _charSet.Add(0x76, "v");
            _charSet.Add(0x77, "w");
            _charSet.Add(0x78, "x");
            _charSet.Add(0x79, "y");
            _charSet.Add(0x7A, "z");
            _charSet.Add(0x7B, "{");
            _charSet.Add(0x7C, "|");
            _charSet.Add(0x7D, "}");
            _charSet.Add(0x7E, "‾");
            _charSet.Add(0x7F, "代");

            _charSet.Add(0x80, "形");
            _charSet.Add(0x81, "勇");
            _charSet.Add(0x82, "気");
            _charSet.Add(0x83, "年");
            _charSet.Add(0x84, "杯");
            _charSet.Add(0x85, "体");
            _charSet.Add(0x86, "を");
            _charSet.Add(0x87, "ぁ");
            _charSet.Add(0x88, "ぃ");
            _charSet.Add(0x89, "ぅ");
            _charSet.Add(0x8A, "ぇ");
            _charSet.Add(0x8B, "ぉ");
            _charSet.Add(0x8C, "ゃ");
            _charSet.Add(0x8D, "ゅ");
            _charSet.Add(0x8E, "ょ");
            _charSet.Add(0x8F, "っ");

            _charSet.Add(0x90, "真");
            _charSet.Add(0x91, "あ");
            _charSet.Add(0x92, "い");
            _charSet.Add(0x93, "う");
            _charSet.Add(0x94, "え");
            _charSet.Add(0x95, "お");
            _charSet.Add(0x96, "か");
            _charSet.Add(0x97, "き");
            _charSet.Add(0x98, "く");
            _charSet.Add(0x99, "け");
            _charSet.Add(0x9A, "こ");
            _charSet.Add(0x9B, "さ");
            _charSet.Add(0x9C, "し");
            _charSet.Add(0x9D, "す");
            _charSet.Add(0x9E, "せ");
            _charSet.Add(0x9F, "そ");

            _charSet.Add(0xA0, "実");
            _charSet.Add(0xA1, "｡");
            _charSet.Add(0xA2, "｢");
            _charSet.Add(0xA3, "｣");
            _charSet.Add(0xA4, "､");
            _charSet.Add(0xA5, "･");
            _charSet.Add(0xA6, "ｦ");
            _charSet.Add(0xA7, "ｧ");
            _charSet.Add(0xA8, "ｨ");
            _charSet.Add(0xA9, "ｩ");
            _charSet.Add(0xAA, "ｪ");
            _charSet.Add(0xAB, "ｫ");
            _charSet.Add(0xAC, "ｬ");
            _charSet.Add(0xAD, "ｭ");
            _charSet.Add(0xAE, "ｮ");
            _charSet.Add(0xAF, "ｯ");

            _charSet.Add(0xB0, "ｰ");
            _charSet.Add(0xB1, "ｱ");
            _charSet.Add(0xB2, "ｲ");
            _charSet.Add(0xB3, "ｳ");
            _charSet.Add(0xB4, "ｴ");
            _charSet.Add(0xB5, "ｵ");
            _charSet.Add(0xB6, "ｶ");
            _charSet.Add(0xB7, "ｷ");
            _charSet.Add(0xB8, "ｸ");
            _charSet.Add(0xB9, "ｹ");
            _charSet.Add(0xBA, "ｺ");
            _charSet.Add(0xBB, "ｻ");
            _charSet.Add(0xBC, "ｼ");
            _charSet.Add(0xBD, "ｽ");
            _charSet.Add(0xBE, "ｾ");
            _charSet.Add(0xBF, "ｿ");

            _charSet.Add(0xC0, "ﾀ");
            _charSet.Add(0xC1, "ﾁ");
            _charSet.Add(0xC2, "ﾂ");
            _charSet.Add(0xC3, "ﾃ");
            _charSet.Add(0xC4, "ﾄ");
            _charSet.Add(0xC5, "ﾅ");
            _charSet.Add(0xC6, "ﾆ");
            _charSet.Add(0xC7, "ﾇ");
            _charSet.Add(0xC8, "ﾈ");
            _charSet.Add(0xC9, "ﾉ");
            _charSet.Add(0xCA, "ﾊ");
            _charSet.Add(0xCB, "ﾋ");
            _charSet.Add(0xCC, "ﾌ");
            _charSet.Add(0xCD, "ﾍ");
            _charSet.Add(0xCE, "ﾎ");
            _charSet.Add(0xCF, "ﾏ");

            _charSet.Add(0xD0, "ﾐ");
            _charSet.Add(0xD1, "ﾑ");
            _charSet.Add(0xD2, "ﾒ");
            _charSet.Add(0xD3, "ﾓ");
            _charSet.Add(0xD4, "ﾔ");
            _charSet.Add(0xD5, "ﾕ");
            _charSet.Add(0xD6, "ﾖ");
            _charSet.Add(0xD7, "ﾗ");
            _charSet.Add(0xD8, "ﾘ");
            _charSet.Add(0xD9, "ﾙ");
            _charSet.Add(0xDA, "ﾚ");
            _charSet.Add(0xDB, "ﾛ");
            _charSet.Add(0xDC, "ﾜ");
            _charSet.Add(0xDD, "ﾝ");
            _charSet.Add(0xDE, "ﾞ");
            _charSet.Add(0xDF, "ﾟ");

            _charSet.Add(0xE0, "た");
            _charSet.Add(0xE1, "ち");
            _charSet.Add(0xE2, "つ");
            _charSet.Add(0xE3, "て");
            _charSet.Add(0xE4, "と");
            _charSet.Add(0xE5, "な");
            _charSet.Add(0xE6, "に");
            _charSet.Add(0xE7, "ぬ");
            _charSet.Add(0xE8, "ね");
            _charSet.Add(0xE9, "の");
            _charSet.Add(0xEA, "は");
            _charSet.Add(0xEB, "ひ");
            _charSet.Add(0xEC, "ふ");
            _charSet.Add(0xED, "へ");
            _charSet.Add(0xEE, "ほ");
            _charSet.Add(0xEF, "ま");

            _charSet.Add(0xF0, "み");
            _charSet.Add(0xF1, "む");
            _charSet.Add(0xF2, "め");
            _charSet.Add(0xF3, "も");
            _charSet.Add(0xF4, "や");
            _charSet.Add(0xF5, "ゆ");
            _charSet.Add(0xF6, "よ");
            _charSet.Add(0xF7, "ら");
            _charSet.Add(0xF8, "り");
            _charSet.Add(0xF9, "る");
            _charSet.Add(0xFA, "れ");
            _charSet.Add(0xFB, "ろ");
            _charSet.Add(0xFC, "わ");
            _charSet.Add(0xFD, "ん");
            _charSet.Add(0xFE, "我");
            _charSet.Add(0xFF, "▼");

            return _charSet;
        }
    }
}