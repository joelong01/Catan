using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using Windows.ApplicationModel;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Composition;
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
    public enum KnightRank { Unset = -1, Basic=1, Strong=2, Mighty=3 };


    public sealed partial class KnightCtrl : UserControl
    {
        private Point _lastPressedPosition;
        public event PointerEventHandler PointerClicked;

        public KnightCtrl()
        {
            this.InitializeComponent();
        }

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

        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(CityCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer));
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(CityCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer));
        public static readonly DependencyProperty BuildingIndexProperty = DependencyProperty.Register("Index", typeof(int), typeof(KnightCtrl), new PropertyMetadata(false));
        public static readonly DependencyProperty ActivatedProperty = DependencyProperty.Register("Activated", typeof(bool), typeof(KnightCtrl), new PropertyMetadata(false, ActivatedChanged));
        public static readonly DependencyProperty KnightRankProperty = DependencyProperty.Register("KnightRank", typeof(KnightRank), typeof(KnightCtrl), new PropertyMetadata(KnightRank.Basic, KnightRankChanged));
        public KnightRank KnightRank
        {
            get => ( KnightRank )GetValue(KnightRankProperty);
            set => SetValue(KnightRankProperty, value);
        }
        private static void KnightRankChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as KnightCtrl;
            var depPropValue = (KnightRank)e.NewValue;
            depPropClass?.SetKnightRank(depPropValue);
        }
        private void SetKnightRank(KnightRank value)
        {
            CurrentPlayer.GameData.UpdateTotalKnightRank();
        }

        public bool Activated
        {
            get => ( bool )GetValue(ActivatedProperty);
            set => SetValue(ActivatedProperty, value);
        }
        private static void ActivatedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as KnightCtrl;
            var depPropValue = (bool)e.NewValue;
            depPropClass?.SetActivated(depPropValue);
        }
        private void SetActivated(bool value)
        {
            //
            // because we aren't using a good separation between model and view, we don't have a way to fire change notifications to the player model when a knight is activated.  so 
            // we are going to do it in a hacky way...
            CurrentPlayer.GameData.UpdateTotalKnightRank();

        }

        public int BuildingIndex
        {
            get => ( int )GetValue(BuildingIndexProperty);
            set => SetValue(BuildingIndexProperty, value);
        }

       

      

        public double ActivatedOpacity(bool activated)
        {
            if (activated) return 1.0;

            return 0.5;
        }

        public BuildingCtrl GetParentBuilding()
        {
            DependencyObject parentObj = VisualTreeHelper.GetParent(this);

            while (parentObj != null)
            {
                if (parentObj is BuildingCtrl parentBuilding)
                {
                    return parentBuilding;
                }

                parentObj = VisualTreeHelper.GetParent(parentObj);
            }

            return null; // Return null if no BuildingCtrl parent is found
        }



        public LinearGradientBrush GetBackgroundBrush(PlayerModel current, PlayerModel owner, bool activated)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return ConverterGlobals.CreateLinearGradiantBrush(Colors.Green, Colors.Black);
            }
            return PlayerBindingFunctions.GetBackgroundBrush(current, owner);

        }

        public SolidColorBrush GetForegroundBrush(PlayerModel current, PlayerModel owner, bool activated)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return new SolidColorBrush(Colors.Green);
            }
            return PlayerBindingFunctions.GetForegroundBrush(current, owner);
        }

        public Visibility RankVisibility(KnightRank currentRank, KnightRank rankToShow)
        {
            if (currentRank == rankToShow) return Visibility.Visible;
            return Visibility.Collapsed;

        }

        private void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            _lastPressedPosition = e.GetCurrentPoint(this).Position;
        }

        private void OnPointerReleased(object sender, PointerRoutedEventArgs e)
        {
            Point releasedPosition = e.GetCurrentPoint(this).Position;

            // Check if the pointer is released at approximately the same position where it was pressed
            // You can adjust the threshold as needed
            if (Math.Abs(releasedPosition.X - _lastPressedPosition.X) < 5 &&
                Math.Abs(releasedPosition.Y - _lastPressedPosition.Y) < 5)
            {
                PointerClicked?.Invoke(sender, e);
                e.Handled = true;
            }
        }

        public Duration GetAnimationDuration(AnimationSpeed requestedSpeed, bool testing)
        {
            if (testing) return new Duration(TimeSpan.FromMilliseconds(( double )AnimationSpeed.Testing));

            return new Duration(TimeSpan.FromMilliseconds(( double )requestedSpeed));
        }
    }
}
