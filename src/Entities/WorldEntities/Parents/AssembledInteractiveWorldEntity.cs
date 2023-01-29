using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using OpenLaMulana.System;
using OpenLaMulana.Graphics;
using static OpenLaMulana.Global;

namespace OpenLaMulana.Entities.WorldEntities.Parents
{
    abstract class AssembledInteractiveWorldEntity : InteractableWorldEntity
    {
        public new virtual int HP { get; set; } = 2;
        internal Protag _protag = Global.Protag;
        public new virtual int HitboxWidth { get; set; } = 8;
        public new virtual int HitboxHeight { get; set; } = 8;
        public new World.ChipTypes CollisionBehavior { get; internal set; } = World.ChipTypes.VOID;
        public new View SourceDestView = null;
        public new bool LockTo30FPS { get; set; } = true;
        public new virtual int Hsp { get; set; } = 0;
        public new virtual int Vsp { get; set; } = 0;
        public new virtual int MoveSpeed { get; set; } = 0;
        internal List<SpriteDef> _mySpriteDef = null;
        internal SpriteDef _maskIndex = null;
        internal int _sprDefIndex = 0;
        internal int _spritesMax = -1;
        internal Sprite[] _mySprites = null;
        internal List<Rectangle> _collisionAssemblyData = null;
        private int _ts = World.CHIP_SIZE;

        protected AssembledInteractiveWorldEntity(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            SourceDestView = destView;
        }

        internal void InitAssembly(SpriteDefs sprSheetIndex)
        {
            int spritesMax = Global.SpriteDefManager.GetDefSheetSize(Global.SpriteDefs.BOSS02);
            _tex = Global.SpriteDefManager.GetTexture(sprSheetIndex);
            _mySprites = new Sprite[spritesMax];
            _spritesMax = spritesMax;

            for (var i = 0; i < spritesMax; i++)
            {
                _mySprites[i] = Global.SpriteDefManager.GetSprite(sprSheetIndex, i);
            }
            _mySpriteDef = Global.SpriteDefManager.GetDefSheet(sprSheetIndex);

            UpdateSpriteIndex();
            UpdateMaskIndex();
        }

        internal void UpdateMaskIndex()
        {
            _sprIndex = _mySprites[_sprDefIndex];

        }

        internal void UpdateSpriteIndex()
        {
            _maskIndex = _mySpriteDef[_sprDefIndex];
            _collisionAssemblyData = _maskIndex.GetAssemblyData();
        }

        public virtual Rectangle BBox
        {
            get
            {
                Rectangle box = new Rectangle(
                    (int)Math.Round(Position.X - BBoxOriginX),
                    (int)Math.Round(Position.Y - BBoxOriginY),
                    HitboxWidth,
                    HitboxHeight
                );
                //box.Inflate(-COLLISION_BOX_INSET, -COLLISION_BOX_INSET);
                return box;
            }
        }

        public bool CollidesWithPlayer(Vector2 offset = default, bool wholeRectangle = false)
        {
            if (wholeRectangle)
            {
                //return BBox.Intersects(_protag.BBox);
                return HelperFunctions.CollisionRectangle(BBox, _protag.BBox);
            }
            else
            {
                if (_collisionAssemblyData == null)
                    return false;

                foreach(Rectangle rect in _collisionAssemblyData)
                {
                    Rectangle checkingRect = new Rectangle(rect.X * _ts, rect.Y * _ts, rect.Width * _ts, rect.Height * _ts);
                    checkingRect.Offset(offset);
                    if (HelperFunctions.CollisionRectangle(checkingRect, _protag.BBox))
                        return true;
                }
                return false;
            }
        }
    }
}