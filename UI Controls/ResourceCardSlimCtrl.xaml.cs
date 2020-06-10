using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class ResourceCardSlimCtrl : UserControl
    {
        #region Properties + Fields 

        public int Count
        {
            get => (int)GetValue(CountProperty);
            set => SetValue(CountProperty, value);
        }

        public bool CountVisible
        {
            get => (bool)GetValue(CountVisibleProperty);
            set => SetValue(CountVisibleProperty, value);
        }

        public Brush Image
        {
            get => (Brush)GetValue(ImageProperty);
            set => SetValue(ImageProperty, value);
        }

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof(int), typeof(ResourceCardSlimCtrl), new PropertyMetadata(0));

        public static readonly DependencyProperty CountVisibleProperty = DependencyProperty.Register("CountVisible", typeof(bool), typeof(ResourceCardSlimCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(Brush), typeof(ResourceCardSlimCtrl), new PropertyMetadata(null));

        #endregion Properties + Fields 

        #region Constructors

        public ResourceCardSlimCtrl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors
    }
}
