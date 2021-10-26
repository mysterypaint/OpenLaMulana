using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Xml.Linq;

namespace OpenLaMulana.Entities
{
    public class PseudoXML
    {
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

            String str = File.ReadAllText(inFile, Encoding.UTF8);

            for (var i = 0; i < (int)TokenTypes.Max; i++)
                GetMatches(str, (TokenTypes)i);
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
    }
}