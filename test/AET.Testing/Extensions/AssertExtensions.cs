using System;
using Xunit;

namespace AET.Testing.Extensions;

/// <summary>
/// Provides extension methods for the <see cref="Assert"/> class.
/// </summary>
public static class AssertExtensions
{
    extension(Assert)
    {
        /// <summary>
        /// Verifies that the specified action does not throw any exception.
        /// </summary>
        /// <typeparam name="T">The type of the result returned by the action.</typeparam>
        /// <param name="action">A delegate to the code to be tested.</param>
        /// <returns>The result of the executed test code.</returns>
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

        /// <summary>
        /// Verifies that the specified action does not throw any exception.
        /// </summary>
        /// <param name="action">A delegate to the code to be tested.</param>
        public static void AssertDoesNotThrowException(Action action)
        {
            Assert.AssertDoesNotThrowException(() => action);
        }
    }
}