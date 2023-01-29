using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class SinkingRuinsTablet : ParentInteractableWorldEntity
    {
        private int _triggerFlag = -1;
        private int _dyingTimer = -1;
        private int _dyingTimerReset = 50;

        public SinkingRuinsTablet(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            // Fields 10 and above are programmatically treated as back fields. Enemy object bats automatically become back bats when placed behind them.
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrMapTexture());

            _sprIndex = new Sprite(_tex, op1, op2, 16, 16);
            _triggerFlag = op3;

            HP = 1;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    break;
                case Global.WEStates.IDLE:
                    _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                    break;
                case Global.WEStates.DYING:
                    // TODO: Make the sinking animation
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags) && (!Global.GameFlags.InGameFlags[_triggerFlag]))
                        State = Global.WEStates.IDLE;
                    break;
                case Global.WEStates.IDLE:
                    if (Global.GameFlags.InGameFlags[_triggerFlag]) {
                        State = Global.WEStates.DYING;
                        _dyingTimer = _dyingTimerReset;
                    }
                    break;
                case Global.WEStates.DYING:
                    if (Global.AnimationTimer.OneFrameElapsed())
                    {
                        if (_dyingTimer <= 0)
                        {

                            // TODO: Make the sinking animation


                            State = Global.WEStates.UNSPAWNED;
                        }
                        else
                            _dyingTimer--;
                    }
                    break;
            }
        }
    }
}