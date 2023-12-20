using System;
using Catan.Proxy;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void ShowPipsHandler(int pipCount);
    public sealed partial class BoardMeasurementCtrl : UserControl
    {
        public int FiveStarPositions
        {
            get => ( int )GetValue(FiveStarPositionsProperty);
            set => SetValue(FiveStarPositionsProperty, value);
        }

        public int FourStarPositions
        {
            get => ( int )GetValue(FourStarPositionsProperty);
            set => SetValue(FourStarPositionsProperty, value);
        }

        public TradeResources PipCount
        {
            get => ( TradeResources )GetValue(PipCountProperty);
            set => SetValue(PipCountProperty, value);
        }

        public int ThreeStarPositions
        {
            get => ( int )GetValue(ThreeStarPositionsProperty);
            set => SetValue(ThreeStarPositionsProperty, value);
        }

        public int TwoStarPositions
        {
            get => ( int )GetValue(TwoStarPositionsProperty);
            set => SetValue(TwoStarPositionsProperty, value);
        }

        public static readonly DependencyProperty FiveStarPositionsProperty = DependencyProperty.Register("FiveStarPositions", typeof(int), typeof(BoardMeasurementCtrl), new PropertyMetadata(0));
        public static readonly DependencyProperty FourStarPositionsProperty = DependencyProperty.Register("FourStarPositions", typeof(int), typeof(BoardMeasurementCtrl), new PropertyMetadata(0));
        public static readonly DependencyProperty PipCountProperty = DependencyProperty.Register("PipCount", typeof(TradeResources), typeof(BoardMeasurementCtrl), new PropertyMetadata(new TradeResources()));
        public static readonly DependencyProperty ThreeStarPositionsProperty = DependencyProperty.Register("ThreeStarPositions", typeof(int), typeof(BoardMeasurementCtrl), new PropertyMetadata(0));
        public static readonly DependencyProperty TwoStarPositionsProperty = DependencyProperty.Register("TwoStarPositions", typeof(int), typeof(BoardMeasurementCtrl), new PropertyMetadata(0));
        public event ShowPipsHandler ShowPips;

        public BoardMeasurementCtrl()
        {
            this.InitializeComponent();
        }

        private void OnPointerPressed(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            var number = Int32.Parse(((FrameworkElement)sender).Tag as String);
            ShowPips?.Invoke(number);
        }
    }
}
