using System;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void EventHandler(PlayerPickerItemCtrl sender, bool val);

    public sealed partial class PlayerPickerItemCtrl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty PlayerProperty = DependencyProperty.Register("Player", typeof(PlayerModel), typeof(PlayerPickerItemCtrl), new PropertyMetadata(new PlayerModel()));

        #endregion Fields

        #region Constructors

        public PlayerPickerItemCtrl()
        {
            this.InitializeComponent();
        }

        public PlayerPickerItemCtrl(PlayerModel data) : base()
        {
            Player = data;
        }

        #endregion Constructors

        #region Properties

        public PlayerModel Player
        {
            get => (PlayerModel)GetValue(PlayerProperty);
            set => SetValue(PlayerProperty, value);
        }

        #endregion Properties

        #region Methods

        public override string ToString()
        {
            return String.Format($"{Player?.PlayerName}");
        }

        #endregion Methods
    }
}
