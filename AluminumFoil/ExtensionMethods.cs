using System;
using System.Text;

namespace ExtensionMethods
{
    public static class ArrayExtensions
    {
        public static T[] SubArray<T>(this T[] data, int index, int length)
        {
            T[] result = new T[length];
            Array.Copy(data, index, result, 0, length);
            return result;
        }

        public static string AsString(this byte[] data)
        {
            return Encoding.Default.GetString(data);
        }

        public static byte[] AsBytes(this string data)
        {
            return Encoding.ASCII.GetBytes(data);
        }
    }
}