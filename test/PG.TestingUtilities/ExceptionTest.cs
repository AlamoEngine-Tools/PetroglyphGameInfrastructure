using System;

namespace PG.TestingUtilities;

public class ExceptionTest
{
    public static void AssertException(Exception e,
        Exception? innerException = null,
        string? message = null,
        string? stackTrace = null,
        bool validateMessage = true)
    {
        Assert.Equal(innerException, e.InnerException);
        if (validateMessage)
            Assert.Equal(message, e.Message);
        else
            Assert.NotNull(e.Message);
        Assert.Equal(stackTrace, e.StackTrace);
    }
}