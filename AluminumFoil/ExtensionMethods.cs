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

    public static class UIint64Extensions
    {

        private static readonly string[] Suffixes = new string[] { "B", "KB", "MB", "GB", "TB" };

        public static string HumanSize(this ulong size)
        {
            var s = size;
            var suff = 0;
            while (s / 1024 > 0)
            {
                s /= 1024;
                suff++;
            }
            return s.ToString() + Suffixes[suff];
        }
    }
}