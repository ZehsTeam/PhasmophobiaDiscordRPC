using System;
using System.Collections.Generic;
using System.Linq;

namespace PhasmophobiaDiscordRPC
{
    #region Enums
    public enum ServerMode
    {
        None,
        Offline,
        Online
    }

    public enum PlayerState
    {
        None,
        Initializing,
        Menus,
        Lobby,
        InMatch
    }

    public enum Difficulty
    {
        None = -1,
        Training = 0,
        Amateur = 1,
        Intermediate = 2,
        Professional = 3,
        Nightmare = 4,
        Insanity = 5,
        ChallengeMode = 6,
        Custom = 7,
    }
    #endregion

    public class GameState
    {
        #region Variables
        public DateTime StartDateTime { get; private set; }
        public ServerMode ServerMode { get; private set; }
        public string ServerRegion { get; private set; }
        public PlayerState PlayerState { get; private set; }
        public MapType MapType { get; private set; }
        public Difficulty Difficulty { get; private set; }
        public List<PlayerData> Players { get; private set; }
        public int PlayerCount => Players.Count;
        #endregion

        public GameState()
        {
            ServerRegion = string.Empty;
            Players = new List<PlayerData>();

            Reset();
        }

        private void Reset()
        {
            StartDateTime = DateTime.UtcNow;
            ServerMode = ServerMode.None;
            ServerRegion = string.Empty;
            PlayerState = PlayerState.None;
            MapType = MapType.None;
            Difficulty = Difficulty.None;
            Players = new List<PlayerData>();
        }

        #region Base Functions
        public void OnPhasmophobiaStateChanged(PhasmophobiaAppState phasmophobiaAppState)
        {
            Reset();

            if (phasmophobiaAppState == PhasmophobiaAppState.Open)
            {
                SetPlayerState(PlayerState.Initializing);
            }
        }

        public void SetStartDateTime(DateTime startDateTime)
        {
            StartDateTime = startDateTime;
        }

        public void SetServerMode(ServerMode serverMode)
        {
            ServerMode = serverMode;
        }

        public void SetServerRegion(string serverRegion)
        {
            ServerRegion = serverRegion;
        }

        public void SetPlayerState(PlayerState playerState)
        {
            if (playerState != PlayerState) SetStartDateTime(DateTime.UtcNow);

            PlayerState = playerState;

            if (playerState == PlayerState.Menus || playerState == PlayerState.Initializing)
            {
                SetMapType(MapType.MainMenu);
                SetPlayers(new List<PlayerData>());
                SetDifficulty(Difficulty.None);
            }
        }

        public void SetMapType(MapType mapType)
        {
            MapType = mapType;
        }

        public void SetDifficulty(Difficulty difficulty)
        {
            Difficulty = difficulty;
        }

        public void SetPlayers(List<PlayerData> players)
        {
            Players = players;
        }

        public void AddPlayer(string username, string steamId, PlayerType playerType)
        {
            string[] steamIds = Players.Select(player => player.SteamId).ToArray();
            if (steamIds.Contains(steamId)) return;

            Players.Add(new PlayerData(username, steamId, playerType));
        }

        public void RemovePlayer(string username, string steamId, PlayerType playerType)
        {
            string[] steamIds = Players.Select(player => player.SteamId).ToArray();
            if (!steamIds.Contains(steamId)) return;

            int targetPlayerIndex = -1;

            for (int i = 0; i < steamIds.Length; i++)
            {
                if (steamIds[i] == steamId)
                {
                    targetPlayerIndex = i;
                    break;
                }
            }

            Players.RemoveAt(targetPlayerIndex);
        }

        public void SetHostPlayer(string username)
        {
            int length = Players.Count;

            // Reset Host
            for (int i = 0;i < length; i++)
            {
                PlayerData playerData = Players[i];

                if (playerData.PlayerType == PlayerType.Host)
                {
                    Players[i].PlayerType = PlayerType.Other;
                }
            }

            // Set Host
            for (int i = 0; i < length; i++)
            {
                PlayerData playerData = Players[i];

                if (playerData.Username == username)
                {
                    Players[i].PlayerType = PlayerType.Host;
                    break;
                }
            }
        }
        #endregion
    }
}
