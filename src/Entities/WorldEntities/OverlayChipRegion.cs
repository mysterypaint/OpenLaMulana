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
    internal class OverlayChipRegion : ParentInteractableWorldEntity
    {
        private ChipTypes _rewritingTileType;
        private View _previousDestView = null;
        private View _activeDestView = null;
        private Field _currField = null;
        private int _tileWidth;
        private int _tileHeight;

        public OverlayChipRegion(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tileWidth = op1;
            _tileHeight = op2;
            HitboxWidth = _tileWidth * World.CHIP_SIZE;
            HitboxHeight = _tileHeight * World.CHIP_SIZE;
            _rewritingTileType = Global.World.GetCurrField().GetSpecialChipTypeAtIndex(op3 - 1);
            _activeDestView = Global.World.GetActiveViews()[(int)AViews.DEST].GetView();
            SourceDestView = destView;
            _currField = destView.GetParentField();
            _sprIndex = null;

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = Global.WEStates.IDLE;
                RewriteMapRegion(_rewritingTileType);
            }
            else
            {
                State = Global.WEStates.UNSPAWNED;
            }
        }

        private void RewriteMapRegion(World.ChipTypes tileType)
        {
            int tX = (int)(Position.X / World.CHIP_SIZE) % World.VIEW_WIDTH;
            int tY = (int)((Position.Y / World.CHIP_SIZE) % World.VIEW_HEIGHT);
            int tilesWide = HitboxWidth / World.CHIP_SIZE;
            int tilesHigh = HitboxHeight / World.CHIP_SIZE;

            if (tX < 0)
                tX += World.VIEW_WIDTH;
            if (tY < 0)
                tY += World.VIEW_HEIGHT;

            View activeOverlayView = Global.World.GetActiveViews()[(int)AViews.OVERLAY].GetView();

            for (var y = 0; y < tilesHigh; y++)
            {
                for (var x = 0; x < tilesWide; x++)
                {
                    //_overwrittenChips[x - tX, y - tY] = _activeDestView.Chips[x % World.ROOM_WIDTH, y % World.ROOM_HEIGHT];
                    if ((tX + x > VIEW_WIDTH - 1) || (tY + y > VIEW_HEIGHT - 1))
                        continue;

                    Chip thisDestViewChip = SourceDestView.Chips[(tX + x), (tY + y)];
                    thisDestViewChip.SpecialChipBehavior = _rewritingTileType;
                    thisDestViewChip.IsOverlay = true;

                    activeOverlayView.Chips[(tX + x), (tY + y)].CloneTile(thisDestViewChip);
                    _activeDestView.Chips[(tX + x), (tY + y)].CloneTile(thisDestViewChip);
                }
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (Global.Protag.State == PlayerState.NPC_DIALOGUE)
                return;
            Rectangle rect = new Rectangle((int)Position.X, (int)Position.Y + World.HUD_HEIGHT, HitboxWidth, HitboxHeight);
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (Global.DevModeEnabled)
                    {
                        HelperFunctions.DrawRectangle(spriteBatch, rect, new Color(66, 7, 66, 6));
                    }
                    break;
                case Global.WEStates.IDLE:
                    // The forced draw order in the Entity Manager screws this up... not gonna bother fixing, because it's a debug feature anyway (these objects are always invisible: they just change the contents of the Overlay Chip layer)
                    // Keeping it here to vaguely get an idea of where these objects are, before the tiles are overwritten when the camera stops scrolling
                    if (Global.DevModeEnabled)
                    {
                        HelperFunctions.DrawRectangle(spriteBatch, rect, new Color(200, 20, 200, 20));
                    }
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        State = Global.WEStates.IDLE;
                        RewriteMapRegion(_rewritingTileType);
                    }
                    break;
                case Global.WEStates.IDLE:
                    _activeDestView = Global.World.GetActiveViews()[(int)AViews.DEST].GetView();

                    if (_previousDestView != _activeDestView)
                    {
                        if (Global.World.CurrViewX == SourceDestView.X && Global.World.CurrViewY == SourceDestView.Y)
                            RewriteMapRegion(_rewritingTileType);
                    }
                    _previousDestView = _activeDestView;
                    break;
            }
        }
    }
}