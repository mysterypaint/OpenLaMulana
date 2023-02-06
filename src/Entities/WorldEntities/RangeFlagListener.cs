using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.System.GameFlags;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class RangeFlagListener : ParentWorldEntity
    {
        private int _flagToSet = -1;
        private int _currCheckingFlag = 0;
        private View _myView = null;

        enum RangeFlagListeningType
        {
            UNDEFINED = -1,
            CHECK_IF_ON_ALL,
            CHECK_IF_ON_SEQUENTIAL,
            CHECK_IF_ON_SIMULTANEOUS,
            MAX
        };

        private RangeFlagListeningType _monitoringType = RangeFlagListeningType.UNDEFINED;
        private int[] _flagsToCheck = null;

        /// <summary>
        /// An object that monitors multiple flags. Monitors all flags in the range specified in OP2 and OP3.
        /// If the flag is turned on according to the type specified by OP1, the flag specified by OP4 is turned on. The types of OP1 are as follows.
        /// 0: flags within the range are turned on
        /// 1: flags within the range are turned on sequentially
        /// 2: flags within the range are turned on simultaneously
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
        public RangeFlagListener(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _myView = destView;
            _sprIndex = null;

            int flagRange = op3 - op2 + 1;

            _flagsToCheck = new int[flagRange];

            int i = 0;
            for (int flag = 0; flag < flagRange; flag++)
            {
                _flagsToCheck[flag] = op2 + i;
                i++;
            }

            switch (op1)
            {
                default:
                case 0: // Flags within the range are turned on
                    _monitoringType = RangeFlagListeningType.CHECK_IF_ON_ALL;
                    break;
                case 1: // Flags within the range are turned on sequentially
                    _monitoringType = RangeFlagListeningType.CHECK_IF_ON_SEQUENTIAL;
                    break;
                case 2: // Flags within the range are turned on simultaneously
                    
                    _monitoringType = RangeFlagListeningType.CHECK_IF_ON_SIMULTANEOUS;
                    break;
            }

            _flagToSet = op4;

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = Global.WEStates.IDLE;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_sprIndex != null)
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        State = Global.WEStates.IDLE;
                    }
                    break;
                case Global.WEStates.IDLE:
                    switch (_monitoringType)
                    {
                        case RangeFlagListeningType.CHECK_IF_ON_ALL:
                            if (AllFlagsOn())
                                Global.GameFlags.InGameFlags[_flagToSet] = true;
                            break;
                        case RangeFlagListeningType.CHECK_IF_ON_SEQUENTIAL:
                            if (CheckFlagsSequentially())
                                Global.GameFlags.InGameFlags[_flagToSet] = true;
                            break;
                        case RangeFlagListeningType.CHECK_IF_ON_SIMULTANEOUS:
                            // TODO: Verify that this was implemented properly
                            if (AnyFlagOn())
                            {
                                if (AllFlagsOn()) // This checks them all sequentially anyway
                                    Global.GameFlags.InGameFlags[_flagToSet] = true;
                                else
                                    State = Global.WEStates.DYING;
                            }
                            break;
                    }
                    break;
                case Global.WEStates.DYING:
                    break;
            }
        }

        private bool CheckFlagsSequentially()
        {
            int lastFlagOn = -1;
            foreach (int flag in _flagsToCheck)
            {
                if (Global.GameFlags.InGameFlags[flag])
                    lastFlagOn = flag;
                else if (flag > lastFlagOn)
                    return false;
            }
            return true;
        }

        private bool AllFlagsOn()
        {
            foreach (int flag in _flagsToCheck)
            {
                if (!Global.GameFlags.InGameFlags[flag])
                    return false;
            }
            return true;
        }

        private bool AnyFlagOn()
        {
            foreach (int flag in _flagsToCheck)
            {
                if (Global.GameFlags.InGameFlags[flag])
                    return true;
            }
            return false;
        }
    }
}