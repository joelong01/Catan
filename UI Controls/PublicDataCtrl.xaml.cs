using System;
using System.Collections.ObjectModel;

using Windows.System;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class PublicDataCtrl : UserControl
    {
        private static void PlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PublicDataCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetPlayer(depPropValue);
        }

        private static void RollOrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PublicDataCtrl;
            depPropClass?.SetRollOrientation((TileOrientation)e.OldValue, (TileOrientation)e.NewValue);
        }


        private void Picture_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //
            //  7/9/2020:  this used to open Trade in multiplayer.  leave it here in case we need a real time debug entry point
        }

        private void SetPlayer(PlayerModel value)
        {
            if (value == null) return;
        }

        private void SetRollOrientation(TileOrientation oldValue, TileOrientation newValue)
        {
            if (Player == null) return;

            GameState state = MainPage.Current.CurrentGameState;

            if (state == GameState.AllocateResourceForward)
            {
                ShowStats.Begin();
                return;
            }
            if (state != GameState.WaitingForRollForOrder && state != GameState.WaitingForRoll && state != GameState.BeginResourceAllocation)
            {
                this.TraceMessage($"rejecting call to SetOrientation for state={MainPage.Current.CurrentGameState}");
                return;
            }

            if (newValue == TileOrientation.FaceDown)
            {
                ShowStats.Begin();
            }
            else
            {
                ShowLatestRoll.Begin();
            }
        }

        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public TileOrientation RollOrientation
        {
            get => (TileOrientation)GetValue(RollOrientationProperty);
            set => SetValue(RollOrientationProperty, value);
        }

        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PublicDataCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer, PlayerChanged));

        public static readonly DependencyProperty RollOrientationProperty = DependencyProperty.Register("RollOrientation", typeof(TileOrientation), typeof(PublicDataCtrl), new PropertyMetadata(TileOrientation.FaceDown, RollOrientationChanged));

        public PublicDataCtrl()
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

                return (isShiftDown || isCtrlDown);
            }
        }

       
    }
}