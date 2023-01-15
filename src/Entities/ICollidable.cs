using Microsoft.Xna.Framework;
using static OpenLaMulana.Entities.World;
using System;

namespace OpenLaMulana.Entities
{
    public abstract class ICollidable
    {
        Rectangle CollisionBox { get; }

        short BBoxOriginX { get; set; }
        short BBoxOriginY { get; set; }

        public static World.ChipTypes TileGetCellAtPixel(View currRoom, float pX, float pY)
        {
            int rTX = (int)Math.Floor(pX / World.CHIP_SIZE);
            int rTY = (int)Math.Floor(pY / World.CHIP_SIZE);

            bool returnAnEmptyTile = false;
            if (rTX >= World.ROOM_WIDTH || rTX < 0) { returnAnEmptyTile = true; }
            if (rTY >= World.ROOM_HEIGHT || rTY < 0) { returnAnEmptyTile = true; }

            World.ChipTypes returningTile = World.ChipTypes.BACKGROUND;

            if (!returnAnEmptyTile)
            {
                int tID = currRoom.Chips[rTX, rTY].TileID;
                returningTile = DetermineCollidingTile(tID);
            }

            return returningTile;
        }

        public static ChipTypes TilePlaceMeeting(View currRoom, Rectangle bbox, Vector2 position, float checkingX, float checkingY, ChipTypes checkingType = ChipTypes.BACKGROUND)
        {
            int ts = World.CHIP_SIZE;
            int _x1 = (int)Math.Floor(Math.Round(bbox.Left + (checkingX - position.X)) / ts);
            int _y1 = (int)Math.Floor(Math.Ceiling(bbox.Top + (checkingY - position.Y)) / ts);
            int _x2 = (int)Math.Floor(Math.Round(bbox.Right + (checkingX - position.X)) / ts);
            int _y2 = (int)Math.Floor(Math.Ceiling(bbox.Bottom + (checkingY - position.Y)) / ts);

            for (int _x = _x1; _x <= _x2; _x++)
            {
                for (int _y = _y1; _y <= _y2; _y++)
                {
                    if (_x >= World.ROOM_WIDTH || _y >= World.ROOM_HEIGHT || _x < 0 || _y < 0)
                        continue;

                    int tID = currRoom.Chips[_x, _y].TileID;
                    World.ChipTypes returningTile = DetermineCollidingTile(tID);

                    if (checkingType != ChipTypes.BACKGROUND)
                    {
                        if (returningTile == checkingType)
                            return returningTile;
                    } else {
                        if (returningTile != ChipTypes.BACKGROUND)
                        {
                            return returningTile;
                        }
                    }
                }
            }

            return ChipTypes.BACKGROUND;
        }

        private static ChipTypes DetermineCollidingTile(int tID)
        {
            World.ChipTypes returningTile = World.ChipTypes.BACKGROUND;
            Field currField = Global.World.GetCurrField();
            int[] chipLines = currField.GetChipline();
            int chipLine1 = chipLines[0];
            int chipLine2 = chipLines[1];
            int specialChipID = chipLines[2];
            if (tID >= chipLine2)
            {
                int relativeTileID = tID - World.SPECIAL_TILES_BEGIN;
                if (relativeTileID >= 0)
                {
                    // This tile is at 1120 or higher (the last 2 rows of the tilemap)
                    int hitValue = currField.GetHitValue(relativeTileID);
                    returningTile = (World.ChipTypes)hitValue;
                }
                else
                {
                    // This tile is between Chip Line 1 and the special tiles that start at 1120)
                    returningTile = (World.ChipTypes)specialChipID;
                }
            }
            else if (tID >= chipLine1)
            {
                returningTile = World.ChipTypes.SOLID;
            }

            return returningTile;
        }
    }
}
