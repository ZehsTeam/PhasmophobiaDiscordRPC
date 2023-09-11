namespace PhasmophobiaDiscordRPC
{
    public class Map
    {
        public MapType MapType;
        public string Name;
        public string LevelName;
        public string ImageKey;

        public Map(MapType mapType, string name, string levelName, string imageKey)
        {
            MapType = mapType;
            Name = name;
            LevelName = levelName;
            ImageKey = imageKey;
        }
    }
}
