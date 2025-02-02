using System;
using System.Collections.Generic;
using AET.Modinfo.Model;
using AET.Modinfo.Spec;
using PG.StarWarsGame.Infrastructure.Clients.Arguments;
using PG.StarWarsGame.Infrastructure.Clients.Arguments.GameArguments;
using PG.StarWarsGame.Infrastructure.Games;
using PG.StarWarsGame.Infrastructure.Mods;
using PG.StarWarsGame.Infrastructure.Testing.Game.Installation;
using PG.StarWarsGame.Infrastructure.Testing.Mods;
using PG.StarWarsGame.Infrastructure.Testing.TestBases;
using Testably.Abstractions.Testing;
using Xunit;

namespace PG.StarWarsGame.Infrastructure.Test.Clients.Arguments;

public class GameArgumentsBuilderTest : CommonTestBase
{
    private readonly MockFileSystem _fileSystem = new();
    private readonly GameArgumentsBuilder _builder = new();
    private readonly IGame _game;

    public GameArgumentsBuilderTest()
    {
        _game = _fileSystem.InstallGame(new GameIdentity(GameType.Foc, GamePlatform.SteamGold), ServiceProvider);
    }

    [Fact]
    public void NullArgument_ThrowsArgumentNullException()
    {
        var mod = CreateMod("MyMod");
        Assert.Throws<ArgumentNullException>(() => _builder.Add(null!));
        Assert.Throws<ArgumentNullException>(() => _builder.AddSingleMod(null!));
        Assert.Throws<ArgumentNullException>(() => _builder.AddSingleMod(mod, null!));
        Assert.Throws<ArgumentNullException>(() => _builder.AddSingleMod(null!, _game.Directory));
        Assert.Throws<ArgumentNullException>(() => _builder.AddMods(null!));
        Assert.Throws<ArgumentNullException>(() => _builder.AddMods([], null!));
        Assert.Throws<ArgumentNullException>(() => _builder.AddMods(null!, _game.Directory));
        Assert.Throws<ArgumentNullException>(() => _builder.Remove((string)null!));
        Assert.Throws<ArgumentNullException>(() => _builder.Remove((GameArgument)null!));

        Assert.Throws<ArgumentException>(() => _builder.AddMods([mod, null!], _game.Directory));
        Assert.Throws<ArgumentException>(() => _builder.AddMods([mod, null!]));
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Add_ModArgument_ThrowsNotSupportedException(bool isWorkshop)
    {
        var modArgument = new ModArgument(_fileSystem.DirectoryInfo.New("123456"), _fileSystem.DirectoryInfo.New("game"), isWorkshop);
        Assert.Throws<NotSupportedException>(() => _builder.Add(modArgument));
    }

    [Fact]
    public void Add_ModArgumentList_ThrowsNotSupportedException()
    {
        Assert.Throws<NotSupportedException>(() => _builder.Add(ModArgumentList.Empty));
    }

    [Fact]
    public void AddMods_EmptyList_Succeeds()
    {
        var result = _builder.AddMods(new List<IMod>());
        Assert.Same(_builder, result);
    }

    [Fact]
    public void RemoveByName_EmptyName_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => _builder.Remove(string.Empty));
    }

    [Fact]
    public void RemoveMods_Succeeds()
    {
        var result = _builder.RemoveMods();
        Assert.Same(_builder, result);
    }

    [Fact]
    public void Add_ArgumentAdded_BuildContainsArgument()
    {
        var argument = new WindowedArgument();
        _builder.Add(argument);

        var result = _builder.Build();
        var a = Assert.Single(result);
        Assert.Same(argument, a);
    }

    [Fact]
    public void Add_SameNameArgument_OverwritesPreviousArgument()
    {
        var argument1 = new CDKeyArgument("value");
        var argument2 = new CDKeyArgument("other");
        _builder
            .Add(argument1)
            .Add(argument2);

        var result = _builder.Build();
        Assert.DoesNotContain(argument1, result);
        Assert.Contains(argument2, result);
    }

    [Fact]
    public void Add_MultipleDifferentArguments_AllPresentInBuild()
    {
        var argument1 = new CDKeyArgument("value");
        var argument2 = new WindowedArgument();
        var mod = CreateMod("MyMod");

        _builder.Add(argument1).Add(argument2).Add(argument2).AddSingleMod(mod);

        var result = _builder.Build();
        Assert.Equal([argument1, argument2, new ModArgumentList([new ModArgument(mod.Directory, _game.Directory, false)])], result);
    }

    [Fact]
    public void AddMod_UseGameDirFromMod()
    {
        var mod = CreateMod("MyMod");

        _builder.AddSingleMod(mod);
        var builtResult = _builder.Build();

        var a = Assert.Single(builtResult);
        var modListArg = Assert.IsType<ModArgumentList>(a);
        var modArg = Assert.Single(modListArg.Value);

        Assert.Equal(mod.Directory.FullName, modArg.Value.FullName);
        Assert.Equal(_game.Directory.FullName, modArg.GameDir.FullName);
    }

    [Fact]
    public void AddMod_UseDifferentGameDir()
    {
        var otherBasePath = _fileSystem.DirectoryInfo.New("other");
        var mod = CreateMod("MyMod");

        _builder.AddSingleMod(mod, otherBasePath);
        var builtResult = _builder.Build();

        var a = Assert.Single(builtResult);
        var modListArg = Assert.IsType<ModArgumentList>(a);
        var modArg = Assert.Single(modListArg.Value);

        Assert.Equal(mod.Directory.FullName, modArg.Value.FullName);
        Assert.Equal(otherBasePath.FullName, modArg.GameDir.FullName);
    }

    [Fact]
    public void AddMods_EmptyModsCollection_IsNop()
    {
        _builder.AddMods([]);
        var builtResult = _builder.Build();
        Assert.Empty(builtResult);
    }

    [Fact]
    public void AddMods_WithGame_EmptyModsCollection_IsNop()
    {
        _builder.AddMods([], _fileSystem.DirectoryInfo.New("path"));
        var builtResult = _builder.Build();
        Assert.Empty(builtResult);
    }

    [Fact]
    public void AddMods_MixOfVirtualAndPhysicalMods_OnlyPhysicalModsAdded()
    {
        var modData = CreateVirtualMod();

        _builder.AddMods([modData.virtualMod, modData.dep]);
        var builtResult = _builder.Build();
        
        var a = Assert.Single(builtResult);
        var modListArg = Assert.IsType<ModArgumentList>(a);
        var modArg = Assert.Single(modListArg.Value);

        Assert.Equal(modData.dep.Directory.FullName, modArg.Value.FullName);
        Assert.Equal(_game.Directory.FullName, modArg.GameDir.FullName);
    }

    [Fact]
    public void AddMods_RemovesPreviousMods()
    {
        var initialMod = CreateMod("MyMod");

        var other1 = CreateMod("Other1");
        var other2 = CreateMod("Other2");
        
        _builder.AddSingleMod(initialMod);
        _builder.AddMods([other1, other2]);

        var builtResult = _builder.Build();

        var a = Assert.Single(builtResult);
        var modArgList = Assert.IsType<ModArgumentList>(a);

        Assert.DoesNotContain(modArgList.Value, arg => arg.Value.FullName == initialMod.Directory.FullName);
        Assert.Equal([
            new ModArgument(other1.Directory, _game.Directory, false),
            new ModArgument(other2.Directory, _game.Directory, false),
        ], modArgList.Value);
    }

    [Fact]
    public void AddMod_RemovesPreviousMods()
    {
        var initialMod = CreateMod("MyMod");

        var other1 = CreateMod("Other1");
        var other2 = CreateMod("Other2");

        _builder.AddMods([other1, other2]);
        _builder.AddSingleMod(initialMod);

        var builtResult = _builder.Build();

        var a = Assert.Single(builtResult);
        var modArgList = Assert.IsType<ModArgumentList>(a);

        Assert.DoesNotContain(modArgList.Value, arg => arg.Value.FullName == other1.Directory.FullName);
        Assert.DoesNotContain(modArgList.Value, arg => arg.Value.FullName == other2.Directory.FullName);
        Assert.Equal([
            new ModArgument(initialMod.Directory, _game.Directory, false)
        ], modArgList.Value);
    }

    [Fact]
    public void AddMods_AllVirtualMods_NoModsAdded()
    {
        var virtualMods = new List<IMod>
        {
            CreateVirtualMod().virtualMod,
            CreateVirtualMod().virtualMod
        };

        var result = _builder.AddMods(virtualMods);

        var builtResult = _builder.Build();
        Assert.Empty(builtResult);
    }

    [Fact]
    public void Remove_ValidArgument_Succeeds()
    {
        var argument = new WindowedArgument();
        _builder.Add(argument);
        var result = _builder.Remove(argument);
        Assert.Same(_builder, result);
        Assert.Empty(result.Build());
    }

    [Fact]
    public void Remove_ByName_ValidArgument_Succeeds()
    {
        var argument = new WindowedArgument();
        _builder.Add(argument);
        var result = _builder.Remove(GameArgumentNames.WindowedArg);
        Assert.Empty(result.Build());
    }

    [Fact]
    public void Build_WithoutAddingArguments_Succeeds()
    {
        var result = _builder.Build();
        Assert.Empty(result);
    }

    [Theory]
    [MemberData(nameof(GameArgumentTestBase.GetInvalidArguments), MemberType = typeof(GameArgumentTestBase))]
    public void Build_ContainsInvalidArgument_Throws(GameArgument invalidArg, ArgumentValidityStatus _)
    {
        if (invalidArg is ModArgumentList or ModArgument)
            return;

        _builder.Add(new WindowedArgument()).Add(invalidArg);
        Assert.Throws<GameArgumentException>(() => _builder.Build());
    }

    [Fact]
    public void DisposedBuilder_Add_ThrowsObjectDisposedException()
    {
        _builder.Dispose();
        Assert.Throws<ObjectDisposedException>(() => _builder.Add(new WindowedArgument()));
        Assert.Throws<ObjectDisposedException>(() => _builder.AddSingleMod(CreateMod("myMod")));
        Assert.Throws<ObjectDisposedException>(() => _builder.AddMods(new List<IMod>()));
        Assert.Throws<ObjectDisposedException>(() => _builder.Remove(new WindowedArgument()));
        Assert.Throws<ObjectDisposedException>(_builder.Build);
    }

    private IPhysicalMod CreateMod(string name)
    {
        return _game.InstallMod(name, false, ServiceProvider);
    }

    private (IMod virtualMod, IPhysicalMod dep) CreateVirtualMod()
    {
        var dep = _game.InstallMod("dep", false, ServiceProvider);
        var modinfo = new ModinfoData("VirtualMod")
        {
            Dependencies = new DependencyList([dep], DependencyResolveLayout.FullResolved)
        };
        return (new VirtualMod(_game, modinfo.ToJson(), modinfo, ServiceProvider), dep);
    }
}