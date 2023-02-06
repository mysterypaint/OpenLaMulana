using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class MovingPlatform : ParentInteractableWorldEntity
    {
        private int _moveTimer = 0;
        private int _moveTimerReset = 180;
        public int HspDir { get; private set; } = 0;
        public int VspDir { get; private set; } = -1;

        public MovingPlatform(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            _sprIndex = new Sprite(_tex, 272, 160, 32, 16);

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


            HitboxWidth = 32;
            HitboxHeight = 16;
            IsCollidable = true;

            Hsp = HspDir * MoveSpeed;
            Vsp = VspDir * MoveSpeed;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            if (IsGlobal)
            {
                Vector2 finalVec = OriginPosition + OriginDisplacement + Position;
                Vector2 finalPos = new Vector2(finalVec.X % (World.FIELD_WIDTH * World.VIEW_WIDTH * World.CHIP_SIZE), finalVec.Y % (World.FIELD_HEIGHT * World.VIEW_HEIGHT * World.CHIP_SIZE));

                _sprIndex.DrawScaled(spriteBatch, finalPos + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);

                // Visually, draw a second copy on the opposite side of the map, in case the camera is wrapping around the bounds of the map.
                // This prevents the object from visually disappearing for a moment while the camera is busy wrapping around
                _sprIndex.DrawScaled(spriteBatch, OriginPosition + Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
            }
            else
                _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            if (Global.Camera.GetState() != System.Camera.CamStates.NONE)
                return;

            int ts = World.CHIP_SIZE;
            int viewWidthPx = World.VIEW_WIDTH * ts;
            int viewHeightPx = World.VIEW_HEIGHT * ts;

            OriginDisplacement = Vector2.Zero;

            // If the player wrapped around the map, wrap the global entities around the map, too

            Vector2 currEntityPos = OriginPosition + Position;
            Point currEntityRoom = new Point((int)(currEntityPos.X / viewWidthPx), (int)(currEntityPos.Y / viewHeightPx));

            if (currEntityRoom.X >= World.FIELD_WIDTH)
                OriginDisplacement += new Vector2(0, -(World.FIELD_WIDTH * viewWidthPx));
            else if (currEntityRoom.X < 0)
                OriginDisplacement += new Vector2(0, (World.FIELD_WIDTH * viewWidthPx));

            if (currEntityRoom.Y >= World.FIELD_HEIGHT)
                OriginDisplacement += new Vector2(0, -(World.FIELD_HEIGHT * viewHeightPx));
            else if (currEntityRoom.Y < 0)
                OriginDisplacement += new Vector2(0, (World.FIELD_HEIGHT * viewHeightPx));



            if (_moveTimer <= 0)
            {
                _moveTimer = _moveTimerReset;
                HspDir *= -1;
                VspDir *= -1;
                Hsp = HspDir * MoveSpeed;
                Vsp = VspDir * MoveSpeed;
            }
            else
                _moveTimer--;

            Position += new Vector2(Hsp, Vsp);
        }
    }
}