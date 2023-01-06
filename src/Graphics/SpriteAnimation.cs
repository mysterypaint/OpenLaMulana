﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Graphics
{
    public class SpriteAnimation
    {
        public SpriteAnimation()
        {
        }

        private List<SpriteAnimationFrame> _frames = new List<SpriteAnimationFrame>();

        public SpriteAnimationFrame this[int index]
        {
            get
            {
                return GetFrame(index);

            }

        }

        public int FrameCount => _frames.Count;

        public SpriteAnimationFrame CurrentFrame
        {

            get
            {
                return _frames
                    .Where(f => f.TimeStamp <= PlaybackProgress)
                    .OrderBy(f => f.TimeStamp)
                    .LastOrDefault();

            }

        }

        public float Duration
        {

            get
            {

                if (!_frames.Any())
                    return 0;

                return _frames.Max(f => f.TimeStamp);

            }

        }

        public bool IsPlaying { get; private set; }

        public float PlaybackProgress { get; private set; }

        public bool ShouldLoop { get; set; } = true;

        public void AddFrame(Sprite sprite, float timeStamp)
        {

            SpriteAnimationFrame frame = new SpriteAnimationFrame(sprite, timeStamp);

            _frames.Add(frame);

        }

        public void Update(GameTime gameTime)
        {
            if (IsPlaying)
            {

                PlaybackProgress += (float)gameTime.ElapsedGameTime.TotalSeconds;

                if (PlaybackProgress > Duration)
                {
                    if (ShouldLoop)
                        PlaybackProgress -= Duration;
                    else
                        Stop();
                }

            }

        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position)
        {

            SpriteAnimationFrame frame = CurrentFrame;

            if (frame != null)
                frame.Sprite.Draw(spriteBatch, position);

        }

        public void Play()
        {

            IsPlaying = true;

        }

        public void Stop()
        {

            IsPlaying = false;
            PlaybackProgress = 0;

        }

        public SpriteAnimationFrame GetFrame(int index)
        {
            if (index < 0 || index >= _frames.Count)
                throw new ArgumentOutOfRangeException(nameof(index), "A frame with index " + index + " does not exist in this animation.");

            return _frames[index];

        }

        public void Clear()
        {

            Stop();
            _frames.Clear();

        }

        public static SpriteAnimation CreateSimpleAnimation(Texture2D texture, Point startPos, int width, int height, Point offset, int frameCount, float frameLength)
        {
            if (texture == null)
                throw new ArgumentNullException(nameof(texture));

            SpriteAnimation anim = new SpriteAnimation();

            for (int i = 0; i < frameCount; i++)
            {
                Sprite sprite = new Sprite(texture, startPos.X + i * offset.X, startPos.Y + i * offset.Y, width, height);
                anim.AddFrame(sprite, frameLength * i);

                if (i == frameCount - 1)
                    anim.AddFrame(sprite, frameLength * (i + 1));

            }

            return anim;

        }

        public static SpriteAnimation CreateSimpleAnimation(Sprite[] spriteArray, float frameLength)
        {
            SpriteAnimation anim = new SpriteAnimation();

            int i = 0;
            foreach(Sprite s in spriteArray)
            {
                Sprite sprite = new Sprite(s.Texture, s.X + i * s.OriginX, s.Y + i * s.OriginY, s.Width, s.Height);
                anim.AddFrame(sprite, frameLength * i);

                if (i == spriteArray.Length - 1)
                    anim.AddFrame(sprite, frameLength * (i + 1));
                i++;
            }

            return anim;
        }

        internal static SpriteAnimation CreateSimpleAnimation(Sprite sprite, float frameLength)
        {
            SpriteAnimation anim = new SpriteAnimation();
            anim.AddFrame(sprite, frameLength);
            return anim;
        }
    }
}
