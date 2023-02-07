using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System.Collections.Generic;

namespace OpenLaMulana.Entities
{
    internal class SubWeaponParticle : ParentInteractableWorldEntity
    {
        SpriteAnimation _subWeaponAnim;

        public SubWeaponParticle(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            HitboxWidth = 8;
            HitboxHeight = 8;
            State = Global.WEStates.ACTIVATING;
            _tex = Global.TextureManager.GetTexture(Global.Textures.PROT1);
            Depth = (int)Global.DrawOrder.ProtagWeaponParticles;

            switch ((Global.SubWeapons)op1)
            {
                case Global.SubWeapons.SHURIKEN:
                    //subWeaponSprites = new Sprite[] { Global.TextureManager.Get8x8Tile(_tex, 24, 4, Vector2.Zero), Global.TextureManager.Get8x8Tile(_tex, 25, 4, Vector2.Zero) };
                    _subWeaponAnim = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(192, 32), 8, 8, new Point(8, 0), 2, 0.03f);
                    break;
                case Global.SubWeapons.THROWING_KNIFE:
                    _subWeaponAnim = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(272, 16), 8, 8, new Point(8, 8), 4, 0.03f, 2);
                    break;
                case Global.SubWeapons.SPEAR:
                    _subWeaponAnim = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(288, 16), 16, 16, new Point(0, 0), 1, 0.0f);
                    break;
                case Global.SubWeapons.FLARES:
                    _subWeaponAnim = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(272, 32), 16, 16, new Point(16, 0), 2, 0.1f);
                    break;
                case Global.SubWeapons.BOMB:
                    _subWeaponAnim = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(240, 32), 8, 8, new Point(8, 8), 8, 0.02f, 4);
                    break;
                case Global.SubWeapons.PISTOL:
                    int xOff = 0;
                    if (Global.Protag.FacingX < 0)
                        xOff = 32;

                    _subWeaponAnim = SpriteAnimation.CreateSimpleAnimation(_tex, new Point(240 + xOff, 48), 8, 8, new Point(8, 0), 4, 0.2f, 4);
                    break;
            }

            Position = RelativeViewChipPos;
            _subWeaponAnim.Play();
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.ACTIVATING:
                case Global.WEStates.ACTIVE:
                    _subWeaponAnim.Draw(spriteBatch, Position + new Vector2(0, World.HUD_HEIGHT));
                    break;
                case Global.WEStates.DYING:
                    break;
                case Global.WEStates.DELETED:
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch(State)
            {
                case Global.WEStates.ACTIVATING:
                    _subWeaponAnim.Update(gameTime);
                    break;
                case Global.WEStates.ACTIVE:
                    _subWeaponAnim.Update(gameTime);
                    break;
                case Global.WEStates.DYING:
                    break;
                case Global.WEStates.DELETED:
                    break;
            }
        }
    }
}