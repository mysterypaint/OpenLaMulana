using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;
using static System.Net.Mime.MediaTypeNames;

namespace OpenLaMulana.Entities.WorldEntities.Parents
{
    public abstract class IGlobalWorldEntity : IGameEntity
    {
        public int Depth { get; set; } = (int)Global.DrawOrder.Entities;
        public Effect ActiveShader { get; set; } = null;
        internal Texture2D _tex;
        internal World _world;
        public List<ObjectStartFlag> StartFlags = null;
        internal Sprite _sprIndex;
        public Vector2 Position;
        public Point RelativeView { get; }
        public bool Visible { get; set; } = true;
        public bool ManuallySpawned = false;

        List<IGameEntity> _myEntities = new List<IGameEntity>();
        public Global.WEStates State { get; set; } = Global.WEStates.UNSPAWNED;
        
        internal View _parentView = null;
        public Point ViewCoords = new Point(-1, -1);
        public Point TrueGlobalTilePosition { get; }
        public Point TrueSpawnCoord { get; }

        internal float _imgScaleX = 1f, _imgScaleY = 1f;

        public IGlobalWorldEntity(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.DEBUG_ENTITY_TEMPLATE);
            _sprIndex = new Sprite(_tex, 16, 0, 16, 16);
            _world = Global.World;
            StartFlags = startFlags;

            if (StartFlags!= null)
            {
                if (HelperFunctions.EntityMaySpawn(StartFlags))
                    State = Global.WEStates.IDLE;
            }

            RelativeView = new Point(x % World.ROOM_WIDTH, y % World.ROOM_HEIGHT);
            ViewCoords = new Point(x / World.ROOM_WIDTH, y / World.ROOM_HEIGHT);
            TrueGlobalTilePosition = new Point(ViewCoords.X * World.ROOM_WIDTH, ViewCoords.Y * World.ROOM_HEIGHT) + RelativeView;
            TrueSpawnCoord = new Point(TrueGlobalTilePosition.X * World.CHIP_SIZE, TrueGlobalTilePosition.Y * World.CHIP_SIZE);
            
            Point tileOffset;
            Vector2 offsetCoords;

            /*
            if (sameField)
            {
                tileOffset = new Point(TrueGlobalTilePosition.X - (destView.X * World.ROOM_WIDTH), TrueGlobalTilePosition.Y - (destView.Y * World.ROOM_HEIGHT));
                offsetCoords = new Vector2(tileOffset.X * World.ROOM_WIDTH, tileOffset.Y * World.ROOM_HEIGHT);
                Position = TrueSpawnCoord.ToVector2() - offsetCoords;
            }*/
            Point roomOffset = new Point(ViewCoords.X - Global.World.CurrViewX, ViewCoords.Y - Global.World.CurrViewY);
            tileOffset = new Point(roomOffset.X * World.ROOM_WIDTH, roomOffset.Y * World.ROOM_HEIGHT) + RelativeView;
            offsetCoords = new Vector2(tileOffset.X * World.CHIP_SIZE, tileOffset.Y * World.CHIP_SIZE);

            Position = offsetCoords;
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