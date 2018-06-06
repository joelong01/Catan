using Windows.Foundation;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class BaronCtrl : UserControl
    {
        public BaronCtrl()
        {
            this.InitializeComponent();
        }

        public void MoveAsync(Point to)
        {
            _daX.To = to.X;
            _daY.To = to.Y;            
            _sbMove.Begin();
        }
    }
}
