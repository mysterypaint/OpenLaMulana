using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class EnemyTogGenerator : IEnemyWorldEntity
    {
        public EnemyTogGenerator(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ENEMY1);
            _sprIndex = new Sprite(_tex, 0, 0, 16, 16);
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
        }
    }
}