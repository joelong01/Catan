using System;
using System.Collections.Generic;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class ResourceCountCtrl : UserControl
    {

        public ResourceCountCtrl()
        {
            this.InitializeComponent();

            foreach (ResourceCardCtrl card in _stackpanel.Children)
            {
                _dictCards[card.ResourceType] = card;

            }


            BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/guest.jpg", UriKind.RelativeOrAbsolute));


            PlayerImageSource = bitmapImage;

        }


        Dictionary<ResourceType, ResourceCardCtrl> _dictCards = new Dictionary<ResourceType, ResourceCardCtrl>();

        public static readonly DependencyProperty PlayerNameProperty = DependencyProperty.Register("PlayerName", typeof(string), typeof(ResourceCountCtrl), new PropertyMetadata("Nobody"));
        public static readonly DependencyProperty RoadsLeftProperty = DependencyProperty.Register("RoadsLeft", typeof(int), typeof(ResourceCountCtrl), new PropertyMetadata(15));
        public static readonly DependencyProperty SettlementsLeftProperty = DependencyProperty.Register("SettlementsLeft", typeof(int), typeof(ResourceCountCtrl), new PropertyMetadata(5));
        public static readonly DependencyProperty CitiesLeftProperty = DependencyProperty.Register("CitiesLeft", typeof(int), typeof(ResourceCountCtrl), new PropertyMetadata(4));
        public static readonly DependencyProperty ShipsLeftProperty = DependencyProperty.Register("ShipsLeft", typeof(int), typeof(ResourceCountCtrl), new PropertyMetadata(0));
        public static readonly DependencyProperty PlayerImageSourceProperty = DependencyProperty.Register("PlayerImageSource", typeof(ImageSource), typeof(ResourceCountCtrl), new PropertyMetadata(new BitmapImage(new Uri("ms-appx:///Assets/guest.jpg", UriKind.RelativeOrAbsolute))));
        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register("FillColor", typeof(Color), typeof(ResourceCountCtrl), new PropertyMetadata(Colors.White));
        public static readonly DependencyProperty PlayerColorProperty = DependencyProperty.Register("PlayerColor", typeof(Color), typeof(ResourceCountCtrl), new PropertyMetadata(Colors.White, PlayerColorChanged));

        //
        //  the background of the rectangle that the stats are stored in
        public Color PlayerColor
        {
            get { return (Color)GetValue(PlayerColorProperty); }
            set { SetValue(PlayerColorProperty, value); }
        }
        private static void PlayerColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            ResourceCountCtrl depPropClass = d as ResourceCountCtrl;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetPlayerColor(depPropValue);
        }
        private void SetPlayerColor(Color color)
        {
        //    FillColor = StaticHelpers.BackgroundToForegroundColorDictionary[color];
        }
          public Color FillColor
        {
            get { return (Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }



        public ImageSource PlayerImageSource
        {
            get { return (ImageSource)GetValue(PlayerImageSourceProperty); }
            set { SetValue(PlayerImageSourceProperty, value); }
        }


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
            return true;
        }

        internal void ResetTotalCards()
        {

        }
    }
}
