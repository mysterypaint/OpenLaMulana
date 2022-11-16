using Microsoft.Xna.Framework;

namespace OpenLaMulana.Entities
{
    public interface ICollidable
    {
        Rectangle CollisionBox { get; }

        short bBoxOriginX { get; set; }
        short bBoxOriginY { get; set; }
    }
}
