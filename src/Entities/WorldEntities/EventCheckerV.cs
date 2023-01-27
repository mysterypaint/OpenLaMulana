using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Global;
using static OpenLaMulana.System.Camera;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class EventCheckerV : IRoomWorldEntity
    {
        private int _flagToSetWhenConditionsMet = -1;
        private SFX? _playingSFX = null;
        private int[] _checkingFlags;
        private View _myView = null;

        /// <summary>
        /// Monitors the four check flags, turns on the set flag when all are turned on, and plays a sound. Unlike event checker F, it monitors flags only within the view. It is also useful for simply playing sounds.
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
        public EventCheckerV(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());

            _flagToSetWhenConditionsMet = x;
            if (y >= 0)
                _playingSFX = (SFX)(y + 1);

            _checkingFlags = new int[] { op1, op2, op3, op4 };

            _sprIndex = null;
            _myView = destView;

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = WEStates.IDLE;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    break;
                case WEStates.IDLE:
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        State = WEStates.IDLE;
                    }
                    break;
                case WEStates.IDLE:
                    if (Global.World.GetCurrentView() != _myView)
                        return;
                    if (HelperFunctions.CheckFlags(_checkingFlags))
                    {
                        State = WEStates.ACTIVATING;
                        if (_flagToSetWhenConditionsMet >= 0)
                            Global.GameFlags.InGameFlags[_flagToSetWhenConditionsMet] = true;
                        if (_playingSFX != null)
                            Global.AudioManager.PlaySFX((SFX)_playingSFX);
                    }
                    break;
                case WEStates.ACTIVATING:
                    break;
            }
        }
    }
}