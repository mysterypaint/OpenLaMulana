using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GuardianViyTentacle : IGlobalWorldEntity
    {
        private SpriteAnimation _sprAnim = null;

        public GuardianViyTentacle(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.DEBUG_ENTITY_TEMPLATE);
            _sprIndex = new Sprite(_tex, 16, 0, 16, 16);
            _world = Global.World;
            _imgScaleX = 0.5f;
            _imgScaleY = 0.5f;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _sprAnim.Draw(spriteBatch, Position);
        }

        public override void Update(GameTime gameTime)
        {
            _sprAnim.Update(gameTime);
        }

        public new virtual void SetSprite(Sprite sprIndex)
        {
            _sprIndex = sprIndex;
            _imgScaleX = 1;
            _imgScaleY = 1;
        }

        internal void Init(SpriteAnimation sprAnim, Vector2 position)
        {
            Position = position;
            _sprAnim = sprAnim;
            _sprAnim.Play();
        }
    }
}