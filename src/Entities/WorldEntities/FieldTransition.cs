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
    internal class FieldTransition : InteractableWorldEntity
    {
        private Sprite _maskingSprite = null;

        public FieldTransition(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            int direction = op1;

            int unused = op4;

            switch (direction) {
                default:
                case 0:
                    // Left
                    HitboxWidth = 8;
                    HitboxHeight = 16;
                    break;
                case 1:
                    // Right
                    HitboxWidth = 8;
                    HitboxHeight = 16;
                    break;
                case 2:
                    // Down
                    HitboxWidth = 16;
                    HitboxHeight = 8;
                    break;
                case 3:
                    // Up
                    HitboxWidth = 16;
                    HitboxHeight = 8;
                    break;
            }

            // Specify the graphics that are overwritten in front of Lemeza when he passes through the gate
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            _maskingSprite = new Sprite(_tex, new Vector2(op2, op3), new Vector2(HitboxWidth, HitboxHeight));

            _sprIndex = _maskingSprite;
            Depth = (int)Global.DrawOrder.Foreground;

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = Global.WEStates.IDLE;
            }
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (Global.DevModeEnabled)
            {
                Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y + Main.HUD_HEIGHT, HitboxWidth, HitboxHeight);//(int)(0.5f * (HitboxWidth / World.CHIP_SIZE)), (int)(0.5f * (HitboxWidth / World.CHIP_SIZE)));
                HelperFunctions.DrawRectangle(spriteBatch, rect, new Color(0, 80, 200, 25));
            }

            switch (State)
            {
                case Global.WEStates.ACTIVATING:
                    _sprIndex.Draw(spriteBatch, Position + new Vector2(0, World.HUD_HEIGHT));
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
                    State = Global.WEStates.ACTIVATING;
                    break;
            }
        }
    }
}