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
        public enum TextModes : int
        {
            INVISIBLE,
            WRITE,
            IMMEDIATE,
            SCANNER,
            WAITING,
            MAX
        };

        /* TEXT CONTROL CODES
            \\1: Wait for key input. Use the weapon key to advance to the next page. It cannot be used in the corpse memo.
            \\2: Flag on. Specify the flag number in the following 2 bytes.    \2\byteOne\byteTwo     CALCULATION: (byteOne - 1) * 256 + byteTwo
            \\3: Flag off. Specify the flag number in the following 2 bytes.   \3\byteOne\byteTwo     CALCULATION: (byteOne - 1) * 256 + byteTwo
            \\4: Item acquisition. Get the item in the next 1 byte. Item number from 1.
            \\5: ROM acquisition. 1 byte after this will get the ROM. ROM number starts from 1.
            \\6: Sound playback. Sounds the SE with the number specified by the following 1 byte. SE number from 1.
            \\7: Prevents you from leaving the conversation.
            \\8: Cancel the \\7 state.
            \\10: Line feed. Do not use line breaks in the talk editor.
        */


        public bool LockTo30FPS { get; set; } = true;
        public int Depth { get; set; } = (int)Global.DrawOrder.Text;
        public Effect ActiveShader { get; set; } = null;
        public bool FrozenWhenCameraIsBusy { get; set; } = false;


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
        private List<Vector2> _queuedTextPadding = new List<Vector2>();
        private List<bool> _queuedTextPotentiallyLamulanese = new List<bool>();
        private List<World.VIEW_DIR> _queuedTextAlignment = new List<World.VIEW_DIR>();
        private List<bool> _queuedTextFollowCamera = new List<bool>();

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
"@ABCDEFGHIJKLMNO" +
"PQRSTUVWXYZ[\\]^_" +
"`abcdefghijklmno" +
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
            codeMapDictJPN['没'] = 66;
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
                    Vector2 padding = _queuedTextPadding[i];
                    bool potentiallyLamulanese = _queuedTextPotentiallyLamulanese[i];
                    World.VIEW_DIR alignment = _queuedTextAlignment[i];
                    bool followCamera = _queuedTextFollowCamera[i];
                    DisplayString(spriteBatch, str, posX, posY, charLimit, color, padding, potentiallyLamulanese, alignment, followCamera);
                    i++;
                }
                _queuedText.Clear();
                _queuedTextColor.Clear();
                _queuedXYPos.Clear();
                _queuedTextLengths.Clear();
                _queuedTextPadding.Clear();
                _queuedTextPotentiallyLamulanese.Clear();
                _queuedTextAlignment.Clear();
                _queuedTextFollowCamera.Clear();
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

        private void DisplayString(SpriteBatch spriteBatch, string str, int x, int y, int charLimit, Color col = default, Vector2 padding = default, bool potentiallyLamulanese = false, World.VIEW_DIR alignment = World.VIEW_DIR.LEFT, bool followCamera = false)
        {
            if (padding == default)
                padding = Vector2.Zero;

            int posX = x + (int)padding.X;
            int posY = y;
            if (followCamera)
            {
                posX += (int)Global.Camera.Position.X;
                posY += (int)Global.Camera.Position.Y;
            }

            var xOff = 0;

            switch (alignment)
            {
                default:
                case World.VIEW_DIR.LEFT:
                    break;
                case World.VIEW_DIR.RIGHT:
                    xOff = -str.Length * TEXT_WIDTH;
                    break;
            }

            var yOff = 0;

            //var yoff = 0;
            int j = 1;
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

                            if (str[strPos + 1] != '\\' && Char.IsDigit(str[strPos + 1]) && (textControl.Length < 2 || (charLimit != 28 && charLimit != 22 && charLimit != 21))) {
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
                   
                    lineBreak = false;

                    Vector2 shiftedVecAfterWriting = Vector2.Zero;
                    if (c != ' ' || drawTile > 15)
                    {
                        shiftedVecAfterWriting = DrawChar(spriteBatch, new Point(posX + xOff + (int)_drawOff.X, posY + yOff + (int)_drawOff.Y), c, drawTile, col, potentiallyLamulanese);


                        if (textControlValue > 10 || c != ' ')
                        {

                            switch (alignment)
                            {
                                default:
                                case World.VIEW_DIR.LEFT:
                                case World.VIEW_DIR.RIGHT:
                                    xOff += TEXT_WIDTH + (int)shiftedVecAfterWriting.X;
                                    break;
                            }
                        }
                    }

                    switch (alignment)
                    {
                        default:
                        case World.VIEW_DIR.LEFT:
                            xOff = 0;
                            break;
                        case World.VIEW_DIR.RIGHT:
                            xOff = -str.Length * TEXT_WIDTH;
                            break;
                    }
                    yOff += TEXT_HEIGHT + (int)padding.Y;
                    j = 0;

                    j -= (int)shiftedVecAfterWriting.Y; // The Y vector represents the dakuten/handakuten counters for Japanese characters
                    if (!lineBreak)
                        j++;


                }
                else {
                    Vector2 shiftedVecAfterWriting = Vector2.Zero;
                    if ((c != ' ') || j > 0)
                        shiftedVecAfterWriting = DrawChar(spriteBatch, new Point(posX + xOff + (int)_drawOff.X, posY + yOff + (int)_drawOff.Y), c, drawTile, col, potentiallyLamulanese);
                    if (textControlValue > 10)
                        xOff += TEXT_WIDTH + (int)shiftedVecAfterWriting.X;
                    j -= (int)shiftedVecAfterWriting.Y; // The Y vector represents the dakuten/handakuten counters for Japanese characters
                    j++;
                }
            }
        }

        private Vector2 DrawChar(SpriteBatch spriteBatch, Point point, char c, int drawTile, Color col = default, bool potentiallyLamulanese = false)
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

                        try
                        {
                            thisChar = codeMapDictJPN[c] - 32;
                        }
                        catch (KeyNotFoundException)
                        {
                            thisChar = 0;
                        }
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

            if (potentiallyLamulanese) {
                if (Global.Protag != null)
                {
                    if (Global.Inventory.EquippedSoftware != null)
                    {
                        Global.ObtainableSoftware[] equippedRoms = Global.Inventory.EquippedSoftware;
                        if ((equippedRoms[0] != Global.ObtainableSoftware.GLYPH_READER) && (equippedRoms[1] != Global.ObtainableSoftware.GLYPH_READER))
                            _dx -= 16;
                    }
                }
            }
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

        public void QueueText(int x, int y, string str, int charLimit = 32, Color col = default, int paddingX = 0, int paddingY = 0, bool potentiallyLamulanese = false, World.VIEW_DIR alignment = World.VIEW_DIR.LEFT, bool followCamera = false)
        {
            if (str == null)
                return;
            byte[] bytes = Encoding.Default.GetBytes(str);
            str = Encoding.UTF8.GetString(bytes);

            _queuedXYPos.Add(new Point(x, y));
            _queuedText.Add(str);
            _queuedTextColor.Add(col);
            _queuedTextLengths.Add(charLimit);
            _queuedTextPadding.Add(new Vector2(paddingX, paddingY));
            _queuedTextPotentiallyLamulanese.Add(potentiallyLamulanese);
            _queuedTextAlignment.Add(alignment);
            _queuedTextFollowCamera.Add(followCamera);
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

        internal void DrawText(Vector2 position, string str, int charLimit = 32, Color col = default, World.VIEW_DIR alignment = World.VIEW_DIR.LEFT, bool followCamera = false)
        {
            QueueText((int)position.X, (int)position.Y, str, charLimit, col, 0, 0, false, alignment, followCamera);
        }

        internal void DrawTextImmediate(Vector2 position, string str, int charLimit = 32)
        {
            DrawTextImmediate((int)position.X, (int)position.Y, str, charLimit);
        }
    }
}
