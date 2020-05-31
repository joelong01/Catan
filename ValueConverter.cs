using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Catan10
{
    public static class ConverterGlobals
    {
        #region Properties

        public static Dictionary<(Color, Color), LinearGradientBrush> LinearGradientBrushCache { get; } = new Dictionary<(Color, Color), LinearGradientBrush>();
        public static Dictionary<Color, SolidColorBrush> SolidColorBrushCache { get; } = new Dictionary<Color, SolidColorBrush>();

        #endregion Properties

        #region Methods

        public static SolidColorBrush GetBrush(Color color)
        {
            if (StaticHelpers.IsInVisualStudioDesignMode)
            {
                return new SolidColorBrush(color);
            }

            if (ConverterGlobals.SolidColorBrushCache.TryGetValue(color, out SolidColorBrush brush))
            {
                return brush;
            }

            brush = new SolidColorBrush(color);
            ConverterGlobals.SolidColorBrushCache[color] = brush;
            return brush;
        }

        public static LinearGradientBrush GetLinearGradientBrush(Color color1, Color color2)
        {
            //< LinearGradientBrush EndPoint = "0.5,1" StartPoint = "0.5,0" >

            //               < GradientStop Color = "Black" />

            //                < GradientStop Color = "Purple" Offset = "1" />

            //               </ LinearGradientBrush >

            if (!StaticHelpers.IsInVisualStudioDesignMode && ConverterGlobals.LinearGradientBrushCache.TryGetValue((color1, color2), out LinearGradientBrush brush))
            {
                return brush;
            }
            var gradientStopCollection = new GradientStopCollection();
            gradientStopCollection.Add(new GradientStop() { Color = color1, Offset = 0 });
            gradientStopCollection.Add(new GradientStop() { Color = color2, Offset = 1 });
            brush = new LinearGradientBrush(gradientStopCollection, 45);
            brush.StartPoint = new Windows.Foundation.Point(0, 0);
            brush.EndPoint = new Windows.Foundation.Point(1.0, 1.0);

            if (!StaticHelpers.IsInVisualStudioDesignMode)
            {
                ConverterGlobals.LinearGradientBrushCache[(color1, color2)] = brush;
            }
            return brush;
        }

        #endregion Methods
    }

    //public class RoadOwnerOrVisitorColorConverter : IValueConverter
    //{
    //    public object Convert(object value, Type targetType, object parameter, string language)
    //    {
    //        RoadCtrl ctrl = (RoadCtrl)value;
    //        if (ctrl != null)
    //        {
    //            if (ctrl.Owner != null)
    //            {
    //                if ((string)parameter == "Foreground")
    //                {
    //                    return ConverterGlobals.GetBrush(ctrl.Owner.Foreground);
    //                }
    //                else
    //                {
    //                    return ctrl.Owner.;
    //                }
    //            }
    //        }
    //    }

    //    public object ConvertBack(object value, Type targetType, object parameter, string language)
    //    {
    //        throw new NotImplementedException();
    //    }
    //}

    public class AnimationSpeedValueConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter,
                              string language)
        {
            // val goes 1-4

            string[] Names = new string[] { "Slow - Fun to watch", "Medium - pretty normal", "Faster - if you are in a hurry", "Super Fast -- if you are debugging" };

            int val = System.Convert.ToInt32(value);

            return Names[val - 1];
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }

    public class BoolToOrientationConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((bool)value) ? TileOrientation.FaceUp : TileOrientation.FaceDown;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is TileOrientation)
                {
                    if (((TileOrientation)value) == TileOrientation.FaceDown)
                    {
                        return false;
                    }
                    else
                    {
                        return true;
                    }
                }

                return false;
            }
            catch
            {
                return false;
            }
        }

        #endregion Methods
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is bool)
            {
                if (parameter is string)
                {
                    if (((string)parameter).ToLower() == "true")  // pass in TRUE to invert!
                    {
                        return ((bool)value == false) ? Visibility.Visible : Visibility.Collapsed;
                    }
                }
                return (bool)value ? Visibility.Visible : Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((Visibility)value == Visibility.Visible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        #endregion Methods
    }

    /// <summary>
    ///     used in the building control.  this takes a parameter to indicate which state should be Visible and all others should be collapsed
    ///     parameter is a string
    /// </summary>
    public class BuildingStateToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            BuildingState desiredState = StaticHelpers.ParseEnum<BuildingState>((string)parameter);
            BuildingState actualState = (BuildingState)value;
            if (actualState == desiredState)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("BuildingStateToVisibilityConverter cannot be used in a TwoWay binding");
        }

        #endregion Methods
    }

    public class CachedColorToBrushConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            Color color = (Color)value;
            return ConverterGlobals.GetBrush(color);
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            SolidColorBrush brush = (SolidColorBrush)value;
            return brush.Color;
        }

        #endregion Methods
    }

    public class CountToOrientationConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return ((int)value) > 0 ? TileOrientation.FaceUp : TileOrientation.FaceDown;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                if (value is TileOrientation)
                {
                    if (((TileOrientation)value) == TileOrientation.FaceDown)
                    {
                        return 0;
                    }
                    else
                    {
                        return 100;
                    }
                }

                return 60;
            }
            catch
            {
                return 0;
            }
        }

        #endregion Methods
    }

    public class DiceRollToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            int diceNumber = (int)Int32.Parse((string)parameter);
            int rolledNumber = (int)value;
            if (diceNumber == rolledNumber)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("DiceRollToVisibilityConverter cannot be used in a TwoWay binding");
        }

        #endregion Methods
    }

    public class EnumToStringValueConverter<T>
    {
        #region Methods

        public object Convert(object value)
        {
            return value.ToString();
        }

        public object ConvertBack(object value)
        {
            T t = (T)Enum.Parse(typeof(T), value.ToString());
            return t;
        }

        #endregion Methods
    }

    /// <summary>
    ///     used in the MainPage.xaml
    ///     allows you to bind IsEnabled to one or more gamestates. seperate the GameStates with a "|"
    /// </summary>
    public class GameStateToBooleanConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            var tokens = (parameter as string).Split('|', StringSplitOptions.RemoveEmptyEntries);

            foreach (var e in tokens)
            {
                GameState desiredState = StaticHelpers.ParseEnum<GameState>(e);
                GameState actualState = (GameState)value;
                if (actualState == desiredState)
                {
                    return true;
                }
            }

            return false;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("GameStateToBooleanConverter cannot be used in a TwoWay binding");
        }

        #endregion Methods
    }

    /// <summary>
    ///     used in the PlayersTrackerCtrl
    ///     allows you to bind Visibility to *one* gamestate.  if the GameState is the parameter value, then be visible.
    ///     else be collapsed
    /// </summary>
    public class GameStateToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            GameState desiredState = StaticHelpers.ParseEnum<GameState>((string)parameter);
            GameState actualState = (GameState)value;
            if (actualState == desiredState)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("BuildingStateToVisibilityConverter cannot be used in a TwoWay binding");
        }

        #endregion Methods
    }

    public class GameStateValueConverter : IValueConverter
    {
        #region Fields

        private readonly EnumToStringValueConverter<GameState> _converter = new EnumToStringValueConverter<GameState>();

        #endregion Fields

        #region Methods

        public object Convert(object value, Type targetType, object parameter,
                              string language)
        {
            return _converter.Convert(value);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string language)
        {
            return _converter.ConvertBack(value);
        }

        #endregion Methods
    }

    public class HarborLocationValueConverter : IValueConverter
    {
        #region Fields

        private readonly EnumToStringValueConverter<HarborLocation> _converter = new EnumToStringValueConverter<HarborLocation>();

        #endregion Fields

        #region Methods

        public object Convert(object value, Type targetType, object parameter,
                              string language)
        {
            return _converter.Convert(value);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string language)
        {
            return _converter.ConvertBack(value);
        }

        #endregion Methods
    }

    /// <summary>
    ///     given a HarborType return the proper image
    /// </summary>
    public class HarborTypeToHarborBrushConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            HarborType harbor = (HarborType)value;

            switch (harbor)
            {
                case HarborType.ThreeForOne:
                    return App.Current.Resources["bmThreeForOne"];

                case HarborType.Brick:
                    return App.Current.Resources["bmTwoForOneBrick"];

                case HarborType.Ore:
                    return App.Current.Resources["bmTwoForOneOre"];

                case HarborType.Sheep:
                    return App.Current.Resources["bmTwoForOneSheep"];

                case HarborType.Wood:
                    return App.Current.Resources["bmTwoForOneWood"];

                case HarborType.Wheat:
                    return App.Current.Resources["bmTwoForOneWheat"];

                default:
                    throw new Exception($"Unexpected Harbor Type {harbor} in value HarborTypeToHarborBrushConverter");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("HarborTypeToHarborBrush cannot be used in a TwoWay binding");
        }

        #endregion Methods
    }

    public class HarborTypeValueConverter : IValueConverter
    {
        #region Fields

        private readonly EnumToStringValueConverter<HarborType> _converter = new EnumToStringValueConverter<HarborType>();

        #endregion Fields

        #region Methods

        public object Convert(object value, Type targetType, object parameter,
                              string language)
        {
            return _converter.Convert(value);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string language)
        {
            return _converter.ConvertBack(value);
        }

        #endregion Methods
    }

    public class IntToStringConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                return Int32.Parse((string)value);
            }
            catch
            {
                return 0;
            }
        }

        #endregion Methods
    }

    public class IntToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if ((int)value > 0)
                return Visibility.Visible;

            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException("IntToVisibilityConverter can't convert back");
        }

        #endregion Methods
    }

    //
    //  used to bind to IsEnabled - e.g. "if the CurrentTile == null, the control is disabled"
    public class NullToBoolConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter,
                              string language)
        {
            if (value == null)
            {
                return false;
            }

            return true;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string language)
        {
            return value;
        }

        #endregion Methods
    }

    public class ObjectToObjectValueConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter,
                              string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string language)
        {
            return value;
        }

        #endregion Methods
    }

    public class ResourceTypeToTileBrushConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ResourceType resource = (ResourceType)value;
            switch (resource)
            {
                case ResourceType.Sheep:
                    return (ImageBrush)App.Current.Resources["bmTileSheep"];

                case ResourceType.Wood:
                    return (ImageBrush)App.Current.Resources["bmTileWood"];

                case ResourceType.Ore:
                    return (ImageBrush)App.Current.Resources["bmTileOre"];

                case ResourceType.Wheat:
                    return (ImageBrush)App.Current.Resources["bmTileWheat"];

                case ResourceType.Brick:
                    return (ImageBrush)App.Current.Resources["bmTileBrick"];

                case ResourceType.GoldMine:
                    return (ImageBrush)App.Current.Resources["bmTileGoldMine"];

                case ResourceType.Desert:
                    return (ImageBrush)App.Current.Resources["bmTileDesert"];

                case ResourceType.Back:
                    return (ImageBrush)App.Current.Resources["bmTileBack"];

                case ResourceType.Sea:
                    return (ImageBrush)App.Current.Resources["bmTileSheep"];

                case ResourceType.None:
                    return (ImageBrush)App.Current.Resources["TransparentBrush"];

                default:
                    throw new Exception($"Unexpected value type in ResourceType {resource} in LoadTileImage");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("HarborTypeToHarborBrush cannot be used in a TwoWay binding");
        }

        #endregion Methods
    }

    /// <summary>
    ///     used in the Tile Control for the "small resource type".  this takes a parameter to indicate which state should be Visible and all others should be collapsed
    ///     parameter is a string
    /// </summary>
    public class ResourceTypeToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            ResourceType desiredState = StaticHelpers.ParseEnum<ResourceType>((string)parameter);
            ResourceType actualState = (ResourceType)value;
            if (actualState == desiredState)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("BuildingStateToVisibilityConverter cannot be used in a TwoWay binding");
        }

        #endregion Methods
    }

    public class ResourceTypeValueConverter : IValueConverter
    {
        #region Fields

        private readonly EnumToStringValueConverter<ResourceType> _converter = new EnumToStringValueConverter<ResourceType>();

        #endregion Fields

        #region Methods

        public object Convert(object value, Type targetType, object parameter,
                              string language)
        {
            return _converter.Convert(value);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string language)
        {
            return _converter.ConvertBack(value);
        }

        #endregion Methods
    }

    /// <summary>
    ///     used in the road control.  this takes a parameter to indicate which state should be Visible and all others should be collapsed
    ///     parameter is a string
    /// </summary>
    public class RoadStateToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            RoadState desiredState = StaticHelpers.ParseEnum<RoadState>((string)parameter);
            RoadState actualState = (RoadState)value;
            if (actualState == desiredState)
            {
                return Visibility.Visible;
            }
            return Visibility.Collapsed;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("BuildingStateToVisibilityConverter cannot be used in a TwoWay binding");
        }

        #endregion Methods
    }

    public class ScoreIntToStringConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            return "Score: " + value.ToString();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                string s = ((string)value).TrimStart(new char[] { 'S', 'c', 'o', 'r', 'e', ':', ' ' });
                return Int32.Parse(s);
            }
            catch
            {
                return 0;
            }
        }

        #endregion Methods
    }

    public class StorageFileToString : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter,
                              string language)
        {
            if (value == null)
            {
                return value;
            }

            ObservableCollection<string> col = new ObservableCollection<string>();
            foreach (StorageFile f in value as ObservableCollection<StorageFile>)
            {
                col.Add(f.DisplayName);
            }

            return col;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string language)
        {
            if (parameter.GetType() != typeof(ComboBox))
            {
                return null;
            }

            ComboBox bx = parameter as ComboBox;
            ObservableCollection<StorageFile> list = bx.Tag as ObservableCollection<StorageFile>;
            foreach (StorageFile f in list)
            {
                if (f.DisplayName == (string)value)
                {
                    return f;
                }
            }

            return null;
        }

        #endregion Methods
    }

    public class StorageFileValueConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }

    public class StringToImageBrushConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (string.IsNullOrEmpty(value as string))
            {
                return null;
            }

            ImageBrush brush = new ImageBrush();
            BitmapImage bitmapImage = new BitmapImage(new Uri(value as string, UriKind.RelativeOrAbsolute))
            {
                DecodePixelHeight = 125,
                DecodePixelWidth = 125,
                CreateOptions = BitmapCreateOptions.IgnoreImageCache
            };
            brush.ImageSource = bitmapImage;
            brush.Stretch = Stretch.UniformToFill;

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            return ((BitmapImage)value).UriSource.ToString();
        }

        #endregion Methods
    }

    public class StringToImageSourceConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (string.IsNullOrEmpty(value as string))
            {
                return null;
            }
            else
            {
                return new BitmapImage(new Uri(value as string, UriKind.Absolute));
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return null;
            }

            return ((BitmapImage)value).UriSource.ToString();
        }

        #endregion Methods
    }

    //
    //  DON'T FORGET: add your converter class to app.xaml as a resource...
    public class StringToIntConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is Int32)
                return value.ToString();

            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            return Int32.TryParse((string)value, out int result) ? result : 0;
        }

        #endregion Methods
    }

    //
    //  pass in a list like "1,2,3,4" and convert it to a List<int> {1, 2, 3, 4}
    public class StringToIntListConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string s = "";
            foreach (int val in (List<int>)value)
            {
                s += val.ToString() + ",";
            }

            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                List<int> list = new List<int>();
                string s = value as string;
                string[] values = s.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string val in values)
                {
                    list.Add(Int32.Parse(val));
                }

                return list;
            }
            catch
            {
                return 0;
            }
        }

        #endregion Methods
    }

    /// <summary>
    ///  given something that looks like TileGroups="0-18;True,19-20;True,21-22;True,23-27;True,28-43;False"
    ///  create a List<TileGroup>
    /// </summary>
    public class StringToTileGroup : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            string s = "";
            foreach (TileGroup val in (List<TileGroup>)value)
            {
                s += val.ToString() + ";";
            }

            return s;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            try
            {
                List<TileGroup> list = new List<TileGroup>();
                string s = value as string;
                string[] values = s.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (string val in values)
                {
                    TileGroup tg = new TileGroup();
                    string[] tokens = s.Split(new char[] { '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
                    tg.Start = Int32.Parse(tokens[0]);
                    tg.End = Int32.Parse(tokens[1]);
                    tg.Randomize = bool.Parse(tokens[2]);
                }

                return list;
            }
            catch
            {
                return 0;
            }
        }

        #endregion Methods
    }

    public class StringToVisibilityConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value is string)
            {
                if (String.IsNullOrEmpty((string)value)) return Visibility.Collapsed;
            }

            return Visibility.Visible;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("One Way only");
        }

        #endregion Methods
    }

    public class TileOrientationToObjectConverter : IValueConverter
    {
        #region Methods

        public object Convert(object value, Type targetType, object parameter,
                              string language)
        {
            return value;
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string language)
        {
            return value;
        }

        #endregion Methods
    }

    public class TileOrientationValueConverter : IValueConverter
    {
        #region Fields

        private readonly EnumToStringValueConverter<TileOrientation> _converter = new EnumToStringValueConverter<TileOrientation>();

        #endregion Fields

        #region Methods

        public object Convert(object value, Type targetType, object parameter,
                              string language)
        {
            return _converter.Convert(value);
        }

        public object ConvertBack(object value, Type targetType,
                                  object parameter, string language)
        {
            return _converter.ConvertBack(value);
        }

        #endregion Methods
    }
}
