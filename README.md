# Phasmophobia Discord Rich Presence
Display a custom Discord rich presence for Phasmophobia depending on the current game state.

Made using WPF in .NET 7

**Libraries in use**<be>
- [Lachee's DiscordRPC for C#](https://github.com/Lachee/discord-rpc-csharp) (v1.2.1.24)<br>
- [MaterialDesignInXamlToolkit](https://github.com/MaterialDesignInXAML/MaterialDesignInXamlToolkit) (v4.9.0)

## Download
Download the [Latest Release](https://github.com/ZehsTeam/PhasmophobiaDiscordRPC/releases)

#### Requires
- [Phasmophobia](https://store.steampowered.com/app/739630/Phasmophobia/)

## Game state detection

It uses **Player.log** to detect the current game state.
```
C:\Users\<User>\AppData\LocalLow\Kinetic Games\Phasmophobia\Player.log
```

## Features

- Shows server mode (Singleplayer / Multiplayer)
- Shows player state (Initializing, Main Menu, Lobby, In Match)
- Shows map info (Map Icon & Map Name)
- Shows difficulty
- Shows player count
- Shows elapsed time for each player state
- View player list (Username & SteamID)

#### Optional
- Show lobby code in a private game
  - Shows server region
- Change max player count

## App settings you can change
- Lobby Type (Public / Private)
- Lobby Code
- Max Players

## App Window
![App Window](https://i.imgur.com/VAV5Tv7.png?raw=true)
![App Window](https://i.imgur.com/tAo35fj.png?raw=true)

## Default Phasmophobia Discord rich presence<be>
![Default](https://i.imgur.com/bRYOoxi.png?raw=true)

## Custom Phasmophobia Discord rich presence<br>
![In Menus](https://i.imgur.com/cWVDidl.png?raw=true)
![In Lobby Singleplayer](https://i.imgur.com/flXtT3h.png?raw=true)
![In Match Singleplayer](https://i.imgur.com/jD4CkAL.png?raw=true)
![In Lobby Multiplayer](https://i.imgur.com/LvPTykr.png?raw=true)
