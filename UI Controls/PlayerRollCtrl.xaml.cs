using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void OnRolledHandler(List<RollModel> rolls);

    public sealed partial class PlayerRollCtrl : UserControl
    {
        public ObservableCollection<RollModel> Rolls { get; } = new ObservableCollection<RollModel>();
        private bool _rolled = false;
        private bool clicked = false;


        

        private void OnFaceDown(object sender, RoutedEventArgs e)
        {
            var list = new List<Task>();
            Rolls.ForEach((ctrl) => ctrl.Orientation = TileOrientation.FaceDown);
        }

        private void OnFaceUp(object sender, RoutedEventArgs e)
        {
            var list = new List<Task>();
            Rolls.ForEach((ctrl) => ctrl.Orientation = TileOrientation.FaceUp);
        }

        private async void OnReset(object sender, RoutedEventArgs e)
        {
            await Reset();
            Randomize();
        }

        private void OnShowAll(object sender, RoutedEventArgs e)
        {
            if (!_rolled) return;

            if (clicked) return;
            clicked = true;

            var list = new List<Task>();
            Rolls.ForEach((ctrl) => ctrl.Orientation = TileOrientation.FaceUp);
            OnShowAllRolls?.Invoke(new List<RollModel>(Rolls));
            clicked = false;
        }

        private void PopulateControlList()
        {
        }

        private void Roll_PointerPressed(object sender, PointerRoutedEventArgs e)
        {

            if (MainPage.Current.MainPageModel.EnableRolls == false) return;

            if (_rolled) return;
            try
            {
                _rolled = true;
                RollModel roll = ((RollCtrl)sender).Roll;
                roll.Selected = true;
                roll.Orientation = TileOrientation.FaceUp;

                OnRolled?.Invoke(new List<RollModel>(Rolls));
            }
            finally
            {
            }
        }

        public void TestSetRolls(List<RollModel> rolls)
        {
            Rolls.Clear();
            Rolls.AddRange(rolls);
            if (_rolled) return;
            try
            {
                _rolled = true;
                //
                //  which roll was selected?
                foreach (var r in rolls)
                {
                    if (r.Selected)
                    {
                        r.Orientation = TileOrientation.FaceUp;
                        break;
                    }
                }

                OnRolled?.Invoke(new List<RollModel>(Rolls));
            }
            finally
            {
            }

        }

        public PlayerRollCtrl()
        {
            this.InitializeComponent();

            for (int i = 0; i < 4; i++)
            {
                Rolls.Add(new RollModel());
            }

            Randomize();
        }

        public event OnRolledHandler OnRolled;

        public event OnRolledHandler OnShowAllRolls;

        public void Randomize()
        {
            Rolls.ForEach((roll) => roll.Randomize());
        }

        public async Task Reset()
        {
            PopulateControlList();
            Rolls.ForEach((roll) =>
               {
                   roll.Orientation = TileOrientation.FaceDown;
                   roll.Randomize();
               });

            _rolled = false;
            
            await ShowAllRollsLog.Post(MainPage.Current, new List<RollModel>(Rolls));
            
        }
    }
}
