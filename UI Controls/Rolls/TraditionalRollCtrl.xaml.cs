using System;
using System.Collections.Generic;
using System.Reflection;
using Windows.ApplicationModel.VoiceCommands;
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
            Specials.Add(SpecialPolitics);
            Specials.Add(SpecialTrade);
            Specials.Add(SpecialScience);
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

        private RollModel BuildRollModel()
        {
            RollModel rollModel = new RollModel()
            {
                RedDie = -1,
                WhiteDie = -1,
                SpecialDice = SpecialDice.None,
                Roll = -2,
            };

            foreach (ToggleButton d in RedDice)
            {
                if (d.IsChecked == true)
                {
                    rollModel.RedDie = ( d.Content as DiceCtrl ).Number;
                    break;
                }
            }

            foreach (ToggleButton d in WhiteDice)
            {
                if (d.IsChecked == true)
                {
                    rollModel.WhiteDie = ( d.Content as DiceCtrl ).Number;
                    break;

                }
            }
            foreach (ToggleButton d in Specials)
            {
                if (d.IsChecked == true)
                {
                    rollModel.SpecialDice = ( SpecialDice )Enum.Parse(typeof(SpecialDice), d.Tag as string);
                    break;
                }
            }

            rollModel.Roll = rollModel.RedDie + rollModel.WhiteDie;

            return rollModel;
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

            var rollModel = BuildRollModel();
            if (rollModel.IsValidRoll)
            {
                OnRoll?.Invoke(rollModel);
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
            var rollModel = BuildRollModel();
            if (rollModel.IsValidRoll)
            {
                OnRoll?.Invoke(rollModel);
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
            var rollModel = BuildRollModel();
            if (rollModel.IsValidRoll)
            {
                OnRoll?.Invoke(rollModel);
            }
        }

        public void NewTurn()
        {
            SetRolls(new RollModel());
        }

        private void SetDie(IList<ToggleButton> ctrls, int roll)
        {
            for (int i = 0; i < ctrls.Count; i++)
            {
                ctrls[i].IsChecked = ( roll == i ); // -1 won't error...
            }
        }

        public void SetRolls(RollModel roll)
        {
            SetDie(RedDice, roll.RedDie);
            SetDie(WhiteDice, roll.WhiteDie);
            SetDie(Specials, ( int )roll.SpecialDice);
        }
    }
}
