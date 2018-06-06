using System.Collections.Generic;
using System.Collections.ObjectModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{


    public sealed partial class ScoreViewCtrl : UserControl
    {

    
        public ObservableCollection<PlayerData> PlayingPlayers { get; set; } = new ObservableCollection<PlayerData>();
        public ScoreViewCtrl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty ActivePlayerBackgroundProperty = DependencyProperty.Register("ActivePlayerBackground", typeof(string), typeof(MainPage), new PropertyMetadata("Blue"));
        public static readonly DependencyProperty ActivePlayerForegroundProperty = DependencyProperty.Register("ActivePlayerForeground", typeof(string), typeof(MainPage), new PropertyMetadata("Blue"));
        
      
      
        public string ActivePlayerBackground
        {
            get
            {
                return (string)GetValue(ActivePlayerBackgroundProperty);
            }
            set
            {
                SetValue(ActivePlayerBackgroundProperty, value);
                ActivePlayerForeground = StaticHelpers.BackgroundToForegroundDictionary[value];
            }
        }
        public string ActivePlayerForeground
        {
            get
            {
                return (string)GetValue(ActivePlayerForegroundProperty);
            }
            set
            {
                SetValue(ActivePlayerForegroundProperty, value);
            }
        }

        public void AddPlayers(IEnumerable<PlayerData> players)
        {
            PlayingPlayers.Clear();
            PlayingPlayers.AddRange(players);

            this.Height = PlayingPlayers.Count * 75 + 49;

        }

        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            int zIndex = Canvas.GetZIndex((UIElement)sender);
            Canvas.SetZIndex((UIElement)sender, zIndex+1000);
            await StaticHelpers.DragAsync(this, e);
            Canvas.SetZIndex((UIElement)sender, zIndex);
        }
    }
}
