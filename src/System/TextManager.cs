using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLaMulana.System
{
    public class TextManager : IGameEntity
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

        public int Depth { get; set; } = (int)Global.DrawOrder.Text;
        public Effect ActiveShader { get; set; } = null;

        private const int TEXT_WIDTH = 8;
        private const int TEXT_HEIGHT = 8;
        private const int TEXT_TABLE_WIDTH = 16;


        public List<string> _dialogueJP;
        public List<string> _dialogueEN;

        public static Dictionary<int, string> s_charSet;

        private Texture2D _gameFontTex;
        private List<Point> _queuedXYPos;
        private List<String> _queuedText;
        private List<Color> _queuedTextColor;
        private List<TextModes> _queuedTextMode;
        private string _myString = "";
        private Vector2 _drawOff = Vector2.Zero;
        private List<int> _queuedTextLengths = new List<int>();

        public TextManager()
        {
            s_charSet = new Dictionary<int, string>();

            switch (Global.CurrLang) {
                default:
                case Global.Languages.Japanese:
                    _gameFontTex = Global.TextureManager.GetTexture(Global.Textures.FONT_JP);
                    break;
                case Global.Languages.English:
                    _gameFontTex = Global.TextureManager.GetTexture(Global.Textures.FONT_EN);
                    break;
            }
            _queuedXYPos = new List<Point>();
            _queuedText = new List<String>();
            _queuedTextColor = new List<Color>();
            _queuedTextMode = new List<TextModes>();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var i = 0;
            if (_queuedText.Count > 0)
            {
                foreach (string str in _queuedText)
                {
                    int posX = _queuedXYPos[i].X;
                    int posY = _queuedXYPos[i].Y;
                    int charLimit = _queuedTextLengths[i];
                    Color color = _queuedTextColor[i];
                    DisplayString(spriteBatch, str, posX, posY, charLimit, color);
                    i++;
                }
                _queuedText.Clear();
                _queuedTextColor.Clear();
                _queuedXYPos.Clear();
                _queuedTextLengths.Clear();
            }
        }

        public void SetDialogue(Global.Languages lang, List<string> data)
        {
            switch (lang)
            {
                default:
                case Global.Languages.English:
                    _dialogueEN = data;
                    break;
                case Global.Languages.Japanese:
                    _dialogueJP = data;
                    break;
            }
        }

        internal Dictionary<int, string> GetCharSet()
        {
            return s_charSet;
        }

        private void DisplayString(SpriteBatch spriteBatch, string str, int x, int y, int charLimit, Color col = default)
        {
            int posX = x;
            int posY = y;

            var xOff = 0;
            var yOff = 0;

            //var yoff = 0;
            int j = 0;
            bool lineBreak = false;
            bool specialTextControl = false;


            for (int strPos = 0; strPos < str.Length; strPos++)
            {
                string textControl = "";
                char c = str[strPos];

                int drawTile = -1;
                if (!specialTextControl)
                {
                    if (c == '¥' || c == '\\')
                    {
                        strPos++;
                        c = str[strPos];

                        while (Char.IsDigit(str[strPos]) && (strPos + 1) < str.Length)
                        {
                            textControl += c;
                            strPos++;
                            c = str[strPos];
                            if (strPos >= str.Length)
                                break;
                            if (textControl.Length >= 2)
                                break;
                        }

                        if (Char.IsDigit(str[strPos]) && textControl.Length < 2) {
                            textControl += c;
                            strPos++;
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
                            lineBreak = true;
                            break;
                    }

                    if (result > 15)
                    {
                        drawTile = result;
                        strPos--;
                    }
                    specialTextControl = false;
                }

                if (j >= charLimit || lineBreak)
                {
                    // Perform a linebreak, because we hit the max char limit or run into a linebreak command
                    xOff = 0;
                    yOff += TEXT_HEIGHT;
                    j = 0;
                    lineBreak = false;
                }
                DrawChar(spriteBatch, new Point(posX + xOff + (int)_drawOff.X, posY + yOff + (int)_drawOff.Y), c, drawTile, col);
                xOff += TEXT_WIDTH;
                j++;
            }
        }

        private void DrawChar(SpriteBatch spriteBatch, Point point, char c, int drawTile, Color col = default)
        {
            if (col == default)
                col = Color.White;
            var _otx = 16; // Origin Tile X
            var _oty = 0;
            int thisChar = c;

            if (drawTile <= -1)
            {
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
            spriteBatch.Draw(_gameFontTex, new Vector2(point.X, point.Y), new Rectangle(_dx * TEXT_WIDTH, _dy * TEXT_HEIGHT, TEXT_WIDTH, TEXT_HEIGHT), col);
        }

        public void DrawText(int x, int y, string str, int charLimit = 32, Color col = default)
        {
            if (str == null)
                return;
            byte[] bytes = Encoding.Default.GetBytes(str);
            str = Encoding.UTF8.GetString(bytes);

            _queuedXYPos.Add(new Point(x, y));
            _queuedText.Add(str);
            _queuedTextColor.Add(col);
            _queuedTextLengths.Add(charLimit);
        }

        public void DrawTextImmediate(int x, int y, string str, int charLimit = 32)
        {
            if (str == null)
                return;
            byte[] bytes = Encoding.Default.GetBytes(str);
            str = Encoding.UTF8.GetString(bytes);
            DisplayString(Global.SpriteBatch, str, x, y,  charLimit);
        }

        public void Update(GameTime gameTime)
        {

        }

        internal string GetText(int index, Global.Languages lang)
        {
            if (index < 0)
                return null;

            List<string> _dialogue;

            switch (lang)
            {
                default:
                case Global.Languages.English:
                    _dialogue = _dialogueEN;
                    break;
                case Global.Languages.Japanese:
                    _dialogue = _dialogueJP;
                    break;
            }

            return _dialogue[Math.Clamp(index, 0, _dialogue.Count - 1)];
        }

        internal int GetTextCount(Global.Languages lang)
        {
            List<string> _dialogue;

            switch (lang)
            {
                default:
                case Global.Languages.English:
                    _dialogue = _dialogueEN;
                    break;
                case Global.Languages.Japanese:
                    _dialogue = _dialogueJP;
                    break;
            }

            return _dialogue.Count;
        }

        public void SetText(string str)
        {
            if (str.Trim().Equals(""))
                return;
            _myString = str;
        }

        internal void DrawOwnString()
        {
            _queuedXYPos.Add(new Point(0, 0));
            _queuedText.Add(_myString);
        }

        internal void SetDrawPosition(Vector2 drawOffset)
        {
            _drawOff = drawOffset;
        }

        internal void DrawText(Vector2 position, string str, int charLimit = 32, Color col = default)
        {
            DrawText((int)position.X, (int)position.Y, str, charLimit, col);
        }

        internal void DrawTextImmediate(Vector2 position, string str, int charLimit = 32)
        {
            DrawTextImmediate((int)position.X, (int)position.Y, str, charLimit);
        }

        internal void GetText(object spawnPoint)
        {
            throw new NotImplementedException();
        }
    }
}
