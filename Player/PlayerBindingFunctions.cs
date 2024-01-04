using System;
using Windows.ApplicationModel;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Xaml.Media;

namespace Catan10
{
    public static class PlayerBindingFunctions
    {
        public static LinearGradientBrush GetBackgroundBrush(PlayerModel current, PlayerModel owner)
        {
            if (DesignMode.DesignModeEnabled)
            {
                return ConverterGlobals.CreateLinearGradiantBrush(Colors.Green, Colors.Black);
            }
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

                return ConverterGlobals.GetLinearGradientBrush(owner.PrimaryBackgroundColor, owner.SecondaryBackgroundColor);
            }
            catch
            {
                return ConverterGlobals.CreateLinearGradiantBrush(Colors.Black, Colors.HotPink);
            }
        }

      

        public static SolidColorBrush GetForegroundBrush(PlayerModel current, PlayerModel owner)
        {
            try
            {
                if (DesignMode.DesignModeEnabled)
                {
                    return new SolidColorBrush(Colors.White);
                }

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
            catch
            {
                return new SolidColorBrush(Colors.White);
            }

        }

        public static int PerceivedBrightness(Color c)
        {
            return ( int )Math.Sqrt(
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
