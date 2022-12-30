using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenLaMulana.Entities
{
    public interface IGameEntity
    {
        int DrawOrder { get; }
        //Effect activeShader { get; }

        void Update(GameTime gameTime);

        void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    }
}
