namespace PhasmophobiaDiscordRPC
{
    public enum PlayerType
    {
        Other = 0,
        Host = 1,
    }

    public class PlayerData
    {
        public string Username;
        public string SteamId;
        public PlayerType PlayerType;

        public PlayerData(string username, string steamId, PlayerType playerType)
        {
            Username = username;
            SteamId = steamId;
            PlayerType = playerType;
        }
    }
}
