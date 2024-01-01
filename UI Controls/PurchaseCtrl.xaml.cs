﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;



namespace Catan10
{
    public delegate void PurchaseEntitlementDelegate(Entitlement entitlement);
    public sealed partial class PurchaseCtrl : UserControl
    {

        public event PurchaseEntitlementDelegate OnPurchaseEntitlement;

        public PurchaseCtrl()
        {
            this.InitializeComponent();
        }


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

        private void OnBuyEntitlement(object sender, RoutedEventArgs e)
        {
            Entitlement entitlement = StaticHelpers.ParseEnum<Entitlement>(((Button)sender).Tag as String);
            OnPurchaseEntitlement?.Invoke(entitlement);
        }
    }
}
