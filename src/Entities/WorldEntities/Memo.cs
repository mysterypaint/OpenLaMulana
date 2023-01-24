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
    internal class Memo : InteractableWorldEntity
    {
        Protag _protag = Global.Protag;
        Sprite _tabletLeftImage = null;
        Sprite _tabletRightImage = null;
        string _textData = String.Empty;
        private string _currText = String.Empty;
        public override int HitboxWidth { get; set; } = 16;
        public override int HitboxHeight { get; set; } = 16;

        private int _flagsToSet = -1;
        private bool _burnAfterReading = false;

        public Memo(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            State = Global.WEStates.UNSPAWNED;

            _textData = Global.TextManager.GetText(op1, Global.CurrLang);

            _flagsToSet = op2;

            if (_flagsToSet >= 10000)
            {
                _flagsToSet -= 10000;
                _burnAfterReading = true;
            }
            
            Position += new Vector2(0, -8);

            if (HelperFunctions.EntityMaySpawn(StartFlags))
                State = Global.WEStates.IDLE;
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                        State = Global.WEStates.IDLE;
                    break;
                case Global.WEStates.IDLE:
                    var cBox = _protag.BBox;
                    if (BBox.Intersects(cBox))
                    {
                        if (InputManager.PressedKeys[(int)Global.ControllerKeys.SUB_WEAPON] && Global.Inventory.EquippedSubWeapon == Global.SubWeapons.HANDY_SCANNER && _protag.IsGrounded())
                        {
                            State = Global.WEStates.ACTIVATING;
                            Global.MobileSuperX.SetState(Global.MSXStates.SCANNING);
                            Global.Main.SetState(Global.GameState.MSX_OPEN);
                            _currText = _textData;
                            Global.MobileSuperX.SetScannerText(_currText, _tabletLeftImage, _tabletRightImage, false);
                            Global.AudioManager.PlaySFX(SFX.HANDY_SCANNER_DONE);
                        }
                    }
                    break;
                case Global.WEStates.ACTIVATING:
                    if (Global.AnimationTimer.OneFrameElapsed())
                    {
                        State = Global.WEStates.RESETTING;
                        Global.AudioManager.PlaySFX(SFX.HANDY_SCANNER_DONE);
                    }
                    break;
                case Global.WEStates.RESETTING: // This won't run immediately because the Entity Manager isn't running: It will execute as soon as the player exits the MobileSuperX
                    State = Global.WEStates.IDLE;
                    if (_flagsToSet >= 0)
                    {
                        Global.GameFlags.InGameFlags[_flagsToSet] = true;

                        if (_burnAfterReading || !HelperFunctions.EntityMaySpawn(StartFlags))
                            State = Global.WEStates.UNSPAWNED;
                    }
                    Global.MobileSuperX.SetState(Global.MSXStates.INACTIVE);
                    Global.Main.SetState(Global.GameState.PLAYING);
                    break;
            }
        }
    }
}