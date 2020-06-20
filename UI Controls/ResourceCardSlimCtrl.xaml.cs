using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void ResourceCountChangedHandler(ResourceType resourceType, int newValue);

    public sealed partial class ResourceCardSlimCtrl : UserControl
    {

        public event ResourceCountChangedHandler OnResourceChanged;

        #region Properties + Fields

        public static readonly DependencyProperty CountProperty = DependencyProperty.Register("Count", typeof(int), typeof(ResourceCardSlimCtrl), new PropertyMetadata(0));

        public static readonly DependencyProperty CountVisibleProperty = DependencyProperty.Register("CountVisible", typeof(bool), typeof(ResourceCardSlimCtrl), new PropertyMetadata(true));

        public static readonly DependencyProperty ImageProperty = DependencyProperty.Register("Image", typeof(Brush), typeof(ResourceCardSlimCtrl), new PropertyMetadata(null));

        public static readonly DependencyProperty ResourceTypeProperty = DependencyProperty.Register("ResourceType", typeof(ResourceType), typeof(ResourceCardSlimCtrl), new PropertyMetadata(ResourceType.None, ResourceTypeChanged));
        public static readonly DependencyProperty ShowButtonsProperty = DependencyProperty.Register("ShowButtons", typeof(bool), typeof(ResourceCardSlimCtrl), new PropertyMetadata(false));
        public bool ShowButtons
        {
            get => (bool)GetValue(ShowButtonsProperty);
            set => SetValue(ShowButtonsProperty, value);
        }

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

        public ResourceType ResourceType
        {
            get => (ResourceType)GetValue(ResourceTypeProperty);
            set => SetValue(ResourceTypeProperty, value);
        }

        private static void ResourceTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ResourceCardSlimCtrl;
            var depPropValue = (ResourceType)e.NewValue;
            depPropClass?.SetResourceType(depPropValue);
        }

        private void SetResourceType(ResourceType resourceType)
        {
            if (ResourceType != ResourceType.None)
            {
                string key = "ResourceType." + resourceType.ToString();
                Image = (Brush)App.Current.Resources[key];
            }
        }

        #endregion Properties + Fields



        #region Constructors

        public ResourceCardSlimCtrl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        private void OnIncrement(object sender, RoutedEventArgs e)
        {
            Count++;
            OnResourceChanged?.Invoke(this.ResourceType, this.Count);
        }

        private void OnDecrement(object sender, RoutedEventArgs e)
        {
            if (Count > 0)
            {
                Count--;
                OnResourceChanged?.Invoke(this.ResourceType, this.Count);
            }
        }
    }
}