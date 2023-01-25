using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using System;

namespace OpenLaMulana
{
    public class AnimationTimer
    {
        private float _animationFrameTime = 1f / (Main.FPS / 2);
        private float _timeUntilNextFrame = 1.0f;
        private bool _canStepFrame = false;

        public int DrawOrder { get; set; } = 0;

        public Effect ActiveShader => null;

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        public void Update(GameTime gameTime)
        {
            Global.DeltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;
            _canStepFrame = false;

            _timeUntilNextFrame -= Global.DeltaTime;

            if (_timeUntilNextFrame <= 0)
            {
                _canStepFrame = true;
                _timeUntilNextFrame += _animationFrameTime;
            }
        }

        internal bool OneFrameElapsed(bool disregardPauseState = false)
        {
            if (!disregardPauseState)
                return _canStepFrame;
            return true;
        }
    }
}