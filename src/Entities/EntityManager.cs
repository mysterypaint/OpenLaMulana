using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Audio;
using OpenLaMulana.Entities.WorldEntities.Parents;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using static OpenLaMulana.Entities.World;
using static OpenLaMulana.System.Camera;

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
            World worldEnt = null;
            foreach (IGameEntity entity in _entities.OrderBy(e => e.Depth))
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
                        worldEnt = (World)entity;
                        switch (Global.Camera.GetState())
                        {
                            default:
                            case CamStates.NONE:
                                // No fancy shaders: Draw everything normally
                                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                                    samplerState: SamplerState.PointClamp,
                                    transformMatrix: Global.Camera.GetTransformation(graphicsDevice),
                                    effect: worldEnt.ActiveShader);
                                worldEnt.Draw(spriteBatch, gameTime);
                                Global.SpriteBatch.End();
                                break;
                            case CamStates.TRANSITION_PIXELATE:
                                // Draw the Current View to a Render Target
                                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                            samplerState: SamplerState.PointClamp,
                            transformMatrix: null,
                            effect: null);
                                worldEnt.DrawPixelate(spriteBatch, gameTime, ShaderDrawingState.CURR_VIEW);
                                Global.SpriteBatch.End();

                                // Draw the transition layer to a Render Target
                                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                                    samplerState: SamplerState.PointClamp,
                                    transformMatrix: null,
                                    effect: Global.ShdBinary);
                                worldEnt.DrawPixelate(spriteBatch, gameTime, ShaderDrawingState.TRANSITION_LAYER);
                                Global.SpriteBatch.End();

                                // Draw the Destination View to a Render Target
                                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                                    samplerState: SamplerState.PointClamp,
                                    transformMatrix: null,
                                    effect: null);
                                worldEnt.DrawPixelate(spriteBatch, gameTime, ShaderDrawingState.DEST_VIEW);
                                Global.SpriteBatch.End();

                                // Draw everything to a final Render Target using the Transition shader
                                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                                    samplerState: SamplerState.PointClamp,
                                    transformMatrix: null,
                                    effect: worldEnt.ActiveShader);
                                worldEnt.DrawPixelate(spriteBatch, gameTime, ShaderDrawingState.OUTPUT_1X);
                                Global.SpriteBatch.End();

                                // Finally, draw the final render target to the screen, blowing up the transform
                                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                                    samplerState: SamplerState.PointClamp,
                                    transformMatrix: Global.Camera.GetTransformation(graphicsDevice),
                                    effect: worldEnt.ActiveShader);
                                worldEnt.DrawPixelate(spriteBatch, gameTime, ShaderDrawingState.OUTPUT);
                                Global.SpriteBatch.End();
                                break;
                        }
                        break;
                }
            }
            if (worldEnt != null)
            {
                Global.SpriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend,
                    samplerState: SamplerState.PointClamp,
                    transformMatrix: Global.Camera.GetTransformation(graphicsDevice),
                    effect: worldEnt.ActiveShader);
                worldEnt.DrawOverlayAView(spriteBatch, gameTime);
                Global.SpriteBatch.End();
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

        public int GetCount()
        {
            return _entities.Count;
        }

        public void SanityCheck()
        {
            // Juuuust in case there's any lingering particles we don't want loaded in memory... This deletes all the non-global entities
            foreach (IGameEntity entity in _entities)
            {
                bool deleteMe = true;
                if (entity is Protag || entity is World || entity is SpriteDefManager || entity is GameMenu || entity is MobileSuperX || entity is Jukebox)
                {
                    deleteMe = false;
                }

                if (deleteMe)
                    _entitiesToRemove.Add(entity);
            }

            Global.World.GetCurrField().QueueDeleteAllFieldAndRoomEntities();
            Global.World.GetCurrField().DeleteAllFieldAndRoomEntities();
            /*
            List<Field> fields = Global.World.GetAllFields();
            foreach(Field f in fields)
                f.DeleteAllFieldAndRoomEntities();
            */
        }
    }
}
