﻿using System.Collections.Generic;
using System.IO.Abstractions.TestingHelpers;
using EawModinfo.Spec;
using Microsoft.Extensions.DependencyInjection;
using Moq;
using PetroGlyph.Games.EawFoc.Clients.Arguments;
using PetroGlyph.Games.EawFoc.Clients.Arguments.GameArguments;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using PetroGlyph.Games.EawFoc.Services.Steam;
using Xunit;
#if NET
using System;
#endif

namespace PetroGlyph.Games.EawFoc.Clients.Test.Arguments;

public class ModArgumentListFactoryTest
{
    private readonly ModArgumentListFactory _service;
    private readonly Mock<IArgumentValidator> _validator;
    private readonly Mock<IModDependencyTraverser> _traverser;
    private readonly Mock<ISteamGameHelpers> _steamHelper;

    public ModArgumentListFactoryTest()
    {
        var sc = new ServiceCollection();
        _validator = new Mock<IArgumentValidator>();
        _traverser = new Mock<IModDependencyTraverser>();
        _steamHelper = new Mock<ISteamGameHelpers>();
        sc.AddTransient(_ => _validator.Object);
        sc.AddTransient(_ => _traverser.Object);
        sc.AddTransient(_ => _steamHelper.Object);
        _service = new ModArgumentListFactory(sc.BuildServiceProvider());
    }

    [Fact]
    public void TestSingleArgSteam()
    {
        var modA = new Mock<IPhysicalMod>();
        modA.Setup(m => m.Type).Returns(ModType.Workshops);
        modA.Setup(m => m.Identifier).Returns("123");

        var id = 123ul;
        _steamHelper.Setup(s => s.ToSteamWorkshopsId("123", out id)).Returns(true);

        var modList = _service.BuildArgumentList(new List<IMod> { modA.Object }, false);

        var arg = Assert.Single(modList.Value);
        Assert.Equal("123", arg.Value);
        Assert.Equal(ArgumentNameCatalog.SteamModArg, arg.Name);
    }

    [Fact]
    public void TestSingleArgVirtual()
    {
        var modA = new Mock<IMod>();
        modA.Setup(m => m.Type).Returns(ModType.Virtual);

        var modList = _service.BuildArgumentList(new List<IMod> { modA.Object }, false);
        Assert.Empty(modList.Value);
        modList = _service.BuildArgumentList(modA.Object, false);
        Assert.Empty(modList.Value);
    }


    [Fact]
    public void TestSingleArgSteamInvalid_Throws()
    {
        var modA = new Mock<IPhysicalMod>();
        modA.Setup(m => m.Type).Returns(ModType.Workshops);
        modA.Setup(m => m.Identifier).Returns("bla");

        var id = 123ul;
        _steamHelper.Setup(s => s.ToSteamWorkshopsId("bla", out id)).Returns(false);

        Assert.Throws<SteamException>(() => _service.BuildArgumentList(new List<IMod> { modA.Object }, false));
    }

    [Fact]
    public void TestSingleArgModIsRelativeToGame()
    {
        var fs = new MockFileSystem();
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("game"));
        var modA = new Mock<IPhysicalMod>();
        modA.Setup(m => m.Type).Returns(ModType.Default);
        modA.Setup(m => m.Game).Returns(game.Object);
        modA.Setup(m => m.Directory).Returns(fs.DirectoryInfo.New("game/mods/a"));

        var modList = _service.BuildArgumentList(new List<IMod> { modA.Object }, false);

        var arg = Assert.Single(modList.Value);
#if NET
        if (!OperatingSystem.IsWindows())
            Assert.Equal("mods/a", arg.Value);
        else
#endif
        Assert.Equal("mods\\a", arg.Value);
        Assert.Equal(ArgumentNameCatalog.ModPathArg, arg.Name);
    }

    [Fact]
    public void TestSingleArgModIsAbsolute()
    {
        var fs = new MockFileSystem();
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("game"));
        var modA = new Mock<IPhysicalMod>();
        modA.Setup(m => m.Type).Returns(ModType.Default);
        modA.Setup(m => m.Game).Returns(game.Object);
        modA.Setup(m => m.Directory).Returns(fs.DirectoryInfo.New("d:\\a"));

        var modList = _service.BuildArgumentList(new List<IMod> { modA.Object }, false);

        var arg = Assert.Single(modList.Value);
#if NET
        if (!OperatingSystem.IsWindows())
            Assert.Equal("../d:/a", arg.Value);
        else
#endif
        Assert.Equal("d:\\a", arg.Value);
        Assert.Equal(ArgumentNameCatalog.ModPathArg, arg.Name);
    }


    [Fact]
    public void TestTraversedInput()
    {
        ulong id;
        _steamHelper.Setup(s => s.ToSteamWorkshopsId(It.IsAny<string>(), out id)).Returns(true);

        var fs = new MockFileSystem();
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("game"));
        var modA = new Mock<IPhysicalMod>();
        modA.Setup(m => m.Type).Returns(ModType.Workshops);
        modA.Setup(m => m.Identifier).Returns("123");
        var modB = new Mock<IPhysicalMod>();
        modB.Setup(m => m.Type).Returns(ModType.Workshops);
        modB.Setup(m => m.Identifier).Returns("456");

        var modList = _service.BuildArgumentList(new List<IMod> { modA.Object, modB.Object }, false);
        Assert.Equal(new List<ModArgument>
        {
            new("123", true),
            new("456", true)
        }, modList.Value);
    }

    [Fact]
    public void TestNotTraversedNotResolved()
    {
        ulong id;
        _steamHelper.Setup(s => s.ToSteamWorkshopsId(It.IsAny<string>(), out id)).Returns(true);

        var modA = new Mock<IPhysicalMod>();
        modA.Setup(m => m.Type).Returns(ModType.Workshops);
        modA.Setup(m => m.Identifier).Returns("123");
        var modB = new Mock<IPhysicalMod>();
        modB.Setup(m => m.Type).Returns(ModType.Workshops);
        modB.Setup(m => m.Identifier).Returns("456");

        _traverser.Setup(t => t.Traverse(It.IsAny<IMod>())).Returns(new List<ModDependencyEntry>
        {
            new(modA.Object),
            new(modB.Object)
        });

        var modList = _service.BuildArgumentList(modA.Object, false);
        var arg = Assert.Single(modList.Value);
        Assert.Equal("123", arg.Value);
    }

    [Fact]
    public void TestNotTraversed()
    {
        ulong id;
        _steamHelper.Setup(s => s.ToSteamWorkshopsId(It.IsAny<string>(), out id)).Returns(true);

        var modA = new Mock<IPhysicalMod>();
        modA.Setup(m => m.Type).Returns(ModType.Workshops);
        modA.Setup(m => m.Identifier).Returns("123");
        modA.Setup(m => m.DependencyResolveStatus).Returns(DependencyResolveStatus.Resolved);
        var modB = new Mock<IPhysicalMod>();
        modB.Setup(m => m.Type).Returns(ModType.Workshops);
        modB.Setup(m => m.Identifier).Returns("456");

        _traverser.Setup(t => t.Traverse(It.IsAny<IMod>())).Returns(new List<ModDependencyEntry>
        {
            new(modA.Object),
            new(modB.Object)
        });

        var modList = _service.BuildArgumentList(modA.Object, false);
        Assert.Equal(new List<ModArgument>
        {
            new("123", true),
            new("456", true)
        }, modList.Value);
    }

    [Fact]
    public void TestArgNotValid()
    {
        var fs = new MockFileSystem();
        var game = new Mock<IGame>();
        game.Setup(g => g.Directory).Returns(fs.DirectoryInfo.New("game"));

        var modA = new Mock<IPhysicalMod>();
        modA.Setup(m => m.Type).Returns(ModType.Default);
        modA.Setup(m => m.Game).Returns(game.Object); 
        modA.Setup(m => m.Directory).Returns(fs.DirectoryInfo.New("d:\\path to mod\\a"));

        _validator.Setup(v => v.CheckArgument(It.IsAny<IGameArgument>(), out It.Ref<string>.IsAny, out It.Ref<string>.IsAny))
            .Returns(ArgumentValidityStatus.InvalidData);

        Assert.Throws<ModException>(() => _service.BuildArgumentList(modA.Object, true));
    }
}