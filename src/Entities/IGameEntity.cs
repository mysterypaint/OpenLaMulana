using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenLaMulana.Entities
{
    public interface IGameEntity
    {
        int Depth { get; set; }
        Effect ActiveShader { get; }
        bool LockTo30FPS { get; set; }
        bool FrozenWhenCameraIsBusy { get; set; }

        void Draw(SpriteBatch spriteBatch, GameTime gameTime);
        void Update(GameTime gameTime);
    }
}
