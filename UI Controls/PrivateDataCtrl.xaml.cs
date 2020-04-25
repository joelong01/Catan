using Catan.Proxy;

using System.Collections.ObjectModel;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class PrivateDataCtrl : UserControl
    {
        #region properties
        private ObservableCollection<DevCardType> PlayedDevCards { get; set; } = new ObservableCollection<DevCardType>();
        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PrivateDataCtrl), new PropertyMetadata(new PlayerModel(), PlayerChanged));
        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }
        private static void PlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as PrivateDataCtrl;
            var depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetPlayer(depPropValue);
        }
        private void SetPlayer(PlayerModel value)
        {
            PlayedDevCards.Clear();
            PlayedDevCards.AddRange(value.GameData.PlayerResources.PlayedDevCards);
            this.PlayedDevCards.Add(DevCardType.Knight);
            this.PlayedDevCards.Add(DevCardType.YearOfPlenty);
            this.PlayedDevCards.Add(DevCardType.Knight);
            this.PlayedDevCards.Add(DevCardType.Monopoly);
        }

        #endregion
        public PrivateDataCtrl()
        {
            this.InitializeComponent();
            this.PlayedDevCards.Add(DevCardType.Knight);
            this.PlayedDevCards.Add(DevCardType.YearOfPlenty);
            this.PlayedDevCards.Add(DevCardType.Knight);
            this.PlayedDevCards.Add(DevCardType.Monopoly);
        }

        private async void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            UIElement uiElement = sender as UIElement;
            int zIndex = Canvas.GetZIndex(uiElement);
            if (e.GetCurrentPoint(uiElement).Position.Y < 30)
            {
                Canvas.SetZIndex(uiElement, zIndex + 1000);

                if (sender.GetType() == typeof(Grid))
                {
                    Grid grid = sender as Grid;
                    await StaticHelpers.DragAsync(grid, e);
                }

                Canvas.SetZIndex(uiElement, zIndex);
            }
        }
    }
}
