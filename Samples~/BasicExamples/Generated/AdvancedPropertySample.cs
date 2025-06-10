namespace easycodegenunity.Editor.Samples.BasicExamples.Generated
{
    public class AdvancedPropertyDemo
    {
        public int SimpleProperty { get; set; }

        public string ReadMostlyProperty { get; private set; }

        private double _backingValue;
        public double BackedProperty { get => _backingValue; set => _backingValue = value; }

        private int _internalCounter;
        public int Counter { get => _internalCounter; set => _internalCounter = value < 0 ? 0 : value; }

        public bool IsValid
        {
            get
            {
                if (SimpleProperty < 0)
                return false;
                if (string.IsNullOrEmpty(ReadMostlyProperty))
                return false;
                return true;
            }
        }

        private int _validatedValue;
        public int ValidatedValue
        {
            get => _validatedValue;
            set
            {
                if (value < 0)
                {
                _validatedValue = 0;
                return;
                 }
                if (value > 100)
                {
                _validatedValue = 100;
                return;
                 }
                _validatedValue = value;
            }
        }
    }
}