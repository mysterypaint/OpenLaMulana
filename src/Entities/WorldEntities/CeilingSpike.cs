using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using OpenLaMulana.Entities;
using OpenLaMulana.Entities.WorldEntities.Parents;
using OpenLaMulana.Graphics;
using System;

namespace OpenLaMulana.Entities.WorldEntities
{
    internal class CeilingSpike : IRoomWorldEntity
    {
        public CeilingSpike(int x, int y, int op1, int op2, int op3, int op4, View destView) : base(x, y, op1, op2, op3, op4, destView)
        {
            UInt32[] pixels = new UInt32[16 * 16];
            pixels[0] = 0x000000FF;
            _tex = new Texture2D(Global.GraphicsDevice, 16, 16, false, SurfaceFormat.Color);
            _tex.SetData<UInt32>(pixels, 0, 16 * 16);

            _sprIndex = new Sprite(_tex, 48, 16, 16, 16);
        }
    }
}