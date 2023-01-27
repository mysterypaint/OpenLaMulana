using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities.Enemies.Guardians
{
    internal class GuardianPalenque : IGlobalWorldEntity
    {
        private int spritesMax = 65;
        private Sprite[] _sprites = new Sprite[65];
        private View _bossRoom, _bossDefeatedRoom = null;
        private int _ts = World.CHIP_SIZE;
        private GenericGlobalWorldEntity _upperBody, _lowerBody = null;

        enum PalenqueSprites
        {
            PalenqueWhole,
            PalenqueHitbox,
            SmallLightning_Yellow,
            BigLightning_Yellow,
            SmallLightning_Blue,
            BigLightning_Blue,
            SmallBladeParticle_White_Left,
            SmallBladeParticle_White_Up,
            SmallBladeParticle_White_Right,
            SmallBladeParticle_White_Down,
            BigBladeParticle_White_Left,
            BigBladeParticle_White_Up,
            BigBladeParticle_White_Right,
            BigBladeParticle_White_Down,
            SmallBladeParticle_Red_Left,
            SmallBladeParticle_Red_Up,
            SmallBladeParticle_Red_Right,
            SmallBladeParticle_Red_Down,
            BigBladeParticle_Red_Left,
            BigBladeParticle_Red_Up,
            BigBladeParticle_Red_Right,
            BigBladeParticle_Red_Down,
            LightBlueBullet_1,
            LightBlueBullet_2,
            DarkBlueBullet_1,
            DarkBlueBullet_2,
            Torpedo,
            TorpedoExplosion_1,
            TorpedoExplosion_2,
            BottomLeftPixel_1,
            BottomLeftPixel_2,
            SmallExplosion_1,
            SmallExplosion_2,
            ElectricSpray,
            GroundSkid_1,
            GroundSkid_2,
            GroundSkid_3,
            GroundSkid_4,
            PlaneModel,
            MidairSkid_1,
            MidairSkidAlt_1,
            MidairSkid_2,
            MidairSkidAlt_2,
            MidairSkid_3,
            MidairSkidAlt_3,
            MidairSkid_4,
            MidairSkidAlt_4,
            RockParticleTL,
            RockParticleTR,
            RockParticleBL,
            RockParticleBR,
            PillarSad,
            PillarSadistic,
            BrokenPillarTop,
            BrokenPillarBottom,
            BossExplosion_1,
            BossExplosion_2,
            BossExplosion_3,
            BossExplosion_4,
            GroundSkidAlt_1,
            GroundSkidAlt_2,
            GroundSkidAlt_3,
            GroundSkidAlt_4,
            PreBattleUpperBody,
            PreBattleLowerBody,
            MAX
        };

        public GuardianPalenque(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.SpriteDefManager.GetTexture(Global.SpriteDefs.BOSS05);
            for (var i = 0; i < spritesMax; i++)
            {
                _sprites[i] = Global.SpriteDefManager.GetSprite(Global.SpriteDefs.BOSS05, i);
            }
            _sprIndex = _sprites[(int)PalenqueSprites.PalenqueWhole];
            Position = new Vector2(7 * _ts, 4 * _ts);
            _bossRoom = Global.World.GetField(Global.World.CurrField).GetBossViews()[0];
            _bossDefeatedRoom = Global.World.GetField(Global.World.CurrField).GetBossViews()[1];

            _upperBody = (GenericGlobalWorldEntity)InstanceCreatePersistent(new GenericGlobalWorldEntity((int)Position.X, (int)Position.Y, 0, 0, 0, 0, true, null, null));
            _upperBody.SetSprite(_sprites[(int)PalenqueSprites.PreBattleUpperBody]);
            _upperBody.Position = new Vector2(Position.X, Position.Y - 6 * _ts);
            _lowerBody = (GenericGlobalWorldEntity)InstanceCreatePersistent(new GenericGlobalWorldEntity((int)Position.X, (int)Position.Y, 0, 0, 0, 0, true, null, null));
            _lowerBody.SetSprite(_sprites[(int)PalenqueSprites.PreBattleLowerBody]);
            _lowerBody.Position = new Vector2(Position.X, Position.Y + 6 * _ts);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            // Snap the actual drawing position to the closest 8x8 tile
            float posX = (float)Math.Round(Position.X % World.CHIP_SIZE) * World.CHIP_SIZE;
            float posY = Main.HUD_HEIGHT + (float)Math.Round(Position.Y / World.CHIP_SIZE) * World.CHIP_SIZE;
            _sprIndex.DrawScaled(spriteBatch, new Vector2(posX, posY), _imgScaleX, _imgScaleY);
        }

        public override void Update(GameTime gameTime)
        {
            int animeSpeed = 6;
            if (gameTime.TotalGameTime.Ticks % (animeSpeed * 6) == 0)
            {
                ShiftScreenLeft();
            }

            Position += new Vector2(0.01f, 0);
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