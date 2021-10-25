using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenLaMulana.Entities
{
    public abstract class SkyObject : IGameEntity
    {
        protected readonly Protag _trex;

        public int DrawOrder { get; set; }

        public abstract float Speed { get; }

        public Vector2 Position { get; set; }

        protected SkyObject(Protag trex, Vector2 position)
        {
            _trex = trex;
            Position = position;
        }

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);

        public virtual void Update(GameTime gameTime)
        {
            if(_trex.IsAlive)
                Position = new Vector2(Position.X - Speed * (float)gameTime.ElapsedGameTime.TotalSeconds, Position.Y);

        }

    }
}
