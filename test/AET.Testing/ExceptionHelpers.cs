using System;
using Xunit;

namespace AET.Testing;

// Based on https://github.com/dotnet/runtime/blob/main/src/libraries/System.Runtime/tests/System.Runtime.Tests/System/Exception.Helpers.cs

/// <summary>
/// Provides helper methods for validating properties of exceptions in unit tests.
/// </summary>
public static class ExceptionHelpers
{
    /// <summary>
    /// Validates the properties of the specified <see cref="Exception"/> instance against the provided values.
    /// </summary>
    /// <param name="e">The exception instance to validate.</param>
    /// <param name="innerException">The expected inner exception of <paramref name="e"/>. Defaults to <c>null</c>.</param>
    /// <param name="message">The expected message of <paramref name="e"/>. Defaults to <c>null</c>.</param>
    /// <param name="source">The expected source of <paramref name="e"/>. Defaults to <c>null</c>.</param>
    /// <param name="stackTrace">The expected stack trace of <paramref name="e"/>. Defaults to <c>null</c>.</param>
    /// <param name="validateMessage">A value indicating whether to validate the <paramref name="message"/> property.</param>
    public static void ValidateExceptionProperties(Exception e,
        Exception? innerException = null,
        string? message = null,
        string? source = null,
        string? stackTrace = null,
        bool validateMessage = true)
    {
        Assert.Equal(innerException, e.InnerException);
        if (validateMessage)
            Assert.Equal(message, e.Message);
        else
            Assert.NotNull(e.Message);
        Assert.Equal(source, e.Source);
        Assert.Equal(stackTrace, e.StackTrace);
    }
}