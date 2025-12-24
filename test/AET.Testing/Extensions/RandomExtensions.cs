// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;
using System.Collections.Generic;

namespace AET.Testing.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="System.Random"/> class.
/// </summary>
public static class RandomExtensions
{
    private static readonly Random Random = new();
    private static readonly string AllowedChars = "ABCDEFGHJKLMNOPQRSTUVWXYZabcdefghijkmnopqrstuvwxyz0123456789!@$?_-";

    extension(Random)
    {
        /// <summary>
        /// Generates a random <see cref="bool"/> value.
        /// </summary>
        /// <returns>
        /// A randomly generated <see cref="bool"/> value.
        /// </returns>
        public static bool Bool()
        {
            return Random.Next() % 2 == 0;
        }

        /// <summary>
        /// Generates a random <see cref="sbyte"/> value.
        /// </summary>
        /// <returns>
        /// A randomly generated <see cref="sbyte"/> value within the range of <see cref="sbyte.MinValue"/> to <see cref="sbyte.MaxValue"/>.
        /// </returns>
        public static sbyte SByte()
        {
            return (sbyte)Random.Next(sbyte.MinValue, sbyte.MaxValue);
        }

        /// <summary>
        /// Generates a random <see cref="byte"/> value.
        /// </summary>
        /// <returns>
        /// A randomly generated <see cref="byte"/> value within the range of <see cref="byte.MinValue"/> to <see cref="byte.MaxValue"/>.
        /// </returns>
        public static byte Byte()
        {
            return (byte)Random.Next(byte.MinValue, byte.MaxValue);
        }

        /// <summary>
        /// Generates a random <see cref="short"/> value.
        /// </summary>
        /// <returns>
        /// A randomly generated <see cref="short"/> value within the range of <see cref="short.MinValue"/> to <see cref="short.MaxValue"/>.
        /// </returns>
        public static short Short()
        {
            return (short)Random.Next(short.MinValue, short.MaxValue);
        }

        /// <summary>
        /// Generates a random <see cref="ushort"/> value.
        /// </summary>
        /// <returns>
        /// A randomly generated <see cref="ushort"/> value within the range of <see cref="ushort.MinValue"/> to <see cref="ushort.MaxValue"/>.
        /// </returns>
        public static ushort UShort()
        {
            return (ushort)Random.Next(ushort.MinValue, ushort.MaxValue);
        }

        /// <summary>
        /// Generates a random <see cref="int"/> value.
        /// </summary>
        /// <remarks>
        /// In contrast to <see cref="Random.Next()"/>, this method can return the full range of <see cref="int"/> values, including negative values.
        /// </remarks>
        /// <returns>
        /// A randomly generated <see cref="int"/> value within the range of <see cref="int.MinValue"/> to <see cref="int.MaxValue"/>.
        /// </returns>
        public static int Int()
        {
            return Random.Next(int.MinValue, int.MaxValue);
        }

        /// <summary>
        /// Generates a random <see cref="uint"/> value.
        /// </summary>
        /// <returns>
        /// A randomly generated <see cref="uint"/> value within the range of <see cref="uint.MinValue"/> to <see cref="uint.MaxValue"/>.
        /// </returns>
        public static uint UInt()
        {
            return (uint)Random.Int();
        }

        /// <summary>
        /// Generates a random <see cref="long"/> value.
        /// </summary>
        /// <returns>
        /// A randomly generated <see cref="long"/> value within the range of <see cref="long.MinValue"/> to <see cref="long.MaxValue"/>.
        /// </returns>
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

        /// <summary>
        /// Generates a random <see cref="ulong"/> value.
        /// </summary>
        /// <returns>
        /// A randomly generated <see cref="ulong"/> value within the range of <see cref="ulong.MinValue"/> to <see cref="ulong.MaxValue"/>.
        /// </returns>
        public static ulong ULong()
        {
            return (ulong)Random.Long();
        }

        /// <summary>
        /// Returns a random value from the enumeration type <typeparamref name="T"/>.
        /// </summary>
        /// <typeparam name="T">
        /// The enumeration type from which a random value will be selected. Must be a struct and an <see cref="System.Enum"/>.
        /// </typeparam>
        /// <returns>
        /// A randomly selected value from the enumeration type <typeparamref name="T"/>.
        /// </returns>
        /// <exception cref="ArgumentException">
        /// Thrown if <typeparamref name="T"/> is not an enumeration type.
        /// </exception>
        public static T Enum<T>() where T : struct, Enum
        {
            var values =
#if NET5_0_OR_GREATER
                System.Enum.GetValues<T>();
#else
                (T[])System.Enum.GetValues(typeof(T));
#endif
            return values[Random.Next(values.Length)];
        }


        // From: https://stackoverflow.com/questions/648196/random-row-from-linq-to-sql/648240#648240
        /// <summary>
        /// Selects a random item from the provided sequence of items.
        /// </summary>
        /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
        /// <param name="items">The sequence of items to select from.</param>
        /// <returns>A randomly selected item from the sequence.</returns>
        /// <exception cref="InvalidOperationException">Thrown if the sequence is empty.</exception>
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

        /// <summary>
        /// Generates a random string of the specified length using a mix of letters (any case), numbers and special characters
        /// </summary>
        /// <param name="length">The desired length of the generated string. Must be a non-negative value.</param>
        /// <returns>A randomly generated string of the specified length. Returns an empty string if <paramref name="length"/> is 0.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is less than 0.</exception>
        public static string String(int length)
        {
            return Random.String(length, AllowedChars);
        }

        /// <summary>
        /// Generates a random string of the specified length using the specified pool of characters.
        /// </summary>
        /// <param name="length">The desired length of the generated string. Must be a non-negative value.</param>
        /// <param name="charPool">The pool of characters to pick random characters from.</param>
        /// <returns>A randomly generated string of the specified length. Returns an empty string if <paramref name="length"/> is 0.</returns>
        /// <exception cref="ArgumentOutOfRangeException">Thrown when <paramref name="length"/> is less than 0.</exception>
        /// <exception cref="ArgumentException">Thrown when <paramref name="charPool"/> is empty or <see langword="null"/>.</exception>
        public static unsafe string String(int length, ReadOnlySpan<char> charPool)
        {
            if (charPool == ReadOnlySpan<char>.Empty || charPool.IsEmpty)
                throw new ArgumentException("charPool must not be null or empty", nameof(charPool));
            switch (length)
            {
                case < 0:
                    throw new ArgumentOutOfRangeException(nameof(length));
                case 0:
                    return string.Empty;
            }

            var buffer = length <= 256
                ? stackalloc char[length]
                : new char[length];

            var random = Random;
            for (var i = 0; i < buffer.Length; i++)
            {
                var index = random.Next(charPool.Length);
                buffer[i] = charPool[index];
            }

#if NET
            return new string(buffer);
#else
            fixed (char* t = buffer)
                return new string(t, 0, length);
#endif
        }
    }
}