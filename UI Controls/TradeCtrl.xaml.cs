using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class TradeCtrl : UserControl
    {
        #region Delegates + Fields + Events + Enums

        public static readonly DependencyProperty TheHumanProperty = DependencyProperty.Register("TheHuman", typeof(PlayerModel), typeof(TradeCtrl), new PropertyMetadata(null));

        public static readonly DependencyProperty PlayingPlayersProperty = DependencyProperty.Register("PlayingPlayers", typeof(ObservableCollection<PlayerModel>), typeof(TradeCtrl), new PropertyMetadata(null));
        

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public PlayerModel TheHuman
        {
            get => (PlayerModel)GetValue(TheHumanProperty);
            set => SetValue(TheHumanProperty, value);
        }

        public ObservableCollection<PlayerModel> PlayingPlayers
        {
            get => (ObservableCollection<PlayerModel>)GetValue(PlayingPlayersProperty);
            set => SetValue(PlayingPlayersProperty, value);
        }

       

        #endregion Properties

        #region Constructors + Destructors

        public TradeCtrl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors + Destructors

        #region Methods

        public ObservableCollection<PlayerModel> ExceptMe(ObservableCollection<PlayerModel> list)
        {
            var ret = new ObservableCollection<PlayerModel>();
            ret.AddRange(list);
            ret.Remove(TheHuman);
            return ret;
        }

       

       

        private void OnClickAll(object sender, RoutedEventArgs e)
        {
        }


        #endregion Methods

        private void OnClickPlayer(object sender, RoutedEventArgs e)
        {
            var player = ((PlayerModel)((FrameworkElement)sender).Tag);
            TheHuman.GameData.Trades.TradeRequest.TradePartner = player;
        }
    }
}