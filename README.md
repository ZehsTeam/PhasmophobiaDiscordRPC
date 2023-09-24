# Phasmophobia Discord Rich Presence
Display a custom Discord rich presence for Phasmophobia depending on the current game state.

Made using WPF in .NET 7

Uses [Lachee's DiscordRPC for C#](https://github.com/Lachee/discord-rpc-csharp)<br>

## ![Download Icon](https://i.imgur.com/TpnrFSH.png) Download
Download the latest release [here](https://github.com/ZehsTeam/PhasmophobiaDiscordRPC/releases)

## Safe to use? - Game state detection

This program is safe to use since it only uses the **Player.log** file to detect the current game state.

**Player.log** file location:
```
C:\Users\<User>\AppData\LocalLow\Kinetic Games\Phasmophobia\Player.log
```

## How to use
Download the [latest release](https://github.com/ZehsTeam/PhasmophobiaDiscordRPC/releases) and run the program while Phasmophobia & Discord are running.

## Discord rich presence not showing?
**Make sure these settings are enabled in your Discord client:**
- "Display current activity as a status message."
- "Share your activity status by default when joining large servers"

**Discord > User Settings > Activity Settings > Activity Privacy**

![Discord Activity Privacy Settings](https://i.imgur.com/rjkxIcl.png)

[More info on activity status](https://support.discord.com/hc/en-us/articles/7931156448919)

## Features
**Discord rich presence**
- Shows server mode (Singleplayer / Multiplayer)
- Shows player state (Initializing, Main Menu, Lobby, In Match)
- Shows elapsed time for each player state
- Shows map info (Map Icon & Map Name)
- Shows difficulty
- Shows player count
- Shows max player count
- Show private lobby code
  - Shows server region
 
**Program**
- View player list (Username & Steam ID)
- Discord rich presence preview

## Settings you can change
- Lobby Type (Public / Private)
- Lobby Code
- Max Players
- Map Type
- Difficulty

## Screenshots - Program
![Program Window](https://i.imgur.com/pa7hFoE.png.png)
![Program Window](https://i.imgur.com/6t5Eapl.png.png)
## Screenshots - Discord rich presence
![Discord rich presence](https://i.imgur.com/cWVDidl.png)
![Discord rich presence](https://i.imgur.com/flXtT3h.png)
![Discord rich presence](https://i.imgur.com/jD4CkAL.png)
![Discord rich presence](https://i.imgur.com/LvPTykr.png)
