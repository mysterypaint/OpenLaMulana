﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenLaMulana.Entities
{
    public class RectangleSprite
    {
        static Texture2D _pointTexture;
        public static void DrawRectangle(SpriteBatch spriteBatch, Rectangle rectangle, Color color, int lineWidth)
        {
            if (_pointTexture == null)
            {
                _pointTexture = new Texture2D(spriteBatch.GraphicsDevice, 1, 1);
                _pointTexture.SetData<Color>(new Color[] { Color.White });
            }

            var _x = (int)Math.Round((float)rectangle.X);
            var _y = (int)Math.Round((float)rectangle.Y);

            spriteBatch.Draw(_pointTexture, new Rectangle(_x, _y, lineWidth, rectangle.Height - 1 + lineWidth), color); // bbox left
            spriteBatch.Draw(_pointTexture, new Rectangle(_x, _y, rectangle.Width - 1 + lineWidth, lineWidth), color); // bbox top
            spriteBatch.Draw(_pointTexture, new Rectangle(_x + rectangle.Width - 1, _y, lineWidth, rectangle.Height - 1 + lineWidth), color); // bbox right
            spriteBatch.Draw(_pointTexture, new Rectangle(_x, _y + rectangle.Height - 1, rectangle.Width - 1 + lineWidth, lineWidth), color); // bbox bottom
        }
    }
}
