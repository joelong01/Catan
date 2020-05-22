using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class CityCtrl : UserControl
    {
        public CityCtrl()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(CityCtrl), new PropertyMetadata(new PlayerModel()));
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(CityCtrl), new PropertyMetadata(null));
        public PlayerModel Owner
        {
            get => (PlayerModel)GetValue(OwnerProperty);
            set => SetValue(OwnerProperty, value);
        }
        public PlayerModel CurrentPlayer
        {
            get => (PlayerModel)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public Brush GetForegroundBrush(PlayerModel current, PlayerModel owner)
        {
            return PlayerBindingFunctions.GetForegroundBrush(current, owner);
        }
        public LinearGradientBrush GetBackgroundBrush(PlayerModel current, PlayerModel owner)
        {
            return PlayerBindingFunctions.GetBackgroundBrush(current, owner);
        }
    }
}
