using System;
using PG.StarWarsGame.Infrastructure.Games;
using PG.TestingUtilities;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public static class GameExceptionTest
{
    [Fact]
    public static void Ctor()
    {
        var exception = new GameException();
        ExceptionTest.AssertException(exception, validateMessage: false);
    }

    [Fact]
    public static void Ctor_String()
    {
        var message = "game error";
        var exception = new GameException(message);
        ExceptionTest.AssertException(exception, message: message);
    }

    [Fact]
    public static void Ctor_String_Exception()
    {
        var message = "game error";
        var innerException = new Exception("Inner exception");
        var exception = new GameException(message, innerException);
        ExceptionTest.AssertException(exception, innerException: innerException, message: message);
    }
}