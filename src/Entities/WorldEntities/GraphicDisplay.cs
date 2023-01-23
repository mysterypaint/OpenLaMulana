using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Chip;
using static OpenLaMulana.Entities.World;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class GraphicDisplay : InteractableWorldEntity
    {
        private int _checkFlag = -1;
        private bool _revertMapCollisionAfterDestroyed = false;
        private bool _restoreCollisionWhenFlagConditionIsMet = false;
        private bool _isSolid = false;
        private Sprite _displayedGraphic;
        private Field _currField = null;
        private ChipTypes _rewritingTileType = ChipTypes.VOID;
        private View _previousDestView = null;
        private View _activeDestView = null;

        public GraphicDisplay(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            Vector2 _texCoords = HelperFunctions.GetOPCoords(op1);
            Vector2 _spriteWidthHeight = HelperFunctions.GetOPCoords(op2);

            _checkFlag = op3;

            _activeDestView = Global.World.GetActiveViews()[(int)AViews.DEST].GetView();
            SourceDestView = destView;
            _currField = destView.GetParentField();

            HitboxWidth = (int)_spriteWidthHeight.X;
            HitboxHeight = (int)_spriteWidthHeight.Y;

            bool _useEventTexture = false;
            if (op4 >= 4)
            {
                _revertMapCollisionAfterDestroyed = true;
                _restoreCollisionWhenFlagConditionIsMet = HelperFunctions.GetBit((byte)op4, 3);

                _rewritingTileType = World.ChipTypes.SOLID;
                if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkFlag])
                {
                    RewriteMapRegion(_rewritingTileType);
                }
                _useEventTexture = HelperFunctions.GetBit((byte)op4, 1);
            }
            else if (op4 >= 2)
            {
                _restoreCollisionWhenFlagConditionIsMet = HelperFunctions.GetBit((byte)op4, 3);
                _isSolid = true;// HelperFunctions.GetBit((byte)op4, 2);

                _rewritingTileType = World.ChipTypes.SOLID;
                if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkFlag])
                {
                    RewriteMapRegion(_rewritingTileType);
                }

                _useEventTexture = HelperFunctions.GetBit((byte)op4, 1);
            }
            else if (op4 > 1)
            {
                _useEventTexture = HelperFunctions.GetBit((byte)op4, 1);
                if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkFlag])
                {
                    State = Global.WEStates.IDLE;
                    _sprIndex = _displayedGraphic;

                    _rewritingTileType = World.ChipTypes.VOID;
                    RewriteMapRegion(_rewritingTileType);
                }
            }
            else if (op4 == 1)
            {
                _useEventTexture = true;
                if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkFlag])
                {
                    State = Global.WEStates.IDLE;
                    _sprIndex = _displayedGraphic;

                    _rewritingTileType = World.ChipTypes.VOID;
                    RewriteMapRegion(_rewritingTileType);
                }
            }
            else if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkFlag])
            {
                _rewritingTileType = World.ChipTypes.VOID;
                RewriteMapRegion(_rewritingTileType);
            }

            if (_useEventTexture)
            {
                _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
                Depth = (int)Global.DrawOrder.AboveTilesetGraphicDisplay;
            }
            else
            {
                _tex = Global.TextureManager.GetTexture(Global.World.GetCurrMapTexture());
                Depth = (int)Global.DrawOrder.AboveTilesetGraphicDisplay;
            }

            _displayedGraphic = new Sprite(_tex, _texCoords, _spriteWidthHeight);

            if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkFlag])
            {
                State = Global.WEStates.IDLE;
                _sprIndex = _displayedGraphic;
            }
            else
            {
                State = Global.WEStates.UNSPAWNED;
                _sprIndex = null;

                if (!_revertMapCollisionAfterDestroyed)
                {
                    _isSolid = false;
                    //CollisionBehavior = World.ChipTypes.VOID;
                    //RestoreMapRegion();
                }
            }
            /*
        switch (_texturePage)
        {
            default:
            case 0:
                break;
            case 1:
                break;
            case 2:
                _tex = Global.TextureManager.GetTexture(Global.World.GetCurrMapTexture());
                Depth = (int)Global.DrawOrder.AboveEntitiesGraphicDisplay;
                break;
            case 3:
                _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
                Depth = (int)Global.DrawOrder.Foreground;
                break;
            case 4:
                _tex = Global.TextureManager.GetTexture(Global.World.GetCurrMapTexture());
                Depth = (int)Global.DrawOrder.Overlay;
                break;
        }
            */
        }

        private void RestoreMapRegion()
        {
            int tX = (int)(Position.X / World.CHIP_SIZE) % World.ROOM_WIDTH;
            int tY = (int)((Position.Y / World.CHIP_SIZE) % World.ROOM_HEIGHT);
            int tilesWide = HitboxWidth / World.CHIP_SIZE;
            int tilesHigh = HitboxHeight / World.CHIP_SIZE;

            if (tX < 0)
                tX = World.ROOM_WIDTH + tX;
            if (tY < 0)
                tY = World.ROOM_HEIGHT + tY;

            View activeCurrView = Global.World.GetBackupView();
            View sourceView = Global.World.GetCurrentView();
            for (var y = tY; y < tY + tilesHigh; y++)
            {
                for (var x = tX; x < tX + tilesWide; x++)
                {
                    _activeDestView.Chips[x % World.ROOM_WIDTH, y % World.ROOM_HEIGHT] = sourceView.Chips[x % World.ROOM_WIDTH, y % World.ROOM_HEIGHT]; //_overwrittenChips[x - tX, y - tY];
                    activeCurrView.Chips[x % World.ROOM_WIDTH, y % World.ROOM_HEIGHT] = sourceView.Chips[x % World.ROOM_WIDTH, y % World.ROOM_HEIGHT];//_overwrittenChips[x - tX, y - tY];
                }
            }
        }

        private void RewriteMapRegion(World.ChipTypes tileType)
        {
            int tX = (int)(Position.X / World.CHIP_SIZE) % World.ROOM_WIDTH;
            int tY = (int)((Position.Y / World.CHIP_SIZE) % World.ROOM_HEIGHT);
            int tilesWide = HitboxWidth / World.CHIP_SIZE;
            int tilesHigh = HitboxHeight / World.CHIP_SIZE;

            if (tX < 0)
                tX += World.ROOM_WIDTH;
            if (tY < 0)
                tY += World.ROOM_HEIGHT;

            Chip replacingTile = _currField.CreateTileOfType(tileType);

            View activeDestView = Global.World.GetActiveViews()[(int)AViews.DEST].GetView();

            for (var y = 0; y < tilesHigh; y++)
            {
                for (var x = 0; x < tilesWide; x++)
                {
                    //_overwrittenChips[x - tX, y - tY] = _activeDestView.Chips[x % World.ROOM_WIDTH, y % World.ROOM_HEIGHT];
                    if ((tX + x > ROOM_WIDTH - 1) || (tY + y > ROOM_HEIGHT - 1))
                        continue;
                    activeDestView.Chips[(tX + x), (tY + y)] = replacingTile;
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_sprIndex != null)
                _sprIndex.Draw(spriteBatch, Position + new Vector2(0, World.HUD_HEIGHT));
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if ((HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_checkFlag]))
                    {
                        State = Global.WEStates.IDLE;
                        _sprIndex = _displayedGraphic;
                        //_rewritingTileType = ChipTypes.VOID;
                        RewriteMapRegion(_rewritingTileType);
                    }
                    break;
                case Global.WEStates.IDLE:

                    if (Global.DevModeEnabled)
                    {
                        if (InputManager.DirectKeyboardCheckPressed(Microsoft.Xna.Framework.Input.Keys.E))
                        {
                            Global.GameFlags.InGameFlags[_checkFlag] = true;
                        }
                    }

                    _activeDestView = Global.World.GetActiveViews()[(int)AViews.DEST].GetView();

                    if (_previousDestView != _activeDestView)
                    {
                        if (Global.World.CurrViewX == SourceDestView.X && Global.World.CurrViewY == SourceDestView.Y && Global.World.GetCurrField() == SourceDestView.GetParentField())
                            RewriteMapRegion(_rewritingTileType);
                    }
                    _previousDestView = _activeDestView;

                    if (Global.GameFlags.InGameFlags[_checkFlag])
                    {
                        State = Global.WEStates.DYING;
                        _sprIndex = null;
                        if (_revertMapCollisionAfterDestroyed)
                        {
                            RestoreMapRegion();
                            _isSolid = false;
                            CollisionBehavior = World.ChipTypes.VOID;
                        } else
                        {
                            _rewritingTileType = ChipTypes.VOID;
                            if (Global.World.CurrViewX == SourceDestView.X && Global.World.CurrViewY == SourceDestView.Y && Global.World.GetCurrField() == SourceDestView.GetParentField())
                                RestoreMapRegion(); //RewriteMapRegion(_rewritingTileType);
                        }
                    }
                    break;
                case Global.WEStates.DYING:
                    break;
            }
        }
    }
}