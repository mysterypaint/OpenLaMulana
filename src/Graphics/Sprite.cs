using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Drawing;
using static OpenLaMulana.Global;
using Color = Microsoft.Xna.Framework.Color;
using Rectangle = Microsoft.Xna.Framework.Rectangle;

namespace OpenLaMulana.Graphics
{
    public class Sprite
    {
        private Texture2D tex;
        private Vector2 texCoords;
        private Vector2 spriteWidthHeight;
        private float Rotation = 0.0f;
        private Vector2 Origin = Vector2.Zero;
        private SpriteEffects SpriteEffects = SpriteEffects.None;
        private float LayerDepth = 0.0f;
        private bool _flipX = false;
        private bool _flipY = false;

        public Texture2D Texture { get; set; } = null;

        public int X { get; set; } = 0;
        public int Y { get; set; } = 0;

        public int Width { get; set; } = 0;
        public int Height { get; set; } = 0;

        public int OriginX { get; set; } = 0;
        public int OriginY { get; set; } = 0;

        public Color TintColor { get; set; } = Color.White;
        public Vector2 Scale { get; set; } = Vector2.One;

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

        public Sprite(Texture2D texture, Vector2 coords, Vector2 size)
        {
            Texture = texture;
            X = (int)coords.X;
            Y = (int)coords.Y;
            Width = (int)size.X;
            Height = (int)size.Y;
            OriginX = 0;
            OriginY = 0;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {

            SpriteEffects flipXSE = SpriteEffects.None;
            SpriteEffects flipYSE = SpriteEffects.None;
            if (_flipX)
                flipXSE = SpriteEffects.FlipHorizontally;
            if (_flipY)
                flipYSE = SpriteEffects.FlipVertically;
            spriteBatch.Draw(Texture, new Vector2((int)Math.Round(position.X - OriginX), (int)Math.Round(position.Y - OriginY)), new Rectangle(X, Y, Width, Height), TintColor, Rotation, Origin, Scale, SpriteEffects, LayerDepth);

        }

        internal void DrawScaled(SpriteBatch spriteBatch, Vector2 position, float scaleX, float scaleY)
        {
            Rectangle srcRect = new Rectangle(X, Y, Width, Height);
            Rectangle destRect = new Rectangle(
                (int)Math.Round(position.X - OriginX),
                (int)Math.Round(position.Y - OriginY), (int)Math.Round(Width * scaleX), (int)Math.Round(Height * scaleY));
            //spriteBatch.Draw(Texture, destRect, srcRect, TintColor);
            SpriteEffects flipXSE = SpriteEffects.None;
            SpriteEffects flipYSE = SpriteEffects.None;
            if (_flipX)
                flipXSE = SpriteEffects.FlipHorizontally;
            if (_flipY)
                flipYSE = SpriteEffects.FlipVertically;
            SpriteEffects = flipXSE;
            spriteBatch.Draw(Texture, destRect, srcRect, TintColor, Rotation, Origin, SpriteEffects, LayerDepth);
        }

        internal Sprite Clone()
        {
            return this;
        }

        internal void FlipX(bool value)
        {
            _flipX = value;
        }

        internal void FlipY(bool value)
        {
            _flipY = value;
        }

    }
}
