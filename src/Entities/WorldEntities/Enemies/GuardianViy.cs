using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GuardianViy : IRoomWorldEntity
    {
        private int spritesMax = 23;
        Sprite[] sprites = new Sprite[23];
        private int _sprNum = 0;
        private View _bossRoom = null;

        public GuardianViy(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.SpriteDefManager.GetTexture(Global.SpriteDefs.BOSS04);
            for (var i = 0; i < spritesMax; i++)
            {
                sprites[i] = Global.SpriteDefManager.GetSprite(Global.SpriteDefs.BOSS04, i);
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


        private void ShiftScreenDown()
        {
            // Grab the bottom-most column of the boss arena
            Chip[] bottomMostRow = new Chip[World.ROOM_WIDTH];
            for (int x = 0; x < World.ROOM_WIDTH; x++)
            {
                bottomMostRow[x] = _bossRoom.Chips[x, World.ROOM_HEIGHT - 1];
            }

            // Shift every single tile in the room toward the top; The bottom-most column will be written at the top of the room, effectively wrapping the screen
            _bossRoom.ShiftTiles(World.VIEW_DIR.DOWN, bottomMostRow);
        }
    }
}