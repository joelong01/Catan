using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class GameViewControl : UserControl
    {

       
        public TileCtrl CenterTile
        {
            get
            {
                //return _hexPanel.Children.Last() as TileCtrl;
                return null;
            }
        }

        public string GameName
        {
            get
            {
                return (string)GetValue(GameNameProperty).ToString();

            }
            set
            {
                if (GameName != value)
                    SetValue(GameNameProperty, value);
            }
        }
        public bool? SetOrder
        {
            get
            {
                return (bool?)GetValue(SetOrderProperty);
            }
            set
            {
                _tileIndex = 0;
                SetValue(SetOrderProperty, value);
            }
        }



        public GameType GameType
        {
            get
            {
                return _gameType;
            }
            set
            {
                _gameType = value; // set when you open a file
            }
        }



        public List<TileCtrl> Tiles
        {
            get
            {
                List<TileCtrl> tiles = new List<TileCtrl>();
                foreach (TileGroup tg in _currentGame.TileGroups)
                {
                    if (tg.Randomize)
                    {
                        tiles.AddRange(tg.Tiles);
                    }
                }

                return tiles;
            }
        }

        public List<TileCtrl> AllTiles
        {
            get
            {
                List<TileCtrl> tiles = new List<TileCtrl>();
                foreach (TileGroup tg in _currentGame?.TileGroups)
                {

                    tiles.AddRange(tg.Tiles);

                }

                return tiles;
            }
        }



        internal async Task Reset()
        {
            List<Task> tasks = new List<Task>();
            foreach (TileGroup tg in _currentGame.TileGroups)
            {
                if (tg.Randomize == false) continue;
                foreach (TileCtrl t in tg.Tiles)
                {
                    t.Reset();
                    t.AnimateFade(1.0, tasks);
                    t.Rotate(0, tasks, false);
                    t.SetTileOrientation(TileOrientation.FaceDown, true, tasks, MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.SuperFast));

                }
            }

            ResetRoads();

            await Task.WhenAll(tasks.ToArray());
        }

        public void ResetRoads()
        {
            foreach (var kvp in _currentGame.RoadKeyToRoadDictionary)
            {
                kvp.Value.RoadState = RoadState.Unowned;
            }

            BuildColor = Colors.Transparent;
        }

        public async Task RotateTiles()
        {
            List<Task> tasks = new List<Task>();

            foreach (TileGroup tg in _currentGame.TileGroups)
            {
                if (tg.Randomize == false) continue;

                foreach (TileCtrl t in tg.Tiles)
                {
                    t.Rotate(180, tasks, true);
                }
            }

            await Task.WhenAll(tasks.ToArray());
        }

        internal TileCtrl GetAdjacentTile(TileCtrl tile, RoadLocation adjacentLocation)
        {
            TileCtrl adj = null;
            List<List<TileCtrl>> visualLayout = _currentGame.VisualLayout();
            for (int col = 0; col < visualLayout.Count; col++)
            {
                for (int row = 0; row < visualLayout.ElementAt(col).Count; row++)
                {
                    TileCtrl t = visualLayout.ElementAt(col).ElementAt(row);
                    if (t == tile)
                    {
                        switch (adjacentLocation)
                        {
                            case RoadLocation.TopRight:
                                return NextUpperRight(row, col);
                            case RoadLocation.TopLeft:
                                return PreviousUpperLeft(row, col);
                            case RoadLocation.BottomRight:
                                return NextLowerRight(row, col);
                            case RoadLocation.BottomLeft:
                                return PreviousLowerLeft(row, col);
                            case RoadLocation.Top:
                                return AboveTile(row, col);
                            case RoadLocation.Bottom:
                                return BelowTile(row, col);
                            default:
                                break;
                        }
                    }

                }

            }


            return adj;
        }



        //
        //  given a tile and a location, find the line segment that is in the same road
        public Polygon GetAdjacentRoadSegment(TileCtrl tile, RoadLocation myLocation)
        {
            return null;
            //TileCtrl adjacent = this.GetAdjacentTile(tile, myLocation);


            //if (adjacent != null)
            //{
            //    return adjacent.GetRoadSegment(GetAdjacentLocation(myLocation));
            //}


            //return null;
        }

        public RoadLocation GetAdjacentLocation(RoadLocation myLocation)
        {
            RoadLocation adjacentLocation = RoadLocation.None;
            switch (myLocation)
            {

                case RoadLocation.TopRight:
                    adjacentLocation = RoadLocation.BottomLeft;
                    break;
                case RoadLocation.TopLeft:
                    adjacentLocation = RoadLocation.BottomRight;
                    break;
                case RoadLocation.BottomRight:
                    adjacentLocation = RoadLocation.TopLeft;
                    break;
                case RoadLocation.BottomLeft:
                    adjacentLocation = RoadLocation.TopRight;
                    break;
                case RoadLocation.Top:
                    adjacentLocation = RoadLocation.Bottom;
                    break;
                case RoadLocation.Bottom:
                    adjacentLocation = RoadLocation.Top;
                    break;
                default:
                    throw new InvalidDataException("bad tag on a road polygon");
            }

            return adjacentLocation;
        }



        public void BuildRoadDictionary()
        {
            //Dictionary<RoadKey, Road> roadKeyToRoad = _currentGame.RoadKeyToRoadDictionary;
            //Dictionary<Polygon, Road> roadSegmentToRoad = _currentGame.RoadSegmentToRoadDictionary;
            //if (roadKeyToRoad.Count == 0)
            //{



            //    List<List<TileCtrl>> visualTiles = CurrentGame.VisualLayout();

            //    //
            //    //  Need to check the last column to see if one red tile is below another
            //    for (int col = 0; col < Columns; col++)
            //    {
            //        for (int row = 0; row < visualTiles.ElementAt(col).Count; row++)
            //        {
            //            TileCtrl tile = visualTiles.ElementAt(col).ElementAt(row);
            //            foreach (SettlementCtrl settlement in tile.Settlements)
            //            {
            //                AddSameSettlementLocationToDictionary(tile, settlement);
            //            }


            //            foreach (RoadLocation loc in Enum.GetValues(typeof(RoadLocation)))
            //            {
            //                if (loc == RoadLocation.None) continue;
            //                Polygon polyA = tile.GetRoadSegment(loc);
            //                TileCtrl adjacentTile = GetAdjacentTile(tile, loc);
            //                Polygon polyB = null;
            //                RoadLocation adjacentLocation = RoadLocation.None;
            //                if (adjacentTile != null)
            //                {

            //                    adjacentLocation = GetAdjacentLocation(loc);
            //                    polyB = adjacentTile.GetRoadSegment(adjacentLocation);
            //                    if (polyB != null)
            //                    {
            //                        adjacentLocation = (RoadLocation)Enum.Parse(typeof(RoadLocation), (string)polyB.Tag);
            //                    }
            //                    else
            //                    {
            //                        this.TraceMessage("null location in adjacent tile");
            //                    }
            //                }

            //                Road road = new Road(polyA, tile, loc, adjacentTile, polyB, adjacentLocation);
            //                //
            //                //  we need to be able to find a Road from the RoadSegment
            //                roadSegmentToRoad[polyA] = road;
            //                if (polyB != null)
            //                {
            //                    roadSegmentToRoad[polyB] = road;
            //                }

            //                //
            //                // we will also sometimes need to find a road given a tile and a location                            
            //                roadKeyToRoad[new RoadKey(tile, loc)] = road;
            //                if (adjacentTile != null)
            //                {
            //                    roadKeyToRoad[new RoadKey(adjacentTile, adjacentLocation)] = road;
            //                }

            //                //
            //                //  for longest road, we need to know adjacent settlements                            
            //                foreach (SettlementCtrl adjSettlementCtrl in tile.SettlementAt(loc))
            //                {
            //                    Settlement s = SettlementCtrlToSettlementData[adjSettlementCtrl];
            //                    road.Settlements.Add(s);
            //                }


            //            }
            //        }
            //    }
            //}

            //AddAdjacentRoads();
        }


        Color _buildColor = Colors.Tomato;

        /// <summary>
        ///     Update the colors of all non-used settlements spots and road spots
        /// </summary>
        public Color BuildColor
        {
            get
            {
                return _buildColor;
            }
            set
            {
                if (_currentGame.RoadKeyToRoadDictionary.Count == 0)
                    return;

                if (_buildColor != value)
                {
                    _buildColor = value;

                    //
                    //  update the build color for all roads
                    foreach (var kvp in _currentGame.RoadKeyToRoadDictionary)
                    {
                        if (!kvp.Value.IsOwned)
                        {
                            kvp.Value.Color = _buildColor;
                            kvp.Value.Show(false);
                        }

                    }
                    //
                    //  tiles own settlements -- go through them
                    foreach (TileCtrl tile in AllTiles)
                    {
                        foreach (var s in tile.Settlements)
                        {
                            if (s.SettlementType == SettlementType.None)
                                s.Color = _buildColor;
                        }
                    }

                }

            }
        }

        public Road RoadFromSegment(Polygon roadSegement)
        {
            return _currentGame.RoadSegmentToRoadDictionary[roadSegement];

        }

        internal Road GetRoadAt(TileCtrl tile, RoadLocation roadLocation)
        {
            if (tile == null) return null;
            RoadKey key = new RoadKey(tile, roadLocation);
            Road road = null;
            _currentGame.RoadKeyToRoadDictionary.TryGetValue(key, out road);
            return road;
        }

        //
        //  Settlement Data
        public Dictionary<SettlementCtrl, SettlementCtrl> SettlementCtrlToSettlementData { get; } = new Dictionary<SettlementCtrl, SettlementCtrl>(); // lookup table used for envent handlers
        public Dictionary<SettlementKey, SettlementCtrl> TileLocToSettlementData { get; } = new Dictionary<SettlementKey, SettlementCtrl>(new KeyComparer()); // lookup table used for envent handlers

        //
        //  returns all Settlements associated with the control, or NULL if none are
        public SettlementCtrl GetSettlement(SettlementCtrl control)
        {
            SettlementCtrl retSettlement = null;
            SettlementCtrlToSettlementData.TryGetValue(control, out retSettlement);
            return retSettlement;
        }

        public SettlementCtrl GetSettlement(SettlementKey key)
        {
            SettlementCtrl retSettlement = null;
            TileLocToSettlementData.TryGetValue(key, out retSettlement);
            return retSettlement;
        }
        public SettlementCtrl GetSettlement(TileCtrl tile, SettlementLocation loc)
        {
            return GetSettlement(new SettlementKey(tile, loc));
        }

       

        private void AddSameSettlementLocationToDictionary(TileCtrl tile, SettlementCtrl inputSettlementCtrl)
        {
            RoadLocation[] adjTileLocations = null;
            SettlementLocation[] settlementLocation = null;
            SettlementLocation inputLocation = inputSettlementCtrl.SettlementLocation;
            switch (inputLocation)
            {
                case SettlementLocation.TopRight:
                    adjTileLocations = new RoadLocation[3] { RoadLocation.None, RoadLocation.Top, RoadLocation.TopRight };
                    settlementLocation = new SettlementLocation[3] { inputLocation, SettlementLocation.BottomRight, SettlementLocation.MiddleLeft };
                    break;
                case SettlementLocation.MiddleRight:
                    adjTileLocations = new RoadLocation[3] { RoadLocation.None, RoadLocation.TopRight, RoadLocation.BottomRight };
                    settlementLocation = new SettlementLocation[3] { inputLocation, SettlementLocation.BottomLeft, SettlementLocation.TopLeft };
                    break;
                case SettlementLocation.BottomRight:
                    adjTileLocations = new RoadLocation[3] { RoadLocation.None, RoadLocation.BottomRight, RoadLocation.Bottom };
                    settlementLocation = new SettlementLocation[3] { inputLocation, SettlementLocation.MiddleLeft, SettlementLocation.TopRight };
                    break;
                case SettlementLocation.BottomLeft:
                    adjTileLocations = new RoadLocation[3] { RoadLocation.None, RoadLocation.Bottom, RoadLocation.BottomLeft };
                    settlementLocation = new SettlementLocation[3] { inputLocation, SettlementLocation.TopRight, SettlementLocation.MiddleRight };
                    break;
                case SettlementLocation.MiddleLeft:
                    adjTileLocations = new RoadLocation[3] { RoadLocation.None, RoadLocation.BottomLeft, RoadLocation.TopLeft };
                    settlementLocation = new SettlementLocation[3] { inputLocation, SettlementLocation.TopRight, SettlementLocation.BottomRight };
                    break;
                case SettlementLocation.TopLeft:
                    adjTileLocations = new RoadLocation[3] { RoadLocation.None, RoadLocation.TopLeft, RoadLocation.Top };
                    settlementLocation = new SettlementLocation[3] { inputLocation, SettlementLocation.MiddleRight, SettlementLocation.BottomLeft };
                    break;
                case SettlementLocation.None:
                    break;
                default:
                    break;
            }

            AddSettlementClonesToDictionary(tile, adjTileLocations, settlementLocation);
        }


        void AddSettlementClonesToDictionary(TileCtrl inputTile, RoadLocation[] roadLocations, SettlementLocation[] settlementLocations)
        {
            //SettlementCtrl adjSettlementCtr = null;
            //TileCtrl adjTile = null;
            //Settlement settlement = new Settlement(); // all SettlementCtrls in the SettlementLocations[] array should map to this Settlement
            //for (int i = 0; i < 3; i++)
            //{
            //    if (roadLocations[i] == RoadLocation.None)
            //        adjTile = inputTile;
            //    else
            //        adjTile = GetAdjacentTile(inputTile, roadLocations[i]);

            //    if (adjTile != null)
            //    {
            //        adjSettlementCtr = adjTile.SettlementAt(settlementLocations[i]);

            //        SettlementCtrlToSettlementData[adjSettlementCtr] = settlement; // so given a control, we can find the Settlement it is a member of

            //        //
            //        //  is this tile/control pair already in the map?                    
            //        SettlementKey key = new SettlementKey(adjTile, adjSettlementCtr.SettlementLocation);
            //        if (TileLocToSettlementData.ContainsKey(key) == false)  // note: we've implemented IComparer so that the key is looked at by value...
            //        {
            //            // 
            //            // we haven't seen it with this key
            //            TileLocToSettlementData[key] = settlement;
            //        }

            //        SettlementData data = new SettlementData(adjTile, adjSettlementCtr); // the next settlement at the same location
            //        if (settlement.Contains(data) == false)
            //        {
            //            //
            //            //  don't add the SettlementData to the list unless it is a unique combo of location and tile
            //            settlement.Settlements.Add(data);
            //        }
            //    }

            //}
        }

        private Road GetRoadAt(TileCtrl startTile, TileLocation adjTileLocation, RoadLocation roadLocation)
        {
            TileCtrl adjTile = null;
            if (adjTileLocation == TileLocation.Self)
                adjTile = startTile;
            else
                adjTile = GetAdjacentTile(startTile, ((RoadLocation)(int)adjTileLocation));

            return GetRoadAt(adjTile, roadLocation);
        }

        //
        //  setup all the Heads and Tails for the Tiles
        //
        //  by convetion Top, TopRight, and BottomRight are "heads" and Bottom, BottomLeft, and TopLeft are "tails"
        //private void BuildRoadBinaryTree()
        //{
        //    List<List<TileCtrl>> visualTiles = CurrentGame.VisualLayout();

        //    for (int col = 0; col < Columns; col++)
        //    {
        //        for (int row = 0; row < visualTiles.ElementAt(col).Count; row++)
        //        {
        //            TileCtrl tile = visualTiles.ElementAt(col).ElementAt(row);
        //            Road thatRoad = null;
        //            Road thisRoad = null;
        //            foreach (TileLocation loc in Enum.GetValues(typeof(TileLocation)))
        //            {
        //                if (loc == TileLocation.Self) continue;

        //                thisRoad = GetRoadAt(tile, (RoadLocation)loc);

        //                switch (loc)
        //                {

        //                    case TileLocation.Top:
        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.TopRight);
        //                        thisRoad.AddHeads(thatRoad);
        //                        thatRoad?.AddTails(thisRoad);
        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.TopLeft);
        //                        thisRoad.AddTails(thatRoad);
        //                        thatRoad?.AddHeads(thisRoad);
        //                        if (thatRoad != null) thatRoad.BreakCycleRoad = thisRoad; // only place this is set

        //                        thatRoad = GetRoadAt(tile, TileLocation.Top, RoadLocation.BottomLeft);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddTails(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad);
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.TopLeft, RoadLocation.TopRight);
        //                            thisRoad.AddTails(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad);
        //                        }

        //                        thatRoad = GetRoadAt(tile, TileLocation.Top, RoadLocation.BottomRight);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddTails(thisRoad);
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.TopRight, RoadLocation.TopLeft);
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddTails(thisRoad);
        //                        }
        //                        break;
        //                    case TileLocation.TopRight:
        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.BottomRight);
        //                        thisRoad.AddHeads(thatRoad);
        //                        thatRoad?.AddHeads(thisRoad);   // note cycle
        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.Top);
        //                        thisRoad.AddTails(thatRoad);
        //                        thatRoad?.AddHeads(thisRoad);

        //                        thatRoad = GetRoadAt(tile, TileLocation.Top, RoadLocation.BottomRight);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddTails(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad);
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.TopRight, RoadLocation.TopLeft);
        //                            thisRoad.AddTails(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad);
        //                        }

        //                        thatRoad = GetRoadAt(tile, TileLocation.TopRight, RoadLocation.Bottom);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddTails(thisRoad);
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.BottomRight, RoadLocation.Top);
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddTails(thisRoad);
        //                        }
        //                        break;

        //                    case TileLocation.BottomRight:
        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.Bottom);
        //                        thisRoad.AddTails(thatRoad);
        //                        thatRoad?.AddHeads(thisRoad);

        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.TopRight);
        //                        thisRoad.AddTails(thatRoad);
        //                        thatRoad?.AddHeads(thisRoad);

        //                        thatRoad = GetRoadAt(tile, TileLocation.Bottom, RoadLocation.TopRight);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddTails(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad);
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.BottomLeft, RoadLocation.TopRight);
        //                            thisRoad.AddTails(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad);
        //                        }

        //                        thatRoad = GetRoadAt(tile, TileLocation.Bottom, RoadLocation.TopRight);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddTails(thisRoad);
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.BottomRight, RoadLocation.TopLeft);
        //                            thisRoad.AddTails(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad);
        //                        }
        //                        break;
        //                    case TileLocation.Bottom:
        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.BottomRight);
        //                        thisRoad.AddHeads(thatRoad);
        //                        thatRoad?.AddTails(thisRoad);
        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.BottomLeft);
        //                        thisRoad.AddTails(thatRoad);
        //                        thatRoad?.AddHeads(thisRoad);

        //                        thatRoad = GetRoadAt(tile, TileLocation.Bottom, RoadLocation.TopRight);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddTails(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad);
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.BottomRight, RoadLocation.BottomLeft);
        //                            thisRoad.AddTails(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad);
        //                        }

        //                        thatRoad = GetRoadAt(tile, TileLocation.BottomLeft, RoadLocation.Top);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddTails(thisRoad);
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.TopRight, RoadLocation.Bottom);
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddTails(thisRoad);
        //                        }
        //                        break;
        //                    case TileLocation.BottomLeft:
        //                        //  my first Head (on the same tile)
        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.TopLeft);
        //                        thisRoad.AddHeads(thatRoad);
        //                        thatRoad?.AddTails(thisRoad);
        //                        // my first tail (on the same tile)
        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.Bottom);
        //                        thisRoad.AddTails(thatRoad);
        //                        thatRoad?.AddHeads(thisRoad);

        //                        thatRoad = GetRoadAt(tile, TileLocation.Bottom, RoadLocation.TopLeft);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddTails(thisRoad);
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.BottomLeft, RoadLocation.BottomLeft);
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddTails(thisRoad);
        //                        }

        //                        thatRoad = GetRoadAt(tile, TileLocation.BottomLeft, RoadLocation.Top);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad); // this is the cylce!!
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.TopLeft, RoadLocation.Bottom);
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad); // this is the cylce!!
        //                        }
        //                        break;
        //                    case TileLocation.TopLeft:
        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.Top);
        //                        thisRoad.AddHeads(thatRoad);
        //                        thatRoad?.AddTails(thisRoad);
        //                        thatRoad = GetRoadAt(tile, TileLocation.Self, RoadLocation.BottomLeft);
        //                        thisRoad.AddTails(thatRoad);
        //                        thatRoad?.AddHeads(thisRoad);

        //                        thatRoad = GetRoadAt(tile, TileLocation.Top, RoadLocation.BottomLeft);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddTails(thisRoad);
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.TopLeft, RoadLocation.TopRight);
        //                            thisRoad.AddHeads(thatRoad);
        //                            thatRoad?.AddTails(thisRoad);
        //                        }

        //                        thatRoad = GetRoadAt(tile, TileLocation.BottomLeft, RoadLocation.Top);
        //                        if (thatRoad != null)
        //                        {
        //                            thisRoad.AddTails(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad);
        //                        }
        //                        else
        //                        {
        //                            thatRoad = GetRoadAt(tile, TileLocation.TopLeft, RoadLocation.Bottom);
        //                            thisRoad.AddTails(thatRoad);
        //                            thatRoad?.AddHeads(thisRoad);
        //                        }
        //                        break;
        //                    default:
        //                        break;
        //                }

        //            }
        //        }
        //    }
        //}


        private void AddAdjacentRoads()
        {
            List<List<TileCtrl>> visualTiles = CurrentGame.VisualLayout();

            for (int col = 0; col < Columns; col++)
            {
                for (int row = 0; row < visualTiles.ElementAt(col).Count; row++)
                {
                    TileCtrl tile = visualTiles.ElementAt(col).ElementAt(row);
                    Road thisRoad = null;
                    Road adjRoad = null;
                    foreach (TileLocation loc in Enum.GetValues(typeof(TileLocation)))
                    {
                        if (loc == TileLocation.Self) continue;

                        thisRoad = GetRoadAt(tile, (RoadLocation)loc);

                        switch (loc)
                        {

                            case TileLocation.Top:
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.TopRight);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.TopLeft);

                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Top, RoadLocation.BottomLeft);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Top, RoadLocation.BottomRight);

                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.TopLeft, RoadLocation.TopRight);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.TopRight, RoadLocation.TopLeft);
                                break;
                            case TileLocation.TopRight:
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.BottomRight);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.Top);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.TopRight, RoadLocation.TopLeft);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.BottomRight, RoadLocation.Top);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Top, RoadLocation.BottomRight);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.TopRight, RoadLocation.Bottom);

                                break;

                            case TileLocation.BottomRight:
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.Bottom);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.TopRight);

                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Bottom, RoadLocation.TopRight);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.BottomRight, RoadLocation.BottomLeft);

                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.TopRight, RoadLocation.Bottom);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.BottomRight, RoadLocation.Top);

                                break;
                            case TileLocation.Bottom:
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.BottomRight);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.BottomLeft);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Bottom, RoadLocation.TopRight);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Bottom, RoadLocation.TopLeft);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.BottomRight, RoadLocation.BottomLeft);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.BottomLeft, RoadLocation.BottomRight);

                                break;
                            case TileLocation.BottomLeft:

                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.TopLeft);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.Bottom);

                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Bottom, RoadLocation.TopLeft);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.BottomLeft, RoadLocation.BottomRight);

                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.BottomLeft, RoadLocation.Top);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.TopLeft, RoadLocation.Bottom);
                                break;
                            case TileLocation.TopLeft:
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.Top);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Self, RoadLocation.BottomLeft);

                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.Top, RoadLocation.BottomLeft);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.TopLeft, RoadLocation.TopRight);

                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.BottomLeft, RoadLocation.Top);
                                adjRoad = AddAdjacentRoad(tile, thisRoad, TileLocation.TopLeft, RoadLocation.Bottom);
                                break;
                            default:
                                break;
                        }

                    }
                }
            }
        }
        private Road AddAdjacentRoad(TileCtrl tile, Road thisRoad, TileLocation adjTileLocation, RoadLocation adjRoadLocation)
        {
            Road thatRoad = GetRoadAt(tile, adjTileLocation, adjRoadLocation);
            thisRoad.AdjacentRoads.Add(thatRoad);
            return thatRoad;


        }


   


    }




    

  
}
