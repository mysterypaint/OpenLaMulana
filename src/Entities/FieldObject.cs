using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace OpenLaMulana.Entities.WorldEntities
{
    public class FieldObject : IGameEntity
    {
        public int DrawOrder { get; set; } = 0;
        public Effect ActiveShader { get; set; } = null;
        internal Texture2D _tex;
        internal World _world;
        public Vector2 Position;
        internal View _parentView;
        internal Point viewCoords = new Point(-1, -1);

        public FieldObject(int x, int y, int op1, int op2, int op3, int op4, Texture2D tex, World world, View destView)
        {
            _tex = tex;
            _world = world;
            _parentView = destView;
            viewCoords = new Point(_parentView.X, _parentView.Y);

            Position = new Vector2((x) * World.CHIP_SIZE, (y) * World.CHIP_SIZE);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if ((viewCoords.X == _world.CurrViewX) && (viewCoords.Y == _world.CurrViewY))
            {
                Rectangle srcRect;
                srcRect = new Rectangle(0, 0, 16, 16);

                Rectangle destRect = new Rectangle((int)Position.X, (int)Position.Y + Main.HUD_HEIGHT, 8, 8);
                spriteBatch.Draw(_tex, destRect, srcRect, Color.White);
                //_textManager.DrawText((int)Position.X, (int)Position.Y, "ok");
            }
        }

        public void Update(GameTime gameTime)
        {
        }
    }
}