using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class ObtainableMajorWeapon : InteractableWorldEntity
    {
        private int _itemID;
        /*
        (int)Global.HardCodedText.ITEM_ACQUISITION_MESSAGE
        (int)Global.HardCodedText.ITEM_NAMES_BEGIN
        (int)Global.HardCodedText.ITEM_NAMES_END
        */

        public ObtainableMajorWeapon(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.ITEM);
            _itemID = op1;
            int texOffX = (_itemID % 4) * 16;
            int texOffY = (int)Math.Floor((double)_itemID / 4) * 16;
            _sprIndex = new Sprite(_tex, 256 + texOffX, 0 + texOffY, 16, 16);
            HP = 1;
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