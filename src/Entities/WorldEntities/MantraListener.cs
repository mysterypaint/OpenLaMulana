using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class MantraListener : ParentWorldEntity
    {
        private int _flagToSet = -1;
        private View _myView = null;
        private string _checkingMantra = string.Empty;

        /// <summary>
        /// An object that monitors spell input from the keyboard. If you enter the spell according to the talk specified in OP1, the flag specified in OP2 will be turned on. Since the acquisition of the Magatama (flag number 773) is monitored, it will not work unless you have the magic ball.
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
        public MantraListener(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _checkingMantra = Global.TextManager.GetText(op1, Global.CurrLang);
            _flagToSet = op2;
            _myView = destView;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (Global.DevModeEnabled)
                    {
                        State = Global.WEStates.IDLE;
                    }

                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        State = Global.WEStates.IDLE;
                    }
                    break;
                case Global.WEStates.IDLE:
                    if (Global.GameFlags.InGameFlags[(int)GameFlags.Flags.MAGATAMA_TAKEN])
                    {
                        if (HelperFunctions.PlayerInTheSameView(_myView))
                        {
                            if (Global.Input.CheckChantedMantra(_checkingMantra))
                            {
                                if (_flagToSet > -1)
                                {
                                    if (!Global.GameFlags.InGameFlags[_flagToSet])
                                    {
                                        Global.GameFlags.InGameFlags[_flagToSet] = true;
                                        State = Global.WEStates.DYING;
                                    }
                                }
                            }
                        }
                    }
                    break;
                case Global.WEStates.DYING:
                    break;
            }
        }
    }
}