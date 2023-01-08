using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection.Metadata;
using System.Text.RegularExpressions;
using static OpenLaMulana.Global;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace OpenLaMulana
{
    public class SpriteDefManager : IGameEntity
    {
        private Dictionary<Global.SpriteDefs, List<SpriteDef>> _sprDefSheets = new Dictionary<Global.SpriteDefs, List<SpriteDef>>();
        private Dictionary<Global.SpriteDefs, List<Sprite>> _globalSprites = new Dictionary<SpriteDefs, List<Sprite>>();

        private int _chipSize = OpenLaMulana.Entities.World.CHIP_SIZE;

        public SpriteDefManager()
        {
            InitSpriteDef();
            InitSpriteLibrary();
        }

        private void InitSpriteLibrary()
        {
            for (Global.SpriteDefs sheetID = Global.SpriteDefs.BOSS01; sheetID < Global.SpriteDefs.MAX; sheetID++) {
                List<SpriteDef> spriteDefs = _sprDefSheets[sheetID];

                List<Sprite> spriteSheet = new List<Sprite>();
                int index = 0;
                foreach (SpriteDef s in spriteDefs) {
                    Sprite spr = InitSprite(sheetID, index);
                    spriteSheet.Add(spr);
                    index++;
                }

                _globalSprites.Add(sheetID, spriteSheet);
            }
        }

        public Sprite GetSprite(Global.SpriteDefs sheet, int index)
        {
            List<Sprite> sprAtlas = _globalSprites[sheet];
            Sprite spr = sprAtlas[index];
            return spr;
        }

        private void InitSpriteDef()
        {
            string[] allSpriteDefs = { "boss01",
                "boss03",
                "boss04",
                "boss05"
            };


            string spriteDefsPath = "Content/spritedefs/";

            for (Global.SpriteDefs sheetID = Global.SpriteDefs.BOSS01; sheetID < Global.SpriteDefs.MAX; sheetID++)
            {
                string fName = allSpriteDefs[(int)sheetID] + ".spritedef";
                string path = Path.Combine(spriteDefsPath, fName);
                string[] defContents = File.ReadAllLines(path);

                List<SpriteDef> newSheet = new List<SpriteDef>();
                SpriteDef newSpriteDef = null;

                int spriteWidth = 0;
                int spriteHeight = 0;

                foreach (string line in defContents)
                {
                    if (line.StartsWith("[Sprite "))
                    {
                        int.TryParse(line.Split(" ")[1].Split("]")[0], out int sprIndex);

                        if (newSpriteDef == null)
                            newSpriteDef = new SpriteDef(sprIndex);
                        else
                        {
                            newSpriteDef.SetSize(new Vector2(spriteWidth, spriteHeight));
                            newSheet.Add(newSpriteDef);
                            newSpriteDef = new SpriteDef(sprIndex);
                            spriteWidth = 0;
                            spriteHeight = 0;
                        }
                    }
                    else if (line.StartsWith("O:"))
                    {
                        string[] vectorCoords = line.Split(":")[1].Split(",");
                        int.TryParse(vectorCoords[0], out int originX);
                        int.TryParse(vectorCoords[1], out int originY);

                        newSpriteDef.SetOrigin(new Vector2(originX, originY));
                    }
                    else if (line.Trim().Length > 0)
                    {
                        string[] potentiallyANumber = line.Split(":");

                        if (Regex.IsMatch(potentiallyANumber[0], @"^\d+$"))
                        {
                            int[] frameData = new int[4];

                            string[] frameDataAsString = potentiallyANumber[1].Split(",");

                            for (var i = 0; i < frameDataAsString.Length; i++)
                            {
                                if (int.TryParse(frameDataAsString[i], out int value))
                                {
                                    frameData[i] = value;
                                }
                            }

                            int x = frameData[0];
                            int y = frameData[1];
                            int width = frameData[2];
                            int height = frameData[3];
                            int totalFrameWidth = x + width;
                            int totalFrameHeight = y + height;

                            if (spriteWidth <= totalFrameWidth)
                                spriteWidth = totalFrameWidth;
                            if (spriteHeight <= totalFrameHeight)
                                spriteHeight = totalFrameHeight;

                            Rectangle spriteRegion = new Rectangle(x, y, width, height);

                            newSpriteDef.AddAssemblyData(spriteRegion);
                        }
                    }

                    //char.IsDigit(line[0]) && line[1] == ':'
                }
                newSpriteDef.SetSize(new Vector2(spriteWidth, spriteHeight));
                newSheet.Add(newSpriteDef);
                //newSpriteDef = null;

                _sprDefSheets.Add(sheetID, newSheet);
                //List<SpriteDef> spriteDefs = new List<SpriteDef>();
                //
            }
        }

        List<SpriteDef> GetDefSheet(Global.SpriteDefs sheetID)
        {
            return _sprDefSheets[sheetID];
        }

        public Texture2D GetTexture(Global.SpriteDefs sheetID)
        {
            switch (sheetID)
            {
                default:
                case Global.SpriteDefs.BOSS01:
                    return Global.TextureManager.GetTexture(Global.Textures.BOSS01);
                case Global.SpriteDefs.BOSS03:
                    return Global.TextureManager.GetTexture(Global.Textures.BOSS03);
                case Global.SpriteDefs.BOSS04:
                    return Global.TextureManager.GetTexture(Global.Textures.BOSS04);
                case Global.SpriteDefs.BOSS05:
                    return Global.TextureManager.GetTexture(Global.Textures.BOSS05);
            }
        }

        public int Depth { get; set; } = (int)Global.DrawOrder.Abstract;
        Effect IGameEntity.ActiveShader => null;


        internal Sprite InitSprite(SpriteDefs sheetID, int index)
        {
            // Get details about the sprite definition
            List<SpriteDef> defSheet = _sprDefSheets[sheetID];
            SpriteDef sprDef = defSheet[index];
            Vector2 origin = sprDef.GetOrigin();
            List<Rectangle> assemblyData = sprDef.GetAssemblyData();
            Vector2 size = sprDef.GetSize();
            
            int sprWidth = (int)size.X;
            int sprHeight = (int)size.Y;

            // Initialize a texture
            Texture2D tex = new Texture2D(Global.GraphicsDevice, sprWidth * _chipSize, sprHeight * _chipSize);
            Texture2D srcTex = GetTexture(sheetID);
            Color[] mainPxData = new Color[sprWidth * sprHeight * _chipSize * _chipSize];

            foreach (Rectangle rect in assemblyData)
            {
                Rectangle r = new Rectangle((int)(rect.X + origin.X) * _chipSize,
                    (int)(rect.Y + origin.Y) * _chipSize,
                    rect.Width * _chipSize,
                    rect.Height * _chipSize);

                // Cut out a small piece of the source texture using this Rectangle
                Color[] srcPxData = GetPxData(srcTex, r);

                // Iterate through the bounds of the hole we cut out. For every single pixel, print it to the output sprite, relative to its final texture position

                for (var y = 0; y < r.Height; y++)
                {
                    for (var x = 0; x < r.Width; x++)
                    {
                        Color color = GetPixel(srcPxData, x, y, r.Width);
                        SetPixel(mainPxData, (rect.X * _chipSize) + x, (rect.Y * _chipSize) + y, sprWidth * _chipSize, color);
                    }
                }
                /*
                for (var y = r.Y; y < r.Y + r.Height; y++)
                {
                    for (var x = r.X; x < r.X + r.Width; x++)
                    {
                        Color color = GetPixel(srcPxData, x - r.X, y - r.Y, r.Width);
                        SetPixel(mainPxData, r.X - (int)(origin.X * _chipSize), r.Y - (int)(origin.Y * _chipSize), sprWidth * _chipSize, color);
                    }
                }
                */


            }

            tex.SetData(mainPxData);


            // Return the final texture

            //Sprite spr = new Sprite(tex, 0, 0, sprWidth, sprHeight, 0, 0);
            Sprite spr = new Sprite(tex, 0, 0, sprWidth * _chipSize, sprHeight * _chipSize, 0, 0);

            return spr;
        }

        private void SetPixel(Color[] data, int x, int y, int width, Color color)
        {
            data[(y * width) + x] = color;
        }

        private Color GetPixel(Color[] data, int x, int y, int width)
        {
            return data[(y * width) + x];
        }

        private Color[] GetPxData(Texture2D srcTex, Rectangle extractRegion)
        {
            int width = extractRegion.Width;
            int height = extractRegion.Height;

            Color[] regionData = new Color[width * height];
            srcTex.GetData<Color>(0, extractRegion, regionData, 0, width * height);

            return regionData;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        public void Update(GameTime gameTime)
        {
        }
    }
}