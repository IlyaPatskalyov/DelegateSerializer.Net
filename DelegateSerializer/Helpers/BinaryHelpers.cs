namespace DelegateSerializer.Helpers
{
    public static class BinaryHelpers
    {
        public static int ReadInt16(this byte[] il, ref int position)
        {
            return il[position++] | il[position++] << 8;
        }

        public static ushort ReadUInt16(this byte[] il, ref int position)
        {
            return (ushort) (il[position++] | il[position++] << 8);
        }

        public static int ReadInt32(this byte[] il, ref int position)
        {
            return il[position++] | il[position++] << 8 | il[position++] << 16 | il[position++] << 24;
        }

        public static ulong ReadInt64(this byte[] il, ref int position)
        {
            return
                (ulong)
                (il[position++] | il[position++] << 8 | il[position++] << 16 | il[position++] << 24 | il[position++] |
                 il[position++] << 8 | il[position++] << 16 | il[position++] << 24);
        }

        public static double ReadDouble(this byte[] il, ref int position)
        {
            return il[position++] | il[position++] << 8 | il[position++] << 16 | il[position++] << 24 | il[position++] |
                   il[position++] << 8 | il[position++] << 16 | il[position++] << 24;
        }

        public static sbyte ReadSByte(this byte[] il, ref int position)
        {
            return (sbyte) il[position++];
        }

        public static byte ReadByte(this byte[] il, ref int position)
        {
            return il[position++];
        }

        public static float ReadFloat(this byte[] il, ref int position)
        {
            return il[position++] | il[position++] << 8 | il[position++] << 16 | il[position++] << 24;
        }
    }
}