using System;
using System.Collections.Generic;

namespace PhasmophobiaDiscordRPC
{
    public enum PhasmophobiaAppStatus
    {
        None,
        Open,
        Closed
    }

    public enum ServerMode
    {
        None,
        Offline,
        Online
    }

    public enum LobbyType
    {
        Public,
        Private
    }

    public enum PlayerState
    {
        None,
        Menus,
        Lobby,
        InMatch
    }

    public enum Difficulty
    {
        None,
        Amateur = 1,
        Intermediate = 2,
        Professional = 3,
        Nightmare = 4,
        Insanity = 5,
        ChallengeMode = 10,
        Custom
    }

    internal class GameStatus
    {
        public static GameStatus Instance;

        public PhasmophobiaAppStatus PhasmophobiaAppStatus;
        public DateTime StartDateTime;
        public ServerMode ServerMode;
        public LobbyType LobbyType;
        public string ServerRegion;
        public string LobbyCode { get; private set; }
        public PlayerState PlayerState { get; private set; }
        public MapType MapType { get; private set; }
        public Difficulty Difficulty { get; private set; }

        public int MaxPlayers { get; private set; }
        private List<String> _players = new List<String>();
        public int PlayerCount => _players.Count;


        private PlayerLogReader _logReader;

        public GameStatus()
        {
            GameStatus.Instance = this;
            _logReader = new PlayerLogReader();
        }

        public void Initialize()
        {
            PhasmophobiaAppStatus = PhasmophobiaAppStatus.None;
            MaxPlayers = 4;

            ResetValues();

            _logReader.Initialize();
        }

        #region Private
        private void ResetValues()
        {
            ServerMode = ServerMode.None;
            ServerRegion = string.Empty;
            LobbyCode = string.Empty;
            PlayerState = PlayerState.None;
            MapType = MapType.None;
            Difficulty = Difficulty.None;
            _players = new List<String>();
        }

        private void SetPlayerState(PlayerState newState)
        {
            if (PlayerState != newState) StartDateTime = DateTime.UtcNow;
            PlayerState = newState;
        }

        private void AddPlayerToList(string username)
        {
            if (_players.Contains(username)) return;

            _players.Add(username);
        }
        
        private void RemovePlayerFromList(string username)
        {
            if (!_players.Contains(username)) return;

            _players.Remove(username);
        }

        private void UpdateDiscordRPC()
        {
            if (PhasmophobiaAppStatus != PhasmophobiaAppStatus.Open) return;

            if (PlayerState == PlayerState.Menus)
            {
                MainWindow.Instance.SetMenusRPC();
            }

            if (PlayerState == PlayerState.Lobby)
            {
                MainWindow.Instance.SetLobbyRPC();
            }

            if (PlayerState == PlayerState.InMatch)
            {
                MainWindow.Instance.SetInMatchRPC();
            }
        }
        #endregion

        #region Setters
        public void SetLobbyType(LobbyType _lobbyType)
        {
            LobbyType = _lobbyType;
            UpdateDiscordRPC();
        }

        public void SetLobbyCode(string _lobbyCode)
        {
            LobbyCode = _lobbyCode;
            UpdateDiscordRPC();
        }

        public void SetMaxPlayers(int _maxPlayers)
        {
            MaxPlayers = _maxPlayers;
            UpdateDiscordRPC();
        }
        #endregion

        #region PlayerLogReader On Callbacks
        public void OnPhasmophobiaStarted()
        {
            PhasmophobiaAppStatus = PhasmophobiaAppStatus.Open;
            StartDateTime = DateTime.UtcNow;

            MainWindow.Instance.InitializeDiscordRPC();
            MainWindow.Instance.SetPhasmophobiaAppStatus(PhasmophobiaAppStatus);
        }

        public void OnPhasmophobiaClosed()
        {
            PhasmophobiaAppStatus = PhasmophobiaAppStatus.Closed;

            MainWindow.Instance.StopDiscordRPC();
            MainWindow.Instance.SetPhasmophobiaAppStatus(PhasmophobiaAppStatus);
            MainWindow.Instance.SetDiscordRichPresencePreviewIsEnabled(false);
        }

        public void OnServerModeOffline()
        {
            ServerMode = ServerMode.Offline;
            ServerRegion = "";
        }

        public void OnServerModeOnline(string region)
        {
            ServerMode = ServerMode.Online;
            ServerRegion = region;

            MainWindow.Instance.SetLobbyRegion(region);
        }

        public void OnLoadedLevel(string levelName, Difficulty newDifficulty, int newPlayerCount)
        {
            PlayerState newState = PlayerState.None;

            if (levelName == "MainMenu")
            {
                MapType = MapType.None;
                Difficulty = Difficulty.None;
                newState = newPlayerCount == 0 ? PlayerState.Menus : PlayerState.Lobby;
            }
            else
            {
                MapType = MapManager.GetMapByLevelName(levelName).type;
                Difficulty = newDifficulty;
                newState = PlayerState.InMatch;
            }

            SetPlayerState(newState);
            UpdateDiscordRPC();
        }

        public void OnAppliedDifficulty(Difficulty newDifficulty)
        {
            Difficulty = newDifficulty;

            UpdateDiscordRPC();
        }

        public void OnRoomCreated()
        {
            SetPlayerState(PlayerState.Lobby);

            UpdateDiscordRPC();
        }

        public void OnJoinedRoom()
        {
            SetPlayerState(PlayerState.Lobby);

            UpdateDiscordRPC();
        }

        public void OnRecievedPlayerInfo(string username)
        {
            AddPlayerToList(username);

            UpdateDiscordRPC();
        }

        public void OnPlayerEntered(string username)
        {
            AddPlayerToList(username);

            UpdateDiscordRPC();
        }

        public void OnPlayerLeft(string username)
        {
            RemovePlayerFromList(username);

            UpdateDiscordRPC();
        }

        public void OnLeftRoom()
        {
            ResetValues();

            UpdateDiscordRPC();
        }
        #endregion
    }
}
