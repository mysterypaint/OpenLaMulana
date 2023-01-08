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
        private List<Point> queuedXYPos;
        private List<String> queuedText;
        private List<TextModes> queuedTextMode;
        private string _myString = "";
        private Vector2 _drawOff = Vector2.Zero;

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
            queuedXYPos = new List<Point>();
            queuedText = new List<String>();
            queuedTextMode = new List<TextModes>();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            var i = 0;
            if (queuedText.Count > 0)
            {
                foreach (string str in queuedText)
                {
                    DisplayString(spriteBatch, str, i);
                    i++;
                }
                queuedText.Clear();
                queuedXYPos.Clear();
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

        private void DisplayString(SpriteBatch spriteBatch, string str, int i)
        {
            int posX = queuedXYPos[i].X;
            int posY = queuedXYPos[i].Y;

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
                        }

                        if (Char.IsDigit(str[strPos])) {
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

                if (j >= 32 || lineBreak)
                {
                    // Perform a linebreak, because we hit the max char limit or run into a linebreak command
                    xOff = 0;
                    yOff += TEXT_HEIGHT;
                    j = 0;
                    lineBreak = false;
                }
                DrawChar(spriteBatch, new Point(posX + xOff + (int)_drawOff.X, posY + yOff + (int)_drawOff.Y), c, drawTile);
                xOff += TEXT_WIDTH;
                j++;
            }
        }

        private void DrawChar(SpriteBatch spriteBatch, Point point, char c, int drawTile)
        {

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
            spriteBatch.Draw(_gameFontTex, new Vector2(point.X, point.Y), new Rectangle(_dx * TEXT_WIDTH, _dy * TEXT_HEIGHT, TEXT_WIDTH, TEXT_HEIGHT), Color.White);
        }

        public void DrawText(int x, int y, string str)
        {
            if (str == null)
                return;
            byte[] bytes = Encoding.Default.GetBytes(str);
            str = Encoding.UTF8.GetString(bytes);

            queuedXYPos.Add(new Point(x, y));
            queuedText.Add(str);
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
            queuedXYPos.Add(new Point(0, 0));
            queuedText.Add(_myString);
        }

        internal void SetDrawPosition(Vector2 drawOffset)
        {
            _drawOff = drawOffset;
        }
    }
}
