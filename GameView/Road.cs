using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Shapes;

namespace Catan10
{
    public class RoadSegmentData
    {
        public RoadState RoadState { get; set; } = RoadState.Unowned;
        public Polygon RoadSegment { get; set; }
        public TileCtrl Tile { get; set; }
        public RoadLocation Location { get; set; }
      

        public RoadSegmentData(Polygon rs, TileCtrl tile, RoadLocation loc, RoadState state)
        {
            RoadSegment = rs;
            Tile = tile;
            Location = loc;
            RoadState = state;
        }
        public override string ToString()
        {
            return String.Format($"[{Tile.ToString()}:{Location}:{RoadState}]");
        }
    }

    //
    //  a class that contains all necessary data to draw a road and keep track of the color 
    //public class Road : IEquatable<Road>
    //{

    //    SolidColorBrush _brush = new SolidColorBrush(Colors.Black);
    //    SolidColorBrush _hightlightColor = new SolidColorBrush(Colors.Blue);
    //    RoadState _roadState = RoadState.Unowned;



    //    public RoadSegmentData RoadSegment1 { get; set; } = null;
    //    public RoadSegmentData RoadSegment2 { get; set; } = null;

    //    public List<Road> Heads { get; set; } = new List<Road>();
    //    public List<Road> Tails { get; set; } = new List<Road>();
    //    public List<RoadSegmentData> RoadSegments { get; } = new List<RoadSegmentData>();

    //    public List<Settlement> Settlements { get; } = new List<Settlement>();

    //    private bool AddHeadsorTails(Road road, bool heads)
    //    {
    //        if (road == null) return true;

    //        if (heads)
    //        {
    //            if (!Heads.Contains(road))
    //                Heads.Add(road);
    //        }
    //        else
    //        {
    //            if (!Tails.Contains(road))
    //                Tails.Add(road);
    //        }

    //        return true;
    //    }

    //    public Road() { Init(); }
    //    public Road(Polygon polyA, TileCtrl tileA, RoadLocation locA, TileCtrl tileB, Polygon polyB, RoadLocation locB, RoadState state = RoadState.Unowned)
    //    {
    //        RoadSegment1 = new RoadSegmentData(polyA, tileA, locA, state);
    //        if (tileB != null)
    //        {
    //            RoadSegment2 = new RoadSegmentData(polyB, tileB, locB, state);
    //        }

    //        Init();
    //    }

    //    private void Init()
    //    {
    //        RoadSegments.Add(RoadSegment1);
    //        RoadSegments.Add(RoadSegment2);

    //    }
    //    public override string ToString()
    //    {
    //        return String.Format($"IsOwned:{IsOwned} {RoadSegment1} {RoadSegment2}");
    //    }

    //    public RoadState RoadState
    //    {
    //        get
    //        {
    //            return _roadState;
    //        }
    //        set
    //        {
    //            _roadState = value;
    //            RoadSegment1.RoadState = value;
    //            if (RoadSegment2 != null)
    //                RoadSegment2.RoadState = value;
    //            //
    //            //  do not set the color to Transparent as that will make the build color wrong
    //            if (value == RoadState.Unowned)
    //                Show(false);
    //            else
    //                Show(true);

    //            this.Color = Color;

    //        }
    //    }

    //    public bool IsOwned
    //    {
    //        get
    //        {
    //            return (RoadState != RoadState.Unowned);
    //        }
    //    }

    //    private void UpdateColor(Brush fill, Brush highlight, int thickness)
    //    {
           
    //        RoadSegment1.RoadSegment.Fill = fill;           
    //        RoadSegment1.RoadSegment.Stroke = highlight;
    //        RoadSegment1.RoadSegment.StrokeThickness = thickness;

    //        if (RoadSegment2 != null)
    //        {
    //            RoadSegment2.RoadSegment.Fill = fill;
    //            RoadSegment2.RoadSegment.Stroke = highlight;
    //            RoadSegment2.RoadSegment.StrokeThickness = thickness;
    //        }

    //    }
        
    //    public Color Color
    //    {
    //        get
    //        {
    //            return _hightlightColor.Color;
    //        }
    //        set
    //        {
    //            if (_roadState == RoadState.Road || _roadState == RoadState.Unowned)
    //            {
    //                _brush = new SolidColorBrush(Colors.Black); // as in "black top"                                       
    //                _hightlightColor = new SolidColorBrush(value);
    //                UpdateColor(_brush, _hightlightColor, 3);
    //            }
    //            if (_roadState == RoadState.Ship)
    //            {
    //                BitmapImage bitmapImage = new BitmapImage(new Uri("ms-appx:///Assets/back.jpg", UriKind.RelativeOrAbsolute));
    //                ImageBrush brush = new ImageBrush();
    //                brush.AlignmentX = AlignmentX.Left;
    //                brush.AlignmentY = AlignmentY.Top;
    //                brush.Stretch = Stretch.UniformToFill;
    //                brush.ImageSource = bitmapImage;
    //                UpdateColor(brush, _hightlightColor, 3);
    //            }
                

    //        }
    //    }

    //    public int Number { get; set; } = -1;


    //    public void Show(bool show)
    //    {
    //        if (show)
    //        {
    //            RoadSegment1.RoadSegment.Opacity = 1.0;
    //            if (RoadSegment2 != null)
    //            {
    //                RoadSegment2.RoadSegment.Opacity = 1.0;
    //            }
    //        }
    //        else
    //        {
    //            RoadSegment1.RoadSegment.Opacity = 0.0;
    //            if (RoadSegment2 != null)
    //            {
    //                RoadSegment2.RoadSegment.Opacity = 0.0;
    //            }
    //        }
    //    }

    //    public List<Road> AdjacentRoads { get; set; } = new List<Road>();
    //    public void AddAjacentRoad(Road thatRoad)
    //    {
    //        if (thatRoad == null) return;
    //        if (!AdjacentRoads.Contains(thatRoad))
    //            AdjacentRoads.Add(thatRoad);
    //        if (AdjacentRoads.Count > 4)
    //            throw new InvalidOperationException();
    //    }



    //    public List<Road> OwnedAdjacentRoads
    //    {
    //        get
    //        {
    //            List<Road> list = new List<Road>();
    //            foreach (Road r in AdjacentRoads)
    //            {
    //                bool add = true;
    //                if (r.IsOwned && r.Color == this.Color)
    //                {
    //                    //
    //                    //   make sure that there isn't an apponent's settlement next to it
    //                    foreach (Settlement setlment in Settlements)
    //                    {
    //                        if (setlment.SettlementType != SettlementType.None && setlment.Color != this.Color)
    //                        {
    //                            // uh oh, settlement has blocked road chain...
    //                            add = false;
    //                            break;
    //                        }
    //                    }
    //                    if (add)
    //                        list.Add(r);
    //                }
    //            }

    //            return list;
    //        }
    //    }

    //    public List<Road> OwnedAdjacentRoadsNotMe
    //    {
    //        get
    //        {
    //            List<Road> list = new List<Road>();
    //            foreach (Road r in AdjacentRoads)
    //            {
    //                bool add = true;
    //                if (r.IsOwned && r.Color == this.Color && r != this)
    //                {
    //                    //
    //                    //   make sure that there isn't an apponent's settlement next to it
    //                    foreach (Settlement setlment in Settlements)
    //                    {
    //                        if (setlment.SettlementType != SettlementType.None && setlment.Color != this.Color)
    //                        {
    //                            // uh oh, settlement has blocked road chain...
    //                            add = false;
    //                            break;
    //                        }
    //                    }
    //                    if (add)
    //                        list.Add(r);
    //                }
    //            }
                

    //            return list;
    //        }
    //    }

    //    internal List<Road> OwnedAdjacentRoadsNotCounted(List<Road> owned, Road blockedFork, out bool adjacentFork)
    //    {
    //        List<Road> list = new List<Road>();
    //        foreach (Road r in AdjacentRoads)
    //        {
    //            if (r.IsOwned && r.Color == this.Color)
    //            {
    //                if (Settlements[0].Color != this.Color && Settlements[0].SettlementType != SettlementType.None)
    //                    continue;
    //                if (Settlements[1].Color != this.Color && Settlements[1].SettlementType != SettlementType.None)
    //                    continue;
    //                if (owned.Contains(r) == false)
    //                {
    //                    list.Add(r);
    //                }
    //            }
    //        }

    //        adjacentFork = false;
    //        if (list.Contains(blockedFork))
    //        {
    //            list.Remove(blockedFork);
    //            adjacentFork = true;
    //        }

    //        return list;
    //    }

    //    public bool Equals(Road other)
    //    {
    //        if (other == null) return false;

    //        if (this.ToString() == other.ToString())
    //            return true;

    //        return false;
    //    }


    //}
}
