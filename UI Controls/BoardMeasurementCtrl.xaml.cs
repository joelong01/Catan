using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class BoardMeasurementCtrl : UserControl
    {
        #region Fields

        public static readonly DependencyProperty FiveStarPositionsProperty = DependencyProperty.Register("FiveStarPositions", typeof(int), typeof(BoardMeasurementCtrl), new PropertyMetadata(0));
        public static readonly DependencyProperty FourStarPositionsProperty = DependencyProperty.Register("FourStarPositions", typeof(int), typeof(BoardMeasurementCtrl), new PropertyMetadata(0));
        public static readonly DependencyProperty PipCountProperty = DependencyProperty.Register("PipCount", typeof(TradeResources), typeof(BoardMeasurementCtrl), new PropertyMetadata(new TradeResources()));
        public static readonly DependencyProperty ThreeStarPositionsProperty = DependencyProperty.Register("ThreeStarPositions", typeof(int), typeof(BoardMeasurementCtrl), new PropertyMetadata(0));
        public static readonly DependencyProperty TwoStarPositionsProperty = DependencyProperty.Register("TwoStarPositions", typeof(int), typeof(BoardMeasurementCtrl), new PropertyMetadata(0));

        #endregion Fields

        #region Constructors

        public BoardMeasurementCtrl()
        {
            this.InitializeComponent();
        }

        #endregion Constructors

        #region Properties

        public int FiveStarPositions
        {
            get => (int)GetValue(FiveStarPositionsProperty);
            set => SetValue(FiveStarPositionsProperty, value);
        }

        public int FourStarPositions
        {
            get => (int)GetValue(FourStarPositionsProperty);
            set => SetValue(FourStarPositionsProperty, value);
        }

        public TradeResources PipCount
        {
            get => (TradeResources)GetValue(PipCountProperty);
            set => SetValue(PipCountProperty, value);
        }

        public int ThreeStarPositions
        {
            get => (int)GetValue(ThreeStarPositionsProperty);
            set => SetValue(ThreeStarPositionsProperty, value);
        }

        public int TwoStarPositions
        {
            get => (int)GetValue(TwoStarPositionsProperty);
            set => SetValue(TwoStarPositionsProperty, value);
        }

        #endregion Properties
    }
}
