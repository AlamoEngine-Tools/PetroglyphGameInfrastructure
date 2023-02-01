using System;
using System.Collections.Generic;
using EawModinfo;
using EawModinfo.Model;
using EawModinfo.Spec;
using Moq;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;
using PetroGlyph.Games.EawFoc.Services.Dependencies;
using PetroGlyph.Games.EawFoc.Services.Icon;
using PetroGlyph.Games.EawFoc.Services.Language;
using Semver;
using Xunit;

namespace PetroGlyph.Games.EawFoc.Test;

public class ModBaseTest
{
    [Fact]
    public void InvalidCtor_Throws()
    {
        Assert.ThrowsAny<Exception>(() => new Mock<ModBase>((IGame)null, ModType.Default, (string)null, (IServiceProvider)null).Object);
        Assert.ThrowsAny<Exception>(() => new Mock<ModBase>((IGame)null, ModType.Default, (IModinfo)null, (IServiceProvider)null).Object);
        var game = new Mock<IGame>();
        Assert.ThrowsAny<Exception>(() => new Mock<ModBase>(game.Object, ModType.Default, (string)null, (IServiceProvider)null).Object);
        Assert.ThrowsAny<Exception>(() => new Mock<ModBase>(game.Object, ModType.Default, (IModinfo)null, (IServiceProvider)null).Object);
        var sp = new Mock<IServiceProvider>();
        Assert.ThrowsAny<Exception>(() => new Mock<ModBase>(game.Object, ModType.Default, (string)null, sp.Object).Object);
        Assert.ThrowsAny<Exception>(() => new Mock<ModBase>(game.Object, ModType.Default, string.Empty, sp.Object).Object);
        Assert.ThrowsAny<Exception>(() => new Mock<ModBase>(game.Object, ModType.Default, (IModinfo)null, sp.Object).Object);

        Assert.ThrowsAny<Exception>(() => new Mock<ModBase>(game.Object, ModType.Default, "abs", null).Object);
        Assert.ThrowsAny<Exception>(() => new Mock<ModBase>(game.Object, ModType.Default, new Mock<IModinfo>().Object, null).Object);

        Assert.ThrowsAny<Exception>(() => new Mock<ModBase>(game.Object, ModType.Default, new Mock<IModinfo>().Object, sp.Object).Object);
    }

    [Fact]
    public void ValidCtors_Properties()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Name", sp.Object);
        Assert.Equal("Name", mod.Name);
        Assert.Equal(ModType.Default, mod.Type);
        Assert.Empty(mod.Dependencies);
        Assert.Equal(DependencyResolveLayout.ResolveRecursive, mod.DependencyResolveLayout);
        Assert.Equal(DependencyResolveStatus.None, mod.DependencyResolveStatus);
        Assert.Empty(mod.Mods);
        Assert.Null(mod.Version);
        Assert.Null(mod.VersionRange);

        var modinfo = new ModinfoData("Name")
        {
            Icon = "IconPath",
            Version = new SemVersion(1, 0, 0),
            Languages = new List<ILanguageInfo>(),
            Dependencies = new DependencyList(new List<IModReference>(), DependencyResolveLayout.ResolveLastItem)
        };
        var modA = new ModMock(game.Object, ModType.Default, modinfo, sp.Object);
        Assert.Equal("Name", modA.Name);
        Assert.Equal(ModType.Default, modA.Type);
        Assert.Equal(modinfo, modA.ModInfo);
        Assert.Empty(modA.Dependencies);
        Assert.Equal(DependencyResolveLayout.ResolveLastItem, modA.DependencyResolveLayout);
        Assert.Equal(DependencyResolveStatus.None, modA.DependencyResolveStatus);
        Assert.Empty(modA.Mods);
        Assert.Equal(modinfo.Version, modA.Version);
        Assert.Null(modA.VersionRange);
        Assert.Equal(modinfo.Icon, modA.IconFile);
    }

    [Fact]
    public void TestModinfoResolving()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Name", sp.Object);

        var counter = 0;

        void OnModOnResolvingModinfoA(object? sender, ResolvingModinfoEventArgs args)
        {
            counter++;
            args.Cancel = true;
        }

        void OnModOnResolvingModinfoB(object? sender, ResolvingModinfoEventArgs args)
        {
            counter++;
        }

        mod.ResolvingModinfo += OnModOnResolvingModinfoA;
        var flag = false;
        mod.ModinfoResolved += (_, _) => flag = true;
        Assert.Null(mod.ResetModinfo());
        Assert.Null(mod.ModInfo);
        Assert.Equal(1, counter);
        Assert.Null(mod.ModInfo);
        Assert.Equal(1, counter);
        Assert.False(flag);
        mod.ResolvingModinfo -= OnModOnResolvingModinfoA;
        mod.ResolvingModinfo += OnModOnResolvingModinfoB;
        Assert.Null(mod.ResetModinfo());
        Assert.NotNull(mod.ModInfo);
        Assert.Equal(2, counter);
        Assert.True(flag);

    }

    [Fact]
    public void TestModinfoResolving_Throws()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Other", sp.Object);
        Assert.Throws<ModinfoException>(() => mod.ModInfo);
    }

    [Fact]
    public void TestIconResolving()
    {
        var flag = false;
        var resolver = new Mock<IModIconFinder>();
        resolver.Setup(r => r.FindIcon(It.IsAny<IMod>())).Returns((string)null!).Callback(() => flag = true);
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        sp.Setup(p => p.GetService(typeof(IModIconFinder))).Returns(resolver.Object);
        var mod = new ModMock(game.Object, ModType.Default, "Name", sp.Object);
        Assert.Null(mod.IconFile);
        Assert.True(flag);
    }

    [Fact]
    public void TestLanguageResolving_Throws()
    {
        var flag = false;
        var sp = new Mock<IServiceProvider>();
        var resolver = new Mock<IModLanguageFinderFactory>();
        resolver.Setup(r => r.CreateLanguageFinder(It.IsAny<IMod>(), It.IsAny<IServiceProvider>())).Callback(() => flag = true).Returns(new Mock<IModLanguageFinder>().Object);
        var game = new Mock<IGame>();
        sp.Setup(p => p.GetService(typeof(IModLanguageFinderFactory))).Returns(resolver.Object);
        var mod = new ModMock(game.Object, ModType.Default, "Name", sp.Object);
        Assert.Throws<PetroglyphException>(() => mod.InstalledLanguages);
        Assert.True(flag);
    }

    [Fact]
    public void TestLanguageResolving()
    {
        var flag = false;
        var sp = new Mock<IServiceProvider>();
        var finder = new Mock<IModLanguageFinder>();
        finder.Setup(f => f.FindInstalledLanguages(It.IsAny<IMod>())).Returns(new HashSet<ILanguageInfo>());
        var resolver = new Mock<IModLanguageFinderFactory>();
        resolver.Setup(r => r.CreateLanguageFinder(It.IsAny<IMod>(), It.IsAny<IServiceProvider>())).Callback(() => flag = true).Returns(finder.Object);
        var game = new Mock<IGame>();
        sp.Setup(p => p.GetService(typeof(IModLanguageFinderFactory))).Returns(resolver.Object);
        var mod = new ModMock(game.Object, ModType.Default, "Name", sp.Object);
        Assert.Empty(mod.InstalledLanguages);
        Assert.True(flag);
    }

    [Fact]
    public void AddRemoveMods()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Other", sp.Object);

        Assert.Equal(0, mod.Mods.Count);

        var modMock = new Mock<IMod>();
        modMock.Setup(m => m.Game).Returns(game.Object);
        modMock.Setup(m => m.Equals(It.IsAny<IMod>())).Returns(true);
        var modA = modMock.Object;

        mod.AddMod(modA);
        Assert.Equal(1, mod.Mods.Count);
        mod.AddMod(modA);
        Assert.Equal(1, mod.Mods.Count);
        Assert.Single(mod);

        mod.RemoveMod(modA);
        Assert.Equal(0, mod.Mods.Count);

        mod.RemoveMod(modA);
        Assert.Equal(0, mod.Mods.Count);
        Assert.Empty(mod);
    }

    [Fact]
    public void AddInvalidMod_Throws()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Other", sp.Object);

        var modMock = new Mock<IMod>();

        Assert.Throws<ModException>(() => mod.AddMod(modMock.Object));
    }

    [Fact]
    public void AddModRaiseEvent()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Other", sp.Object);

        var raised = false;
        mod.ModsCollectionModified += (_, args) =>
        {
            raised = true;
            Assert.Equal(ModCollectionChangedAction.Add, args.Action);
        };

        var modMock = new Mock<IMod>();
        modMock.Setup(m => m.Game).Returns(game.Object);
        mod.AddMod(modMock.Object);

        Assert.True(raised);
    }


    [Fact]
    public void RemoveModRaiseEvent()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Other", sp.Object);

        var modMock = new Mock<IMod>();
        var modA = modMock.Object;
        modMock.Setup(m => m.Equals(It.IsAny<IMod>())).Returns(true);
        modMock.Setup(m => m.Game).Returns(game.Object);
        mod.AddMod(modA);

        var raised = false;
        mod.ModsCollectionModified += (_, args) =>
        {
            raised = true;
            Assert.Equal(ModCollectionChangedAction.Remove, args.Action);
        };

        mod.RemoveMod(modA);
        Assert.True(raised);
    }

    [Fact]
    public void FindMod()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Other", sp.Object);

        var modMock = new Mock<IMod>();
        var modA = modMock.Object;
        modMock.Setup(m => m.Equals(It.IsAny<IModReference>())).Returns(true);
        modMock.Setup(m => m.Game).Returns(game.Object);
        mod.AddMod(modA);

        var foundMod = mod.FindMod(modA);
        Assert.NotNull(foundMod);

        Assert.True(mod.TryFindMod(modA, out _));
    }

    [Fact]
    public void FindMod_Throws()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Other", sp.Object);

        var modMock = new Mock<IMod>();
        var modA = modMock.Object;
        modMock.Setup(m => m.Game).Returns(game.Object);
        mod.AddMod(modA);

        Assert.Throws<ModNotFoundException>(() => mod.FindMod(modA));
        Assert.False(mod.TryFindMod(modA, out _));
    }

    [Fact]
    public void Resolve_NullArgs()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Other", sp.Object);
        Assert.Throws<ArgumentNullException>(() => mod.ResolveDependencies(null, null));
        Assert.Throws<ArgumentNullException>(() => mod.ResolveDependencies(null, new DependencyResolverOptions()));
        Assert.Throws<ArgumentNullException>(() => mod.ResolveDependencies(new Mock<IDependencyResolver>().Object, null));
    }

    [Fact]
    public void Resolve_AlreadyResolving()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Other", sp.Object);

        mod.SetResolveStatus(DependencyResolveStatus.Resolving);

        var resolver = new Mock<IDependencyResolver>();
        Assert.Throws<ModDependencyCycleException>(() =>
            mod.ResolveDependencies(resolver.Object, new DependencyResolverOptions()));
    }

    [Fact]
    public void Resolve_ResolvingThrows()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Other", sp.Object);
        var resolver = new Mock<IDependencyResolver>();
        resolver.Setup(r => r.Resolve(It.IsAny<IMod>(), It.IsAny<DependencyResolverOptions>())).Throws<Exception>();
        Assert.Throws<Exception>(() => mod.ResolveDependencies(resolver.Object, new DependencyResolverOptions()));
        Assert.Equal(DependencyResolveStatus.Faulted, mod.DependencyResolveStatus);
    }

    [Fact]
    public void ResolveTest()
    {
        var game = new Mock<IGame>();
        var sp = new Mock<IServiceProvider>();
        var mod = new ModMock(game.Object, ModType.Default, "Name", sp.Object);
        var resolver = new Mock<IDependencyResolver>();
        resolver.Setup(r => r.Resolve(It.IsAny<IMod>(), It.IsAny<DependencyResolverOptions>())).Returns(new List<ModDependencyEntry>());
        var flag = false;
        mod.DependenciesChanged += (_, _) => flag = true;
        mod.ResolveDependencies(resolver.Object, new DependencyResolverOptions());
        Assert.Equal(DependencyResolveStatus.Resolved, mod.DependencyResolveStatus);
        Assert.Empty(mod.Dependencies);
        Assert.True(flag);
    }

    private class ModMock : ModBase
    {
        public override string Identifier => "Mod";

        public void SetResolveStatus(DependencyResolveStatus status)
        {
            DependencyResolveStatus = status;
        }

        public ModMock(IGame game, ModType type, string name, IServiceProvider serviceProvider) : base(game, type, name, serviceProvider)
        {
        }

        public ModMock(IGame game, ModType type, IModinfo modinfo, IServiceProvider serviceProvider) : base(game, type, modinfo, serviceProvider)
        {
        }

        protected override IModinfo? ResolveModInfoCore()
        {
            return new ModinfoData("Name");
        }
    }
}