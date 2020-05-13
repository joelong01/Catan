using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Devices.Enumeration;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{

    public sealed partial class PlayerRollCtrl : UserControl
    {
        bool _rolled = false;
        List<RollCtrl> _rollControls = new List<RollCtrl>();
        TaskCompletionSource<int> RollTcs { get; set; } = null;


        public PlayerRollCtrl()
        {
            this.InitializeComponent();
            this.DataContext = this;
            _rollControls.Add(RollCtrl_One);
            _rollControls.Add(RollCtrl_Two);
            _rollControls.Add(RollCtrl_Three);
            _rollControls.Add(RollCtrl_Four);
        }

        private void Roll_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_rolled) return;

            RollCtrl rollCtrl = sender as RollCtrl;
            DiceOne = rollCtrl.DiceOne;
            DiceTwo = rollCtrl.DiceTwo;
            
            rollCtrl.Orientation = TileOrientation.FaceUp;
            if (RollTcs != null)
            {
                RollTcs.SetResult(Roll);
                RollTcs = null;
            }
        }

        public int Roll => DiceOne + DiceTwo;
        public int DiceOne { get; private set; } = 0;
        public int DiceTwo { get; private set; } = 0;

        public async Task Reset()
        {
            var list = new List<Task>();
            _rollControls.ForEach((ctrl) =>
            {
                var task = ctrl.GetFlipTask(TileOrientation.FaceDown);
                list.Add(task);
            });
            await Task.WhenAll(list);
            _rolled = false;
            Randomize();

        }

        public void Randomize()
        {
            _rollControls.ForEach((ctrl) => ctrl.Randomize());
        }
        private bool clicked = false;
        private async void OnShowAll(object sender, RoutedEventArgs e)
        {
            if (clicked) return;
            clicked = true;

            var list = new List<Task>();
            _rollControls.ForEach((ctrl) => list.Add(ctrl.GetFlipTask(TileOrientation.FaceUp)));
            await Task.WhenAll(list);
            clicked = false;
        }

        private async void OnReset(object sender, RoutedEventArgs e)
        {
            await Reset();
            Randomize();
        }

        private async void OnFaceUp(object sender, RoutedEventArgs e)
        {
            var list = new List<Task>();
            _rollControls.ForEach((ctrl) => list.Add(ctrl.GetFlipTask(TileOrientation.FaceUp)));
            await Task.WhenAll(list);
        }

        private async void OnFaceDown(object sender, RoutedEventArgs e)
        {
            var list = new List<Task>();
            _rollControls.ForEach((ctrl) => list.Add(ctrl.GetFlipTask(TileOrientation.FaceDown)));
            await Task.WhenAll(list);
        }

        internal async Task<int> GetRoll()
        {
            Contract.Assert(RollTcs == null);
            RollTcs = new TaskCompletionSource<int>();
            await this.Reset();
            return await RollTcs.Task;

        }
    }
}
