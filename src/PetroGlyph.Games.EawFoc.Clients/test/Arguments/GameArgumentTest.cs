using Moq;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Clients.Test.Arguments;

public class GameArgumentTest
{
    [Fact]
    public void TestArgValid()
    {
        var validator = new Mock<IArgumentValidator>();
        var arg = new Argument("valid");

        var valid = arg.IsValid(validator.Object, out var reason);

        Assert.True(valid);
        Assert.Equal(ArgumentValidityStatus.Valid, reason);
    }

    [Fact]
    public void TestArgInvalid()
    {
        var validator = new Mock<IArgumentValidator>();
        var arg = new Argument("valid");

        validator.Setup(v =>
                v.CheckArgument(It.IsAny<IGameArgument>(), out It.Ref<string>.IsAny, out It.Ref<string>.IsAny))
            .Returns(ArgumentValidityStatus.IllegalCharacter);

        var valid = arg.IsValid(validator.Object, out var reason);

        Assert.False(valid);
        Assert.Equal(ArgumentValidityStatus.IllegalCharacter, reason);
    }

    [Fact]
    public void TestArgInvalidCustom()
    {
        var validator = new Mock<IArgumentValidator>();
        var arg = new Argument("invalid");

        var valid = arg.IsValid(validator.Object, out var reason);

        Assert.False(valid);
        Assert.Equal(ArgumentValidityStatus.InvalidData, reason);
    }


    private class Argument : GameArgument<string>
    {
        public Argument(string value, bool isDebug = false) : base(value, isDebug)
        {
        }

        public override ArgumentKind Kind { get; }
        public override string Name { get; }

        protected override bool IsDataValid()
        {
            if (Value == "valid")
                return true;
            return false;
        }

        public override string ValueToCommandLine()
        {
            throw new System.NotImplementedException();
        }

        public override bool Equals(IGameArgument? other)
        {
            throw new System.NotImplementedException();
        }
    }
}