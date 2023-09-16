using System;
using System.Collections.Generic;
using System.Diagnostics;
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
        None = 0,
        Amateur = 1,
        Intermediate = 2,
        Professional = 3,
        Nightmare = 4,
        Insanity = 5,
        ChallengeMode = 6,
        Custom = 7,
        Training = 8
    }
    #endregion

    public class GameState
    {
        public DateTime StartDateTime { get; private set; }
        public ServerMode ServerMode { get; private set; }
        public string ServerRegion { get; private set; }
        public PlayerState PlayerState { get; private set; }
        public MapType MapType { get; private set; }
        public Difficulty Difficulty { get; private set; }
        public List<PlayerData> Players { get; private set; }
        public int PlayerCount => Players.Count;

        private bool _nextPlayerIsHostAndLocal;

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

            _nextPlayerIsHostAndLocal = false;
        }

        #region Public Events
        public void OnPhasmophobiaStateChanged(PhasmophobiaAppState phasmophobiaAppState)
        {
            Reset();

            if (phasmophobiaAppState == PhasmophobiaAppState.Open)
            {
                SetPlayerState(PlayerState.Initializing);
            }
        }

        public void OnRoomCreated()
        {
            _nextPlayerIsHostAndLocal = true;
        }
        #endregion

        #region Public Functions
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

            //Debug.WriteLine($"[Set {players.Count} Players]");

            //players.ForEach(player =>
            //{
            //    Debug.WriteLine($"[Set Player] Username: {player.Username}, Steam ID: {player.SteamId}, PlayerType: {Enum.GetName(player.PlayerType)}");
            //});
        }

        public void AddPlayer(string username, string steamId, PlayerType playerType)
        {
            PlayerData player = GetPlayerFromUsername(username);
            if (player != null) return;

            // Room Created
            if (_nextPlayerIsHostAndLocal)
            {
                playerType = PlayerType.HostAndLocal;
                _nextPlayerIsHostAndLocal = false;
            }

            //Debug.WriteLine($"[Added Player] Username: {username}, Steam ID: {steamId}, PlayerType: {Enum.GetName(playerType)}");

            Players.Add(new PlayerData(username, steamId, playerType));
        }

        public void RemovePlayer(string steamId)
        {
            PlayerData player = GetPlayerFromSteamId(steamId);
            if (player == null) return;

            int index = GetIndexForPlayer(player);

            //Debug.WriteLine($"[Removed Player] Username: {player.Username}, Steam ID: {player.SteamId}, PlayerType: {Enum.GetName(player.PlayerType)}");

            Players.RemoveAt(index);
        }

        public void SetHostPlayer(string username)
        {
            SetPlayerTypeForPlayerUsername(username, PlayerType.Host);
        }
        
        public void SetLocalPlayer(string username)
        {
            SetPlayerTypeForPlayerUsername(username, PlayerType.Local);
        }

        public void SetHostAndLocalPlayer(string username)
        {
            SetPlayerTypeForPlayerUsername(username, PlayerType.HostAndLocal);
        }
        #endregion

        #region Private Player Functions
        private void SetPlayerTypeForPlayerUsername(string username, PlayerType playerType)
        {
            PlayerData player = GetPlayerFromUsername(username);
            if (player == null) return;

            SetPlayerTypeForPlayer(player, playerType);
        }

        private void SetPlayerTypeForPlayerSteamId(string steamId, PlayerType playerType)
        {
            PlayerData player = GetPlayerFromSteamId(steamId);
            if (player == null) return;

            SetPlayerTypeForPlayer(player, playerType);
        }

        private void SetPlayerTypeForPlayer(PlayerData player, PlayerType playerType)
        {
            int index = GetIndexForPlayer(player);
            Players[index].PlayerType = playerType;
        }

        private PlayerData GetPlayerWithPlayerType(PlayerType playerType)
        {
            PlayerData[] players = Players.Where(player => player.PlayerType == playerType).ToArray();
            if (players.Length == 0) return null;
            return players[0];
        }

        private PlayerData GetPlayerFromUsername(string username)
        {
            PlayerData[] players = Players.Where(player => player.Username == username).ToArray();
            if (players.Length == 0) return null;
            return players[0];
        }

        private PlayerData GetPlayerFromSteamId(string steamId)
        {
            PlayerData[] players = Players.Where(player => player.SteamId == steamId).ToArray();
            if (players.Length == 0) return null;
            return players[0];
        }
        
        private int GetIndexForPlayer(PlayerData player)
        {
            int length = Players.Count;

            for (int i = 0; i < length; i++)
            {
                if (Players[i] == player) return i;
            }

            return -1;
        }
        #endregion
    }
}
