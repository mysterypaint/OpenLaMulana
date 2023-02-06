using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class PushableBlock : ParentInteractableWorldEntity
    {
        enum PushableBlockTypes
        {
            Gray = -1,
            Red = 0,
            Green,
            Yellow
        };

        private int _flagToSet;
        private int _unsolvedXPos;
        private int _unsolvedYPos;
        private int _solutionChipXCoord;
        private int _solutionChipYCoord;
        private PushableBlockTypes _blockType;
        private bool _updatedPositions = true;
        private Sprite _blockSpr;

        public PushableBlock(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            _blockSpr = new Sprite(_tex, 288, 64, 16, 16);
            _sprIndex = null;
            /*
             A block that can be pushed. If you put it on the OBJECT36 block place, you will not be able to push it.
            If you specify the same flag as the set flag of the block storage in OP1, you can move the block to the
            coordinates of OP2 and 3 by switching the view after placing it in the block storage.
            
            This is used to fix the solved block puzzle to the solved position even after switching the view.
            OP4 becomes a normal block if nothing is specified. If you specify 0, it will be a red block, 1 will be a green block,
            and 2 will be a yellow block, and it will only respond to blocks of the same color.
            
            If there is no need, only normal graphics will be prepared. Blocks normally don't respond to item time ramps, but if you set OP4 to 3, you'll be able to stop them from falling.
             */
            _flagToSet = op1;

            // These specify the top-left coordinate 8x8 chip position of the block
            _unsolvedXPos = x;
            _unsolvedYPos = y;

            if (_unsolvedXPos < 0)
                _unsolvedXPos += World.VIEW_WIDTH;
            else if (_unsolvedXPos > World.VIEW_WIDTH)
                _unsolvedXPos -= World.VIEW_WIDTH;
            if (_unsolvedYPos < 0)
                _unsolvedYPos += World.VIEW_HEIGHT;
            else if (_unsolvedYPos > World.VIEW_HEIGHT)
                _unsolvedYPos -= World.VIEW_HEIGHT;

            _unsolvedXPos *= World.CHIP_SIZE;
            _unsolvedYPos *= World.CHIP_SIZE;

            _solutionChipXCoord = op2;
            _solutionChipYCoord = op3;
            _blockType = (PushableBlockTypes)op4;

            HitboxWidth = 16;
            HitboxHeight = 16;
            IsCollidable = false;

            if (_flagToSet < 0 || _flagToSet >= Global.GameFlags.InGameFlags.Length)
            {
                if (HelperFunctions.EntityMaySpawn(StartFlags))
                {
                    State = Global.WEStates.IDLE;
                    _sprIndex = _blockSpr;
                    IsCollidable = true;
                }
            }
            else if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_flagToSet])
            {
                State = Global.WEStates.IDLE;
                _sprIndex = _blockSpr;
                IsCollidable = true;

            }
            else if (Global.GameFlags.InGameFlags[_flagToSet])
            {
                State = Global.WEStates.DYING;
                _sprIndex = _blockSpr;
                IsCollidable = true;
                _updatedPositions = false;
                //Position = new Vector2(_solutionChipXCoord * World.CHIP_SIZE, _solutionChipYCoord * World.CHIP_SIZE);
            }
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (_sprIndex != null)
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            int roomWidthPx, roomHeightPx;
            Vector2 relativeRoomCoords;

            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (_flagToSet < 0 || _flagToSet >= Global.GameFlags.InGameFlags.Length)
                    {
                        if (HelperFunctions.EntityMaySpawn(StartFlags))
                        {
                            IsCollidable = true;
                            State = Global.WEStates.IDLE;
                            _sprIndex = _blockSpr;
                        }
                    }
                    else if (HelperFunctions.EntityMaySpawn(StartFlags) && !Global.GameFlags.InGameFlags[_flagToSet])
                    {
                        IsCollidable = true;
                        State = Global.WEStates.IDLE;
                        _sprIndex = _blockSpr;
                    }
                    else if (Global.GameFlags.InGameFlags[_flagToSet])
                    {
                        roomWidthPx = (World.VIEW_WIDTH * World.CHIP_SIZE);
                        roomHeightPx = (World.VIEW_HEIGHT * World.CHIP_SIZE);
                        State = Global.WEStates.DYING;
                        _sprIndex = _blockSpr;
                        IsCollidable = true;
                        relativeRoomCoords = new Vector2((float)Math.Floor(Position.X / roomWidthPx) * roomWidthPx, (float)Math.Floor(Position.Y / roomHeightPx) * roomHeightPx);
                        Position = relativeRoomCoords + new Vector2(_solutionChipXCoord * World.CHIP_SIZE, _solutionChipYCoord * World.CHIP_SIZE);
                    }
                    break;
                case Global.WEStates.IDLE:
                    if (Global.DevModeEnabled)
                    {
                        if (InputManager.DirectKeyboardCheckPressed(Microsoft.Xna.Framework.Input.Keys.E))
                        {
                            if (_flagToSet >= 0 && _flagToSet < Global.GameFlags.InGameFlags.Length)
                                Global.GameFlags.InGameFlags[_flagToSet] = true;
                        }
                    }

                    if ((SourceDestView.X != Global.World.CurrViewX || SourceDestView.Y != Global.World.CurrViewY) && Global.Camera.GetState() == Camera.CamStates.NONE && !_updatedPositions)
                    {
                        //ActiveView destView = Global.World.GetActiveViews()[(int)World.AViews.DEST];
                        roomWidthPx = (World.VIEW_WIDTH * World.CHIP_SIZE);
                        roomHeightPx = (World.VIEW_HEIGHT * World.CHIP_SIZE);
                        relativeRoomCoords = new Vector2((float)Math.Floor(Position.X / roomWidthPx) * roomWidthPx, (float)Math.Floor(Position.Y / roomHeightPx) * roomHeightPx);
                        if (_flagToSet < 0 || _flagToSet >= Global.GameFlags.InGameFlags.Length)
                        {
                            Position = relativeRoomCoords + new Vector2(_unsolvedXPos, _unsolvedYPos);
                        }
                        else if (Global.GameFlags.InGameFlags[_flagToSet])
                        {
                            Position = relativeRoomCoords + new Vector2(_solutionChipXCoord * World.CHIP_SIZE, _solutionChipYCoord * World.CHIP_SIZE);
                        }
                        else
                            Position = relativeRoomCoords + new Vector2(_unsolvedXPos, _unsolvedYPos);

                        _updatedPositions = true;
                    }
                    else if (SourceDestView.X == Global.World.CurrViewX && SourceDestView.Y == Global.World.CurrViewY)
                    {
                        _updatedPositions = false;
                    }
                    break;
                case Global.WEStates.DYING:
                    if (Global.Camera.GetState() == Camera.CamStates.TRANSITION_CARDINAL || !_updatedPositions)
                    {
                        //ActiveView destView = Global.World.GetActiveViews()[(int)World.AViews.DEST];
                        roomWidthPx = (World.VIEW_WIDTH * World.CHIP_SIZE);
                        roomHeightPx = (World.VIEW_HEIGHT * World.CHIP_SIZE);
                        relativeRoomCoords = new Vector2((float)Math.Floor(Position.X / roomWidthPx) * roomWidthPx, (float)Math.Floor(Position.Y / roomHeightPx) * roomHeightPx);
                        if (_flagToSet < 0 || _flagToSet >= Global.GameFlags.InGameFlags.Length)
                        {
                            Position = relativeRoomCoords + new Vector2(_unsolvedXPos, _unsolvedYPos);
                        }
                        else if (Global.GameFlags.InGameFlags[_flagToSet])
                        {
                            Position = relativeRoomCoords + new Vector2(_solutionChipXCoord * World.CHIP_SIZE, _solutionChipYCoord * World.CHIP_SIZE);
                        }
                        else
                            Position = relativeRoomCoords + new Vector2(_unsolvedXPos, _unsolvedYPos);

                        _updatedPositions = true;
                    }
                    else if (SourceDestView.X == Global.World.CurrViewX && SourceDestView.Y == Global.World.CurrViewY)
                    {
                        _updatedPositions = false;
                    }
                    break;
            }
        }
    }
}