using Microsoft.Xna.Framework;

namespace OpenLaMulana.Entities
{
    public interface ICollidable
    {
        Rectangle CollisionBox { get; }

        short BBoxOriginX { get; set; }
        short BBoxOriginY { get; set; }
    }
}
