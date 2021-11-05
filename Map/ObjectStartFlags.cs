namespace OpenLaMulana
{
    internal class ObjectStartFlag
    {
        private int value = 0;
        private bool initiallyDisabled;

        public ObjectStartFlag(int _value, bool _initiallyDisabled)
        {
            this.value = _value;
            this.initiallyDisabled = _initiallyDisabled;
        }

        private bool initiallyEnabled { get; set; } = false;
    }
}