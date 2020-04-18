using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;
using Windows.UI.Xaml.Media.Imaging;

namespace Catan10
{
   
    public class RoadCtrlOld
    {
        SolidColorBrush _brush = new SolidColorBrush(Colors.Black);
        SolidColorBrush _hightlightColor = new SolidColorBrush(Colors.Blue);
        RoadState _roadState = RoadState.Unowned;
        public List<SettlementCtrl> AdjacentSettlements { get; } = new List<SettlementCtrl>();
        public Polygon RoadSegment1 { get; set; } = null;
        public Polygon RoadSegment2 { get; set; } = null;
        public List<RoadKey> Keys { get; set; } = new List<RoadKey>();
        public IGameCallback Callback { get; internal set; }

        public PlayerView Owner { get; set; } = null;

        public int Number { get; internal set; } = 0; // number of roads that have been created for this player
        public List<RoadCtrl> AdjacentRoads { get; } = new List<RoadCtrl>();


        public RoadState RoadState
        {
            get
            {
                return _roadState;
            }
            set
            {
                _roadState = value;

                //
                //  do not set the color to Transparent as that will make the build color wrong
                if (value == RoadState.Unowned)
                    Show(false);
                else
                    Show(true);

                this.Color = Color;

            }
        }

        public bool IsOwned
        {
            get
            {
                return (RoadState != RoadState.Unowned);
            }
        }
        internal List<RoadCtrl> OwnedAdjacentRoadsNotCounted(List<RoadCtrl> owned, RoadCtrl blockedFork, out bool adjacentFork)
        {
            List<RoadCtrl> list = new List<RoadCtrl>();
            foreach (RoadCtrl r in AdjacentRoads)
            {
                if (r.IsOwned && r.Color == this.Color)
                {

                    if (r.AdjacentSettlements[0].Color != this.Color && r.AdjacentSettlements[0].SettlementType != SettlementType.None)
                        continue;
                    if (r.AdjacentSettlements[1].Color != this.Color && r.AdjacentSettlements[1].SettlementType != SettlementType.None)
                        continue;
                    if (owned.Contains(r) == false)
                    {
                        list.Add(r);
                    }
                }
            }

            adjacentFork = false;
            if (list.Contains(blockedFork))
            {
                list.Remove(blockedFork);
                adjacentFork = true;
            }

            return list;
        }

        public void Show(bool show, bool valid = true)
        {
            if (show)
            {
                double opacity = 1.0;
                if (!valid) opacity = 0.25;

                RoadSegment1.Opacity = opacity;
                if (RoadSegment2 != null)
                {
                    RoadSegment2.Opacity = opacity;
                }
            }
            else
            {
                RoadSegment1.Opacity = 0.0;
                if (RoadSegment2 != null)
                {
                    RoadSegment2.Opacity = 0.0;
                }
            }
        }

        private void UpdateColor(Brush fill, Brush highlight, int thickness)
        {

            RoadSegment1.Fill = fill;
            RoadSegment1.Stroke = highlight;
            RoadSegment1.StrokeThickness = thickness;

            if (RoadSegment2 != null)
            {
                RoadSegment2.Fill = fill;
                RoadSegment2.Stroke = highlight;
                RoadSegment2.StrokeThickness = thickness;
            }

        }

        public Color Color
        {
            get
            {
                return _hightlightColor.Color;
            }
            set
            {
                if (this._roadState == Catan10.RoadState.Road || this._roadState == Catan10.RoadState.Unowned)
                {
                    this._brush = new SolidColorBrush(Colors.Black); // as in "black top"                                       
                    this._hightlightColor = new SolidColorBrush(value);
                    this.UpdateColor((Brush)this._brush, (Brush)this._hightlightColor, (int)3);
                }
                if (_roadState == RoadState.Ship)
                {
                    BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/back.jpg", UriKind.RelativeOrAbsolute));
                    ImageBrush brush = new ImageBrush();
                    brush.AlignmentX = AlignmentX.Left;
                    brush.AlignmentY = AlignmentY.Top;
                    brush.Stretch = Stretch.UniformToFill;
                    brush.ImageSource = bitmapImage;
                    UpdateColor(brush, _hightlightColor, 3);
                }


            }
        }

        //
        //  looks for alignment on the Y axis
        public static bool PolygonClose(PointCollection pc1, PointCollection pc2, RoadLocation location)
        {
            double closeX = 30;
            if (location == RoadLocation.Bottom || location == RoadLocation.Top)
            {
                closeX = 30;
            }
            for (int i = 0; i < pc1.Count; i++)
            {
                // location.TraceMessage($"{Math.Abs(pc1[i].Y - pc2[i].Y)} {Math.Abs(pc1[i].X - pc2[i].X)}");
                if (Math.Abs(pc1[i].Y - pc2[i].Y) > closeX ||
                    Math.Abs(pc1[i].X - pc2[i].X) > closeX)
                {
                    return false;
                }
            }
            return true;
        }
        public static RoadCtrl GetCloneOrCreate(PointCollection pointCollection, RoadLocation location, TileCtrl tile, Grid home, List<RoadCtrl> roads, Dictionary<RoadKey, RoadCtrl> roadKeyDictionary)
        {

            //foreach (RoadCtrl road in roads)
            //{
            //    if (road.Keys.Count > 0)
            //    {
            //        if (location == RoadLocation.Top && road.Keys[0].RoadLocation == RoadLocation.Bottom)
            //        {
            //            //  road.TraceMessage("this one");

            //        }
            //    }


            //    if (RoadCtrl.PolygonClose(road.RoadSegment1.Points, pointCollection, location) == true)
            //    {
            //        //
            //        //  same Road -- add to the list of keys stored inside the road (its "clones")
            //        //  and add it to both the dictionary of keys --> roads that is passed in and to
            //        //  the list of all the roads. 

            //        road.RoadSegment2 = road.CreatePolygon(pointCollection, Colors.HotPink, location);
            //        RoadKey key = new RoadKey(tile, location);
            //        road.Keys.Add(key);
            //        roadKeyDictionary[key] = road;
            //        Canvas.SetZIndex(road.RoadSegment2, -1);
            //        home.Children.Add(road.RoadSegment2);
            //        return road;
            //    }
            //}

            ////
            ////  didn't find it -- i'm assuming it is a new road--every road has win or it is on the edge

            //RoadCtrl rd = null;
            //rd = new RoadCtrl();
            //rd.RoadSegment1 = rd.CreatePolygon(pointCollection, Colors.HotPink, location);
            //Canvas.SetZIndex(rd.RoadSegment1, -1);

            //roads.Add(rd);
            //home.Children.Add(rd.RoadSegment1);
            //RoadKey k = new RoadKey(tile, location);
            //rd.Keys.Add(k);
            //roadKeyDictionary[k] = rd;

            //return rd;
            return null;
        }

        public Polygon CreatePolygon(PointCollection pc, Color fill, RoadLocation location)
        {

            Polygon p = new Polygon();
            p.Stroke = new SolidColorBrush(Colors.Purple);
            p.StrokeThickness = 2;
            p.Points = pc;
            p.Fill = new SolidColorBrush(fill);
            p.Tag = location;
            p.Opacity = 0.0;
            p.PointerPressed += Road_PointerPressed;
            p.PointerEntered += Road_PointerEntered;
            p.PointerExited += Road_PointerExited;
            p.Visibility = Visibility.Visible;
            Canvas.SetZIndex(p, 10);
            return p;
        }

        private void Road_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            Callback?.RoadExited(null, e);
            //RoadSegment1.Opacity = 0.0;
            //if (RoadSegment2 != null)
            //{
            //    RoadSegment2.Opacity = 0.0;
            //}
        }

        private void Road_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            Callback?.RoadEntered(null, e);


            //RoadSegment1.Opacity = 1.0;
            //if (RoadSegment2 != null)
            //{
            //    RoadSegment2.Opacity = 1.0;
            //}


        }

        public override string ToString()
        {
            string s = "";
            foreach (RoadKey key in Keys)
            {
                s += string.Format($"\n{key.Tile} {key.RoadLocation} ");
            }

            return s + "\n";
        }

        private void Road_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.TraceMessage(this.ToString());
            Callback?.RoadPressed(null, e);
        }

        internal void Reset()
        {
            RoadState = RoadState.Unowned;
            Show(false);
        }
    }



}
