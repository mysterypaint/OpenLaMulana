using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class DivinePunishmentRegion : ParentInteractableWorldEntity
    {
        private int _flagToSet = -1;
        private int _damage = -1;
        private View _myView = null;

        /// <summary>
        /// If you attack the range specified in OP1 and 2, you will be punished.
        /// The divine punishment graphic is in item.bmp, but it is recommended to use it as it is because the vertical connections must also be calculated.
        /// For OP3, specify the damage received by Divine Punishment. If left at -1, the damage will be 16.
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
        public DivinePunishmentRegion(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            HitboxWidth = op1 * World.CHIP_SIZE;
            HitboxHeight = op2 * World.CHIP_SIZE;

            _damage = op3;

            if (_damage < 0)
                _damage = 16;

            _sprIndex = null;
            _myView = destView;
            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = Global.WEStates.IDLE;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                case Global.WEStates.DYING:
                    break;
                case Global.WEStates.IDLE:
                    if (Global.DevModeEnabled)
                    {
                        Rectangle rect = BBox;// new Rectangle((int)Position.X, (int)Position.Y + Main.HUD_HEIGHT, HitboxWidth, HitboxHeight);//(int)(0.5f * (HitboxWidth / World.CHIP_SIZE)), (int)(0.5f * (HitboxWidth / World.CHIP_SIZE)));
                        rect.Y += World.HUD_HEIGHT;
                        HelperFunctions.DrawRectangle(spriteBatch, rect, new Color(160, 25, 25, 115));
                    }
                    break;
            }
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
                    if (!HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        State = Global.WEStates.DYING;
                    }
                    break;
                case Global.WEStates.DYING:
                    break;
            }
        }
    }
}