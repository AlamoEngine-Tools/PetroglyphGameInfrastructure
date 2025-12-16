using System;
using System.Collections.Generic;

namespace AET.Testing.Extensions;

public static class RandomExtensions
{
    private static readonly Random Random = new();
    private static readonly string AllowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";

    extension(Random)
    {
        public static bool Bool()
        {
            return Random.Next() % 2 == 0;
        }

        public static sbyte SByte()
        {
            return (sbyte)Random.Next(sbyte.MinValue, sbyte.MaxValue);
        }

        public static byte Byte()
        {
            return (byte)Random.Next(byte.MinValue, byte.MaxValue);
        }

        public static short Short()
        {
            return (short)Random.Next(short.MinValue, short.MaxValue);
        }

        public static ushort UShort()
        {
            return (ushort)Random.Next(ushort.MinValue, ushort.MaxValue);
        }

        public static int Int()
        {
            return Random.Next(int.MinValue, int.MaxValue);
        }

        public static uint UInt()
        {
            return (uint)Random.Int();
        }

        public static long Long()
        {
#if NET || NETSTANDARD2_1_OR_GREATER
            Span<byte> buf = stackalloc byte[8];
            Random.NextBytes(buf);
            return BitConverter.ToInt64(buf);
#else
            var buf = new byte[8];
            Random.NextBytes(buf);
            return BitConverter.ToInt64(buf, 0);
#endif
        }

        public static ulong ULong()
        {
            return (ulong)Random.Long();
        }

        public static T Enum<T>() where T : struct, Enum
        {
            var values = System.Enum.GetValues(typeof(T));
            return (T)values.GetValue(Random.Next(values.Length))!;
        }


        // From: https://stackoverflow.com/questions/648196/random-row-from-linq-to-sql/648240#648240
        public static T Item<T>(IEnumerable<T> items)
        {
            T current = default!;
            var count = 0;
            foreach (var element in items)
            {
                count++;
                if (Random.Next(count) == 0) 
                    current = element;
            }
            return count == 0 
                ? throw new InvalidOperationException("Sequence was empty") 
                : current;
        }

        public static unsafe string GetRandomStringOfLength(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException(nameof(length));

            var result = new string('\0', length);

            fixed (char* p = result)
            {
                var span = new Span<char>(p, length);
                for (var i = 0; i < span.Length; i++) 
                    span[i] = AllowedChars[Random.Next(0, AllowedChars.Length)];
            }

            return result;
        }
    }
}