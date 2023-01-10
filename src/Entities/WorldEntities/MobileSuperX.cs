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

        int[] _inventoryOrder = new int[] {
            0, 2, 3, 16, 7, 24, 6, 4, 38, 27,
            10, 33, 28, 13, 22, 32, 21, 1, 37, 11,
            20, 5, 31, 23, 25, 36, 26, 30, 19, 35,
            9, 17, 18, 29, 14, 12, 34, 15, 42, 39,
            40, 46, 47, 8, 41, 48, 49, 50};

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
        public MobileSuperX()
        {
            InitAllMenuSprites();
            _f5Menu = new Menu();
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
                }
            }

            switch (_state)
            {
                case Global.MSXStates.INACTIVE:
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
                        Global.MobileSuperX.SetState(Global.MSXStates.ROM_SELECTION);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_CONFIG])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.CONFIG_SCREEN);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
                    }
                    break;
                case Global.MSXStates.ROM_SELECTION:
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
                        Global.Main.SetState(Global.GameState.PLAYING);
                        Global.MobileSuperX.SetState(Global.MSXStates.INACTIVE);
                    }
                    else if (InputManager.PressedKeys[(int)Global.ControllerKeys.MENU_OPEN_CONFIG])
                    {
                        Global.MobileSuperX.SetState(Global.MSXStates.CONFIG_SCREEN);
                        Global.AudioManager.PlaySFX(SFX.MSX_OPEN);
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
                        Global.MobileSuperX.SetState(Global.MSXStates.ROM_SELECTION);
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
                        Global.MobileSuperX.SetState(Global.MSXStates.ROM_SELECTION);
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

        internal void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (_state)
            {
                case Global.MSXStates.INVENTORY:
                    DrawMSXBackground(spriteBatch, gameTime);
                    DrawInventoryWindowBorders(spriteBatch, gameTime);

                    int x = 0;
                    int y = 0;
                    for (var i = 0; i < _inventoryOrder.Length; i++)
                    {
                        //Global.TextManager.DrawText(3 * 8, 17 * 8, "SUB-WEAPON");
                        _treasureSprites[_inventoryOrder[i]].Draw(spriteBatch, new Vector2(48 + x * 16, 32 + y * 16));
                        x++;
                        if (x > 9)
                        {
                            x = 0;
                            y++;
                        }
                    }

                    int j = 0;
                    for (Global.Weapons weapons = Global.Weapons.WHIP; weapons <= Global.Weapons.KEYBLADE; weapons++)
                    {
                        _weaponSprites[(int)weapons - 1].Draw(spriteBatch, new Vector2(14 * 8 + (j % 5) * 24, 14 * 8 + (j / 5) * 16));
                        j++;
                    }

                    j = 0;
                    for (Global.SubWeapons subW = Global.SubWeapons.SHURIKEN; subW <= Global.SubWeapons.HANDY_SCANNER; subW++)
                    {
                        _subWeaponSprites[(int)subW - 1].Draw(spriteBatch, new Vector2(14 * 8 + (j % 5) * 24, 17 * 8 + (j / 5) * 16));
                        j++;
                    }
                    break;
                case Global.MSXStates.ROM_SELECTION:
                    DrawMSXBackground(spriteBatch, gameTime);
                    DrawRomSelectionScreen(spriteBatch, gameTime);
                    break;
                case Global.MSXStates.EMULATOR:
                    DrawMSXBackground(spriteBatch, gameTime, true);
                    Global.TextManager.DrawText(2 * 8, 3 * 8, "MSX? BASIC version 1.x\\10Copyright 1987 by Kobamisoft\\108806 Bytes free\\10ROM BASIC version 1.0\\10Ok");
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

        private void DrawRomSelectionScreen(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Global.TextManager.DrawText(1 * 8, 2 * 8, "MSX2WINDOW");
            Global.TextManager.DrawText(2 * 8, 5 * 8, "SLOT1");
            Global.TextManager.DrawText(2 * 8, 8 * 8, "SLOT2");

            // Grab and draw the player's equipped roms
            int eqRom1 = Global.Protag.GetInventory().EquippedRoms[0];
            int eqRom2 = Global.Protag.GetInventory().EquippedRoms[1];
            int equippedRomSprID1 = Global.World.SoftwareGetGraphicID(eqRom1);
            int equippedRomSprID2 = Global.World.SoftwareGetGraphicID(eqRom2);

            if (equippedRomSprID1 >= 0)
                _romSprites[equippedRomSprID1].Draw(spriteBatch, new Vector2(8 * 8, 4 * 8));
            if (equippedRomSprID2 >= 0)
                _romSprites[equippedRomSprID2].Draw(spriteBatch, new Vector2(8 * 8, 7 * 8));

            Global.TextManager.DrawText(11 * 8, 5 * 8, Global.TextManager.GetText((int)Global.HardCodedText.ROM_NAMES_BEGIN + eqRom1, Global.CurrLang));
            Global.TextManager.DrawText(11 * 8, 8 * 8, Global.TextManager.GetText((int)Global.HardCodedText.ROM_NAMES_BEGIN + eqRom2, Global.CurrLang));

            mainWindowSprites[(int)WindowSprites.INV_TL].Draw(spriteBatch, new Vector2(7 * 8, 3 * 8));
            mainWindowSprites[(int)WindowSprites.INV_TR].Draw(spriteBatch, new Vector2(10 * 8, 3 * 8));
            mainWindowSprites[(int)WindowSprites.INV_BL].Draw(spriteBatch, new Vector2(7 * 8, 9 * 8));
            mainWindowSprites[(int)WindowSprites.INV_BR].Draw(spriteBatch, new Vector2(10 * 8, 9 * 8));
            mainWindowSprites[(int)WindowSprites.INV_BLTL].Draw(spriteBatch, new Vector2(7 * 8, 6 * 8));
            mainWindowSprites[(int)WindowSprites.INV_BRTR].Draw(spriteBatch, new Vector2(10 * 8, 6 * 8));
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

            for (var romID = 0; romID < (int)Global.ObtainableSoftware.MAX; romID++)
            {
                int softwareSpriteID = Global.World.SoftwareGetGraphicID(romID);
                _romSprites[softwareSpriteID].Draw(spriteBatch, new Vector2(2 * 8 + (romID % 14) * 16, 10 * 8 + (romID / 14) * 16));
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

                if (_state != Global.MSXStates.EMULATOR)
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


            Sprite scannerOK = Global.TextureManager.Get8x8Tile(_tex, 19, 2, Vector2.Zero);
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
    }
}