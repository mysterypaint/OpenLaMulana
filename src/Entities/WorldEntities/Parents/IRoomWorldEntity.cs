using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace OpenLaMulana.Entities.WorldEntities.Parents
{
    abstract class IRoomWorldEntity : IGameEntity
    {
        public int DrawOrder => 0;
        public Effect ActiveShader { get; set; } = null;
        internal Texture2D _tex;
        internal World _world;
        internal Sprite _sprIndex;
        public Vector2 Position;
        public Vector2 RelativeViewTilePos { get; }

        internal View _parentView = null;
        internal Point viewCoords = new Point(-1, -1);
        internal float _imgScaleX = 1f, _imgScaleY = 1f;

        public IRoomWorldEntity(int x, int y, int op1, int op2, int op3, int op4, View destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.DEBUG_ENTITY_TEMPLATE);
            _sprIndex = new Sprite(_tex, 0, 0, 16, 16);
            _world = Global.World;

            RelativeViewTilePos = new Vector2(x, y);
            Position = new Vector2(RelativeViewTilePos.X * World.CHIP_SIZE, RelativeViewTilePos.Y * World.CHIP_SIZE);
            _parentView = destView;
            viewCoords = new Point(_parentView.X, _parentView.Y);
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    }
}