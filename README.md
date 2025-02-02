# Petroglyph Game Infrastructure
.NET library for managing and launching Petroglyph's Star Wars Empire at War and mod installations.

[![Nuget](https://img.shields.io/nuget/v/AlamoEngineTools.PG.StarWarsGame.Infrastructure)](https://www.nuget.org/packages/AlamoEngineTools.PG.StarWarsGame.Infrastructure) 
[![Build](https://github.com/AlamoEngine-Tools/PetroglyphGameInfrastructure/actions/workflows/release.yml/badge.svg)](https://github.com/AlamoEngine-Tools/PetroglyphGameInfrastructure/actions/workflows/build.yml) 

## Main Features
- Works for Empire at War, Forces of Corruption on any platform (Steam, Disk, GoG, EA Origin)
- Supports any kind of mod, including mods from Steam Workshops
- Supports Steam Sub-mods (e.g, STEAMMOD=123 STEAMMOD=456)
- Finds any game and mod installations automatically
- Launches the game, including debug builds with full Steam support. 

## Usage

Include the library as a nuget package: `AlamoEngineTools.PG.StarWarsGame.Infrastructure`

See the listed code below to see a minimal example how to use the library.

```cs
var fs = new FileSystem();
var sc = new ServiceCollection();
sc.AddSingleton(WindowsRegistry.Default);
sc.AddSingleton<IFileSystem>(fs);

// Initialize the library
PetroglyphGameInfrastructure.InitializeServices(sc);

var serviceProvider = sc.BuildServiceProvider();

// Search for Forces of Corruption at the specified directory
var gameFactory = services.GetRequiredService<IGameFactory>();
var detector = new DirectoryGameDetector(fs.DirectoryInfo.New("YOUR_GAME_DIR"), serviceProvider);
var game = gameFactory.CreateGame(detector.Detect(GameType.Foc), CultureInfo.CurrentCulture);


// Create a client and launch the game with WINDOWED arugment.
var client = services.GetRequiredService<IGameClientFactory>().CreateClient(game);
using var gameArgs = new GameArgumentsBuilder().Add(new WindowedArgument());
client.Play(gameArgs.Build());
```

See the sample application for an extended example including how to work with game mods. 

