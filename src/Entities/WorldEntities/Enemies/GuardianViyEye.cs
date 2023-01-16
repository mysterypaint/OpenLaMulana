using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GuardianViyEye : IGlobalWorldEntity
    {
        private GuardianViy _parent;

        public GuardianViyEye(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = null;
            _sprIndex = null;
            _world = Global.World;
            _imgScaleX = 0.5f;
            _imgScaleY = 0.5f;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _sprIndex.Draw(spriteBatch, Position);
        }

        public override void Update(GameTime gameTime)
        {
        }

        public new virtual void SetSprite(Sprite sprIndex)
        {
            _sprIndex = sprIndex;
            _imgScaleX = 1;
            _imgScaleY = 1;
        }

        internal void Init(Sprite[] eyeFrames, Vector2 position, GuardianViy parent)
        {
            Position = position;
            _sprIndex = eyeFrames[0];
            _parent = parent;
            Depth = _parent.Depth + 1;
        }
    }
}