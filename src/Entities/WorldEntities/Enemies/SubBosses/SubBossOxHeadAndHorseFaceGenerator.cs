using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Global;
using static OpenLaMulana.System.Camera;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class SubBossOxHeadAndHorseFaceGenerator : IRoomWorldEntity
    {
        private int spritesMax = 30;
        Sprite[] _sprites = new Sprite[30];
        private int _sprNum = 0;
        private View _currView = null;
        private int _flagToSetWhenDefeated = -1;

        public SubBossOxHeadAndHorseFaceGenerator(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.SpriteDefManager.GetTexture(Global.SpriteDefs.BOSS02);
            for (var i = 0; i < spritesMax; i++)
            {
                _sprites[i] = Global.SpriteDefManager.GetSprite(Global.SpriteDefs.BOSS02, i);
            }
            _sprIndex = _sprites[_sprNum];
            Position += new Vector2(200, -3);


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
                    //HelperFunctions.DrawSplashscreen(spriteBatch, true, Color.Gray);
                    //_sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
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

                    if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.JUMP))
                    {
                        _sprNum++;
                        if (_sprNum >= spritesMax)
                            _sprNum = 0;
                    }
                    _sprIndex = _sprites[_sprNum];
                    break;
            }
        }
    }
}