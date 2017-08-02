namespace PID_HSV.Util
{
    public class HSVOptions : ModelBase
    {
        private int _hue;

        public int Hue
        {
            get => _hue;
            set => SetProperty(ref _hue, value);
        }

        private double _saturation;

        public double Saturation
        {
            get => _saturation;
            set => SetProperty(ref _saturation, value);
        }

        private double _value;

        public double Value
        {
            get => _value;
            set => SetProperty(ref _value, value);
        }

        public HSVOptions()
        {
            _hue = 0;
            _saturation = 0;
            _value = 0;
        }

        public HSVOptions(int hue)
        {
            _hue = hue;
            _saturation = 0;
            _value = 0;
        }

        public HSVOptions(int hue, double saturation)
        {
            _hue = hue;
            _saturation = saturation;
            _value = 0;
        }

        public HSVOptions(int hue, double saturation, double value)
        {
            _hue = hue;
            _saturation = saturation;
            _value = value;
        }
    }
}