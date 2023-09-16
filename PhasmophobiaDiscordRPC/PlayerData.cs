namespace PhasmophobiaDiscordRPC
{
    public enum PlayerType
    {
        Other = 0,
        Host = 1,
    }

    public class PlayerData
    {
        public string Username { get; set; }
        public string SteamId { get; set; }
        public PlayerType PlayerType { get; set; }
        public bool IsHost
        {
            get
            {
                return PlayerType == PlayerType.Host;
            }
        }

        public PlayerData(string username, string steamId, PlayerType playerType)
        {
            Username = username;
            SteamId = steamId;
            PlayerType = playerType;
        }
    }
}
