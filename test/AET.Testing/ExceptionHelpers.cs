using System;
using Xunit;

namespace AET.Testing;

// Based on https://github.com/dotnet/runtime/blob/main/src/libraries/System.Runtime/tests/System.Runtime.Tests/System/Exception.Helpers.cs

public class ExceptionHelpers
{
    public static void ValidateExceptionProperties(Exception e,
        Exception? innerException = null,
        string? message = null,
        string? source = null,
        string? stackTrace = null,
        bool validateMessage = true)
    {
        Assert.Equal(innerException, e.InnerException);
        if (validateMessage)
        {
            Assert.Equal(message, e.Message);
        }
        else
        {
            Assert.NotNull(e.Message);
        }
        Assert.Equal(source, e.Source);
        Assert.Equal(stackTrace, e.StackTrace);
    }
}