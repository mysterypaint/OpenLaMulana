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
    internal class RuinsTablet : ParentInteractableWorldEntity
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

        public RuinsTablet(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            State = Global.WEStates.UNSPAWNED;

            UInt32[] pixels = new UInt32[16 * 16];
            pixels[0] = 0x00FF00FF;
            _textData = Global.TextManager.GetText(op1, Global.CurrLang);

            if (op2 >= 0 || op3 >= 0)
            {
                _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
                if (op2 >= 0)
                    _tabletLeftImage = Global.TextureManager.Get64x64Tile(_tex, op2);
                if (op3 >= 0)
                    _tabletRightImage = Global.TextureManager.Get64x64Tile(_tex, op3);
            }


            //OP4 can specify a flag that turns on when reading an ancient document in the decryption state.
            //If you specify a flag number plus 10000, after the flag is turned on, the old document event itself
            //will be deleted from the view. This is because the flag change is determined only when the view is switched,
            //and the ancient document display is not a view switch, so it does not respond well to the start flag,
            //so it is used for ancient documents that you want to disappear on the spot, such as a sinking stone monument.

            _flagsToSet = op4;
            if (_flagsToSet >= 10000)
            {
                _flagsToSet -= 10000;
                _burnAfterReading = true;
            }

            _tex = new Texture2D(Global.GraphicsDevice, 16, 16, false, SurfaceFormat.Color);
            _tex.SetData<UInt32>(pixels, 0, 16 * 16);
            
            _sprIndex = new Sprite(_tex, 48, 16, 16, 16);
            

            Position += new Vector2(0, -8);
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    break;
                case Global.WEStates.IDLE:
                    _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                    break;
                case Global.WEStates.ACTIVATING:
                    break;
                case Global.WEStates.RESETTING:
                    break;
            }
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
                        if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.SUB_WEAPON) && Global.Inventory.EquippedSubWeapon == Global.SubWeapons.HANDY_SCANNER && _protag.IsGrounded())
                        {
                            State = Global.WEStates.ACTIVATING;
                            Global.MobileSuperX.SetState(Global.MSXStates.SCANNING);
                            Global.Main.SetState(Global.GameState.MSX_OPEN);
                            _currText = _textData;
                            Global.MobileSuperX.SetScannerText(_currText, _tabletLeftImage, _tabletRightImage);
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
                        if (Global.Inventory.EquippedRoms[0] == Global.ObtainableSoftware.GLYPH_READER || Global.Inventory.EquippedRoms[1] == Global.ObtainableSoftware.GLYPH_READER)
                        {
                            Global.GameFlags.InGameFlags[_flagsToSet] = true;

                            if (_burnAfterReading || !HelperFunctions.EntityMaySpawn(StartFlags))
                                State = Global.WEStates.UNSPAWNED;
                        }
                    }
                    Global.MobileSuperX.SetState(Global.MSXStates.INACTIVE);
                    Global.Main.SetState(Global.GameState.PLAYING);
                    break;
            }
        }
    }
}