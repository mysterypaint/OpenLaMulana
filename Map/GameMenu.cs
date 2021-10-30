using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.System;
using static OpenLaMulana.Entities.World;

namespace OpenLaMulana.Entities
{
    internal class GameMenu : IGameEntity
    {
        public int DrawOrder => -160;

        private int _currentScreen;
        private TextManager _textManager;

        public GameMenu(ScreenOverlayState screen, TextManager textManager)
        {
            _currentScreen = (int) screen;
            _textManager = textManager;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _textManager.DrawText(0, 0, "Hello world! I hope you are, well!¥10¥20¥21");
        }

        public void Update(GameTime gameTime)
        {
            
        }

    }
}