using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace OpenLaMulana.Entities.WorldEntities.Parents
{
    abstract class IGlobalWorldEntity : IGameEntity
    {
        public int DrawOrder => 0;
        public Effect ActiveShader { get; set; } = null;
        internal Texture2D _tex;
        internal Sprite _spr;
        internal World _world;
        public Vector2 RelativeViewPos;
        public Vector2 RelativeViewTilePos { get; }

        internal View _parentView = null;
        internal Point viewCoords = new Point(-1, -1);
        internal float _imgScaleX = 1f, _imgScaleY = 1f;

        public IGlobalWorldEntity(int x, int y, int op1, int op2, int op3, int op4, View destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.DEBUG_ENTITY_TEMPLATE);
            _spr = new Sprite(_tex, 16, 0, 16, 16);
            _world = Global.World;

            RelativeViewTilePos = new Vector2(x % World.ROOM_WIDTH, y % World.ROOM_HEIGHT);
            viewCoords = new Point(x / World.ROOM_WIDTH, y / World.ROOM_HEIGHT);
            RelativeViewPos = new Vector2(RelativeViewTilePos.X * World.CHIP_SIZE, RelativeViewTilePos.Y * World.CHIP_SIZE);
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (viewCoords.X == _world.CurrViewX && viewCoords.Y == _world.CurrViewY)
            {
                _spr.DrawScaled(spriteBatch, RelativeViewPos + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                //_textManager.DrawText((int)Position.X, (int)Position.Y, "ok");
            }
        }

        public void Update(GameTime gameTime)
        {
        }
    }
}