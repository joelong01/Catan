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
        public CatanHexPanel HexPanel { get { return _HexPanel; } }

        #region ICatanGameData
        public string Description
        {
            get
            {
                return _HexPanel.Description;
            }
        }
        public GameType GameType
        {
            get
            {
                return _HexPanel.GameType;
            }
        }
        public int MaxCities
        {
            get
            {
                return _HexPanel.MaxCities;
            }
        }
        public int MaxRoads
        {
            get
            {
                return _HexPanel.MaxRoads;
            }
        }
        public int MaxSettlements
        {
            get
            {
                return _HexPanel.MaxSettlements;
            }
        }
        public int MaxShips
        {
            get
            {
                return _HexPanel.MaxShips;
            }
        }

        public List<TileCtrl> Tiles
        {
            get
            {
                return _HexPanel.Tiles;
            }
        }

        public List<TileCtrl> DesertTiles
        {
            get
            {
                return _HexPanel.DesertTiles;
            }
        }
        #endregion
    }
}
