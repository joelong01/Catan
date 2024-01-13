using System;
using System.Collections.Generic;
using System.Linq;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Media;

namespace Catan10
{
    public partial class CatanHexPanel : Canvas
    {
        private Canvas HarborLayer { get; set; } = new Canvas();
        private Canvas RoadLayer { get; set; } = new Canvas();

        private readonly BaronCtrl _baron = new BaronCtrl();
        private TileCtrl _baronTile = null;

        private readonly MerchantCtrl _merchant = new MerchantCtrl();
        private  TileCtrl _merchantTile = null;

        private readonly List<TileCtrl> _desertTiles = new List<TileCtrl>();

        private readonly PirateShip _pirateShip = new PirateShip();

        private readonly List<TileGroup> _tileSets = new List<TileGroup>();

        private readonly List<List<TileCtrl>> _tilesInVisualLayout = new List<List<TileCtrl>>();

        private readonly Dictionary<int, int> BuildingIndexToHarborIndexDict = new Dictionary<int, int>();

        private readonly List<int> RowCounts = new List<int>();

        private bool _arrangeRoadsDone = false;

        private int _colCount = 0;

        //
        //  callbacks
        private IGameCallback _gameCallback = null;

        private double _normalHeight = 96;

        private double _normalWidth = 110;

        private TileCtrl _pirateTile = null;

        private ResourceType[] _resourceTypes = null;

        private ITileControlCallback _tileCallback = null;

        private int[] _tileNumbers = null;

        private TileCtrl[] _tilesInIndexOrder = null;

        private static void BaronTileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CatanHexPanel panel = d as CatanHexPanel;
            TileCtrl newTile = e.NewValue as TileCtrl;
            panel.SetBaronTile(newTile);
        }

        private static void BaronVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CatanHexPanel depPropClass = d as CatanHexPanel;
            Visibility depPropValue = (Visibility)e.NewValue;
            depPropClass.SetBaronVisibility(depPropValue);
        }

        private static void MerchantTileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CatanHexPanel panel = d as CatanHexPanel;
            TileCtrl newTile = e.NewValue as TileCtrl;
            panel.SetMerchantTile(newTile);
        }

        private static void MerchantVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CatanHexPanel depPropClass = d as CatanHexPanel;
            Visibility depPropValue = (Visibility)e.NewValue;
            depPropClass.SetMerchantVisibility(depPropValue);
        }

        private static void BuildingIndexToHarborIndexChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as CatanHexPanel;
            var depPropValue = (string)e.NewValue;
            depPropClass?.SetBuildingIndexToHarborIndex(depPropValue);
        }

        private static void GameTypeChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as CatanHexPanel;
            var depPropValue = (GameType)e.NewValue;
            depPropClass?.SetGameType(depPropValue);
        }

        private static void IslandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CatanHexPanel depPropClass = d as CatanHexPanel;
            string depPropValue = (string)e.NewValue;
            depPropClass.SetIslands(depPropValue);
        }

        private static void PirateTileChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CatanHexPanel panel = d as CatanHexPanel;
            TileCtrl newTile = e.NewValue as TileCtrl;
            panel.SetPirateTile(newTile);
        }

        private static void PirateVisibilityChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            CatanHexPanel depPropClass = d as CatanHexPanel;
            Visibility depPropValue = (Visibility)e.NewValue;
            depPropClass.SetPirateVisibility(depPropValue);
        }

        private static void TileCountChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as CatanHexPanel;
            var depPropValue = (int)e.NewValue;
            depPropClass?.SetTileCount(depPropValue);
        }

        private void AddAdjacentBuildings(BuildingCtrl currentBuilding, TileCtrl tile, BuildingLocation location)
        {
            if (tile != null)
            {
                BuildingKey key = new BuildingKey(tile, location);
                if (BuildingKeyToBuildingCtrlDictionary.TryGetValue(key, out BuildingCtrl adjacentBuilding))
                {
                    if (currentBuilding.AdjacentBuildings.Contains(adjacentBuilding) == false)
                    {
                        currentBuilding.AdjacentBuildings.Add(adjacentBuilding);
                    }
                }
            }
        }

        private bool AddAdjacentRoad(RoadCtrl currentRoad, TileCtrl tile, RoadLocation location)
        {
            if (tile != null)
            {
                RoadKey key = new RoadKey(tile, location);
                if (RoadKeyToRoadDictionary.TryGetValue(key, out RoadCtrl road))
                {
                    if (currentRoad.AdjacentRoads.Contains(road) == false)
                    {
                        currentRoad.AdjacentRoads.Add(road);
                    }
                }

                return true;
            }

            return false;
        }

        private bool AddBuildingAdjacentRoad(BuildingCtrl building, TileCtrl tile, RoadLocation location)
        {
            if (tile == null)
            {
                return false;
            }

            RoadKey key = new RoadKey(tile, location);
            RoadCtrl road = RoadKeyToRoadDictionary[key];
            if (building.AdjacentRoads.Contains(road, new RoadCloneComparer()) == false)
            {
                building.AdjacentRoads.Add(road);
            }
            return true;
        }

        private void ArrangeBuildings()
        {
            if (Columns == 0)
            {
                return;
            }

            if (VisualTiles.Count == 0)
            {
                return;
            }

            int count = 0;
            for (int col = 0; col < this.Columns; col++)
            {
                for (int row = 0; row < VisualTiles.ElementAt(col).Count; row++)
                {
                    TileCtrl tile = VisualTiles.ElementAt(col).ElementAt(row);
                    GeneralTransform gt = tile.Visual.TransformToVisual(TopLayer);

                    Point point = new Point();
                    foreach (BuildingLocation location in Enum.GetValues(typeof(BuildingLocation)))
                    {
                        switch (location)
                        {
                            case BuildingLocation.TopRight:
                                point.X = 81;
                                point.Y = 3;
                                break;

                            case BuildingLocation.MiddleRight:
                                point.X = 106.5;
                                point.Y = 48;
                                break;

                            case BuildingLocation.BottomRight:
                                point.X = 81;
                                point.Y = 93;
                                break;

                            case BuildingLocation.BottomLeft:
                                point.X = 29;
                                point.Y = 93;
                                break;

                            case BuildingLocation.MiddleLeft:
                                point.X = 3.5;
                                point.Y = 48;
                                break;

                            case BuildingLocation.TopLeft:
                                point.X = 29;
                                point.Y = 3;
                                break;

                            case BuildingLocation.None:
                                continue;
                            default:
                                continue;
                        }

                        point = gt.TransformPoint(point);

                        //
                        //  tag the Building with the tile and the position

                        bool cloned = FindBuildingAtVisualLocation(point, Buildings, location, tile, out BuildingCtrl building);
                        if (!cloned && count < Buildings.Count)
                        {
                            building = Buildings[count];
                            building.LayoutPoint = point;
                            building.Transform.TranslateX = point.X - .5 * building.Width;
                            building.Transform.TranslateY = point.Y - .5 * building.Height;

                            //
                            //  add to dictionary...
                            if (!building.BuildingToTileDictionary.ContainsKey(location))
                            {
                                building.BuildingToTileDictionary[location] = tile;
                            }
                            else
                            {
                                // this.TraceMessage($"{tile} @ {location} already in map!");
                            }
                            building.AddKey(tile, location);
                            // setl2.BuildingCtrl.Show(BuildingState.City);
                            count++;
                        }

                        BuildingKey key = new BuildingKey(tile, location);
                        BuildingKeyToBuildingCtrlDictionary[key] = building;

                        if (count == Buildings.Count)
                        {
                        }
                    }
                }
            }

            //foreach (var kvp in BuildingKeyToBuildingCtrlDictionary)
            //{
            //    Debug.Write($"{kvp.Key}: {kvp.Value.LayoutPoint}\t");
            //    foreach (var clone in kvp.Value.Clones)
            //    {
            //        Debug.Write($"{clone}");
            //    }
            //    Debug.WriteLine("");
            //}
        }

        private void BuildChildList()
        {
            if (_tilesInVisualLayout.Count != 0)
            {
                return;
            }

            //
            //  first bucket the controls into their various collections
            //
            //  the order they are entered in the XAML file will determine the order they are here
            List<Harbor> harbors = this.Harbors;
            for (int i = Children.Count - 1; i >= 0; i--)
            {
                UIElement element = Children[i];
                if (element.GetType() == typeof(TileCtrl))
                {
                    Tiles.Insert(0, element as TileCtrl);
                    if (StaticHelpers.IsInVisualStudioDesignMode)
                    {
                        ( ( TileCtrl )element ).ShowIndex = true;
                    }
                }

                if (element.GetType() == typeof(Harbor))
                {
                    harbors.Add(element as Harbor); // we don't care the order these are in
                    this.Children.RemoveAt(i);
                    HarborLayer.Children.Add(element);
                    Canvas.SetZIndex(element, 10);
                }
            }

            if (HarborLayer.Children.Count == 0)
            {
                foreach (Harbor h in harbors)
                {
                    HarborLayer.Children.Add(h);
                }
            }

            //
            //   put the tiles into the row/col data structure

            if (RowCounts.Count == 0)
            {
                return;
            }

            int tileCount = 0;
            int middleCol = _colCount / 2;
            for (int col = 0; col < RowCounts.Count(); col++)
            {
                if (tileCount == Tiles.Count) // might have forgot to add all the tiles to layout...
                {
                    break;
                }

                List<TileCtrl> innerList = new List<TileCtrl>();
                for (int row = 0; row < RowCounts[col]; row++)
                {
                    if (tileCount == Tiles.Count)
                    {
                        break;
                    }

                    innerList.Add(Tiles[tileCount] as TileCtrl);
                    Tiles[tileCount].Row = row;
                    Tiles[tileCount].Col = col;

                    tileCount++;
                }
                _tilesInVisualLayout.Add(innerList);
            }

            if (Islands != "")
            {
                SetIslands(Islands);
            }
        }

        private void BuildDataLists()
        {
            try
            {
                if (_tilesInIndexOrder == null || _tilesInIndexOrder.Count() == 0)
                {
                    _tilesInIndexOrder = new TileCtrl[Tiles.Count];
                    _resourceTypes = new ResourceType[Tiles.Count];
                    _tileNumbers = new int[Tiles.Count];
                    foreach (TileCtrl tile in Tiles)
                    {
                        _tilesInIndexOrder[tile.Index] = tile;
                        _resourceTypes[tile.Index] = tile.ResourceType;
                        _tileNumbers[tile.Index] = tile.Number;
                    }

                    StartingHarborTypes = new HarborType[Harbors.Count];
                    for (int i = 0; i < Harbors.Count; i++)
                    {
                        StartingHarborTypes[i] = Harbors[i].HarborType;
                    }
                }
            }
            catch
            {
            }
        }

        private void CreateBuildings()
        {
            if (_colCount == 0)
            {
                return;
            }

            if (VisualTiles.Count == 0)
            {
                return;
            }

            if (Buildings.Count > 0)
            {
                // this.TraceMessage("returning from CreateBuildings because there are already Buildings here.");
                return;
            }

            int count = 0;
            int middleCol = _colCount / 2;

            count = VisualTiles[0].Count; // first edges
            for (int i = 0; i < middleCol; i++)
            {
                count += 2 * ( VisualTiles[i].Count + 1 );
            }

            count += VisualTiles[middleCol].Count + 1;
            count *= 2;

            Harbors.Sort((h1, h2) => h1.Index - h2.Index);

            for (int i = 0; i < count; i++)
            {
                BuildingCtrl building = new BuildingCtrl();
                if (StaticHelpers.IsInVisualStudioDesignMode)
                {
                    building.Opacity = 0.5;
                }

                Binding binding = new Binding()
                {
                    Path = new PropertyPath("CurrentPlayer"),
                    Mode = BindingMode.OneWay,
                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                    Source = MainPage.Current
                };

                building.SetBinding(BuildingCtrl.CurrentPlayerProperty, binding);

                building.Index = Buildings.Count;
                Buildings.Add(building);

                if (BuildingIndexToHarborIndexDict.TryGetValue(building.Index, out int harborIndex))
                {
                    //
                    //  Note below the assumption that Harbors is in the same order as its index
                    //  hence the .Sort above
                    building.AdjacentHarbor = Harbors[harborIndex];
                }

                building.Callback = _gameCallback;

                TopLayer.Children.Add(building);
            }
        }

        //
        //  we have here an n! algorythm where we keep a list of all of the building and then when we add one, we check to see if there is another "close" to the one
        //  we are trying to add. if they are close, we add it to the Clone map and return "true".  if it isn't a Clone, we return false
        //
        //  this makes it so that one building can "connect" to multiple tiles.
        //
        private bool FindBuildingAtVisualLocation(Point p, List<BuildingCtrl> buildings, BuildingLocation location, TileCtrl tile, out BuildingCtrl clone)
        {
            //  this.TraceMessage($"looking for a clone for {tile} at {location}");
            foreach (BuildingCtrl s in buildings)
            {
                //  this.TraceMessage($"\t\tChecking {s.Clones[0].Tile} at {s.Clones[0].Location} DelatX: {Math.Abs(s.LayoutPoint.X - p.X)} DeltaY: {Math.Abs(s.LayoutPoint.Y - p.Y)}");
                if (Math.Abs(s.LayoutPoint.X - p.X) < 20 && Math.Abs(s.LayoutPoint.Y - p.Y) < 20)
                {
                    //  this.TraceMessage($"found clone of {s} from {s.LayoutPoint} at {p} deltaX={Math.Abs(s.LayoutPoint.X - p.X)} deltaY={Math.Abs(s.LayoutPoint.Y - p.Y)}");
                    if (!s.BuildingToTileDictionary.ContainsKey(location))
                    {
                        s.BuildingToTileDictionary[location] = tile;
                    }
                    else
                    {
                        //this.TraceMessage($"{tile} @ {location} already in map!");
                    }
                    clone = s;
                    s.AddKey(tile, location);
                    return true;
                }
            }
            clone = null;
            return false;
        }

        private BuildingCtrl GetBuildingAt(TileCtrl tile, BuildingLocation location)
        {
            BuildingKey key = new BuildingKey(tile, location);
            return BuildingKeyToBuildingCtrlDictionary[key];
        }

        //
        //  given a current tile and road location, return the Road that sits in the same place visually
        private RoadCtrl GetCloneRoad(int row, int col, RoadLocation location)
        {
            TileCtrl cloneTile = null;
            RoadLocation cloneLoc = RoadLocation.None;
            switch (location)
            {
                case RoadLocation.None:
                    cloneLoc = RoadLocation.None;
                    break;

                case RoadLocation.Top:
                    cloneTile = GetAdjacentTile(row, col, TileLocation.Top);
                    cloneLoc = RoadLocation.Bottom;
                    break;

                case RoadLocation.TopRight:
                    cloneTile = GetAdjacentTile(row, col, TileLocation.TopRight);
                    cloneLoc = RoadLocation.BottomLeft;
                    break;

                case RoadLocation.BottomRight:
                    cloneTile = GetAdjacentTile(row, col, TileLocation.BottomRight);
                    cloneLoc = RoadLocation.TopLeft;
                    break;

                case RoadLocation.Bottom:
                    cloneTile = GetAdjacentTile(row, col, TileLocation.Bottom);
                    cloneLoc = RoadLocation.Top;
                    break;

                case RoadLocation.BottomLeft:
                    cloneTile = GetAdjacentTile(row, col, TileLocation.BottomLeft);
                    cloneLoc = RoadLocation.TopRight;
                    break;

                case RoadLocation.TopLeft:
                    cloneTile = GetAdjacentTile(row, col, TileLocation.TopLeft);
                    cloneLoc = RoadLocation.BottomRight;
                    break;

                default:
                    break;
            }

            if (cloneLoc == RoadLocation.None || cloneTile == null)
            {
                return null;
            }

            RoadKeyToRoadDictionary.TryGetValue(new RoadKey(cloneTile, cloneLoc), out RoadCtrl cloanRoad);

            return cloanRoad; // may be null
        }

        private TileCtrl GetTileAt(int col, int row)
        {
            if (col < VisualTiles.Count && col >= 0)
            {
                if (row < VisualTiles[col].Count && row >= 0)
                {
                    return VisualTiles.ElementAt(col).ElementAt(row);
                }
            }

            return null;
        }

        private void MoveHarbors()
        {
            if (HarborLayoutDataDictionary.Count == 0)
            {
                SetupHarborData();
            }

            Size desiredHarborSize = new Size(50, 50);

            foreach (Harbor harbor in Harbors)
            {
                harbor.Arrange(new Rect(0, 0, desiredHarborSize.Width, desiredHarborSize.Height));
                TileCtrl tile = TilesInIndexOrder[harbor.TileIndex];
                HarborLayoutData data = HarborLayoutDataDictionary[harbor.HarborLocation];
                GeneralTransform gtToSource = tile.TransformToVisual(HarborLayer);
                Point to = gtToSource.TransformPoint(data.Anchor);
                CompositeTransform transform = harbor.RenderTransform as CompositeTransform;
                harbor.RenderTransformOrigin = data.RenderTransformOrigin;
                transform.Rotation = data.Rotation;
                harbor.RotateImage = -transform.Rotation;
                transform.TranslateX = to.X;
                transform.TranslateY = to.Y;
                transform.ScaleX = data.Scale;
                transform.ScaleY = data.Scale;
                harbor.Visibility = Visibility.Visible;
            }
        }

        private int RoadCount()
        {
            //
            //  first calculate the perimeter

            int total = 12; // these are the edges of the 4 corners

            //
            //  next is the edges of the fist and last colums (e.g. the TopLeft, BottomLeft of the first column and the corresponding ones on the last column
            total += ( RowCounts[0] - 2 ) * 4;

            //
            //  next the TopRight/Top, Top/TopLeft, Bottom/BottomRight, and Bottom/BottomLeft pairs around the board.
            //  these are the top and bottom of the columns between the first and middle and between the middle and last column
            int middleCol = _colCount / 2;
            total += ( middleCol - 1 ) * 8;

            // at this point, Total == count of roads on the perimiter...now look for the roads in the middle -- they all overlap

            return 0;
        }

        private void SetBaronTile(TileCtrl tile)
        {
            if (_baronTile != null)
            {
                _baronTile.HasBaron = false;
            }

            if (tile == null)
            {
                _baronTile.Visibility = Visibility.Collapsed;
                return;
            }

            tile.HasBaron = true;
            _baronTile = tile;
            GeneralTransform gt = tile.TransformToVisual(HarborLayer);

            Point to = new Point((tile.Width - _baron.Width) * .5, (tile.Height - _baron.Height - tile.HexThickness));
            to = gt.TransformPoint(to);
            _baron.Visibility = Visibility.Visible;
            _baron.MoveAsync(to);
        }

        private void SetMerchantTile(TileCtrl tile)
        {

            if (tile == null)
            {
                HideMerchant();
                return;
            }

            _merchantTile = tile;
            GeneralTransform gt = tile.TransformToVisual(HarborLayer);

            Point to = new Point((tile.Width - _merchant.Width) * .5, (tile.Height - _merchant.Height - tile.HexThickness));
            to = gt.TransformPoint(to);
            ShowMerchant();
            _merchant.MoveAsync(to);
        }

        private void SetMerchantVisibility(Visibility visibility)
        {
            _merchant.Visibility = visibility;
        }
        private void SetBaronVisibility(Visibility visibility)
        {
            _baron.Visibility = visibility;
        }

        private void SetBuildingIndexToHarborIndex(string value)
        {
            if (value == null)
            {
                return;
            }

            if (value == "")
            {
                return;
            }

            //
            //  parse this into the map of building index => Harbor Index
            //  form: "4=0,5=0,8=1,36=2,37=2,47=3,48=3,50=4,51=4,45=5,46=5,26=6,38=6,14=7,17=7,11=8,24=8"
            //          buildingIndex=HarborIndex,

            string[] tokens = value.Split(new char[] { ',', '=' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Length; i += 2)
            {
                BuildingIndexToHarborIndexDict[Int32.Parse(tokens[i])] = Int32.Parse(tokens[i + 1]);
            }
        }

        private void SetGameType(GameType value)
        {
        }

        private void SetIslands(string islands)
        {
            if (Tiles.Count == 0)
            {
                return;
            }

            string[] tokens = islands.Split(new char[] { '-', ' ', ',' }, StringSplitOptions.RemoveEmptyEntries);
            for (int i = 0; i < tokens.Count(); i += 2)
            {
                Island island = new Island
                {
                    Start = int.Parse(tokens[i]),
                    End = int.Parse(tokens[i + 1])
                };
                island.BonusPoint = ( island.Start != 0 ); // no points for the default map
                for (int count = island.Start; count <= island.End; count++)
                {
                    TileToIslandDictionary[TilesInIndexOrder[count]] = island;
                }
            }
        }

        private void SetPirateTile(TileCtrl tile)
        {
            if (_pirateTile != null)
            {
                _pirateTile.HasPirateShip = false;
            }

            if (tile == null)
            {
                _pirateShip.Visibility = Visibility.Collapsed;
                return;
            }

            tile.HasPirateShip = true;
            _pirateTile = tile;
            GeneralTransform gt = tile.TransformToVisual(HarborLayer);

            Point to = new Point((tile.Width - _pirateShip.Width) * .5, (tile.Height - _pirateShip.Height) * .5 - tile.HexThickness); // Horizontal = center and Vertical = bottom
            to = gt.TransformPoint(to);
            _pirateShip.Visibility = Visibility.Visible;
            _pirateShip.MoveAsync(to);
        }

        private void SetPirateVisibility(Visibility visibility)
        {
            _pirateShip.Visibility = Visibility;
        }

        private void SetTileCount(int value)
        {
        }

        private void SetupHarborData()
        {
            //
            //  can't do this w/o knowing the Width, so this is called from the Width.set property
            if (NormalWidth == 0)
            {
                return;
            }

            HarborLayoutDataDictionary.Clear();

            double side = NormalWidth * 0.5;
            double sinLen = Math.Round(Math.Sin(Math.PI / 3) * side, 5);

            //
            //  these are our anchor points in the local coordinates of a tile
            Point middleLeft = new Point(0, sinLen);
            Point topLeft = new Point(side * .5, 0);
            Point topRight = new Point(1.5 * side, 0);
            Point middleRight = new Point(2 * side, sinLen);
            Point bottomRight = new Point(1.5 * side, 2 * sinLen);
            Point bottomLeft = new Point(.5 * side, 2 * sinLen);

            HarborLayoutData.AddLocation(HarborLocation.Top, new Point(0.5, 0.5), 90, 1.0, new Point(topLeft.X + 2, topLeft.Y - HarborLayoutData.Height), HarborLayoutDataDictionary);
            HarborLayoutData.AddLocation(HarborLocation.TopRight, new Point(1, 1), 150, 1.0, new Point(topRight.X + 1 - HarborLayoutData.Width, topRight.Y - HarborLayoutData.Height + 2), HarborLayoutDataDictionary);
            HarborLayoutData.AddLocation(HarborLocation.BottomRight, new Point(1, 1), 209.5, 1.0, new Point(middleRight.X - 1 - HarborLayoutData.Width, middleRight.Y - HarborLayoutData.Height + 2), HarborLayoutDataDictionary);
            HarborLayoutData.AddLocation(HarborLocation.Bottom, new Point(0.5, 0.5), 270, 1.0, new Point(bottomLeft.X + 2.5, bottomLeft.Y + .5), HarborLayoutDataDictionary);
            HarborLayoutData.AddLocation(HarborLocation.BottomLeft, new Point(1, 0), 330.5, 1.0, new Point(middleLeft.X + 1.5 - HarborLayoutData.Width, middleLeft.Y + 3), HarborLayoutDataDictionary);
            HarborLayoutData.AddLocation(HarborLocation.TopLeft, new Point(1, 1), 29.5, 1.0, new Point(middleLeft.X + 1.5 - HarborLayoutData.Width, middleLeft.Y - HarborLayoutData.Height - 2.5), HarborLayoutDataDictionary);
        }

        private void SimpleHexPanel_SizeChanged(object sender, SizeChangedEventArgs e)
        {
            HarborLayer.Width = e.NewSize.Width;
            HarborLayer.Height = e.NewSize.Height;
            RoadLayer.Width = e.NewSize.Width;
            RoadLayer.Height = e.NewSize.Height;
        }

        // Arrange the child elements to their final position
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (DisableLayout || ( RowCounts.Count == 0 ))
            {
                return finalSize;
            }

            Size size;

            size = Update(finalSize);

            return size;
        }

        /// <summary>
        ///     Fit the Tiles into the right size
        ///     ther are also Harbors, Roads, and Buildings in the panel, but none of these take up space
        ///     as they are implemented in their own layer
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (DisableLayout || ( RowCounts.Count == 0 ) || NormalHeight == 0 || NormalWidth == 0)
            {
                return availableSize;
            }

            Size maxSize = new Size();
            int middleCol = _colCount / 2;

            foreach (UIElement child in Children)
            {
                child.Measure(availableSize);
            }

            BuildChildList();
            //
            //  Build the roads and buildings on the TopLayer
            CreateBuildings();

            maxSize.Width = ( _normalWidth + TileGap ) * ( _colCount - 1 ) + UniformMargin.Left + UniformMargin.Right;

            maxSize.Height = ( _normalHeight + TileGap ) * RowCounts[middleCol] + UniformMargin.Top + UniformMargin.Bottom;

            return maxSize;
        }

        internal void Reset()
        {
            foreach (TileCtrl tile in Tiles)
            {
                tile.Reset();
                if (tile.ResourceType != ResourceType.Sea)
                {
                    // tile.TileOrientation = TileOrientation.FaceUp;
                    tile.TileOrientation = TileOrientation.FaceDown;
                }
            }
            foreach (RoadCtrl road in Roads)
            {
                road.Reset();
            }

            foreach (BuildingCtrl s in Buildings)
            {
                s.Reset();
            }

            foreach (Harbor h in Harbors)
            {
                h.Reset();
                h.SetOrientationAsync(TileOrientation.FaceUp);
                h.SetOrientationAsync(TileOrientation.FaceDown);
            }
        }

        public bool AllowShips
        {
            get => ( bool )GetValue(AllowShipsProperty);
            set => SetValue(AllowShipsProperty, value);
        }

        public TileCtrl BaronTile
        {
            get => ( TileCtrl )GetValue(BaronTileProperty);
            set => SetValue(BaronTileProperty, value);
        }

        public Visibility BaronVisibility
        {
            get => ( Visibility )GetValue(BaronVisibilityProperty);
            set
            {   //
                // 7/23/202: the changed property handler isn't being called...don't know why.
                //           instead of debugging it, will set it directly here.

                SetValue(BaronVisibilityProperty, value);
                _baron.Visibility = value;
            }
        }

        public string BuildingIndexToHarborIndex
        {
            get => ( string )GetValue(BuildingIndexToHarborIndexProperty);
            set => SetValue(BuildingIndexToHarborIndexProperty, value);
        }

        //
        //   UI Elements
        public List<BuildingCtrl> Buildings { get; } = new List<BuildingCtrl>();

        public CatanGames CatanGame
        {
            get => ( CatanGames )GetValue(CatanGameProperty);
            set => SetValue(CatanGameProperty, value);
        }

        public int Columns => RowCounts.Count();

        public string Description
        {
            get => ( string )GetValue(DescriptionProperty);
            set => SetValue(DescriptionProperty, value);
        }

        // book keeping
        //
        public int DesertCount => _desertTiles.Count;

        public List<TileCtrl> DesertTiles => _desertTiles;

        public List<DevCardType> DevCards
        {
            get => ( List<DevCardType> )GetValue(DevCardsProperty);
            set => SetValue(DevCardsProperty, value);
        }

        public bool DisableLayout
        {
            get => ( bool )GetValue(DisableLayoutProperty);
            set => SetValue(DisableLayoutProperty, value);
        }

        public IGameCallback GameCallback
        {
            get => _gameCallback;
            set
            {
                _gameCallback = value;

                foreach (RoadCtrl road in Roads)
                {
                    road.Callback = _gameCallback;
                }

                foreach (BuildingCtrl s in Buildings)
                {
                    s.Callback = _gameCallback;
                }
            }
        }

        public CatanGames GameName
        {
            get => ( CatanGames )GetValue(GameNameProperty);
            set => SetValue(GameNameProperty, value);
        }

        public GameType GameType
        {
            get => ( GameType )GetValue(GameTypeProperty);
            set => SetValue(GameTypeProperty, value);
        }

        public int HarborCount
        {
            get => ( int )GetValue(HarborCountProperty);
            set => SetValue(HarborCountProperty, value);
        }

        public List<Harbor> Harbors { get; } = new List<Harbor>();
        public bool HasIslands => TileToIslandDictionary.Keys.Count > 1;

        public string Islands
        {
            get => ( string )GetValue(IslandsProperty);
            set => SetValue(IslandsProperty, value);
        }

        public int Knights
        {
            get => ( int )GetValue(KnightsProperty);
            set => SetValue(KnightsProperty, value);
        }

        public int MaxCities
        {
            get => ( int )GetValue(MaxCitiesProperty);
            set => SetValue(MaxCitiesProperty, value);
        }

        public int MaxResourceAllocated
        {
            get => ( int )GetValue(MaxResourceAllocatedProperty);
            set => SetValue(MaxResourceAllocatedProperty, value);
        }

        public int MaxRoads
        {
            get => ( int )GetValue(MaxRoadsProperty);
            set => SetValue(MaxRoadsProperty, value);
        }

        public int MaxKnights
        {
            get => ( int )GetValue(MaxKnightsProperty);
            set => SetValue(MaxKnightsProperty, value);
        }

        public int MaxSettlements
        {
            get => ( int )GetValue(MaxSettlementsProperty);
            set => SetValue(MaxSettlementsProperty, value);
        }

        public int MaxShips
        {
            get => ( int )GetValue(MaxShipsProperty);
            set => SetValue(MaxShipsProperty, value);
        }

        public int Monopoly
        {
            get => ( int )GetValue(MonopolyProperty);
            set => SetValue(MonopolyProperty, value);
        }

        public double NormalHeight
        {
            get => _normalHeight;

            set => _normalHeight = value;
        }

        public double NormalWidth
        {
            get => _normalWidth;

            set
            {
                _normalWidth = value;
                SetupHarborData();
            }
        }

        public TileCtrl PirateShipTile
        {
            get => ( TileCtrl )GetValue(PirateShipTileProperty);
            set => SetValue(PirateShipTileProperty, value);
        }

        public Visibility PirateVisibility
        {
            get => ( Visibility )GetValue(PirateVisibilityProperty);
            set => SetValue(PirateVisibilityProperty, value);
        }

        public ResourceType[] ResourceTypes
        {
            get
            {
                BuildDataLists();
                return _resourceTypes;
            }
        }

        public int RoadBuilding
        {
            get => ( int )GetValue(RoadBuildingProperty);
            set => SetValue(RoadBuildingProperty, value);
        }

        //
        //  ways to look Road/Buildings/Tiles up
        public Dictionary<RoadKey, RoadCtrl> RoadKeyToRoadDictionary { get; } = new Dictionary<RoadKey, RoadCtrl>(new RoadKeyComparer());

        public List<RoadCtrl> Roads { get; } = new List<RoadCtrl>();

        public string RowsPerColumn
        {
            get
            {
                string s = "";
                foreach (int i in RowCounts)
                {
                    s += string.Format($"{i},");
                }

                return s;
            }
            set
            {
                try
                {
                    if (value == "")
                    {
                        return;
                    }

                    RowCounts.Clear();

                    string[] tokens = value.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
                    foreach (string s in tokens)
                    {
                        RowCounts.Add(int.Parse(s));
                    }

                    _colCount = RowCounts.Count();
                }
                catch { }
            }
        }

        public HarborType[] StartingHarborTypes { get; private set; } = null;

        public ITileControlCallback TileCallback
        {
            get => _tileCallback;
            set
            {
                _tileCallback = value;
                foreach (TileCtrl tile in Tiles)
                {
                    tile.SetTileCallback(value);
                }
            }
        }

        public double TileGap
        {
            get => ( double )GetValue(TileGapProperty);
            set => SetValue(TileGapProperty, value);
        }

        public string TileGroups
        {
            get => ( string )GetValue(TileGroupsProperty);
            set => SetValue(TileGroupsProperty, value);
        }

        public int[] TileNumbers
        {
            get
            {
                BuildDataLists();
                return _tileNumbers;
            }
        }

        public List<TileCtrl> Tiles { get; } = new List<TileCtrl>();

        //
        //  this is where be build up all the state needed in a TileGroup list
        //  try to iterate over the tiles only once...
        public List<TileGroup> TileSets
        {
            get
            {
                if (_tileSets.Count == 0)
                {
                    if (TileGroups != "")
                    {
                        string[] values = TileGroups.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                        foreach (string val in values)
                        {
                            TileGroup tg = new TileGroup();
                            string[] tokens = val.Split(new char[] { '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
                            tg.Start = int.Parse(tokens[0]);
                            tg.End = int.Parse(tokens[1]);
                            tg.Randomize = bool.Parse(tokens[2]);
                            for (int i = tg.Start; i <= tg.End; i++)
                            {
                                TileCtrl tile = TilesInIndexOrder[i];
                                tg.AllTiles.Add(tile);
                                tg.StartingResourceTypes.Add(tile.ResourceType);
                                tg.StartingTileNumbers.Add(tile.Number);
                                if (tile.ResourceType == ResourceType.Desert)
                                {
                                    tg.DesertCount++;
                                }

                                if (tile.ResourceType != ResourceType.Sea)
                                {
                                    tg.TilesToRandomize.Add(tile);
                                    tg.OriginalNonSeaTiles.Add(tile);
                                }
                                //
                                //  Bind to CurrentPlayer
                                Binding binding = new Binding()
                                {
                                    Path = new PropertyPath("CurrentPlayer"),
                                    Mode = BindingMode.OneWay,
                                    UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                                    Source = MainPage.Current
                                };

                                tile.SetBinding(BuildingCtrl.CurrentPlayerProperty, binding);
                            }
                            _tileSets.Add(tg);
                        }
                    }
                    else
                    {
                        TileGroup tg = new TileGroup
                        {
                            Start = 0,
                            End = Tiles.Count() - 1,
                            Randomize = true
                        };
                        tg.AllTiles.AddRange(Tiles);
                        tg.TilesToRandomize.AddRange(Tiles);
                        tg.OriginalNonSeaTiles.AddRange(Tiles);
                        tg.StartingResourceTypes.AddRange(_resourceTypes);
                        tg.StartingTileNumbers.AddRange(_tileNumbers);
                        _tileSets.Add(tg);
                    }
                }
                return _tileSets;
            }
        }

        public TileCtrl[] TilesInIndexOrder
        {
            get
            {
                BuildChildList();
                BuildDataLists();
                return _tilesInIndexOrder;
            }
        }

        public Dictionary<TileCtrl, Island> TileToIslandDictionary { get; set; } = new Dictionary<TileCtrl, Island>();

        //
        //  layers
        public Grid TopLayer { get; set; } = new Grid();

        public Thickness UniformMargin
        {
            get => ( Thickness )GetValue(UniformMarginProperty);
            set => SetValue(UniformMarginProperty, value);
        }

        public int VictoryPoints
        {
            get => ( int )GetValue(VictoryPointsProperty);
            set => SetValue(VictoryPointsProperty, value);
        }

        public List<List<TileCtrl>> VisualTiles => _tilesInVisualLayout;

        public int YearOfPlenty
        {
            get => ( int )GetValue(YearOfPlentyProperty);
            set => SetValue(YearOfPlentyProperty, value);
        }

        public static readonly DependencyProperty AllowShipsProperty = DependencyProperty.Register("AllowShips", typeof(bool), typeof(CatanHexPanel), new PropertyMetadata(false));
        public static readonly DependencyProperty BaronTileProperty = DependencyProperty.Register("BaronTile", typeof(TileCtrl), typeof(CatanHexPanel), new PropertyMetadata(null, BaronTileChanged));
        public static readonly DependencyProperty MerchantTileProperty = DependencyProperty.Register("MerchantTile", typeof(TileCtrl), typeof(CatanHexPanel), new PropertyMetadata(null, MerchantTileChanged));

        public static readonly DependencyProperty BaronVisibilityProperty = DependencyProperty.Register("BaronVisibility", typeof(Visibility), typeof(CatanHexPanel), new PropertyMetadata(Visibility.Collapsed, BaronVisibilityChanged));
        public static readonly DependencyProperty MerchantVisibilityProperty = DependencyProperty.Register("MerchantVisibility", typeof(Visibility), typeof(CatanHexPanel), new PropertyMetadata(Visibility.Collapsed, MerchantVisibilityChanged));

        public static readonly DependencyProperty BuildingIndexToHarborIndexProperty = DependencyProperty.Register("BuildingIndexToHarborIndex", typeof(string), typeof(CatanHexPanel), new PropertyMetadata(null, BuildingIndexToHarborIndexChanged));
        public static readonly DependencyProperty CatanGameProperty = DependencyProperty.Register("CatanGame", typeof(CatanGames), typeof(CatanHexPanel), new PropertyMetadata(CatanGames.Regular));
        public static readonly DependencyProperty DescriptionProperty = DependencyProperty.Register("Description", typeof(string), typeof(CatanHexPanel), new PropertyMetadata(""));
        public static readonly DependencyProperty DevCardsProperty = DependencyProperty.Register("DevCards", typeof(List<DevCardType>), typeof(CatanHexPanel), new PropertyMetadata(new List<DevCardType>()));
        public static readonly DependencyProperty DisableLayoutProperty = DependencyProperty.Register("DisableLayout", typeof(bool), typeof(CatanHexPanel), new PropertyMetadata(false));
        public static readonly DependencyProperty GameNameProperty = DependencyProperty.Register("GameName", typeof(CatanGames), typeof(CatanHexPanel), new PropertyMetadata(CatanGames.Regular));
        public static readonly DependencyProperty GameTypeProperty = DependencyProperty.Register("GameType", typeof(GameType), typeof(CatanHexPanel), new PropertyMetadata(GameType.Regular));
        public static readonly DependencyProperty HarborCountProperty = DependencyProperty.Register("HarborCount", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(9));
        public static readonly DependencyProperty IslandsProperty = DependencyProperty.Register("Islands", typeof(string), typeof(CatanHexPanel), new PropertyMetadata("", IslandsChanged));
        public static readonly DependencyProperty KnightsProperty = DependencyProperty.Register("Knights", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(14));
        public static readonly DependencyProperty MaxCitiesProperty = DependencyProperty.Register("MaxCities", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(4));
        public static readonly DependencyProperty MaxResourceAllocatedProperty = DependencyProperty.Register("MaxResourceAllocated", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(19));
        public static readonly DependencyProperty MaxRoadsProperty = DependencyProperty.Register("MaxRoads", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(15));
        public static readonly DependencyProperty MaxKnightsProperty = DependencyProperty.Register("MaxKnights", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(6));
        public static readonly DependencyProperty MaxSettlementsProperty = DependencyProperty.Register("MaxSettlements", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(5));
        public static readonly DependencyProperty MaxShipsProperty = DependencyProperty.Register("MaxShips", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(0));
        public static readonly DependencyProperty MonopolyProperty = DependencyProperty.Register("Monopoly", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(2));
        public static readonly DependencyProperty PirateShipTileProperty = DependencyProperty.Register("PirateShipTile", typeof(TileCtrl), typeof(CatanHexPanel), new PropertyMetadata(null, PirateTileChanged));
        public static readonly DependencyProperty PirateVisibilityProperty = DependencyProperty.Register("PirateVisibility", typeof(Visibility), typeof(CatanHexPanel), new PropertyMetadata(Visibility.Collapsed, PirateVisibilityChanged));
        public static readonly DependencyProperty RoadBuildingProperty = DependencyProperty.Register("RoadBuilding", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(2));
        public static readonly DependencyProperty TileCountProperty = DependencyProperty.Register("TileCount", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(19));
        public static readonly DependencyProperty TileGapProperty = DependencyProperty.Register("TileGap", typeof(double), typeof(CatanHexPanel), new PropertyMetadata(0));
        public static readonly DependencyProperty TileGroupsProperty = DependencyProperty.Register("TileGroups", typeof(string), typeof(CatanHexPanel), new PropertyMetadata(""));
        public static readonly DependencyProperty UniformMarginProperty = DependencyProperty.Register("UniformMargin", typeof(Thickness), typeof(CatanHexPanel), new PropertyMetadata(new Thickness(0,0,0,0)));
        public static readonly DependencyProperty VictoryPointsProperty = DependencyProperty.Register("VictoryPoints", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(5));
        public static readonly DependencyProperty YearOfPlentyProperty = DependencyProperty.Register("YearOfPlenty", typeof(int), typeof(CatanHexPanel), new PropertyMetadata(2));
        public static readonly DependencyProperty CitiesAndKnightsProperty = DependencyProperty.Register("CitiesAndKnights", typeof(bool), typeof(CatanHexPanel), new PropertyMetadata(false, CitiesAndKnightsChanged));
        public bool CitiesAndKnights
        {
            get => ( bool )GetValue(CitiesAndKnightsProperty);
            set => SetValue(CitiesAndKnightsProperty, value);
        }
        private static void CitiesAndKnightsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as CatanHexPanel;
            var depPropValue = (bool)e.NewValue;
            depPropClass?.SetCitiesAndKnights(depPropValue);
        }
        private void SetCitiesAndKnights(bool value)
        {
            if (value)
            {
                ShowMerchant();
            }
            else
            {
                HideMerchant();
            }
        }

        public Dictionary<BuildingKey, BuildingCtrl> BuildingKeyToBuildingCtrlDictionary = new Dictionary<BuildingKey, BuildingCtrl>(new KeyComparer());

        public Dictionary<HarborLocation, HarborLayoutData> HarborLayoutDataDictionary = new Dictionary<HarborLocation, HarborLayoutData>();

        public CatanHexPanel()
        {
            _pirateShip.Visibility = Visibility.Collapsed;
            _baron.Visibility = Visibility.Collapsed;
            _baron.Width = 50;
            _baron.Height = 60;

            HideMerchant();
            _merchant.Width = 40;
            _merchant.Height = 50;
            _merchant.StrokeThickness = 5;
            _merchant.Background = new SolidColorBrush(Windows.UI.Colors.Blue);
            _merchant.Stroke = new SolidColorBrush(Windows.UI.Colors.Black);
            _merchant.HorizontalAlignment = HorizontalAlignment.Left;
            _merchant.VerticalAlignment = VerticalAlignment.Top;
            _merchant.GameController = MainPage.GameController;
            _merchant.EnableMove = true;

            HarborLayer.HorizontalAlignment = HorizontalAlignment.Stretch;
            HarborLayer.VerticalAlignment = VerticalAlignment.Stretch;

            RoadLayer.HorizontalAlignment = HorizontalAlignment.Stretch;
            RoadLayer.VerticalAlignment = VerticalAlignment.Stretch;

            Canvas.SetZIndex(this, 5);
            Canvas.SetZIndex(HarborLayer, 10);
            Canvas.SetZIndex(RoadLayer, 20);
            Canvas.SetZIndex(TopLayer, 95);

            HarborLayer.Width = 5000;
            HarborLayer.Height = 5000;
            HarborLayer.Opacity = 1.0;
            RoadLayer.Width = 5000;
            RoadLayer.Height = 5000;
            RoadLayer.Opacity = 1.0;

            TopLayer.Width = 5000;
            TopLayer.Height = 5000;
            TopLayer.Opacity = 1.0;
            TopLayer.HorizontalAlignment = HorizontalAlignment.Stretch;
            TopLayer.VerticalAlignment = VerticalAlignment.Stretch;
            TopLayer.IsHitTestVisible = true;
            TopLayer.IsDoubleTapEnabled = true;
            TopLayer.IsHoldingEnabled = true;
            TopLayer.IsRightTapEnabled = true;
            TopLayer.IsTapEnabled = true;

            Canvas.SetZIndex(_baron, 5);
            Canvas.SetZIndex(_merchant, 5);
            Canvas.SetZIndex(_pirateShip, 5);

            HarborLayer.Children.Add(_pirateShip);
            HarborLayer.Children.Add(_baron);
            HarborLayer.Children.Add(_merchant);

            this.SizeChanged += SimpleHexPanel_SizeChanged;
            this.Children.Add(HarborLayer);
            this.Children.Add(RoadLayer);
            this.Children.Add(TopLayer);
        }

        public void ArrangeRoads()
        {
            if (RoadKeyToRoadDictionary.Count != 0)
            {
                return;
            }

            if (!_arrangeRoadsDone)
            {
                RoadKeyToRoadDictionary.Clear();
                Roads.Clear();
                RoadLayer.Children.Clear();
                CreateAndArrangeRoads();
                FindAdjacentRoads();
                FindRoadAdjacentToBuildings();
                _arrangeRoadsDone = true;
            }
        }

        //
        //  we should have all of the buildings on the board when this is called.
        //  every tile/location combo should be in the map, or somethign screwed up.
        //  don't use TryGet* so that you'll know when it messes up...
        public void CalculateAdjacentBuildings()
        {
            int middleCol = VisualTiles.Count / 2;

            for (int col = 0; col < VisualTiles.Count; col++)
            {
                for (int row = 0; row < VisualTiles[col].Count; row++)
                {
                    TileCtrl tile = VisualTiles.ElementAt(col).ElementAt(row);

                    foreach (BuildingLocation location in Enum.GetValues(typeof(BuildingLocation)))
                    {
                        if (location == BuildingLocation.None)
                        {
                            continue;
                        }

                        if (BuildingKeyToBuildingCtrlDictionary.TryGetValue(new BuildingKey(tile, location), out BuildingCtrl currentBuilding) == false)
                        {
                            continue;
                        }

                        switch (location)
                        {
                            case BuildingLocation.TopRight:
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.TopLeft);
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.MiddleRight);
                                if (col < middleCol)
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col + 1, row), BuildingLocation.TopLeft); //checked
                                }
                                else
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col + 1, row - 1), BuildingLocation.TopLeft);
                                }
                                break;

                            case BuildingLocation.MiddleRight: //checked
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.TopRight);
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.BottomRight);
                                if (col < middleCol)
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col + 1, row), BuildingLocation.BottomRight); //checked
                                }
                                else
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col + 1, row), BuildingLocation.TopRight); // checked
                                }

                                break;

                            case BuildingLocation.BottomRight:
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.MiddleRight);
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.BottomLeft);
                                if (col < middleCol)
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col, row + 1), BuildingLocation.MiddleRight); //checked
                                }
                                else
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col + 1, row), BuildingLocation.BottomLeft);
                                }

                                break;

                            case BuildingLocation.BottomLeft:
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.BottomRight);
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.MiddleLeft);
                                if (col <= middleCol)
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col - 1, row), BuildingLocation.BottomRight);
                                }
                                else
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col - 1, row), BuildingLocation.BottomRight);
                                }

                                break;

                            case BuildingLocation.MiddleLeft:
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.TopLeft);
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.BottomLeft);
                                if (col <= middleCol)
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col - 1, row - 1), BuildingLocation.BottomLeft);
                                }
                                else
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col - 1, row + 1), BuildingLocation.TopLeft);
                                }

                                break;

                            case BuildingLocation.TopLeft:
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.TopRight);
                                AddAdjacentBuildings(currentBuilding, tile, BuildingLocation.MiddleLeft);
                                if (col < middleCol)
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col, row - 1), BuildingLocation.MiddleLeft);
                                }
                                else
                                {
                                    AddAdjacentBuildings(currentBuilding, GetTileAt(col - 1, row), BuildingLocation.TopRight); // checked
                                }

                                break;

                            case BuildingLocation.None:
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        //
        //  Create the road if needed and build out all the road datastructures we need
        public bool CreateAndArrangeRoads()
        {
            if (Columns == 0)
            {
                return false;
            }

            if (VisualTiles.Count == 0)
            {
                return false;
            }

            for (int col = 0; col < VisualTiles.Count; col++)
            {
                for (int row = 0; row < VisualTiles.ElementAt(col).Count; row++)
                {
                    TileCtrl tile = VisualTiles.ElementAt(col).ElementAt(row);
                    GeneralTransform gt = tile.TransformToVisual(RoadLayer);
                    Point tileZeroZero = gt.TransformPoint(new Point(0, 0));  // this is the upper left hand corner of the Tile

                    foreach (RoadLocation loc in Enum.GetValues(typeof(RoadLocation)))
                    {
                        if (loc == RoadLocation.None)
                        {
                            continue;
                        }

                        RoadKey key = new RoadKey(tile, loc);
                        RoadCtrl clone = GetCloneRoad(row, col, loc);
                        if (clone != null)
                        {
                            RoadKeyToRoadDictionary[key] = clone;
                            clone.Keys.Add(key);
                            clone.RoadType = RoadType.Double;
                        }
                        else
                        {
                            RoadCtrl road = new RoadCtrl
                            {
                                Name = string.Format($"Road_{Roads.Count}"),
                                Height = tile.HexThickness * 2 + TileGap,
                                Width = tile.Height / 1.732, // the side of the Hexagon is the Height divided by the squareroot of 3
                                                             //
                                                             //  this *must* come after setting the width and Height...or you will throw
                                RoadType = RoadType.Single, // born Single, become a Double when there is a clone
                                Location = loc,
                                TileZeroZero = new Point(0, 0) // force the update trigger to fire... bug?
                            };

                            Binding binding = new Binding()
                            {
                                Path = new PropertyPath("CurrentPlayer"),
                                Mode = BindingMode.OneWay,
                                UpdateSourceTrigger = UpdateSourceTrigger.PropertyChanged,
                                Source = MainPage.Current
                            };

                            road.SetBinding(RoadCtrl.CurrentPlayerProperty, binding);

                            road.TileZeroZero = tileZeroZero;
                            road.Callback = _gameCallback;

                            road.Index = Roads.Count;
                            Roads.Add(road);
                            RoadLayer.Children.Add(road);
                            RoadKeyToRoadDictionary[key] = road;
                            road.Keys.Add(key);
                        }
                    }
                }
            }

            return true;
        }

        public void FindAdjacentRoads()
        {
            int middleCol = VisualTiles.Count / 2;

            for (int col = 0; col < VisualTiles.Count; col++)
            {
                for (int row = 0; row < VisualTiles[col].Count; row++)
                {
                    TileCtrl tile = VisualTiles.ElementAt(col).ElementAt(row);
                    foreach (RoadLocation location in Enum.GetValues(typeof(RoadLocation)))
                    {
                        if (location == RoadLocation.None)
                        {
                            continue;
                        }

                        if (RoadKeyToRoadDictionary.TryGetValue(new RoadKey(tile, location), out RoadCtrl road) == false)
                        {
                            continue;
                        }

                        switch (location)
                        {
                            case RoadLocation.Top:
                                AddAdjacentRoad(road, tile, RoadLocation.TopLeft);
                                AddAdjacentRoad(road, tile, RoadLocation.TopRight);

                                if (AddAdjacentRoad(road, GetTileAt(col, row - 1), RoadLocation.BottomLeft) == true)
                                {
                                    // not on top
                                    AddAdjacentRoad(road, GetTileAt(col, row - 1), RoadLocation.BottomRight);
                                }
                                else // I am at the top
                                {
                                    if (col < middleCol)
                                    {
                                        AddAdjacentRoad(road, GetTileAt(col + 1, row), RoadLocation.TopLeft);
                                    }
                                    else if (col > middleCol)  // if you are the top of the center, you only have 2 adjacent roads
                                    {
                                        AddAdjacentRoad(road, GetTileAt(col - 1, row), RoadLocation.TopRight);
                                    }
                                }

                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.TopLeft));
                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.TopRight));
                                break;

                            case RoadLocation.TopRight:
                                AddAdjacentRoad(road, tile, RoadLocation.Top);
                                AddAdjacentRoad(road, tile, RoadLocation.BottomRight);
                                if (col < middleCol)
                                {
                                    AddAdjacentRoad(road, GetTileAt(col + 1, row), RoadLocation.TopLeft);
                                    AddAdjacentRoad(road, GetTileAt(col + 1, row), RoadLocation.Bottom);
                                }
                                else
                                {
                                    AddAdjacentRoad(road, GetTileAt(col + 1, row), RoadLocation.Top);
                                    if (!AddAdjacentRoad(road, GetTileAt(col + 1, row - 1), RoadLocation.TopLeft))
                                    {
                                        AddAdjacentRoad(road, GetTileAt(col, row - 1), RoadLocation.BottomRight);
                                    }
                                }

                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.MiddleRight));
                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.TopRight));
                                break;

                            case RoadLocation.BottomRight:
                                AddAdjacentRoad(road, tile, RoadLocation.TopRight);
                                AddAdjacentRoad(road, tile, RoadLocation.Bottom);
                                if (col < middleCol)
                                {
                                    AddAdjacentRoad(road, GetTileAt(col + 1, row + 1), RoadLocation.BottomLeft);
                                    AddAdjacentRoad(road, GetTileAt(col + 1, row), RoadLocation.Bottom);
                                }
                                else
                                {
                                    if (!AddAdjacentRoad(road, GetTileAt(col + 1, row), RoadLocation.BottomLeft))
                                    {
                                        AddAdjacentRoad(road, GetTileAt(col, row + 1), RoadLocation.TopRight);
                                    }

                                    AddAdjacentRoad(road, GetTileAt(col + 1, row - 1), RoadLocation.Bottom);
                                }

                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.MiddleRight));
                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.BottomRight));
                                break;

                            case RoadLocation.Bottom:
                                AddAdjacentRoad(road, tile, RoadLocation.BottomLeft);
                                AddAdjacentRoad(road, tile, RoadLocation.BottomRight);

                                if (AddAdjacentRoad(road, GetTileAt(col, row + 1), RoadLocation.TopLeft))
                                {
                                    AddAdjacentRoad(road, GetTileAt(col, row + 1), RoadLocation.TopRight);
                                }
                                else // bottom row
                                {
                                    if (col < middleCol)
                                    {
                                        AddAdjacentRoad(road, GetTileAt(col + 1, row + 1), RoadLocation.BottomLeft);
                                    }
                                    else
                                    {
                                        AddAdjacentRoad(road, GetTileAt(col - 1, row + 1), RoadLocation.BottomRight);
                                    }
                                }
                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.BottomRight));
                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.BottomLeft));
                                break;

                            case RoadLocation.BottomLeft:
                                AddAdjacentRoad(road, tile, RoadLocation.TopLeft);
                                AddAdjacentRoad(road, tile, RoadLocation.Bottom);
                                if (col <= middleCol)
                                {
                                    AddAdjacentRoad(road, GetTileAt(col - 1, row - 1), RoadLocation.Bottom);
                                    if (!AddAdjacentRoad(road, GetTileAt(col - 1, row), RoadLocation.BottomRight))
                                    {
                                        AddAdjacentRoad(road, GetTileAt(col, row + 1), RoadLocation.TopLeft);
                                    }
                                }
                                else
                                {
                                    AddAdjacentRoad(road, GetTileAt(col - 1, row + 1), RoadLocation.BottomRight);
                                    AddAdjacentRoad(road, GetTileAt(col - 1, row + 1), RoadLocation.Top);
                                }
                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.BottomLeft));
                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.MiddleLeft));
                                break;

                            case RoadLocation.TopLeft:
                                AddAdjacentRoad(road, tile, RoadLocation.Top);
                                AddAdjacentRoad(road, tile, RoadLocation.BottomLeft);
                                if (col <= middleCol)
                                {
                                    AddAdjacentRoad(road, GetTileAt(col, row - 1), RoadLocation.BottomLeft);
                                    AddAdjacentRoad(road, GetTileAt(col - 1, row), RoadLocation.Top);
                                }
                                else
                                {
                                    AddAdjacentRoad(road, GetTileAt(col - 1, row), RoadLocation.TopRight);
                                    AddAdjacentRoad(road, GetTileAt(col - 1, row), RoadLocation.Bottom);
                                }
                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.TopLeft));
                                road.AdjacentBuildings.Add(GetBuildingAt(tile, BuildingLocation.MiddleLeft));
                                break;

                            default:
                                continue;
                        }
                    }
                }
            }
        }

        public void FindRoadAdjacentToBuildings()
        {
            int middleCol = VisualTiles.Count / 2;

            for (int col = 0; col < VisualTiles.Count; col++)
            {
                for (int row = 0; row < VisualTiles[col].Count; row++)
                {
                    TileCtrl tile = VisualTiles.ElementAt(col).ElementAt(row);

                    foreach (BuildingLocation location in Enum.GetValues(typeof(BuildingLocation)))
                    {
                        if (location == BuildingLocation.None)
                        {
                            continue;
                        }

                        if (BuildingKeyToBuildingCtrlDictionary.TryGetValue(new BuildingKey(tile, location), out BuildingCtrl currentBuilding) == false)
                        {
                            continue;
                        }

                        switch (location)
                        {
                            case BuildingLocation.TopRight:
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.Top);
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.TopRight);
                                if (col < middleCol)
                                {
                                    AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col + 1, row), RoadLocation.TopLeft);
                                }
                                else
                                {
                                    AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col, row - 1), RoadLocation.BottomRight);
                                }
                                break;

                            case BuildingLocation.MiddleRight:
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.TopRight);
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.BottomRight);
                                if (col < middleCol)
                                {
                                    AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col + 1, row), RoadLocation.Bottom);
                                }
                                else
                                {
                                    if (!AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col + 1, row), RoadLocation.Top))
                                    {
                                        AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col + 1, row - 1), RoadLocation.Bottom);
                                    }
                                }
                                break;

                            case BuildingLocation.BottomRight:
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.Bottom);
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.BottomRight);
                                if (col < middleCol)
                                {
                                    if (!AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col + 1, row + 1), RoadLocation.BottomLeft))
                                    {
                                        AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col, row + 1), RoadLocation.TopRight);
                                    }
                                }
                                else
                                {
                                    AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col + 1, row), RoadLocation.BottomLeft);
                                }
                                break;

                            case BuildingLocation.BottomLeft:
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.Bottom);
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.BottomLeft);
                                if (col <= middleCol)
                                {
                                    if (!AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col - 1, row), RoadLocation.BottomRight))
                                    {
                                        AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col, row + 1), RoadLocation.TopLeft);
                                    }
                                }
                                else
                                {
                                    AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col - 1, row + 1), RoadLocation.BottomRight);
                                }
                                break;

                            case BuildingLocation.MiddleLeft:
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.TopLeft);
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.BottomLeft);
                                if (col <= middleCol)
                                {
                                    AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col - 1, row), RoadLocation.Top);
                                }
                                else
                                {
                                    AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col - 1, row), RoadLocation.Bottom);
                                }
                                break;

                            case BuildingLocation.TopLeft:
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.TopLeft);
                                AddBuildingAdjacentRoad(currentBuilding, tile, RoadLocation.Top);
                                if (col <= middleCol)
                                {
                                    AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col, row - 1), RoadLocation.BottomLeft);
                                }
                                else
                                {
                                    AddBuildingAdjacentRoad(currentBuilding, GetTileAt(col - 1, row), RoadLocation.TopRight);
                                }
                                break;

                            case BuildingLocation.None:
                                break;

                            default:
                                break;
                        }
                    }
                }
            }
        }

        public void ShuffleHarbors(List<int> randomHarborTypeList)
        {
            for (int i = 0; i < Harbors.Count; i++)
            {
                Harbors[i].HarborType = StartingHarborTypes[randomHarborTypeList[i]];
            }
        }

        public List<int> ShuffleTileGroup(TileGroup tileGroup, List<int> randomResourceTypeList)
        {
            if (randomResourceTypeList == null)
            {
                randomResourceTypeList = GameContainerCtrl.GetRandomList(tileGroup.TilesToRandomize.Count - 1);
            }

            for (int i = 0; i < tileGroup.TilesToRandomize.Count; i++)
            {
                if (tileGroup.TilesToRandomize[i].ResourceType != tileGroup.StartingResourceTypes[randomResourceTypeList[i]])
                {
                    tileGroup.TilesToRandomize[i].ResourceType = tileGroup.StartingResourceTypes[randomResourceTypeList[i]];
                }
            }

            return randomResourceTypeList;
        }

        public Size Update(Size finalSize)
        {
            if (Tiles.Count == 0)
            {
                return finalSize;
            }

            if (DisableLayout || RowCounts.Count == 0 || _tilesInVisualLayout == null)
            {
                return finalSize;
            }

            if (_tilesInVisualLayout.Count == 0)
            {
                return finalSize;
            }

            //
            //
            int middleCol = _colCount / 2;
            double left = UniformMargin.Left + _normalWidth * 0.25;
            double top = 0;
            double gap = -TileGap / 2.0;

            for (int col = 0; col < _tilesInVisualLayout.Count; col++)
            {
                List<TileCtrl> childColumn = _tilesInVisualLayout[col];

                if (col <= middleCol)
                {
                    gap += TileGap / 2.0;
                }
                else
                {
                    gap -= TileGap / 2.0;
                }

                try
                {
                    top = Math.Abs(( RowCounts[col] - RowCounts[middleCol] )) * NormalHeight * .5 + UniformMargin.Top - gap;
                    foreach (TileCtrl tile in childColumn)
                    {
                        tile.Arrange(new Rect(0, 0, tile.DesiredSize.Width, tile.DesiredSize.Height));
                        Canvas.SetTop(tile, top);
                        Canvas.SetLeft(tile, left);
                        top += tile.DesiredSize.Height + TileGap;
                    }
                }
                catch { }
                left += _normalWidth * .75 + TileGap;
            }

            MoveHarbors();

            if (!StaticHelpers.IsInVisualStudioDesignMode)
            {
                //#pragma warning disable 4014
                //                CoreWindow.GetForCurrentThread().Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                //                {
                //                    ArrangeRoads();
                //                });
                //#pragma warning restore 4014
            }

            HarborLayer.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));
            TopLayer.Arrange(new Rect(0, 0, finalSize.Width, finalSize.Height));

            ArrangeBuildings();
            ArrangeRoads();

            CalculateAdjacentBuildings();
            return finalSize;
        }
        public bool BaronShown
        {
            get
            {
                return ( _baron.Opacity == 100 && _baron.Visibility == Visibility.Visible );
            }
        }
        public void HideBaron()
        {
            _baron.HideAsync();
        }

        public void ShowBaron()
        {
            _baron.ShowAsync();
        }

        public bool MerchantShown
        {
            get
            {
                return ( _merchant.Opacity == 100 );
            }
        }
        public void HideMerchant()
        {
            _merchant.HideAsync();

        }

        public void ShowMerchant()
        {
            _merchant.ShowAsync();
        }

        internal void MoveMerchantAsync(Point to)
        {
            _merchant.MoveAsync(to);
        }
    } // End CatanHexPanel

    public class HarborLayoutData
    {
        public static double Height { get; } = 50;

        public static double Width { get; } = 50;

        public Point Anchor { get; set; }

        public Point RenderTransformOrigin { get; set; }

        public double Rotation { get; set; }

        public double Scale { get; set; } = 1.1;

        public HarborLayoutData(Point rto, double rotate, double scale, Point anchor)
        {
            RenderTransformOrigin = rto;
            Rotation = rotate;
            Scale = scale;
            Anchor = anchor;
        }

        public static void AddLocation(HarborLocation location, Point rto, double rotate, double scale, Point anchor, Dictionary<HarborLocation, HarborLayoutData> harborLayoutDataDictionary)
        {
            HarborLayoutData data = new HarborLayoutData(rto, rotate, scale, anchor);
            harborLayoutDataDictionary[location] = data;
        }

        //
        //  move this harbor to this location on this tile
        public static void DoLayout(Harbor harbor, TileCtrl tile, UIElement canvas, Dictionary<HarborLocation, HarborLayoutData> harborLayoutDataDictionary)
        {
            HarborLayoutData data = harborLayoutDataDictionary[harbor.HarborLocation];
            GeneralTransform gtToSource = tile.TransformToVisual(canvas);
            Point to = gtToSource.TransformPoint(data.Anchor);
            harbor.RenderTransformOrigin = data.RenderTransformOrigin;
            harbor.Transform.Rotation = data.Rotation;
            harbor.Transform.TranslateX = to.X;
            harbor.Transform.TranslateY = to.Y;
            harbor.Transform.ScaleX = data.Scale;
            harbor.Transform.ScaleY = data.Scale;
        }
    }

    public class Island
    {
        public bool BonusPoint = false;
        public int End = -1;
        public int Start = -1;
    }

    public class StaticGameInfo : DependencyObject
    {
        public static readonly DependencyProperty MaxRoadsProperty = DependencyProperty.RegisterAttached("MaxRoads", typeof(int), typeof(StaticGameInfo), new PropertyMetadata(15));

        public static int GetMaxRoads(CatanHexPanel element)
        {
            return ( int )element.GetValue(MaxRoadsProperty);
        }

        public static void SetMaxRoads(CatanHexPanel element, int value)
        {
            element.SetValue(MaxRoadsProperty, value);
        }
    }
}
