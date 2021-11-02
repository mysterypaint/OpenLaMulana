using System;
namespace OpenLaMulana.Entities
{
    public class FieldObject
    {
        private bool FieldEvent { get; set; } = false;                      // Flag identifying whether or not this object will modify Field tiles with matching OP arguments
        private int EventNumber { get; set; } = -1;                         // Event ID
        private int X { get; set; } = -1;                                   // X-Position, relative to current Field (Divide all values by 2048)
        private int Y { get; set; } = -1;                                   // Y-Position, relative to current Field (Divide all values by 2048)
        private int[] OP = { -1, -1, -1, -1};                               // Object-specific arguments
        private int StartFlagsCount = 0;                                    // The number of Conditions that must be met before this object may spawn
        private int[] StartFlags = { 0, 0, 0, 0 };                          // The conditions which must be met before this object may spawn
        private bool[] StartFlagsDisabled = { false, false, false, false }; // Which conditions act; Disabled if true

        public FieldObject()
        {

        }
    }
}
