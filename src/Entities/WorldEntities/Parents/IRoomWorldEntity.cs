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

        public Point TrueGlobalTilePosition { get; private set; }
        public Vector2 TrueSpawnCoord { get; private set; }
        public bool IsGlobal { get; private set; } = false;

        internal float _imgScaleX = 1f, _imgScaleY = 1f;
        public bool ManuallySpawned = false;
        public List<ObjectStartFlag> StartFlags = null;
        List<IGameEntity> _myEntities = new List<IGameEntity>();
        public Global.WEStates State { get; set; } = Global.WEStates.UNSPAWNED;

        public IRoomWorldEntity(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.DEBUG_ENTITY_TEMPLATE);
            _sprIndex = new Sprite(_tex, 0, 0, 16, 16);
            _world = Global.World;
            StartFlags = startFlags;

            if (!spawnIsGlobal)
            {
                RelativeViewTilePos = new Vector2(x, y);
                Position = new Vector2(RelativeViewTilePos.X * World.CHIP_SIZE, RelativeViewTilePos.Y * World.CHIP_SIZE);
            }
            else
            {
                RelativeViewTilePos = new Vector2(x % World.ROOM_WIDTH, y % World.ROOM_HEIGHT);
                viewCoords = new Point(x / World.ROOM_WIDTH, y / World.ROOM_HEIGHT);
                TrueGlobalTilePosition = new Point(viewCoords.X * World.ROOM_WIDTH, viewCoords.Y * World.ROOM_HEIGHT) + RelativeViewTilePos.ToPoint();
                TrueSpawnCoord = new Vector2(TrueGlobalTilePosition.X * World.CHIP_SIZE, TrueGlobalTilePosition.Y * World.CHIP_SIZE);
                IsGlobal = true;

                Point tileOffset;
                Vector2 offsetCoords;
                Point roomOffset = new Point(viewCoords.X - Global.World.CurrViewX, viewCoords.Y - Global.World.CurrViewY);
                tileOffset = new Point(roomOffset.X * World.ROOM_WIDTH, roomOffset.Y * World.ROOM_HEIGHT) + RelativeViewTilePos.ToPoint();
                offsetCoords = new Vector2(tileOffset.X * World.CHIP_SIZE, tileOffset.Y * World.CHIP_SIZE);

                Position = offsetCoords;
            }

            if (destView == null)
                destView = Global.World.GetCurrentView();
            _parentView = destView;
            viewCoords = new Point(_parentView.X, _parentView.Y);

            if (startFlags != null)
            {
                if (HelperFunctions.EntityMaySpawn(StartFlags))
                    State = Global.WEStates.IDLE;
            }
        }

        ~IRoomWorldEntity()
        {
            foreach (IGameEntity entity in _myEntities)
            {
                Global.EntityManager.RemoveEntity(entity);
            }

            Global.World.GetCurrField().GetViewEntities().Remove(this);
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