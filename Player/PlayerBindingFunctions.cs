using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Catan10
{
    public static class PlayerBindingFunctions
    {
        public static Brush PickPlayerBackground(PlayerModel owner, PlayerModel current)
        {
            if (owner != null)
            {
                return owner.BackgroundBrush;
            }
            if (current != null)
            {
                return current.BackgroundBrush;
            }

            var brush = ConverterGlobals.GetLinearGradientBrush(Colors.Purple, Colors.Black);
            return brush;
        }
        public static Brush PickPlayerForegroundBrush(PlayerModel owner, PlayerModel current)
        {
            if (owner != null)
            {
                return owner.ForegroundBrush;
            }
            if (current != null)
            {
                return current.ForegroundBrush;
            }

            var brush = ConverterGlobals.GetBrush(Colors.HotPink);
            return brush;
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

        public static int PerceivedBrightness(Color c)
        {
            return (int)Math.Sqrt(
            c.R * c.R * .299 +
            c.G * c.G * .587 +
            c.B * c.B * .114);
        }

        
    }
}
