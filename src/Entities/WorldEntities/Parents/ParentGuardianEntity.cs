using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities.Enemies.Guardians
{
    internal class ParentGuardianEntity : ParentAssembledInteractiveWorldEntity
    {
        public ParentGuardianEntity(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        public override void Update(GameTime gameTime)
        {
        }
    }
}