using System.Collections.Generic;

namespace PhasmophobiaDiscordRPC
{
    public enum MapType
    {
        None,
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
        CampWoodwind
    }

    public static class MapManager
    {
        public static List<Map> maps = new List<Map>();

        public static void Initialize()
        {
            maps.Add(new Map(MapType.Tanglewood, "6 Tanglewood Drive", "map-6_tanglewood_drive", "TanglewoodStreetHouse"));
            maps.Add(new Map(MapType.EdgefieldRoad, "42 Edgefield Road", "map-42_edgefield_road", "EdgefieldStreetHouse"));
            maps.Add(new Map(MapType.RidgeviewCourt, "10 Ridgeview Court", "map-10_ridgeview_court", "RidgeviewRoadHouse"));
            maps.Add(new Map(MapType.GraftonFarmhouse, "Grafton Farmhouse", "map-grafton_farmhouse", "GraftonFarmhouse"));
            maps.Add(new Map(MapType.WillowStreet, "13 Willow Street", "map-13_willow_street", "WillowStreetHouse"));
            maps.Add(new Map(MapType.BrownstoneHighSchool, "Brownstone High School", "map-brownstone_high_school", "BrownstoneHighSchool"));
            maps.Add(new Map(MapType.BleasdaleFarmhouse, "Bleasdale Farmhouse", "map-bleasdale_farmhouse", "BleasdaleFarmhouse"));
            maps.Add(new Map(MapType.SunnyMeadows, "Sunny Meadows", "map-sunny_meadows", "Asylum"));
            maps.Add(new Map(MapType.SunnyMeadowsRestricted, "Sunny Meadows Restricted", "map-sunny_meadows_restricted", "Asylum"));
            maps.Add(new Map(MapType.Prison, "Prison", "map-prison", "Prison"));
            maps.Add(new Map(MapType.MapleLodgeCampsite, "Maple Lodge Campsite", "map-maple_lodge_campsite", "MapleLodgeCampsite"));
            maps.Add(new Map(MapType.CampWoodwind, "Camp Woodwind", "map-camp_woodwind", "CampWoodwind"));
        }

        public static Map GetMapByMapType(MapType mapType)
        {
            Map resultMap = null;

            foreach (Map map in maps)
            {
                if (resultMap != null) continue;
                if (map.type == mapType) resultMap = map;
            }

            return resultMap;
        }

        public static Map GetMapByLevelName(string levelName)
        {
            Map resultMap = null;

            foreach (Map map in maps)
            {
                if (resultMap != null) continue;
                if (map.levelName == levelName) resultMap = map;
            }

            return resultMap;
        }
    }
}
