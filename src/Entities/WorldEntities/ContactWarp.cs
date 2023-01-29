using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class ContactWarp : ParentInteractableWorldEntity
    {
        private int _fieldID = -1;
        private int _viewNumber = -1;    // Specifies the graphics that are overwritten in front of Lemeza when he passes through the gate
        private int _relViewX = -1;
        private int _relViewY = -1;
        private bool _playerMayWarp = true;

        public ContactWarp(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _fieldID = op1;
            _viewNumber = op2;    // Specifies the graphics that are overwritten in front of Lemeza when he passes through the gate
            _relViewX = op3;
            _relViewY = op4;
            HitboxWidth = 2 * World.CHIP_SIZE;
            HitboxHeight = 2 * World.CHIP_SIZE;

            _tex = null;
            _sprIndex = null;

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = Global.WEStates.IDLE;
            }
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    break;
                case WEStates.IDLE:
                    if (Global.DevModeEnabled)
                    {
                        Rectangle rect = BBox;// new Rectangle((int)Position.X, (int)Position.Y + Main.HUD_HEIGHT, HitboxWidth, HitboxHeight);//(int)(0.5f * (HitboxWidth / World.CHIP_SIZE)), (int)(0.5f * (HitboxWidth / World.CHIP_SIZE)));
                        rect.Y += World.HUD_HEIGHT;
                        HelperFunctions.DrawRectangle(spriteBatch, rect, new Color(75, 100, 100, 105));
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
                    if (CollidesWithPlayer())
                    {
                        if (Global.Protag.ContactWarpCooldownTimer <= 0 && _playerMayWarp)
                        {
                            Field currField = Global.World.GetCurrField();
                            View currView = Global.World.GetCurrentView();

                            Field destField = Global.World.GetField(_fieldID);
                            View destView = destField.GetView(_viewNumber);

                            bool updateEntities = currField.ID != destField.ID || currView.X != destView.X || currView.Y != destView.Y;

                            Global.Protag.ContactWarpCooldownTimer = 10;

                            bool forceRespawnGlobals = false;
                            bool updateMusic = false;
                            if (updateEntities)
                            {
                                currField.QueueDeleteAllFieldAndRoomEntities();
                                currField.UnlockAllViewSpawning();
                                currField.ForgetVisitedViews();

                                destField.QueueDeleteAllFieldAndRoomEntities();
                                destField.UnlockAllViewSpawning();
                                destField.ForgetVisitedViews();

                                forceRespawnGlobals = true;

                                if (currField.ID != destField.ID)
                                    updateMusic = true;
                            }
                            Global.World.FieldTransitionImmediate(currView, destView, updateEntities, updateMusic, forceRespawnGlobals);

                            Global.Protag.SetPositionToTile(new Point(_relViewX, _relViewY));
                            Global.AudioManager.PlaySFX(SFX.WARP_TRIGGERED);
                        }
                        else
                        {
                            _playerMayWarp = false;
                        }
                    }
                    else
                    {
                        _playerMayWarp = true;
                    }
                    break;
            }
        }
    }
}