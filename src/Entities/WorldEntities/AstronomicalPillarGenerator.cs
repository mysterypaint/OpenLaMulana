using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Enemies.Guardians;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class AstronomicalPillarGenerator : ParentWorldEntity
    {
        private int _flagToSet = -1;
        private AstronomicalPillar[] _pillars = new AstronomicalPillar[3];
        private View _myView = null;
        private int _starFlag = -1;
        private int _moonFlag = -1;
        private int _sunFlag = -1;
        private bool _pillarsGeneratedAlready = false;
        private int _ts = World.CHIP_SIZE;

        /// <summary>
        /// It is a device that turns on the flag specified in OP1 to 3 by aligning the three patterns.
        /// The flag corresponding to the matching pattern is turned on.
        /// The flag will also be turned off if it collapses from the aligned state.
        /// The position of the pattern is fixed, so place it at coordinates 1,1 in the view.
        /// 
        /// 
        /// 
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        /// <param name="op3"></param>
        /// <param name="op4"></param>
        /// <param name="spawnIsGlobal"></param>
        /// <param name="destView"></param>
        /// <param name="startFlags"></param>
        public AstronomicalPillarGenerator(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _starFlag = op1;
            _moonFlag = op2;
            _sunFlag = op3;

            _sprIndex = null;

            _myView = destView;

            State = Global.WEStates.UNSPAWNED;
            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                GeneratePillars();
                State = Global.WEStates.IDLE;
            }
        }

        public enum PillarPositions
        {
            LEFT,
            CENTER,
            RIGHT,
            MAX
        };

        private void GeneratePillars()
        {
            if (_pillarsGeneratedAlready)
                return;

            Sprite[] pillarSprites = new Sprite[3];

            Texture2D tex = Global.TextureManager.GetTexture(Global.Textures.MAPG02);
            pillarSprites[0] = Global.TextureManager.Get16x16Tile(tex, 6, 10, new Vector2(8, 8));
            pillarSprites[1] = Global.TextureManager.Get16x16Tile(tex, 8, 10, new Vector2(0, 8));
            pillarSprites[2] = Global.TextureManager.Get16x16Tile(tex, 9, 10, new Vector2(0, 8));
            _pillars[0] = (AstronomicalPillar)InstanceCreate(new AstronomicalPillar((int)Position.X + 2 * _ts, (int)Position.Y + 9 * _ts, 0, 0, 0, 0, false, null, null, pillarSprites));
            _pillars[1] = (AstronomicalPillar)InstanceCreate(new AstronomicalPillar((int)Position.X + 14 * _ts, (int)Position.Y + 13 * _ts, 1, 0, 0, 0, false, null, null, pillarSprites));
            _pillars[2] = (AstronomicalPillar)InstanceCreate(new AstronomicalPillar((int)Position.X + 26 * _ts, (int)Position.Y + 9 * _ts, 2, 0, 0, 0, false, null, null, pillarSprites));
            _pillarsGeneratedAlready = true;

            if (Global.GameFlags.InGameFlags[_starFlag])
            {
                _pillars[0].SetPillarValue(0);
                _pillars[1].SetPillarValue(0);
                _pillars[2].SetPillarValue(0);
            }
            else if (Global.GameFlags.InGameFlags[_sunFlag])
            {
                _pillars[0].SetPillarValue(2);
                _pillars[1].SetPillarValue(2);
                _pillars[2].SetPillarValue(2);
            }
            else if (Global.GameFlags.InGameFlags[_moonFlag])
            {
                _pillars[0].SetPillarValue(1);
                _pillars[1].SetPillarValue(1);
                _pillars[2].SetPillarValue(1);
            }
        }

        ~AstronomicalPillarGenerator()
        {
            DestroyMyPillars();
        }

        private void DestroyMyPillars()
        {
            if (!_pillarsGeneratedAlready)
                return;
            foreach (AstronomicalPillar pillar in _pillars)
            {
                InstanceDestroy(pillar);
            }

            _pillars[0] = null;
            _pillars[1] = null;
            _pillars[2] = null;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            //_sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {

            _pillars[0].Position = new Vector2(Position.X + 2 * _ts, (int)Position.Y + 9 * _ts);
            _pillars[1].Position = new Vector2(Position.X + 14 * _ts, (int)Position.Y + 13 * _ts);
            _pillars[2].Position = new Vector2(Position.X + 26 * _ts, (int)Position.Y + 9 * _ts);

            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        GeneratePillars();
                        State = Global.WEStates.IDLE;
                    }
                    break;
                case Global.WEStates.IDLE:
                    if (Global.DevModeEnabled)
                    {
                        if (InputManager.DirectKeyboardCheckPressed30FPS(Microsoft.Xna.Framework.Input.Keys.D1))
                        {
                            IncrementPillarPositions(PillarPositions.LEFT);
                        }
                        else
                        if (InputManager.DirectKeyboardCheckPressed30FPS(Microsoft.Xna.Framework.Input.Keys.D2))
                        {
                            IncrementPillarPositions(PillarPositions.CENTER);
                        }
                        else if (InputManager.DirectKeyboardCheckPressed30FPS(Microsoft.Xna.Framework.Input.Keys.D3))
                        {
                            IncrementPillarPositions(PillarPositions.RIGHT);
                        }
                    }
                    break;
                case Global.WEStates.DYING:
                    break;
                case Global.WEStates.DELETED:
                    DestroyMyPillars();
                    break;
            }
        }

        private void IncrementPillarPositions(PillarPositions pillarPosition)
        {
            switch (pillarPosition)
            {
                case PillarPositions.LEFT:
                    _pillars[0].IncrementCounter(PillarPositions.CENTER);
                    _pillars[1].IncrementCounter(PillarPositions.RIGHT);
                    _pillars[2].IncrementCounter(PillarPositions.LEFT);
                    break;
                case PillarPositions.CENTER:
                    _pillars[0].IncrementCounter(PillarPositions.LEFT);
                    _pillars[1].IncrementCounter(PillarPositions.CENTER);
                    _pillars[2].IncrementCounter(PillarPositions.RIGHT);
                    break;
                case PillarPositions.RIGHT:
                    _pillars[0].IncrementCounter(PillarPositions.RIGHT);
                    _pillars[1].IncrementCounter(PillarPositions.LEFT);
                    _pillars[2].IncrementCounter(PillarPositions.CENTER);
                    break;
            }

            if (_pillars[0].IsSun() && _pillars[1].IsSun() && _pillars[2].IsSun())
            {
                Global.GameFlags.InGameFlags[_sunFlag] = true;
                Global.GameFlags.InGameFlags[_moonFlag] = false;
                Global.GameFlags.InGameFlags[_starFlag] = false;
            }
            else if (_pillars[0].IsMoon() && _pillars[1].IsMoon() && _pillars[2].IsMoon())
            {
                Global.GameFlags.InGameFlags[_sunFlag] = false;
                Global.GameFlags.InGameFlags[_moonFlag] = true;
                Global.GameFlags.InGameFlags[_starFlag] = false;
            }
            else if (_pillars[0].IsStar() && _pillars[1].IsStar() && _pillars[2].IsStar())
            {
                Global.GameFlags.InGameFlags[_sunFlag] = false;
                Global.GameFlags.InGameFlags[_moonFlag] = false;
                Global.GameFlags.InGameFlags[_starFlag] = true;
            } else
            {
                Global.GameFlags.InGameFlags[_sunFlag] = false;
                Global.GameFlags.InGameFlags[_moonFlag] = false;
                Global.GameFlags.InGameFlags[_starFlag] = false;
            }
        }
    }
}