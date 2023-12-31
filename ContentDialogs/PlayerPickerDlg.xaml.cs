using System.Collections.Generic;
using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class PlayerPickerDlg : ContentDialog
    {
        private readonly ObservableCollection<PlayerModel> Players = new ObservableCollection<PlayerModel>();

        private void OnOk(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void OnPlayerPicDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            this.Hide();
        }

        private void OnSelectionChanged(object sender, SelectionChangedEventArgs e)
        {
        }

        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PlayerPickerDlg), new PropertyMetadata(PlayerModel.DefaultPlayer));

        public PlayerPickerDlg()
        {
            this.InitializeComponent();
            this.DataContext = Players;
        }

        public PlayerPickerDlg(IEnumerable<PlayerModel> players)
        {
            this.InitializeComponent();
            Players.AddRange(players);
        }
    }
}
