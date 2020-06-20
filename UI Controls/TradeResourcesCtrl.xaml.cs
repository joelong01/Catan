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

        public static readonly DependencyProperty TradeResourcesProperty = DependencyProperty.Register("TradeResources", typeof(TradeResources), typeof(TradeResourcesCtrl), new PropertyMetadata(null));

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

        private void ResourceChanged(ResourceType resourceType, int newValue)
        {
            OnResourceChanged?.Invoke(this.TradeResources, resourceType, newValue);
        }

        #endregion Methods
    }
}