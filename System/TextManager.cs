using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static OpenLaMulana.Entities.World;

namespace OpenLaMulana.System
{
    class TextManager : IGameEntity
    {
        public enum TextModes
        {
            INVISIBLE,
            WRITE,
            IMMEDIATE,
            SCANNER,
            WAITING,
            MAX
        };

        public int DrawOrder => -999;
        private const int TEXT_WIDTH = 8;
        private const int TEXT_HEIGHT = 8;
        private const int TEXT_TABLE_WIDTH = 16;


        public List<string> _dialogueJP;
        public List<string> _dialogueEN;

        public static Dictionary<int, string> s_charSet;

        private Texture2D _gameFontTex;
        private List<Point> queuedXYPos;
        private List<String> queuedText;
        private List<TextModes> queuedTextMode;

        public TextManager(Texture2D gameFontTex)
        {
            s_charSet = new Dictionary<int, string>();

            _gameFontTex = gameFontTex;
            queuedXYPos = new List<Point>();
            queuedText = new List<String>();
            queuedTextMode = new List<TextModes>();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var i = 0;
            if (queuedText.Count > 0) {
                foreach(string str in queuedText)
                {
                    DisplayString(spriteBatch, str, i);
                    i++;
                }
                queuedText.Clear();
                queuedXYPos.Clear();
            }
        }

        public void SetDialogue(OpenLaMulanaGame.Languages lang, List<string> data)
        {
            switch (lang)
            {
                default:
                case OpenLaMulanaGame.Languages.English:
                    _dialogueEN = data;
                    break;
                case OpenLaMulanaGame.Languages.Japanese:
                    _dialogueJP = data;
                    break;
            }
        }

        internal Dictionary<int, string> GetCharSet()
        {
            return s_charSet;
        }

        private void DisplayString(SpriteBatch spriteBatch, string str, int i)
        {
            int _posx = queuedXYPos[i].X;
            int _posy = queuedXYPos[i].Y;

            var _xoff = 0;
            var _yoff = 0;

            //var yoff = 0;
            int j = 0;
            bool linebreak = false;
            bool specialTextControl = false;
            

            for (int strPos = 0; strPos < str.Length; strPos++)
            {
                string textControl = "";
                char c = str[strPos];

                int _drawTile = -1;
                if (!specialTextControl) {
                    if (c == '¥')
                    {
                        strPos++;
                        
                        while (Char.IsDigit(str[strPos]))
                        {
                            textControl += c;
                            strPos++;

                            if (strPos >= str.Length)
                                break;
                        }
                        if (strPos < str.Length)
                            c = str[strPos];
                        else
                            c = ' ';
                        specialTextControl = true;
                    }
                }

                if (specialTextControl)
                {
                    Int32.TryParse(textControl, out int result);
                    switch (result)
                    {
                        default:
                            break;
                        case 10:
                            linebreak = true;
                            break;
                    }

                    if (result > 15)
                    {
                        _drawTile = result;
                        strPos--;
                    }
                    specialTextControl = false;
                }

                if (j >= 32 || linebreak)
                {
                    // Perform a linebreak, because we hit the max char limit or run into a linebreak command
                    _xoff = 0;
                    _yoff += TEXT_HEIGHT;
                    j = 0;
                    linebreak = false;
                }
                DrawChar(spriteBatch, new Point(_posx + _xoff, _posy + _yoff), c, _drawTile);
                _xoff += TEXT_WIDTH;
                j++;
            }
        }

        private void DrawChar(SpriteBatch spriteBatch, Point point, char c, int drawTile)
        {

            var _otx = 16; // Origin Tile X
            var _oty = 0;
            int thisChar = c;

            if (drawTile <= -1) {
                _oty = 1;
                thisChar = (int)c - 32;

            }
            else if (c >= 3040) // First unicode value for Japanese letters
            {
                c = (char)((int)c - 3040);
            }
            else
            {
                _oty = -1;
                thisChar = drawTile;
            }

            var _dx = _otx + (thisChar % TEXT_TABLE_WIDTH);
            var _dy = _oty + (thisChar / TEXT_TABLE_WIDTH);

            bool Lamulanese = false;
            if (Lamulanese)
                _dx -= 16;
            // Space ' ' is = to 32
            spriteBatch.Draw(_gameFontTex, new Vector2(point.X, point.Y), new Rectangle(_dx * TEXT_WIDTH, _dy * TEXT_HEIGHT, TEXT_WIDTH, TEXT_HEIGHT), Color.White);
        }

        public void DrawText(int x, int y, string str)
        {
            byte[] bytes = Encoding.Default.GetBytes(str);
            str = Encoding.UTF8.GetString(bytes);

            queuedXYPos.Add(new Point(x, y));
            queuedText.Add(str);
        }

        public void Update(GameTime gameTime)
        {
            
        }

        internal string GetText(int index, OpenLaMulanaGame.Languages lang)
        {
            List<string> _dialogue;

            switch (lang)
            {
                default:
                case OpenLaMulanaGame.Languages.English:
                    _dialogue = _dialogueEN;
                    break;
                case OpenLaMulanaGame.Languages.Japanese:
                    _dialogue = _dialogueJP;
                    break;
            }

            return _dialogue[index];
        }

        internal int GetTextCount(OpenLaMulanaGame.Languages lang)
        {
            List<string> _dialogue;

            switch (lang)
            {
                default:
                case OpenLaMulanaGame.Languages.English:
                    _dialogue = _dialogueEN;
                    break;
                case OpenLaMulanaGame.Languages.Japanese:
                    _dialogue = _dialogueJP;
                    break;
            }

            return _dialogue.Count;
        }
    }
}
