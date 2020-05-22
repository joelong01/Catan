﻿using System;
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
        public static Brush GetForegroundBush(PlayerModel current, PlayerModel owner)
        {
            if (StaticHelpers.IsInVisualStudioDesignMode)
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
        public static LinearGradientBrush GetBackgroundBush(PlayerModel current, PlayerModel owner)
        {
            if (StaticHelpers.IsInVisualStudioDesignMode)
            {

                var gradientStopCollection = new GradientStopCollection();
                gradientStopCollection.Add(new GradientStop() { Color = Colors.Black }); ;
                gradientStopCollection.Add(new GradientStop() { Color = Colors.DarkGray });
                var brush = new LinearGradientBrush(gradientStopCollection, 45);
                brush.StartPoint = new Windows.Foundation.Point(0.5, 0);
                brush.EndPoint = new Windows.Foundation.Point(0.5, 1.0);
                return brush;
            }
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
