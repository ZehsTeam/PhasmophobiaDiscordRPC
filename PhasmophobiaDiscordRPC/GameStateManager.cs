using System;
using System.Diagnostics;

namespace PhasmophobiaDiscordRPC
{
    #region Enums
    public enum PhasmophobiaAppState
    {
        None,
        Open,
        Closed
    }

    public enum LobbyType
    {
        Public,
        Private
    }
    #endregion

    internal class GameStateManager
    {
        public static GameStateManager Instance;
        public GameState GameState { get; private set; }

        private LobbyType _lobbyType;
        private int _maxPlayers;
        private string _lobbyCode;

        public event Action<PhasmophobiaAppState> OnPhasmophobiaAppStateChanged;

        private PhasmophobiaAppState _phasmophobiaAppState;

        public GameStateManager()
        {
            GameStateManager.Instance = this;
        }

        public void Initialize()
        {
            GameState = new GameState();
            _maxPlayers = 4;
            _lobbyCode = string.Empty;

            _phasmophobiaAppState = PhasmophobiaAppState.None;
        }

        public void UpdatePresence()
        {
            if (_phasmophobiaAppState != PhasmophobiaAppState.Open) return;

            MainWindow.Instance.ShowPresence(GameState, _lobbyType, _maxPlayers, _lobbyCode);
        }

        public void UpdatePlayersListUI()
        {
            MainWindow.Instance.SetPlayersListUI(GameState.Players);
        }

        public PhasmophobiaAppState GetPhasmophobiaAppState()
        {
            return _phasmophobiaAppState;
        }

        public void Tick()
        {
            CheckPhasmophobiaAppState();
        }

        #region Private
        private void CheckPhasmophobiaAppState()
        {
            bool isRunning = IsPhasmophobiaRunning();

            if (_phasmophobiaAppState != PhasmophobiaAppState.Open && isRunning) // Phasmophobia started running
            {
                SetPhasmophobiaAppState(PhasmophobiaAppState.Open);
            }

            if (_phasmophobiaAppState == PhasmophobiaAppState.Open && !isRunning) // Phasmophobia stopped running
            {
                SetPhasmophobiaAppState(PhasmophobiaAppState.Closed);
            }
        }

        private bool IsPhasmophobiaRunning()
        {
            return Process.GetProcessesByName("phasmophobia").Length > 0;
        }

        private void SetPhasmophobiaAppState(PhasmophobiaAppState phasmophobiaAppState)
        {
            _phasmophobiaAppState = phasmophobiaAppState;
            GameState.OnPhasmophobiaStateChanged(phasmophobiaAppState);
            OnPhasmophobiaAppStateChanged?.Invoke(phasmophobiaAppState);
        }
        #endregion

        #region MainWindow Callbacks
        public void SetLobbyType(LobbyType lobbyType)
        {
            _lobbyType = lobbyType;

            UpdatePresence();
        }

        public void SetLobbyCode(string lobbyCode)
        {
            _lobbyCode = lobbyCode;

            UpdatePresence();
        }

        public void SetMaxPlayers(int maxPlayers)
        {
            _maxPlayers = maxPlayers;

            UpdatePresence();
        }
        #endregion

        #region PlayerLogReader Callbacks
        public void OnServerModeOffline(string[] data, bool update)
        {
            GameState.SetServerMode(ServerMode.Offline);
        }

        public void OnServerModeOnline(string[] data, bool update)
        {
            string region = data[0];

            GameState.SetServerMode(ServerMode.Online);
            GameState.SetServerRegion(region);

            MainWindow.Instance.SetLobbyRegionTextBlock(region);
        }

        public void OnLoadedLevel(string[] data, bool update)
        {
            string levelName = data[0];
            int playerCount = int.Parse(data[1]);
            //bool isHost = data[2] == "True";
            //Difficulty difficulty = GetDifficultyFromId(data[3]);

            MapType mapType = MapDatabase.GetMapTypeByLevelName(levelName);

            PlayerState playerState = PlayerState.None;

            if (mapType == MapType.MainMenu)
            {
                playerState = playerCount == 0 ? PlayerState.Menus : PlayerState.Lobby;
            }
            else
            {
                playerState = PlayerState.InMatch;
            }

            GameState.SetMapType(mapType);
            GameState.SetPlayerState(playerState);
            //if (playerState != PlayerState.Menus) GameState.SetDifficulty(difficulty);

            if (update)
            {
                UpdatePlayersListUI();
                UpdatePresence();
            }
        }

        public void OnApplyingDifficulty(string[] data, bool update)
        {
            Difficulty difficulty = GetDifficultyFromId(data[0]);

            GameState.SetDifficulty(difficulty);

            if (update) UpdatePresence();
        }

        public void OnRoomCreated(string[] data, bool update)
        {
            GameState.OnRoomCreated();
            GameState.SetPlayerState(PlayerState.Lobby);
        }

        public void OnJoinedRoom(string[] data, bool update)
        {
            GameState.SetPlayerState(PlayerState.Lobby);

            if (update) UpdatePresence();
        }

        public void OnReceivedPlayerInfo(string[] data, bool update)
        {
            string username = data[0];
            string steamId = data[1];
            PlayerType playerType = (PlayerType)int.Parse(data[2]);

            GameState.AddPlayer(username, steamId, playerType);

            if (update)
            {
                UpdatePlayersListUI();
                UpdatePresence();
            }
        }

        public void OnReceivedAllPlayerInfo(string[] data, bool update)
        {
            string hostUsername = data[0];
            string localUsername = data[1];

            GameState.SetHostPlayer(hostUsername);
            GameState.SetLocalPlayer(localUsername);

            if (update) UpdatePlayersListUI();
        }

        public void OnPlayerEntered(string[] data, bool update)
        {
            string username = data[0];
            string steamId = data[1];
            PlayerType playerType = (PlayerType)int.Parse(data[2]);

            GameState.AddPlayer(username, steamId, playerType);

            if (update)
            {
                UpdatePlayersListUI();
                UpdatePresence();
            }
        }

        public void OnPlayerLeft(string[] data, bool update)
        {
            //string username = data[0];
            string steamId = data[1];

            GameState.RemovePlayer(steamId);

            if (update)
            {
                UpdatePlayersListUI();
                UpdatePresence();
            }
        }

        public void OnLeftRoom(string[] data, bool update)
        {
            GameState.SetPlayerState(PlayerState.Menus);

            if (update)
            {
                UpdatePlayersListUI();
                UpdatePresence();
            }
        }

        public void OnHostChanged(string[] data, bool update)
        {
            //string steamId = data[0];
            string username = data[1];

            GameState.SetHostPlayer(username);

            if (update) UpdatePlayersListUI();
        }

        public void OnHostAndLocalPlayer(string[] data, bool update)
        {
            string username = data[0];

            GameState.SetHostAndLocalPlayer(username);

            if (update) UpdatePlayersListUI();
        }

        private Difficulty GetDifficultyFromId(int id)
        {
            if (!Enum.IsDefined(typeof(Difficulty), id)) return Difficulty.None;

            return (Difficulty)id;
        }

        private Difficulty GetDifficultyFromId(string id)
        {
            return GetDifficultyFromId(int.Parse(id));
        }
        #endregion
    }
}
