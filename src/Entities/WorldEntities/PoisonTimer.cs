using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.Runtime.Intrinsics.X86;
using static OpenLaMulana.Entities.WorldEntities.NPCRoom;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class PoisonTimer : InteractableWorldEntity
    {
        private Protag _protag = Global.Protag;
        public override int HitboxWidth { get; set; } = 16;
        public override int HitboxHeight { get; set; } = 16;

        private int _checkingFlag = -1;
        private View _myView = null;
        private Field _myField = null;
        private bool _flagValueToSet = true;
        private Sprite[] _sprFont = null;
        private int _timerSeconds = -1;
        private float _timerFrames = -1;
        private int _timerSecondsReset = -1;
        private float _timerFramesReset = -1;
        private string _fmt = "00";
        private int _activationTimer = 0;
        private int _activationTimerReset = 120;

        enum PoisonFont : int
        {
            ZERO,
            ONE,
            TWO,
            THREE,
            FOUR,
            FIVE,
            SIX,
            SEVEN,
            EIGHT,
            NINE,
            COLON,
            MAX
        };

        public PoisonTimer(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            _sprFont = new Sprite[11];
            for (var i = 0; i < _sprFont.Length; i++)
            {
                _sprFont[i] = Global.TextureManager.Get8x8Tile(_tex, i, 18, Vector2.Zero);
            }
            _timerSecondsReset = op1;
            _timerFramesReset = op2;
            _timerSeconds = op1;
            _timerFrames = op2;
            _checkingFlag = op3;

            _myView = destView;
            _myField = destView.GetParentField();

            if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkingFlag])
            {
                State = WEStates.IDLE;
            }
            else
                State = WEStates.UNSPAWNED;


            Depth = (int)Global.DrawOrder.AboveTilesetGraphicDisplay;
            Position += new Vector2(0, 16);
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    break;
                case WEStates.IDLE:
                case WEStates.ACTIVATING:
                    Rectangle rect = new Rectangle(Position.ToPoint(), new Point(5 * World.CHIP_SIZE, World.CHIP_SIZE));
                    HelperFunctions.DrawRectangle(spriteBatch, rect, Color.Black);
                    Global.TextManager.DrawText(Position, String.Format("{0}:{1}", _timerSeconds.ToString(_fmt), Math.Round(_timerFrames).ToString(_fmt)));
                    break;
                case WEStates.DYING:
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkingFlag])
                    {
                        State = WEStates.IDLE;
                    }
                    break;
                case WEStates.IDLE:
                    if (Global.GameFlags.InGameFlags[_checkingFlag])
                    {
                        State = WEStates.DYING;
                    }

                    if ((Global.World.CurrViewX != _myView.X || Global.World.CurrViewY != _myView.Y) || Global.World.CurrField != _myField.ID)
                    {
                        if (Global.Camera.GetState() == System.Camera.CamStates.NONE)
                        {
                            _timerSeconds = _timerSecondsReset;
                            _timerFrames = _timerFramesReset;
                        }
                        break;
                    }

                    if (Global.Camera.GetState() != System.Camera.CamStates.NONE)
                    {
                        break;
                    }

                    var delta = (float)gameTime.ElapsedGameTime.TotalSeconds;

                    _timerFrames -= delta * 200f;

                    if (_timerFrames < 0)
                    {
                        _timerSeconds--;

                        if (_timerSeconds > 0)
                            _timerFrames = 99;
                        else
                        {
                            //Timer went off here
                            Global.AudioManager.StopMusic();
                            Global.AudioManager.PlaySFX(SFX.P_DEATH);

                            _activationTimer = _activationTimerReset;
                            _timerSeconds = 0;
                            _timerFrames = 0;
                            Global.Main.SetState(Global.GameState.CUTSCENE);
                            State = WEStates.ACTIVATING;
                            _protag.State = PlayerState.DYING;
                            break;
                        }
                    }
                    break;
                case WEStates.ACTIVATING:
                    if (Global.AnimationTimer.OneFrameElapsed())
                    {
                        if (_activationTimer > 0)
                            _activationTimer--;
                        else
                        {
                            View currView = Global.World.GetCurrentView();
                            View destView = Global.World.GetView(3, 0, 0);
                            Global.Main.SetState(GameState.PLAYING);
                            Global.World.FieldTransitionImmediate(currView, destView, true, true, true);
                            _protag.SetPositionToTile(new Point(14, 3));
                            _protag.ResetState();
                            State = WEStates.DYING;
                        }
                    }
                    break;
                case WEStates.DYING:
                    break;
            }
        }
    }
}