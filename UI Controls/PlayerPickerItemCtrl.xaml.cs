using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void EventHandler(PlayerPickerItemCtrl sender, bool val);

    public sealed partial class PlayerPickerItemCtrl : UserControl
    {
        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PlayerPickerItemCtrl), new PropertyMetadata(new PlayerModel()));

        public PlayerPickerItemCtrl()
        {
            this.InitializeComponent();
        }

        public PlayerPickerItemCtrl(PlayerModel data) : base()
        {
            Player = data;
        }

        public override string ToString()
        {
            return String.Format($"{Player?.PlayerName}");
        }
    }
}
