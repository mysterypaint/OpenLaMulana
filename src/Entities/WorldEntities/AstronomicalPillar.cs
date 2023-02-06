using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class AstronomicalPillar : ParentInteractableWorldEntity
    {
        private Sprite[] _sprites;
        private int _pillarIndex = 0;
        private int _pillarIncrementValue = 0;

        enum PillarSprites : int
        {
            STAR,
            MOON,
            SUN,
            MAX
        };

        public AstronomicalPillar(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags, Sprite[] sprites) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _sprites = sprites;
            _pillarIndex = op1;
            Depth = (int)Global.DrawOrder.Entities;
            LockToCamera = true;

            switch (_pillarIndex)
            {
                default:
                case 0:
                    _pillarIncrementValue = 0;
                    break;
                case 1:
                    _pillarIncrementValue = 1;
                    break;
                case 2:
                    _pillarIncrementValue = 2;
                    break;
            }

            UpdatePillarSprite();

            HitboxWidth = 16;
            HitboxHeight = 16;

            Position = new Vector2(x, y);
            _world = Global.World;
        }

        private void UpdatePillarSprite()
        {
            switch (_pillarIncrementValue)
            {
                default:
                case 0:
                    _sprIndex = _sprites[(int)PillarSprites.STAR];
                    break;
                case 1:
                    _sprIndex = _sprites[(int)PillarSprites.MOON];
                    break;
                case 2:
                    _sprIndex = _sprites[(int)PillarSprites.SUN];
                    break;
                case 3:
                    _sprIndex = _sprites[(int)PillarSprites.STAR];
                    break;
                case 4:
                    _sprIndex = _sprites[(int)PillarSprites.SUN];
                    break;
                case 5:
                    _sprIndex = _sprites[(int)PillarSprites.MOON];
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _sprIndex.Draw(spriteBatch, Position + new Vector2(0, World.HUD_HEIGHT));
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

        internal void IncrementCounter(AstronomicalPillarGenerator.PillarPositions myPosition)
        {
            Global.AudioManager.PlaySFX(SFX.P_WEAK_WALL_2);

            switch (myPosition)
            {
                case AstronomicalPillarGenerator.PillarPositions.CENTER:
                    _pillarIncrementValue++;
                    break;
                case AstronomicalPillarGenerator.PillarPositions.LEFT:
                    _pillarIncrementValue += 3;
                    break;
                case AstronomicalPillarGenerator.PillarPositions.RIGHT:
                    _pillarIncrementValue += 2;
                    break;
            }

            _pillarIncrementValue %= 6;

            UpdatePillarSprite();
        }

        internal void SetPillarValue(int value)
        {
            _pillarIncrementValue = value % 6;
            UpdatePillarSprite();
        }

        internal bool IsSun()
        {
            return _pillarIncrementValue == 2 || _pillarIncrementValue == 4;
        }

        internal bool IsMoon()
        {
            return _pillarIncrementValue == 1 || _pillarIncrementValue == 5;
        }

        internal bool IsStar()
        {
            return _pillarIncrementValue == 0 || _pillarIncrementValue == 3;
        }
    }
}