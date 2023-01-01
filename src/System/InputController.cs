using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;
using OpenLaMulana.Audio;
using OpenLaMulana.Entities;
using System;
using static OpenLaMulana.Entities.World;
using static OpenLaMulana.System.Camera;

namespace OpenLaMulana.System
{
    public class InputController
    {
        private Protag _protag;
        private Jukebox _jukebox;

        static KeyboardState t_keyboardState;
        static KeyboardState t_previousKeyboardState;

        bool _isBlocked = false;

        public short DirMoveX, DirMoveY = 0;

        public bool KeyJumpPressed { get; private set; }
        public bool KeyJumpHeld { get; private set; }
        public bool KeyWhipPressed { get; private set; }

        public InputController(Protag protag, Jukebox jukebox)
        {
            _protag = protag;
            _jukebox = jukebox;
        }

        public void ProcessControls(GameTime gameTime)
        {
            GetState();

            DirMoveX = (short)(Convert.ToInt16(KeyCheck(Keys.Right)) - Convert.ToInt16(KeyCheck(Keys.Left)));
            DirMoveY = (short)(Convert.ToInt16(KeyCheck(Keys.Down)) - Convert.ToInt16(KeyCheck(Keys.Up)));

            KeyJumpPressed = KeyPressed(Keys.Up);
            KeyJumpHeld = KeyCheck(Keys.Up);
            KeyWhipPressed = KeyPressed(Keys.Z);
            bool keyCancelPressed = KeyPressed(Keys.J);
            bool keyConfirmPressed = KeyPressed(Keys.K);

            WorldTransitionTesting();
            //JukeboxControls(keyCancelPressed, keyConfirmPressed);
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
                Global.World.FieldTransitionCardinal(VIEW_DIR.RIGHT);
            }
            else if (camMoveX == -1)
            {
                Global.World.FieldTransitionCardinal(VIEW_DIR.LEFT);
            }
            if (camMoveY == 1)
            {
                Global.World.FieldTransitionCardinal(VIEW_DIR.DOWN);
            }
            else if (camMoveY == -1)
            {
                Global.World.FieldTransitionCardinal(VIEW_DIR.UP);
            }

            if (KeyPressed(Keys.K))
            {
                var newVal = Global.World.CurrField + 1;
                if (newVal > Global.World.FieldCount - 1)
                    newVal = 0;
                Global.World.CurrField = newVal;
            }
            else if (KeyPressed(Keys.J))
            {
                var newVal = Global.World.CurrField - 1;
                if (newVal < 0)
                    newVal = Global.World.FieldCount - 1;
                Global.World.CurrField = newVal;
            }
            else if (KeyPressed(Keys.T))
            {
                //Global.Camera.SetState((int)CamStates.TRANSITION_PIXELATE_1);

                Global.World.FieldTransitionPixelate(0, 3, 0, 0);

                //Global.World.FieldTransitionPixelate(0, -1, 0, 0);
                //Global.World.FieldTransitionPixelate(1, 9, 3, 2);
            }
        }

        public static KeyboardState GetState()
        {
            t_previousKeyboardState = t_keyboardState;
            t_keyboardState = Keyboard.GetState();
            return t_keyboardState;
        }

        public static bool KeyPressed(Keys key)
        {
            return t_keyboardState.IsKeyDown(key) && !t_previousKeyboardState.IsKeyDown(key);
        }

        public static bool KeyReleased(Keys key)
        {
            return t_previousKeyboardState.IsKeyDown(key);
        }

        public static bool KeyCheck(Keys key)
        {
            return t_keyboardState.IsKeyDown(key);
        }
    }
}
