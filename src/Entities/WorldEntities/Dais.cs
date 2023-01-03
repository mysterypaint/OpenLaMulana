using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class Dais : IRoomWorldEntity
    {
        public Dais(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            _tex = Global.TextureManager.MakeTexture(16, 8, new Vector4(0, 255, 0, 255));
            _sprIndex = new Sprite(_tex, 0, 8, 16, 8);
        }
    }
}