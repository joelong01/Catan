using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    [ContentProperty(Name = "Child")]
    public sealed partial class PurchaseItemCtrl : UserControl
    {
        public PurchaseItemCtrl()
        {
            this.InitializeComponent();
            this.DataContext = this;
        }
        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(nameof(Child), typeof(UIElement), typeof(DragableGridCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty EntitlementProperty = DependencyProperty.Register("Entitlement", typeof(Entitlement), typeof(PurchaseItemCtrl), new PropertyMetadata(Entitlement.Undefined));
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(PurchaseCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer));
        public static readonly DependencyProperty DecorationProperty = DependencyProperty.Register("Decoration", typeof(string), typeof(PurchaseItemCtrl), new PropertyMetadata(""));
        public static readonly DependencyProperty ShowCountProperty = DependencyProperty.Register("ShowCount", typeof(bool), typeof(PurchaseItemCtrl), new PropertyMetadata(true));
        public bool ShowCount
        {
            get => ( bool )GetValue(ShowCountProperty);
            set => SetValue(ShowCountProperty, value);
        }

        public string Decoration
        {
            get => ( string )GetValue(DecorationProperty);
            set => SetValue(DecorationProperty, value);
        }


        public UIElement Child
        {
            get { return ( UIElement )GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }

        public Entitlement Entitlement
        {
            get => ( Entitlement )GetValue(EntitlementProperty);
            set => SetValue(EntitlementProperty, value);
        }

        public event PurchaseEntitlementDelegate OnPurchaseEntitlement;
          public PlayerModel CurrentPlayer
        {
            get => ( PlayerModel )GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
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
        public string GetEntitlementDescription(Entitlement entitlement)
        {
        
            
                var field = entitlement.GetType().GetField(entitlement.ToString());
                var attribute = field.GetCustomAttribute<DescriptionAttribute>();
                return attribute == null ? entitlement.ToString() : attribute.Description;
            

        }
        private void OnBuyEntitlement(object sender, RoutedEventArgs e)
        {
            
            OnPurchaseEntitlement?.Invoke(this.Entitlement);
        }
    }
}
