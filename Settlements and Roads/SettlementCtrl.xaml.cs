using Windows.UI;
using Windows.UI.Composition.Interactions;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

    


    public sealed partial class SettlementCtrl : UserControl
    {
        public SettlementCtrl()
        {
            
            this.InitializeComponent();
            this.DataContext = this;

        }

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(SettlementCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(SettlementCtrl), new PropertyMetadata(null));
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

        public Brush GetForegroundBush(PlayerModel current, PlayerModel owner)
        {
            return PlayerBindingFunctions.GetForegroundBush(current, owner);
        }
        public LinearGradientBrush GetBackgroundBush(PlayerModel current, PlayerModel owner)
        {
            LinearGradientBrush brush =  PlayerBindingFunctions.GetBackgroundBush(current, owner);
            if (brush == null)
            {
                this.TraceMessage("this one");
            }
            return brush;
        }



    }
}
