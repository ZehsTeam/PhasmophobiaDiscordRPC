using System;
using System.Collections.Generic;
using System.IO;
using System.Timers;
using System.Windows;

namespace PhasmophobiaDiscordRPC
{
    internal class PlayerLogReader
    {
        private string LogFilePath = Environment.GetFolderPath(Environment.SpecialFolder.UserProfile) + "\\AppData\\LocalLow\\Kinetic Games\\Phasmophobia\\Player.log";
        private List<string> lines = new List<string>();
        private int currentLine = 0;

        private int loopInterval = 1000;

        public void Initialize()
        {
            if (!File.Exists(LogFilePath))
            {
                MessageBox.Show($"ERROR!\n\nPlayer.log file at \"{LogFilePath}\" does not exist!", "Phasmophobia Discord Rich Presence");
                Application.Current.Shutdown();
                return;
            }

            UpdateLines();
            currentLine = lines.Count;

            StartLoop();
        }

        private void StartLoop()
        {
            Timer timer = new Timer();
            timer.Elapsed += new ElapsedEventHandler(Tick);
            timer.Interval = loopInterval;
            timer.Enabled = true;
        }

        private void Tick(object source, ElapsedEventArgs e)
        {
            ReadLines();
        }

        private void UpdateLines()
        {
            lines.Clear();

            using (FileStream stream = File.Open(LogFilePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
            {
                using (StreamReader reader = new StreamReader(stream))
                {
                    string line;

                    while ((line = reader.ReadLine()) != null)
                    {
                        lines.Add(line);
                    }
                }
            }
        }

        private void ReadLines()
        {
            UpdateLines();
            int amount = lines.Count;

            for (int i = currentLine; i < amount; i++)
            {
                CheckLine(lines[i]);
            }

            currentLine = amount;
        }

        private void CheckLine(string line)
        {
            if (line.StartsWith("Connected to master server in offline mode")) ParseServerModeOffline();
            if (line.StartsWith("Connected to master server successfully")) ParseServerModeOnline(line);
            if (line.StartsWith("Loaded Level")) ParseLoadedLevel(line);
            if (line.StartsWith("Applying difficulty")) ParseAppliedDifficulty(line);
            if (line.StartsWith("Room Created")) ParseRoomCreated(line);
            if (line.StartsWith("Joined room")) ParseJoinedRoom(line);
            if (line.StartsWith("Recieved Player Info")) ParseRecievedPlayerInfo(line);
            if (line.StartsWith("Player Entered")) ParsePlayerEntered(line);
            if (line.StartsWith("Player Left")) ParsePlayerLeft(line);
            if (line.StartsWith("Left Room")) ParseLeftRoom(line);
            if (line.StartsWith("Memory Statistics")) ParsePhasmophobiaClosed();
        }

        private void ParseServerModeOffline()
        {
            GameStatus.Instance.OnServerModeOffline();
        }

        private void ParseServerModeOnline(string line)
        {
            string region = line.Replace("/*", "").Split(":")[1].Trim();

            if (region == "us") region = "na";
            if (region == "asia") region = "as";

            GameStatus.Instance.OnServerModeOnline(region.ToUpper());
        }

        private void ParseLoadedLevel(string line)
        {
            CheckPhasmophobiaStarted();

            string[] words = line.Replace("Main Menu", "MainMenu").Split(' ');

            string levelName = words[2];
            int playerCount = int.Parse(words[5]);

            Difficulty difficulty = GetDifficultyFromLine(line);

            GameStatus.Instance.OnLoadedLevel(levelName, difficulty, playerCount);
        }

        private void ParseAppliedDifficulty(string line)
        {
            GameStatus.Instance.OnAppliedDifficulty(GetDifficultyFromLine(line));
        }

        private void ParseRoomCreated(string line)
        {
            GameStatus.Instance.OnRoomCreated();
        }

        private void ParseJoinedRoom(string line)
        {
            CheckPhasmophobiaStarted();

            GameStatus.Instance.OnJoinedRoom();
        }

        private void ParseRecievedPlayerInfo(string line)
        {
            string username = line.Substring(line.IndexOf("|") + 1).Trim();

            GameStatus.Instance.OnRecievedPlayerInfo(username);
        }

        private void ParsePlayerEntered(string line)
        {
            string username = line.Substring(line.IndexOf("|") + 1).Trim();

            GameStatus.Instance.OnPlayerEntered(username);
        }

        private void ParsePlayerLeft(string line)
        {
            string username = line.Substring(line.IndexOf("|") + 1).Trim();

            GameStatus.Instance.OnPlayerLeft(username);
        }

        private void ParseLeftRoom(string line)
        {
            GameStatus.Instance.OnLeftRoom();
        }

        private void CheckPhasmophobiaStarted()
        {
            if (GameStatus.Instance.PhasmophobiaAppStatus == PhasmophobiaAppStatus.Open) return;

            GameStatus.Instance.OnPhasmophobiaStarted();
        }

        private void ParsePhasmophobiaClosed()
        {
            GameStatus.Instance.OnPhasmophobiaClosed();
        }
        
        private Difficulty GetDifficultyFromLine(string line)
        {
            Difficulty difficulty = Difficulty.None;

            if (line.Contains("Amateur")) difficulty = Difficulty.Amateur;
            if (line.Contains("Intermediate")) difficulty = Difficulty.Intermediate;
            if (line.Contains("Professional")) difficulty = Difficulty.Professional;
            if (line.Contains("Nightmare")) difficulty = Difficulty.Nightmare;
            if (line.Contains("Insanity")) difficulty = Difficulty.Insanity;

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
