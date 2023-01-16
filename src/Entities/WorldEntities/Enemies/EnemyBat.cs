using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class EnemyBat : InteractableWorldEntity
    {
        public EnemyBat(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            // Fields 10 and above are programmatically treated as back fields. Enemy object bats automatically become back bats when placed behind them.
            _tex = Global.TextureManager.GetTexture(Global.Textures.ENEMY1);

            bool backsideBat = (Global.World.CurrField >= 10);

            Vector2 gfxOffset = new Vector2(80, 0);

            if (backsideBat)
                gfxOffset = new Vector2(64, 112);

            _sprIndex = new Sprite(_tex, (int)gfxOffset.X, (int)gfxOffset.Y, 16, 16);
            HP = 1;
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