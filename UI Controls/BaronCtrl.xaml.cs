using Windows.Foundation;
using Windows.UI.Xaml;
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
        // bound in code behind in CatanHexPanel.cs
        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(BaronCtrl), new PropertyMetadata(MainPageModel.Default));
        public MainPageModel MainPageModel
        {
            get => ( MainPageModel )GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }

        public void MoveAsync(Point to)
        {
            var animationDuration = System.TimeSpan.FromMilliseconds(MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.VeryFast));

            _sbMove.Duration = animationDuration;
            _daX.Duration = animationDuration;
            _daY.Duration = animationDuration;
            _daX.To = to.X;
            _daY.To = to.Y;
            _sbMove.Begin();
        }

        public void SkipAnimationToEnd()
        {
            _sbMove.SkipToFill();
        }

        public void ShowAsync()
        {
            DA_Opacity.To = 1.0;
            SB_AnimateOpacity.Begin();
        }

        public void HideAsync()
        {
            DA_Opacity.To = 0.0;
            SB_AnimateOpacity.Begin();
        }
    }
}
