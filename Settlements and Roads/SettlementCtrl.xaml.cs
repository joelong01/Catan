using Windows.UI;
using Windows.UI.Core;
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
            get => ( PlayerModel )GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public PlayerModel Owner
        {
            get => ( PlayerModel )GetValue(OwnerProperty);
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
            try
            {
                LinearGradientBrush brush = PlayerBindingFunctions.GetBackgroundBrush(current, owner);
                return brush;
            }
            catch
            {
                return ConverterGlobals.CreateLinearGradiantBrush(Colors.Blue, Colors.Black);
            }
        }

        public Brush GetForegroundBrush(PlayerModel current, PlayerModel owner)
        {
            try
            {
                if (owner != null)
                {
                    return owner.ForegroundBrush;
                }

                if (current != null)
                {
                    return current.ForegroundBrush;
                }

                return ConverterGlobals.GetBrush(Colors.White);
            }
            catch
            {
                return new SolidColorBrush(Colors.White);
            }
        }
    }
}
