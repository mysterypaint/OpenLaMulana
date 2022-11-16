﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenLaMulana.Entities
{
    public class ObstacleManager : IGameEntity
    {

        private static readonly int[] FLYING_DINO_Y_POSITIONS = new int[] { 90, 62, 24 };

        private const float MIN_SPAWN_DISTANCE = 10;

        private const int MIN_OBSTACLE_DISTANCE = 6;
        private const int MAX_OBSTACLE_DISTANCE = 28;

        private const int OBSTACLE_DISTANCE_SPEED_TOLERANCE = 5;

        private const int LARGE_CACTUS_POS_Y = 80;
        private const int SMALL_CACTUS_POS_Y = 94;

        private const int OBSTACLE_DRAW_ORDER = 12;

        private const int OBSTACLE_DESPAWN_POS_X = -200;

        private const int FLYING_DINO_SPAWN_SCORE_MIN = 150;

        private double _lastSpawnScore = -1;
        private double _currentTargetDistance;

        private readonly EntityManager _entityManager;
        private readonly Protag _trex;

        private readonly Random _random;

        private Texture2D _spriteSheet;

        public bool IsEnabled { get; set; }

        public bool CanSpawnObstacles => IsEnabled;

        public int DrawOrder => 0;

        public void Update(GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            throw new NotImplementedException();
        }

        /*
         * 
        public ObstacleManager(EntityManager entityManager, Protag trex, Texture2D spriteSheet)
        {
            _entityManager = entityManager;
            _trex = trex;
            _random = new Random();
            _spriteSheet = spriteSheet;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

        }

        public void Update(GameTime gameTime)
        {
            if (!IsEnabled)
                return;

            if(CanSpawnObstacles &&
                (_lastSpawnScore <= 0 || (_scoreBoard.Score - _lastSpawnScore >= _currentTargetDistance)))
            {
                _currentTargetDistance = _random.NextDouble()
                    * (MAX_OBSTACLE_DISTANCE - MIN_OBSTACLE_DISTANCE) + MIN_OBSTACLE_DISTANCE;

                //_currentTargetDistance += (_trex.Speed - Protag.START_SPEED) / (Protag.MAX_SPEED - Protag.START_SPEED) * OBSTACLE_DISTANCE_SPEED_TOLERANCE;

                _lastSpawnScore = _scoreBoard.Score;

                SpawnRandomObstacle();
            }

            foreach(Obstacle obstacle in _entityManager.GetEntitiesOfType<Obstacle>())
            {
                if (obstacle.Position.X < OBSTACLE_DESPAWN_POS_X)
                    _entityManager.RemoveEntity(obstacle);

            }

        }

        private void SpawnRandomObstacle()
        {
            // TODO: Create instance of obstacle and add it to entity manager

            Obstacle obstacle = null;

            int cactusGroupSpawnRate = 75;
            int flyingDinoSpawnRate = _scoreBoard.Score >= FLYING_DINO_SPAWN_SCORE_MIN ? 25 : 0;

            int rng = _random.Next(0, cactusGroupSpawnRate + flyingDinoSpawnRate + 1);

            if(rng <= cactusGroupSpawnRate)
            {
                CactusGroup.GroupSize randomGroupSize = (CactusGroup.GroupSize)_random.Next((int)CactusGroup.GroupSize.Small, (int)CactusGroup.GroupSize.Large + 1);

                bool isLarge = _random.NextDouble() > 0.5f;

                float posY = isLarge ? LARGE_CACTUS_POS_Y : SMALL_CACTUS_POS_Y;

                obstacle = new CactusGroup(_spriteSheet, isLarge, randomGroupSize, _trex, new Vector2(OpenLaMulana.WINDOW_WIDTH, posY));

            }
            else
            {
                int verticalPosIndex = _random.Next(0, FLYING_DINO_Y_POSITIONS.Length);
                float posY = FLYING_DINO_Y_POSITIONS[verticalPosIndex];

                obstacle = new FlyingDino(_trex, new Vector2(OpenLaMulana.WINDOW_WIDTH, posY), _spriteSheet);
            }

            obstacle.DrawOrder = OBSTACLE_DRAW_ORDER;

            _entityManager.AddEntity(obstacle);

        }

        public void Reset()
        {

            foreach(Obstacle obstacle in _entityManager.GetEntitiesOfType<Obstacle>())
            {
                _entityManager.RemoveEntity(obstacle);
            }

            _currentTargetDistance = 0;
            _lastSpawnScore = -1;

        }
        */

    }
}
