using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class ResourceCountCtrl : UserControl  
    {
       
        Dictionary<ResourceType, ResourceCardCtrl> _dictCards = new Dictionary<ResourceType, ResourceCardCtrl>();
        Dictionary<ResourceType, ResourceCardCtrl> _dictTotalCards = new Dictionary<ResourceType, ResourceCardCtrl>();
        public static readonly DependencyProperty PlayerNameProperty = DependencyProperty.Register("PlayerName", typeof(string), typeof(ResourceCountCtrl), new PropertyMetadata("Nobody"));
        public static readonly DependencyProperty ColorProperty = DependencyProperty.Register("Color", typeof(Color), typeof(ResourceCountCtrl), new PropertyMetadata(Colors.Green));
        public static readonly DependencyProperty RoadsLeftProperty = DependencyProperty.Register("RoadsLeft", typeof(int), typeof(ResourceCountCtrl), new PropertyMetadata(15));
        public static readonly DependencyProperty SettlementsLeftProperty = DependencyProperty.Register("SettlementsLeft", typeof(int), typeof(ResourceCountCtrl), new PropertyMetadata(5));
        public static readonly DependencyProperty CitiesLeftProperty = DependencyProperty.Register("CitiesLeft", typeof(int), typeof(ResourceCountCtrl), new PropertyMetadata(4));
        public static readonly DependencyProperty ShipsLeftProperty = DependencyProperty.Register("ShipsLeft", typeof(int), typeof(ResourceCountCtrl), new PropertyMetadata(0));
        public int ShipsLeft
        {
            get { return (int)GetValue(ShipsLeftProperty); }
            set { SetValue(ShipsLeftProperty, value); }
        }

        public int CitiesLeft
        {
            get { return (int)GetValue(CitiesLeftProperty); }
            set { SetValue(CitiesLeftProperty, value); }
        }

        public int SettlementsLeft
        {
            get { return (int)GetValue(SettlementsLeftProperty); }
            set { SetValue(SettlementsLeftProperty, value); }
        }

        public int RoadsLeft
        {
            get { return (int)GetValue(RoadsLeftProperty); }
            set { SetValue(RoadsLeftProperty, value); }
        }


        public Color Color
        {
            get
            {
                return (Color)GetValue(ColorProperty);
            }
            set
            {
                SetValue(ColorProperty, value);
            }
        }
        public string PlayerName
        {
            get
            {
                return (string)GetValue(PlayerNameProperty);
            }
            set
            {
                SetValue(PlayerNameProperty, value);
            }
        }

        public ResourceCountCtrl()
        {
            this.InitializeComponent();

            foreach (ResourceCardCtrl card in _stackpanel.Children)
            {
                _dictCards[card.ResourceType] = card;
                
            }



            _dictTotalCards[ResourceType.Wood] = _wood;
            _dictTotalCards[ResourceType.Wheat] = _wheat;
            _dictTotalCards[ResourceType.Sheep] = _sheep;
            _dictTotalCards[ResourceType.Brick] = _brick;
            _dictTotalCards[ResourceType.Ore] = _ore;
            _dictTotalCards[ResourceType.GoldMine] = _goldmine;








        }

        public void SetOrientation(TileOrientation orientation)
        {
            foreach (var kvp in _dictCards)
            {
                kvp.Value.Orientation = orientation;
                if (orientation == TileOrientation.FaceDown)
                    kvp.Value.Count = 0; // everytime you go face down, reset it.
            }
        }

        public bool AddResourceCount(ResourceType resource, int count)
        {
            if (_dictCards.TryGetValue(resource, out ResourceCardCtrl card))
            {
                card.Count += count;
                card.SetOrientationAsync(TileOrientation.FaceUp);
                return true;
            }

            return false;
        }

        public bool UpdateTotaResourceCount(ResourceType resource, int count)
        {
            if (_dictTotalCards.TryGetValue(resource, out ResourceCardCtrl card))
            {
                card.Count += count;
                card.SetOrientationAsync(TileOrientation.FaceUp);
                return true;
            }

            return false;
        }

        internal void ResetTotalCards()
        {
            foreach (var kvp in _dictTotalCards)
            {
                kvp.Value.Count = 0;
            }
        }
    }
}
