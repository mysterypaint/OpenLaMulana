using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Entities.WorldEntities.NPCRoom;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class Dais : InteractableWorldEntity
    {
        private bool _daisPlaced = false;
        private Sprite _weightSprite = null;
        private Protag _protag = Global.Protag;
        public override int HitboxWidth { get; set; } = 16;
        public override int HitboxHeight { get; set; } = 8;

        public Dais(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            //_tex = Global.TextureManager.MakeTexture(16, 8, new Vector4(0, 255, 0, 255));
            //_sprIndex = new Sprite(_tex, 0, 8, 16, 8);
            _tex = Global.TextureManager.GetTexture(Textures.ITEM);
            _weightSprite = Global.TextureManager.Get8x8Tile(_tex, 20, 3, Vector2.Zero);
        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            //_sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
            //Rectangle offBox = new Rectangle(BBox.X, BBox.Y + World.HUD_HEIGHT, BBox.Width, BBox.Height);
            //HelperFunctions.DrawRectangle(spriteBatch, offBox, Color.Green);

            if (_daisPlaced)
                _weightSprite.Draw(spriteBatch, new Vector2(Position.X + 4, Position.Y + 8));
        }

        public override void Update(GameTime gameTime)
        {
            if (CollidesWithPlayer())
            {
                if (InputManager.PressedKeys[(int)ControllerKeys.DOWN] && !_daisPlaced && _protag.Inventory.Weights > 0)
                {
                    _daisPlaced = true;
                    Global.AudioManager.PlaySFX(SFX.SE11);
                    _protag.Inventory.Weights--;
                }
            }
        }
    }
}