namespace PhasmophobiaDiscordRPC
{
    public class Map
    {
        public MapType type;
        public string name = "";
        public string imageKey = "";
        public string levelName = "";

        public Map(MapType type, string name, string imageKey, string levelName)
        {
            this.type = type;
            this.name = name;
            this.imageKey = imageKey;
            this.levelName = levelName;
        }
    }
}
