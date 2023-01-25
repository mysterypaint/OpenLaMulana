using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenLaMulana.System
{
    public class Camera
    {
        public Vector2 CurrWindowSize = new Vector2(0, 0);
        public int DisplayZoomFactor { get; private set; }
        public Vector2 BaseWindowSize { get; private set; }

        public Matrix Transform; // Matrix Transform
        public Vector2 Position; // Camera Position
        protected float _rotation; // Camera Rotation
        protected int _viewWidth = 0;
        protected int _viewHeight = 0;

        private int _moveSpeedX = 0;
        private int _moveSpeedY = 0;
        private int _moveToX = 0;
        private int _moveToY = 0;
        public bool LockTo30FPS { get; set; } = false;

        private Protag _protag;

        public Camera()
        {
            _rotation = 0.0f;
            Position = Vector2.Zero;
        }

        internal void UpdateWindowSize(int width, int height, int dispZoomFactor)
        {
            BaseWindowSize = new Vector2(width / dispZoomFactor, height / dispZoomFactor);
            CurrWindowSize = new Vector2(width, height);
            DisplayZoomFactor = dispZoomFactor;
            _viewWidth = width;
            _viewHeight = height;

            Transform = Matrix.CreateTranslation(new Vector3(-Position.X - (BaseWindowSize.X / 2), -Position.Y - (BaseWindowSize.Y / 2), 0))
                * Matrix.Identity
                * Matrix.CreateRotationZ(Rotation)
                * Matrix.CreateScale(DisplayZoomFactor, DisplayZoomFactor, 1)
                * Matrix.CreateTranslation(new Vector3(_viewWidth * 0.5f, _viewHeight * 0.5f, 0));
        }


        public float Rotation
        {
            get { return _rotation; }
            set { _rotation = value; }
        }

        // Auxiliary function to move the camera
        public void Move(Vector2 amount)
        {
            Position += amount;
        }
        // Get set position
        public Vector2 Pos
        {
            get { return Position; }
            set { Position = value; }
        }

        public enum CamStates : int
        {
            NONE,
            TRANSITION_CARDINAL,
            TRANSITION_PIXELATE,
            TRANSITION_WIPE,
            STANDBY, // Waiting for next action from elsewhere
            MAX
        };

        private int _state = (int)CamStates.NONE;

        private const int ROOM_PX_WIDTH = (World.ROOM_WIDTH * World.CHIP_SIZE);
        private const int ROOM_PX_HEIGHT = (World.ROOM_HEIGHT * World.CHIP_SIZE);

        private const int SCREEN_LEFT_EDGE = (Protag.SPRITE_WIDTH / 2);
        private const int SCREEN_RIGHT_EDGE = -(Protag.SPRITE_WIDTH / 2);
        private const int SCREEN_BOTTOM_EDGE = (Protag.SPRITE_HEIGHT / 2);
        private const int SCREEN_TOP_EDGE = -(Protag.SPRITE_HEIGHT / 2);

        //private int cardinalTransitionTimer = 0;
        public Matrix GetTransformation(GraphicsDevice graphicsDevice)
        {
            Transform = Matrix.CreateTranslation(new Vector3(-Position.X - (BaseWindowSize.X / 2), -Position.Y - (BaseWindowSize.Y / 2), 0))
                * Matrix.Identity
                * Matrix.CreateRotationZ(Rotation)
                * Matrix.CreateScale(DisplayZoomFactor, DisplayZoomFactor, 1)
                * Matrix.CreateTranslation(new Vector3(_viewWidth * 0.5f, _viewHeight * 0.5f, 0));
            /*
            Matrix.CreateTranslation(new Vector3(-Position.X - (BaseWindowSize.X / 2), -Position.Y - (BaseWindowSize.Y / 2), 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(new Vector3(DisplayZoomFactor, DisplayZoomFactor, 1)) *
                Matrix.CreateTranslation(new Vector3(_viewWidth * 0.5f, _viewHeight * 0.5f, 0));
            */
            return Transform;
        }

        public void Update(GameTime gameTime)
        {
            switch (_state)
            {
                case (int)CamStates.NONE:
                    break;
                case (int)CamStates.STANDBY:
                    break;
                case (int)CamStates.TRANSITION_PIXELATE:
                    // Perform this when done to update visuals
                    //Global.World.UpdateCurrActiveView();
                    //_state = (int)CamStates.NONE;
                    //Global.World.activeShader = null;
                    break;
                case (int)CamStates.TRANSITION_CARDINAL:
                    if (Global.Main.State == Global.GameState.PAUSED)
                        break;

                    // The screen transitions specifically should be 30fps
                    if (Global.AnimationTimer.OneFrameElapsed())
                        break;
                    float posX = Position.X;
                    _protag.SetHsp(0);
                    float newPlayerX = _protag.BBox.X;

                    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

                    View destView, currView;
                    int destRoomX, destRoomY, currRoomX, currRoomY;

                    if (_moveToX < 0)
                    {
                        float targetX = SCREEN_RIGHT_EDGE -5;
                        if (Position.X + _moveSpeedX > _moveToX)
                        {
                            posX += _moveSpeedX;
                            newPlayerX = HelperFunctions.Lerp(SCREEN_LEFT_EDGE, targetX, Math.Abs(Position.X) / Math.Abs(_moveToX)
                            );// Lerp(newPX, targetX, Math.Abs(Position.X) / Math.Abs(_moveToX));
                            _protag.SetPosition(new Point((int)Math.Round(newPlayerX), _protag.BBox.Y));
                        }
                        else
                        {
                            posX = 0;
                            _protag.SetPosition(new Point(ROOM_PX_WIDTH + (int)targetX, _protag.BBox.Y));

                            destView = Global.World.GetActiveViews()[(int)World.AViews.DEST].GetView();
                            destRoomX = destView.X;
                            destRoomY = destView.Y;
                            currView = Global.World.GetActiveViews()[(int)World.AViews.CURR].GetView();
                            currRoomX = currView.X;
                            currRoomY = currView.Y;
                            MoveAllEntities(new Vector2(ROOM_PX_WIDTH, 0), true, destView);
                            
                            _moveSpeedX = 0;
                            _moveToX = 0;
                            Global.World.UpdateCurrActiveView();
                            _state = (int)CamStates.NONE;
                            _protag.State = _protag.GetPrevState();
                        }
                    }
                    else if (_moveToX > 0)
                    {
                        float targetX = SCREEN_LEFT_EDGE - 5;
                        if (Position.X + _moveSpeedX < _moveToX)
                        {
                            posX += _moveSpeedX;
                            _protag.SetPosition(new Point((int)HelperFunctions.Lerp(ROOM_PX_WIDTH + SCREEN_RIGHT_EDGE, ROOM_PX_WIDTH + targetX, Math.Abs(Position.X) / Math.Abs(_moveToX)), _protag.BBox.Y));
                        }
                        else
                        {
                            posX = 0;
                            _protag.SetPosition(new Point((int)targetX, _protag.BBox.Y));

                            destView = Global.World.GetActiveViews()[(int)World.AViews.DEST].GetView();
                            destRoomX = destView.X;
                            destRoomY = destView.Y;
                            currView = Global.World.GetActiveViews()[(int)World.AViews.CURR].GetView();
                            currRoomX = currView.X;
                            currRoomY = currView.Y;
                            MoveAllEntities(new Vector2(-ROOM_PX_WIDTH, 0), true, destView);
                            
                            _moveSpeedX = 0;
                            _moveToX = 0;
                            Global.World.UpdateCurrActiveView();
                            _state = (int)CamStates.NONE;
                            _protag.State = _protag.GetPrevState();
                        }
                    }




                    float posY = Position.Y;
                    _protag.SetVsp(0);
                    float newPlayerY = _protag.BBox.Y;

                    if (_moveToY < 0)
                    {
                        float targetY = SCREEN_BOTTOM_EDGE - World.CHIP_SIZE*2 - _protag.BBox.Height;
                        if (Position.Y + _moveSpeedY > _moveToY)
                        {
                            posY += _moveSpeedY;
                            newPlayerY = HelperFunctions.Lerp(World.HUD_HEIGHT + Protag.SPRITE_HEIGHT, targetY, Math.Abs(Position.Y) / Math.Abs(_moveToY)
                            );// Lerp(newPX, targetY, Math.Abs(Position.Y) / Math.Abs(_moveToY));
                            _protag.SetPosition(new Point(_protag.BBox.X, (int)Math.Round(newPlayerY)));
                        }
                        else
                        {
                            posY = 0;
                            _protag.SetPosition(new Point(_protag.BBox.X, ROOM_PX_HEIGHT + (int)targetY));

                            destView = Global.World.GetActiveViews()[(int)World.AViews.DEST].GetView();
                            destRoomX = destView.X;
                            destRoomY = destView.Y;
                            currView = Global.World.GetActiveViews()[(int)World.AViews.CURR].GetView();
                            currRoomX = currView.X;
                            currRoomY = currView.Y;
                            MoveAllEntities(new Vector2(0, ROOM_PX_HEIGHT), true, destView);
                            
                            _moveSpeedY = 0;
                            _moveToY = 0;
                            Global.World.UpdateCurrActiveView();
                            _state = (int)CamStates.NONE;
                            _protag.State = _protag.GetPrevState();
                        }
                    }
                    else if (_moveToY > 0)
                    {
                        float targetY = Protag.SPRITE_HEIGHT;
                        if (Position.Y + _moveSpeedY < _moveToY - World.HUD_HEIGHT)
                        {
                            posY += _moveSpeedY;
                            _protag.SetPosition(new Point(_protag.BBox.X,
                                (int)HelperFunctions.Lerp(ROOM_PX_HEIGHT, ROOM_PX_HEIGHT + targetY, Math.Abs(Position.Y) / Math.Abs(_moveToY))
                                ));
                        }
                        else
                        {
                            posY = 0;
                            _protag.SetPosition(new Point(_protag.BBox.X, (int)targetY));

                            destView = Global.World.GetActiveViews()[(int)World.AViews.DEST].GetView();
                            destRoomX = destView.X;
                            destRoomY = destView.Y;
                            currView = Global.World.GetActiveViews()[(int)World.AViews.CURR].GetView();
                            currRoomX = currView.X;
                            currRoomY = currView.Y;
                            MoveAllEntities(new Vector2(0, -ROOM_PX_HEIGHT), true, destView);
                            _moveSpeedY = 0;
                            _moveToY = 0;
                            Global.World.UpdateCurrActiveView();
                            _state = (int)CamStates.NONE;
                            _protag.State = _protag.GetPrevState();
                        }
                    }

                    Position = new Vector2(posX, posY); //Global.InputManager.DirMoveY
                    break;
            }
        }

        private void MoveAllGlobalEntities(Vector2 globalOffsetVector)
        {
            Field currField = Global.World.GetField(Global.World.CurrField);
            List<IGameEntity> allFieldEntities = new List<IGameEntity>();
            allFieldEntities.AddRange(currField.GetFieldEntities());

            foreach (IGameEntity worldEntity in allFieldEntities)
            {
                if (worldEntity is IGlobalWorldEntity)
                {
                    IGlobalWorldEntity gE = (IGlobalWorldEntity)worldEntity;
                    gE.OriginPosition = gE.TrueSpawnCoord.ToVector2() + globalOffsetVector;
                } else if (worldEntity is IRoomWorldEntity)
                {
                    IRoomWorldEntity rE = (IRoomWorldEntity)worldEntity;
                    if (rE.IsGlobal)
                        rE.OriginPosition = rE.TrueSpawnCoord + globalOffsetVector;
                }
            }
        }

        private void MoveAllEntities(Vector2 offsetVector, bool includeGlobals = false, View destView = null)
        {
            Field currField = Global.World.GetField(Global.World.CurrField);
            List<IGameEntity> allFieldEntities = new List<IGameEntity>();
            allFieldEntities.AddRange(currField.GetViewEntities());
            if (includeGlobals)
                allFieldEntities.AddRange(currField.GetFieldEntities());

            int ts = World.CHIP_SIZE;

            foreach (IGameEntity worldEntity in allFieldEntities)
            {
                if (worldEntity is IRoomWorldEntity)
                {
                    IRoomWorldEntity rE = (IRoomWorldEntity)worldEntity;

                    if (rE.IsGlobal)
                    {
                        // Calculate the Origin's Position for each Global Platform entity (This should hopefully not need any modification...)
                        Point roomOffset = new Point(rE.ViewCoords.X - destView.X, rE.ViewCoords.Y - destView.Y);

                        if (roomOffset.X < 0)
                            roomOffset.X += World.FIELD_WIDTH;
                        if (roomOffset.Y < 0)
                            roomOffset.Y += World.FIELD_HEIGHT;

                        Point tileOffset = new Point(roomOffset.X * World.ROOM_WIDTH, roomOffset.Y * World.ROOM_HEIGHT) + rE.RelativeViewTilePos.ToPoint();
                        Vector2 offsetCoords = new Vector2(tileOffset.X * World.CHIP_SIZE, tileOffset.Y * World.CHIP_SIZE);

                        rE.OriginPosition = offsetCoords;



                        /*
                         * 

*/

                        //rE.OriginPosition = rE.TrueSpawnCoord + globalOffsetVector;
                    }
                    else if (!rE.ManuallySpawned)
                        rE.Position += offsetVector;
                }
                else
                {
                    if (includeGlobals)
                    {
                        if (worldEntity is IGlobalWorldEntity)
                        {
                            IGlobalWorldEntity gE = (IGlobalWorldEntity)worldEntity;

                            if (!gE.ManuallySpawned)
                            {
                                /*
                                 * 
                                Point roomOffset = new Point(gE.ViewCoords.X - destView.X, gE.ViewCoords.Y - destView.Y);

                                Point tileOffset = new Point(roomOffset.X * World.ROOM_WIDTH, roomOffset.Y * World.ROOM_HEIGHT) + gE.RelativeView;
                                Vector2 offsetCoords = new Vector2(tileOffset.X * World.CHIP_SIZE, tileOffset.Y * World.CHIP_SIZE);

                                gE.OriginPosition = offsetCoords;
                                */

                                Point roomOffset = new Point(gE.ViewCoords.X - destView.X, gE.ViewCoords.Y - destView.Y);
                                Point tileOffset = new Point(roomOffset.X * World.ROOM_WIDTH, roomOffset.Y * World.ROOM_HEIGHT) + gE.RelativeView;
                                Vector2 offsetCoords = new Vector2(tileOffset.X * World.CHIP_SIZE, tileOffset.Y * World.CHIP_SIZE);

                                gE.OriginPosition = offsetCoords;

                                //gE.OriginPosition = gE.TrueSpawnCoord.ToVector2() + globalOffsetVector;
                            }
                        }
                    }
                }
            }
        }

        internal void UpdateMoveTarget(World.VIEW_DIR movingDirection)
        {
            switch (movingDirection)
            {
                case World.VIEW_DIR.SELF:
                    _moveSpeedX = 0;
                    _moveSpeedY = 0;
                    _moveToX = 0;
                    _moveToY = 0;
                    break;
                case World.VIEW_DIR.LEFT:
                    _moveSpeedX = -8;
                    _moveSpeedY = 0;
                    _moveToX = -(World.ROOM_WIDTH * World.CHIP_SIZE);
                    _moveToY = 0;
                    _state = (int)CamStates.TRANSITION_CARDINAL;
                    break;
                case World.VIEW_DIR.DOWN:
                    _moveSpeedX = 0;
                    _moveSpeedY = 8;
                    _moveToX = 0;
                    _moveToY = ((World.ROOM_HEIGHT * World.CHIP_SIZE) * 1) + (World.HUD_HEIGHT);
                    _state = (int)CamStates.TRANSITION_CARDINAL;
                    break;
                case World.VIEW_DIR.RIGHT:
                    _moveSpeedX = 8;
                    _moveSpeedY = 0;
                    _moveToX = (World.ROOM_WIDTH * World.CHIP_SIZE);
                    _moveToY = 0;
                    _state = (int)CamStates.TRANSITION_CARDINAL;
                    break;
                case World.VIEW_DIR.UP:
                    _moveSpeedX = 0;
                    _moveSpeedY = -8;
                    _moveToX = 0;
                    _moveToY = -(World.ROOM_HEIGHT * World.CHIP_SIZE);
                    _state = (int)CamStates.TRANSITION_CARDINAL;
                    break;
            }
        }

        internal CamStates GetState() {
            return (CamStates)_state;
        }
        internal void SetProtag(Protag protag)
        {
            _protag = protag;
        }

        internal void SetState(int state)
        {
            _state = state;

            switch(state)
            {
                default:
                    break;
                case (int)CamStates.TRANSITION_CARDINAL:
                    //cardinalTransitionTimer = 200;
                    break;
            }
        }
    }
}
