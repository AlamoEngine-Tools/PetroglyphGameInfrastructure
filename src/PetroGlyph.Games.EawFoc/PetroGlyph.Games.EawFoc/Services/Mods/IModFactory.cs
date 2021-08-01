using System.Collections.Generic;
using System.IO;
using EawModinfo.Model;
using EawModinfo.Spec;
using PetroGlyph.Games.EawFoc.Games;
using PetroGlyph.Games.EawFoc.Mods;

namespace PetroGlyph.Games.EawFoc.Services
{
    /// <summary>
    /// Factory to create one or more <see cref="IMod"/>
    /// </summary>
    public interface IModFactory
    {
        /// <summary>
        /// Creates one or many <see cref="IPhysicalMod"/>s according to
        /// <see href="https://github.com/AlamoEngine-Tools/eaw.modinfo#ii2-file-position"/>
        /// The mod's filesystem location will be interpreted from <see cref="IModReference.Identifier"/>.
        /// If no modinfo file is present in that location, the directory itself will define the mod.
        /// </summary>
        /// <remarks>The created mods dot NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
        /// <param name="modReference">The mod reference of the new mod.</param>
        /// <returns>One mod or multiple variant mods. </returns>
        /// <exception cref="IModReference">when <see cref="ModException"/> could not be located as an existing location</exception>
        /// <exception cref="DirectoryNotFoundException">when no instance could be created due to missing information (such as the mod's name)</exception>
        IEnumerable<IPhysicalMod> FromReference(IGame game, IModReference modReference);

        /// <summary>
        /// Creates a new <see cref="IPhysicalMod"/> instance for a game from a file system path.
        /// The mod's filesystem location will be interpreted from <see cref="IModReference.Identifier"/>.
        /// </summary>
        /// <remarks>The created mods dot NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
        /// <param name="modReference">The mod reference of the new mod.</param>
        /// <param name="modinfo">Optional <see cref="DirectoryNotFoundException"/> from which the mod will get initialized.</param>
        /// <returns>the Mod instance</returns>
        /// <exception cref="IModReference.Identifier">when <see cref="IModReference"/> could not be located as an existing location</exception>
        /// <exception cref="ModinfoData">when no instance could be created due to missing information (such as the mod's name)</exception>
        IPhysicalMod FromReference(IGame game, IModReference modReference, IModinfo? modinfo);

        /// <summary>
        /// Creates a new <see cref="IPhysicalMod"/> instance for a game from a file system path.
        /// The mod's filesystem location will be interpreted from <see cref="IModReference.Identifier"/>.
        /// </summary>
        /// <remarks>The created mods dot NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mod.</param>
        /// <param name="modReference">The mod reference of the new mod.</param>
        /// <param name="searchModinfoFile">When <see langword="true"/> a modinfo.json file, if present, will be used to initialize;
        /// otherwise a modinfo.json will be ignored</param>
        /// <returns>the Mod instance</returns>
        /// <exception cref="DirectoryNotFoundException">when <see cref="IModReference.Identifier"/> could not be located as an existing location</exception>
        /// <exception cref="ModException">when no instance could be created due to missing information (such as the mod's name)</exception>
        /// <exception cref="ModException">if <paramref name="searchModinfoFile"/> is true and the directory contains any variant files.</exception>
        IPhysicalMod FromReference(IGame game, IModReference modReference, bool searchModinfoFile);

        /// <summary>
        /// Searches for variant modinfo files and returns new instances for each variant.
        /// The mod's filesystem location will be interpreted from <see cref="IModReference.Identifier"/>.
        /// </summary>
        /// <remarks>The created mods dot NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
        /// <param name="modReference">Mod reference of the new mods.</param>
        /// <returns>All mod variants which are found.</returns>
        /// <exception cref="DirectoryNotFoundException">when <see cref="IModReference.Identifier"/> could not be located as an existing location</exception>
        /// <exception cref="ModException">when no instance could be created due to missing information (such as the mod's name)</exception>
        /// <exception cref="ModException">when the same instance was created multiple times.</exception>
        IEnumerable<IPhysicalMod> VariantsFromReference(IGame game, IModReference modReference);


        /// <summary>
        /// Creates a virtual mods from a given <see cref="IModinfo"/>.
        /// </summary>
        /// <remarks>The created mods dot NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
        /// <param name="virtualModInfo">Set of <see cref="IModinfo"/> where each element defines its own virtual mod.</param>
        /// <returns>One or many virtual mods</returns>
        /// <exception cref="PetroglyphException">if the virtual mod could not be created.</exception>
        IVirtualMod CreateVirtualVariant(IGame game, IModinfo virtualModInfo);

        /// <summary>
        /// Creates virtual mods for a game.
        /// </summary>
        /// <remarks>The created mods dot NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
        /// <param name="name">The name of the virtual mod.</param>
        /// <param name="dependencies">dependencies of the mod.
        /// The value are the sorted dependencies of the virtual mod</param>
        /// <param name="resolveLayout">The resolve layout of the <paramref name="dependencies"/> list.</param>
        /// <returns>One or many virtual mods</returns>
        /// <exception cref="PetroglyphException">if the virtual mod could not be created.</exception>
        IVirtualMod CreateVirtualVariant(IGame game, string name, IList<ModDependencyEntry> dependencies, DependencyResolveLayout resolveLayout);


        /// <summary>
        /// Creates virtual mods for a game.
        /// </summary>
        /// <remarks>The created mods dot NOT get added to the <see cref="IModContainer.Mods"/>collection of the <paramref name="game"/>.</remarks>
        /// <param name="game">The parent <see cref="IGame"/> instance of the mods.</param>
        /// <param name="name">The name of the virtual mod.</param>
        /// <param name="dependencies">dependencies of the mod.
        /// The value are the sorted dependencies of the virtual mod</param>
        /// <param name="resolveLayout">The resolve layout of the <paramref name="dependencies"/> list.</param>
        /// <returns>One or many virtual mods</returns>
        /// <exception cref="PetroglyphException">if the virtual mod could not be created.</exception>
        IVirtualMod CreateVirtualVariant(IGame game, string name, IList<IModReference> dependencies, DependencyResolveLayout resolveLayout);
    }
}
