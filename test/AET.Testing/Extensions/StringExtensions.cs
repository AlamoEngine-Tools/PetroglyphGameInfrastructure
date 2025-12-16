using System;

namespace AET.Testing.Extensions;

public static class StringExtensions
{
    private static readonly Random Random = new();

    extension(string)
    {
        public static string ShuffleCasing(string input)
        {
            var characters = input.ToCharArray();

            for (var i = 0; i < characters.Length; i++)
            {
                if (char.IsLetter(characters[i]))
                {
                    if (Random.Next(2) == 0)
                    {
                        characters[i] = char.IsUpper(characters[i])
                            ? char.ToLower(characters[i])
                            : char.ToUpper(characters[i]);
                    }
                }
            }

            return new string(characters);
        }
    }
}