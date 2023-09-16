using DiscordRPC;
using DiscordRPC.Logging;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Timers;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using Timer = System.Timers.Timer;

namespace PhasmophobiaDiscordRPC
{
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;
        
        private DiscordRpcClient _client;

        private static readonly Regex _lobbyCodeRegex = new Regex("^[0-9]+$");
        private Timer _timer;

        private bool _settingDifficultyComboBoxUI;

        public MainWindow()
        {
            MainWindow.Instance = this;

            DataContext = this;
            _playersList = new ObservableCollection<PlayerData>();

            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            SetPhasmophobiaAppStatusUI(PhasmophobiaAppState.Closed);
            SetPlayersListUI(new List<PlayerData>());
            SetDiscordRichPresencePreviewUIState(false);

            _timer = new Timer();
            _timer.Elapsed += new ElapsedEventHandler(MainTick);
            _timer.Interval = 1000;
            _timer.Start();

            _settingDifficultyComboBoxUI = false;

            InitializeDiscordRpc();
            ClearPresence();

            MapDatabase.Initialize();

            new GameStateManager();
            new PlayerLogReader();

            GameStateManager.Instance.Initialize();
            PlayerLogReader.Instance.Initialize();

            GameStateManager.Instance.OnPhasmophobiaAppStateChanged += OnPhasmophobiaAppStateChanged;

            LobbyTypeComboBox.SelectedIndex = 0;
            MaxPlayersComboBox.SelectedIndex = 0;
            DifficultyComboBox.SelectedIndex = 0;
        }

        private void Deinitialize()
        {
            GameStateManager.Instance.OnPhasmophobiaAppStateChanged -= OnPhasmophobiaAppStateChanged;

            PlayerLogReader.Instance.Deinitialize();

            DisposeDiscordRpc();
        }

        private void InitializeDiscordRpc()
        {
            /*
            Create a Discord client
            NOTE: 	If you are using Unity3D, you must use the full constructor and define
                     the pipe connection.
            */
            _client = new DiscordRpcClient("1147579832193011762");

            //Set the logger
            _client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            //Subscribe to events
            _client.OnReady += (sender, e) =>
            {
                Debug.WriteLine($"Received Ready from user {e.User.Username}");
            };

            _client.OnClose += (sender, e) =>
            {
                Debug.WriteLine("Closed client");
            };

            _client.OnPresenceUpdate += (sender, e) =>
            {
                Debug.WriteLine($"Received Presence Update! {e.Presence}");
            };

            //Connect to the RPC
            _client.Initialize();
        }

        private void DisposeDiscordRpc()
        {
            if (_client == null) return;
            if (_client.IsDisposed) return;

            _client.Dispose();
        }

        private void MainTick(object source, ElapsedEventArgs e)
        {
            GameStateManager.Instance.Tick();

            if (GameStateManager.Instance.GetPhasmophobiaAppState() != PhasmophobiaAppState.Open) return;

            DiscordRichPresenceTick();
            PlayerLogReader.Instance.Tick();
        }

        #region Phasmophobia App State
        private void OnPhasmophobiaAppStateChanged(PhasmophobiaAppState phasmophobiaAppState)
        {
            if (phasmophobiaAppState == PhasmophobiaAppState.Closed) OnPhasmophobiaClosed();
            if (phasmophobiaAppState == PhasmophobiaAppState.Open) OnPhasmophobiaOpened();
        }

        private void OnPhasmophobiaClosed()
        {
            SetPhasmophobiaAppStatusUI(PhasmophobiaAppState.Closed);
            SetPlayersListUI(new List<PlayerData>());

            ClearPresence();
        }

        private void OnPhasmophobiaOpened()
        {
            SetPhasmophobiaAppStatusUI(PhasmophobiaAppState.Open);
        }
        #endregion

        #region Presence
        public void ShowPresence(GameState gameState, LobbyType lobbyType, int maxPlayers, string lobbyCode)
        {
            if (_client == null) return;

            try
            {
                Map map = MapDatabase.GetMapByMapType(gameState.MapType);
            
                string details = Presence_GetDetails(gameState, map);
                string state = Presence_GetState(gameState, lobbyType, maxPlayers, lobbyCode);
                Assets assets = Presence_GetAssetsForMap(map, gameState.PlayerState);

                _client.SetPresence(new RichPresence()
                {
                    Details = details,
                    State = state,
                    Assets = assets,
                    Timestamps = new Timestamps()
                    {
                        Start = gameState.StartDateTime
                    }
                });

                SetDiscordRichPresencePreviewUI(map.ImageKey, details, state, gameState.StartDateTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing presence!\n\n{ex}", "Phasmophobia Rich Presence");
            }
        }
        
        public void ClearPresence()
        {
            SetDiscordRichPresencePreviewUIState(false);

            if (_client == null) return;

            _client.ClearPresence();
        }

        private string Presence_GetDetails(GameState gameState, Map map)
        {
            PlayerState playerState = gameState.PlayerState;
            Difficulty difficulty = gameState.Difficulty;

            if (playerState == PlayerState.Initializing) return "Initializing";
            if (playerState == PlayerState.Menus) return "In Menus";

            string details = "";

            if (playerState == PlayerState.Lobby) details = "In Lobby";
            if (playerState == PlayerState.InMatch) details = map.Name;

            if (map.MapType == MapType.Training) return details;

            bool showDifficulty = difficulty != Difficulty.None && difficulty != Difficulty.Training;
            if (showDifficulty)
            {
                details += $" - {Presence_GetDifficultyName(difficulty)}";
            }

            return details;
        }

        private string Presence_GetDifficultyName(Difficulty difficulty)
        {
            if (difficulty == Difficulty.ChallengeMode) return "Challenge Mode";
            return Enum.GetName(difficulty);
        }

        private string Presence_GetState(GameState gameState, LobbyType lobbyType, int maxPlayers, string lobbyCode)
        {
            PlayerState playerState = gameState.PlayerState;
            int playerCount = gameState.PlayerCount;
            ServerMode serverMode = gameState.ServerMode;
            string serverRegion = gameState.ServerRegion;

            if (playerState == PlayerState.Initializing) return string.Empty;
            if (playerState == PlayerState.Menus) return string.Empty;

            if (serverMode == ServerMode.Offline) return "Singleplayer";

            string state = lobbyType == LobbyType.Public ? "Public " : "";
            state += $"Party ({playerCount} of {maxPlayers})";

            if (lobbyCode != string.Empty && lobbyType == LobbyType.Private)
            {
                state += $", {serverRegion}-{lobbyCode}";
            }

            return state;
        }

        private Assets Presence_GetAssetsForMap(Map map, PlayerState playerState)
        {
            Assets assets = new Assets()
            {
                LargeImageKey = "logo"
            };

            if (playerState == PlayerState.InMatch)
            {
                assets = new Assets()
                {
                    LargeImageKey = map.ImageKey,
                    LargeImageText = map.Name,
                    SmallImageKey = "logo",
                    SmallImageText = "Phasmophobia"
                };
            }

            return assets;
        }
        #endregion

        private void Window_Closed(object sender, EventArgs e)
        {
            Deinitialize();
        }

        #region Window - Topbar
        private void TopBar_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            DragMove();
        }

        private void windowMinimizeBtn_Click(object sender, RoutedEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        private void windowCloseBtn_Click(object sender, RoutedEventArgs e)
        {
            Application.Current.Shutdown();
        }
        #endregion

        #region Window - Phasmophobia App Status
        public void SetPhasmophobiaAppStatusUI(PhasmophobiaAppState phasmophobiaAppStatus)
        {
            string status = "Offline";
            if (phasmophobiaAppStatus == PhasmophobiaAppState.Open) status = "Online";

            string foregroundColor = phasmophobiaAppStatus == PhasmophobiaAppState.Open ? "#4CAF50" : "#F44336";

            this.Dispatcher.Invoke(() =>
            {
                PhasmophobiaAppStatusTextBlock.Text = status;
                PhasmophobiaAppStatusTextBlock.Foreground = GetBrushFromHex(foregroundColor);
            });
        }
        #endregion

        #region Window - Settings
        // Lobby Type
        public void SetLobbyTypeIsEnabled(bool isEnabled)
        {
            Cursor cursor = isEnabled ? Cursors.Arrow : Cursors.No;

            LobbyTypeBorder.Cursor = cursor;
            LobbyTypeComboBox.IsEnabled = isEnabled;
        }

        private void LobbyTypeComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string value = ("" + comboBox.SelectedValue.ToString()).Split(":")[1].Trim();

            LobbyType lobbyType = value == "Public" ? LobbyType.Public : LobbyType.Private;

            GameStateManager.Instance.SetLobbyType(lobbyType);

            SetLobbyCodeIsEnabled(lobbyType == LobbyType.Private);
        }

        // Lobby Code
        public void SetLobbyCodeIsEnabled(bool isEnabled)
        {
            Cursor cursor = isEnabled ? Cursors.Arrow : Cursors.No;

            LobbyCodeBorder.Cursor = cursor;
            LobbyTypeGrid.IsEnabled = isEnabled;
        }

        public void SetLobbyRegionTextBlock(string lobbyRegion)
        {
            if (lobbyRegion == string.Empty) lobbyRegion = "##";

            this.Dispatcher.Invoke(() =>
            {
                LobbyRegionTextBlock.Text = $"{lobbyRegion}  —";
            });
        }

        private void LobbyCodeTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox textBox = (TextBox)sender;
            int caretIndex = textBox.CaretIndex;
            textBox.Text = textBox.Text.Replace(" ", "");
            textBox.CaretIndex = caretIndex;
        }

        private void LobbyCodeTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !IsLobbyCodeTextAllowed(e.Text);
        }

        private void LobbyCodeTextBox_Pasting(object sender, DataObjectPastingEventArgs e)
        {
            if (e.DataObject.GetDataPresent(typeof(String)))
            {
                String text = (String)e.DataObject.GetData(typeof(String));
                if (!IsLobbyCodeTextAllowed(text))
                {
                    e.CancelCommand();
                }
            }
            else
            {
                e.CancelCommand();
            }
        }

        private bool IsLobbyCodeTextAllowed(string text)
        {
            return _lobbyCodeRegex.IsMatch(text);
        }

        private void LobbyCodeCancelButton_Click(object sender, RoutedEventArgs e)
        {
            LobbyCodeTextBox.Text = string.Empty;

            GameStateManager.Instance.SetLobbyCode(string.Empty);
        }

        private void LobbyCodeConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string lobbyCode = LobbyCodeTextBox.Text;
            if (lobbyCode.Length != 6) return;

            GameStateManager.Instance.SetLobbyCode(lobbyCode);
        }
        // Max Players
        public void SetMaxPlayersIsEnabled(bool isEnabled)
        {
            Cursor cursor = isEnabled ? Cursors.Arrow : Cursors.No;

            MaxPlayersBorder.Cursor = cursor;
            MaxPlayersComboBox.IsEnabled = isEnabled;
        }

        private void MaxPlayersComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string value = ("" + comboBox.SelectedValue.ToString()).Split(":")[1].Trim();

            int maxPlayers = Convert.ToInt32(value);

            GameStateManager.Instance.SetMaxPlayers(maxPlayers);
        }

        // Difficulty
        private void DifficultyComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (_settingDifficultyComboBoxUI) return;

            ComboBox comboBox = (ComboBox)sender;
            string value = ("" + comboBox.SelectedValue.ToString()).Split(":")[1].Trim();

            Difficulty difficulty = GetDifficultyFromString(value);

            Debug.WriteLine($"Setting GameStateManager Difficulty: {Enum.GetName(difficulty)}");

            GameStateManager.Instance.SetDifficulty(difficulty);
        }

        public void SetDifficultyComboBoxSelection(Difficulty difficulty)
        {
            int index = (int)difficulty;
            if (index == (int)Difficulty.Training) index = 0;

            _settingDifficultyComboBoxUI = true;

            this.Dispatcher.Invoke(() =>
            {
                DifficultyComboBox.SelectedIndex = index;
            });

            _settingDifficultyComboBoxUI = false;
        }

        private Difficulty GetDifficultyFromString(string difficultyString)
        {
            Difficulty difficulty = Difficulty.None;

            if (difficultyString == "Amateur") difficulty = Difficulty.Amateur;
            if (difficultyString == "Intermediate") difficulty = Difficulty.Intermediate;
            if (difficultyString == "Professional") difficulty = Difficulty.Professional;
            if (difficultyString == "Nightmare") difficulty = Difficulty.Nightmare;
            if (difficultyString == "Insanity") difficulty = Difficulty.Insanity;
            if (difficultyString == "Challenge Mode") difficulty = Difficulty.ChallengeMode;
            if (difficultyString == "Custom") difficulty = Difficulty.Custom;
            if (difficultyString == "Training") difficulty = Difficulty.Training;

            return difficulty;
        }

        // Players List
        private ObservableCollection<PlayerData> _playersList;

        public ObservableCollection<PlayerData> PlayersList
        {
            get { return _playersList; }
            set { _playersList = value; }
        }

        public void SetPlayersListUI(List<PlayerData> players)
        {
            this.Dispatcher.Invoke(() =>
            {
                PlayersList.Clear();

                foreach (PlayerData player in players)
                {
                    PlayersList.Add(player);
                }
            });
        }
        #endregion

        #region Window - Discord Rich Presence Preview
        public void SetDiscordRichPresencePreviewUIState(bool isEnabled)
        {
            if (isEnabled)
            {
                this.Dispatcher.Invoke(() =>
                {
                    DiscordRichPresencePreviewOnline.Visibility = Visibility.Visible;
                    DiscordRichPresencePreviewOffline.Visibility = Visibility.Collapsed;
                });
            }
            else
            {
                this.Dispatcher.Invoke(() =>
                {
                    DiscordRichPresencePreviewOnline.Visibility = Visibility.Collapsed;
                    DiscordRichPresencePreviewOffline.Visibility = Visibility.Visible;
                });
            }
        }
        
        public void SetDiscordRichPresencePreviewUI(string imageKey, string details, string state, DateTime dateTime)
        {
            SetDiscordRichPresencePreviewUIState(true);

            Visibility detailsVisibility = details == string.Empty ? Visibility.Collapsed : Visibility.Visible;
            Visibility stateVisibility = state == string.Empty ? Visibility.Collapsed : Visibility.Visible;

            this.Dispatcher.Invoke(() =>
            {
                Resources["PreviewImage"] = GetBitmapImageFromImageKey(imageKey);

                DiscordRichPresencePreviewDetails.Text = details;
                DiscordRichPresencePreviewDetails.Visibility = detailsVisibility;

                DiscordRichPresencePreviewState.Text = state;
                DiscordRichPresencePreviewState.Visibility = stateVisibility;
            });
        }

        private BitmapImage GetBitmapImageFromImageKey(string imageKey)
        {
            BitmapImage image = new BitmapImage(new Uri($"pack://application:,,,/img/{imageKey}.png", UriKind.Absolute));

            return image;
        }

        private void DiscordRichPresenceTick()
        {
            UpdateDiscordRichPresenceElapsedUI();
        }

        private void UpdateDiscordRichPresenceElapsedUI()
        {
            DateTime startDateTime = GameStateManager.Instance.GameState.StartDateTime;
            string elapsed = GetElapsedTimeString(startDateTime);

            this.Dispatcher.Invoke(() =>
            {
                DiscordRichPresencePreviewElapsed.Text = elapsed;
            });
        }

        private string GetElapsedTimeString(DateTime startDateTime)
        {
            TimeSpan elapsed = DateTime.UtcNow - startDateTime;

            return FormatElapsedTime(elapsed);
        }

        private string FormatElapsedTime(TimeSpan elapsed)
        {
            if (elapsed.TotalDays >= 1)
            {
                return $"{((int)elapsed.TotalDays).ToString("00")}:{elapsed.Hours.ToString("00")}:{elapsed.Minutes.ToString("00")}:{elapsed.Seconds.ToString("00")} elapsed";
            }
            else if (elapsed.TotalHours >= 1)
            {
                return $"{((int)elapsed.TotalHours).ToString("00")}:{elapsed.Minutes.ToString("00")}:{elapsed.Seconds.ToString("00")} elapsed";
            }
            else
            {
                return $"{((int)elapsed.TotalMinutes).ToString("00")}:{elapsed.Seconds.ToString("00")} elapsed";
            }
        }
        #endregion

        private Brush GetBrushFromHex(string hex)
        {
            Color color = (Color)ColorConverter.ConvertFromString(hex);
            return new System.Windows.Media.SolidColorBrush(color);
        }
    }
}
