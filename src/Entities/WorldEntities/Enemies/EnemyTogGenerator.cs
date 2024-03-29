﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;
using static OpenLaMulana.System.Camera;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class EnemyTogGenerator : ParentInteractableWorldEntity
    {
        public EnemyTogGenerator(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ENEMY1);
            _sprIndex = null;

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = Global.WEStates.IDLE;
            }
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_sprIndex != null)
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        State = Global.WEStates.IDLE;
                        _sprIndex = new Sprite(_tex, 0, 0, 16, 16);
                    }
                    break;
                case Global.WEStates.IDLE:

                    break;
            }
        }
    }
}