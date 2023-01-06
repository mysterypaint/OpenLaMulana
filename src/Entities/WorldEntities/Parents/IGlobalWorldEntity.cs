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
        public int DrawOrder { get; set; } = 0;
        public Effect ActiveShader { get; set; } = null;
        internal Texture2D _tex;
        internal World _world;
        internal Sprite _sprIndex;
        public Vector2 Position;
        public Vector2 RelativeViewTilePos { get; }
        public bool Visible { get; set; } = true;
        public bool ManuallySpawned = false;

        List<IGameEntity> _myEntities = new List<IGameEntity>();

        internal View _parentView = null;
        internal Point viewCoords = new Point(-1, -1);
        internal float _imgScaleX = 1f, _imgScaleY = 1f;

        public IGlobalWorldEntity(int x, int y, int op1, int op2, int op3, int op4, View destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.DEBUG_ENTITY_TEMPLATE);
            _sprIndex = new Sprite(_tex, 16, 0, 16, 16);
            _world = Global.World;

            RelativeViewTilePos = new Vector2(x % World.ROOM_WIDTH, y % World.ROOM_HEIGHT);
            viewCoords = new Point(x / World.ROOM_WIDTH, y / World.ROOM_HEIGHT);
            Position = new Vector2(RelativeViewTilePos.X * World.CHIP_SIZE, RelativeViewTilePos.Y * World.CHIP_SIZE);
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        public void SetSprite(Sprite sprIndex)
        {
            _sprIndex = sprIndex;
        }

        ~IGlobalWorldEntity()
        {
            foreach (IGameEntity entity in _myEntities)
            {
                Global.EntityManager.RemoveEntity(entity);
            }

            Global.World.GetCurrField().GetFieldEntities().Remove(this);
        }

        public IGameEntity InstanceCreate(IGameEntity entity)
        {
            IGlobalWorldEntity gE = (IGlobalWorldEntity)entity;
            gE.ManuallySpawned = true;

            Global.EntityManager.AddEntity(gE);
            _myEntities.Add(gE);

            return gE;
        }

        public IGameEntity InstanceCreatePersistent(IGameEntity entity)
        {
            IGlobalWorldEntity gE = (IGlobalWorldEntity)entity;
            gE.ManuallySpawned = true;

            Global.EntityManager.AddEntity(gE);
            Global.World.GetCurrField().GetFieldEntities().Add(gE);

            return gE;
        }

        public IGameEntity InstanceDestroy(IGameEntity entity)
        {
            Global.EntityManager.RemoveEntity(entity);
            _myEntities.Remove(entity);

            return null;
        }
    }
}