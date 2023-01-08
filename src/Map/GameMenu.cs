using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.System;

namespace OpenLaMulana.Entities
{
    public class GameMenu : IGameEntity
    {
        public int Depth { get; set; } = (int)Global.DrawOrder.Overlay;
        public Effect ActiveShader { get; set; } = null;
        private int _currentScreen;
        private TextManager _textManager;

        public GameMenu(Global.ScreenOverlayState screen, TextManager textManager)
        {
            _currentScreen = (int)screen;
            _textManager = textManager;
        }

        public void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

        }

        public void Update(GameTime gameTime)
        {

        }

    }
}