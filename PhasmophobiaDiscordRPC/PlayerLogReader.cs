using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Windows;

namespace PhasmophobiaDiscordRPC
{
    public enum LogType
    {
        None,
        ServerModeOffline,
        ServerModeOnline,
        LoadedLevel,
        ApplyingDifficulty,
        RoomCreated,
        JoinedRoom,
        ReceivedPlayerInfo,
        ReceivedAllPlayerInfo,
        PlayerEntered,
        PlayerLeft,
        LeftRoom,
        HostChanged,
        HostAndLocalPlayer
    }

    internal class PlayerLogReader
    {
        public static PlayerLogReader Instance;

        private string _logFilePath;
        private int _lineIndex;
        private int _lineCount;

        public PlayerLogReader()
        {
            PlayerLogReader.Instance = this;
        }

        public void Initialize()
        {
            _logFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\LocalLow\\Kinetic Games\\Phasmophobia\\Player.log";
            _lineIndex = 0;
            _lineCount = 0;

            if (!File.Exists(_logFilePath))
            {
                MessageBox.Show($"Error! File: \"Player.log\" doesn't exist at \"{_logFilePath}\"\n\n", "Phasmophobia Rich Presence");
                Application.Current.Shutdown();
                return;
            }

            GameStateManager.Instance.OnPhasmophobiaAppStateChanged += OnPhasmophobiaAppStateChanged;

            if (GameStateManager.Instance.GetPhasmophobiaAppState() == PhasmophobiaAppState.Open)
            {
                ReadAllLines();
                GameStateManager.Instance.UpdatePresence();
                GameStateManager.Instance.UpdatePlayersListUI();
            }
        }

        public void Deinitialize()
        {
            GameStateManager.Instance.OnPhasmophobiaAppStateChanged -= OnPhasmophobiaAppStateChanged;
        }

        public void Tick()
        {
            ReadNewLines();
        }

        #region Phasmophobia App State
        private void OnPhasmophobiaAppStateChanged(PhasmophobiaAppState phasmophobiaAppState)
        {
            if (phasmophobiaAppState == PhasmophobiaAppState.Open) OnPhasmophobiaOpened();
            if (phasmophobiaAppState == PhasmophobiaAppState.Closed) OnPhasmophobiaClosed();
        }

        private void OnPhasmophobiaOpened()
        {
            ReadAllLines();
            GameStateManager.Instance.UpdatePresence();
            GameStateManager.Instance.UpdatePlayersListUI();
        }

        private void OnPhasmophobiaClosed()
        {

        }
        #endregion

        private void ReadAllLines()
        {
            _lineIndex = 0;

            using (FileStream stream = File.Open(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        ReadLine(line, false);
                        _lineIndex++;
                    }
                }
            }

            _lineCount = _lineIndex;
        }

        private void ReadNewLines()
        {
            using (FileStream stream = File.Open(_logFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;
                    int i = 0;
                    int startLineIndex = _lineIndex;

                    while ((line = reader.ReadLine()) != null)
                    {
                        if (i >= startLineIndex)
                        {
                            ReadLine(line);
                            _lineIndex++;
                        }

                        i++;
                    }

                    if (i < _lineCount) _lineIndex = 0; // Player.log reset

                    _lineCount = _lineIndex;
                }
            }
        }

        private void ReadLine(string line, bool update = true)
        {
            LogType logType = GetLogTypeFromLine(line);
            if (logType == LogType.None) return;

            // Get line data and update GameStateManager
            ParseLogType(logType, line, update);
        }

        private LogType GetLogTypeFromLine(string line)
        {
            if (line.StartsWith("Connected to master server in offline mode")) return LogType.ServerModeOffline;
            if (line.StartsWith("Connected to master server successfully")) return LogType.ServerModeOnline;
            if (line.StartsWith("Loaded Level")) return LogType.LoadedLevel;
            if (line.StartsWith("Applying difficulty")) return LogType.ApplyingDifficulty;
            if (line.StartsWith("Room Created")) return LogType.RoomCreated;
            if (line.StartsWith("Joined room")) return LogType.JoinedRoom;
            if (line.StartsWith("Recieved Player Info")) return LogType.ReceivedPlayerInfo;
            if (line.StartsWith(" | Host Player:")) return LogType.ReceivedAllPlayerInfo;
            if (line.StartsWith("Player Entered")) return LogType.PlayerEntered;
            if (line.StartsWith("Player Left")) return LogType.PlayerLeft;
            if (line.StartsWith("Left Room")) return LogType.LeftRoom;
            if (line.StartsWith("Host Changed")) return LogType.HostChanged;
            if (line.StartsWith("Host & Local Player")) return LogType.HostAndLocalPlayer;

            return LogType.None;
        }

        private void ParseLogType(LogType logType, string line, bool update)
        {
            switch (logType)
            {
                case LogType.ServerModeOffline:
                    ParseServerModeOfflineLine(line, update);
                    break;
                case LogType.ServerModeOnline:
                    ParseServerModeOnlineLine(line, update);
                    break;
                case LogType.LoadedLevel:
                    ParseLoadedLevelLine(line, update);
                    break;
                case LogType.ApplyingDifficulty:
                    ParseApplyingDifficultyLine(line, update);
                    break;
                case LogType.RoomCreated:
                    ParseRoomCreatedLine(line, update);
                    break;
                case LogType.JoinedRoom:
                    ParseJoinedRoomLine(line, update);
                    break;
                case LogType.ReceivedPlayerInfo:
                    ParseReceivedPlayerInfoLine(line, update);
                    break;
                case LogType.ReceivedAllPlayerInfo:
                    ParseReceivedAllPlayerInfoLine(line, update);
                    break;
                case LogType.PlayerEntered:
                    ParsePlayerEnteredLine(line, update);
                    break;
                case LogType.PlayerLeft:
                    ParsePlayerLeftLine(line, update);
                    break;
                case LogType.LeftRoom:
                    ParseLeftRoomLine(line, update);
                    break;
                case LogType.HostChanged:
                    ParseHostChangedLine(line, update);
                    break;
                case LogType.HostAndLocalPlayer:
                    ParseHostAndLocalPlayerLine(line, update);
                    break;
            }
        }

        #region Line Parsing
        private void ParseServerModeOfflineLine(string line, bool update)
        {
            GameStateManager.Instance.OnServerModeOffline(update);
        }

        private void ParseServerModeOnlineLine(string line, bool update)
        {
            string region = line.Replace("/*", "").Split(":")[1].Trim();

            if (region == "us") region = "na";
            if (region == "asia") region = "as";

            GameStateManager.Instance.OnServerModeOnline(region.ToUpper(), update);
        }

        private void ParseLoadedLevelLine(string line, bool update)
        {
            string[] words = line.Split("|");

            string levelName = words[0].Split(":")[1].Replace(" ", "").Trim();
            int playerCount = int.Parse(words[1].Split(":")[1].Trim());
            bool isHost = words[2].Split(":")[1].Trim() == "True";
            Difficulty difficulty = GetDifficultyFromLine(words[3].Split(":")[1]);

            GameStateManager.Instance.OnLoadedLevel(levelName, playerCount, isHost, difficulty, update);
        }

        private void ParseApplyingDifficultyLine(string line, bool update)
        {
            Difficulty difficulty = GetDifficultyFromLine(line);

            GameStateManager.Instance.OnApplyingDifficulty(difficulty, update);
        }

        private void ParseRoomCreatedLine(string line, bool update)
        {
            GameStateManager.Instance.OnRoomCreated(update);
        }

        private void ParseJoinedRoomLine(string line, bool update)
        {
            GameStateManager.Instance.OnJoinedRoom(update);
        }

        private void ParseReceivedPlayerInfoLine(string line, bool update)
        {
            string[] words = line.Split("|");

            string username = words[1].Trim();
            string steamId = words[0].Split(':')[1].Trim();

            GameStateManager.Instance.OnReceivedPlayerInfo(username, steamId, update);
        }

        private void ParseReceivedAllPlayerInfoLine(string line, bool update)
        {
            List<string> words = line.Split("|").ToList();
            words.RemoveAt(0);

            string hostUsername = string.Empty;
            string localUsername = string.Empty;

            foreach (string word in words)
            {
                string playerTypeString = word.Split(":")[0].Trim();
                string username = word.Split(":")[1].Trim();

                if (playerTypeString.Contains("Host"))
                {
                    hostUsername = username;
                    break;
                }

                if (playerTypeString.Contains("Local"))
                {
                    localUsername = username;
                    break;
                }
            }

            GameStateManager.Instance.OnReceivedAllPlayerInfo(hostUsername, localUsername, update);
        }

        private void ParsePlayerEnteredLine(string line, bool update)
        {
            string[] words = line.Split("|");

            string username = words[1].Trim();
            string steamId = words[0].Split(':')[1].Trim();

            GameStateManager.Instance.OnPlayerEntered(username, steamId, update);
        }

        private void ParsePlayerLeftLine(string line, bool update)
        {
            string[] words = line.Split("|");

            string username = words[1].Trim();
            string steamId = words[0].Split(':')[1].Trim();

            GameStateManager.Instance.OnPlayerLeft(username, steamId, update);
        }

        private void ParseLeftRoomLine(string line, bool update)
        {
            GameStateManager.Instance.OnLeftRoom(update);
        }
        
        private void ParseHostChangedLine(string line, bool update)
        {
            string[] words = line.Split("|");

            string steamId = words[0].Split(':')[1].Trim();
            string username = words[1].Trim();

            GameStateManager.Instance.OnHostChanged(username, steamId, update);
        }
        
        private void ParseHostAndLocalPlayerLine(string line, bool update)
        {
            string username = line.Split(':')[1].Trim();

            GameStateManager.Instance.OnHostAndLocalPlayer(username, update);
        }
        #endregion

        private Difficulty GetDifficultyFromLine(string line)
        {
            Difficulty difficulty = Difficulty.None;

            if (line.Contains("Amateur")) difficulty = Difficulty.Amateur;
            if (line.Contains("Intermediate")) difficulty = Difficulty.Intermediate;
            if (line.Contains("Professional")) difficulty = Difficulty.Professional;
            if (line.Contains("Nightmare")) difficulty = Difficulty.Nightmare;
            if (line.Contains("Insanity")) difficulty = Difficulty.Insanity;
            if (line.Contains("Training")) difficulty = Difficulty.Training;

            if (difficulty == Difficulty.None)
            {
                bool isChallengeMode = line.Substring(line.IndexOf("y:") + 2, 2) != "  ";

                if (isChallengeMode) // Is Challenge Mode
                {
                    difficulty = Difficulty.ChallengeMode;
                }
                else // Is Custom
                {
                    difficulty = Difficulty.Custom;
                }
            }

            return difficulty;
        }
    }
}
