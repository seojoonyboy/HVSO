using System.Runtime.InteropServices;

namespace G.Util
{
    [StructLayout(LayoutKind.Explicit)]
    struct DoubleKey
    {
        [FieldOffset(0)]
        public long Key;

        [FieldOffset(0)]
        public int Sub1;

        [FieldOffset(4)]
        public int Sub2;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct TripleKey
    {
        [FieldOffset(0)]
        public long Key;

        [FieldOffset(0)]
        public int Sub1;

        [FieldOffset(4)]
        public short Sub2;

        [FieldOffset(6)]
        public short Sub3;
    }

    [StructLayout(LayoutKind.Explicit)]
    struct FourKey
    {
        [FieldOffset(0)]
        public long Key;

        [FieldOffset(0)]
        public short Sub1;

        [FieldOffset(2)]
        public short Sub2;

        [FieldOffset(4)]
        public short Sub3;

        [FieldOffset(6)]
        public short Sub4;
    }

}
