
using Windows.UI.Xaml;
using Windows.UI;


namespace Catan10
{
 
    class CatanAttachedProperties : DependencyObject
    {
        public static DependencyProperty CurrentPlayerModelProperty = DependencyProperty.RegisterAttached("GetCurrentPlayerModel", typeof(PlayerModel), typeof(CatanAttachedProperties), new PropertyMetadata(new PlayerModel(), PlayerModelChanged));

       

        public static PlayerModel GetCurrentPlayerModel(DependencyObject obj)
        {
            return (PlayerModel)obj.GetValue(CurrentPlayerModelProperty);
        }
        public static void SetCurrentPlayerModel(DependencyObject obj, PlayerModel current)
        {
            obj.SetValue(CurrentPlayerModelProperty, current);
        }
        private static void PlayerModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (e.NewValue != null && d != null)
            {
                var player = GetCurrentPlayerModel(d);
                SetCurrentPlayerForegroundBrush(d, player.ForegroundColor);
            }
        }

        public static DependencyProperty CurrentPlayerForegroundBrushProperty = DependencyProperty.RegisterAttached("GetCurrentPlayerForegroundBrush", typeof(Color), typeof(CatanAttachedProperties), new PropertyMetadata(Colors.White));
        public static Color GetCurrentPlayerForegroundBrush(DependencyObject obj)
        {
            return (Color)obj.GetValue(CurrentPlayerForegroundBrushProperty);
        }
        public static void SetCurrentPlayerForegroundBrush(DependencyObject obj, Color color)
        {
            obj.SetValue(CurrentPlayerForegroundBrushProperty, color);
        }
    }
}
