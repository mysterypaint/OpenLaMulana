using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GuardianBahamut : IRoomWorldEntity
    {
        private int spritesMax = 18;
        Sprite[] sprites = new Sprite[18];
        private int _sprNum = 0;
        private View _bossRoom = null;

        public GuardianBahamut(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.SpriteDefManager.GetTexture(Global.SpriteDefs.BOSS03);
            for (var i = 0; i < spritesMax; i++)
            {
                sprites[i] = Global.SpriteDefManager.GetSprite(Global.SpriteDefs.BOSS03, i);
            }
            _sprIndex = sprites[_sprNum];
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