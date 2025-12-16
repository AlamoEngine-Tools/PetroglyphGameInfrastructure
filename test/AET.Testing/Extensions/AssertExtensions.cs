using System;
using Xunit;

namespace AET.Testing.Extensions;

public static class AssertExtensions
{
    extension(Assert)
    {
        public static T AssertDoesNotThrowException<T>(Func<T> action)
        {
            try
            {
                return action();
            }
            catch (Exception e)
            {
                Assert.Fail($"Expected no exception to be thrown but got '{e.GetType().Name}' instead");
                return default;
            }
        }

        public static void AssertDoesNotThrowException(Action action)
        {
            AssertDoesNotThrowException(() => action);
        }
    }
}