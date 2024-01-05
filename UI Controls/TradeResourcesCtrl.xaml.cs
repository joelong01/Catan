using Microsoft.Toolkit.Uwp.UI.Controls;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

    public delegate void TradeResourcesChangedHandler(TradeResources tradeResources, ResourceType resourceType, int count);

    public sealed partial class TradeResourcesCtrl : UserControl
    {
        #region Delegates + Fields + Events + Enums

        public event TradeResourcesChangedHandler OnResourceChanged;

        public static readonly DependencyProperty ShowButtonsProperty = DependencyProperty.Register("ShowButtons", typeof(bool), typeof(TradeResourcesCtrl), new PropertyMetadata(true));
        public static readonly DependencyProperty TradeResourcesProperty = DependencyProperty.Register("TradeResources", typeof(TradeResources), typeof(TradeResourcesCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(Orientation), typeof(TradeResourcesCtrl), new PropertyMetadata(Orientation.Horizontal));
        public static readonly DependencyProperty VerticalSpacingProperty = DependencyProperty.Register("VerticalSpacing", typeof(int), typeof(TradeResourcesCtrl), new PropertyMetadata(2));
        public static readonly DependencyProperty HorizontalSpacingProperty = DependencyProperty.Register("HorizontalSpacing", typeof(int), typeof(TradeResourcesCtrl), new PropertyMetadata(5));
        public static readonly DependencyProperty AvailableResourcesProperty = DependencyProperty.Register("AvailableResources", typeof(TradeResources), typeof(TradeResourcesCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty ShowAvailableResourcesProperty = DependencyProperty.Register("ShowAvailableResources", typeof(bool), typeof(TradeResourcesCtrl), new PropertyMetadata(false));
        public static readonly DependencyProperty CardWidthProperty = DependencyProperty.Register("CardWidth", typeof(double), typeof(TradeResourcesCtrl), new PropertyMetadata(35.0));
        public double CardWidth
        {
            get => (double)GetValue(CardWidthProperty);
            set => SetValue(CardWidthProperty, value);
        }
        public bool ShowAvailableResources
        {
            get => (bool)GetValue(ShowAvailableResourcesProperty);
            set => SetValue(ShowAvailableResourcesProperty, value);
        }
        public TradeResources AvailableResources
        {
            get => (TradeResources)GetValue(AvailableResourcesProperty);
            set => SetValue(AvailableResourcesProperty, value);
        }
        public int HorizontalSpacing
        {
            get => (int)GetValue(HorizontalSpacingProperty);
            set => SetValue(HorizontalSpacingProperty, value);
        }
        public int VerticalSpacing
        {
            get => (int)GetValue(VerticalSpacingProperty);
            set => SetValue(VerticalSpacingProperty, value);
        }
        public Orientation Orientation
        {
            get => (Orientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public TradeResources TradeResources
        {
            get => (TradeResources)GetValue(TradeResourcesProperty);
            set => SetValue(TradeResourcesProperty, value);
        }

        #endregion Properties

        #region Constructors + Destructors

        public TradeResourcesCtrl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors + Destructors

        #region Methods
        public bool ShowButtons
        {
            get => (bool)GetValue(ShowButtonsProperty);
            set => SetValue(ShowButtonsProperty, value);
        }
        private void ResourceChanged(ResourceType resourceType, int newValue)
        {
            OnResourceChanged?.Invoke(this.TradeResources, resourceType, newValue);
        }

        public Visibility CountVisibility(TradeResources tr, ResourceType resourceType)
        {            
            if (ShowButtons) return Visibility.Visible;
            if (tr == null) return Visibility.Collapsed;
            if (tr.GetCount(resourceType) > 0) return Visibility.Visible;
            return Visibility.Collapsed;
        }

        #endregion Methods
    }
}