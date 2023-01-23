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
    internal class ContactDamage : InteractableWorldEntity
    {
        private int _damageValue = 0;
        private int _contactSFX = -1;

        public ContactDamage(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            HitboxWidth = op1 * World.CHIP_SIZE;
            HitboxHeight = op2 * World.CHIP_SIZE;
            _damageValue = op3;
            _contactSFX = op4;

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = Global.WEStates.IDLE;
            }
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (Global.DevModeEnabled)
            {
                Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y + World.HUD_HEIGHT, HitboxWidth, HitboxHeight);
                HelperFunctions.DrawRectangle(spriteBatch, rect, new Color(200, 20, 200, 20));
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
                        Global.Protag.Hurt(_damageValue);
                        if (_contactSFX >= 0)
                        {
                            Global.AudioManager.PlaySFX((SFX)_contactSFX);
                        }
                    }
                    break;
            }
        }
    }
}