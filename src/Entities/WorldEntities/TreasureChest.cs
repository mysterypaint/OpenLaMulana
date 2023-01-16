using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using OpenLaMulana.System;
using System.Collections.Generic;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class TreasureChest : IRoomWorldEntity
    {
        private Sprite _sprOpen, _sprClosed;

        public TreasureChest(int x, int y, int op1, int op2, int op3, int op4, bool spawnIsGlobal, View destView, List<ObjectStartFlag> startFlags) : base(x, y, op1, op2, op3, op4, spawnIsGlobal, destView, startFlags)
        {
            _tex = Global.TextureManager.GetTexture(Global.Textures.MAPG00);
            _sprOpen = new Sprite(_tex, 304, 16, 16, 16);
            _sprClosed = new Sprite(_tex, 304, 0, 16, 16);
            _sprIndex = _sprClosed;


            Global.GameFlags.InGameFlags[60] = true;
            Global.GameFlags.InGameFlags[794] = false;
            Global.GameFlags.InGameFlags[793] = true;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    break;
                case Global.WEStates.INIT:
                    _sprIndex.DrawScaled(spriteBatch, Position + new Vector2(0, Main.HUD_HEIGHT), _imgScaleX, _imgScaleY);
                    break;
            }
        }

        public override void Update(GameTime gameTime)
        {
            switch (State)
            {
                case Global.WEStates.UNSPAWNED:
                    if (HelperFunctions.EntityMaySpawn(_startFlags))
                        State = Global.WEStates.INIT;
                    break;
                case Global.WEStates.INIT:
                    break;
            }
        }
    }
}