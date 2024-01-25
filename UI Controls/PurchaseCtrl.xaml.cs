using System.Collections.ObjectModel;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Catan10
{
    public delegate void PurchaseEntitlementDelegate(Entitlement entitlement);
    public sealed partial class PurchaseCtrl : UserControl
    {

      

        public PurchaseCtrl()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }

        public event PurchaseEntitlementDelegate OnPurchaseEntitlement;
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(PurchaseCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer));
        public PlayerModel CurrentPlayer
        {
            get => ( PlayerModel )GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(PurchaseCtrl), new PropertyMetadata(MainPageModel.Default));
        public MainPageModel MainPageModel
        {
            get => ( MainPageModel )GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }

        public string UnplayedResourceCount(ObservableCollection<Entitlement> entitlements, Entitlement toCheck)
        {
            int count = 0;

            foreach (Entitlement entitlement in entitlements)
            {
                if (entitlement.Equals(toCheck))
                {
                    count++;
                }
            }

            return count.ToString();
        }

        private void OnBuyEntitlement(Entitlement entitlement)
        {
            OnPurchaseEntitlement?.Invoke(entitlement);
        }

        public SolidColorBrush GetForegroundBrush(PlayerModel current)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return new SolidColorBrush(Colors.White);
            }
            return PlayerBindingFunctions.GetForegroundBrush(current, current);
        }

        public double GetHeight(bool pirates)
        {
            if (!pirates)
            {
                return 135;
            }
            else
            {
                return 395;
            }
        }
    }
}
