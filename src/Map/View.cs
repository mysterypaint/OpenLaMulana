using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Entities.World;

namespace OpenLaMulana
{
    public class View
    {
        private int[] _destWorld = { 0, 0, 0, 0 };   // World may never go below 0
        private int[] _destField = { 0, 0, 0, 0 };   // Field may never go below 0
        private int[] _destX = { 0, 0, 0, 0 };       // If X is negative, forbid moving toward this direction
        private int[] _destY = { 0, 0, 0, 0 };       // Y may never go below 0
        private List<ObjectSpawnData> _viewSpawnData;
        private Field _parentField = null;

        public Chip[,] Chips { get; set; }
        public int RoomNumber { get; set; } = 0; // Unsure what this does... Maybe has to do with sharing room numbers -> how enemies persist/de-spawn?
        public bool CanSpawnEntities { get; internal set; } = true;
        public int X { get; internal set; } = 0;
        public int Y { get; internal set; } = 0;

        public View(int roomWidth, int roomHeight, Field parentField, int x, int y)
        {
            Chips = new Chip[roomWidth, roomHeight];
            _viewSpawnData = new List<ObjectSpawnData>();
            _parentField = parentField;
            X = x;
            Y = y;
            /*
            for (var y = 0; y < roomHeight; y++)
            {
                for (var x = 0; x < roomWidth; x++)
                {
                    Tiles[x, y] = -1;
                }
            }
            */
        }

        internal void DefineViewDestination(VIEW_DIR direction, int destWorld, int destField, int destX, int destY)
        {
            _destWorld[(int)direction] = destWorld;
            _destField[(int)direction] = destField;
            _destX[(int)direction] = destX;
            _destY[(int)direction] = destY;
        }

        internal int[] GetDestinationView(VIEW_DIR direction)
        {
            int[] destination = { _destWorld[(int)direction], _destField[(int)direction], _destX[(int)direction], _destY[(int)direction] };
            return destination;
        }

        internal void AddSpawnData(ObjectSpawnData obj)
        {
            _viewSpawnData.Add(obj);
        }

        internal void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        internal void Draw(Texture2D texture, SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Loop through every single Room[_x][_y] Chip to draw every single Chip in a given room
            for (int y = 0; y < World.ROOM_HEIGHT; y++)
            {
                for (int x = 0; x < World.ROOM_WIDTH; x++)
                {
                    // Grab the current Chip (tile) we're looking at
                    Chip thisChip = Chips[x, y];

                    // Animate the Chip, if applicable
                    if (!thisChip.isAnime)
                    {
                        // This Chip isn't animated, so we'll just draw it regularly
                        var posX = (x * 8);
                        var posY = (y * 8);
                        var texX = (thisChip.tileID % 40) * 8;
                        var texY = (thisChip.tileID / 40) * 8;

                        spriteBatch.Draw(texture, new Vector2(posX, Main.HUD_HEIGHT + posY), new Rectangle(texX, texY, World.CHIP_SIZE, World.CHIP_SIZE), Color.White);
                    }
                    else
                    {
                        // Handle animated Chips here
                        var animeSpeed = thisChip.animeSpeed;
                        var animeFrames = thisChip.GetAnimeFrames();
                        var maxFrames = animeFrames.Length;

                        if (animeSpeed > 0)
                        {
                            if (gameTime.TotalGameTime.Ticks % (animeSpeed * 6) == 0)
                            {
                                thisChip.currFrame++;

                                if (thisChip.currFrame >= maxFrames)
                                    thisChip.currFrame = 0;
                            }
                        }

                        var drawingTileID = animeFrames[(int)thisChip.currFrame];

                        //spriteBatch.Draw(_fieldTextures[currField], new Vector2(0, 0), new Rectangle(16, 16, tileWidth, tileHeight), Color.White);
                        var posX = (x * 8);
                        var posY = (y * 8);
                        var texX = (drawingTileID % 40) * 8;
                        var texY = (drawingTileID / 40) * 8;

                        spriteBatch.Draw(texture, new Vector2(posX, Main.HUD_HEIGHT + posY), new Rectangle(texX, texY, World.CHIP_SIZE, World.CHIP_SIZE), Color.White);
                    }
                }
            }
        }

        internal List<ObjectSpawnData> GetSpawnData()
        {
            return _viewSpawnData;
        }

        internal Field GetParentField()
        {
            return _parentField;
        }
    }
}