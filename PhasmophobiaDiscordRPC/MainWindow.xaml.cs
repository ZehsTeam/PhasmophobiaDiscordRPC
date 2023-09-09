using DiscordRPC.Logging;
using DiscordRPC;
using System;
using System.Windows;
using System.Diagnostics;
using System.Windows.Input;
using System.Windows.Controls;
using System.Text.RegularExpressions;
using System.Windows.Media;
using System.Windows.Documents;
using System.Windows.Media.Imaging;
using System.IO;
using System.Threading;

namespace PhasmophobiaDiscordRPC
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    /// 
    public partial class MainWindow : Window
    {
        public static MainWindow Instance;

        private DiscordRpcClient client;

        private static readonly Regex _lobbyCodeRegex = new Regex("^[0-9]+$");

        public MainWindow()
        {
            MainWindow.Instance = this;

            InitializeComponent();
            Initialize();
        }

        private void Initialize()
        {
            MapManager.Initialize();

            new GameStatus();
            GameStatus.Instance.Initialize();

            SetPhasmophobiaAppStatus(PhasmophobiaAppStatus.None);
            LobbyTypeComboBox.SelectedIndex = 0;
            MaxPlayersComboBox.SelectedIndex = 0;
            SetDiscordRichPresencePreviewIsEnabled(false);
        }

        public void InitializeDiscordRPC()
        {
            /*
            Create a Discord client
            NOTE: 	If you are using Unity3D, you must use the full constructor and define
                     the pipe connection.
            */
            client = new DiscordRpcClient("1147579832193011762");

            //Set the logger
            client.Logger = new ConsoleLogger() { Level = LogLevel.Warning };

            //Subscribe to events
            client.OnReady += (sender, e) =>
            {
                Debug.WriteLine("Received Ready from user {0}", e.User.Username);
            };

            client.OnClose += (sender, e) =>
            {
                Debug.WriteLine("Closed client");
            };

            client.OnPresenceUpdate += (sender, e) =>
            {
                Debug.WriteLine("Received Update! {0}", e.Presence);
            };

            //Connect to the RPC
            client.Initialize();

            //Set the rich presence
            //Call this as many times as you want and anywhere in your code.
            SetMenusRPC();
        }

        public void StopDiscordRPC()
        {
            if (client == null) return;

            try
            {
                client.Dispose();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error!\n\n{ex}", "Phasmophobia Discord Rich Presence");
            }
        }

        public void SetMenusRPC()
        {
            string imageKey = "logo";
            string details = "In Menus";
            string state = "";

            DateTime dateTime = GameStatus.Instance.StartDateTime;

            try
            {
                client.SetPresence(new RichPresence()
                {
                    Details = details,
                    State = state,
                    Assets = new Assets()
                    {
                        LargeImageKey = imageKey,
                        LargeImageText = "Phasmophobia"
                    },
                    Timestamps = new Timestamps()
                    {
                        Start = GameStatus.Instance.StartDateTime
                    }
                });

                SetDiscordRichPresencePreview(imageKey, details, state, dateTime);
            } 
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing presence!\n\n{ex}", "Phasmophobia Discord Rich Presence");
            }
        }

        public void SetLobbyRPC()
        {
            ServerMode serverMode = GameStatus.Instance.ServerMode;
            string serverRegion = GameStatus.Instance.ServerRegion;
            LobbyType lobbyType = GameStatus.Instance.LobbyType;
            string lobbyCode = GameStatus.Instance.LobbyCode;
            int playerCount = GameStatus.Instance.PlayerCount;
            int maxPlayerCount = GameStatus.Instance.MaxPlayers;
            Difficulty difficulty = GameStatus.Instance.Difficulty;

            string difficultyString = Enum.GetName(difficulty).Replace("ChallengeMode", "Challenge Mode");
            string details = difficulty == Difficulty.None ? "In Lobby" : $"In Lobby - {difficultyString}";

            string state = string.Empty;
            if (serverMode == ServerMode.Online)
            {
                if (lobbyType == LobbyType.Public)
                {
                    state = $"Public Party ({playerCount} of {maxPlayerCount})";
                }
                else
                {
                    bool hasCode = lobbyCode != string.Empty;

                    state = hasCode ? $"Private Party ({playerCount} of {maxPlayerCount}), {serverRegion}-{lobbyCode}"
                        : $"Private Party ({playerCount} of {maxPlayerCount})";
                }
            }
            else
            {
                state = "Singleplayer";
            }

            string imageKey = "logo";
            DateTime dateTime = GameStatus.Instance.StartDateTime;

            try
            {
                client.SetPresence(new RichPresence()
                {
                    Details = details,
                    State = state,
                    Assets = new Assets()
                    {
                        LargeImageKey = imageKey,
                        LargeImageText = "Phasmophobia"
                    },
                    Timestamps = new Timestamps()
                    {
                        Start = dateTime
                    }
                });

                SetDiscordRichPresencePreview(imageKey, details, state, dateTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing presence!\n\n{ex}", "Phasmophobia Discord Rich Presence");
            }
        }

        public void SetInMatchRPC()
        {
            ServerMode serverMode = GameStatus.Instance.ServerMode;
            string serverRegion = GameStatus.Instance.ServerRegion;
            LobbyType lobbyType = GameStatus.Instance.LobbyType;
            string lobbyCode = GameStatus.Instance.LobbyCode;
            MapType mapType = GameStatus.Instance.MapType;
            int playerCount = GameStatus.Instance.PlayerCount;
            int maxPlayerCount = GameStatus.Instance.MaxPlayers;
            Difficulty difficulty = GameStatus.Instance.Difficulty;

            Map map = MapManager.GetMapByMapType(mapType);
            if (map == null) return;

            string difficultyString = Enum.GetName(difficulty).Replace("ChallengeMode", "Challenge Mode");
            string details = difficulty == Difficulty.None ? map.name : $"{map.name} - {difficultyString}";

            string state = string.Empty;
            if (serverMode == ServerMode.Online)
            {
                if (lobbyType == LobbyType.Public)
                {
                    state = $"Public Party ({playerCount} of {maxPlayerCount})";
                }
                else
                {
                    bool hasCode = lobbyCode != string.Empty;

                    state = hasCode ? $"Private Party ({playerCount} of {maxPlayerCount}), {serverRegion}-{lobbyCode}"
                        : $"Private Party ({playerCount} of {maxPlayerCount})";
                }
            }
            else
            {
                state = "Singleplayer";
            }

            string imageKey = map.imageKey;
            DateTime dateTime = GameStatus.Instance.StartDateTime;

            try
            {
                client.SetPresence(new RichPresence()
                {
                    Details = details,
                    State = state,
                    Assets = new Assets()
                    {
                        LargeImageKey = imageKey,
                        LargeImageText = map.name,
                        SmallImageKey = "logo",
                        SmallImageText = "Phasmophobia"
                    },
                    Timestamps = new Timestamps()
                    {
                        Start = dateTime
                    }
                });

                SetDiscordRichPresencePreview(imageKey, details, state, dateTime);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error showing presence!\n\n{ex}", "Phasmophobia Discord Rich Presence");
            }
        }

        private void Deinitialize()
        {
            StopDiscordRPC();
        }

        #region Window
        private void Window_Closed(object sender, EventArgs e)
        {
            StopDiscordRPC();
        }
        #endregion

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
        public void SetPhasmophobiaAppStatus(PhasmophobiaAppStatus phasmophobiaAppStatus)
        {
            string status = "Offline";
            if (phasmophobiaAppStatus == PhasmophobiaAppStatus.Open) status = "Online";

            string foregroundColor = phasmophobiaAppStatus == PhasmophobiaAppStatus.Open ? "#4CAF50" : "#F44336";

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

            GameStatus.Instance.SetLobbyType(lobbyType);

            SetLobbyCodeIsEnabled(lobbyType == LobbyType.Private);
        }

        // Lobby Code
        public void SetLobbyCodeIsEnabled(bool isEnabled)
        {
            Cursor cursor = isEnabled ? Cursors.Arrow : Cursors.No;

            LobbyCodeBorder.Cursor = cursor;
            LobbyTypeGrid.IsEnabled = isEnabled;
        }

        public void SetLobbyRegion(string lobbyRegion)
        {
            if (lobbyRegion == string.Empty) lobbyRegion = "##";

            this.Dispatcher.Invoke(() =>
            {
                LobbyRegionTextBlock.Text = $"{lobbyRegion} —";
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

            GameStatus.Instance.SetLobbyCode(string.Empty);
        }

        private void LobbyCodeConfirmButton_Click(object sender, RoutedEventArgs e)
        {
            string lobbyCode = LobbyCodeTextBox.Text;
            if (lobbyCode.Length != 6) return;

            GameStatus.Instance.SetLobbyCode(lobbyCode);
        }
        // Max Players
        public void SetMaxPlayersIsEnabled(bool isEnabled)
        {
            Cursor cursor = isEnabled ? Cursors.Arrow : Cursors.No;

            MaxPlayersBorder.Cursor = cursor;
            MaxPlayersComboBox.IsEnabled = isEnabled;
        }

        private void MaxPlayersComboBox_SelectionChanged(object sender, System.Windows.Controls.SelectionChangedEventArgs e)
        {
            ComboBox comboBox = (ComboBox)sender;
            string value = ("" + comboBox.SelectedValue.ToString()).Split(":")[1].Trim();

            int maxPlayers = Convert.ToInt32(value);

            GameStatus.Instance.SetMaxPlayers(maxPlayers);
        }
        #endregion

        #region Window - Discord Rich Presence Preview
        public void SetDiscordRichPresencePreviewIsEnabled(bool isEnabled)
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
        
        public void SetDiscordRichPresencePreview(string image, string details, string state, DateTime dateTime)
        {
            SetDiscordRichPresencePreviewIsEnabled(true);

            Visibility detailsVisibility = details == string.Empty ? Visibility.Collapsed : Visibility.Visible;
            Visibility stateVisibility = state == string.Empty ? Visibility.Collapsed : Visibility.Visible;

            this.Dispatcher.Invoke(() =>
            {
                var bitmap = new BitmapImage(new Uri($"pack://application:,,,/img/{image}.png", UriKind.Absolute));
                Resources["PreviewImage"] = bitmap;

                DiscordRichPresencePreviewDetails.Text = details;
                DiscordRichPresencePreviewDetails.Visibility = detailsVisibility;

                DiscordRichPresencePreviewState.Text = state;
                DiscordRichPresencePreviewState.Visibility = stateVisibility;

                DiscordRichPresencePreviewElapsed.Text = "00:00 elapsed";
            });
        }
        #endregion
        
        private Brush GetBrushFromHex(string hex)
        {
            Color color = (Color)ColorConverter.ConvertFromString(hex);
            return new System.Windows.Media.SolidColorBrush(color);
        }
    }
}
