using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class RuinsTablet : IRoomWorldEntity
    {
        public RuinsTablet(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView)
        {
            UInt32[] pixels = new UInt32[16 * 16];
            pixels[0] = 0x00FF00FF;
            string textData = Global.TextManager.GetText(op1, Global.CurrLang);
            Sprite _tabletLeftImage, _tabletRightImage = null;


            if (op2 >= 0 || op3 >= 0)
            {
                _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
                if (op2 >= 0)
                    _tabletLeftImage = Global.TextureManager.Get64x64Tile(_tex, op2);
                if (op3 >= 0)
                    _tabletRightImage = Global.TextureManager.Get64x64Tile(_tex, op3);
            }

            //OP4 can specify a flag that turns on when reading an ancient document in the decryption state.
            //If you specify a flag number plus 10000, after the flag is turned on, the old document event itself
            //will be deleted from the view. This is because the flag change is determined only when the view is switched,
            //and the ancient document display is not a view switch, so it does not respond well to the start flag,
            //so it is used for ancient documents that you want to disappear on the spot, such as a sinking stone monument. To do.

            // ^^^ Heavily related to F13[1,1]... will look into this another time
            /// TODO: Investigate

            _tex = new Texture2D(Global.GraphicsDevice, 16, 16, false, SurfaceFormat.Color);
            _tex.SetData<UInt32>(pixels, 0, 16 * 16);
            
            _sprIndex = new Sprite(_tex, 48, 16, 16, 16);
            

            Position += new Vector2(0, -8);
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
        }
    }
}