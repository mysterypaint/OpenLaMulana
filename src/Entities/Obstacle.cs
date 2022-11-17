﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenLaMulana.Entities
{

    public abstract class Obstacle : IGameEntity, ICollidable
    {
        private Protag _trex;

        public abstract Rectangle CollisionBox { get; }

        public int DrawOrder { get; set; }

        public Vector2 Position { get; protected set; }

        public short BBoxOriginX { get; set; }
        public short BBoxOriginY { get; set; }

        protected Obstacle(Protag trex, Vector2 position)
        {
            _trex = trex;
            Position = position;
            BBoxOriginX = 0;
            BBoxOriginY = 0;
        }

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        public virtual void Update(GameTime gameTime)
        {
            float posX = Position.X;// - _trex.Speed * (float)gameTime.ElapsedGameTime.TotalSeconds;

            Position = new Vector2(posX, Position.Y);

            CheckCollisions();

        }

        private void CheckCollisions()
        {
            Rectangle obstacleCollisionBox = CollisionBox;
            Rectangle trexCollisionBox = _trex.CollisionBox;

            if (obstacleCollisionBox.Intersects(trexCollisionBox))
            {
                //_trex.Die();
            }

        }

    }
}
