using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class Kakoujuu : InteractableWorldEntity
    {
        private SpriteAnimation _walkingLeftAnim = null;
        private double _hsp = 0;
        private double _vsp = 0;
        private double _moveSpeed = 1;
        private int _facingDirection = -1;
        private View _currRoom = null;

        public Kakoujuu(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());

            _walkingLeftAnim = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(0, 16), 16, 16, new Point(16, 0), 4, 0.065f);
            HP = 1;

            _walkingLeftAnim.Play();
            _sprIndex = _walkingLeftAnim.CurrentFrame.Sprite;

            _currRoom = Global.World.GetCurrentView();


            _hsp = _moveSpeed * _facingDirection;
            _walkingLeftAnim.FlipX();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            if (Global.AnimationTimer.OneFrameElapsed())
            {
                if (World.TileIsSolid(World.TilePlaceMeeting(_currRoom, BBox, Position.X + _hsp, Position.Y)) > 0)
                {
                    while (!(World.TileIsSolid(World.TilePlaceMeeting(_currRoom, BBox, Position.X + Math.Sign(_hsp), Position.Y)) > 0))
                    {
                        Position += new Vector2(Math.Sign(_hsp), 0);
                    }
                    _hsp *= -1;
                    _walkingLeftAnim.FlipX();
                }
                Position += new Vector2((float)_hsp, 0);

                _walkingLeftAnim.Update(gameTime);
            }
            _sprIndex = _walkingLeftAnim.CurrentFrame.Sprite;
        }
    }
}