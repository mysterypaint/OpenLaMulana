using MeltySynth;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Entities.WorldEntities.MovingPlatform;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class MovingPlatform : ParentInteractableWorldEntity
    {
        //private int _moveTimer = 0;
        //private int _moveTimerReset = 180;
        private Point[] _boundaryViews = null;
        private int[] _boundaryOffsets;
        private MPlatTypes _platType = MPlatTypes.UNDEFINED;

        public int HspDir { get; private set; } = 0;
        public int VspDir { get; private set; } = 0;

        internal enum MPlatTypes : int
        {
            UNDEFINED = -1,
            F_UP,
            F_DOWN,
            V_UP,
            V_DOWN,
            V_RIGHT,
            V_LEFT,
            MAX
        };

        internal enum MPlatIndex : int
        {
            START_POINT,
            END_POINT,
            MAX
        };

        /// <summary>
        /// Two types of moving floor can be specified: field event and view event. Specify the form of the moving bed in OP1.
        /// 0: Field up/down ↑ 1: Field up/down ↓ 2: View up/down ↑ 3: View up/down ↓ 4: View left/right → 5: View left/right ←
        /// Field type moves across screens, View type moves within one screen to hold.The arrow is the initial movement direction.
        /// 
        /// For OP2 and OP3, specify the moving range of the moving floor.
        /// The specification method differs depending on the form of OP1.
        /// The field type is 2 digits of the view number + coordinates within the view, and the view type specifies
        /// only the coordinates within the view.For top and bottom, specify the top edge for OP2 and the bottom edge for OP3.
        /// 
        /// For left and right, specify the left edge for OP2 and the right edge for OP3.
        /// 
        /// In the case of the field type, malfunction will occur if the view number that goes up and down is shifted.
        /// 
        /// OP4 specifies the speed of the moving bed.
        /// 2 is a common speed. Only up to 2 can be specified for top and bottom types.
        /// </summary>
        /// <param name="x"></param>
        /// <param name="y"></param>
        /// <param name="op1"></param>
        /// <param name="op2"></param>
        /// <param name="op3"></param>
        /// <param name="op4"></param>
        /// <param name="spawnIsGlobal"></param>
        /// <param name="destView"></param>
        /// <param name="startFlags"></param>
        public MovingPlatform(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            _sprIndex = new Sprite(_tex, 272, 160, 32, 16);
            int[] viewBounds;
            Field currField;
            View startingView, endingView;
            MoveSpeed = op4;

            _platType = (MPlatTypes)op1;

            switch (_platType)
            {
                case MPlatTypes.F_UP:
                    viewBounds = HelperFunctions.SeparateDigits(op2, new int[] { 2, 2 });
                    _boundaryOffsets = HelperFunctions.SeparateDigits(op3, new int[] { 2, 2 });
                    currField = Global.World.GetCurrField();
                    startingView = currField.GetView(viewBounds[(int)MPlatIndex.START_POINT]);
                    endingView = currField.GetView(viewBounds[(int)MPlatIndex.END_POINT]);
                    _boundaryViews = new Point[] { new Point(startingView.X, startingView.Y), new Point(endingView.X, endingView.Y) };
                    HspDir = 0;
                    VspDir = -1;
                    MoveSpeed = Math.Clamp(MoveSpeed, -2, 2);
                    break;
                case MPlatTypes.F_DOWN:
                    viewBounds = HelperFunctions.SeparateDigits(op2, new int[] { 2, 2 });
                    _boundaryOffsets = HelperFunctions.SeparateDigits(op3, new int[] { 2, 2 });
                    currField = Global.World.GetCurrField();
                    startingView = currField.GetView(viewBounds[(int)MPlatIndex.START_POINT]);
                    endingView = currField.GetView(viewBounds[(int)MPlatIndex.END_POINT]);
                    _boundaryViews = new Point[] { new Point(startingView.X, startingView.Y), new Point(endingView.X, endingView.Y) };
                    HspDir = 0;
                    VspDir = 1;
                    MoveSpeed = Math.Clamp(MoveSpeed, -2, 2);
                    break;
                case MPlatTypes.V_UP:
                    _boundaryOffsets = new int[] { op2, op3 };
                    HspDir = 0;
                    VspDir = -1;
                    break;
                case MPlatTypes.V_DOWN:
                    _boundaryOffsets = new int[] { op2, op3 };
                    HspDir = 0;
                    VspDir = 1;
                    break;
                case MPlatTypes.V_RIGHT:
                    _boundaryOffsets = new int[] { op2, op3 };
                    HspDir = 1;
                    VspDir = 0;
                    break;
                case MPlatTypes.V_LEFT:
                    _boundaryOffsets = new int[] { op2, op3 };
                    HspDir = -1;
                    VspDir = 0;
                    break;
            }

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

            Vector2 currPosition = TrueSpawnCoord + OriginDisplacement + Position;
            Point currViewCoords = new Point((int)(currPosition.X / ts / World.VIEW_WIDTH) % World.FIELD_WIDTH, ((int)(currPosition.Y / ts / World.VIEW_HEIGHT)) % World.FIELD_HEIGHT);

            /*
            Vector2 finalPosition = OriginPosition + OriginDisplacement + Position + new Vector2(Hsp, Vsp);
            Point currRelativeRoom = new Point(SourceDestView.X, SourceDestView.Y) + new Point((int)(((finalPosition.X / ts) / World.VIEW_WIDTH) - Global.World.CurrViewX) % World.FIELD_WIDTH, (int)(((finalPosition.Y / ts) / World.VIEW_HEIGHT) - Global.World.CurrViewY) % World.FIELD_HEIGHT);
            */
            Point currRelativeTile = new Point((int)(currPosition.X / ts) % World.VIEW_WIDTH, (int)(currPosition.Y / ts) % World.VIEW_HEIGHT);
            
            if (IsGlobal)
            {
                // Handle Field Platforms' Collision Code
                switch (_platType)
                {
                    case MPlatTypes.F_UP:
                    case MPlatTypes.F_DOWN:
                        if (currViewCoords == _boundaryViews[(int)MPlatIndex.START_POINT])
                        {
                            if (currRelativeTile.Y <= _boundaryOffsets[(int)MPlatIndex.START_POINT])
                            {
                                //Position.Y = (_boundaryOffsets[(int)MPlatIndex.START_POINT] + 1) * ts;
                                VspDir *= -1;
                            }
                        }
                        else if (currViewCoords == _boundaryViews[(int)MPlatIndex.END_POINT])
                        {
                            if (currRelativeTile.Y >= _boundaryOffsets[(int)MPlatIndex.END_POINT])
                            {
                                //Position.Y = (_boundaryOffsets[(int)MPlatIndex.END_POINT]) * ts;
                                VspDir *= -1;
                            }
                        }
                        break;
                }
            } else
            {
                // Handle View Platforms' Collision Code

                    //_boundaryOffsets

                switch (_platType)
                {
                    case MPlatTypes.V_LEFT:
                    case MPlatTypes.V_RIGHT:
                        if (currRelativeTile.X <= _boundaryOffsets[(int)MPlatIndex.START_POINT])
                        {
                            //Position.X = (_boundaryOffsets[(int)MPlatIndex.START_POINT] + 1) * ts;
                            HspDir *= -1;
                        }
                        else if (currRelativeTile.X >= _boundaryOffsets[(int)MPlatIndex.END_POINT])
                        {
                            //Position.X = (_boundaryOffsets[(int)MPlatIndex.END_POINT]) * ts;
                            HspDir *= -1;
                        }
                        break;
                    case MPlatTypes.V_UP:
                    case MPlatTypes.V_DOWN:
                        if (currRelativeTile.Y <= _boundaryOffsets[(int)MPlatIndex.START_POINT])
                        {
                            //Position.Y = (_boundaryOffsets[(int)MPlatIndex.START_POINT] + 1) * ts;
                            VspDir *= -1;
                        }
                        else if (currRelativeTile.Y >= _boundaryOffsets[(int)MPlatIndex.END_POINT])
                        {
                            //Position.Y = (_boundaryOffsets[(int)MPlatIndex.END_POINT]) * ts;
                            VspDir *= -1;
                        }
                        break;
                }
            }
            /*
             * 
                HspDir *= -1;
                VspDir *= -1;*/

            Hsp = HspDir * MoveSpeed;
            Vsp = VspDir * MoveSpeed;
            Position += new Vector2(Hsp, Vsp);

        }
    }
}