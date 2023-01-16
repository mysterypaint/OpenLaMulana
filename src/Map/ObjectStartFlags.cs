namespace OpenLaMulana
{
    public class ObjectStartFlag
    {
        private int _flagIndex = -1;
        private bool _conditionMetIfFlagIsOn;

        public ObjectStartFlag(int flagIndex, bool conditionMetIfFlagIsOn)
        {
            _flagIndex = flagIndex;
            _conditionMetIfFlagIsOn = conditionMetIfFlagIsOn;
        }

        public int GetIndex()
        {
            return _flagIndex;
        }

        public bool GetFlagCondition()
        {
            return _conditionMetIfFlagIsOn;
        }
    }
}