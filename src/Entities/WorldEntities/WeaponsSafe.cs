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
    internal class WeaponsSafe : InteractableWorldEntity
    {
        private int _checkFlag = -1;
        private Sprite _displayedGraphic;

        public WeaponsSafe(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _checkFlag = op1;

            Depth = (int)Global.DrawOrder.Entities;

            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrMapTexture());
            _displayedGraphic = new Sprite(_tex, 128, 112, 6 * World.CHIP_SIZE, 3 * World.CHIP_SIZE);
            HitboxWidth = 6 * World.CHIP_SIZE;
            HitboxHeight = 3 * World.CHIP_SIZE;

            if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkFlag])
            {
                State = Global.WEStates.IDLE;
                _sprIndex = _displayedGraphic;
                CollisionBehavior = World.ChipTypes.SOLID;
            }
            else
            {
                State = Global.WEStates.UNSPAWNED;
                _sprIndex = null;
                CollisionBehavior = World.ChipTypes.VOID;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (State == Global.WEStates.IDLE && _sprIndex != null)
                _sprIndex.Draw(spriteBatch, Position + new Vector2(0, World.HUD_HEIGHT));
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkFlag])
                    {
                        State = Global.WEStates.IDLE;
                        _sprIndex = _displayedGraphic;
                    }
                    break;
                case Global.WEStates.IDLE:
                    if (Global.GameFlags.InGameFlags[_checkFlag])
                    {
                        State = Global.WEStates.DYING;
                        _sprIndex = null;
                        CollisionBehavior = World.ChipTypes.VOID;
                    }
                    break;
                case Global.WEStates.DYING:
                    break;
            }
        }
    }
}