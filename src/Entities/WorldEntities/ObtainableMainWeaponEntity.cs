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
    internal class ObtainableMainWeaponEntity : InteractableWorldEntity
    {
        private Protag _protag = Global.Protag;
        private int _flagToSet = -1;
        private int _itemID = -1;
        private Sprite _weaponSprite = null;
        public override int HitboxWidth { get; set; } = 16;
        public override int HitboxHeight { get; set; } = 16;

        public ObtainableMainWeaponEntity(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _itemID = op1;
            _flagToSet = op2;
            int texOffX = (_itemID % 4) * 16;
            int texOffY = (int)Math.Floor((double)_itemID / 4) * 16;
            _weaponSprite = new Sprite(_tex, 256 + texOffX, 0 + texOffY, 16, 16);
            _sprIndex = null;
            HP = 1;
            Depth = (int)Global.DrawOrder.ObtainableItems;

            if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_flagToSet])
            {
                if (_itemID >= 0)
                {
                    State = Global.WEStates.IDLE;
                    _sprIndex = _weaponSprite;
                }
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
            if (State == Global.WEStates.IDLE && _sprIndex != null)
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_flagToSet])
                    {
                        if (_itemID >= 0)
                        {
                            State = Global.WEStates.IDLE;
                            _sprIndex = _weaponSprite;
                        }
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
                        HelperFunctions.UpdateInventory(Global.ItemTypes.MAIN_WEAPON, _itemID, true, SFX.P_ITEM_TAKEN, _weaponSprite);
                        if (_flagToSet >= 0)
                        {
                            Global.GameFlags.InGameFlags[_flagToSet] = true;
                        }
                        _sprIndex = null;
                        State = Global.WEStates.DYING;
                    }
                    break;
                case Global.WEStates.DYING:
                    break;
            }
        }
    }
}