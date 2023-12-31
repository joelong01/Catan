using System.Collections.Generic;
using System.Collections.ObjectModel;

using Catan.Proxy;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at https://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class JoinGameDlg : ContentDialog
    {

        #region Delegates + Fields + Events + Enums

        public static readonly DependencyProperty GameSelectedProperty = DependencyProperty.Register("GameSelected", typeof(GameInfo), typeof(JoinGameDlg), new PropertyMetadata(null));

        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(JoinGameDlg), new PropertyMetadata(null));

        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(JoinGameDlg), new PropertyMetadata(PlayerModel.DefaultPlayer));

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public GameInfo GameSelected
        {
            get => (GameInfo)GetValue(GameSelectedProperty);
            set => SetValue(GameSelectedProperty, value);
        }

        public MainPageModel MainPageModel
        {
            get => (MainPageModel)GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }

        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        private ObservableCollection<GameInfo> GameNames { get; } = new ObservableCollection<GameInfo>();

        #endregion Properties

        #region Constructors + Destructors

        public JoinGameDlg(List<GameInfo> games)
        {
            this.InitializeComponent();
            GameNames.AddRange(games);
            if (games.Count > 0)
            {
                GameSelected = games[0];
            }
        }

        #endregion Constructors + Destructors

        #region Methods

        private void ContentDialog_PrimaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }

        private void ContentDialog_SecondaryButtonClick(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
            this.Hide();
        }

        #endregion Methods
    }
}