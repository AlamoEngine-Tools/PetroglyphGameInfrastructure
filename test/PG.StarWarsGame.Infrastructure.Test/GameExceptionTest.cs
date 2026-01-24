using AnakinRaW.CommonUtilities.Testing.Extensions;
using PG.StarWarsGame.Infrastructure.Games;
using System;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test;

public static class GameExceptionTest
{
    [Fact]
    public static void Ctor()
    {
        var exception = new GameException();
        Assert.Exception(exception, validateMessage: false);
    }

    [Fact]
    public static void Ctor_String()
    {
        const string message = "game error";
        var exception = new GameException(message);
        Assert.Exception(exception, message: message);
    }

    [Fact]
    public static void Ctor_String_Exception()
    {
        const string message = "game error";
        var innerException = new Exception("Inner exception");
        var exception = new GameException(message, innerException);
        Assert.Exception(exception, innerException, message);
    }
}