using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class FieldTransition : IRoomWorldEntity
    {
        public FieldTransition(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            int direction = op1;
            int maskX = op2;    // Specifies the graphics that are overwritten in front of Lemeza when he passes through the gate
            int maskY = op3;
            int unused = op4;

            Rectangle BBox;

            switch (direction) {
                default:
                case 0:
                    // Left
                    BBox = new Rectangle((int)Position.X, (int)Position.Y, 8, 16);
                    break;
                case 1:
                    // Right
                    BBox = new Rectangle((int)Position.X, (int)Position.Y, 8, 16);
                    break;
                case 2:
                    // Down
                    BBox = new Rectangle((int)Position.X, (int)Position.Y, 16, 8);
                    break;
                case 3:
                    // Up
                    BBox = new Rectangle((int)Position.X, (int)Position.Y, 16, 8);
                    break;
            }
            _tex = Global.TextureManager.MakeTexture(BBox.Width, BBox.Height, new Vector4(255, 255, 255, 255));
            _sprIndex = new Sprite(_tex, 0, 8, BBox.Width, BBox.Height);
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