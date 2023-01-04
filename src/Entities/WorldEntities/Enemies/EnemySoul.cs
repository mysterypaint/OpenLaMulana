using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class EnemySoul : IRoomWorldEntity
    {
        SpriteAnimation _sprAnimBlueSpinning, _sprAnimRedSpinning, _sprAnimBlueMoveDiagonal, _sprAnimBlueMoveCardinal, _sprAnimRedMoveDiagonal, _sprAnimRedMoveCardinal;
        private bool _isBlue = true;
        private float _blinkSpeed = 4f;

        enum SoulFrames
        {
            DOWN_LEFT,
            UP_LEFT,
            UP_RIGHT,
            DOWN_RIGHT,
            UP,
            RIGHT,
            DOWN,
            LEFT
        };

        public EnemySoul(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ENEMY1);

            // Use the 4 frames on the right side of this sprite's texture if we're in the Maze of Galious area
            int sprOffX = 0;
            if (Global.World.CurrField == 20)
                sprOffX = 64;

            _sprAnimBlueSpinning = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(sprOffX, 144), 16, 16, new Point(16, 0), 4, 0.1f);
            _sprAnimRedSpinning = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(sprOffX, 160), 16, 16, new Point(16, 0), 4, 0.1f);
            _sprAnimBlueMoveDiagonal = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(sprOffX, 176), 16, 16, new Point(16, 0), 4, 0.1f);
            _sprAnimBlueMoveCardinal = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(sprOffX, 192), 16, 16, new Point(16, 0), 4, 0.1f);
            _sprAnimRedMoveDiagonal = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(sprOffX, 208), 16, 16, new Point(16, 0), 4, 0.1f);
            _sprAnimRedMoveCardinal = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(sprOffX, 224), 16, 16, new Point(16, 0), 4, 0.1f);

            //_sprIndex = _sprAnimBlueSpinning.GetFrame(0).Sprite;
            _sprAnimBlueSpinning.Play();
            _sprAnimRedSpinning.Play();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (viewCoords.X == _world.CurrViewX && viewCoords.Y == _world.CurrViewY)
            {
                if (gameTime.TotalGameTime.Ticks % (_blinkSpeed * 6) == 0)
                    _isBlue = !_isBlue;

                if (_isBlue)
                    _sprIndex = _sprAnimBlueSpinning.CurrentFrame.Sprite;
                else
                    _sprIndex = _sprAnimRedSpinning.CurrentFrame.Sprite;
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
            }
        }

        public override void Update(GameTime gameTime)
        {
            _sprAnimBlueSpinning.Update(gameTime);
            _sprAnimRedSpinning.Update(gameTime);
            _sprAnimBlueMoveDiagonal.Update(gameTime);
            _sprAnimBlueMoveCardinal.Update(gameTime);
            _sprAnimRedMoveDiagonal.Update(gameTime);
            _sprAnimRedMoveCardinal.Update(gameTime);
        }
    }
}