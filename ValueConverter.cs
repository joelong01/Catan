﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using Windows.Storage;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;

namespace Catan10
{
    //
    //  DON'T FORGET: add your converter class to app.xaml as a resource...
    public class StringToIntConverter : IValueConverter
    {
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
    }
    public class IntToStringConverter : IValueConverter
    {
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
    }

    public class IntToVisibilityConverter : IValueConverter
    {
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
    }


    /// <summary>
    ///     given a HarborType return the proper image
    /// </summary>
    public class HarborTypeToHarborBrush : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            HarborType harbor = (HarborType)value;
            string bitmapPath = "ms-appx:Assets/back.jpg";
            switch (harbor)
            {
                case HarborType.ThreeForOne:
                    bitmapPath = "ms-appx:Assets/Old Visuals/old 3 for 1.png";
                    break;
                case HarborType.Brick:
                    bitmapPath = "ms-appx:Assets/Old Visuals/old 2 for 1 brick.png";
                    break;
                case HarborType.Ore:
                    bitmapPath = "ms-appx:Assets/Old Visuals/old 2 for 1 Ore.png";
                    break;
                case HarborType.Sheep:
                    bitmapPath = "ms-appx:Assets/Old Visuals/old 2 for 1 sheep.png";
                    break;
                case HarborType.Wood:
                    bitmapPath = "ms-appx:Assets/Old Visuals/old 2 for 1 wood.png";
                    break;
                case HarborType.Wheat:
                    bitmapPath = "ms-appx:Assets/Old Visuals/old 2 for 1 wheat.png";
                    break;
                default:
                    break;
            }
            BitmapImage bitmapImage = new BitmapImage(new Uri(bitmapPath, UriKind.RelativeOrAbsolute));
            ImageBrush brush = new ImageBrush
            {
                AlignmentX = AlignmentX.Left,
                AlignmentY = AlignmentY.Top,
                Stretch = Stretch.UniformToFill,
                ImageSource = bitmapImage
            };

            return brush;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new Exception("HarborTypeToHarborBrush cannot be used in a TwoWay binding");
        }
    }

    /// <summary>
    ///     used in the building control.  this takes a parameter to indicate which state should be Visible and all others should be collapsed
    ///     parameter is a string
    /// </summary>
    public class BuildingStateToVisibilityConverter : IValueConverter
    {
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
    }

    /// <summary>
    ///     used in the PlayerResourceCountCtrl
    ///     allows you to bind Visibility to *one* gamestate.  if the GameState is the parameter value, then be visible.
    ///     else be collapsed
    /// </summary>
    public class GameStateToVisibilityConverter : IValueConverter
    {
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
    }

    /// <summary>
    ///     used in the MainPage.xaml
    ///     allows you to bind IsEnabled to one or more gamestates. seperate the GameStates with a "|"
    /// </summary>
    public class GameStateToBooleanConverter : IValueConverter
    {
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
    }

    /// <summary>
    ///     used in the road control.  this takes a parameter to indicate which state should be Visible and all others should be collapsed
    ///     parameter is a string
    /// </summary>
    public class RoadStateToVisibilityConverter : IValueConverter
    {
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
    }

    public class CountToOrientationConverter : IValueConverter
    {
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
    }

    public class ColorToColorChoice : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {

            foreach (ColorChoices choice in PlayerModel._availableColors)
            {
                if (choice.Background == (Color)value)
                {
                    if (targetType == typeof(String))
                    {
                        return choice.Name;
                    }

                    return choice;
                }
            }

            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
            {
                return Colors.Blue;
            }

            if (targetType == typeof(Color) && value.GetType() == typeof(String))
            {
                return StaticHelpers.StringToColorDictionary[(string)value];
            }


            return ((ColorChoices)value).Background;
        }
    }

    //
    //  pass in a color, return the index into the ColorChoices array
    public class ColorToColorChoiceIndex : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            for (int idx = 0; idx < PlayerModel._availableColors.Count; idx++)
            {
                ColorChoices choice = PlayerModel._availableColors[idx];
                if (choice.Background == (Color)value)
                {
                    return idx;
                }
            }

            return -1;
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            if ((int)value == -1)
            {
                return PlayerModel._availableColors[4].Background;
            }

            return PlayerModel._availableColors[(int)value].Background;
        }
    }

    //
    //  pass in a list like "1,2,3,4" and convert it to a List<int> {1, 2, 3, 4}
    public class StringToIntListConverter : IValueConverter
    {
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
    }

    /// <summary>
    ///  given something that looks like TileGroups="0-18;True,19-20;True,21-22;True,23-27;True,28-43;False" 
    ///  create a List<TileGroup> 
    /// </summary>
    public class StringToTileGroup : IValueConverter
    {
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
    }

    public class BoolToVisibilityConverter : IValueConverter
    {
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
    }

    public class StringToImageSourceConverter : IValueConverter
    {
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
    }

    public class StringToImageBrushConverter : IValueConverter
    {
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
    }

    public class ColorToBrushConverter : IValueConverter
    {
       
        private static Color Parse(string color)
        {
            var offset = color.StartsWith("#") ? 1 : 0;

            var a = Byte.Parse(color.Substring(0 + offset, 2), NumberStyles.HexNumber);
            var r = Byte.Parse(color.Substring(2 + offset, 2), NumberStyles.HexNumber);
            var g = Byte.Parse(color.Substring(4 + offset, 2), NumberStyles.HexNumber);
            var b = Byte.Parse(color.Substring(6 + offset, 2), NumberStyles.HexNumber);

            return Color.FromArgb(a, r, g, b);
        }

        public object Convert(object value, Type targetType, object parameter, string language)
        {
            if (value == null)
                return new SolidColorBrush(Color.FromArgb(0, 0, 0, 0));

            if (value is Color)
                return new SolidColorBrush((Color)value);

            if (value is string)
                return new SolidColorBrush(Parse((string)value));

            throw new NotSupportedException("ColorToBurshConverter only supports converting from Color and String");
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }



    public class ScoreIntToStringConverter : IValueConverter
    {
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
    }

    public class AnimationSpeedValueConverter : IValueConverter
    {
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
    }

    public class ObjectToObjectValueConverter : IValueConverter
    {
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
    }

    public class StorageFileToString : IValueConverter
    {
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
    }

    public class TileOrientationToObjectConverter : IValueConverter
    {
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
    }



    //
    //  used to bind to IsEnabled - e.g. "if the CurrentTile == null, the control is disabled"
    public class NullToBoolConverter : IValueConverter
    {
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
    }

    public class EnumToStringValueConverter<T>
    {
        public object Convert(object value)
        {
            return value.ToString();
        }
        public object ConvertBack(object value)
        {
            T t = (T)Enum.Parse(typeof(T), value.ToString());
            return t;
        }

    }

    public class ResourceTypeValueConverter : IValueConverter
    {
        EnumToStringValueConverter<ResourceType> _converter = new EnumToStringValueConverter<ResourceType>();
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
    }

    public class HarborTypeValueConverter : IValueConverter
    {
        EnumToStringValueConverter<HarborType> _converter = new EnumToStringValueConverter<HarborType>();
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
    }
    public class GameStateValueConverter : IValueConverter
    {
        EnumToStringValueConverter<GameState> _converter = new EnumToStringValueConverter<GameState>();
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
    }

    public class HarborLocationValueConverter : IValueConverter
    {
        EnumToStringValueConverter<HarborLocation> _converter = new EnumToStringValueConverter<HarborLocation>();
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
    }
    public class TileOrientationValueConverter : IValueConverter
    {
        EnumToStringValueConverter<TileOrientation> _converter = new EnumToStringValueConverter<TileOrientation>();
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
    }

    public class StorageFileValueConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }

        public object ConvertBack(object value, Type targetType, object parameter, string language)
        {
            throw new NotImplementedException();
        }
    }

}
