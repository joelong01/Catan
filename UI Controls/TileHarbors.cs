using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.System;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Animation;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
       

    public sealed partial class TileCtrl : UserControl, INotifyPropertyChanged
    {
        enum TransformAdjustOption { AdjustX, AdjustY };

        
        List<Harbor> _harbors = new List<Harbor>();
        List<Harbor> _visibleHarbors = new List<Harbor>();
        HarborType[] _harborTypes = new HarborType[7] { HarborType.None, HarborType.None, HarborType.None, HarborType.None, HarborType.None, HarborType.None, HarborType.None };
        Dictionary<HarborLocation, Harbor> _dictionaryHarbors = new Dictionary<HarborLocation, Harbor>();
        Dictionary<Harbor, Point> _startingTranslatePoints = new Dictionary<Harbor, Point>(); // set in the ctor


        public List<Harbor> VisibleHarbors
        {
            get
            {
                List<Harbor> list = new List<Harbor>();
                foreach (var kvp in _dictionaryHarbors)
                {
                    if (kvp.Key != HarborLocation.None && kvp.Value.Visibility == Visibility.Visible)
                        list.Add(kvp.Value);
                }

                return list;
            }
        }

        public Harbor Harbor
        {
            get
            {
                return GetVisibleHarbor();
            }
        }

        public List<Harbor> Harbors
        {
            get
            {
                return _harbors;
            }
        }

        public TileOrientation HarborOrientations
        {
            get
            {
                if (_visibleHarbors.Count > 0)
                    return _visibleHarbors[0].Orientation;

                return TileOrientation.None;

            }

        }
        private int GetLocationIndex(HarborLocation location)
        {
            int idx = 0;
            switch (location)
            {
                case HarborLocation.None:
                    idx = 0;
                    break;
                case HarborLocation.TopRight:
                    idx = 1;
                    break;
                case HarborLocation.TopLeft:
                    idx = 2;
                    break;
                case HarborLocation.BottomRight:
                    idx = 3;
                    break;
                case HarborLocation.BottomLeft:
                    idx = 4;
                    break;
                case HarborLocation.Top:
                    idx = 5;
                    break;
                case HarborLocation.Bottom:
                    idx = 6;
                    break;
                default:
                    this.TraceMessage("Bad index!");
                    idx = -1; // this will cause a throw...
                    break;
            }

            return idx;
        }

        

        public string HarborLocationsAsShortText
        {
            get
            {
                string s = "";
                foreach (var kvp in _dictionaryHarbors)
                {
                    if (IsHarborVisible(kvp.Key))                    
                        s += String.Format($"{kvp.Key},");
                }

                if (s == "") return "None";

                return s.TrimEnd(new char[] { ',' });
            }
        }
        public string HarborLocations
        {
            get
            {
                string s = "";
                foreach (var kvp in _dictionaryHarbors)
                {
                    s += String.Format($"{kvp.Key}:{kvp.Value.HarborType},");
                }

                return s;
            }
            set
            {
                string[] tokens = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Length != 6)
                {
                    this.TraceMessage("You have a bad saved string -- not enough harborlocations.");
                    return;
                }

                foreach (string s in tokens)
                {
                    string[] pairs = s.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                    if (pairs.Length != 2)
                    {
                        this.TraceMessage("You have a bad saved string -- too many colons in a pair");
                        return;
                    }

                    HarborLocation loc = (HarborLocation)Enum.Parse(typeof(HarborLocation), pairs[0]);
                    HarborType type = (HarborType)Enum.Parse(typeof(HarborType), pairs[1]);
                    _dictionaryHarbors[loc].HarborType = type;
                    if (type != HarborType.None)
                    {
                        _dictionaryHarbors[loc].Visibility = Visibility.Visible;
                    }
                    else
                    {
                        _dictionaryHarbors[loc].Visibility = Visibility.Collapsed;
                    }
                }

                UpdateHarborLocations();
            }
        }

        public void AddHarborLocation(HarborLocation location)
        {
            if (location == HarborLocation.None)
            {
                return;
            }
            Harbor h = _dictionaryHarbors[location];
            h.Visibility = Visibility.Visible;
            h.SetOrientationAsync(TileOrientation.FaceUp);
            h.HarborType = HarborType.ThreeForOne;
            UpdateHarborLocations();
        }



        public void RemoveHarborLocation(HarborLocation location)
        {
            if (location == HarborLocation.None)
            {
                return;
            }
            Harbor h = _dictionaryHarbors[location];
            h.Visibility = Visibility.Collapsed;
            h.HarborType = HarborType.None;
            UpdateHarborLocations();
        }

       

        public bool IsHarborVisible(HarborLocation location)
        {
            if (location == HarborLocation.None) return false;
            return _dictionaryHarbors[location].Visibility == Visibility.Visible;
        }

        public void SetHarborType(HarborLocation location, HarborType harborType)
        {
            _dictionaryHarbors[location].HarborType = harborType;
        }

        public HarborType GetHarborTypeAtLocation(HarborLocation location)
        {
            return _dictionaryHarbors[location].HarborType;
        }

        internal bool HasHarborType(HarborType type)
        {
            foreach (var kvp in _dictionaryHarbors)
            {
                if (kvp.Value.Visibility == Visibility.Visible)
                {
                    if (kvp.Value.HarborType == type )
                    {
                        return true;
                    }
                }                    
            }        
            return false;
        }

        internal HarborLocation GetHarborLocationForHarbor(Harbor harbor)
        {
            foreach (var kvp in _dictionaryHarbors)
            {
                if (kvp.Value == harbor)
                {
                   return kvp.Key;
                }
            }
            throw new InvalidDataException("No key found in GetHarborLocationForHarbor");
        }

        //
        //  for legacy...sigh.
        public HarborType HarborType
        {
            get
            {
                foreach (var kvp in _dictionaryHarbors)
                {
                    if (kvp.Value.Visibility == Visibility.Visible)
                        return kvp.Value.HarborType;
                }
                return HarborType.None;
            }
            set
            {
                foreach (var kvp in _dictionaryHarbors)
                {
                    if (kvp.Value.Visibility == Visibility.Visible)
                    {
                        kvp.Value.HarborType = value;
                        return;
                    }

                }
            }
        }
        public HarborLocation HarborLocation
        {
            get
            {
                foreach (var kvp in _dictionaryHarbors)
                {
                    if (kvp.Value.Visibility == Visibility.Visible)
                        return kvp.Key;
                }
                return HarborLocation.None;
            }
            set
            {
                foreach (var kvp in _dictionaryHarbors)
                {
                    if (kvp.Key == value)
                    {
                        kvp.Value.Visibility = Visibility.Visible;
                        kvp.Value.HarborType = HarborType.ThreeForOne;
                    }
                    else
                    {
                        kvp.Value.Visibility = Visibility.Collapsed;
                        kvp.Value.HarborType = HarborType.None;
                    }
                }

                UpdateHarborLocations();
            }
        }

        private bool Visible(HarborLocation location) { return _dictionaryHarbors[location].Visibility == Visibility.Visible; }

        private VerticalAlignment GetVerticalAlignment()
        {
            VerticalAlignment va = VerticalAlignment.Center;
            if (Visible(HarborLocation.Top) && !Visible(HarborLocation.Bottom))
            {
                va = VerticalAlignment.Bottom;
            }
            else if (!Visible(HarborLocation.Top) && Visible(HarborLocation.Bottom))
            {
                va = VerticalAlignment.Top;
            }
            else if (Visible(HarborLocation.Top) && Visible(HarborLocation.Bottom))
            {
                // both! -- being explict
                va = VerticalAlignment.Center;
            }
            else
            {
                this.Assert((!Visible(HarborLocation.Top) && !Visible(HarborLocation.Bottom)), "Error in bitchecking logic!");
            }


            // both or none
            return va;


        }

        private HorizontalAlignment GetHorizontalAlignment()
        {
            HorizontalAlignment ha = HorizontalAlignment.Center;


            bool leftHarbors = (Visible(HarborLocation.TopLeft) || Visible(HarborLocation.BottomLeft));
            bool rightHarbors = (Visible(HarborLocation.TopRight) || Visible(HarborLocation.BottomRight));

            if (leftHarbors && !rightHarbors)
                ha = HorizontalAlignment.Right;
            else if (!leftHarbors && rightHarbors)
                ha = HorizontalAlignment.Left;
            else if ((!leftHarbors && !rightHarbors))
                ha = HorizontalAlignment.Center;
            else if ((leftHarbors && rightHarbors))
            {
                ha = HorizontalAlignment.Center;
            }
            else
            {
                this.Assert(leftHarbors && rightHarbors, "Error in bitchecking logi");
            }

            return ha;
        }

        double GetWidth()
        {
            double width = _normalWidth;
            bool leftHarbors = (Visible(HarborLocation.TopLeft) == true || Visible(HarborLocation.BottomLeft) == true);
            bool rightHarborts = (Visible(HarborLocation.TopRight) == true || Visible(HarborLocation.BottomRight) == true);
            if (leftHarbors && !rightHarborts)
                width = 137.0;
            else if (!leftHarbors && rightHarborts)
                width = 137.0;
            else if ((!leftHarbors && !rightHarborts))
                width = _normalWidth;
            else if ((leftHarbors && rightHarborts))
            {
                width = 164;
            }
            else
            {
                this.Assert(leftHarbors && rightHarborts, "Error in bitchecking logi");
            }

            return width;
        }

        double GetHeight()
        {
            double height = _normalHeight;
            if (Visible(HarborLocation.Top) && !Visible(HarborLocation.Bottom))
            {
                height = 140.0;
            }
            else if (!Visible(HarborLocation.Top) && Visible(HarborLocation.Bottom))
            {
                height = 140.0;
            }
            else if (Visible(HarborLocation.Top) && Visible(HarborLocation.Bottom))
            {
                // both! -- being explict
                height = 184.0;
            }
            else
            {
                this.Assert((!Visible(HarborLocation.Top) && !Visible(HarborLocation.Bottom)), "Error in bitchecking logic!");
            }

            return height;
        }

        public bool HasTopHarbor
        {
            get
            {
                return _dictionaryHarbors[HarborLocation.Top].Visibility == Visibility.Visible;
            }

        }
        public bool HasBottomHarbor
        {
            get
            {
                return _dictionaryHarbors[HarborLocation.Bottom].Visibility == Visibility.Visible;
            }

        }

        public bool HasLeftHarbor
        {
            get
            {
                return (_dictionaryHarbors[HarborLocation.BottomLeft].Visibility == Visibility.Visible ||
                        _dictionaryHarbors[HarborLocation.TopLeft].Visibility == Visibility.Visible);
            }
        }



        public bool HasRightHarbord
        {
            get
            {
                return (_dictionaryHarbors[HarborLocation.BottomRight].Visibility == Visibility.Visible ||
                        _dictionaryHarbors[HarborLocation.TopRight].Visibility == Visibility.Visible);
            }

        }

        public int HarborCount
        {
            get
            {
                int count = 0;
                foreach (var kvp in _startingTranslatePoints)
                {
                    if (kvp.Key.Visibility == Visibility.Visible)
                        count++;
                }
                return count;
            }
        }

        public int HexOrder { get; internal set; }
        

        public void UpdateHarborLocations()
        {
            this.Width = _normalWidth;
            this.Height = _normalHeight;

            ResourceTileGrid.HorizontalAlignment = GetHorizontalAlignment();
            ResourceTileGrid.VerticalAlignment = GetVerticalAlignment();
            this.Width = GetWidth();
            this.Height = GetHeight();




            //
            //  reset back to starting point
            foreach (var kvp in _startingTranslatePoints)
            {
                AdjustTransform(kvp.Key, kvp.Value.X, TransformAdjustOption.AdjustX, false);
                AdjustTransform(kvp.Key, kvp.Value.Y, TransformAdjustOption.AdjustY, false);
            }

            if (HarborCount > 1)
            {

                if (HasTopHarbor)
                {
                    AdjustTransform(_dictionaryHarbors[HarborLocation.BottomLeft], 44, TransformAdjustOption.AdjustY, true);
                    AdjustTransform(_dictionaryHarbors[HarborLocation.TopLeft], 44, TransformAdjustOption.AdjustY, true);
                    AdjustTransform(_dictionaryHarbors[HarborLocation.TopRight], 44, TransformAdjustOption.AdjustY, true);
                    AdjustTransform(_dictionaryHarbors[HarborLocation.BottomRight], 44, TransformAdjustOption.AdjustY, true);
                }

                if (HasLeftHarbor)
                {
                    AdjustTransform(_dictionaryHarbors[HarborLocation.Top], 12, TransformAdjustOption.AdjustX, true);
                    AdjustTransform(_dictionaryHarbors[HarborLocation.Bottom], 12, TransformAdjustOption.AdjustX, true);
                }

                if (HasRightHarbord)
                {
                    AdjustTransform(_dictionaryHarbors[HarborLocation.Top], -12, TransformAdjustOption.AdjustX, true);
                    AdjustTransform(_dictionaryHarbors[HarborLocation.Bottom], -12, TransformAdjustOption.AdjustX, true);
                }
            }

        }

        private void AdjustTransform(Harbor h, double value, TransformAdjustOption option, bool reletive)
        {
            CompositeTransform ct = (CompositeTransform)h.RenderTransform;
            

            if (option == TransformAdjustOption.AdjustX)
            {
                if (reletive)
                    ct.TranslateX += value;
                else
                    ct.TranslateX = value;
            }
            else
            {
                if (reletive)
                    ct.TranslateY += value;
                else
                    ct.TranslateY = value;
            }
        }

        private Harbor GetVisibleHarbor()
        {
            foreach (var h in _harbors)
            {
                if (h.Visibility == Visibility.Visible)
                {
                    return h;

                }

            }

            return null;
        }

       
    }
    public class HarborData
    {
        public HarborType HarborType { get; set; } = HarborType.None;
        public HarborLocation HarborLocation { get; set; } = HarborLocation.None;
        public Visibility Visibility { get; set; } = Visibility.Collapsed;

        public HarborData() { }
        public HarborData(HarborType type, HarborLocation location, Visibility vis)
        {
            HarborType = type;
            HarborLocation = location;
            Visibility = vis;
        }

        public override string ToString()
        {
            return String.Format($"Type={HarborType} Location={HarborLocation}");
        }


    }

}