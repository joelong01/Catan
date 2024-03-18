﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
 
    public sealed partial class PublicDataCitiesAndKnightCtrl : UserControl
    {
        public event PlayerSelected OnPlayerSelected;
        private static void PlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PublicDataCitiesAndKnightCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetPlayer(depPropValue);
        }

        private static void RollOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PublicDataCitiesAndKnightCtrl;
            depPropClass?.SetRollOrientation(( TileOrientation )e.OldValue, ( TileOrientation )e.NewValue);
        }

        private void Picture_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
  
            OnPlayerSelected?.Invoke(Player);
        }

        private void SetPlayer(PlayerModel value)
        {
            if (value == null) return;
        }

        private void SetRollOrientation(TileOrientation oldValue, TileOrientation newValue)
        {

        }

        public PlayerModel Player
        {
            get => ( PlayerModel )GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public TileOrientation RollOrientation
        {
            get => ( TileOrientation )GetValue(RollOrientationProperty);
            set => SetValue(RollOrientationProperty, value);
        }

        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PublicDataCitiesAndKnightCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer, PlayerChanged));

        public static readonly DependencyProperty RollOrientationProperty = DependencyProperty.Register("RollOrientation", typeof(TileOrientation), typeof(PublicDataCitiesAndKnightCtrl), new PropertyMetadata(TileOrientation.FaceDown, RollOrientationChanged));

        public PublicDataCitiesAndKnightCtrl()
        {
            this.InitializeComponent();

        }

        /// <summary>
        ///     given a collection and the value of the type, return how many dev cards are in the collection
        /// </summary>
        /// <param name="devCards"></param>
        /// <param name="cardType"></param>
        /// <returns></returns>
        public int DevCardCount(ObservableCollection<DevCardModel> devCards, string cardType)
        {
            DevCardType card = Enum.Parse<DevCardType>(cardType);
            int count = 0;
            foreach (var model in devCards)
            {
                if (model.DevCardType == card) count++;
            }
            return count;
        }

        public string UnplayedResourceCount(ObservableCollection<Entitlement> unspent, string name)
        {
            var entitlement = (Entitlement)Enum.Parse(typeof(Entitlement), name);
            var count = 0;
            foreach (var ent in unspent)
            {
                if (ent == entitlement) count++;
            }

            return count.ToString();
        }
       
        private bool IsControlOrShiftPressed
        {
            get
            {
                var shiftState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Shift);
                var ctrlState = CoreWindow.GetForCurrentThread().GetKeyState(VirtualKey.Control);

                var isShiftDown = shiftState == CoreVirtualKeyStates.Down;
                var isCtrlDown = ctrlState == CoreVirtualKeyStates.Down;

                return ( isShiftDown || isCtrlDown );
            }
        }

    }
}