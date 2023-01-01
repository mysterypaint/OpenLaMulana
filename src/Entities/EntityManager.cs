using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static OpenLaMulana.Entities.World;

namespace OpenLaMulana.Entities
{
    public class EntityManager
    {
        private readonly List<IGameEntity> _entities = new List<IGameEntity>();

        private readonly List<IGameEntity> _entitiesToAdd = new List<IGameEntity>();
        private readonly List<IGameEntity> _entitiesToRemove = new List<IGameEntity>();
        
        //private Effect _shdHueShift;
        public IEnumerable<IGameEntity> Entities => new ReadOnlyCollection<IGameEntity>(_entities);

        public void Update(GameTime gameTime)
        {

            foreach (IGameEntity entity in _entities)
            {

                if (_entitiesToRemove.Contains(entity))
                    continue;

                entity.Update(gameTime);

            }

            foreach (IGameEntity entity in _entitiesToAdd)
            {
                _entities.Add(entity);
            }

            foreach (IGameEntity entity in _entitiesToRemove)
            {
                _entities.Remove(entity);
            }

            _entitiesToAdd.Clear();
            _entitiesToRemove.Clear();

        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime, GraphicsDevice graphicsDevice)
        {
            foreach (IGameEntity entity in _entities.OrderBy(e => e.DrawOrder))
            {
                switch(entity)
                {
                    default:
                        Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                            samplerState: SamplerState.PointClamp,
                            transformMatrix: Global.Camera.GetTransformation(graphicsDevice),
                            effect: entity.ActiveShader);
                        entity.Draw(spriteBatch, gameTime);
                        Global.SpriteBatch.End();
                        break;
                    case World:
                        World worldEnt = (World)entity;

                        // Draw the non-shader layers
                        Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                            samplerState: SamplerState.PointClamp,
                            transformMatrix: Global.Camera.GetTransformation(graphicsDevice),
                            effect: null);
                        worldEnt.Draw(spriteBatch, gameTime, ShaderDrawingState.NO_SHADER);
                        Global.SpriteBatch.End();

                        // Draw the transition layers to the render target
                        Global.SpriteBatch.Begin();
                        worldEnt.Draw(spriteBatch, gameTime, ShaderDrawingState.RENDER_TARGET);
                        Global.SpriteBatch.End();

                        // Finally, draw the shader transition layer to the screen
                        Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                            samplerState: SamplerState.PointClamp,
                            transformMatrix: Global.Camera.GetTransformation(graphicsDevice),
                            effect: worldEnt.ActiveShader);
                        worldEnt.Draw(spriteBatch, gameTime, ShaderDrawingState.FIRST_LAYER);
                        Global.SpriteBatch.End();
                        break;
                }
            }

        }

        public void AddEntity(IGameEntity entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity), "Null cannot be added as an entity.");

            _entitiesToAdd.Add(entity);

        }

        public void RemoveEntity(IGameEntity entity)
        {
            if (entity is null)
                throw new ArgumentNullException(nameof(entity), "Null is not a valid entity.");

            _entitiesToRemove.Add(entity);

        }

        public void Clear()
        {

            _entitiesToRemove.AddRange(_entities);

        }

        public IEnumerable<T> GetEntitiesOfType<T>() where T : IGameEntity
        {
            return _entities.OfType<T>();
        }

    }
}
