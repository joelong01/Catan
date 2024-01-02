using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class SettlementCtrl : UserControl
    {
        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public PlayerModel Owner
        {
            get => (PlayerModel)GetValue(OwnerProperty);
            set => SetValue(OwnerProperty, value);
        }

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(SettlementCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer));

        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(SettlementCtrl), new PropertyMetadata(null));

        public SettlementCtrl()
        {
            this.InitializeComponent();
        }

        public LinearGradientBrush GetBackgroundBrush(PlayerModel current, PlayerModel owner)
        {
            LinearGradientBrush brush = PlayerBindingFunctions.GetBackgroundBrush(current, owner);
            return brush;
        }

        public Brush GetForegroundBrush(PlayerModel current, PlayerModel owner)
        {
            return PlayerBindingFunctions.GetForegroundBrush(current, owner);
        }
    }
}
