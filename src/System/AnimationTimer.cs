using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using System;

namespace OpenLaMulana
{
    public class AnimationTimer : IGameEntity
    {
        private float _animationFrameTime = 1f / Main.FPS;
        private float _timeUntilNextFrame = 1.0f;
        private bool _canStepFrame = false;

        public int DrawOrder => 0;

        public Effect ActiveShader => null;

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        public void Update(GameTime gameTime)
        {
            _canStepFrame = false;
            var gameFrameTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _timeUntilNextFrame -= gameFrameTime;

            if (_timeUntilNextFrame <= 0)
            {
                _canStepFrame = true;
                _timeUntilNextFrame += _animationFrameTime;
            }
        }

        internal bool OneFrameElapsed()
        {
            return _canStepFrame;
        }
    }
}