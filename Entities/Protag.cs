using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Tracing;
using System.Text;
using OpenLaMulana.Graphics;
using Microsoft.Xna.Framework.Input;

namespace OpenLaMulana.Entities
{
    public class Protag : IGameEntity, ICollidable
    {
        enum Facing
        {
            LEFT,
            UP,
            RIGHT,
            DOWN
        };

        private Sprite _idleSprite;

        private SoundEffect _jumpSound;

        private Random _random;

        public event EventHandler JumpComplete;
        public event EventHandler Died;

        public PlayerState State { get; private set; }

        private World _world;

        public Vector2 Position { get; set; }
        public int moveX = 0;
        public int moveY = 0;
        public float hsp = 0;
        public float vsp = 0;
        private Facing facingX = Facing.RIGHT;
        private Facing facingY = Facing.DOWN;

        public int DrawOrder { get; set; }

        public Rectangle CollisionBox
        {
            get
            {
                Rectangle box = new Rectangle(
                    (int)Math.Round(Position.X),
                    (int)Math.Round(Position.Y),
                    16,
                    16
                );
                //box.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);
                return box;
            }
        }

        public Protag(Texture2D spriteSheet, World world, Vector2 position, SoundEffect jumpSound)
        {
            _world = world;

            Position = position;
            State = PlayerState.IDLE;

            _jumpSound = jumpSound;

            _random = new Random();

            _idleSprite = new Sprite(spriteSheet, 0, 0, 16, 16);
        }

        public void Initialize()
        {
            State = PlayerState.IDLE;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _idleSprite.Draw(spriteBatch, Position);
        }

        public void Update(GameTime gameTime)
        {
            switch (State)
            {
                case PlayerState.IDLE:

                    break;
                default:
                    break;
            }
        }

        internal string PrintState()
        {
            string str = "Undefined";
            switch (State)
            {
                case PlayerState.IDLE:
                    return "Idle";
                case PlayerState.CUTSCENE:
                    return "Cutscene";
                case PlayerState.FALLING:
                    return "Falling";
                case PlayerState.JUMPING:
                    return "Jumping";
                case PlayerState.MAX:
                    return "Max";
                case PlayerState.PAUSED:
                    return "Paused";
                case PlayerState.WALKING:
                    return "Walking";
            }
            return str;
        }
    }
}
