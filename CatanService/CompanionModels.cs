
using System.Collections.Generic;
using Catan10;


namespace CatanCompanion
{
    public class Player
    {
        public string Name { get; set; }
        public string PrimaryColor { get; set; }
        public string SecondaryColor { get; set; }
    }

    public class Tile
    {
        public ResourceType ResourceType { get; set; }
        public int Number { get; set; }
        public int Index { get; set; }
        public List<Building> OwnedBuildings { get; set; } = new List<Building>();
    }

    public class Building
    {
        public BuildingState State { get; set; }
        public BuildingLocation Location { get; set; }
        public int TileIndex { get; set; }
        public string Owner { get; set; }

    }

    public class Road
    {
        public string Owner { get; set; }
        public int TileIndex { get; set; }
        public RoadLocation Location { get; set; }
    }

    public class CompanionState
    {
        public List<Player> Players { get; set; } = new List<Player>();
        public List<Tile> Tiles { get; set; } = new List<Tile> ();
     //   public List<Building> Buildings { get; set; } = new List<Building>();
        public List<Road> Roads { get; set; }   = new List<Road> ();
    }
}