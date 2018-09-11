using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class RegularGameCtrl : UserControl, ICatanGameData
    {


        public RegularGameCtrl()
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

        public List<TileCtrl> Tiles => _HexPanel.Tiles;

        public List<TileCtrl> DesertTiles => _HexPanel.DesertTiles;
        #endregion
    }
}
