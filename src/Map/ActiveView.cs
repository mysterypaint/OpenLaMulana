using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace OpenLaMulana.Entities
{
    public class ActiveView
    {
        private Field currField = null;
        private Texture2D currFieldTex = null;
        private View myView = null;
        public Vector2 Position { get; set; } = new Vector2(0, 0);
        public ActiveView() { }

        public View GetView()
        {
            return myView;
        }

        public void SetView(View v)
        {
            myView = v.CloneView();
        }

        internal void DrawView(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (myView == null || currFieldTex == null) return;
            myView.Draw(currFieldTex, spriteBatch, gameTime, Position);
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