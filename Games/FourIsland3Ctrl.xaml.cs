using Catan.Proxy;
using System.Collections.Generic;

using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class FourIsland3Ctrl : UserControl, ICatanGameData
    {
        public FourIsland3Ctrl()
        {
            this.InitializeComponent();


        }
        public CatanHexPanel HexPanel => _HexPanel;

        #region ICatanGameData
        public string Description => _HexPanel.Description;
        public GameType GameType => _HexPanel.GameType;
        public int MaxCities => _HexPanel.MaxCities;
        public int MaxRoads => _HexPanel.MaxRoads;
        public int MaxSettlements => _HexPanel.MaxSettlements;
        public int MaxShips => _HexPanel.MaxShips;
        public int Index { get; set; } = -1;
        public List<TileCtrl> Tiles => _HexPanel.Tiles;

        public List<TileCtrl> DesertTiles => _HexPanel.DesertTiles;

        public CatanGames CatanGame{ get => CatanGames.Seafarers; }
        #endregion
    }
}
