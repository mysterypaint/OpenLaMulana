using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GuardianEllmac : IRoomWorldEntity
    {
        private int spritesMax = 7;
        Sprite[] sprites = new Sprite[7];
        private int _sprNum = 0;
        private double timeBeforeDrop = 0.0f;
        private Global.EnemyStates _state = Global.EnemyStates.INIT;

        public GuardianEllmac(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.SpriteDefManager.GetTexture(Global.SpriteDefs.BOSS01);
            for (var i = 0; i < spritesMax; i++)
            {
                sprites[i] = Global.SpriteDefManager.GetSprite(Global.SpriteDefs.BOSS01, i);
            }
            _sprIndex = sprites[_sprNum];
            Position += new Vector2(200, -3);
            Visible = false;

            timeBeforeDrop = 0.8f;
            _state = Global.EnemyStates.INIT;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (!Visible)
                return;


            if (viewCoords.X == _world.CurrViewX && viewCoords.Y == _world.CurrViewY)
            {
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (_state)
            {
                default:
                    break;
                case Global.EnemyStates.INIT:
                    if (Global.Camera.GetState() == System.Camera.CamStates.NONE)
                    {
                        float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

                        if (timeBeforeDrop > 0.0f)
                        {
                            timeBeforeDrop -= 1.0f * dt;
                        } else {
                            timeBeforeDrop = 0.0f;
                            View srcView = Global.World.GetField(3).GetBossViews()[0];//new View(World.ROOM_WIDTH, World.ROOM_HEIGHT, Global.World.GetField(3), 0, 0);
                            View destView = new View(World.ROOM_WIDTH, World.ROOM_HEIGHT, null, 0, 0);
                            destView.InitChipData(0, null);

                            Global.World.FieldTransitionCardinalBoss(World.VIEW_DIR.DOWN, srcView, destView, Global.TextureManager.GetTexture(Global.Textures.BOSS02));

                            _state = Global.EnemyStates.ACTIVATING;
                        }
                    }
                    break;
                case Global.EnemyStates.ACTIVATING:
                    if (Global.Camera.GetState() == System.Camera.CamStates.NONE)
                    {
                        _state = Global.EnemyStates.SPEEDING_UP;
                    }
                    break;
                case Global.EnemyStates.SPEEDING_UP:
                    break;
            }

            /*
            int animeSpeed = 6;
            if (gameTime.TotalGameTime.Ticks % (animeSpeed * 6) == 0)
            {
                //_sprNum++;
                if (_sprNum >= spritesMax)
                    _sprNum = 0;
            }*/
            _sprIndex = sprites[_sprNum];
        }
    }
}