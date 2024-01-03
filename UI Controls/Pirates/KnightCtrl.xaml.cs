using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public enum KnightRank { Unset = -1, Basic=1, Strong=2, Mighty=3 };

    public sealed partial class KnightCtrl : UserControl
    {

        public KnightCtrl()
        {
            this.InitializeComponent();
        }

        public PlayerModel CurrentPlayer
        {
            get => ( PlayerModel )GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public PlayerModel Owner
        {
            get => ( PlayerModel )GetValue(OwnerProperty);
            set => SetValue(OwnerProperty, value);
        }

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(CityCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer));
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(CityCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer));
        public static readonly DependencyProperty KnightRankProperty = DependencyProperty.Register("KnightRank", typeof(KnightRank), typeof(KnightCtrl), new PropertyMetadata(KnightRank.Basic));
        public static readonly DependencyProperty ActivatedProperty = DependencyProperty.Register("Activated", typeof(bool), typeof(KnightCtrl), new PropertyMetadata(false));
        public static readonly DependencyProperty BuildingIndexProperty = DependencyProperty.Register("BuildingIndex", typeof(int), typeof(KnightCtrl), new PropertyMetadata(false));
        public int BuildingIndex
        {
            get => ( int )GetValue(BuildingIndexProperty);
            set => SetValue(BuildingIndexProperty, value);
        }

        public bool Activated
        {
            get => ( bool )GetValue(ActivatedProperty);
            set => SetValue(ActivatedProperty, value);
        }

        public KnightRank KnightRank
        {
            get => ( KnightRank )GetValue(KnightRankProperty);
            set => SetValue(KnightRankProperty, value);
        }

        public double ActivatedOpacity(bool activated)
        {
            if (activated) return 1.0;

            return 0.5;
        }


        public LinearGradientBrush GetBackgroundBrush(PlayerModel current, PlayerModel owner, bool activated)
        {

            return PlayerBindingFunctions.GetBackgroundBrush(current, owner);

        }

        public Brush GetForegroundBrush(PlayerModel current, PlayerModel owner, bool activated)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return new SolidColorBrush(Colors.White);
            }
            return PlayerBindingFunctions.GetForegroundBrush(current, owner);
        }

        public Visibility RankVisibility(KnightRank currentRank, KnightRank rankToShow)
        {
            int current =(int)currentRank;
            int show = (int) rankToShow;

            if (show <= current) return Visibility.Visible;
            return Visibility.Collapsed;

        }


    }
}
