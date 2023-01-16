using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class ContactWarp : InteractableWorldEntity
    {
        private int _fieldID = -1;
        private int _viewNumber = -1;    // Specifies the graphics that are overwritten in front of Lemeza when he passes through the gate
        private int _relViewX = -1;
        private int _relViewY = -1;
        private bool _playerMayWarp = true;

        public ContactWarp(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView)
        {
            _fieldID = op1;
            _viewNumber = op2;    // Specifies the graphics that are overwritten in front of Lemeza when he passes through the gate
            _relViewX = op3;
            _relViewY = op4;

            _tex = null;
            _sprIndex = null;
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        public override void Update(GameTime gameTime)
        {
            if (CollidesWithPlayer())
            {
                if (Global.Protag.ContactWarpCooldownTimer <= 0 && _playerMayWarp) {
                    Field currField = Global.World.GetCurrField();
                    View currView = Global.World.GetCurrentView();
                
                    Field destField = Global.World.GetField(_fieldID);
                    View destView = destField.GetView(_viewNumber);

                    bool updateEntities = currField.ID != destField.ID;

                    Global.Protag.ContactWarpCooldownTimer = 10;
                    Global.World.FieldTransitionImmediate(currView, destView, updateEntities);
                    Global.Protag.SetPositionToTile(_relViewX, _relViewY);
                    Global.AudioManager.PlaySFX(SFX.WARP_TRIGGERED);
                } else
                {
                    _playerMayWarp = false;
                }
            } else
            {
                _playerMayWarp = true;
            }
        }
    }
}