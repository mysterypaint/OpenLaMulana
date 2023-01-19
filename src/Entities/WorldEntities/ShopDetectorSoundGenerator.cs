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
    internal class ShopDetectorSoundGenerator : IGlobalWorldEntity
    {
        private Protag _protag = Global.Protag;
        private bool _switchActivated = false;
        private int[] _checkingFlags = null;

        public ShopDetectorSoundGenerator(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            //_tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            _sprIndex = null;// new Sprite(_tex, 288, 128, 16, 8);

            _checkingFlags = new int[] { op1, op2, op3, op4 };

            State = Global.WEStates.UNSPAWNED;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    bool _conditionMet = false;
                    foreach (int flag in _checkingFlags)
                    {
                        if (flag < 0)
                            continue;
                        if (Global.GameFlags.InGameFlags[flag])
                        {
                            _conditionMet = true;
                            break;
                        }
                    }
                    if (HelperFunctions.EntityMaySpawn(StartFlags) && _conditionMet)
                    {
                        Global.AudioManager.PlaySFX(SFX.DETECTOR_SHOP_NEARBY);
                        State = Global.WEStates.IDLE;
                    }
                    break;
                case Global.WEStates.IDLE:
                    break;
            }
        }
    }
}