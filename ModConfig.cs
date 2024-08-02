using System.Collections.Generic;

namespace FerrisTheRustacean
{
    internal class ModConfig
    {
        public int NumGroups { get; set; } = 4;
        public int MaxNumCrabsPerGroup { get; set; } = 3;

        public int FerrisSkitterDistance { get; set; } = 3;

        public HashSet<string> AllowedLocations { get; set; } = new HashSet<string>()
        {
            "Beach",
            "IslandWest",
            "IslandSouth",
            "IslandSouthEast",
        };

    }
}
