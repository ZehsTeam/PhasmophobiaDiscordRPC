using System;
using System.Collections.Generic;
using System.Diagnostics;
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

    public class LogData
    {
        public LogType LogType;
        public string Line;

        public LogData(LogType logType, string line)
        {
            LogType = logType;
            Line = line;
        }
    }

    internal class PlayerLogReader
    {
        public static PlayerLogReader Instance;

        private string _logFilePath;
        private int _lineIndex;
        private int _lineCount;

        private List<LogData> _logHistory = new List<LogData>();

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

        private void ReadLine(string line, bool update)
        {
            LogType logType = GetLogTypeFromLine(line);
            if (logType == LogType.None) return;

            _logHistory.Add(new LogData(logType, line));

            string[] data = ParseLogType(logType, line);
            UpdateGameStateForLog(logType, data, update);
        }

        private void ReadLine(string line)
        {
            ReadLine(line, true);
        }

        public void Tick()
        {
            ReadNewLines();
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

        private string[] ParseLogType(LogType logType, string line)
        {
            switch (logType)
            {
                case LogType.ServerModeOffline:
                    return ParseServerModeOfflineLine(line);
                case LogType.ServerModeOnline:
                    return ParseServerModeOnlineLine(line);
                case LogType.LoadedLevel:
                    return ParseLoadedLevelLine(line);
                case LogType.ApplyingDifficulty:
                    return ParseApplyingDifficultyLine(line);
                case LogType.RoomCreated:
                    return ParseRoomCreatedLine(line);
                case LogType.JoinedRoom:
                    return ParseJoinedRoomLine(line);
                case LogType.ReceivedPlayerInfo:
                    return ParseReceivedPlayerInfoLine(line);
                case LogType.ReceivedAllPlayerInfo:
                    return ParseReceivedAllPlayerInfoLine(line);
                case LogType.PlayerEntered:
                    return ParsePlayerEnteredLine(line);
                case LogType.PlayerLeft:
                    return ParsePlayerLeftLine(line);
                case LogType.LeftRoom:
                    return ParseLeftRoomLine(line);
                case LogType.HostChanged:
                    return ParseHostChangedLine(line);
                case LogType.HostAndLocalPlayer:
                    return ParseHostAndLocalPlayerLine(line);
            }

            return new string[0];
        }

        private void UpdateGameStateForLog(LogType logType, string[] data, bool update)
        {
            Debug.WriteLine($"Updating game state for {Enum.GetName(logType)} with data: {CovertDataToDebugString(data)}");

            switch (logType)
            {
                case LogType.ServerModeOffline:
                    GameStateManager.Instance.OnServerModeOffline(data, update);
                    break;
                case LogType.ServerModeOnline:
                    GameStateManager.Instance.OnServerModeOnline(data, update);
                    break;
                case LogType.LoadedLevel:
                    GameStateManager.Instance.OnLoadedLevel(data, update);
                    break;
                case LogType.ApplyingDifficulty:
                    GameStateManager.Instance.OnApplyingDifficulty(data, update);
                    break;
                case LogType.RoomCreated:
                    GameStateManager.Instance.OnRoomCreated(data, update);
                    break;
                case LogType.JoinedRoom:
                    GameStateManager.Instance.OnJoinedRoom(data, update);
                    break;
                case LogType.ReceivedPlayerInfo:
                    GameStateManager.Instance.OnReceivedPlayerInfo(data, update);
                    break;
                case LogType.ReceivedAllPlayerInfo:
                    GameStateManager.Instance.OnReceivedAllPlayerInfo(data, update);
                    break;
                case LogType.PlayerEntered:
                    GameStateManager.Instance.OnPlayerEntered(data, update);
                    break; ;
                case LogType.PlayerLeft:
                    GameStateManager.Instance.OnPlayerLeft(data, update);
                    break;
                case LogType.LeftRoom:
                    GameStateManager.Instance.OnLeftRoom(data, update);
                    break;
                case LogType.HostChanged:
                    GameStateManager.Instance.OnHostChanged(data, update);
                    break;
                case LogType.HostAndLocalPlayer:
                    GameStateManager.Instance.OnHostAndLocalPlayer(data, update);
                    break;
            }
        }

        private string CovertDataToDebugString(string[] data)
        {
            string result = "";

            int length = data.Length;
            for (int i = 0; i < length; i++)
            {
                result += data[i].ToString();
                if (i < length - 1) result += ", ";
            }

            return result;
        }

        #region Line Parsing
        private string[] ParseServerModeOfflineLine(string line)
        {
            return new string[0];
        }

        private string[] ParseServerModeOnlineLine(string line)
        {
            string region = line.Replace("/*", "").Split(":")[1].Trim();

            if (region == "us") region = "na";
            if (region == "asia") region = "as";

            return new string[1]
            {
                region.ToUpper()
            };
        }

        private string[] ParseLoadedLevelLine(string line)
        {
            string[] words = line.Split("|");

            string levelName = words[0].Split(":")[1].Replace(" ", "").Trim();
            string playerCount = words[1].Split(":")[1].Trim();
            string isHost = words[2].Split(":")[1].Trim();
            Difficulty difficulty = GetDifficultyFromLine(words[3].Split(":")[1]);

            return new string[4]
            {
                levelName,
                playerCount,
                isHost,
                ((int)difficulty).ToString()
            };
        }

        private string[] ParseApplyingDifficultyLine(string line)
        {
            Difficulty difficulty = GetDifficultyFromLine(line);

            return new string[1]
            {
                ((int)difficulty).ToString()
            };
        }

        private string[] ParseRoomCreatedLine(string line)
        {
            return new string[0];
        }

        private string[] ParseJoinedRoomLine(string line)
        {
            return new string[0];
        }

        private string[] ParseReceivedPlayerInfoLine(string line)
        {
            string[] words = line.Split("|");

            string username = words[1].Trim();
            string steamId = words[0].Split(':')[1].Trim();
            string playerType = ((int)PlayerType.Other).ToString();

            return new string[3]
            {
                username,
                steamId,
                playerType
            };
        }

        private string[] ParseReceivedAllPlayerInfoLine(string line)
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

            return new string[2]
            {
                hostUsername,
                localUsername
            };
        }

        private string[] ParsePlayerEnteredLine(string line)
        {
            string[] words = line.Split("|");

            string username = words[1].Trim();
            string steamId = words[0].Split(':')[1].Trim();
            string playerType = ((int)PlayerType.Other).ToString();

            return new string[3]
            {
                username,
                steamId,
                playerType
            };
        }

        private string[] ParsePlayerLeftLine(string line)
        {
            string[] words = line.Split("|");

            string username = words[1].Trim();
            string steamId = words[0].Split(':')[1].Trim();
            string playerType = ((int)PlayerType.Other).ToString();

            return new string[3]
            {
                username,
                steamId,
                playerType
            };
        }

        private string[] ParseLeftRoomLine(string line)
        {
            return new string[0];
        }
        
        private string[] ParseHostChangedLine(string line)
        {
            string[] words = line.Split("|");

            string steamId = words[0].Split(':')[1].Trim();
            string username = words[1].Trim();

            return new string[2]
            {
                steamId,
                username
            };
        }
        
        private string[] ParseHostAndLocalPlayerLine(string line)
        {
            string username = line.Split(':')[1].Trim();

            return new string[1]
            {
                username
            };
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
