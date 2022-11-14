using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace OpenLaMulana.Entities
{

    public abstract class Obstacle : IGameEntity, ICollidable
    {
        private Protag _trex;

        public abstract Rectangle CollisionBox { get; }

        public int DrawOrder { get; set; }

        public Vector2 Position { get; protected set; }

        public short bBoxOriginX { get; set; }
        public short bBoxOriginY { get; set; }

        protected Obstacle(Protag trex, Vector2 position)
        {
            _trex = trex;
            Position = position;
            bBoxOriginX = 0;
            bBoxOriginY = 0;
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

            if(obstacleCollisionBox.Intersects(trexCollisionBox))
            {
                //_trex.Die();
            }

        } 

    }
}
