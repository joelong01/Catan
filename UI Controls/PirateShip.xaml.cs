using Windows.Foundation;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class PirateShip : UserControl
    {
        #region Constructors

        public PirateShip()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Methods

        public void MoveAsync(Point to)
        {
            _daX.To = to.X;
            _daY.To = to.Y;
            _sbMove.Begin();
        }

        #endregion Methods
    }
}
