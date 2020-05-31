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
        #region Fields

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(CityCtrl), new PropertyMetadata(new PlayerModel()));

        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(CityCtrl), new PropertyMetadata(null));

        #endregion Fields

        #region Constructors

        public CityCtrl()
        {
            this.DataContext = this;
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

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

        #endregion Properties

        #region Methods

        public LinearGradientBrush GetBackgroundBrush(PlayerModel current, PlayerModel owner)
        {
            return PlayerBindingFunctions.GetBackgroundBrush(current, owner);
        }

        public Brush GetForegroundBrush(PlayerModel current, PlayerModel owner)
        {
            return PlayerBindingFunctions.GetForegroundBrush(current, owner);
        }

        #endregion Methods
    }
}
