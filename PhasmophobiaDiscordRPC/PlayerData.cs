namespace PhasmophobiaDiscordRPC
{
    public enum PlayerType
    {
        Local = 0,
        Host = 1,
        HostAndLocal = 2,
        Other = 3,
    }

    public class PlayerData
    {
        public string Username { get; set; }
        public string SteamId { get; set; }
        public PlayerType PlayerType { get; set; }

        // For ListViewPlayerItem Trigger Bindings
        public bool IsLocal {  get { return (PlayerType == PlayerType.Local || PlayerType == PlayerType.HostAndLocal); } }
        public bool IsHost { get { return (PlayerType == PlayerType.Host || PlayerType == PlayerType.HostAndLocal); } }
        public bool IsSteamIdEmpty { get { return SteamId == string.Empty; } }

        public PlayerData(string username, string steamId, PlayerType playerType)
        {
            Username = username;
            SteamId = steamId;
            PlayerType = playerType;
        }
    }
}
