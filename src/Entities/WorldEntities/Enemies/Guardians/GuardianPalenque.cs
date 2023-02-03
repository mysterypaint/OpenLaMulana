using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities.Enemies.Guardians
{
    internal class GuardianPalenque : ParentGuardianEntity
    {
        private View _bossRoom, _bossDefeatedRoom = null;
        private int _ts = World.CHIP_SIZE;
        private TemplateWorldEntity _upperBody, _lowerBody = null;

        enum PalenqueSprites
        {
            PALENQUE_WHOLE,
            PALENQUE_HITBOX,
            SMALL_LIGHTNING_YELLOW,
            BIG_LIGHTNING_YELLOW,
            SMALL_LIGHTNING_BLUE,
            BIG_LIGHTNING_BLUE,
            SMALL_BLADE_PARTICLE_WHITE_LEFT,
            SMALL_BLADE_PARTICLE_WHITE_UP,
            SMALL_BLADE_PARTICLE_WHITE_RIGHT,
            SMALL_BLADE_PARTICLE_WHITE_DOWN,
            BIG_BLADE_PARTICLE_WHITE_LEFT,
            BIG_BLADE_PARTICLE_WHITE_UP,
            BIG_BLADE_PARTICLE_WHITE_RIGHT,
            BIG_BLADE_PARTICLE_WHITE_DOWN,
            SMALL_BLADE_PARTICLE_RED_LEFT,
            SMALL_BLADE_PARTICLE_RED_UP,
            SMALL_BLADE_PARTICLE_RED_RIGHT,
            SMALL_BLADE_PARTICLE_RED_DOWN,
            BIG_BLADE_PARTICLE_RED_LEFT,
            BIG_BLADE_PARTICLE_RED_UP,
            BIG_BLADE_PARTICLE_RED_RIGHT,
            BIG_BLADE_PARTICLE_RED_DOWN,
            LIGHT_BLUE_BULLET_1,
            LIGHT_BLUE_BULLET_2,
            DARK_BLUE_BULLET_1,
            DARK_BLUE_BULLET_2,
            TORPEDO,
            TORPEDO_EXPLOSION_1,
            TORPEDO_EXPLOSION_2,
            BOTTOM_LEFT_PIXEL_1,
            BOTTOM_LEFT_PIXEL_2,
            SMALL_EXPLOSION_1,
            SMALL_EXPLOSION_2,
            ELECTRIC_SPRAY,
            GROUND_SKID_1,
            GROUND_SKID_2,
            GROUND_SKID_3,
            GROUND_SKID_4,
            PLANE_MODEL,
            MIDAIR_SKID_1,
            MIDAIR_SKID_ALT_1,
            MIDAIR_SKID_2,
            MIDAIR_SKID_ALT_2,
            MIDAIR_SKID_3,
            MIDAIR_SKID_ALT_3,
            MIDAIR_SKID_4,
            MIDAIR_SKID_ALT_4,
            ROCK_PARTICLE_TL,
            ROCK_PARTICLE_TR,
            ROCK_PARTICLE_BL,
            ROCK_PARTICLE_BR,
            PILLAR_SAD,
            PILLAR_SADISTIC,
            BROKEN_PILLAR_TOP,
            BROKEN_PILLAR_BOTTOM,
            BOSS_EXPLOSION_1,
            BOSS_EXPLOSION_2,
            BOSS_EXPLOSION_3,
            BOSS_EXPLOSION_4,
            GROUND_SKID_ALT_1,
            GROUND_SKID_ALT_2,
            GROUND_SKID_ALT_3,
            GROUND_SKID_ALT_4,
            PRE_BATTLE_UPPER_BODY,
            PRE_BATTLE_LOWER_BODY,
            MAX
        };

        public GuardianPalenque(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags, Global.SpriteDefs sprSheetIndex) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            InitAssembly(sprSheetIndex);
            _sprIndex = _mySprites[(int)PalenqueSprites.PALENQUE_WHOLE];

            Position = new Vector2(7 * _ts, 4 * _ts);
            _bossRoom = Global.World.GetField(Global.World.CurrField).GetBossViews()[0];
            _bossDefeatedRoom = Global.World.GetField(Global.World.CurrField).GetBossViews()[1];

            _upperBody = (TemplateWorldEntity)InstanceCreatePersistent(new TemplateWorldEntity((int)Position.X, (int)Position.Y, 0, 0, 0, 0, true, null, null));
            _upperBody.SetSprite(_mySprites[(int)PalenqueSprites.PRE_BATTLE_UPPER_BODY]);
            _upperBody.Position = new Vector2(Position.X, Position.Y - 6 * _ts);
            _lowerBody = (TemplateWorldEntity)InstanceCreatePersistent(new TemplateWorldEntity((int)Position.X, (int)Position.Y, 0, 0, 0, 0, true, null, null));
            _lowerBody.SetSprite(_mySprites[(int)PalenqueSprites.PRE_BATTLE_LOWER_BODY]);
            _lowerBody.Position = new Vector2(Position.X, Position.Y + 6 * _ts);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Snap the actual drawing position to the closest 8x8 tile
            float posX = (float)Math.Round(Position.X % World.CHIP_SIZE) * World.CHIP_SIZE;
            float posY = (float)Math.Round(Position.Y / World.CHIP_SIZE) * World.CHIP_SIZE;

            if (Global.DevModeEnabled)
            {
                if (CollidesWithPlayer(new Vector2(posX, posY), false))
                {
                    if (_sprIndex.TintColor != Color.Red)
                        Global.AudioManager.PlaySFX(SFX.SHIELD_BLOCK);
                    _sprIndex.TintColor = Color.Red;
                }
                else
                {
                    _sprIndex.TintColor = Color.White;
                }
            }

            _sprIndex.DrawScaled(spriteBatch, new Vector2(posX, posY + Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            int animeSpeed = 6;
            if (gameTime.TotalGameTime.Ticks % (animeSpeed * 6) == 0)
            {
                ShiftScreenLeft();
            }

            Position += new Vector2(0.01f, 0);


            if (InputManager.ButtonCheckPressed30FPS(Global.ControllerKeys.JUMP))
            {
                _sprDefIndex++;
                if (_sprDefIndex >= _spritesMax)
                    _sprDefIndex = 0;

                UpdateSpriteIndex();
                UpdateMaskIndex();
            }
        }

        private void ShiftScreenLeft()
        {
            // Grab the left-most column of the boss arena
            Chip[] leftMostColumn = new Chip[World.ROOM_HEIGHT];
            for (int y = 0; y < World.ROOM_HEIGHT; y++)
            {
                leftMostColumn[y] = _bossRoom.Chips[0, y];
            }

            // Shift every single tile in the room toward the left; The left-most column will be written on the far right of the room, effectively wrapping the screen
            _bossRoom.ShiftTiles(World.VIEW_DIR.LEFT, leftMostColumn);
        }
    }
}