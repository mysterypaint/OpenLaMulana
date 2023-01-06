﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities;
using OpenLaMulana.Entities.WorldEntities.Enemies;
using OpenLaMulana.Entities.WorldEntities.Parents;
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

        public int DrawOrder { get; set; } = 0;
        public Effect ActiveShader { get; set; } = null;
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
        private View[,] _viewsJPN = new View[World.FIELD_WIDTH, World.FIELD_HEIGHT];
        private View[,] _viewsENG = null;
        private List<View> _visitedViews = new List<View>();
        private int[] _chipline = { 0, 0, -1 };
        private EntityManager _s_entityManager;
        private TextManager _textManager;
        private static List<IGameEntity> _fieldEntities = new List<IGameEntity>();
        private static List<IGameEntity> _roomEntities = new List<IGameEntity>();
        private World _world;
        View[] _bossViews = null;
        private int _bossID = -1;

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


            _viewsJPN = new View[World.FIELD_WIDTH, World.FIELD_HEIGHT];
            for (var y = 0; y < World.FIELD_HEIGHT; y++)
            {
                for (var x = 0; x < World.FIELD_WIDTH; x++)
                {
                    this._viewsJPN[x, y] = new View(World.ROOM_WIDTH, World.ROOM_HEIGHT, this, x, y);
                }
            }

            switch (ID)
            {
                default:
                    break;
                case 21:
                    _viewsENG = new View[World.FIELD_WIDTH, World.FIELD_HEIGHT];

                    for (var y = 0; y < World.FIELD_HEIGHT; y++)
                    {
                        for (var x = 0; x < World.FIELD_WIDTH; x++)
                        {
                            this._viewsENG[x, y] = new View(World.ROOM_WIDTH, World.ROOM_HEIGHT, this, x, y);
                        }
                    }
                    break;
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
            LoadMap(fileName, Global.Languages.Japanese);

            // Load the English maps (There's only one, but just in case we need to add any more in the future...)
            switch(ID) {
                default:
                    break;
                case 21:
                    mapName += "_EN";
                    fileName = "Content/data/" + mapName + ".dat";
                    LoadMap(fileName, Global.Languages.English);
                    break;
            }
        }

        private void LoadMap(string fileName, Global.Languages lang, bool isBossMap = false)
        {
            if (File.Exists(fileName))
            {
                View[,] bossViewsTemp = null;

                if (isBossMap)
                {
                    bossViewsTemp = new View[World.FIELD_WIDTH, World.FIELD_HEIGHT];

                    for (var y = 0; y < World.FIELD_HEIGHT; y++)
                    {
                        for (var x = 0; x < World.FIELD_WIDTH; x++)
                        {
                            bossViewsTemp[x, y] = new View(World.ROOM_WIDTH, World.ROOM_HEIGHT, this, x, y);
                        }
                    }
                }

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

                        if (!isBossMap)
                        {
                            switch (lang)
                            {
                                default:
                                case Global.Languages.Japanese:
                                    _viewsJPN[roomX, roomY].Chips[rtx, rty] = new Chip(tileID, animatedTileInfo);
                                    break;
                                case Global.Languages.English:
                                    _viewsENG[roomX, roomY].Chips[rtx, rty] = new Chip(tileID, animatedTileInfo);
                                    break;
                            }
                        }
                        else
                        {
                            bossViewsTemp[roomX, roomY].Chips[rtx, rty] = new Chip(tileID, animatedTileInfo);
                        }

                        i++;
                    }
                }

                if (isBossMap)
                {
                    // Only remember the first 2 rooms of the map
                    _bossViews[0] = bossViewsTemp[0, 0];
                    _bossViews[1] = bossViewsTemp[1, 0];
                    _bossViews[2] = bossViewsTemp[2, 0];
                }
            }
        }

        internal int[] GetChipline()
        {
            return _chipline;
        }

        public View[,] GetMapData()
        {
            switch (Global.CurrLang)
            {
                default:
                case Global.Languages.Japanese:
                    return _viewsJPN;
                case Global.Languages.English:
                    if (_viewsENG != null)
                        return _viewsENG;
                    else
                        return _viewsJPN;
            }
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

            if (currView != null)
            {
                newObjData.CoordsAreRelativeToView(World.ROOM_WIDTH, World.ROOM_HEIGHT);
                currView.AddSpawnData(newObjData);
            }
            else
            {
                _fieldSpawnData.Add(newObjData);
            }

            return newObjData;
        }

        internal void DrawRoom(Texture2D texture, int currRoomX, int currRoomY, SpriteBatch spriteBatch, GameTime gameTime, Vector2 offsetVec)
        {
            var thisRoom = _viewsJPN[currRoomX, currRoomY];

            thisRoom.Draw(texture, spriteBatch, gameTime, offsetVec);
        }

        internal View GetView(int destViewX, int destViewY)
        {
            return _viewsJPN[destViewX, destViewY];
        }

        internal void DeleteOldRoomEntities(View prevView, View destView)
        {
            if (prevView.RoomNumber == destView.RoomNumber)
            {
                if (prevView.GetParentField() == destView.GetParentField())
                {
                    return; // Don't delete any room entities, because we are not only in the same Field, but also in the same room/region of Views
                }
            }
            else
            {
                // Otherwise, we should delete all room entities from the previous View's Field
                var prevViewRoomEntities = prevView.GetParentField().GetRoomEntities();
                foreach (IGameEntity rE in prevViewRoomEntities)
                {
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

        internal void SpawnEntities(View destView, Field destField, View prevView, Field prevField, Vector2 offsetVector)
        {
            // Abort if the destination View is not allowed to spawn entities (true on initialization)
            if (!destView.CanSpawnEntities)
                return;

            // Check 
            foreach (View v in prevField._visitedViews)
            {
                if (v.RoomNumber != destView.RoomNumber)
                {
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
                IGameEntity newViewObj = SpawnEntityFromData(viewObjData, destView, offsetVector, false);

                if (newViewObj != null)
                    _roomEntities.Add(newViewObj);
            }

            // We are moving to a new field, so we should spawn all the destination Field's global entities right now
            if (destField.MapIndex != prevField.MapIndex)
            {
                foreach (ObjectSpawnData fieldObj in destField._fieldSpawnData)
                {
                    IGameEntity newObj = SpawnEntityFromData(fieldObj, destView, Vector2.Zero, true);

                    if (newObj != null)
                    {
                        _fieldEntities.Add(newObj);
                    }
                }

                Field pf = destView.GetParentField();
                foreach (View view in pf._viewsJPN)
                {
                    view.CanSpawnEntities = true;
                }
            }
            destView.CanSpawnEntities = false;
            _visitedViews.Add(destView); // Keep track of all the views we've visited since entering this region/room
        }

        private IGameEntity SpawnEntityFromData(ObjectSpawnData newObjData, View destView, Vector2 offsetVector, bool spawnIsGlobal)
        {
            IGameEntity newObj = null;

            var eventNumber = newObjData.EventNumber;
            var x = newObjData.X + (int)Math.Sign(offsetVector.X) * World.ROOM_WIDTH;
            var y = newObjData.Y + (int)Math.Sign(offsetVector.Y) * World.ROOM_HEIGHT;
            var op1 = newObjData.OP1;
            var op2 = newObjData.OP2;
            var op3 = newObjData.OP3;
            var op4 = newObjData.OP4;
            var startFlags = newObjData.StartFlags;
            //var spawnIsGlobal = newObjData.SpawnIsGlobal; // No longer needed! Keeping it here just in case, though...

            switch ((EntityIDs)eventNumber)
            {
                default:
                    // TODO: Don't forget to check startFlags here before spawning anything!
                    if (spawnIsGlobal)
                        newObj = new GenericGlobalWorldEntity(x, y, op1, op2, op3, op4, destView);
                    else
                        newObj = new GenericRoomWorldEntity(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.ENEMY_SKELETON:
                    newObj = new EnemySkeleton(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.ENEMY_BAT:
                    newObj = new EnemyBat(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.TREASURE_CHEST:
                    newObj = new TreasureChest(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.BREAKABLE_POT:
                    newObj = new BreakablePot(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.MOVING_PLATFORM:
                    newObj = new MovingPlatform(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.PUSHABLE_BLOCK:
                    newObj = new PushableBlock(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.OBTAINABLE_SUBWEAPON:
                    newObj = new ObtainableSubweapon(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.CEILING_SPIKE:
                    newObj = new CeilingSpike(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.ENEMY_MYRMECOLEON:
                    newObj = new EnemyMyrmecoleon(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.ENEMY_TOG_GENERATOR:
                    newObj = new EnemyTogGenerator(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.ONE_WAY_DOOR:
                    newObj = new OneWayDoor(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.FLOOR_SWITCH:
                    newObj = new FloorSwitch(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.ENEMY_SNOUT_LEAPER:
                    newObj = new EnemySnoutLeaper(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.RUINS_TABLET:
                    newObj = new RuinsTablet(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.DAIS:
                    newObj = new Dais(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.BLOCK_FLOOR_SWITCH:
                    newObj = new BlockFloorSwitch(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.BACKGROUND_SIGIL:
                    newObj = new BackgroundSigil(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.BIG_ANKH:
                    if (op1 != 8072 || op2 != 16008 || op3 != 361)
                        break;
                    newObj = new Ankh(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.AKNH:
                    newObj = new Ankh(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.OBTAINABLE_MAJOR_WEAPON:
                    newObj = new ObtainableMajorWeapon(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.ENEMY_SOUL:
                    newObj = new EnemySoul(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.FIELD_TRANSITION:
                    newObj = new FieldTransition(x, y, op1, op2, op3, op4, destView);
                    break;
                case EntityIDs.ENEMY_A_BAO_A_QU:
                    newObj = new EnemyABaoAQu(x, y, op1, op2, op3, op4, destView);
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
            foreach (View v in _viewsJPN)
            {
                v.CanSpawnEntities = true;
            }
        }

        internal void ClearVisitedViews()
        {
            _visitedViews.Clear();
        }

        internal bool HasEnglishField()
        {
            return (_viewsENG != null);
        }

        internal void InitializeBossRoom()
        {
            if ((ID >= 0 && ID <= 9 && ID != 1 && ID != 7 && ID != 8) || ID == 17 || ID == 19)    // 19 is True Shrine
            {
                _bossViews = new View[3];
                switch (ID)
                {
                    default:
                        break;
                    case 0:
                        _bossID = (int)Global.BossIDs.AMPHISBAENA;
                        break;
                    case 2:
                        _bossID = (int)Global.BossIDs.SAKIT;
                        break;
                    case 3:
                        _bossID = (int)Global.BossIDs.ELLMAC;
                        break;
                    case 4:
                        _bossID = (int)Global.BossIDs.BAHAMUT;
                        break;
                    case 5:
                        _bossID = (int)Global.BossIDs.VIY;
                        break;
                    case 6:
                        _bossID = (int)Global.BossIDs.PALENQUE;
                        break;
                    case 9:
                        _bossID = (int)Global.BossIDs.BAPHOMET;
                        break;
                    case 17:
                        _bossID = (int)Global.BossIDs.TIAMAT;
                        break;
                    case 19:
                        _bossID = (int)Global.BossIDs.MOTHER;
                        break;
                }
            }

            string mapName = "boss";
            if (_bossID <= 9)
                mapName += "0";
            mapName += _bossID.ToString();

            string fileName = "Content/data/" + mapName + ".dat";
            LoadMap(fileName, Global.Languages.Japanese, true);
        }

        internal View[] GetBossViews()
        {
            if (_bossViews == null) {
                // Failsafe
                View[] views = new View[3];
                views[0] = _viewsJPN[0, 0];
                views[1] = _viewsJPN[1, 0];
                views[2] = _viewsJPN[2, 0];
                return views;
            }
            return _bossViews;
        }

        internal void SetBossView(int id, View v)
        {
            _bossViews[id] = v;
        }

        internal int GetBossID()
        {
            return _bossID;
        }
    }
}