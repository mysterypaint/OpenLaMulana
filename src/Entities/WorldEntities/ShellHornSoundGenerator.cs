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
    internal class ShellHornSoundGenerator : ParentWorldEntity
    {
        private Protag _protag = Global.Protag;
        private bool _switchActivated = false;
        private int[] _checkingFlags = null;

        public ShellHornSoundGenerator(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            //_tex = Global.TextureManager.GetTexture(Global.World.GetCurrEveTexture());
            //_sprIndex = null;// new Sprite(_tex, 288, 128, 16, 8);

            _checkingFlags = new int[] { op1, op2, op3, op4 };

            State = Global.WEStates.UNSPAWNED;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            /*
            if (Global.DevModeEnabled)
            {
                _sprIndex.DrawScaled(spriteBatch, OriginPosition + new Vector2(0, World.HUD_HEIGHT), 0.5f, 0.5f);
            }*/
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    bool _alreadyActivated = false;
                    foreach (int flag in _checkingFlags)
                    {
                        if (flag < 0)
                            continue;
                        if (!Global.GameFlags.InGameFlags[flag])
                        {
                            _alreadyActivated = true;
                            break;
                        }
                    }
                    if (HelperFunctions.EntityMaySpawn(StartFlags) && _alreadyActivated)
                    {
                        State = Global.WEStates.IDLE;
                    }
                    break;
                case Global.WEStates.IDLE:
                    bool _triggerNow = true;
                    foreach(int flag in _checkingFlags)
                    {
                        if (flag < 0)
                            continue;
                        if (!Global.GameFlags.InGameFlags[flag])
                        {
                            _triggerNow = false;
                            break;
                        }
                    }
                    if (_triggerNow)
                    {
                        State = Global.WEStates.ACTIVATING;

                        if (Global.Inventory.ObtainedTreasures[Global.ObtainableTreasures.SHELL_HORN])
                        {
                            Global.AudioManager.PlaySFX(SFX.PUZZLE_SOLVED);
                        }
                    }
                    break;
                case Global.WEStates.ACTIVATING:
                    break;
            }
        }
    }
}