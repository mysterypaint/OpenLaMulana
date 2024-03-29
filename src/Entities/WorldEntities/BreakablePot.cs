﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class BreakablePot : ParentInteractableWorldEntity
    {
        private Sprite _potSprite;

        public BreakablePot(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            _potSprite = new Sprite(_tex, 288, 0, 16, 16);
            Depth = (int)Global.DrawOrder.Tileset;
            HitboxWidth = 16;
            HitboxHeight = 16;
            IsCollidable = false;
            _sprIndex = null;

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = Global.WEStates.IDLE;
                _sprIndex = _potSprite;
                IsCollidable = true;
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_sprIndex != null)
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            switch(State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        State = Global.WEStates.IDLE;
                        _sprIndex = _potSprite;
                        IsCollidable = true;
                    }
                    break;
                case Global.WEStates.IDLE:
                    break;
            }
        }
    }
}