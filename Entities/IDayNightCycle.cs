using Microsoft.Xna.Framework;

namespace OpenLaMulana.Entities
{
    public interface IDayNightCycle
    {
        int NightCount { get; }
        bool IsNight { get; }

        Color ClearColor { get; }

    }
}
