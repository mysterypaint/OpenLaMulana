﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace OpenLaMulana.Entities.WorldEntities
{
    public class IRoomWorldEntity : IGameEntity
    {
        public int DrawOrder => 0;
        public Effect ActiveShader { get; set; } = null;
        internal Texture2D _tex;
        internal World _world;
        public Vector2 RelativeViewPos;
        public Vector2 RelativeViewTilePos { get; }

        internal View _parentView = null;
        internal Point viewCoords= new Point(-1, -1);

        public IRoomWorldEntity(int x, int y, int op1, int op2, int op3, int op4, View destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.DEBUG_ENTITY_TEMPLATE);
            _world = Global.World;

            RelativeViewTilePos = new Vector2(x, y);
            RelativeViewPos = new Vector2(RelativeViewTilePos.X * World.CHIP_SIZE, RelativeViewTilePos.Y * World.CHIP_SIZE);
            _parentView = destView;
            viewCoords = new Point(_parentView.X, _parentView.Y);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if ((viewCoords.X == _world.CurrViewX) && (viewCoords.Y == _world.CurrViewY)) {
                Rectangle srcRect;

                srcRect = new Rectangle(0, 0, 16, 16);
                Rectangle destRect = new Rectangle(
                    (int)RelativeViewPos.X,
                    (int)RelativeViewPos.Y + Main.HUD_HEIGHT,
                    8, 8);
                spriteBatch.Draw(_tex, destRect, srcRect, Color.White);
                    //_textManager.DrawText((int)Position.X, (int)Position.Y, "ok");
            }
        }

        public void Update(GameTime gameTime)
        {
        }
    }
}