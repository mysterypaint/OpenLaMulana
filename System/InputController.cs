using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using OpenLaMulana.Entities;

namespace OpenLaMulana.System
{
    public class InputController
    {

        private bool _isBlocked;
        private Protag _trex;
        private World _world;

        static KeyboardState keyboardState;
        static KeyboardState _previousKeyboardState;

        public InputController(Protag trex, World world)
        {
            _trex = trex;
            _world = world;
        }

        public void ProcessControls(GameTime gameTime)
        {
            GetState();

            WorldTransitionTesting();


            if(!_isBlocked)
            {

                if(KeyPressed(Keys.Up) || KeyPressed(Keys.Space))
                {

                    if (_trex.State != TrexState.Jumping)
                        _trex.BeginJump();

                }
                else if (_trex.State == TrexState.Jumping && (KeyReleased(Keys.Up) || KeyReleased(Keys.Space)))
                {

                    _trex.CancelJump();

                }
                else if (keyboardState.IsKeyDown(Keys.Down))
                {

                    if (_trex.State == TrexState.Jumping || _trex.State == TrexState.Falling)
                        _trex.Drop();
                    else
                        _trex.Duck();

                }
                else if (_trex.State == TrexState.Ducking && !keyboardState.IsKeyDown(Keys.Down))
                {

                    _trex.GetUp();

                }

            }

            _isBlocked = false;

        }

        public void BlockInputTemporarily()
        {
            _isBlocked = true;
        }

        private void WorldTransitionTesting()
        {
            int camMoveX = Convert.ToInt32(KeyPressed(Keys.D)) - Convert.ToInt32(KeyPressed(Keys.A));
            int camMoveY = Convert.ToInt32(KeyPressed(Keys.S)) - Convert.ToInt32(KeyPressed(Keys.W));

            if (camMoveX == 1)
            {
                _world.FieldTransitionRight();
            }
            else if (camMoveX == -1)
            {
                _world.FieldTransitionLeft();
            }
            if (camMoveY == 1)
            {
                _world.FieldTransitionDown();
            }
            else if (camMoveY == -1)
            {
                _world.FieldTransitionUp();
            }
        }

        public static KeyboardState GetState()
        {
            _previousKeyboardState = keyboardState;
            keyboardState = Keyboard.GetState();
            return keyboardState;
        }

        public static bool KeyPressed(Keys key)
        {
            return keyboardState.IsKeyDown(key) && !_previousKeyboardState.IsKeyDown(key);
        }

        public static bool KeyReleased(Keys key)
        {
            return _previousKeyboardState.IsKeyDown(key);
        }

        public static bool KeyCheck(Keys key)
        {
            return keyboardState.IsKeyDown(key);
        }
    }
}
