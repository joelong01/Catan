﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
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

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class PlayerPickerDlg : ContentDialog
    {
        private ObservableCollection<PlayerModel> Players = new ObservableCollection<PlayerModel>();
        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PlayerPickerDlg), new PropertyMetadata(null));
        
        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public PlayerPickerDlg()
        {
            this.InitializeComponent();
        }
        public PlayerPickerDlg(IEnumerable<PlayerModel> players)
        {
            this.InitializeComponent();
            Players.AddRange(players);
        }

      

        private void OnOk(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

        }


        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            this.TraceMessage($"{((PlayerModel)e.AddedItems[0])?.PlayerName}");
        }
    }
}
