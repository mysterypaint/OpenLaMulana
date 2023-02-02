using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class EnemyBat : ParentInteractableWorldEntity
    {

        enum BatStates
        {
            Unspawned = -999,
            Flying = 0,
            Hanging,
            Dying
        }

        private BatStates _state = BatStates.Unspawned;
        private int _flyTimer = 0;
        private View _currView = null;
        private int _hsp = 0;
        private int _vsp = 0;
        private SubWeaponItemDrops _itemDrop = SubWeaponItemDrops.NONE;
        private int _flagToSetWhenDefeated = -1;


        public EnemyBat(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            // Fields 10 and above are programmatically treated as back fields. Enemy object bats automatically become back bats when placed behind them.
            _tex = Global.TextureManager.GetTexture(Global.Textures.ENEMY1);

            bool backsideBat = (Global.World.CurrField >= 10);

            Vector2 gfxOffset = new Vector2(80, 0);

            if (backsideBat)
                gfxOffset = new Vector2(64, 112);

            _sprIndex = new Sprite(_tex, (int)gfxOffset.X, (int)gfxOffset.Y, 16, 16);
            HP = 1;

            _itemDrop = (Global.SubWeaponItemDrops)op2;
            _flagToSetWhenDefeated = op3;

            _currView = Global.World.GetCurrentView();

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                _state = BatStates.Hanging;
            }

            HitboxWidth = 16;
            HitboxHeight = 16;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (_state)
            {
                case BatStates.Unspawned:
                    break;
                case BatStates.Flying:
                case BatStates.Hanging:
                    _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                    break;
                case BatStates.Dying:
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (_state)
            {
                case BatStates.Unspawned:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        _state = BatStates.Hanging;
                        _flyTimer = 50;
                    }
                    break;
                case BatStates.Hanging:



                    int tileX = (int)Math.Floor(Position.X / World.CHIP_SIZE);
                    int tileY = (int)Math.Floor(Position.Y / World.CHIP_SIZE);
                    World.ChipTypes currTile = World.TileGetAtPixel(_currView, tileX, tileY);

                    if (World.TileIsSolid(currTile) > 0)
                    {
                        // Tile is solid: Perform collision here

                    }





                    if (_flyTimer <= 0)
                    {
                        _state = BatStates.Flying;
                    }
                    else
                    {
                        _flyTimer--;
                    }
                    break;
                case BatStates.Flying:
                    if (_flyTimer <= 0)
                    {
                        _state = BatStates.Flying;
                    }
                    else
                    {
                        _flyTimer--;
                    }
                    break;
            }
        }
    }
}