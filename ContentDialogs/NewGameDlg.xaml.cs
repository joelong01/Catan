using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class NewGameDlg : ContentDialog
    {
       
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

        public ObservableCollection<CatanGameCtrl> AvailableGames { get; } = new ObservableCollection<CatanGameCtrl>();
        public ObservableCollection<PlayerModel> AvailablePlayers { get; } = new ObservableCollection<PlayerModel>();
        public ObservableCollection<PlayerModel> PlayingPlayers
        {
            get
            {
                var players = new ObservableCollection<PlayerModel>();
                foreach (PlayerModel player in GridView_AvailablePlayers.SelectedItems)
                {
                    players.Add(player);
                }
                return players;
            }
        }
        





        public string SaveFileName
        {
            get => (string)GetValue(SaveFileNameProperty);
            set => SetValue(SaveFileNameProperty, value);
        }

        public CatanGameCtrl SelectedGame
        {
            get => (CatanGameCtrl)GetValue(SelectedGameProperty);
            set => SetValue(SelectedGameProperty, value);
        }

        public int SelectedIndex => AvailableGames.IndexOf(SelectedGame);
        public static readonly DependencyProperty SaveFileNameProperty = DependencyProperty.Register("SaveFileName", typeof(string), typeof(NewGameDlg), new PropertyMetadata(""));
        public static readonly DependencyProperty SelectedGameProperty = DependencyProperty.Register("SelectedGame", typeof(CatanGameCtrl), typeof(NewGameDlg), new PropertyMetadata(null));

        public NewGameDlg()
        {
            this.InitializeComponent();
            this.DataContext = PlayingPlayers;
        }

        public NewGameDlg(IList<PlayerModel> playerData, IList<CatanGameCtrl> games)
        {
            this.InitializeComponent();
            AvailablePlayers.AddRange(playerData);
            AvailableGames.AddRange(games);
            SelectedGame = AvailableGames[0];
            this.DataContext = this;
        }
    }
}
