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
        private int[] _destWorld = new int[8];   // World may never go below 0
        private int[] _destField = new int[8];   // Field may never go below 0
        private int[] _destX = new int[8];       // If X is negative, forbid moving toward this direction
        private int[] _destY = new int[8];       // Y may never go below 0
        private List<ObjectSpawnData> _viewSpawnData;
        private Field _parentField = null;
        private List<IGameEntity> _myEntities = new List<IGameEntity>();

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
            int[] destination = new int[4];

            if (direction != VIEW_DIR.SELF) {
                destination[0] = _destWorld[(int)direction];
                destination[1] = _destField[(int)direction];
                destination[2] = _destX[(int)direction];
                destination[3] = _destY[(int)direction];
            }
            else {
                destination[0] = _destWorld[_parentField.WorldID];
                destination[1] = _destField[_parentField.ID];
                destination[2] = _destX[X];
                destination[3] = _destY[Y];
            }
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

        internal void Draw(Texture2D texture, SpriteBatch spriteBatch, GameTime gameTime, Vector2 offsetVec)
        {
            // Loop through every single Room[_x][_y] Chip to draw every single Chip in a given room
            for (int y = 0; y < World.ROOM_HEIGHT; y++)
            {
                for (int x = 0; x < World.ROOM_WIDTH; x++)
                {
                    // Grab the current Chip (tile) we're looking at
                    Chip thisChip = Chips[x, y];

                    // Animate the Chip, if applicable
                    if (!thisChip.IsAnime)
                    {
                        // This Chip isn't animated, so we'll just draw it regularly
                        var posX = (x * 8);
                        var posY = (y * 8);
                        var texX = (thisChip.TileID % 40) * 8;
                        var texY = (thisChip.TileID / 40) * 8;

                        spriteBatch.Draw(texture, new Vector2(posX, Main.HUD_HEIGHT + posY) + offsetVec, new Rectangle(texX, texY, World.CHIP_SIZE, World.CHIP_SIZE), Color.White);
                    }
                    else
                    {
                        // Handle animated Chips here

                        var animeSpeed = thisChip.AnimeSpeed;
                        var animeFrames = thisChip.GetAnimeFrames();//GetAnimeFramesAsRawData();

                        if (animeSpeed > 0)
                        {
                            if (Global.AnimationTimer.OneFrameElapsed(true))
                            {
                                // One in-game frame has elapsed; let the chip know here (disregard the game paused state, just like the original game)
                                thisChip.StepFrame();
                            }
                        }

                        var drawingTileID = animeFrames[(int)thisChip.CurrFrame];

                        //spriteBatch.Draw(_fieldTextures[currField], new Vector2(0, 0), new Rectangle(16, 16, tileWidth, tileHeight), Color.White);
                        var posX = (x * 8);
                        var posY = (y * 8);
                        var texX = (drawingTileID % 40) * 8;
                        var texY = (drawingTileID / 40) * 8;


                        spriteBatch.Draw(texture, new Vector2(posX, Main.HUD_HEIGHT + posY) + offsetVec, new Rectangle(texX, texY, World.CHIP_SIZE, World.CHIP_SIZE), Color.White);
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

        internal void InitChipData(int tileID, int[] animatedTileInfo = null)
        {
            for (int y = 0; y < World.ROOM_HEIGHT; y++)
            {
                for (int x = 0; x < World.ROOM_WIDTH; x++)
                {
                    Chips[x, y] = new Chip((short)tileID, animatedTileInfo);
                }
            }
        }

        internal void ShiftTiles(VIEW_DIR dir, Chip[] newTiles)
        {
            switch (dir)
            {
                default:
                case VIEW_DIR.LEFT:
                    for (int y = 0; y < World.ROOM_HEIGHT; y++)
                    {
                        for (int x = 1; x < World.ROOM_WIDTH; x++)
                        {
                            Chips[x - 1, y] = Chips[x, y];//new Chip((short)tileID, animatedTileInfo);
                        }
                    }
                    for (int y = 0; y < World.ROOM_HEIGHT; y++)
                    {
                        Chips[World.ROOM_WIDTH - 1, y] = newTiles[y];
                    }
                    break;
                case VIEW_DIR.RIGHT:
                    for (int y = 0; y < World.ROOM_HEIGHT; y++)
                    {
                        for (int x = World.ROOM_WIDTH - 2; x >= 0; x--)
                        {
                            Chips[x + 1, y] = Chips[x, y];//new Chip((short)tileID, animatedTileInfo);
                        }
                    }
                    for (int y = 0; y < World.ROOM_HEIGHT; y++)
                    {
                        Chips[0, y] = newTiles[y];
                    }
                    break;
                case VIEW_DIR.UP:
                    for (int y = 0; y < World.ROOM_HEIGHT - 1; y++)
                    {
                        for (int x = 0; x < World.ROOM_WIDTH; x++)
                        {
                            Chips[x, y] = Chips[x, y + 1];//new Chip((short)tileID, animatedTileInfo);
                        }
                    }
                    for (int x = 0; x < World.ROOM_WIDTH; x++)
                    {
                        Chips[x, World.ROOM_HEIGHT - 1] = newTiles[x];
                    }
                    break;
                case VIEW_DIR.DOWN:
                    for (int y = World.ROOM_HEIGHT - 1; y > 0; y--)
                    {
                        for (int x = 0; x < World.ROOM_WIDTH; x++)
                        {
                            Chips[x, y] = Chips[x, y - 1];//new Chip((short)tileID, animatedTileInfo);
                        }
                    }
                    for (int x = 0; x < World.ROOM_WIDTH; x++)
                    {
                        Chips[x, 0] = newTiles[x];
                    }
                    break;
            }
        }

        internal View CloneView()
        {
            View clonedView = new View(ROOM_WIDTH, ROOM_HEIGHT, _parentField, X, Y);

            for (var y = 0; y < ROOM_HEIGHT; y++)
            {
                for (var x = 0; x < ROOM_WIDTH; x++)
                {
                    clonedView.Chips[x, y] = new Chip((short)this.Chips[x, y].TileID, this.Chips[x, y].GetAnimeFramesAsRawData());
                }
            }

            clonedView.SetParentField(this.GetParentField());
            return clonedView;
        }

        private void SetParentField(Field field)
        {
            _parentField = field;
        }

        internal void DeleteEntities()
        {
            foreach(IGameEntity entity in _myEntities)
            {
                _parentField.GetRoomEntities().Remove(entity);
                Global.EntityManager.RemoveEntity(entity);
            }
            _myEntities.Clear();
        }

        public void AddEntity(IGameEntity entity)
        {
            _myEntities.Add(entity);
            _parentField.GetRoomEntities().Add(entity);
        }

        internal void OverwriteViewDestinationTemporarily(VIEW_DIR direction, int destWorld, int destField, int destX, int destY)
        {
            // Copy the to-be-overwritten data in the mirrored slots
            _destWorld[(int)direction + 4] = _destWorld[(int)direction];
            _destField[(int)direction + 4] = _destField[(int)direction];
            _destX[(int)direction + 4] = _destX[(int)direction];
            _destY[(int)direction + 4] = _destY[(int)direction];

            // Overwrite the actual data with the new data
            _destWorld[(int)direction] = destWorld;
            _destField[(int)direction] = destField;
            _destX[(int)direction] = destX;
            _destY[(int)direction] = destY;
        }

        internal void RestoreOriginalViewDestination(VIEW_DIR direction, int destWorld, int destField, int destX, int destY)
        {

            // Move the original data back from the mirrored slots
            _destWorld[(int)direction] = _destWorld[(int)direction + 4];
            _destField[(int)direction] = _destField[(int)direction + 4];
            _destX[(int)direction] = _destX[(int)direction + 4];
            _destY[(int)direction] = _destY[(int)direction + 4];

            // Re-Initialize the mirrored data 
            _destWorld[(int)direction + 4] = 0;
            _destField[(int)direction + 4] = 0;
            _destX[(int)direction + 4] = 0;
            _destY[(int)direction + 4] = 0;
        }
    }
}