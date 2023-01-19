using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class TreasureChest : InteractableWorldEntity
    {
        private Sprite _sprOpen, _sprClosed, _sprItem = null;
        private int _openConditionFlag = -1;
        ItemTypes _itemType = ItemTypes.UNKNOWN;
        private int _itemID = -1;
        private int _flagToTriggerWhenItemTaken = -1;
        public override int HitboxWidth { get; set; } = 16;
        public override int HitboxHeight { get; set; } = 16;

        public TreasureChest(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Textures.MAPG00);
            _sprOpen = new Sprite(_tex, 304, 16, 16, 16);
            _sprClosed = new Sprite(_tex, 304, 0, 16, 16);

            _openConditionFlag = op1;

            if (op2 >= 0)
            {
                Texture2D itemTex = Global.TextureManager.GetTexture(Textures.ITEM);
                if (op2 < 60)
                {
                    // Chest contains a treasure
                    _itemType = ItemTypes.TREASURE;
                    _itemID = op2;
                    _sprItem = new Sprite(itemTex, 0 + _itemID % 20 * 16, 192 + _itemID / 20 * 16, 16, 16);
                } else if (op2 >= 100 && op2 <= 183)
                {
                    // Chest contains a ROM
                    _itemType = ItemTypes.SOFTWARE;
                    _itemID = op2 - 100;
                    int softwareSpriteID = Global.World.SoftwareGetGraphicID(_itemID) + 1;
                    _sprItem = new Sprite(itemTex, 256 + softwareSpriteID % 4 * 16, 144 + softwareSpriteID / 4 * 16, 16, 16);
                }
            }

            _flagToTriggerWhenItemTaken = op3;      // If this flag is on, the chest is already empty: Do not open it

            if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_flagToTriggerWhenItemTaken])
            {
                _sprIndex = _sprClosed;
                State = WEStates.IDLE;
            }
            else {
                // The chest should be invisible, if there are actual start flags to consider. Otherwise, the chest should be already opened and visible
                if (StartFlags.Count <= 0 || Global.GameFlags.InGameFlags[_flagToTriggerWhenItemTaken])
                {
                    _sprIndex = _sprOpen;
                    State = WEStates.DYING;
                } else
                {
                    _sprIndex = null;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_sprIndex != null)
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
            /*
            switch (State)
            {
                case WEStates.DYING:        // The chest is already opened
                    _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                    break;
                case WEStates.IDLE:
                    _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                    break;
                case WEStates.ACTIVATING:
                    break;
            }*/
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        State = WEStates.ACTIVATING;
                        _sprIndex = _sprItem;
                    }
                    break;
                case WEStates.DYING:        // The chest is already opened
                    break;
                case WEStates.IDLE:
                    if (Global.GameFlags.InGameFlags[_openConditionFlag] || InputManager.PressedKeys[(int)Global.ControllerKeys.JUMP])
                    {
                        State = WEStates.ACTIVATING;
                        _sprIndex = _sprItem;
                        break;
                    }
                    break;
                case WEStates.ACTIVATING:
                    if (CollidesWithPlayer())
                    {
                        State = WEStates.DYING;
                        _sprIndex = _sprOpen;
                        Global.GameFlags.InGameFlags[_flagToTriggerWhenItemTaken] = true;

                        switch (_itemType)
                        {
                            default:
                                break;
                            case ItemTypes.TREASURE:
                                HelperFunctions.UpdateInventory(_itemType, _itemID, true);
                                break;
                            case ItemTypes.SOFTWARE:
                                HelperFunctions.UpdateInventory(_itemType, _itemID, true);
                                break;
                        }

                    }
                    break;
            }
        }
    }
}