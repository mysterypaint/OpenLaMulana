using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using System.IO;

namespace OpenLaMulana
{
    public class Field : IGameEntity
    {
        /*
         * Field 0: The BGM is forced to number 39 only when invading for the first time.
Field 1: When flag number 680 is turned on (all guardians are defeated), BGM will be forced to number 17.
Fields 9 and 10: Treated as one map on the F4 map screen.
Fields 10 and above are programmatically treated as back fields. Enemy object bats automatically become back bats when placed behind them.
Field 8: In the game, after defeating all Guardians, Field 8 will switch to the Shrine of Our Lady, but this is an event that switches the gate leading to the Holy Mother's Hall. When flag 680 is turned on, you will not be able to warp to the Shrine of Our Lady with the Holy Grail.
Fields 31,32: Used for mini games. Field 31 will be the PR3 map. Field 32 is for Mukimuki Memorial and has three scenes.

Some Guardians are forced to relocate after the battle ends. See Guardian commentary. (/_RESOURCE/02-04.html)*/

        public int DrawOrder => throw new NotImplementedException();

        public int WorldID { get; internal set; } = 0;

        public int MusicNumber = 0;
        public int MapIndex;
        public int MapGraphics;
        public int EventGraphics;
        public int ID = 0;

        private int _mapData = 0;
        private Dictionary<int, int> _hitList;   // Starting from ChipLine2 counting upward, this array defines the behavior of each tile in progressive order.
        private List<int[]> _animeList;
        private List<ObjectSpawnData> _fieldSpawnData;
        private View[,] _views = new View[World.FIELD_WIDTH, World.FIELD_HEIGHT];
        private List<View> _visitedViews = new List<View>();
        private int[] _chipline = { 0, 0, -1};
        private EntityManager _s_entityManager;
        private TextManager _textManager;
        private static List<IGameEntity> _fieldEntities = new List<IGameEntity>();
        private static List<IGameEntity> _roomEntities = new List<IGameEntity>();
        private World _world;
        
        public Field(int mapIndex, int mapData, int eventGraphics, int musicNumber, EntityManager s_entityManager, TextManager textManager, World world, int mapGraphics = 65535, int id = 0, int worldID = 0)
        {
            MapIndex = mapIndex;
            _mapData = mapData;
            this.MapGraphics = mapGraphics;
            EventGraphics = eventGraphics;
            MusicNumber = musicNumber;
            _s_entityManager = s_entityManager;
            _textManager = textManager;
            _world = world;
            ID = id;
            WorldID = worldID;

            for (var y = 0; y < World.FIELD_HEIGHT; y++)
            {
                for (var x = 0; x < World.FIELD_WIDTH; x++)
                {
                    this._views[x, y] = new View(World.ROOM_WIDTH, World.ROOM_HEIGHT, this, x, y);
                }
            }

            _hitList = new Dictionary<int, int>();
            _animeList = new List<int[]>();
            _fieldSpawnData = new List<ObjectSpawnData>();
        }

        public void InitializeArea()
        {
            string mapName = "map";
            if (MapIndex <= 9)
                mapName += "0";
            mapName += MapIndex.ToString();

            string fileName = "Content/data/" + mapName + ".dat";

            if (File.Exists(fileName))
            {
                using (BinaryReader reader = new BinaryReader(File.Open(fileName, FileMode.Open)))
                {
                    int i = 0;
                    var roomX = 0;      // The Room X coordinate we're writing to
                    var roomY = 0;      // The Room Y coordinate we're writing to

                    while (reader.BaseStream.Position != reader.BaseStream.Length)
                    {
                        Int16 tileID = reader.ReadInt16();

                        if (i % World.ROOM_WIDTH == 0 && i > 0)
                        { // Besides the first tile, for every <World.RoomWidth> tiles, increment roomX (make sure it never goes beyond the bounds of the map)
                            roomX++;
                        }

                        if (roomX > World.FIELD_WIDTH - 1)
                        {
                            roomX = 0;

                            if (i % (World.ROOM_WIDTH * World.ROOM_HEIGHT * World.FIELD_WIDTH) == 0)
                                roomY++;
                        }

                        int rtx = i % World.ROOM_WIDTH; // Relative Room Tile X
                        int rty = (i / (World.ROOM_WIDTH * World.FIELD_WIDTH)) % World.ROOM_HEIGHT; // Relative Room Tile Y

                        int[] animatedTileInfo = null;

                        foreach (int[] j in _animeList)
                        {
                            int aniIndex = j[0];
                            if (aniIndex + World.ANIME_TILES_BEGIN == tileID)
                                animatedTileInfo = j;
                        }

                        _views[roomX, roomY].Chips[rtx, rty] = new Chip(tileID, animatedTileInfo);

                        i++;
                    }
                }
            }
        }

        internal int[] GetChipline()
        {
            return _chipline;
        }

        public View[,] GetMapData()
        {
            return _views;
        }

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        internal void SetChipline(int chiplineOne, int chiplineTwo)
        {
            _chipline[0] = chiplineOne;
            _chipline[1] = chiplineTwo;
        }

        internal void AddHit(int index, int value)
        {
            if (index < 0)
                _chipline[2] = value;
            else
                _hitList[index] = value;
        }

        internal void DefineAnimatedTile(int[] animationInfo)
        {
            _animeList.Add(animationInfo);
        }

        internal ObjectSpawnData DefineObjectSpawnData(int eventNumber, int tileX, int tileY, int OP1, int OP2, int OP3, int OP4, View currView)
        {
            ObjectSpawnData newObjData = new ObjectSpawnData(eventNumber, tileX / 2048, tileY / 2048, OP1, OP2, OP3, OP4);

            if (currView != null) {
                newObjData.CoordsAreRelativeToView(World.ROOM_WIDTH, World.ROOM_HEIGHT);
                currView.AddSpawnData(newObjData);
            } else {
                _fieldSpawnData.Add(newObjData);
            }

            return newObjData;
        }

        internal void DrawRoom(Texture2D texture, int currRoomX, int currRoomY, SpriteBatch spriteBatch, GameTime gameTime)
        {
            var thisRoom = _views[currRoomX, currRoomY];
            
            thisRoom.Draw(texture, spriteBatch, gameTime);
        }

        internal View GetView(int destViewX, int destViewY)
        {
            return _views[destViewX, destViewY];
        }

        internal void DeleteOldRoomEntities(View prevView, View destView)
        {
            if (prevView.RoomNumber == destView.RoomNumber) {
                if (prevView.GetParentField() == destView.GetParentField())
                {
                    return; // Don't delete any room entities, because we are not only in the same Field, but also in the same room/region of Views
                }
            }
            else {
                // Otherwise, we should delete all room entities from the previous View's Field
                var prevViewRoomEntities = prevView.GetParentField().GetRoomEntities();
                foreach (IGameEntity rE in prevViewRoomEntities) {
                    _s_entityManager.RemoveEntity(rE);
                }
                prevView.GetParentField().GetRoomEntities().Clear();
            }
        }

        internal void DeleteAllFieldAndRoomEntities()
        {
            foreach (IGameEntity fE in _fieldEntities)
            {
                _s_entityManager.RemoveEntity(fE);
            }
            foreach (IGameEntity rE in _roomEntities)
            {
                _s_entityManager.RemoveEntity(rE);
            }
            _roomEntities.Clear();
            _fieldEntities.Clear();
        }

        /*
         * 
        private EntityManager _s_entityManager;
        private static List<IGameEntity> _fieldEntities = new List<IGameEntity>();
        private static List<IGameEntity> _roomEntities = new List<IGameEntity>();*/

        internal void SpawnEntities(View destView, Field destField, View prevView, Field prevField)
        {
            // Abort if the destination View is not allowed to spawn entities (true on initialization)
            if (!destView.CanSpawnEntities)
                return;

            // Check 
            foreach (View v in prevField._visitedViews)
            {
                if (v.RoomNumber != destView.RoomNumber) {
                    // We are entering a new region, so we should forget about all the previous rooms we've visited
                    DeleteOldRoomEntities(v, destView);
                    v.GetParentField().UnlockAllViewSpawning();
                    v.GetParentField()._visitedViews.Clear();
                    break;
                }
            }

            // Finally, check if we've already visited this View since entering this region; Abort if we did
            foreach (View v in _visitedViews)
            {
                if (v == destView)
                    return;
            }

            List<ObjectSpawnData> viewSpawns = destView.GetSpawnData();

            foreach (ObjectSpawnData viewObjData in viewSpawns)
            {
                IGameEntity newViewObj = SpawnEntity(viewObjData, destView);

                if (newViewObj != null)
                    _roomEntities.Add(newViewObj);
            }

            // We are moving to a new field, so we should spawn all the destination Field's global entities right now
            if (destField.MapIndex != prevField.MapIndex)
            {
                foreach (ObjectSpawnData fieldObj in destField._fieldSpawnData) {
                    IGameEntity newObj = SpawnEntity(fieldObj, destView);

                    if (newObj != null)
                    {
                        _fieldEntities.Add(newObj);
                    }
                }

                Field pf = destView.GetParentField();
                foreach (View view in pf._views) {
                    view.CanSpawnEntities = true;
                }
            }
            destView.CanSpawnEntities = false;
            _visitedViews.Add(destView); // Keep track of all the views we've visited since entering this region/room
        }

        private IGameEntity SpawnEntity(ObjectSpawnData newObjData, View destView)
        {
            IGameEntity newObj = null;

            var eventNumber = newObjData.EventNumber;
            var x = newObjData.X;
            var y = newObjData.Y;
            var op1 = newObjData.OP1;
            var op2 = newObjData.OP2;
            var op3 = newObjData.OP3;
            var op4 = newObjData.OP4;
            var startFlags = newObjData.StartFlags;
            var spawnIsGlobal = newObjData.SpawnIsGlobal;

            switch (eventNumber) {
                default:
                case 0:
                    // TODO: Don't forget to check startFlags here before spawning anything!
                    newObj = new IWorldEntity(x, y, op1, op2, op3, op4, spawnIsGlobal, World._genericEntityTex, _world, destView);
                    break;
            }

            if (newObj != null)
                _s_entityManager.AddEntity(newObj);

            return newObj;
        }

        internal List<IGameEntity> GetFieldEntities()
        {
            return _fieldEntities;
        }

        internal List<IGameEntity> GetRoomEntities()
        {
            return _roomEntities;
        }

        internal void UnlockAllViewSpawning()
        {
            foreach (View v in _views)
            {
                v.CanSpawnEntities = true;
            }
        }
    }
}