using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static System.Net.Mime.MediaTypeNames;

namespace OpenLaMulana.Graphics {
    public class TextureManager {
        private Dictionary<Global.Textures, Texture2D> _texDict = new Dictionary<Global.Textures, Texture2D>();
        private Dictionary<int, Global.Textures> _mappedWorldTexturesJPN = new Dictionary<int, Global.Textures>();
        private Dictionary<int, Global.Textures> _mappedWorldTexturesENG = new Dictionary<int, Global.Textures>();
        private Dictionary<int, Global.Textures> _mappedEventTexturesJPN = new Dictionary<int, Global.Textures>();
        private Dictionary<int, Global.Textures> _mappedEventTexturesENG = new Dictionary<int, Global.Textures>();
        private Texture2D _whitePixel;

        public TextureManager() {
        }

        public void InitTextures() {
            _whitePixel = new Texture2D(Global.GraphicsDevice, 1, 1);
            _whitePixel.SetData(new[] { Color.White });


            string gfxPath = "Content/graphics/";
            string[] allTextures = {
                "boss00",
                "boss01",
                "boss02",
                "boss03",
                "boss04",
                "boss05",
                "boss06",
                "boss07",
                "boss08",
                "enddemo",
                "enemy1",
                "enemy2",
                "eveg00",
                "eveg00_EN",
                "eveg01",
                "eveg02",
                "eveg02_EN",
                "eveg03",
                "eveg03_EN",
                "eveg04",
                "eveg04_EN",
                "eveg05",
                "eveg05_EN",
                "eveg06",
                "eveg06_EN",
                "eveg07",
                "eveg07_EN",
                "eveg08",
                "eveg08_EN",
                "eveg09",
                "eveg09_EN",
                "eveg10",
                "eveg10_EN",
                "eveg11",
                "eveg11_EN",
                "eveg12",
                "eveg12_EN",
                "eveg13",
                "eveg13_EN",
                "eveg14",
                "eveg14_EN",
                "eveg15",
                "eveg15_EN",
                "eveg16",
                "eveg16_EN",
                "eveg17",
                "eveg17_EN",
                "eveg18",
                "eveg18_EN",
                "eveg19",
                "eveg20",
                "eveg21",
                "eveg22",
                "font_EN",
                "font_JP",
                "item",
                "logo",
                "logo_EN",
                "mapg00",
                "mapg01",
                "mapg02",
                "mapg03",
                "mapg04",
                "mapg05",
                "mapg06",
                "mapg07",
                "mapg08",
                "mapg09",
                "mapg10",
                "mapg11",
                "mapg12",
                "mapg13",
                "mapg14",
                "mapg15",
                "mapg16",
                "mapg17",
                "mapg18",
                "mapg18_EN",
                "mapg19",
                "mapg20",
                "mapg20_EN",
                "mapg21",
                "mapg22",
                "mapg22_EN",
                "mapg31",
                "mapg31_EN",
                "mapg32",
                "mapg32_EN",
                "msxlogo",
                "msxlogo_EN",
                "opdemo",
                "prot1",
                "prot_debug",
                "stdemo1",
                "title",
                "system\\entityTemplate"
            };
            for (Global.Textures texID = Global.Textures.BOSS00; texID < Global.Textures.MAX; texID++) {
                string fName = allTextures[(int)texID] + ".png";

                if (texID == Global.Textures.DEBUG_ENTITY_TEMPLATE)
                    fName = "system/entityTemplate.png";
                Texture2D newTex = LoadTexture(Path.Combine(gfxPath, fName));
                _texDict.Add(texID, newTex);
            }

            int texIndex = 0;
            for (Global.Textures texID = Global.Textures.MAPG00; texID <= Global.Textures.MAPG32_EN; texID++)
            {
                while (texIndex >= 23 && texIndex <= 30)
                {
                    if (texID == Global.Textures.MAPG22_EN)
                        break;
                    texIndex++;
                }

                switch (texID)
                {
                    default:
                        _mappedWorldTexturesJPN.Add(texIndex, texID);
                        texIndex++;
                        break;
                    case Global.Textures.MAPG18_EN:
                    case Global.Textures.MAPG20_EN:
                    case Global.Textures.MAPG22_EN:
                    case Global.Textures.MAPG31_EN:
                    case Global.Textures.MAPG32_EN:
                        _mappedWorldTexturesENG.Add(texIndex - 1, texID);
                        break;
                }
            }

            texIndex = 0;
            for (Global.Textures texID = Global.Textures.EVEG00; texID <= Global.Textures.EVEG22; texID++)
            {
                switch (texID)
                {
                    default:
                        _mappedEventTexturesJPN.Add(texIndex, texID);
                        texID++;
                        _mappedEventTexturesENG.Add(texIndex, texID);
                        texIndex++;
                        break;
                    case Global.Textures.EVEG01:
                    case Global.Textures.EVEG19:
                    case Global.Textures.EVEG20:
                    case Global.Textures.EVEG21:
                    case Global.Textures.EVEG22:
                        _mappedEventTexturesJPN.Add(texIndex, texID);
                        texIndex++;
                        break;
                }
            }
        }

        public void UnloadContent()
        {
            _whitePixel.Dispose();
        }
        public Texture2D GetTexture(Global.Textures texID)
        {
            return _texDict[texID];
        }

        private Texture2D LoadTexture(string filePath)
        {
            FileStream fileStream = new FileStream(filePath, FileMode.Open);
            Texture2D tex = Texture2D.FromStream(Global.GraphicsDevice, fileStream);
            fileStream.Dispose();
            Color[] buffer = new Color[tex.Width * tex.Height];
            tex.GetData(buffer);
            Color colGray = new Color(68, 68, 68);
            for (int i = 0; i < buffer.Length; i++)
            {
                // Replace all transparent gray pixels in every loaded texture with White, Alpha 0
                if (buffer[i].Equals(colGray))
                    buffer[i] = Color.FromNonPremultiplied(255, 255, 255, 0);
            }
            tex.SetData(buffer);

            return tex;
        }

        /// <summary>
        /// The textures for every language is stored in memory, and their shared IDs need to be accounted for.
        /// This function redirects the map data assets' graphical indexes, so that they know which textures to load internally within this engine.
        /// </summary>
        /// <param name="texID"></param>
        /// <returns></returns>
        public Global.Textures GetMappedWorldTexID(int texID)
        {
            switch (texID)
            {
                default:
                    return _mappedWorldTexturesJPN[texID];
                case 18:
                case 20:
                case 22:
                case 31:
                case 32:
                    switch (Global.CurrLang)
                    {
                        default:
                        case Global.Languages.Japanese:
                            return _mappedWorldTexturesJPN[texID];
                        case Global.Languages.English:
                            return _mappedWorldTexturesENG[texID];
                    }
            }
        }

        public Global.Textures GetMappedEventTexID(int texID)
        {
            switch (texID)
            {
                default:
                    switch (Global.CurrLang)
                    {
                        default:
                        case Global.Languages.Japanese:
                            return _mappedEventTexturesJPN[texID];
                        case Global.Languages.English:
                            return _mappedEventTexturesENG[texID];
                    }
                case 1:
                case 19:
                case 20:
                case 21:
                case 22:
                    return _mappedEventTexturesJPN[texID];
            }
        }

        internal Texture2D MakeTexture(int width, int height, Vector4 color)
        {
            Texture2D tex = new Texture2D(Global.GraphicsDevice, width, height);
            
            Color[] pixelData = new Color[tex.Width * tex.Height];
            for (int i = 0; i < pixelData.Length; i++)
            {
                pixelData[i] = Color.FromNonPremultiplied(color);
            }

            tex.SetData(pixelData);
            return tex;
        }

        public Texture2D GetWhitePixel()
        {
            return _whitePixel;
        }

        public Sprite Get8x8Tile(Texture2D tex, int tileX, int tileY, Vector2 offset)
        {
            return new Sprite(tex, (int)offset.X + tileX * 8, (int)offset.Y + tileY * 8, 8, 8);
        }

        public Sprite Get16x16Tile(Texture2D tex, int tileX, int tileY, Vector2 offset)
        {
            return new Sprite(tex, (int)offset.X + tileX * 16, (int)offset.Y + tileY * 16, 16, 16);
        }

        internal Sprite Get4x4Tile(Texture2D tex, int tileX, int tileY, Vector2 offset)
        {
            return new Sprite(tex, (int)offset.X + tileX * 4, (int)offset.Y + tileY * 4, 4, 4);
        }

        // Specifically used for ripping ruins tablet graphics
        internal Sprite Get64x64Tile(Texture2D tex, int index)
        {
            return new Sprite(tex, index * 64, 176, 64, 64);
        }
    }
}
