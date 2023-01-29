using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class MovingPlatform : ParentInteractableWorldEntity
    {
        private int _moveTimer = 0;
        private int _moveTimerReset = 180;

        public MovingPlatform(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            _sprIndex = new Sprite(_tex, 272, 160, 32, 16);

            Hsp = 0;
            Vsp = 1;
            MoveSpeed = 2;

            /*
             * VERTICAL FIELD PLATFORMS
OP2: [View #1] (2 digits), [View #2] (2 digits) ..... Top-left-most view is #0, (1,0) is #1, (0,1) is #4, etc...
OP3: Coord#1 (2 digits), Coord#2 (2 digits)

Coord#1 is associated with View #1
Coord#2 is associated with View #2

Turns around when Top-left tile overlaps with View #2
Turns around when Top-left tile bumps into the bottom coord (Coord#1) in View #1
            */


        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (IsGlobal)
            {
                _sprIndex.DrawScaled(spriteBatch, OriginPosition + Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
            } else
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            if (Global.Camera.GetState() != System.Camera.CamStates.NONE)
                return;
            if (_moveTimer <= 0)
            {
                _moveTimer = _moveTimerReset;
                Vsp *= -1;
            }
            else
                _moveTimer--;

            Position += new Vector2(0, Vsp * MoveSpeed);

        }
    }
}