using System;
using System.Collections.Generic;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;



// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

    public sealed partial class TraditionalRollCtrl : UserControl
    {
        private List<ToggleButton> RedDice = new List<ToggleButton>();
        private List<ToggleButton> WhiteDice = new List<ToggleButton>();
        private List<ToggleButton> Specials = new List<ToggleButton>();
        public TraditionalRollCtrl()
        {
            this.InitializeComponent();
            RedDice.Add(RedOne);
            RedDice.Add(RedTwo);
            RedDice.Add(RedThree);
            RedDice.Add(RedFour);
            RedDice.Add(RedFive);
            RedDice.Add(RedSix);
            WhiteDice.Add(WhiteOne);
            WhiteDice.Add(WhiteTwo);
            WhiteDice.Add(WhiteThree);
            WhiteDice.Add(WhiteFour);
            WhiteDice.Add(WhiteFive);
            WhiteDice.Add(WhiteSix);
            Specials.Add(SpecialBlue);
            Specials.Add(SpecialGreen);
            Specials.Add(SpecialYellow);
            Specials.Add(SpecialPirate);


        }

        public event RollSelectedHandler OnRoll;


        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(TraditionalRollCtrl), new PropertyMetadata(MainPageModel.Default, GameChanged));

        private static void GameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            Console.WriteLine("New Game in TraditionalRollControl");
        }

        public MainPageModel MainPageModel
        {
            get => ( MainPageModel )GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }



        private void RollSelected(RollModel rollModel)
        {
            OnRoll?.Invoke(rollModel);
        }

        public Visibility ShowLastRoll(GameState state)
        {
            if (state == GameState.WaitingForNext) return Visibility.Visible;

            return Visibility.Collapsed;
        }

        private void OnOk(object sender, RoutedEventArgs e)
        {
            if (MainPageModel.EnableRolls == false) return;
            int redValue = -1;
            int whiteValue = -1;
            SpecialDice special = SpecialDice.None;
            foreach (ToggleButton d in RedDice)
            {
                if (d.IsChecked == true)
                {
                    redValue = ( d.Content as DiceCtrl ).Number;
                }
            }
            if (redValue == -1) return;

            foreach (ToggleButton d in WhiteDice)
            {
                if (d.IsChecked == true)
                {
                    whiteValue = ( d.Content as DiceCtrl ).Number;
                }
            }
            if (whiteValue == -1) return;

            foreach (ToggleButton d in Specials)
            {
                if (d.IsChecked == true)
                {
                    special = ( SpecialDice )Enum.Parse(typeof(SpecialDice), d.Tag as string);
                }
            }
            if (special == SpecialDice.None) return;

            RollModel rollModel = new RollModel()
            {
                RedDie = redValue,
                WhiteDie = whiteValue,
                SpecialDice = special,
                Roll = redValue + whiteValue,

            };
            OnRoll?.Invoke(rollModel);
        }

        private void OnRedClick(object sender, RoutedEventArgs e)
        {
            foreach (var tb in RedDice)
            {
                if (tb != ( ToggleButton )sender)
                {
                    tb.IsChecked = false;
                }
            }
        }

        private void OnSpecialClick(object sender, RoutedEventArgs e)
        {
            foreach (var tb in Specials)
            {
                if (tb != ( ToggleButton )sender)
                {
                    tb.IsChecked = false;
                }
            }
        }

        private void OnWhiteClick(object sender, RoutedEventArgs e)
        {
            foreach (var tb in WhiteDice)
            {
                if (tb != ( ToggleButton )sender)
                {
                    tb.IsChecked = false;
                }
            }
        }
    }
}
