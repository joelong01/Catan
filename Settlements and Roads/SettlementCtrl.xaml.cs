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

        public static readonly DependencyProperty PlayerColorProperty = DependencyProperty.Register("PlayerColor", typeof(Brush), typeof(SettlementCtrl), new PropertyMetadata(new SolidColorBrush(Colors.Green)));
        public static readonly DependencyProperty CastleColorProperty = DependencyProperty.Register("CastleColor", typeof(Brush), typeof(SettlementCtrl), new PropertyMetadata(new SolidColorBrush(Colors.Yellow)));
        public Brush CastleColor
        {
            get => (Brush)GetValue(CastleColorProperty);
            set => SetValue(CastleColorProperty, value);
        }

        public Brush PlayerColor
        {
            get => (Brush)GetValue(PlayerColorProperty);
            set => SetValue(PlayerColorProperty, value);
        }
       

        

    }
}
