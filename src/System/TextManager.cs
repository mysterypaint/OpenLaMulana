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


        public Dictionary<char, int> codeMapDictENG = new Dictionary<char, int>();
        public Dictionary<char, int> codeMapDictJPN = new Dictionary<char, int>();

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

            InitCodeMapDict();
            _queuedXYPos = new List<Point>();
            _queuedText = new List<String>();
            _queuedTextColor = new List<Color>();
            _queuedTextMode = new List<TextModes>();
        }

        private void InitCodeMapDict()
        {
            string CODE_MAP_ENG =
"０１２３４５６７８９\nＢＣＤＥＦ" +
"ＳｄＯ新⑩倍母天道書者闇死地古文" +
" !\"#$%&'()*+,-./" +
"0123456789:;<=>?" +
"@ABCDEFGHIJKLMNO" +
"PQRSTUVWXYZ[\\]^_" +
"`abcdefghijklmno" +
"pqrstuvwxyz{|}~代" +
"形勇気年杯体をぁぃぅぇぉゃゅょっ" +
"真あいうえおかきくけこさしすせそ" +
"実｡｢｣､･ｦｧｨｩｪｫｬｭｮｯ" +
"ｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿ" +
"ﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏ" +
"ﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝﾞﾟ" +
"たちつてとなにぬねのはひふへほま" +
"みむめもやゆよらりるれろわん我▼";

            string CODE_MAP_JPN =
"０１２３４５６７８９\nＢＣＤＥＦ" +
"ＳｄＯ新⑩倍母天道書者間死地古文" +
" !\"#$%&'()*+,-./" +
"0123456789:;<=>?" +
"人ABCDEFGHIJKLMNO" +
"PQRSTUVWXYZ[\\]^_" +
"巨abcdefghijklmno" +
"pqrstuvwxyz{|}時代" +
"形勇気年杯体をぁぃぅぇぉゃゅょっ" +
"真あいうえおかきくけこさしすせそ" +
"実｡｢｣､･ｦｧｨｩｪｫｬｭｮｯ" +
"ｰｱｲｳｴｵｶｷｸｹｺｻｼｽｾｿ" +
"ﾀﾁﾂﾃﾄﾅﾆﾇﾈﾉﾊﾋﾌﾍﾎﾏ" +
"ﾐﾑﾒﾓﾔﾕﾖﾗﾘﾙﾚﾛﾜﾝﾞﾟ" +
"たちつてとなにぬねのはひふへほま" +
"みむめもやゆよらりるれろわん我▼";

            int i = 0;
            foreach (char c in CODE_MAP_ENG)
            {
                codeMapDictENG[c] = i;
                i++;
            }

            i = 0;
            foreach (char c in CODE_MAP_JPN)
            {
                codeMapDictJPN[c] = i;
                i++;
            }
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

                        while (Char.IsDigit(str[strPos]))
                        {
                            textControl += c;
                            if (strPos >= str.Length - 1)
                            {
                                c = ' ';
                                break;
                            }

                            if (str[strPos + 1] != '\\' && Char.IsDigit(str[strPos + 1])) {
                                strPos++;
                                c = str[strPos];
                            } else
                            {
                                c = ' ';
                                break;
                            }
                        }

                        specialTextControl = true;
                    } else if (c == '‾')
                    {
                        textControl = "126";
                        c = ' ';
                        specialTextControl = true;
                    }
                }

                int textControlValue = 11;
                if (specialTextControl)
                {
                    Int32.TryParse(textControl, out int result);

                    textControlValue = result;
                    switch (result)
                    {
                        default:
                            break;
                        /// TODO: All the other control codes go here. \1 in particular needs to account for all the flags following it
                        case 10:
                            lineBreak = true;
                            break;
                    }

                    if (result > 15)
                    {
                        drawTile = result;
                    }
                    specialTextControl = false;
                }

                if ((j >= charLimit || lineBreak) && j > 0)
                {
                    // Perform a linebreak, because we hit the max char limit or run into a linebreak command
                    xOff = 0;
                    yOff += TEXT_HEIGHT;
                    j = 0;

                    lineBreak = false;
                    if (charLimit == 22) {
                        yOff += TEXT_HEIGHT; // Double-space for NPC dialogue

                        Vector2 shiftedVecAfterWriting = DrawChar(spriteBatch, new Point(posX + xOff + (int)_drawOff.X, posY + yOff + (int)_drawOff.Y), c, drawTile, col);
                        if (textControlValue > 10)
                            xOff += TEXT_WIDTH + (int)shiftedVecAfterWriting.X;
                        j -= (int)shiftedVecAfterWriting.Y; // The Y vector represents the dakuten/handakuten counters for Japanese characters
                        j++;
                    }
                } else {
                    Vector2 shiftedVecAfterWriting = DrawChar(spriteBatch, new Point(posX + xOff + (int)_drawOff.X, posY + yOff + (int)_drawOff.Y), c, drawTile, col);
                    if (textControlValue > 10)
                        xOff += TEXT_WIDTH + (int)shiftedVecAfterWriting.X;
                    j -= (int)shiftedVecAfterWriting.Y; // The Y vector represents the dakuten/handakuten counters for Japanese characters
                    j++;
                }
            }
        }

        private Vector2 DrawChar(SpriteBatch spriteBatch, Point point, char c, int drawTile, Color col = default)
        {
            if (col == default)
                col = Color.White;
            var _texOffX = 16; // Origin Tile X
            var _texOffY = 0;
            var _otx = 0;
            var _oty = 0;
            int thisChar = c;
            int textMaskY = 0;
            int dakutenHandakutenUsed = 0;

            if (drawTile <= -1)
            {
                _texOffY = 1;


                switch (Global.CurrLang)
                {
                    default:
                    case Global.Languages.Japanese:
                        if (c == '"')
                            c = 'ﾞ';
                        thisChar = codeMapDictJPN[c] - 32;

                        switch (thisChar)
                        {
                            case 190: // ﾞ
                                _otx = -7;
                                _oty = -1;
                                textMaskY = 7;
                                dakutenHandakutenUsed = 1;
                                break;
                            case 191: // ﾟ
                                _otx = -7;
                                _oty = -3;
                                textMaskY = 5;
                                dakutenHandakutenUsed = 1;
                                break;
                        }
                        break;
                    case Global.Languages.English:
                        thisChar = codeMapDictENG[c] - 32;
                        break;
                }
                // c - 32;//(int)c - 32;

            }
            else if (c >= 3040) // First unicode value for Japanese letters
            {
                c = (char)((int)c - 3040);
            }
            else
            {
                switch (Global.CurrLang)
                {
                    default:
                    case Global.Languages.Japanese:
                        _texOffY = -1;
                        break;
                    case Global.Languages.English:
                        _texOffY = -1;
                        break;
                }

                thisChar = drawTile;

                switch (thisChar)
                {
                    case 222: // ﾞ
                        _otx = -7;
                        _oty = -1;
                        textMaskY = 7;
                        dakutenHandakutenUsed = 1;
                        break;
                    case 223: // ﾟ
                        _otx = -7;
                        _oty = -3;
                        textMaskY = 5;
                        dakutenHandakutenUsed = 1;
                        break;
                }
            }

            var _dx = _texOffX + (thisChar % TEXT_TABLE_WIDTH);
            var _dy = _texOffY + (thisChar / TEXT_TABLE_WIDTH);

            bool Lamulanese = false;
            if (Lamulanese)
                _dx -= 16;
            // Space ' ' is = to 32

            switch (Global.CurrLang)
            {
                default:
                case Global.Languages.Japanese:
                    _gameFontTex = Global.TextureManager.GetTexture(Global.Textures.FONT_JP);
                    break;
                case Global.Languages.English:
                    _gameFontTex = Global.TextureManager.GetTexture(Global.Textures.FONT_EN);
                    break;
            }

            spriteBatch.Draw(_gameFontTex, new Vector2(_otx + point.X, _oty +point.Y), new Rectangle(_dx * TEXT_WIDTH, textMaskY + _dy * TEXT_HEIGHT, TEXT_WIDTH, TEXT_HEIGHT - textMaskY), col);

            return new Vector2(_otx, dakutenHandakutenUsed);
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
    }
}
