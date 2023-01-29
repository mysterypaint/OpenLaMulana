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
    internal class ObtainableSoftwareEntity : ParentInteractableWorldEntity
    {
        private Protag _protag = Global.Protag;
        private int _flagToSet = -1;
        private int _itemID = -1;
        private Sprite _softwareSprite = null;

        public override int HitboxWidth { get; set; } = 8;
        public override int HitboxHeight { get; set; } = 8;

        public ObtainableSoftwareEntity(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _itemID = op1;
            _flagToSet = op2;
            int softwareSpriteID = Global.World.SoftwareGetGraphicID(_itemID) + 1;
            _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _softwareSprite = new Sprite(_tex, 256 + softwareSpriteID % 4 * 16, 144 + softwareSpriteID / 4 * 16, 16, 16);
            _sprIndex = null;
            Depth = (int)Global.DrawOrder.ObtainableItems;

            if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_flagToSet])
            {
                if (_itemID >= 0)
                    State = Global.WEStates.IDLE;
                else
                {
                    _sprIndex = null;
                    State = Global.WEStates.DYING;
                }
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
                    if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_flagToSet])
                    {
                        if (_itemID >= 0)
                            State = Global.WEStates.IDLE;
                        else
                        {
                            _sprIndex = null;
                            State = Global.WEStates.DYING;
                        }
                    }
                    break;
                case Global.WEStates.IDLE:
                    if (CollidesWithPlayer())
                    {
                        if (Global.Camera.GetState() == Camera.CamStates.NONE)
                        {
                            if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.SUB_WEAPON) && Global.Inventory.EquippedSubWeapon == Global.SubWeapons.HANDY_SCANNER)
                            {
                                HelperFunctions.UpdateInventory(Global.ItemTypes.SOFTWARE, _itemID, true, SFX.SOFTWARE_TAKEN, _softwareSprite);
                                if (_flagToSet >= 0)
                                {
                                    Global.GameFlags.InGameFlags[_flagToSet] = true;
                                }
                                State = Global.WEStates.DYING;
                            }
                        }
                    }
                    break;
                case Global.WEStates.DYING:
                    break;
            }
        }
    }
}