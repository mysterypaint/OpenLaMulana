using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using static OpenLaMulana.Global;
using static OpenLaMulana.System.Camera;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class SubBossOxHeadAndHorseFaceGenerator : AssembledInteractiveWorldEntity
    {
        private View _currView = null;
        private int _flagToSetWhenDefeated = -1;
        private bool _offsetPosition = false;

        public SubBossOxHeadAndHorseFaceGenerator(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags, Global.SpriteDefs sprSheetIndex) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            InitAssembly(sprSheetIndex);


            /*
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());

            
            _sprIndex = null;
            */
            _flagToSetWhenDefeated = op1;
            
            _currView = Global.World.GetCurrentView();

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = WEStates.IDLE;
                //Global.AudioManager.PlaySFX(SFX.ROOM_LOCKDOWN);
                Position = Vector2.Zero;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    break;
                case WEStates.IDLE:
                    if (!Visible)
                        return;
                    HelperFunctions.DrawSplashscreen(spriteBatch, true, Color.Gray);

                    if (Global.DevModeEnabled)
                    {
                        if (CollidesWithPlayer(Position))
                        {
                            if (_sprIndex.TintColor != Color.Red)
                                Global.AudioManager.PlaySFX(SFX.SHIELD_BLOCK);
                            _sprIndex.TintColor = Color.Red;
                        }
                        else
                        {
                            _sprIndex.TintColor = Color.White;
                        }
                    }

                    _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);

                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        Visible = true;
                        State = WEStates.IDLE;
                        //Global.AudioManager.PlaySFX(SFX.ROOM_LOCKDOWN);
                        Position = Vector2.Zero;
                    }
                    break;
                case WEStates.IDLE:
                    if (Global.DevModeEnabled)
                    {
                        if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.MAIN_WEAPON))
                        {
                            _offsetPosition = !_offsetPosition;
                        }
                        if (_offsetPosition)
                        {
                            Position = new Vector2(32, 45);
                        }
                        else
                        {
                            Position = new Vector2(128, 45);
                        }
                    }

                    if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.JUMP))
                    {
                        _sprDefIndex++;
                        if (_sprDefIndex >= _spritesMax)
                            _sprDefIndex = 0;

                        UpdateSpriteIndex();
                        UpdateMaskIndex();

                    }
                    break;
            }
        }
    }
}