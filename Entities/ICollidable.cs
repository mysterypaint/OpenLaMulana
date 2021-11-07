using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenLaMulana.Entities
{
    public interface ICollidable
    {
        Rectangle CollisionBox { get; }

        short bBoxOriginX { get; set; }
        short bBoxOriginY { get; set; }
    }
}
