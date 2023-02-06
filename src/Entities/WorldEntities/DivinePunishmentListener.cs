using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class DivinePunishmentListener : ParentWorldEntity
    {
        private int _flagToSet = -1;
        private int[] _flagRange;
        private View _myView = null;
        private string _checkingMantra = string.Empty;

        /// <summary>
        /// This is an object that drops divine punishment if the flags specified in OP1 and OP2 are not turned on in order.
        /// If you turn them on in order, the Divine Punishment will not fall.
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
        public DivinePunishmentListener(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _flagRange = new int[] { op1, op2 };
            _sprIndex = null;

            _myView = destView;
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
                    for (var i = _flagRange[0]; i < _flagRange[1]; i++)
                    {

                    }
                    break;
                case Global.WEStates.DYING:
                    break;
            }
        }
    }
}