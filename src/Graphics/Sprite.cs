using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Graphics
{
    public class Sprite
    {

        public Texture2D Texture { get; set; } = null;

        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;

        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;

        public int OriginX { get; set; } = 0;
        public int OriginY { get; set; } = 0;

        public Color TintColor { get; set; } = Color.White;
        public Sprite()
        {
            Texture = null;
            X = 0;
            Y = 0;
            Width = 0;
            Height = 0;
            OriginX = 0;
            OriginY = 0;
        }

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

        internal void DrawScaled(SpriteBatch spriteBatch, Vector2 position, float scaleX, float scaleY)
        {
            Rectangle srcRect = new Rectangle(X, Y, Width, Height);
            Rectangle destRect = new Rectangle(
                (int)Math.Round(position.X - OriginX),
                (int)Math.Round(position.Y - OriginY), (int)Math.Round(Width * scaleX), (int)Math.Round(Height * scaleY));
            spriteBatch.Draw(Texture, destRect, srcRect, TintColor);
        }

        internal Sprite Clone()
        {
            return this;
        }
    }
}
