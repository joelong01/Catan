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
                return owner.SolidBackgroupBrush;
            }
            if (current != null)
            {
                return current.SolidBackgroupBrush;
            }

            var brush = ConverterGlobals.GetBrush(Colors.Purple);
            return brush;
        }

        public static Visibility UseLightFiles (PlayerModel player, bool dark)
        {
            if (player == null && dark) return Visibility.Collapsed;

            if (player.ForegroundColor == Colors.White)
            {
                return dark ? Visibility.Collapsed : Visibility.Visible;
            }

            return dark ? Visibility.Visible : Visibility.Collapsed;
            
        }
    }
}
