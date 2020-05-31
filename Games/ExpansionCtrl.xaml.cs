using Catan.Proxy;
using System.Collections.Generic;

using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class ExpansionCtrl : UserControl, ICatanGameData
    {
        public ExpansionCtrl()
        {
            this.InitializeComponent(); this.InitializeComponent();
            GameData.AllowShips = _HexPanel.AllowShips;
            GameData.MaxRoads = _HexPanel.MaxRoads;
            GameData.MaxCities = _HexPanel.MaxCities;
            GameData.MaxSettlements = _HexPanel.MaxSettlements;
            GameData.MaxResourceAllocated = _HexPanel.MaxResourceAllocated;
            GameData.AllowShips = _HexPanel.AllowShips;
            GameData.Knight = _HexPanel.Knights;
            GameData.VictoryPoint = _HexPanel.VictoryPoints;
            GameData.YearOfPlenty = _HexPanel.YearOfPlenty;
            GameData.RoadBuilding = _HexPanel.RoadBuilding;
            GameData.Monopoly = _HexPanel.Monopoly;
            GameData.MaxShips = _HexPanel.MaxShips;
            GameData.BuildDevCardList();
        }

        public CatanHexPanel HexPanel => _HexPanel;

        #region ICatanGameData
        public int Index { get; set; } = -1;
        public string Description => _HexPanel.Description;
        public GameType GameType => _HexPanel.GameType;


        public List<TileCtrl> Tiles => _HexPanel.Tiles;

        public List<TileCtrl> DesertTiles => _HexPanel.DesertTiles;
        public CatanGames CatanGame { get => CatanGames.Regular; }
        public CatanGameData GameData { get; } = new CatanGameData();
        #endregion
    }
}
