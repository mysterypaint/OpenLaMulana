using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.Linq;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class DestinationViewChanger : ParentInteractableWorldEntity
    {
        private Protag _protag = Global.Protag;
        private View _destView = null;
        private View _currView = null;

        public override int HitboxWidth { get; set; } = 16;
        public override int HitboxHeight { get; set; } = 16;

        private int _directionToOverwrite = -1;
        private bool _overwroteWarpData = false;

        public DestinationViewChanger(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            HitboxWidth = op1 * World.CHIP_SIZE;
            HitboxHeight = op2 * World.CHIP_SIZE;
            _directionToOverwrite = op3;

            int[] separatedDigits = HelperFunctions.SeparateDigits(op4, new int[] { 2, 2 });
            int destFieldNum = separatedDigits[0];// digits[0] * 10 + digits[1];
            int destRelViewID = separatedDigits[1];// digits[2] * 10 + digits[3];

            _destView = Global.World.GetField(destFieldNum).GetView(destRelViewID);
            _currView = Global.World.GetCurrentView();

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = Global.WEStates.IDLE;
            }
            else
                State = Global.WEStates.UNSPAWNED;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
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
                    if (BBox.Intersects(_protag.BBox))
                    {
                        if (!_overwroteWarpData)
                        {
                            _currView.OverwriteViewDestinationTemporarily((World.VIEW_DIR)_directionToOverwrite, 0, _destView.GetParentField().ID, _destView.X, _destView.Y);

                            _overwroteWarpData = true;
                        }
                    } else
                    {
                        if (_overwroteWarpData)
                        {
                            _currView.RestoreOriginalViewDestination((World.VIEW_DIR)_directionToOverwrite, 0, _destView.GetParentField().ID, _destView.X, _destView.Y);

                            _overwroteWarpData = false;
                        }
                    }
                    break;
                case Global.WEStates.DYING:
                    break;
            }
        }
    }
}