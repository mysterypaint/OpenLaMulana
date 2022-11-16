namespace OpenLaMulana
{
    internal class ObjectStartFlag
    {
        private int value = 0;
        private bool initiallyDisabled;

        public ObjectStartFlag(int _value, bool _initiallyDisabled)
        {
            value = _value;
            initiallyDisabled = _initiallyDisabled;
        }

        private bool initiallyEnabled { get; set; } = false;
    }
}