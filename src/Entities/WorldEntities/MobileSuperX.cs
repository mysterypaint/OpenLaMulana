using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;

namespace OpenLaMulana
{
    public class MobileSuperX : IGameEntity
    {
        private Global.MSXStates _state = Global.MSXStates.INACTIVE;

        public int Depth { get; set; } = (int)Global.DrawOrder.Overlay;

        Effect IGameEntity.ActiveShader => null;

        private Sprite[] mainWindowSprites = null;
        private Texture2D _tex = null;
        private Menu _f5Menu = null;

        enum WindowSprites
        {
            TL,
            TM,
            TM2,
            TR,
            L,
            L2,
            BLU,
            BL,
            R,
            R2,
            BRU,
            BR,
            AB,
            BM,
            MS,
            SX,

            INV_TL,
            INV_TR,
            INV_BL,
            INV_BR,
            INV_BLTL,
            INV_BRTR,
            INV_T,
            INV_B,
            INV_L,
            INV_R,
            INV_M,

            SCANNER_OK,

            EMU_CURSOR,

            MAX
        };

        enum SoftwareSelectionStates
        {
            SELECTING_CART_SLOT,
            SELECTING_SOFTWARE,
            MAX
        };

        string[] _strOptions = {
            "Load Save",
            "Configure Controls",
            "BGM Volume = 127",
            "SFX Volume = 100",
            "Language = English",
            "Platforming Physics\\10    [Classic]  QoL",
            "\\10QoL Changes = OFF",
            "\\10Return to Title",
        };

        private List<Sprite> _treasureSprites = new List<Sprite>();
        private List<Sprite> _weaponSprites = new List<Sprite>();
        private List<Sprite> _subWeaponSprites = new List<Sprite>();
        private Sprite[] _romSprites = new Sprite[12];
        private Sprite _softwareSelectionCursor = null;
        private Sprite _subWeaponSelectionCursor = null;
        private Sprite _mainWeaponSelectionCursor = null;
        private string _scannerText = String.Empty;
        private Sprite _tabletLeftImage;
        private Sprite _tabletRightImage;
        private SoftwareSelectionStates _softwareSelectionState = SoftwareSelectionStates.SELECTING_CART_SLOT;
        private bool _inventoryCursorVisible = true;
        private int _inventoryCursorBlinkTimer = -1;
        private int _inventoryCursorBlinkTimerReset = 15;
        private int _softwareSelectionCursorPosition = 0;
        private int _softwareCartSlotCursorPosition = 0;

        public MobileSuperX()
        {
            InitAllMenuSprites();
            _f5Menu = new Menu();
            _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
        }

        internal void Update(GameTime gameTime)
        {
            // Abort if we're configuring anything in the Config menu
            if (_f5Menu.IsInputting()) {
                _f5Menu.Update(gameTime);
                return;
            }
            if (_state != Global.MSXStates.INACTIVE)
            {
                if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_MOVE_RIGHT])
                {
                    Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    _state += 1;
                    if (_state > Global.MSXStates.MAX - 1)
                    {
                        _state = (Global.MSXStates)1;
                    }
                }
                else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_MOVE_LEFT])
                {
                    Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    _state -= 1;
                    if (_state <= Global.MSXStates.INACTIVE)
                    {
                        _state = Global.MSXStates.MAX - 1;
                    }
                }

                if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_CANCEL] && _state != Global.MSXStates.EMULATOR && _state != Global.MSXStates.CONFIG_SCREEN)
                {
                    Global.Main.SetState(Global.GameState.PLAYING);
                    Global.MobileSuperX.SetState(Global.MSXStates.INACTIVE);


                    _softwareSelectionState = SoftwareSelectionStates.SELECTING_CART_SLOT;
                    _softwareCartSlotCursorPosition = 0;
                    _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                    _inventoryCursorVisible = true;
                }
            }

            switch (_state)
            {
                case Global.MSXStates.INACTIVE:
                    break;
                case Global.MSXStates.SCANNING: // For Handy Scanner
                    if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_CONFIRM])
                    {
                        Global.AudioManager.PlaySFX(SFX.HANDY_SCANNER_DONE);
                    }
                    break;
                case Global.MSXStates.INVENTORY:
                    if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_INVENTORY])
                    {
                        Global.Main.SetState(Global.GameState.PLAYING);
                        Global.MobileSuperX.SetState(Global.MSXStates.INACTIVE);
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_MSX_EMULATOR])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.EMULATOR);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_MSX_ROM_SELECTION])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.SOFTWARE_SELECTION);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_CONFIG])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.CONFIG_SCREEN);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    }
                    break;
                case Global.MSXStates.SOFTWARE_SELECTION:
                    if (Global.AnimationTimer.OneFrameElapsed())
                    {
                        if (_inventoryCursorBlinkTimer <= 0)
                        {
                            _inventoryCursorVisible = !_inventoryCursorVisible;
                            _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                        }
                        else
                        {
                            _inventoryCursorBlinkTimer--;
                        }
                    }


                    switch (_softwareSelectionState)
                    {
                        case SoftwareSelectionStates.SELECTING_CART_SLOT:
                            if (Global.Inventory.ObtainedTreasures[Global.ObtainableTreasures.MSX2])
                            {
                                if (InputManager.DirPressedY != 0)
                                {
                                    _softwareCartSlotCursorPosition += InputManager.DirPressedY;
                                    if (_softwareCartSlotCursorPosition > 1)
                                        _softwareCartSlotCursorPosition = 0;
                                    else if (_softwareCartSlotCursorPosition < 0)
                                        _softwareCartSlotCursorPosition = 1;

                                    _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                                    _inventoryCursorVisible = true;
                                    Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);
                                }
                            }

                            if (InputManager.PressedKeys[(int)Global.ControllerKeys.WHIP])
                            {
                                Global.Inventory.EquippedRoms[_softwareCartSlotCursorPosition] = Global.ObtainableSoftware.NONE;
                            }

                            if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_CONFIRM])
                            {
                                int result = PlayerHasAtLeastOneSoftware();
                                if (result >= 0)
                                {
                                    _softwareSelectionState = SoftwareSelectionStates.SELECTING_SOFTWARE;
                                    _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                                    _inventoryCursorVisible = true;
                                    if (Global.Inventory.EquippedRoms[_softwareCartSlotCursorPosition] != Global.ObtainableSoftware.NONE)
                                    {
                                        _softwareSelectionCursorPosition = (int)Global.Inventory.EquippedRoms[_softwareCartSlotCursorPosition];
                                    } else
                                    {
                                        _softwareSelectionCursorPosition = result;
                                    }
                                    Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);
                                }
                            }
                            break;
                        case SoftwareSelectionStates.SELECTING_SOFTWARE:
                            if (InputManager.DirPressedY != 0)
                            {
                                var searchingDirectionY = InputManager.DirPressedY;
                                int _foundSoftware = FindSoftware(new Vector2 (0, searchingDirectionY), _softwareSelectionCursorPosition);

                                if (_foundSoftware >= 0) {
                                    _softwareSelectionCursorPosition = _foundSoftware;
                                    _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                                    _inventoryCursorVisible = true;
                                    Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);
                                }
                            } else if (InputManager.DirPressedX != 0)
                            {
                                /*
                                if moving horizontally:
                                - keep looking toward the direction we are moving until we exhaust the possibilities
                                - stop and do nothing, if we find nothing*/

                                var searchingDirectionY = InputManager.DirPressedY;


                                _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                                _inventoryCursorVisible = true;
                                Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);
                            }





                            if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_CONFIRM])
                            {
                                // Equip the software we're hovering over, but only if the other slot does not contain the software that we're trying to equip
                                int otherSlot = _softwareCartSlotCursorPosition + 1;
                                if (otherSlot > 1)
                                    otherSlot = 0;

                                if (Global.Inventory.EquippedRoms[otherSlot] != (Global.ObtainableSoftware)_softwareSelectionCursorPosition)
                                {
                                    _softwareSelectionState = SoftwareSelectionStates.SELECTING_CART_SLOT;
                                    _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                                    _inventoryCursorVisible = true;

                                    Global.Inventory.EquippedRoms[_softwareCartSlotCursorPosition] = (Global.ObtainableSoftware)_softwareSelectionCursorPosition;
                                    Global.AudioManager.PlaySFX(SFX.MSX_NAVIGATE);
                                }
                            }
                            else if (InputManager.PressedKeys[(int)Global.ControllerKeys.WHIP])
                            {
                                _softwareSelectionState = SoftwareSelectionStates.SELECTING_CART_SLOT;
                                _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                                _inventoryCursorVisible = true;
                            }
                            break;
                    }


                    if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_INVENTORY])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.INVENTORY);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                        _softwareSelectionState = SoftwareSelectionStates.SELECTING_CART_SLOT;
                        _softwareCartSlotCursorPosition = 0;
                        _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                        _inventoryCursorVisible = true;
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_MSX_EMULATOR])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.EMULATOR);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                        _softwareSelectionState = SoftwareSelectionStates.SELECTING_CART_SLOT;
                        _softwareCartSlotCursorPosition = 0;
                        _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                        _inventoryCursorVisible = true;
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_MSX_ROM_SELECTION])
                    {
                        Global.Main.SetState(Global.GameState.PLAYING);
                        Global.MobileSuperX.SetState(Global.MSXStates.INACTIVE);
                        _softwareSelectionState = SoftwareSelectionStates.SELECTING_CART_SLOT;
                        _softwareCartSlotCursorPosition = 0;
                        _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                        _inventoryCursorVisible = true;
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_CONFIG])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.CONFIG_SCREEN);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                        _softwareSelectionState = SoftwareSelectionStates.SELECTING_CART_SLOT;
                        _softwareCartSlotCursorPosition = 0;
                        _inventoryCursorBlinkTimer = _inventoryCursorBlinkTimerReset;
                        _inventoryCursorVisible = true;
                    }
                    break;
                case Global.MSXStates.EMULATOR:
                    if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_INVENTORY])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.INVENTORY);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_MSX_EMULATOR])
                    {
                        Global.Main.SetState(Global.GameState.PLAYING);
                        Global.MobileSuperX.SetState(Global.MSXStates.INACTIVE);
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_MSX_ROM_SELECTION])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.SOFTWARE_SELECTION);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_CONFIG])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.CONFIG_SCREEN);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    }
                    break;
                case Global.MSXStates.CONFIG_SCREEN:
                    if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_INVENTORY])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.INVENTORY);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_MSX_EMULATOR])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.EMULATOR);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_MSX_ROM_SELECTION])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.SOFTWARE_SELECTION);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_CONFIG])
                    {
                        Global.Main.SetState(Global.GameState.PLAYING);
                        Global.MobileSuperX.SetState(Global.MSXStates.INACTIVE);
                    }

                    _f5Menu.Update(gameTime);
                    break;
            }
        }

        private int FindSoftware(Vector2 movingDirection, int initialRomPosition)
        {
            int romPosX = initialRomPosition % 14;
            int romPosY = initialRomPosition / 14;

            if (movingDirection.Y != 0)
            {
                /*
                 * default position: look from left->right, top->bottom for the very first rom.
                    if absolutely nothing is found, do not do anything

                    if moving vertically:
                        - look at rom below in selection
                        - no rom? go left
                        - still no rom? go right
                        - still no rom? go left * 2
                        - still no rom? go right * 2
                        - repeat until exhausted, then look at the next row
                        - repeat until we hit the end; do not wrap around vertically
                */
                int checkingRow = 1;
                int checkingColumn = 1;
                bool stopSearchingLeft = false;
                bool stopSearchingRight = false;

                if (romPosY + (movingDirection.Y * checkingRow) >= 0 && romPosY + (movingDirection.Y * checkingRow) < (int)Global.ObtainableSoftware.MAX)
                {
                    romPosY += (int)movingDirection.Y;

                    int currCheckingRom = (romPosY * 14) + romPosX + checkingColumn;

                    if (currCheckingRom >= (int)Global.ObtainableSoftware.MAX || currCheckingRom < 0)
                        return -1;

                    while (!Global.Inventory.ObtainedSoftware[(Global.ObtainableSoftware)currCheckingRom])
                    {
                        checkingColumn *= -1;

                        if (stopSearchingLeft && romPosX + checkingColumn < 0)
                            checkingColumn *= -1;
                        else if (stopSearchingRight && romPosX + checkingColumn >= 14)
                            checkingColumn *= -1;

                        currCheckingRom = (romPosY * 14) + romPosX + checkingColumn;
                        if (romPosX + checkingColumn < 0 || romPosX + checkingColumn >= 14)
                        {
                            if (romPosX + checkingColumn < 0) {
                                stopSearchingLeft = true;
                                checkingColumn *= -1;

                                if (!stopSearchingRight)
                                {
                                    currCheckingRom = (romPosY * 14) + romPosX + checkingColumn;

                                    checkingColumn *= -1;
                                    continue;
                                }
                            }
                            else if (romPosX + checkingColumn > 14) {
                                stopSearchingRight = true;
                                checkingColumn++;

                                if (stopSearchingLeft && stopSearchingRight)
                                {
                                    romPosY += (int)movingDirection.Y;
                                    stopSearchingLeft = false;
                                    stopSearchingRight = false;
                                    checkingColumn = 1;
                                }
                                if (romPosY < 0 || (romPosY >= 5 && romPosX + checkingColumn > 14))
                                    break;
                                continue;
                            }

                            if (stopSearchingLeft && stopSearchingRight)
                            {
                                romPosY += (int)movingDirection.Y;
                                stopSearchingLeft = false;
                                stopSearchingRight = false;
                                checkingColumn = 1;
                            }

                            if (romPosY < 0 || (romPosY >= 5 && romPosX + checkingColumn >= 13))
                                break;

                            currCheckingRom = (romPosY * 14) + romPosX + checkingColumn;

                        }

                        if (checkingColumn > 0)
                            checkingColumn++;
                        else if (stopSearchingRight)
                            checkingColumn--;
                    }

                    if (currCheckingRom >= (int)Global.ObtainableSoftware.MAX || currCheckingRom < 0)
                        return -1;

                    if (Global.Inventory.ObtainedSoftware[(Global.ObtainableSoftware)currCheckingRom])
                        return currCheckingRom;
                }
                else
                {

                }


            } else
            {
                // Horizontal checking

            }




            return -1;
        }

        private int PlayerHasAtLeastOneSoftware()
        {
            for (Global.ObtainableSoftware soft = Global.ObtainableSoftware.GAME_MASTER; soft < Global.ObtainableSoftware.MAX; soft++)
            {
                if (Global.Inventory.ObtainedSoftware[soft])
                    return (int)soft;
            }
            return -1;
        }

        internal void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (_state)
            {
                case Global.MSXStates.INVENTORY:
                    DrawMSXBackground(spriteBatch, gameTime);
                    DrawInventoryWindowBorders(spriteBatch, gameTime);

                    int x = 0;
                    int y = 0;

                    /*
                     * 
                    int flagOffset = (int)GameFlags.Flags.MSX1_TAKEN;
                    if (Global.GameFlags.InGameFlags[flagOffset + i])*/


                    for (var i = 0; i < Global.Inventory.TreasureIcons.Length; i++)
                    {
                        //Global.TextManager.DrawText(3 * 8, 17 * 8, "SUB-WEAPON");
                        Global.ObtainableTreasures iconID = Global.Inventory.TreasureIcons[i];
                        if (iconID != Global.ObtainableTreasures.NOTHING)
                            _treasureSprites[(int)iconID].Draw(spriteBatch, new Vector2(48 + x * 16, 32 + y * 16));
                        //_treasureSprites[_inventoryOrder[i]].Draw(spriteBatch, new Vector2(48 + x * 16, 32 + y * 16));
                        x++;
                        if (x > 9)
                        {
                            x = 0;
                            y++;
                        }
                    }

                    int j = 0;
                    foreach(Global.MainWeapons weapons in Global.Inventory.ObtainedMainWeapons)
                    {
                        if (weapons != Global.MainWeapons.NONE)
                            _weaponSprites[(int)weapons].Draw(spriteBatch, new Vector2(14 * 8 + (j % 5) * 24, 14 * 8 + (j / 5) * 16));
                        j++;
                    }

                    j = 0;
                    foreach (Global.SubWeapons subWeapons in Global.Inventory.ObtainedSubWeapons)
                    {
                        if (subWeapons != Global.SubWeapons.NONE)
                            _subWeaponSprites[(int)subWeapons].Draw(spriteBatch, new Vector2(14 * 8 + (j % 5) * 24, 17 * 8 + (j / 5) * 16));
                        j++;
                    }
                    break;
                case Global.MSXStates.SCANNING:
                    DrawMSXBackground(spriteBatch, gameTime, true);
                    Global.TextManager.DrawText(2 * 8, 4 * 8, _scannerText, 28, Color.White, 0, 8, true);

                    int leftTabletOffset = 12;
                    if (_tabletRightImage != null) {
                        _tabletRightImage.Draw(spriteBatch, new Vector2(16 * 8, 11 * 8));
                        leftTabletOffset = 8;
                    }
                    if (_tabletLeftImage != null)
                        _tabletLeftImage.Draw(spriteBatch, new Vector2(leftTabletOffset * 8, 11 * 8));

                    mainWindowSprites[(int)WindowSprites.SCANNER_OK].Draw(spriteBatch, new Vector2(2 * 8, 19 * 8));
                    mainWindowSprites[(int)WindowSprites.EMU_CURSOR].Draw(spriteBatch, new Vector2(2 * 8, 20 * 8));
                    break;
                case Global.MSXStates.SOFTWARE_SELECTION:
                    DrawMSXBackground(spriteBatch, gameTime);
                    DrawSoftwareSelectionScreen(spriteBatch, gameTime);

                    break;
                case Global.MSXStates.EMULATOR:
                    DrawMSXBackground(spriteBatch, gameTime, true);
                    Global.TextManager.DrawText(2 * 8, 3 * 8, "MSX? BASIC version 1.x\\10Copyright 1987 by Kobamisoft");
                    Global.TextManager.DrawText(2 * 8, 5 * 8, "8806 Bytes free\\10ROM BASIC version 1.0\\10Ok");
                    Global.TextManager.DrawText(2 * 8, 20 * 8, "color  auto  goto  list  run");
                    mainWindowSprites[(int)WindowSprites.EMU_CURSOR].Draw(spriteBatch, new Vector2(2 * 8, 8 * 8));
                    break;
                case Global.MSXStates.CONFIG_SCREEN:
                    DrawMSXBackground(spriteBatch, gameTime);

                    _f5Menu.Draw(spriteBatch, gameTime);

                    /*
                    Global.TextManager.DrawText(4 * 8, 2 * 8, "- Options -");
                    
                    y = 0;
                    foreach (string str in _strOptions)
                    {
                        Global.TextManager.DrawText(8 * 8, (4 + y * 2) * 8, str);
                        y++;
                    }

                    int cursorPosition = 0;
                    mainWindowSprites[(int)WindowSprites.OPTIONS_CURSOR].Draw(spriteBatch, new Vector2(6 * 8, (4 + cursorPosition * 2) * 8));
                     */
                    break;
            }
        }

        private void DrawSoftwareSelectionScreen(SpriteBatch spriteBatch, GameTime gameTime)
        {
            string msxStr = "MSX WINDOW";

            if (Global.Inventory.ObtainedTreasures[Global.ObtainableTreasures.MSX2] == true)
                msxStr = "MSX2WINDOW";

            Global.TextManager.DrawText(1 * 8, 2 * 8, msxStr);
            Global.TextManager.DrawText(2 * 8, 5 * 8, "SLOT1");
            // Grab and draw the player's equipped roms
            int eqRom1 = (int)Global.Inventory.EquippedRoms[0];
            int equippedRomSprID1 = Global.World.SoftwareGetGraphicID(eqRom1);

            if (equippedRomSprID1 >= 0)
                _romSprites[equippedRomSprID1].Draw(spriteBatch, new Vector2(8 * 8, 4 * 8));

            Global.TextManager.DrawText(11 * 8, 5 * 8, Global.TextManager.GetText((int)Global.HardCodedText.SOFTWARE_NAMES_BEGIN + eqRom1, Global.CurrLang));

            mainWindowSprites[(int)WindowSprites.INV_TL].Draw(spriteBatch, new Vector2(7 * 8, 3 * 8));
            mainWindowSprites[(int)WindowSprites.INV_TR].Draw(spriteBatch, new Vector2(10 * 8, 3 * 8));

            // Draw the second cart slot, but only if we have the MobileSuperX2 unlocked
            if (Global.Inventory.ObtainedTreasures[Global.ObtainableTreasures.MSX2])
            {
                Global.TextManager.DrawText(2 * 8, 8 * 8, "SLOT2");
                int eqRom2 = (int)Global.Inventory.EquippedRoms[1];
                int equippedRomSprID2 = Global.World.SoftwareGetGraphicID(eqRom2);
                if (equippedRomSprID2 >= 0)
                    _romSprites[equippedRomSprID2].Draw(spriteBatch, new Vector2(8 * 8, 7 * 8));
                Global.TextManager.DrawText(11 * 8, 8 * 8, Global.TextManager.GetText((int)Global.HardCodedText.SOFTWARE_NAMES_BEGIN + eqRom2, Global.CurrLang));

                mainWindowSprites[(int)WindowSprites.INV_BLTL].Draw(spriteBatch, new Vector2(7 * 8, 6 * 8));
                mainWindowSprites[(int)WindowSprites.INV_BRTR].Draw(spriteBatch, new Vector2(10 * 8, 6 * 8));

                mainWindowSprites[(int)WindowSprites.INV_BL].Draw(spriteBatch, new Vector2(7 * 8, 9 * 8));
                mainWindowSprites[(int)WindowSprites.INV_BR].Draw(spriteBatch, new Vector2(10 * 8, 9 * 8));


                for (int x = 8; x < 10; x++)
                {
                    mainWindowSprites[(int)WindowSprites.INV_T].Draw(spriteBatch, new Vector2(x * 8, 3 * 8));
                    mainWindowSprites[(int)WindowSprites.INV_M].Draw(spriteBatch, new Vector2(x * 8, 6 * 8));
                    mainWindowSprites[(int)WindowSprites.INV_B].Draw(spriteBatch, new Vector2(x * 8, 9 * 8));
                }
                for (int y = 4; y < 9; y++)
                {
                    if (y == 6)
                        continue;
                    mainWindowSprites[(int)WindowSprites.INV_L].Draw(spriteBatch, new Vector2(7 * 8, y * 8));
                    mainWindowSprites[(int)WindowSprites.INV_R].Draw(spriteBatch, new Vector2(10 * 8, y * 8));
                }
            } else
            {
                // We don't have the upgrade; draw the MSX1 screen instead
                mainWindowSprites[(int)WindowSprites.INV_BL].Draw(spriteBatch, new Vector2(7 * 8, 6 * 8));
                mainWindowSprites[(int)WindowSprites.INV_BR].Draw(spriteBatch, new Vector2(10 * 8, 6 * 8));


                for (int x = 8; x < 10; x++)
                {
                    mainWindowSprites[(int)WindowSprites.INV_T].Draw(spriteBatch, new Vector2(x * 8, 3 * 8));
                    mainWindowSprites[(int)WindowSprites.INV_B].Draw(spriteBatch, new Vector2(x * 8, 6 * 8));
                }
                for (int y = 4; y < 6; y++)
                {
                    mainWindowSprites[(int)WindowSprites.INV_L].Draw(spriteBatch, new Vector2(7 * 8, y * 8));
                    mainWindowSprites[(int)WindowSprites.INV_R].Draw(spriteBatch, new Vector2(10 * 8, y * 8));
                }
            }

            // Draw all the obtained software
            for (Global.ObtainableSoftware romID = (Global.ObtainableSoftware)0; romID < Global.ObtainableSoftware.MAX; romID++)
            {
                if (Global.Inventory.ObtainedSoftware[romID]) {
                    int softwareSpriteID = Global.World.SoftwareGetGraphicID((int)romID);
                    _romSprites[softwareSpriteID].Draw(spriteBatch, new Vector2(2 * 8 + ((int)romID % 14) * 16, 10 * 8 + ((int)romID / 14) * 16));
                }
            }

            // Draw the selection cursors
            switch (_softwareSelectionState)
            {
                case SoftwareSelectionStates.SELECTING_CART_SLOT:
                    if (_inventoryCursorVisible)
                        _softwareSelectionCursor.Draw(spriteBatch, new Vector2(8 * 8, 4 * 8 + (_softwareCartSlotCursorPosition * World.CHIP_SIZE * 3)));
                    break;
                case SoftwareSelectionStates.SELECTING_SOFTWARE:
                    string softwareName = Global.TextManager.GetText((int)Global.HardCodedText.SOFTWARE_NAMES_BEGIN + _softwareSelectionCursorPosition, Global.CurrLang);
                    Global.TextManager.DrawText(12 * 8, 2 * 8, softwareName);
                    if (_inventoryCursorVisible)
                        _softwareSelectionCursor.Draw(spriteBatch, new Vector2(2 * 8 + ((int)_softwareSelectionCursorPosition % 14) * 16, 10 * 8 + ((int)_softwareSelectionCursorPosition / 14) * 16));
                    break;
            }
        }

        private void DrawInventoryWindowBorders(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Global.TextManager.DrawText(4 * 8, 2 * 8, "ITEM WINDOW");
            Global.TextManager.DrawText(7 * 8, 14 * 8, "WEAPON");
            Global.TextManager.DrawText(3 * 8, 17 * 8, "SUB-WEAPON");

            mainWindowSprites[(int)WindowSprites.INV_TL].Draw(spriteBatch, new Vector2(13 * 8, 13 * 8));
            mainWindowSprites[(int)WindowSprites.INV_TR].Draw(spriteBatch, new Vector2(28 * 8, 13 * 8));
            mainWindowSprites[(int)WindowSprites.INV_BL].Draw(spriteBatch, new Vector2(13 * 8, 21 * 8));
            mainWindowSprites[(int)WindowSprites.INV_BR].Draw(spriteBatch, new Vector2(28 * 8, 21 * 8));
            mainWindowSprites[(int)WindowSprites.INV_BLTL].Draw(spriteBatch, new Vector2(13 * 8, 16 * 8));
            mainWindowSprites[(int)WindowSprites.INV_BRTR].Draw(spriteBatch, new Vector2(28 * 8, 16 * 8));

            for (int x = 14; x < 28; x++)
            {
                mainWindowSprites[(int)WindowSprites.INV_T].Draw(spriteBatch, new Vector2(x * 8, 13 * 8));
                mainWindowSprites[(int)WindowSprites.INV_B].Draw(spriteBatch, new Vector2(x * 8, 21 * 8));
                mainWindowSprites[(int)WindowSprites.INV_M].Draw(spriteBatch, new Vector2(x * 8, 16 * 8));
            }

            for (int y = 14; y < 21; y++)
            {
                if (y != 16)
                {
                    mainWindowSprites[(int)WindowSprites.INV_L].Draw(spriteBatch, new Vector2(13 * 8, y * 8));
                    mainWindowSprites[(int)WindowSprites.INV_R].Draw(spriteBatch, new Vector2(28 * 8, y * 8));
                }
            }
        }

        private void DrawMSXBackground(SpriteBatch spriteBatch, GameTime gameTime, bool drawBlue = false)
        {
            HelperFunctions.DrawBlackSplashscreen(spriteBatch, true);

            if (drawBlue)
                HelperFunctions.DrawSkyBlueSplashscreen(spriteBatch, false);

            mainWindowSprites[(int)WindowSprites.TL].Draw(spriteBatch, new Vector2(0, 0));
            mainWindowSprites[(int)WindowSprites.TR].Draw(spriteBatch, new Vector2(31 * 8, 0));
            mainWindowSprites[(int)WindowSprites.BL].Draw(spriteBatch, new Vector2(0, 23 * 8));
            mainWindowSprites[(int)WindowSprites.BR].Draw(spriteBatch, new Vector2(31 * 8, 23 * 8));

            for (var y = 1; y <= 22; y++)
            {
                if (y == 22)
                {
                    mainWindowSprites[(int)WindowSprites.BLU].Draw(spriteBatch, new Vector2(0, y * 8));
                    mainWindowSprites[(int)WindowSprites.BRU].Draw(spriteBatch, new Vector2(31 * 8, y * 8));
                }
                else if (y == 11)
                {
                    mainWindowSprites[(int)WindowSprites.L2].Draw(spriteBatch, new Vector2(0, y * 8));
                    mainWindowSprites[(int)WindowSprites.R2].Draw(spriteBatch, new Vector2(31 * 8, y * 8));
                }
                else
                {
                    mainWindowSprites[(int)WindowSprites.L].Draw(spriteBatch, new Vector2(0, y * 8));
                    mainWindowSprites[(int)WindowSprites.R].Draw(spriteBatch, new Vector2(31 * 8, y * 8));
                }
            }

            for (var x = 1; x <= 30; x++)
            {
                if (x == 5 || x == 26)
                    mainWindowSprites[(int)WindowSprites.TM2].Draw(spriteBatch, new Vector2(x * 8, 0 * 8));
                else
                    mainWindowSprites[(int)WindowSprites.TM].Draw(spriteBatch, new Vector2(x * 8, 0 * 8));

                if (_state != Global.MSXStates.EMULATOR && _state != Global.MSXStates.SCANNING)
                    mainWindowSprites[(int)WindowSprites.AB].Draw(spriteBatch, new Vector2(x * 8, 22 * 8));
                
                if (x == 15)
                    mainWindowSprites[(int)WindowSprites.MS].Draw(spriteBatch, new Vector2(x * 8, 23 * 8));
                else if (x == 16)
                    mainWindowSprites[(int)WindowSprites.SX].Draw(spriteBatch, new Vector2(x * 8, 23 * 8));
                else
                    mainWindowSprites[(int)WindowSprites.BM].Draw(spriteBatch, new Vector2(x * 8, 23 * 8));
            }
        }

        public void SetState(Global.MSXStates state)
        {
            _state = state;
        }

        private void InitAllMenuSprites()
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _mainWeaponSelectionCursor = Global.TextureManager.Get16x16Tile(_tex, 4, 1, new Vector2(8, 0));
            _subWeaponSelectionCursor = Global.TextureManager.Get16x16Tile(_tex, 5, 1, new Vector2(8, 0));
            _softwareSelectionCursor = Global.TextureManager.Get16x16Tile(_tex, 6, 1, new Vector2(8, 0));

            Sprite tl = Global.TextureManager.Get8x8Tile(_tex, 13, 0, Vector2.Zero);
            Sprite tm = Global.TextureManager.Get8x8Tile(_tex, 14, 0, Vector2.Zero);
            Sprite tm2 = Global.TextureManager.Get8x8Tile(_tex, 15, 0, Vector2.Zero);
            Sprite tr = Global.TextureManager.Get8x8Tile(_tex, 16, 0, Vector2.Zero);
            Sprite l = Global.TextureManager.Get8x8Tile(_tex, 17, 0, Vector2.Zero);
            Sprite l2 = Global.TextureManager.Get8x8Tile(_tex, 18, 0, Vector2.Zero);
            Sprite blu = Global.TextureManager.Get8x8Tile(_tex, 19, 0, Vector2.Zero);
            Sprite bl = Global.TextureManager.Get8x8Tile(_tex, 20, 0, Vector2.Zero);
            Sprite r = Global.TextureManager.Get8x8Tile(_tex, 21, 0, Vector2.Zero);
            Sprite r2 = Global.TextureManager.Get8x8Tile(_tex, 22, 0, Vector2.Zero);
            Sprite bru = Global.TextureManager.Get8x8Tile(_tex, 23, 0, Vector2.Zero);
            Sprite br = Global.TextureManager.Get8x8Tile(_tex, 24, 0, Vector2.Zero);
            Sprite ab = Global.TextureManager.Get8x8Tile(_tex, 25, 0, Vector2.Zero);
            Sprite bm = Global.TextureManager.Get8x8Tile(_tex, 26, 0, Vector2.Zero);
            Sprite ms = Global.TextureManager.Get8x8Tile(_tex, 27, 0, Vector2.Zero);
            Sprite sx = Global.TextureManager.Get8x8Tile(_tex, 28, 0, Vector2.Zero);

            Sprite invTL = Global.TextureManager.Get8x8Tile(_tex, 3, 2, Vector2.Zero);
            Sprite invTR = Global.TextureManager.Get8x8Tile(_tex, 6, 3, Vector2.Zero);
            Sprite invBL = Global.TextureManager.Get8x8Tile(_tex, 4, 3, Vector2.Zero);
            Sprite invBR = Global.TextureManager.Get8x8Tile(_tex, 7, 3, Vector2.Zero);
            Sprite invBLTL = Global.TextureManager.Get8x8Tile(_tex, 4, 2, Vector2.Zero);
            Sprite invBRTR = Global.TextureManager.Get8x8Tile(_tex, 7, 2, Vector2.Zero);
            Sprite invT = Global.TextureManager.Get8x8Tile(_tex, 5, 2, Vector2.Zero);
            Sprite invB = Global.TextureManager.Get8x8Tile(_tex, 6, 2, Vector2.Zero);
            Sprite invL = Global.TextureManager.Get8x8Tile(_tex, 3, 3, Vector2.Zero);
            Sprite invR = Global.TextureManager.Get8x8Tile(_tex, 8, 2, Vector2.Zero);
            Sprite invM = Global.TextureManager.Get8x8Tile(_tex, 5, 3, Vector2.Zero);


            Sprite scannerOK = new Sprite(_tex, 19 * 8, 2 * 8, 16, 8);
            Sprite emuCursor = Global.TextureManager.Get8x8Tile(_tex, 19, 3, Vector2.Zero);
            
            mainWindowSprites = new Sprite[] { tl, tm, tm2, tr, l, l2, blu, bl, r, r2, bru, br, ab, bm, ms, sx,
                invTL, invTR, invBL, invBR, invBLTL, invBRTR, invT, invB, invL, invR, invM,
                scannerOK, emuCursor};

            for (var i = 0; i < 60; i++)
            {
                int tx = (i % 20);
                int ty = (i / 20);
                _treasureSprites.Add(Global.TextureManager.Get16x16Tile(_tex, 0 + tx, 12 + ty, Vector2.Zero));
            }

            for (var i = 0; i < 8; i++)
            {
                _weaponSprites.Add(Global.TextureManager.Get16x16Tile(_tex, 16 + i % 4, 0 + i / 4, Vector2.Zero));
            }

            for (var i = 0; i < 13; i++)
            {
                _subWeaponSprites.Add(Global.TextureManager.Get16x16Tile(_tex, 16 + i % 4, 6 + i / 4, Vector2.Zero));
            }

            for (var i = 1; i <= 12; i++)
            {
                _romSprites[i - 1] = new Sprite(_tex, 256 + (i) % 4 * 16, 144 + (i) / 4 * 16, 16, 16);
            }
        }

        void IGameEntity.Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        void IGameEntity.Update(GameTime gameTime)
        {
        }

        internal Global.MSXStates GetState()
        {
            return _state;
        }

        internal void SetScannerText(string str, Sprite tabletLeftImage, Sprite tabletRightImage)
        {
            _scannerText = str;
            _tabletLeftImage = tabletLeftImage;
            _tabletRightImage = tabletRightImage;
        }
    }
}