using System.Collections.Generic;

namespace PhasmophobiaDiscordRPC
{
    public enum MapType
    {
        None,
        MainMenu,
        Tanglewood,
        EdgefieldRoad,
        RidgeviewCourt,
        GraftonFarmhouse,
        WillowStreet,
        BrownstoneHighSchool,
        BleasdaleFarmhouse,
        SunnyMeadows,
        SunnyMeadowsRestricted,
        Prison,
        MapleLodgeCampsite,
        CampWoodwind,
        Training
    }

    public static class MapDatabase
    {
        public static List<Map> maps;

        public static void Initialize()
        {
            maps = new List<Map>
            {
                new Map(MapType.MainMenu, "Main Menu", "MainMenu", "logo"),
                new Map(MapType.Tanglewood, "6 Tanglewood Drive", "TanglewoodStreetHouse", "map-6_tanglewood_drive"),
                new Map(MapType.EdgefieldRoad, "42 Edgefield Road", "EdgefieldStreetHouse", "map-42_edgefield_road"),
                new Map(MapType.RidgeviewCourt, "10 Ridgeview Court", "RidgeviewRoadHouse", "map-10_ridgeview_court"),
                new Map(MapType.GraftonFarmhouse, "Grafton Farmhouse", "GraftonFarmhouse", "map-grafton_farmhouse"),
                new Map(MapType.WillowStreet, "13 Willow Street", "WillowStreetHouse", "map-13_willow_street"),
                new Map(MapType.BrownstoneHighSchool, "Brownstone High School", "BrownstoneHighSchool", "map-brownstone_high_school"),
                new Map(MapType.BleasdaleFarmhouse, "Bleasdale Farmhouse", "BleasdaleFarmhouse", "map-bleasdale_farmhouse"),
                new Map(MapType.SunnyMeadows, "Sunny Meadows", "Asylum", "map-sunny_meadows"),
                new Map(MapType.SunnyMeadowsRestricted, "Sunny Meadows Restricted", "Asylum", "map-sunny_meadows_restricted"),
                new Map(MapType.Prison, "Prison", "Prison", "map-prison"),
                new Map(MapType.MapleLodgeCampsite, "Maple Lodge Campsite", "MapleLodgeCampsite", "map-maple_lodge_campsite"),
                new Map(MapType.CampWoodwind, "Camp Woodwind", "CampWoodwind", "map-camp_woodwind"),
                new Map(MapType.Training, "Training", "TutorialV2", "map-training")
            };
        }

        public static MapType GetMapTypeByLevelName(string levelName)
        {
            Map map = GetMapByLevelName(levelName);
            if (map == null) return MapType.None;

            return map.MapType;
        }

        public static Map GetMapByMapType(MapType mapType)
        {
            Map resultMap = GetUnknownMap();

            foreach (Map map in maps)
            {
                if (map.MapType == mapType)
                {
                    resultMap = map;
                    break;
                }
            }

            return resultMap;
        }

        public static Map GetMapByLevelName(string levelName)
        {
            Map resultMap = GetUnknownMap();

            foreach (Map map in maps)
            {
                if (map.LevelName == levelName)
                {
                    resultMap = map;
                    break;
                }
            }

            return resultMap;
        }
    
        public static Map GetUnknownMap()
        {
            return new Map(MapType.None, "Unkown", "", "map-random");
        }
    }
}
