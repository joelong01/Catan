﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class NewGameDlg : ContentDialog
    {
        public ObservableCollection<PlayerModel> PlayingPlayers { get; } = new ObservableCollection<PlayerModel>();
        public ObservableCollection<PlayerModel> AvailablePlayers { get; } = new ObservableCollection<PlayerModel>();
        public ObservableCollection<CatanGame> AvailableGames { get; } = new ObservableCollection<CatanGame>();


        public static readonly DependencyProperty SelectedGameProperty = DependencyProperty.Register("SelectedGame", typeof(CatanGame), typeof(NewGameDlg), new PropertyMetadata(null));
        public static readonly DependencyProperty SaveFileNameProperty = DependencyProperty.Register("SaveFileName", typeof(string), typeof(NewGameDlg), new PropertyMetadata(""));
        public string SaveFileName
        {
            get => (string)GetValue(SaveFileNameProperty);
            set => SetValue(SaveFileNameProperty, value);
        }

        public CatanGame SelectedGame
        {
            get => (CatanGame)GetValue(SelectedGameProperty);
            set => SetValue(SelectedGameProperty, value);
        }


        public int SelectedIndex => AvailableGames.IndexOf(SelectedGame);



        public NewGameDlg()
        {
            this.InitializeComponent();
        }
        public NewGameDlg(IList<PlayerModel> playerData, IList<CatanGame> games)
        {
            this.InitializeComponent();
            AvailablePlayers.AddRange(playerData);
            AvailableGames.AddRange(games);
            SelectedGame = AvailableGames[0];



        }


        private void OnOk(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            if (SaveFileName == "")
            {
                return;
            }


        }

        private void OnCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }



        private void SetThickness(object target, double thickness)
        {
            if (target.GetType() == typeof(Grid))
            {
                ((Grid)target).BorderThickness = new Thickness(thickness);
            }
            else if (target.GetType() == typeof(GridView))
            {
                ((GridView)target).BorderThickness = new Thickness(thickness);
            }
        }

        private void OnGameChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedGame == null)
            {
                return;
            }

            DateTime dt = DateTime.Now;

            string ampm = dt.TimeOfDay.TotalMinutes > 720 ? "PM" : "AM";
            string min = dt.TimeOfDay.Minutes.ToString().PadLeft(2, '0');

            SaveFileName = String.Format($"{dt.TimeOfDay.Hours % 12}.{min} {ampm} - {SelectedGame.Description}");
        }

       

        private void OnDragOver(object sender, DragEventArgs e)
        {
            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            e.DragUIOverride.IsGlyphVisible = false;
            e.DragUIOverride.IsCaptionVisible = false;

        }

        private void OnDrageEnter(object target, DragEventArgs e)
        {
            SetThickness(target, 3);

        }

        private void OnDragLeave(object sender, DragEventArgs e)
        {
            SetThickness(sender, 1);

        }

        private void GridView_DragItemsStarting(object sender, DragItemsStartingEventArgs e)
        {
            var source = PlayingPlayers;
            var gridView = sender as GridView;

            if (gridView.Name == "GridView_AvailablePlayers")
            {
                source = AvailablePlayers;

            }
            List<PlayerModel> movedPlayers = new List<PlayerModel>();
            foreach (PlayerModel p in e.Items)
            {
                movedPlayers.Add(p);
            }
            if (movedPlayers.Count == 0) return;

            e.Data.Properties.Add("movedPlayers", movedPlayers);
            e.Data.Properties.Add("source", source);

        }

        private void OnDrop(object sender, DragEventArgs e)
        {
            var target = PlayingPlayers;
            var gridView = sender as GridView;

            if (gridView.Name == "GridView_AvailablePlayers")
            {
                target = AvailablePlayers;

            }

            var source = e.Data.Properties["source"];
            if (source == target )
            {
                e.Handled = false;
                return;
            }
            IEnumerable<PlayerModel> movedPlayers = e.Data.Properties["movedPlayers"] as IEnumerable<PlayerModel>;
            ObservableCollection<PlayerModel> sourcePlayers = e.Data.Properties["source"] as ObservableCollection<PlayerModel>;
            foreach (var player in movedPlayers)
            {
                bool ret = sourcePlayers.Remove(player);
                if (!ret)
                {
                    throw new ArgumentException("A player to be moved wasn't in the source collection.");
                }
                target.Add(player);
            }
            e.Handled = true;

        }

      

       

       

    }
}
