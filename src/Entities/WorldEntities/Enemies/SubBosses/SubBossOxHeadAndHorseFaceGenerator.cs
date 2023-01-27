using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System;
using System.Collections.Generic;
using static OpenLaMulana.Global;
using static OpenLaMulana.System.Camera;

namespace OpenLaMulana.Entities.WorldEntities.Enemies
{
    internal class SubBossOxHeadAndHorseFaceGenerator : IRoomWorldEntity
    {
        private View _currView = null;
        private int _flagToSetWhenDefeated = -1;

        public SubBossOxHeadAndHorseFaceGenerator(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());

            _flagToSetWhenDefeated = op1;

            _sprIndex = null;
            _currView = Global.World.GetCurrentView();

            if (HelperFunctions.EntityMaySpawn(StartFlags))
            {
                State = WEStates.IDLE;
                Global.AudioManager.PlaySFX(SFX.ROOM_LOCKDOWN);
            }
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    break;
                case WEStates.IDLE:
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(StartFlags))
                    {
                        State = WEStates.IDLE;
                        Global.AudioManager.PlaySFX(SFX.ROOM_LOCKDOWN);
                    }
                    break;
                case WEStates.IDLE:
                    break;
            }
        }
    }
}