using System;

using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Catan10
{
    public static class PlayerBindingFunctions
    {
        public static LinearGradientBrush GetBackgroundBrush(PlayerModel current, PlayerModel owner)
        {
            try
            {


                if (owner != null)
                {
                    return owner.BackgroundBrush;
                }

                if (current != null)
                {
                    return current.BackgroundBrush;
                }

                return ConverterGlobals.GetLinearGradientBrush(Colors.Black, Colors.Red);
            }
            catch
            {
                var gradientStopCollection = new GradientStopCollection();
                gradientStopCollection.Add(new GradientStop() { Color = Colors.Black }); ;
                gradientStopCollection.Add(new GradientStop() { Color = Colors.White });
                var brush = new LinearGradientBrush(gradientStopCollection, 45);
                brush.StartPoint = new Windows.Foundation.Point(0.5, 0);
                brush.EndPoint = new Windows.Foundation.Point(0.5, 1.0);
                return brush;
            }
        }

        public static Brush GetForegroundBrush(PlayerModel current, PlayerModel owner)
        {
            //if (StaticHelpers.IsInVisualStudioDesignMode)
            //{
            //    return new SolidColorBrush(Colors.White);
            //}

            if (owner != null)
            {
                return owner.ForegroundBrush;
            }

            if (current != null)
            {
                return current.ForegroundBrush;
            }
          
            return ConverterGlobals.GetBrush(Colors.White);
        }

        public static int PerceivedBrightness(Color c)
        {
            return (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114);
        }

        public static Brush PickPlayerSolidBackground(PlayerModel owner, PlayerModel current)
        {
            if (owner != null)
            {
                return owner.SolidPrimaryBrush;
            }
            if (current != null)
            {
                return current.SolidPrimaryBrush;
            }

            var brush = ConverterGlobals.GetBrush(Colors.Purple);
            return brush;
        }
    }
}
