using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class FloorSwitch : InteractableWorldEntity
    {
        private Protag _protag = Global.Protag;
        private bool _switchActivated = false;
        private int _flagToSet = -1;

        public FloorSwitch(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            _sprIndex = new Sprite(_tex, 288, 128, 16, 8);
            _flagToSet = op1;

            if (HelperFunctions.EntityMaySpawn(_startFlags) && !Global.GameFlags.InGameFlags[_flagToSet])
            {
                State = Global.WEStates.IDLE;
            }
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    break;
                case Global.WEStates.IDLE:
                    if (!_switchActivated)
                        _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(_startFlags))
                    {
                        State = Global.WEStates.IDLE;
                    }
                    break;
                case Global.WEStates.IDLE:
                    if (_flagToSet >= 0)
                    {
                        if (!_switchActivated)
                        {
                            if (BBox.Intersects(_protag.BBox))
                            {
                                if (_protag.IsGrounded())
                                {
                                    if (!_switchActivated)
                                    {
                                        Global.AudioManager.PlaySFX(SFX.WARP_TRIGGERED);
                                        _switchActivated = true;
                                        Global.GameFlags.InGameFlags[_flagToSet] = true;
                                    }
                                }
                            }
                        }
                    }
                    break;
            }
        }
    }
}