using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class NPCRoom : InteractableWorldEntity // Enemy, so that it has a hitbox
    {
        enum NPCTypes
        {
            Store,
            Storyteller,
            Xelpud
        };

        public enum NPCStates
        {
            INACTIVE,
            ACTIVE,
            MAX
        };

        enum ShopDialogue
        {
            WELCOME,
            PRODUCT_1_HOVER,
            PRODUCT_2_HOVER,
            PRODUCT_3_HOVER,
            PRODUCT_SOLDOUT,
            PLAYER_TOO_POOR,
            PRODUCT_PURCHASED,
            PRODUCT_CANCELLED,
            MAX
        };

        enum ShopParams
        {
            PRODUCT_TYPE,
            PRODUCT_ID,
            PRICE_HI,
            PRICE_LO,
            PRODUCT_QUANTITY,
            FLAGS_HI,
            FLAGS_LO,
            MAX
        };

        enum DialogueBoxSprites
        {
            TL,
            T,
            TR,
            L,
            R,
            BL,
            B,
            BR,
            TM,
            MAX
        };

        struct ShopData
        {
            public ItemTypes ProductType;
            public int ProductID;
            public int ProductPrice;
            public int ProductQuantity;
            public ushort FlagToSet;
            public string ProductPriceString;
            public bool IsSoldOut;
        }

        private Sprite _NPCGraphic = null;
        private Sprite _houseGraphicTop = null;
        private Sprite _houseGraphicBottom = null;
        private Sprite _houseGraphicBottomPart = null;
        private Sprite _shopCursor = null;
        private Sprite[] _dialogueBoxSprites = null;
        private Sprite[] _productSprites = null;
        private Sprite _doorwaySprite = null;
        private SpriteAnimation _playerShopSprite = SpriteAnimation.CreateSimpleAnimation(Global.TextureManager.GetTexture(Global.Textures.PROT1), new Point(4 * 16, 0 * 16), 16, 16, new Point(16, 0), 2, 0.02f);
        private ShopData[] _shopData = null;
        private NPCTypes _spawnType = NPCTypes.Store;
        private int _shopCursorValue = 0;
        private string[] _dialogueStr;
        private string _currText = "";
        private Protag _protag = Global.Protag;
        private int _myBGM = -1;

        public NPCStates State { get; set; } = NPCStates.INACTIVE;


        /* TEXT CONTROL CODES
            \\1: Wait for key input. Use the weapon key to advance to the next page. It cannot be used in the corpse memo.
            \\2: Flag on. Specify the flag number in the following 2 bytes.    \2\byteOne\byteTwo     CALCULATION: (byteOne - 1) * 256 + byteTwo
            \\3: Flag off. Specify the flag number in the following 2 bytes.   \3\byteOne\byteTwo     CALCULATION: (byteOne - 1) * 256 + byteTwo
            \\4: Item acquisition. Get the item in the next 1 byte. Item number from 1.
            \\5: ROM acquisition. 1 byte after this will get the ROM. ROM number starts from 1.
            \\6: Sound playback. Sounds the SE with the number specified by the following 1 byte. SE number from 1.
            \\7: Prevents you from leaving the conversation.
            \\8: Cancel the \\7 state.
            \\10: Line feed. Do not use line breaks in the talk editor.
        */

        public NPCRoom(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            Depth = (int)Global.DrawOrder.Overlay;

            _tex = null;
            _sprIndex = null;

            Texture2D eventTex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            _doorwaySprite = new Sprite(eventTex, 256, 160, 8, 16);

            _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            Sprite tl = Global.TextureManager.Get8x8Tile(_tex, 13, 1, Vector2.Zero);
            Sprite t = Global.TextureManager.Get8x8Tile(_tex, 14, 1, Vector2.Zero);
            Sprite tr = Global.TextureManager.Get8x8Tile(_tex, 15, 1, Vector2.Zero);
            Sprite l = Global.TextureManager.Get8x8Tile(_tex, 16, 1, Vector2.Zero);
            Sprite r = Global.TextureManager.Get8x8Tile(_tex, 17, 1, Vector2.Zero);
            Sprite bl = Global.TextureManager.Get8x8Tile(_tex, 18, 1, Vector2.Zero);
            Sprite b = Global.TextureManager.Get8x8Tile(_tex, 19, 1, Vector2.Zero);
            Sprite br = Global.TextureManager.Get8x8Tile(_tex, 20, 1, Vector2.Zero);
            Sprite tm = Global.TextureManager.Get16x16Tile(_tex, 7, 1, new Vector2(8, 0));
            _dialogueBoxSprites = new Sprite[] { tl, t, tr, l, r, bl, b, br, tm };

            int[] op1_digits = HelperFunctions.GetDigits(op1).ToArray();

            int shopType, clerkAndRoomGraphic;

            // Example,   op1 = 106      ->    Shop type "1", graphic "06"
            if (op1_digits.Length > 1)
            {
                shopType = op1_digits[0];

                if (op1_digits.Length >= 3)
                    clerkAndRoomGraphic = op1_digits[1] * 10 + op1_digits[2];
                else
                {
                    shopType = 0;
                    clerkAndRoomGraphic = op1;
                }
            }
            else
            {
                shopType = 0;
                clerkAndRoomGraphic = op1;
            }

            _NPCGraphic = new Sprite(_tex, 256 + clerkAndRoomGraphic % 4 * 16, 32 + clerkAndRoomGraphic / 4 * 16, 16, 16);
            _houseGraphicTop = new Sprite(_tex, 64 + clerkAndRoomGraphic % 8 * 24, 32 + clerkAndRoomGraphic / 8 * 16, 24, 8);
            _houseGraphicBottom = new Sprite(_tex, 64 + clerkAndRoomGraphic % 8 * 24, 40 + clerkAndRoomGraphic / 8 * 16, 24, 8);
            _houseGraphicBottomPart = new Sprite(_tex, 64 + clerkAndRoomGraphic % 8 * 24 + 16, 40 + clerkAndRoomGraphic / 8 * 16, 24 - 16, 8);
            //_sprIndex = _shopGraphicTop;

            if (op2 >= 0)
                _myBGM = op2;

            switch (shopType)
            {
                default:
                case (int)NPCTypes.Store:
                    _shopCursor = Global.TextureManager.Get16x16Tile(_tex, 8, 1, new Vector2(8, 0));
                    _productSprites = new Sprite[3];
                    _spawnType = NPCTypes.Store;
                    int shopDialogueID = op3;
                    int shopInfo = op4;

                    // Populate the shop's dialogue box data
                    _dialogueStr = new string[(int)ShopDialogue.MAX + 1];
                    _dialogueStr[0] = Global.TextManager.GetText(shopDialogueID, Global.CurrLang);
                    for (int i = 1; i < _dialogueStr.Length; i++)
                    {
                        _dialogueStr[i] = Global.TextManager.GetText(shopDialogueID + 1 + i, Global.CurrLang);
                    }

                    // Parse and populate the shop's product data
                    //  - Product data format -
                    //  \ Product ID \ Product number \ Price \ Price \ Quantity \ Set flag \ Set flag
                    string[] shopDataStr = Global.TextManager.GetText(shopInfo, Global.CurrLang).Split("\\");
                    byte[] _shopDataValues = new byte[shopDataStr.Length - 1];
                    for (int i = 1; i < shopDataStr.Length; i++)
                    {
                        _shopDataValues[i - 1] = (byte)int.Parse(shopDataStr[i]);
                    }

                    _shopData = new ShopData[3];
                    string fmt = "000";

                    for (int i = 0; i < 3; i++)
                    {
                        int offset = i * (int)ShopParams.MAX;

                        _shopData[i].ProductType = (ItemTypes)_shopDataValues[offset + (int)ShopParams.PRODUCT_TYPE];
                        _shopData[i].ProductPrice = _shopDataValues[offset + (int)ShopParams.PRICE_LO] + (_shopDataValues[offset + (int)ShopParams.PRICE_HI] - 1) * 256;
                        if (_shopData[i].ProductPrice > 999)
                            _shopData[i].ProductPrice = 999;
                        _shopData[i].ProductPriceString = _shopData[i].ProductPrice.ToString(fmt);
                        _shopData[i].IsSoldOut = false;

                        switch (_shopData[i].ProductType)
                        {
                            default:
                            case ItemTypes.SUBWEAPON:
                                _shopData[i].ProductID = _shopDataValues[offset + (int)ShopParams.PRODUCT_ID];
                                _shopData[i].ProductQuantity = _shopDataValues[offset + (int)ShopParams.PRODUCT_QUANTITY] - 1; // Sell the actual subweapon if this value is 0. Anything higher is the quantity to grant.
                                if (_shopData[i].ProductID == (int)Global.SubWeapons.ANKH_JEWEL || _shopData[i].ProductID == (int)Global.SubWeapons.PISTOL_AMMUNITION)
                                {
                                    if (_shopData[i].ProductQuantity <= 0)
                                        throw new InvalidDataException($"Ankh Jewels and Pistol Bullets may not be <= 0, because the slot would be invisible in-game");
                                }

                                _productSprites[i] = new Sprite(_tex, 256 + (_shopData[i].ProductID - 1) % 4 * 16, 96 + (_shopData[i].ProductID - 1) / 4 * 16, 16, 16);
                                break;
                            case ItemTypes.TREASURE:
                                _shopData[i].ProductID = _shopDataValues[offset + (int)ShopParams.PRODUCT_ID] - 1;
                                _shopData[i].ProductQuantity = 0;
                                _productSprites[i] = new Sprite(_tex, 0 + _shopData[i].ProductID % 20 * 16, 192 + _shopData[i].ProductID / 20 * 16, 16, 16);
                                break;
                            case ItemTypes.SOFTWARE:
                                _shopData[i].ProductID = _shopDataValues[offset + (int)ShopParams.PRODUCT_ID] - 1;
                                _shopData[i].ProductQuantity = 0;
                                //_shopData[i].ProductID
                                int softwareSpriteID = Global.World.SoftwareGetGraphicID(_shopData[i].ProductID) + 1;
                                _productSprites[i] = new Sprite(_tex, 256 + softwareSpriteID % 4 * 16, 144 + softwareSpriteID / 4 * 16, 16, 16);
                                break;
                        }

                        /*
                        if (_shopDataValues[offset + (int)ShopParams.FLAGS_HI] % 255 == 0 || _shopDataValues[offset + (int)ShopParams.FLAGS_LO] % 255 == 0)
                        {
                            _shopData[i].FlagToSet = null;
                        }*/

                        _shopData[i].FlagToSet = (ushort)(((_shopDataValues[offset + (int)ShopParams.FLAGS_HI] - 1) * 256) + _shopDataValues[offset + (int)ShopParams.FLAGS_LO]);
                    }

                    _currText = _dialogueStr[(int)ShopDialogue.WELCOME];
                    break;
                case (int)NPCTypes.Storyteller:
                    _spawnType = NPCTypes.Storyteller;
                    int storytellerDialogueID = op3;
                    _dialogueStr = new string[] { Global.TextManager.GetText(storytellerDialogueID, Global.CurrLang) };

                    _currText = _dialogueStr[0];
                    break;
                case (int)NPCTypes.Xelpud:
                    _spawnType = NPCTypes.Xelpud;
                    //Message Index 84-93: Save/load relation
                    /*
                    (int)HardCodedText.SAVE_LOAD_DIALOGUE_BEGINS
                    (int)HardCodedText.SAVE_LOAD_DIALOGUE_ENDS*/
                    break;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_sprIndex != null)
                return;

            switch (_spawnType)
            {
                case NPCTypes.Store:
                    switch (State)
                    {
                        case NPCStates.INACTIVE:
                            break;
                        case NPCStates.ACTIVE:
                            DrawNPCRoom(spriteBatch, gameTime, 17);
                            _shopCursor.Draw(spriteBatch, new Vector2((6 + _shopCursorValue * 7) * 8, 13 * 8));
                            for (var i = 0; i < 3; i++)
                            {
                                _productSprites[i].Draw(spriteBatch, new Vector2((8 + 7 * i) * 8, 13 * 8));

                                if (!_shopData[i].IsSoldOut)
                                    Global.TextManager.DrawText((7 + (i * 7)) * 8, 15 * 8, _shopData[i].ProductPriceString);
                                else
                                    Global.TextManager.DrawText((7 + (i * 7)) * 8, 15 * 8, "\\16\\17\\18"); // Draw "SoldOut"
                            }
                            _playerShopSprite.Draw(spriteBatch, new Vector2(15 * 8, 21 * 8));
                            Global.TextManager.DrawText(5 * 8, 9 * 8, _currText, 22, Color.White, 0, World.CHIP_SIZE, false);
                            break;
                    }
                    break;
                case NPCTypes.Storyteller:
                    switch (State)
                    {
                        case NPCStates.INACTIVE:
                            break;
                        case NPCStates.ACTIVE:
                            DrawNPCRoom(spriteBatch, gameTime);
                            Global.TextManager.DrawText(5 * 8, 9 * 8, _currText, 22, Color.White, 0, World.CHIP_SIZE, false);
                            break;
                    }
                    break;
                case NPCTypes.Xelpud:
                    switch (State)
                    {
                        case NPCStates.INACTIVE:
                            break;
                        case NPCStates.ACTIVE:
                            DrawNPCRoom(spriteBatch, gameTime);
                            Global.TextManager.DrawText(5 * 8, 9 * 8, _currText, 22, Color.White, 0, World.CHIP_SIZE, false);
                            break;
                    }
                    break;
            }

        }

        private void DrawNPCRoom(SpriteBatch spriteBatch, GameTime gameTime, int dBoxHeight = 18)
        {
            HelperFunctions.DrawBlackSplashscreen(spriteBatch, false);
            _NPCGraphic.Draw(spriteBatch, new Vector2(15 * 8, Main.HUD_HEIGHT + 2 * 8));
            for (var x = 0; x < 11; x++)
            {
                _houseGraphicTop.Draw(spriteBatch, new Vector2(x * 24, 16));

                if (x != 5)
                    _houseGraphicBottom.Draw(spriteBatch, new Vector2(x * 24, 184));
                else
                    _houseGraphicBottomPart.Draw(spriteBatch, new Vector2(x * 24 + 16, 184));
            }

            _dialogueBoxSprites[(int)DialogueBoxSprites.TL].Draw(spriteBatch, new Vector2(4 * 8, 7 * 8));
            _dialogueBoxSprites[(int)DialogueBoxSprites.TR].Draw(spriteBatch, new Vector2(27 * 8, 7 * 8));
            _dialogueBoxSprites[(int)DialogueBoxSprites.BL].Draw(spriteBatch, new Vector2(4 * 8, dBoxHeight * 8));
            _dialogueBoxSprites[(int)DialogueBoxSprites.BR].Draw(spriteBatch, new Vector2(27 * 8, dBoxHeight * 8));

            for (var y = 8; y <= dBoxHeight - 1; y++)
            {
                _dialogueBoxSprites[(int)DialogueBoxSprites.L].Draw(spriteBatch, new Vector2(4 * 8, y * 8));
                _dialogueBoxSprites[(int)DialogueBoxSprites.R].Draw(spriteBatch, new Vector2(27 * 8, y * 8));
            }

            for (var x = 5; x <= 26; x++)
            {
                if (x != 15 && x != 16)
                    _dialogueBoxSprites[(int)DialogueBoxSprites.T].Draw(spriteBatch, new Vector2(x * 8, 7 * 8));

                _dialogueBoxSprites[(int)DialogueBoxSprites.B].Draw(spriteBatch, new Vector2(x * 8, dBoxHeight * 8));
            }
            _dialogueBoxSprites[(int)DialogueBoxSprites.TM].Draw(spriteBatch, new Vector2(15 * 8, 6 * 8));


            //_shopGraphicBottom.Draw(spriteBatch, new Vector2(64, 104));
            //            _shopGraphicTop = new Sprite(_tex, 64 + (clerkAndRoomGraphic % 8) * 24, 32 + (clerkAndRoomGraphic / 8) * 16, 24, 8);
            //            _shopGraphicBottom = new Sprite(_tex, 64 + (clerkAndRoomGraphic % 8) * 24, 40 + (clerkAndRoomGraphic / 8) * 16, 24, 8);
        }

        public override void Update(GameTime gameTime)
        {
            switch (_spawnType)
            {
                default:
                case NPCTypes.Store:
                    switch (State)
                    {
                        case NPCStates.INACTIVE:
                            if (CollidesWithPlayer())
                            {
                                if (InputManager.PressedKeys[(int)ControllerKeys.DOWN])
                                {
                                    if (_myBGM >= 0)
                                        Global.AudioManager.ChangeSongs(_myBGM);
                                    State = NPCStates.ACTIVE;
                                    _protag.State = PlayerState.NPC_DIALOGUE;
                                }
                            }
                            break;
                        case NPCStates.ACTIVE:
                            _shopCursorValue += InputManager.DirPressedX;

                            if (_shopCursorValue > 2)
                                _shopCursorValue = 0;
                            if (_shopCursorValue < 0)
                                _shopCursorValue = 2;

                            if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_CONFIRM])
                            {
                                _currText = _dialogueStr[(int)ShopDialogue.PRODUCT_1_HOVER + _shopCursorValue];
                            }

                            if (InputManager.PressedKeys[(int)ControllerKeys.MENU_CANCEL])
                            {
                                Global.AudioManager.ChangeSongs(Global.World.GetCurrField().MusicNumber);
                                State = NPCStates.INACTIVE;
                                _protag.State = PlayerState.IDLE;
                            }
                            break;
                    }
                    break;
                case NPCTypes.Storyteller:
                    switch (State)
                    {
                        case NPCStates.INACTIVE:
                            if (CollidesWithPlayer())
                            {
                                if (InputManager.PressedKeys[(int)ControllerKeys.DOWN])
                                {
                                    if (_myBGM >= 0)
                                        Global.AudioManager.ChangeSongs(_myBGM);
                                    State = NPCStates.ACTIVE;
                                    _protag.State = PlayerState.NPC_DIALOGUE;
                                }
                            }
                            break;
                        case NPCStates.ACTIVE:
                            if (InputManager.PressedKeys[(int)ControllerKeys.MENU_CANCEL])
                            {
                                Global.AudioManager.ChangeSongs(Global.World.GetCurrField().MusicNumber);
                                State = NPCStates.INACTIVE;
                                _protag.State = PlayerState.IDLE;
                            }
                            break;
                    }
                    break;
                case NPCTypes.Xelpud:
                    switch (State)
                    {
                        case NPCStates.INACTIVE:
                            if (CollidesWithPlayer())
                            {
                                if (InputManager.PressedKeys[(int)ControllerKeys.DOWN])
                                {
                                    if (_myBGM >= 0)
                                        Global.AudioManager.ChangeSongs(_myBGM);
                                    State = NPCStates.ACTIVE;
                                    _protag.State = PlayerState.NPC_DIALOGUE;
                                }
                            }
                            break;
                        case NPCStates.ACTIVE:
                            if (InputManager.PressedKeys[(int)ControllerKeys.MENU_CANCEL])
                            {
                                Global.AudioManager.ChangeSongs(Global.World.GetCurrField().MusicNumber);
                                State = NPCStates.INACTIVE;
                                _protag.State = PlayerState.IDLE;
                            }
                            break;
                    }
                    break;
            }
        }
    }
}