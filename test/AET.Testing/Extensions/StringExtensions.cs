// Copyright (c) Alamo Engine Tools and contributors. All rights reserved.
// Licensed under the MIT license. See LICENSE file in the project root for details.

using System;

namespace AET.Testing.Extensions;

/// <summary>
/// Provides extension methods for string manipulation and testing.
/// </summary>
public static class StringExtensions
{
    private static readonly Random Random = new();

    extension(string)
    {
        /// <summary>
        /// Randomly shuffles the casing of the characters in the specified string.
        /// </summary>
        /// <param name="input">The input string whose character casing will be shuffled.</param>
        /// <returns>A new string with randomly shuffled character casing.</returns>
        /// <exception cref="ArgumentNullException">Thrown if <paramref name="input"/> is <c>null</c>.</exception>
        public static unsafe string ShuffleCasing(string input)
        {
            if (input is null)
                throw new ArgumentNullException(nameof(input));

            if (input.Length == 0)
                return string.Empty;

            var buffer = input.Length <= 256
                ? stackalloc char[input.Length]
                : new char[input.Length];

            input.AsSpan().CopyTo(buffer);

            var rnd = Random;

            for (var i = 0; i < buffer.Length; i++)
            {
                var c = buffer[i];
                if (!char.IsLetter(c))
                    continue;

                if (rnd.Next(2) != 0)
                    continue;

                buffer[i] = char.IsUpper(c)
                    ? char.ToLower(c)
                    : char.ToUpper(c);
            }

#if NET
            return new string(buffer);
#else
            fixed (char* t = buffer)
                return new string(t, 0, buffer.Length);
#endif
        }
    }
}