﻿using System.Collections.Generic;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class ExpansionCtrl : UserControl, ICatanGameData
    {
        public ExpansionCtrl()
        {
            this.InitializeComponent();
        }
        #region ICatanGameData
        public CatanHexPanel HexPanel => _HexPanel;
        public string Description => _HexPanel.Description;
        public GameType GameType => _HexPanel.GameType;
        public int MaxCities => _HexPanel.MaxCities;
        public int MaxRoads => _HexPanel.MaxRoads;
        public int MaxSettlements => _HexPanel.MaxSettlements;
        public int MaxShips => _HexPanel.MaxShips;
        public List<TileCtrl> Tiles => _HexPanel.Tiles;
        public int Index { get; set; } = -1;
        public List<TileCtrl> DesertTiles => _HexPanel.DesertTiles;
        #endregion
    }
}
