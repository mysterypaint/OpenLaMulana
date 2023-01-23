using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;

namespace OpenLaMulana.Entities
{
    public class ActiveView
    {
        private Field currField = null;
        private Texture2D currFieldTex = null;
        private View currView = null;
        public Vector2 Position { get; set; } = new Vector2(0, 0);
        public ActiveView() { }

        public View GetView()
        {
            return currView;
        }
        public void SetView(View v)
        {
            currView = v.CloneView();
        }

        internal void DrawView(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (currView == null || currFieldTex == null) return;
            currView.Draw(currFieldTex, spriteBatch, gameTime, Position);
        }

        internal void SetField(Field field)
        {
            currField = field;
        }

        internal void SetFieldTex(Texture2D tex)
        {
            currFieldTex = tex;
        }

        internal Texture2D GetFieldTex()
        {
            return currFieldTex;
        }

        internal Field GetField()
        {
            return currField;
        }
    }
}