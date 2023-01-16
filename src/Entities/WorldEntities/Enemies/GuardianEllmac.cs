using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GuardianEllmac : IGlobalWorldEntity
    {
        private int spritesMax = 7;
        Sprite[] sprites = new Sprite[7];
        private int _sprNum = 0;
        private double timeBeforeDrop = 0.0f;
        private Global.EnemyStates _state = Global.EnemyStates.INIT;
        private View _bossRoom = null;
        private int speedUpTimer = 30;

        public GuardianEllmac(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView)
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

            _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
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

                            for (int y = 0; y < 4; y++)
                            {
                                for (int x = 0; x < World.ROOM_WIDTH; x++)
                                {
                                    destView.Chips[x, y].TileID = 36 + (x % 4) + (y * 40);
                                }
                            }
                            for (int x = 0; x < World.ROOM_WIDTH; x++)
                            {
                                destView.Chips[x, World.ROOM_HEIGHT - 1].TileID = 26 + (x % 2);
                            }

                            _bossRoom = destView;

                            Global.World.FieldTransitionCardinalBoss(World.VIEW_DIR.DOWN, srcView, destView, Global.TextureManager.GetTexture(Global.Textures.BOSS02), this);

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
                    if (Global.AnimationTimer.OneFrameElapsed())
                    {
                        if (speedUpTimer > 0)
                            speedUpTimer--;
                    }

                    if (speedUpTimer <= 0 || speedUpTimer == 2 || speedUpTimer == 5 || speedUpTimer == 7 || speedUpTimer == 9 || speedUpTimer == 15 || speedUpTimer == 23)
                        ShiftScreenLeft();
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

        private void ShiftScreenLeft()
        {
            // Grab the left-most column of the boss arena
            Chip[] leftMostColumn = new Chip[World.ROOM_HEIGHT];
            for (int y = 0; y < World.ROOM_HEIGHT; y++)
            {
                leftMostColumn[y] = _bossRoom.Chips[0, y];
            }

            // Shift every single tile in the room toward the left; The left-most column will be written on the far right of the room, effectively wrapping the screen
            _bossRoom.ShiftTiles(World.VIEW_DIR.LEFT, leftMostColumn);
        }
    }
}