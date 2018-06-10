using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class GameSummaryCtrl : UserControl
    {

        public GameSummaryCtrl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty PlayerDataProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerData>), typeof(GameSummaryCtrl), new PropertyMetadata(null, PlayerDataChanged));
        public ObservableCollection<PlayerData> PlayingPlayers
        {
            get { return (ObservableCollection<PlayerData>)GetValue(PlayerDataProperty); }
            set { SetValue(PlayerDataProperty, value); }
        }
        private static void PlayerDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameSummaryCtrl depPropClass = d as GameSummaryCtrl;
            ObservableCollection<PlayerData> depPropValue = (ObservableCollection<PlayerData>)e.NewValue;
            depPropClass.SetPlayerData(depPropValue);
        }
        private void SetPlayerData(ObservableCollection<PlayerData> value)
        {
            _ListView.ItemsSource = value;
        }

        public void StartGame()
        {
            this.Height = (PlayingPlayers.Count + 1) * 45 + LayoutRoot.RowDefinitions[0].ActualHeight;
            _gameTimer.StartTimer();
        }

        public void ClearPlayers()
        {
            PlayingPlayers.Clear();
        }

        private async void OnItemRightTapped(object sender, RightTappedRoutedEventArgs e)
        {
            PlayerData player = ((Grid)sender).Tag as PlayerData;

            if (await StaticHelpers.AskUserYesNoQuestion($"Let {player.PlayerName} go first?", "Yes", "No"))
            {
                await MainPage.Current.SetFirst(player);
            }
        }

        private void OnGrowShrinkStopWatch(object sender, RoutedEventArgs e)
        {
            if (_gameTimer.Visibility == Visibility.Collapsed)
            {              
                _gameTimer.Visibility = Visibility.Visible;
            }
            else
            {             
                _gameTimer.Visibility = Visibility.Collapsed;
            }
        }

        internal void Reset()
        {
            _gameTimer.TotalTime = TimeSpan.FromMilliseconds(0);
            _gameTimer.StartTimer();
        }
    }
}
