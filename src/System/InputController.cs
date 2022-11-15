using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Text;
using OpenLaMulana.Entities;
using static OpenLaMulana.Entities.World;
using OpenLaMulana.Audio;

namespace OpenLaMulana.System
{
    public class InputController
    {
        private Protag _protag;
        private World _world;
        private Jukebox _jukebox;

        static KeyboardState keyboardState;
        static KeyboardState _previousKeyboardState;

        bool _isBlocked = false;

        public short dirMoveX, dirMoveY = 0;

        public bool keyJumpPressed { get; private set; }
        public bool keyJumpHeld { get; private set; }
        public bool keyWhipPressed { get; private set; }

        public InputController(Protag protag, World world, Jukebox jukebox)
        {
            _protag = protag;
            _world = world;
            _jukebox = jukebox;
        }

        public void ProcessControls(GameTime gameTime)
        {
            GetState();

            dirMoveX = (short)(Convert.ToInt16(KeyCheck(Keys.Right)) - Convert.ToInt16(KeyCheck(Keys.Left)));
            dirMoveY = (short)(Convert.ToInt16(KeyCheck(Keys.Down)) - Convert.ToInt16(KeyCheck(Keys.Up)));

            keyJumpPressed = KeyPressed(Keys.Up);
            keyJumpHeld = KeyCheck(Keys.Up);
            keyWhipPressed = KeyPressed(Keys.Z);
            bool keyCancelPressed = KeyPressed(Keys.J);
            bool keyConfirmPressed = KeyPressed(Keys.K);


            //WorldTransitionTesting();
            JukeboxControls(keyCancelPressed, keyConfirmPressed);
        }

        private void JukeboxControls(bool keyCancelPressed, bool keyConfirmPressed)
        {

            int selectionMoveX = Convert.ToInt32(KeyPressed(Keys.D)) - Convert.ToInt32(KeyPressed(Keys.A));
            _jukebox.Control(selectionMoveX, keyCancelPressed, keyConfirmPressed);
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
                _world.FieldTransition(VIEW_DIR.RIGHT);
            }
            else if (camMoveX == -1)
            {
                _world.FieldTransition(VIEW_DIR.LEFT);
            }
            if (camMoveY == 1)
            {
                _world.FieldTransition(VIEW_DIR.DOWN);
            }
            else if (camMoveY == -1)
            {
                _world.FieldTransition(VIEW_DIR.UP);
            }

            if (KeyPressed(Keys.K))
            {
                var newVal = _world.currField + 1;
                if (newVal > _world.fieldCount - 1)
                    newVal = 0;
                _world.currField = newVal;
            }
            else if (KeyPressed(Keys.J))
            {
                var newVal = _world.currField - 1;
                if (newVal < 0)
                    newVal = _world.fieldCount - 1;
                _world.currField = newVal;
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
