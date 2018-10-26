using System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void EventHandler(PlayerPickerItemCtrl sender, bool val);
    public sealed partial class PlayerPickerItemCtrl : UserControl
    {

        public event EventHandler FirstChanged;
        public PlayerPosition Position { get; set; } = PlayerPosition.None;
        public override string ToString()
        {
            return String.Format($"{PlayerName}:{IsFirst}");
        }
        public PlayerData PlayerData { get; set; } = null;
        public static readonly DependencyProperty IsFirstProperty = DependencyProperty.Register("To", typeof(bool?), typeof(PlayerPickerItemCtrl), new PropertyMetadata(false, IsFirstChanged));
        public static readonly DependencyProperty ForegroundColorProperty = DependencyProperty.Register("Foreground", typeof(Color), typeof(PlayerPickerItemCtrl), new PropertyMetadata(Colors.HotPink, null));
        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register("Background", typeof(Color), typeof(PlayerPickerItemCtrl), new PropertyMetadata(Colors.HotPink, null));
        public static readonly DependencyProperty PlayerNameProperty = DependencyProperty.Register("PlayerName", typeof(string), typeof(PlayerPickerItemCtrl), new PropertyMetadata("Nameless"));
        public static readonly DependencyProperty ImageFileNameProperty = DependencyProperty.Register("ImageFileName", typeof(string), typeof(PlayerPickerItemCtrl), new PropertyMetadata("ms-appx:///Assets/guest.jpg"));
        public static readonly DependencyProperty ImageBrushProperty = DependencyProperty.Register("ImageBrush", typeof(ImageBrush), typeof(PlayerPickerItemCtrl), new PropertyMetadata(null));
        public ImageBrush ImageBrush
        {
            get => (ImageBrush)GetValue(ImageBrushProperty);
            set => SetValue(ImageBrushProperty, value);
        }


        public string ImageFileName
        {
            get => (string)GetValue(ImageFileNameProperty);
            set => SetValue(ImageFileNameProperty, value);
        }

        public string PlayerName
        {
            get => (string)GetValue(PlayerNameProperty);
            set => SetValue(PlayerNameProperty, value);
        }

        public Color FillColor
        {
            get => (Color)GetValue(FillColorProperty);
            set => SetValue(FillColorProperty, value);
        }


        public Color ForegroundColor
        {
            get => (Color)GetValue(ForegroundColorProperty);
            set => SetValue(ForegroundColorProperty, value);
        }

        public bool? IsFirst
        {
            get => (bool)GetValue(IsFirstProperty);
            set => SetValue(IsFirstProperty, value);
        }
        private static void IsFirstChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerPickerItemCtrl depPropClass = d as PlayerPickerItemCtrl;
            bool depPropValue = (bool)e.NewValue;
            depPropClass.SetIsFirst(depPropValue);
        }
        private void SetIsFirst(bool? value)
        {
            FirstChanged?.Invoke(this, value == true);
        }


        public PlayerPickerItemCtrl()
        {
            this.InitializeComponent();
        }
        public PlayerPickerItemCtrl(PlayerData data)
        {
            this.InitializeComponent();
            ForegroundColor = data.GameData.Foreground.Color;
            FillColor = data.GameData.PlayerColor;
            PlayerName = data.PlayerName;
            ImageFileName = data.ImageFileName;
            PlayerData = data;
            ImageBrush = data.ImageBrush;
        }

        private void OnFirstChecked(object sender, RoutedEventArgs e)
        {
            CheckBox chkBox = sender as CheckBox;
            IsFirst = chkBox.IsChecked == true;
        }
    }
}
