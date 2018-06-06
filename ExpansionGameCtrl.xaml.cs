using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{


    public sealed partial class ExpansionGameCtrl : UserControl,  INotifyPropertyChanged
    {
        int[] _numberSequence = { 8, 11, 11, 10, 6, 3, 8, 4, 8, 2, 5, 4, 6, 3, 9, 5, 4, 9, 5, 9, 10, 11, 12, 10, 2, 6, 12, 3 };
        List<TileCtrl> _tiles = new List<TileCtrl>();
        List<Harbor> _harbors = new List<Harbor>();
        string _name = "Expansion";

        ResourceType[] _resources = { ResourceType.Wood,
                                      ResourceType.Ore,
                                      ResourceType.Brick,
                                      ResourceType.Brick,
                                      ResourceType.Brick,
                                      ResourceType.Brick,
                                      ResourceType.Wheat,
                                      ResourceType.Wood,
                                      ResourceType.Ore,
                                      ResourceType.Wheat,
                                      ResourceType.Wheat,
                                      ResourceType.Wheat,
                                      ResourceType.Wheat,
                                      ResourceType.Wheat,
                                      ResourceType.Sheep,
                                      ResourceType.Brick,
                                      ResourceType.Desert,
                                      ResourceType.Ore,
                                      ResourceType.Sheep,
                                      ResourceType.Sheep,
                                      ResourceType.Wood,
                                      ResourceType.Ore,
                                      ResourceType.Sheep,
                                      ResourceType.Sheep,
                                      ResourceType.Wood,
                                      ResourceType.Sheep,
                                      ResourceType.Desert,
                                      ResourceType.Wood,
                                      ResourceType.Ore,
                                      ResourceType.Wood  };

        HarborType[] _harborTypes = { HarborType.Sheep, HarborType.ThreeForOne, HarborType.Wood, HarborType.Brick,
                                      HarborType.Sheep, HarborType.ThreeForOne, HarborType.ThreeForOne, HarborType.ThreeForOne,
                                      HarborType.Wheat, HarborType.Ore, HarborType.ThreeForOne };

        int[] _tileOrderExpansionGame = {17, 22, 26, 29, 28, 27, 23, 18, 12, 7, 3, 0, 1, 2, 6, 11, 16, 21, 25, 24, 19, 13, 8, 4, 5, 10, 15, 20, 14, 9 };

        public ExpansionGameCtrl()
        {
            this.InitializeComponent();

            
        }
        public List<TileCtrl> Tiles
        {
            get { return _tiles; }
        }

        public List<Harbor> Harbors
        {
            get { return _harbors; }
        }

        public int[] NumberSequence
        {
            get { return _numberSequence; }
        }

        public double Zoom
        {
            get
            {
                return _transform.ScaleX;

            }
            set
            {
                _transform.ScaleX = value;
                _transform.ScaleY = value;
                NotifyPropertyChanged();
            }
        }

        public ResourceType[] ResourceTypes
        {
            get
            {
                return _resources;
            }
        }
        public GameType GameType { get { return GameType.SupplementalBuildPhase; } }

        public string GameName
        {
            get { return _name; }
        }

        public HarborType[] TypesOfHarbors
        {
            get { return _harborTypes; }
        }

      

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;


    }
}
