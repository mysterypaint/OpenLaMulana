using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class BackgroundSigil : IRoomWorldEntity
    {
        public BackgroundSigil(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            // These seem to be the same value?
            // 0 - Origin, 1 - Birth, 2 - Life, 3 - Death
            int sigilType = op1;
            int graphicType = op2;

            _tex = Global.TextureManager.GetTexture(Global.World.GetCurrMapTexture());
            _sprIndex = new Sprite(_tex, 112 + ((World.CHIP_SIZE * 2) * sigilType), 0, 16, 16);
        }
    }
}