using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenLaMulana.Graphics
{
    public class Sprite
    {

        public Texture2D Texture { get; set; }

        public int X { get; set; }
        public int Y { get; set; }

        public int Width { get; set; }
        public int Height { get; set; }

        public int OriginX { get; set; }
        public int OriginY { get; set; }

        public Color TintColor { get; set; } = Color.White;

        public Sprite(Texture2D texture, int x, int y, int width, int height)
        {
            Texture = texture;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            OriginX = 0;
            OriginY = 0;
        }

        public Sprite(Texture2D texture, int x, int y, int width, int height, int originX, int originY)
        {
            Texture = texture;
            X = x;
            Y = y;
            Width = width;
            Height = height;
            OriginX = originX;
            OriginY = originY;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {

            spriteBatch.Draw(Texture, new Vector2((int)Math.Round(position.X - OriginX), (int)Math.Round(position.Y - OriginY)), new Rectangle(X, Y, Width, Height), TintColor);

        }

    }
}
