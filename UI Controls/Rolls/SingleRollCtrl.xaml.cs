
using System.ComponentModel;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;



namespace Catan10
{
    public delegate void RollSelectedHandler(RollModel roll);
    /// <summary>
    ///     has a button with a number on it to enter the roll
    ///     keeps track of the count of the roll and the % of the roll
    ///     when the MainPageModel is set by the parent, it will subscribe to the changed event for the rolls
    ///     when the rolls are changed, it updates the count and the percent.
    ///     TODO: we only need to update the count when the number changes, but we always update the percent. 
    /// </summary>
    public sealed partial class SingleRollCtrl : UserControl
    {
        public event RollSelectedHandler RollSelected;


        public SingleRollCtrl()
        {
            this.InitializeComponent();
        }

        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(TraditionalRollCtrl), new PropertyMetadata(new MainPageModel()));
        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register("Number", typeof(int), typeof(SingleRollCtrl), new PropertyMetadata(2));
        public int Number
        {
            get => ( int )GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }


        public MainPageModel MainPageModel
        {
            get => ( MainPageModel )GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }


        private void OnNumberTapped(object sender, TappedRoutedEventArgs e)
        {
            if (( ( Button )sender ).Content is CatanNumber number)
            {
                RollModel rm = new RollModel()
                {
                    Roll = number.Number
                };
                RollSelected?.Invoke(rm);
            }
        }
    }
}
