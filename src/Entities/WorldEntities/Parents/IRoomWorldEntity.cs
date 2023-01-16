using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;

namespace OpenLaMulana
{
    abstract class IRoomWorldEntity : IGameEntity
    {
        public int Depth { get; set; } = (int)Global.DrawOrder.Entities;
        public Effect ActiveShader { get; set; } = null;
        internal Texture2D _tex;
        internal World _world;
        internal Sprite _sprIndex;
        public Vector2 Position;
        public Vector2 RelativeViewTilePos { get; }
        public bool Visible { get; set; } = true;
        public short BBoxOriginX { get; set; } = 0;
        public short BBoxOriginY { get; set; } = 0;

        internal View _parentView = null;
        internal Point viewCoords = new Point(-1, -1);
        internal float _imgScaleX = 1f, _imgScaleY = 1f;
        public bool ManuallySpawned = false;
        public List<ObjectStartFlag> _startFlags = null;
        List<IGameEntity> _myEntities = new List<IGameEntity>();
        public Global.WEStates State { get; set; } = Global.WEStates.UNSPAWNED;

        public IRoomWorldEntity(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.DEBUG_ENTITY_TEMPLATE);
            _sprIndex = new Sprite(_tex, 0, 0, 16, 16);
            _world = Global.World;
            _startFlags = startFlags;

            RelativeViewTilePos = new Vector2(x, y);
            Position = new Vector2(RelativeViewTilePos.X * World.CHIP_SIZE, RelativeViewTilePos.Y * World.CHIP_SIZE);

            if (destView == null)
                destView = Global.World.GetCurrentView();
            _parentView = destView;
            viewCoords = new Point(_parentView.X, _parentView.Y);
        }

        ~IRoomWorldEntity()
        {
            foreach (IGameEntity entity in _myEntities)
            {
                Global.EntityManager.RemoveEntity(entity);
            }

            Global.World.GetCurrField().GetRoomEntities().Remove(this);
        }

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        public void SetSprite(Sprite sprite)
        {
            _sprIndex = sprite;
        }

        public IGameEntity InstanceCreate(IGameEntity entity)
        {
            IRoomWorldEntity rE = (IRoomWorldEntity)entity;
            rE.ManuallySpawned = true;

            Global.EntityManager.AddEntity(rE);
            _myEntities.Add(rE);

            return rE;
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