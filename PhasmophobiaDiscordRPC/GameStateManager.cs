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

        public void SetDifficulty(Difficulty difficulty)
        {
            GameState.SetDifficulty(difficulty);

            UpdatePresence();
        }
        #endregion

        #region PlayerLogReader Callbacks
        public void OnServerModeOffline(bool update)
        {
            GameState.SetServerMode(ServerMode.Offline);
        }

        public void OnServerModeOnline(string region, bool update)
        {
            GameState.SetServerMode(ServerMode.Online);
            GameState.SetServerRegion(region);

            MainWindow.Instance.SetLobbyRegionTextBlock(region);
        }

        public void OnLoadedLevel(string levelName, int playerCount, bool isHost, Difficulty difficulty, bool update)
        {
            MapType mapType = MapDatabase.GetMapTypeByLevelName(levelName);

            PlayerState playerState = PlayerState.None;

            if (mapType == MapType.MainMenu)
            {
                playerState = playerCount == 0 ? PlayerState.Menus : PlayerState.Lobby;
                GameState.SetDifficulty(Difficulty.None);
                MainWindow.Instance.SetDifficultyComboBoxSelection(Difficulty.None);
            }
            else
            {
                playerState = PlayerState.InMatch;
            }

            GameState.SetMapType(mapType);
            GameState.SetPlayerState(playerState);

            // This broke with the new Ascension Hotfix v0.9.0.10
            //if (playerState != PlayerState.Menus) GameState.SetDifficulty(difficulty);

            if (mapType == MapType.Training)
            {
                GameState.SetDifficulty(Difficulty.Training);
                MainWindow.Instance.SetDifficultyComboBoxSelection(Difficulty.Training);
            }

            if (update)
            {
                UpdatePlayersListUI();
                UpdatePresence();
            }
        }

        public void OnApplyingDifficulty(Difficulty difficulty, bool update)
        {
            GameState.SetDifficulty(difficulty);
            MainWindow.Instance.SetDifficultyComboBoxSelection(difficulty);

            if (update) UpdatePresence();
        }

        public void OnRoomCreated(bool update)
        {
            GameState.OnRoomCreated();
            GameState.SetPlayerState(PlayerState.Lobby);
        }

        public void OnJoinedRoom(bool update)
        {
            GameState.SetPlayerState(PlayerState.Lobby);

            if (update) UpdatePresence();
        }

        public void OnReceivedPlayerInfo(string username, string steamId, bool update)
        {
            GameState.AddPlayer(username, steamId);

            if (update)
            {
                UpdatePlayersListUI();
                UpdatePresence();
            }
        }

        public void OnReceivedAllPlayerInfo(string hostUsername, string localUsername, bool update)
        {
            GameState.SetHostPlayer(hostUsername);
            GameState.SetLocalPlayer(localUsername);

            if (update) UpdatePlayersListUI();
        }

        public void OnPlayerEntered(string username, string steamId, bool update)
        {
            GameState.AddPlayer(username, steamId);

            if (update)
            {
                UpdatePlayersListUI();
                UpdatePresence();
            }
        }

        public void OnPlayerLeft(string username, string steamId, bool update)
        {
            GameState.RemovePlayer(username);

            if (update)
            {
                UpdatePlayersListUI();
                UpdatePresence();
            }
        }

        public void OnLeftRoom(bool update)
        {
            GameState.SetPlayerState(PlayerState.Menus);

            if (update)
            {
                UpdatePlayersListUI();
                UpdatePresence();
            }
        }

        public void OnHostChanged(string username, string steamId, bool update)
        {
            GameState.SetHostPlayer(username);

            if (update) UpdatePlayersListUI();
        }

        public void OnHostAndLocalPlayer(string username, bool update)
        {
            GameState.SetHostAndLocalPlayer(username);

            if (update) UpdatePlayersListUI();
        }

        private Difficulty GetDifficultyFromId(int id)
        {
            if (!Enum.IsDefined(typeof(Difficulty), id)) return Difficulty.None;

            return (Difficulty)id;
        }
        #endregion
    }
}
