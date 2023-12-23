using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;


// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

    public sealed partial class TraditionalRollCtrl : UserControl
    {
        public TraditionalRollCtrl()
        {
            this.InitializeComponent();
        }

        public event RollSelectedHandler OnRoll;


        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(TraditionalRollCtrl), new PropertyMetadata(null, GameChanged));

        private static void GameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine("New Game in TraditionalRollControl");
        }

        public MainPageModel MainPageModel
        {
            get => ( MainPageModel )GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }



        private void RollSelected(int roll)
        {
            OnRoll?.Invoke(roll);
        }

        public Visibility ShowLastRoll(GameState state)
        {
            if (state == GameState.WaitingForNext) return Visibility.Visible;

            return Visibility.Collapsed;
        }


    }
}
