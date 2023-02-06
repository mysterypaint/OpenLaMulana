using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class Pitfall : ParentWorldEntity
    {
        private int _flagToSet = -1;
        private View _myView = null;

        /// <summary>
        /// A floor that opens when the protagonist steps on it.
        /// In OP1, specify the coordinates of the upper left corner of the image in units of dots with 3 digits of X and 3 digits of Y.
        /// The image size is fixed at 16 x 24 dots arranged vertically before and after opening.
        /// Forced operation when the flag specified by OP2 is turned on.
        /// OP4 is a pitfall type, and if you specify 0, it will open when the main character steps on it.
        /// 
        /// Other than that, it will not open unless it is forcibly released with the flag specified in OP2.
        /// Only when 0 is specified in OP4, the flag specified in OP3 is turned on when the floor opens.
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
        public Pitfall(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _myView = destView;
            _sprIndex = null;
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
                    break;
                case Global.WEStates.DYING:
                    break;
            }
        }
    }
}