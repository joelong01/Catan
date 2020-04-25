using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Catan10
{
    //public class ColorChoices
    //{
    //    public Color Background { get; set; } = Colors.Blue;
    //    public Color Foreground { get; set; } = Colors.White;
    //    public string Name { get; set; } = "Blue";

    //    public ColorChoices(string name, Color background, Color foreground)
    //    {
    //        Background = background;
    //        Foreground = foreground;
    //        Name = Background.ToString();
    //    }
    //    public override string ToString()
    //    {
    //        return CatanProxy.Serialize(this);
    //    }
    //}

    public static class CatanColors
    {
        public static Dictionary<string, Color> NameToColorDictionary { get; } = new Dictionary<string, Color>()

        {
            {"Red", Colors.Red },
            {"Yellow", Colors.Yellow},
            {"Green", Colors.Green},
            {"White", Colors.White},
            {"Brown", Colors.Brown},
            {"DarkGray", Colors.DarkGray},
            {"Black", Colors.Black},
            {"Purple", Colors.Purple},
            {"Blue", Colors.Blue }

        };
        public static List<SolidColorBrush> AllAvailableBrushes()
        {
            var list = new List<SolidColorBrush>();
            foreach (var kvp in NameToColorDictionary)
            {
                var scb = GetResourceBrush(kvp.Key, kvp.Value);
                list.Add(scb);
            }
            return list;
        }
        public static ICollection<string> ColorNames => NameToColorDictionary.Keys;
        public static ICollection<Color> AvailableColors => NameToColorDictionary.Values;
        private static string GetKnownColorName(string hexString)
        {
            var color = (System.Drawing.Color)new System.Drawing.ColorConverter().ConvertFromString(hexString);
            System.Drawing.KnownColor knownColor = color.ToKnownColor();

            string name = knownColor.ToString();
            return name.Equals("0") ? "" : name;
        }

        public static SolidColorBrush GetBackgroundBrush(string colorName)
        {
            Color color = NameToColorDictionary[colorName];
            return GetResourceBrush(colorName, color);
        }

        public static (string Name, Color Color) GetForegroundColor(Color background)
        {
            bool isDark = (5 * background.G + 2 * background.R + background.B) <= 8 * 128;
            return isDark ? ("White", Colors.White) : ("Black", Colors.Black);
        }
        public static SolidColorBrush GetForegroundBrush(SolidColorBrush background)
        {
            var fgColor = GetForegroundColor(background.Color);
            
            return GetResourceBrush(fgColor.Name, fgColor.Color);
        }
        public static SolidColorBrush GetForegroundBrush(Color background)
        {
            var fgColor = GetForegroundColor(background);
            return GetResourceBrush(fgColor.Name, fgColor.Color);
        }
        public static SolidColorBrush GetForegroundBrush(string background)
        {
            Color bgColor = NameToColorDictionary[background];
            return GetForegroundBrush(bgColor);
            
        }
        public static SolidColorBrush GetResourceBrush(string Name, Color color)
        {
            if (StaticHelpers.IsInVisualStudioDesignMode)
            {
                return new SolidColorBrush(color);
            }
            string resourceName = Name + "Brush";
            App.Current.Resources.TryGetValue(resourceName, out object ret);
            if (ret != null)
            {
              return (SolidColorBrush)ret;
            }
            App.Current.TraceMessage($"did NOT find {resourceName} brush in resources");
            SolidColorBrush brush = new SolidColorBrush(color);
            return brush;
        }

       

        public static SolidColorBrush GetResourceBrush(string name)
        {
            bool found = NameToColorDictionary.TryGetValue(name, out Color color);
            if (found)
            {
                return GetResourceBrush(name, color);
            }

           return new SolidColorBrush(Colors.HotPink);
        }



    }
}
