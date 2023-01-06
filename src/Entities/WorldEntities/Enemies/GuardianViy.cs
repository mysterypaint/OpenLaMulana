using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;
using static OpenLaMulana.Entities.World;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GuardianViy : IGlobalWorldEntity
    {
        private int spritesMax = 23;
        Sprite[] sprites = new Sprite[23];
        private int _sprNum = 0;
        private View _bossRoom = null;
        private View _bossRoomUnmodified = null;
        private View _currActiveView = null;
        private int _moveTimerMax = 10;
        private int _moveTimer = 0;
        private int _shiftYSides = World.ROOM_HEIGHT - 1;
        private int _shiftYMiddle = World.ROOM_HEIGHT;
        private Protag _protag = null;

        public GuardianViy(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.SpriteDefManager.GetTexture(Global.SpriteDefs.BOSS04);
            for (var i = 0; i < spritesMax; i++)
            {
                sprites[i] = Global.SpriteDefManager.GetSprite(Global.SpriteDefs.BOSS04, i);
            }
            _sprIndex = sprites[_sprNum];

            _bossRoom = Global.World.GetField(Global.World.CurrField).GetBossViews()[1];
            _bossRoomUnmodified = _bossRoom.CloneView();
            _currActiveView = Global.World.GetCurrentView();
            _moveTimer = _moveTimerMax;
            for (int ty = 0; ty <= 4; ty++)
            {
                for (int tx = 7; tx <= 8; tx++)
                {
                    _bossRoom.Chips[tx, ty].CloneTile(_bossRoom.Chips[6, 0]);
                }
            }

            _protag = Global.World.GetProtag();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (viewCoords.X == _world.CurrViewX && viewCoords.Y == _world.CurrViewY)
            {
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
            }
        }

        public override void Update(GameTime gameTime)
        {
            int animeSpeed = 6;
            if (gameTime.TotalGameTime.Ticks % (animeSpeed * 6) == 0)
            {
                _sprNum++;
                if (_sprNum >= spritesMax)
                    _sprNum = 0;
            }
            _sprIndex = sprites[_sprNum];

            if (Global.Camera.GetState() == System.Camera.CamStates.NONE) {
                if (Global.AnimationTimer.OneFrameElapsed())
                {
                    if (_moveTimer <= 0)
                    {
                        ShiftScreenDown();
                        _moveTimer = _moveTimerMax;
                    }
                    else
                        _moveTimer--;
                }
            }
        }


        private void ShiftScreenDown()
        {
            // Grab the bottom-most column of the boss arena
            Chip[] bottomMostRow = new Chip[World.ROOM_WIDTH];
            for (int x = 1; x < World.ROOM_WIDTH - 1; x++)
            {
                if (_shiftYMiddle < World.ROOM_HEIGHT)
                    bottomMostRow[x] = _bossRoomUnmodified.Chips[x, _shiftYMiddle];
                else // Grab the top-most row if we just finished looping through the whole room
                    bottomMostRow[x] = _bossRoomUnmodified.Chips[x, 0];


            }
            bottomMostRow[0] = _bossRoomUnmodified.Chips[0, _shiftYSides];
            bottomMostRow[World.ROOM_WIDTH - 1] = _bossRoomUnmodified.Chips[World.ROOM_WIDTH - 1, _shiftYSides];

            // Shift every single tile in the room toward the top; The bottom-most column will be written at the top of the room, effectively wrapping the screen
            _bossRoom.ShiftTiles(World.VIEW_DIR.DOWN, bottomMostRow);
            
            // Update the current room's collision tiles to reflect the updated visual tiles
            _currActiveView.Chips = _bossRoom.Chips;

            _shiftYSides--;
            _shiftYMiddle--;
            if (_shiftYSides < 0)
                _shiftYSides = World.ROOM_HEIGHT - 1;
            if (_shiftYMiddle < 0)
                _shiftYMiddle = World.ROOM_HEIGHT;

            if (_protag.IsGrounded()) {
                _protag.ApplyVector(new Vector2(0, 8));
            }
        }
    }
}