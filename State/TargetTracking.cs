using System;

namespace Catan10
{
    public class Target
    {
        public PlayerModel Player { get; private set; } = null;

        public int ResourcePotential { get; private set; } = 0;

        public TileCtrl Tile { get; private set; } = null;

        public Target(PlayerModel p, TileCtrl t)
        {
            Player = p;
            Tile = t;
            foreach (var building in Tile.OwnedBuilding)
            {
                if (building.BuildingState == BuildingState.Settlement)
                {
                    ResourcePotential++;
                }
                else if (building.BuildingState == BuildingState.City)
                {
                    ResourcePotential += 2;
                }
                else
                {
                    throw new Exception("This building shouldn't be owned");
                }
            }

            ResourcePotential *= Tile.Pips;

            if (ResourcePotential > 0)
            {
                //
                //  check to see if this player has 2:1 in a resource from this tile

                foreach (var harbor in Player.GameData.OwnedHarbors)
                {
                    if (StaticHelpers.HarborTypeToResourceType(harbor.HarborType) == Tile.ResourceType)
                    {
                        ResourcePotential *= 2;
                    }
                }
            }
        }

        public override string ToString()
        {
            return $"{Player,-15} | {Tile,-15} | {ResourcePotential}";
        }
    }
}
