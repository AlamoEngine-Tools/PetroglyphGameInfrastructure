using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.CommandLine;
using PG.TestingUtilities;

namespace PG.StarWarsGame.Infrastructure.Clients.Test.Arguments;

public class TestNamedArg(string name, string value, bool isDebug) : NamedArgument<string>(name, value, isDebug)
{
    public TestNamedArg(string name) : this(name, GameArgumentTestBase.ValidStringValue, false)
    {
    }

    public static TestNamedArg FromValue(string value)
    {
        var name = TestHelpers.GetRandom(GameArgumentNames.SupportedKeyValueArgumentNames);
        return new TestNamedArg(name, value, false);
    }
}

public class TestFlagArg(string name, bool value, bool dashed = false, bool debug = false)
    : FlagArgument(name, value, dashed, debug)
{
    public TestFlagArg(bool value, bool dashed) : this(
        TestHelpers.GetRandom(GameArgumentNames.SupportedFlagArgumentNames), value, dashed)
    {
    }

    public TestFlagArg(string name) : this(name, true)
    {
    }
}

public class LowerCaseNameArg()
    : FlagArgument(TestHelpers.GetRandom(GameArgumentNames.AllInternalSupportedArgumentNames).ToLowerInvariant(), true);


[SerializeEnumValue]
public enum MyEnumSerializeByValue
{
    A = 1,
    B = 2
}

public enum SomeRandomEnum
{
    Value
}