using System;
using System.Collections.Generic;
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
    public delegate void RolledDelegate(int roll);
    public sealed partial class PlayerRollCtrl : UserControl
    {
        bool _rolled = false;
        List<RollCtrl> _rollControls = new List<RollCtrl>();

        public RolledDelegate OnRolled;
        public PlayerRollCtrl()
        {
            this.InitializeComponent();
            _rollControls.Add(RollCtrl_One);
            _rollControls.Add(RollCtrl_Two);
            _rollControls.Add(RollCtrl_Three);
            _rollControls.Add(RollCtrl_Four);
        }

        private void Roll_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (_rolled) return;

            RollCtrl rollCtrl = sender as RollCtrl;
            _roll = rollCtrl.Roll;
            rollCtrl.Orientation = TileOrientation.FaceUp;
            OnRolled?.Invoke(_roll);
        }

        private int _roll = 0;
        public int Roll => _roll;

        public async Task Reset()
        {
            var list = new List<Task>();
            _rollControls.ForEach((ctrl) => list.Add(ctrl.GetFlipTask(TileOrientation.FaceDown)));
            await Task.WhenAll(list);
            _rolled = false;

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
    }
}
