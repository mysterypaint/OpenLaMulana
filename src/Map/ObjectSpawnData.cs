using System.Collections.Generic;

namespace OpenLaMulana
{
    internal class ObjectSpawnData
    {
        private int _eventNumber;
        private int _x;
        private int _y;
        private int _OP1;
        private int _OP2;
        private int _OP3;
        private int _OP4;
        private bool _isAFieldObject;
        private List<ObjectStartFlag> startFlags;

        public ObjectSpawnData(int eventNumber, int x, int y, int OP1, int OP2, int OP3, int OP4, bool isAFieldObject)
        {
            _eventNumber = eventNumber;
            _x = x;
            _y = y;
            _OP1 = OP1;
            _OP2 = OP2;
            _OP3 = OP3;
            _OP4 = OP4;
            _isAFieldObject = isAFieldObject;

            startFlags = new List<ObjectStartFlag>();
        }

        internal void AddStartFlag(int value, bool initiallyDisabled)
        {
            ObjectStartFlag newStartFlag = new ObjectStartFlag(value, initiallyDisabled);
            startFlags.Add(newStartFlag);
        }
    }
}