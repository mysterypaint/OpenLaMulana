﻿using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
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

        public enum CamStates {
            NONE,
            TRANSITION_CARDINAL,
            TRANSITION_PIXELATE_1,
            TRANSITION_PIXELATE_2,
            TRANSITION_WIPE,
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
            Transform = Matrix.CreateTranslation(new Vector3(-Position.X - (BaseWindowSize.X / 2), -Position.Y - (BaseWindowSize.Y / 2), 0)) *
                Matrix.CreateRotationZ(Rotation) *
                Matrix.CreateScale(new Vector3(DisplayZoomFactor, DisplayZoomFactor, 1)) *
                Matrix.CreateTranslation(new Vector3(_viewWidth * 0.5f, _viewHeight * 0.5f, 0));
            return Transform;
        }

        static float Lerp(float start_value, float end_value, float pct)
        {
            return (start_value + (end_value - start_value) * pct);
        }

        public void Update(GameTime gameTime)
        {

            switch (_state)
            {
                case (int)CamStates.NONE:
                    break;
                case (int)CamStates.TRANSITION_PIXELATE_1:
                    //_state = (int)CamStates.NONE;
                    break;
                case (int)CamStates.TRANSITION_CARDINAL:
                    float posX = Position.X;
                    _protag.Hsp = 0;
                    float newPlayerX = _protag.Position.X;

                    float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

                    if (_moveToX < 0)
                    {
                        float targetX = SCREEN_RIGHT_EDGE;
                        if (Position.X + _moveSpeedX > _moveToX)
                        {
                            posX += _moveSpeedX;

                            newPlayerX = Lerp(SCREEN_LEFT_EDGE, targetX, Math.Abs(Position.X) / Math.Abs(_moveToX)
                            );// Lerp(newPX, targetX, Math.Abs(Position.X) / Math.Abs(_moveToX));
                            _protag.Position = new Vector2((float)Math.Round(newPlayerX), _protag.Position.Y);
                        }
                        else
                        {
                            posX = 0;
                            _protag.Position = new Vector2(ROOM_PX_WIDTH + targetX, _protag.Position.Y);
                            _moveSpeedX = 0;
                            _moveToX = 0;
                            Global.World.UpdateCurrActiveView();
                            _state = (int)CamStates.NONE;
                        }
                    }
                    else if (_moveToX > 0)
                    {
                        float targetX = SCREEN_LEFT_EDGE;
                        if (Position.X + _moveSpeedX < _moveToX)
                        {
                            posX += _moveSpeedX;
                            _protag.Position = new Vector2(Lerp(ROOM_PX_WIDTH + SCREEN_RIGHT_EDGE, ROOM_PX_WIDTH + targetX, Math.Abs(Position.X) / Math.Abs(_moveToX)), _protag.Position.Y);
                        }
                        else
                        {
                            posX = 0;
                            _protag.Position = new Vector2(SCREEN_LEFT_EDGE, _protag.Position.Y);
                            _moveSpeedX = 0;
                            _moveToX = 0;
                            Global.World.UpdateCurrActiveView();
                            _state = (int)CamStates.NONE;
                        }
                    }




                    float posY = Position.Y;
                    _protag.Vsp = 0;
                    float newPlayerY = _protag.Position.Y;

                    if (_moveToY < 0)
                    {
                        float targetY = SCREEN_BOTTOM_EDGE;
                        if (Position.Y + _moveSpeedY > _moveToY)
                        {
                            posY += _moveSpeedY;

                            newPlayerY = Lerp(World.HUD_HEIGHT + Protag.SPRITE_HEIGHT, targetY, Math.Abs(Position.Y) / Math.Abs(_moveToY)
                            );// Lerp(newPX, targetY, Math.Abs(Position.Y) / Math.Abs(_moveToY));
                            _protag.Position = new Vector2(_protag.Position.X, (float)Math.Round(newPlayerY));
                        }
                        else
                        {
                            posY = 0;
                            _protag.Position = new Vector2(_protag.Position.X, ROOM_PX_HEIGHT + targetY);
                            _moveSpeedY = 0;
                            _moveToY = 0;
                            Global.World.UpdateCurrActiveView();
                            _state = (int)CamStates.NONE;
                        }
                    }
                    else if (_moveToY > 0)
                    {
                        float targetY = Protag.SPRITE_HEIGHT + World.HUD_HEIGHT;
                        if (Position.Y + _moveSpeedY < _moveToY - World.HUD_HEIGHT)
                        {
                            posY += _moveSpeedY;
                            _protag.Position = new Vector2(_protag.Position.X,
                                Lerp(ROOM_PX_HEIGHT, ROOM_PX_HEIGHT + targetY, Math.Abs(Position.Y) / Math.Abs(_moveToY))
                                );
                        }
                        else
                        {
                            posY = 0;
                            _protag.Position = new Vector2(_protag.Position.X, targetY);
                            _moveSpeedY = 0;
                            _moveToY = 0;
                            Global.World.UpdateCurrActiveView();
                            _state = (int)CamStates.NONE;
                        }
                    }

                    Position = new Vector2(posX, posY); //Global.InputController.DirMoveY
                    break;
            }
        }

        internal void UpdateMoveTarget(World.VIEW_DIR movingDirection)
        {
            switch (movingDirection)
            {
                case World.VIEW_DIR.LEFT:
                    _moveSpeedX = -8;
                    _moveSpeedY = 0;
                    _moveToX = -(World.ROOM_WIDTH * World.CHIP_SIZE);
                    _moveToY = 0;
                    break;
                case World.VIEW_DIR.DOWN:
                    _moveSpeedX = 0;
                    _moveSpeedY = 8;
                    _moveToX = 0;
                    _moveToY = ((World.ROOM_HEIGHT * World.CHIP_SIZE) * 1) + (World.HUD_HEIGHT);
                    break;
                case World.VIEW_DIR.RIGHT:
                    _moveSpeedX = 8;
                    _moveSpeedY = 0;
                    _moveToX = (World.ROOM_WIDTH * World.CHIP_SIZE);
                    _moveToY = 0;
                    break;
                case World.VIEW_DIR.UP:
                    _moveSpeedX = 0;
                    _moveSpeedY = -8;
                    _moveToX = 0;
                    _moveToY = -(World.ROOM_HEIGHT * World.CHIP_SIZE);
                    break;
            }

            _state = (int)CamStates.TRANSITION_CARDINAL;
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
