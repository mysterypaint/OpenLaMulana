using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;

namespace OpenLaMulana
{
    public abstract class ParentWorldEntity : IGameEntity
    {
        public virtual int Depth { get; set; } = (int)Global.DrawOrder.Entities;
        public virtual Effect ActiveShader { get; set; } = null;
        public virtual Texture2D _tex { get; set; } = null;
        internal World _world;
        internal virtual Sprite _sprIndex { get; set; } = null;
        public Vector2 Position;
        public Vector2 RelativeViewChipPos { get; }
        public bool Visible { get; set; } = true;
        public virtual short BBoxOriginX { get; set; } = 0;
        public virtual short BBoxOriginY { get; set; } = 0;
        public virtual bool LockTo30FPS { get; set; } = true;
        public virtual bool LockToCamera { get; set; } = false;

        internal View _parentView = null;
        public Point ViewCoords = new Point(-1, -1);

        public Point TrueGlobalTilePosition { get; private set; }
        public Vector2 TrueSpawnCoord { get; private set; }
        public bool IsGlobal { get; private set; } = false;

        internal float _imgScaleX = 1f, _imgScaleY = 1f;
        public bool ManuallySpawned = false;
        public List<ObjectStartFlag> StartFlags = null;
        List<IGameEntity> _myEntities = new List<IGameEntity>();
        internal Vector2 OriginPosition = Vector2.Zero;
        internal Vector2 OriginDisplacement = Vector2.Zero;

        public Global.WEStates State { get; set; } = Global.WEStates.UNSPAWNED;
        public Point RoomsTravelled { get; internal set; } = Point.Zero;

        public ParentWorldEntity(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.DEBUG_ENTITY_TEMPLATE);
            _sprIndex = new Sprite(_tex, 0, 0, 16, 16);
            _world = Global.World;
            StartFlags = startFlags;
            IsGlobal = spawnIsGlobal;

            if (!(this is Protag))
            {
                if (destView == null)
                    destView = Global.World.GetCurrentView();
            } else
            {
                destView = new View(1, 1, null, -1, -1, false);
            }
            _parentView = destView;

            if (!spawnIsGlobal)
            {
                RelativeViewChipPos = new Vector2(x, y);
                Position = new Vector2(RelativeViewChipPos.X * World.CHIP_SIZE, RelativeViewChipPos.Y * World.CHIP_SIZE);
                ViewCoords = new Point(_parentView.X, _parentView.Y);
            }
            else
            {
                RelativeViewChipPos = new Vector2(x % World.VIEW_WIDTH, y % World.VIEW_HEIGHT);
                ViewCoords = new Point(x / World.VIEW_WIDTH, y / World.VIEW_HEIGHT);
                TrueGlobalTilePosition = new Point(x, y);
                TrueSpawnCoord = new Vector2(TrueGlobalTilePosition.X * World.CHIP_SIZE, TrueGlobalTilePosition.Y * World.CHIP_SIZE);
                IsGlobal = true;

                Point tileOffset;
                Vector2 offsetCoords;
                Point roomOffset = new Point(ViewCoords.X - destView.X, ViewCoords.Y - destView.Y);
                tileOffset = new Point(roomOffset.X * World.VIEW_WIDTH, roomOffset.Y * World.VIEW_HEIGHT) + RelativeViewChipPos.ToPoint();
                offsetCoords = new Vector2(tileOffset.X * World.CHIP_SIZE, tileOffset.Y * World.CHIP_SIZE);
                Position = Vector2.Zero;
                OriginPosition = offsetCoords;
            }

            if (startFlags != null)
            {
                if (HelperFunctions.EntityMaySpawn(StartFlags))
                    State = Global.WEStates.IDLE;
            }
        }

        ~ParentWorldEntity()
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
            ParentWorldEntity rE = (ParentWorldEntity)entity;
            rE.ManuallySpawned = true;

            Global.EntityManager.AddEntity(rE);
            _myEntities.Add(rE);

            return rE;
        }

        public IGameEntity InstanceCreatePersistent(IGameEntity entity)
        {
            ParentWorldEntity wE = (ParentWorldEntity)entity;
            wE.ManuallySpawned = true;
            wE.IsGlobal = true;

            Global.EntityManager.AddEntity(wE);
            Global.World.GetCurrField().GetFieldEntities().Add(wE);

            return wE;
        }

        public IGameEntity InstanceDestroy(IGameEntity entity)
        {
            Global.EntityManager.RemoveEntity(entity);
            _myEntities.Remove(entity);

            return null;
        }
    }
}