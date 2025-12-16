using System;
using AET.Testing;
using PG.StarWarsGame.Infrastructure.Games;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public static class GameExceptionTest
{
    [Fact]
    public static void Ctor()
    {
        var exception = new GameException();
        ExceptionHelpers.ValidateExceptionProperties(exception, validateMessage: false);
    }

    [Fact]
    public static void Ctor_String()
    {
        var message = "game error";
        var exception = new GameException(message);
        ExceptionHelpers.ValidateExceptionProperties(exception, message: message);
    }

    [Fact]
    public static void Ctor_String_Exception()
    {
        var message = "game error";
        var innerException = new Exception("Inner exception");
        var exception = new GameException(message, innerException);
        ExceptionHelpers.ValidateExceptionProperties(exception, innerException: innerException, message: message);
    }
}