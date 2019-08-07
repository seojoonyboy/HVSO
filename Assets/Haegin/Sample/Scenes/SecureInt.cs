namespace Haegin
{
    public struct SecureInt
    {
        private int _value;
        private int _backupValue;
        const int _xor = 0x3D2BA55D;

        public static int RotateLeft(int value, int count)
        {
            uint val = (uint)value;
            return (int)((val << count) | (val >> (32 - count)));
        }

        public SecureInt(int value)
        {
            _value = value;
            _backupValue = RotateLeft(_value, 11) ^ _xor;
        }

        public int Value
        {
            get
            {
                if ((RotateLeft(_value, 11) ^ _xor) != _backupValue)
                {
                    // 메모리 해킹?   
                    return 0;
                }
                return _value;
            }
            set
            {
                _value = value;
                _backupValue = RotateLeft(_value, 11) ^ _xor;
            }
        }

        static public implicit operator SecureInt(int value)
        {
            return new SecureInt(value);
        }

        static public implicit operator int(SecureInt value)
        {
            return value.Value;
        }

        static public implicit operator string(SecureInt value)
        {
            return value.ToString();
        }

        override public string ToString()
        {
            return _value.ToString();
        }
    }
}
