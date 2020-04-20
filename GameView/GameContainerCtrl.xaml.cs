
using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{


    public class CatanGame : ICatanGameData
    {

        public Type ControlType { get; set; } = typeof(RegularGameCtrl);

        public string Description { get; set; } = "Regular";
        public int Index { get; set; } = -1;
        public CatanGame(Type type, string s, int idx)
        {
            ControlType = type;
            Description = s;
            Index = idx;

        }




        public UserControl Control { get; set; } = null;

        ICatanGameData ChildControl
        {
            get
            {
                if (Control != null)
                {
                    return Control as ICatanGameData;
                }
                return null;
            }
        }
        #region ICatanGameData implementation
        public CatanHexPanel HexPanel
        {
            get
            {
                if (Control != null)
                {
                    return ((ICatanGameData)Control).HexPanel;
                }
                return null;
            }
        }
        public GameType GameType => ChildControl.GameType;

        public int MaxCities => ChildControl.MaxCities;

        public int MaxRoads => ChildControl.MaxRoads;

        public int MaxSettlements => ChildControl.MaxSettlements;

        public int MaxShips => ChildControl.MaxShips;
        public List<TileCtrl> Tiles => ChildControl.Tiles;

        public List<TileCtrl> DesertTiles => ChildControl.DesertTiles;

        #endregion
        public override string ToString()
        {
            return String.Format($"{Description}.{ControlType.Name}");
        }


    }



    /// <summary>
    ///     This class is the "container" of games.  its job is to keep track of the various user controls we have that have board layout
    ///     It is called by the Page, and the Page shouldn't have to know anything about the underlying HexControl
    /// </summary>
    /// 

    public sealed partial class GameContainerCtrl : UserControl
    {

        //
        //  when you build a new Game control, add it to this list
        //
        readonly List<CatanGame> _games = new List<CatanGame>()
        {
            new CatanGame (typeof (RegularGameCtrl), "Regular", 0),
            new CatanGame (typeof (ExpansionCtrl), "Expansion (5-6 Players)", 1),
            new CatanGame (typeof (Seafarers4PlayerCtrl), "Seafarers (4 Player)",2),
            new CatanGame (typeof (FourIsland3Ctrl), "Four Islands (3 Player)", 3),

        };

        IGameCallback _gameCallback = null;
        ITileControlCallback _tileCallback = null;


        public RandomBoardSettings RandomBoardSettings { get; private set; } = new RandomBoardSettings();
        private Dictionary<ResourceType, double> _probabilities = new Dictionary<ResourceType, double>();
        public Dictionary<ResourceType, double> Probabilities
        {
            get
            {
                if (_probabilities.Count == 0)
                {
                    _probabilities = new Dictionary<ResourceType, double>();
                    //
                    //  next we need to add up the probabilities for each number in each tile 
                    //  as well as the count of the resource type
                    List<int> ProbabilityList = new List<int>();
                    Dictionary<ResourceType, List<int>> resourceToProbDict = new Dictionary<ResourceType, List<int>>();
                    foreach (TileCtrl tile in _currentHexPanel.Tiles)
                    {
                        resourceToProbDict.TryGetValue(tile.ResourceType, out List<int> tempList);
                        if (tempList == null)
                        {
                            tempList = new List<int>();
                            resourceToProbDict[tile.ResourceType] = tempList;
                        }

                        tempList.Add(tile.Probability);
                    }

                    // now we have the data to calculate our probabilities
                    foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
                    {
                        resourceToProbDict.TryGetValue(resourceType, out List<int> tempList);
                        if (tempList == null)
                        {
                            _probabilities[resourceType] = 0;
                            continue;
                        }

                        int sum = 0;
                        foreach (int i in tempList)
                        {
                            sum += i;
                        }

                        _probabilities[resourceType] = sum / (double)tempList.Count;
                    }
                }

                return _probabilities;
            }
        }

        public GameContainerCtrl()
        {
            this.InitializeComponent();
            Games = _games;


        }

        public int GetGameIndex(string description)
        {
            for (int index = 0; index < _games.Count(); index++)
            {
                CatanGame game = _games[index];
                if (game.Description.Equals(description))
                {
                    return index;
                }
            }
            throw new InvalidDataException("bad description passed into GameContainerCtrl.GetGameIndex");
        }

        public TileCtrl GetTile(int tileIndex, int gameIndex)
        {
            if (CurrentGame.Index != gameIndex)
            {
                CurrentGame = _games[gameIndex];
            }

            return _currentHexPanel.TilesInIndexOrder[tileIndex];

        }
        public RoadCtrl GetRoad(int roadIndex, int gameIndex)
        {
            if (CurrentGame.Index != gameIndex)
            {
                CurrentGame = _games[gameIndex];
            }
            return _currentHexPanel.Roads[roadIndex];
        }

        public BuildingCtrl GetSettlement(int settlementIndex, int gameIndex)
        {
            if (CurrentGame.Index != gameIndex)
            {
                CurrentGame = _games[gameIndex];
            }
            return _currentHexPanel.Buildings[settlementIndex];
        }

        public void Init(IGameCallback gameCallback, ITileControlCallback tileCallback)
        {
            this._gameCallback = gameCallback;
            _tileCallback = tileCallback;
            CurrentGame = _games[0];
            _currentHexPanel.GameCallback = this._gameCallback;
            _currentHexPanel.TileCallback = _tileCallback;
        }


        private CatanHexPanel _currentHexPanel = null;



        public static readonly DependencyProperty GamesProperty = DependencyProperty.Register("Games", typeof(List<CatanGame>), typeof(CatanHexPanel), new PropertyMetadata(""));
        public static readonly DependencyProperty CurrentGameProperty = DependencyProperty.Register("CurrentGame", typeof(CatanGame), typeof(CatanHexPanel), new PropertyMetadata(null, OnCurrentGameChanged));

        private static void OnCurrentGameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameContainerCtrl container = d as GameContainerCtrl;
            container.SetGame(e.NewValue as CatanGame);
        }

        public TileCtrl PirateShipTile
        {
            get => _currentHexPanel.PirateShipTile;
            set => _currentHexPanel.PirateShipTile = value;
        }
        public TileCtrl BaronTile
        {
            get => _currentHexPanel.BaronTile;
            set => _currentHexPanel.BaronTile = value;
        }
        internal Task RotateTiles()
        {
            throw new NotImplementedException();
        }

        public TileCtrl[] TilesInIndexOrder => _currentHexPanel.TilesInIndexOrder;

        public void FlipAllAsync(TileOrientation orientation)
        {
            foreach (TileCtrl tile in _currentHexPanel.Tiles)
            {
                if (tile.RandomGoldEligible)
                {
                    tile.SetTileOrientationAsync(orientation, 0);
                }
            }

            foreach (Harbor harbor in _currentHexPanel.Harbors)
            {
                harbor.SetOrientationAsync(orientation, 0);
            }
        }

        //
        // 1. the dependency property SetGame gets called
        // 2. the change notification function OnCurrentGameChanged gets called
        // 3. that calls this function
        //
        //  please don't call this directly, as it will bypass any UI that has bound to the DP
        //
        private void SetGame(CatanGame newGame)
        {
            if (newGame.Control == null)
            {
                newGame.Control = (UserControl)Activator.CreateInstance(newGame.ControlType);
            }

            PanelGrid.Children.Clear();
            PanelGrid.Children.Add(newGame.Control);
            PanelGrid.UpdateLayout();
            _currentHexPanel = newGame.HexPanel;
            _currentHexPanel.GameCallback = _gameCallback;
            _currentHexPanel.TileCallback = _tileCallback;

            FlipAllAsync(TileOrientation.FaceDown);
        }

        public void SetCallbacks(IGameCallback gameCB, ITileControlCallback tileCb)
        {
            _gameCallback = gameCB;
            _tileCallback = tileCb;
            _currentHexPanel.GameCallback = _gameCallback;
            _currentHexPanel.TileCallback = _tileCallback;
        }

        public CatanGame CurrentGame
        {
            get => (CatanGame)GetValue(CurrentGameProperty);
            set => SetValue(CurrentGameProperty, value);
        }
        public List<CatanGame> Games
        {
            get => (List<CatanGame>)GetValue(GamesProperty);
            set => SetValue(GamesProperty, value);
        }

        public async Task SetRandomBoardSettings(RandomBoardSettings randomBoard)
        {
            await RandomizeCatanBoard(true, randomBoard);
        }

        public async Task RandomizeCatanBoard(bool placeBaron, RandomBoardSettings randomBoard = null)
        {
            _currentHexPanel.DesertTiles.Clear();
            _probabilities.Clear();

            for (int index = 0; index < _currentHexPanel.TileSets.Count; index++)
            {
                TileGroup tileGroup = _currentHexPanel.TileSets[index];
                if (tileGroup.Randomize == false)
                {
                    continue;
                }

                await AssignRandomTilesToTileGroup(index, randomBoard?.TileGroupToRandomListsDictionary[index.ToString()].TileList);
                await AssignRandomNumbersToTileGroup(index, randomBoard?.TileGroupToRandomListsDictionary[index.ToString()].NumberList);
                if (randomBoard == null)
                {
                    RandomBoardSettings = new RandomBoardSettings();
                    RandomBoardSettings.TileGroupToRandomListsDictionary[index.ToString()] = new RandomLists(tileGroup);
                }
                else
                {
                    RandomBoardSettings = randomBoard;
                }

            }

            LogList<int> harborList = GetRandomList(_currentHexPanel.Harbors.Count - 1);
            await _gameCallback.AddLogEntry(null, GameState.Unknown, CatanAction.AssignHarbors, true, LogType.Normal, -1, harborList);
            AssignRandomNumbersToHarbors(harborList);
            RandomBoardSettings.RandomHarborTypeList = harborList;
            if (placeBaron)
            {
                await InitialPlaceBaron();
            }

            foreach (BuildingCtrl building in AllBuildings)
            {
                int pips = 0;
                foreach (KeyValuePair<BuildingLocation, TileCtrl> kvp in building.BuildingToTileDictionary)
                {
                    pips += kvp.Value.Pips;

                }
                building.Pips = pips;
            }

        }

        public async Task AssignRandomTilesToTileGroup(int tileGroupIndex, List<int> randomTileList)
        {
            TileGroup tileGroup = _currentHexPanel.TileSets[tileGroupIndex];

            if (randomTileList == null)
            {
                randomTileList = GameContainerCtrl.GetRandomList(tileGroup.RandomTiles.Count - 1);
                await _gameCallback.AddLogEntry(null, GameState.Unknown, CatanAction.AssignRandomTiles, true, LogType.Normal, tileGroupIndex, randomTileList);
            }

            //
            //  every tile group gets to keep track of the list of random numbers that determine the resource type
            //  of the tiles
            tileGroup.RandomTileList = _currentHexPanel.RandomizeTiles(tileGroup, randomTileList);


        }

        public async Task AssignRandomNumbersToTileGroup(int tileGroupIndex, List<int> randomNumberList)
        {
            TileGroup tileGroup = _currentHexPanel.TileSets[tileGroupIndex];
            await AssignNumbers(tileGroup, randomNumberList);
        }

        public void AssignRandomNumbersToHarbors(List<int> harborList)
        {
            _currentHexPanel.ShuffleHarbors(harborList);

        }

        public async Task VisualShuffle()
        {
            _currentHexPanel.PirateVisibility = Visibility.Collapsed;
            _currentHexPanel.BaronVisibility = Visibility.Collapsed;
            await RandomizeCatanBoard(false);
            await FancyTileDistribution();
            await FancyHarborDistribution();
            await InitialPlaceBaron();


        }
        public async Task InitialPlaceBaron()
        {
            if (_currentHexPanel.DesertTiles.Count > 0)
            {
                BaronTile = _currentHexPanel.DesertTiles[0];
            }
            else
            {
                foreach (TileCtrl tile in _currentHexPanel.Tiles)
                {
                    if (tile.Number == 12)
                    {
                        BaronTile = tile;
                        break;
                    }
                }
            }

            await _gameCallback.AddLogEntry(null, GameState.Unknown, CatanAction.InitialAssignBaron, true, LogType.Normal, BaronTile.Index);

        }
        public async Task FancyTileDistribution()
        {


            List<Task> list = new List<Task>();
            double ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            int i = 0;
            foreach (TileGroup tileGroup in _currentHexPanel.TileSets)
            {

                if (tileGroup.Randomize == false)
                {
                    continue;
                }

                int middleIndex = (tileGroup.End - tileGroup.Start) / 2;
                TileCtrl centerTile = _currentHexPanel.TilesInIndexOrder[tileGroup.Start + middleIndex];

                int index = 0;
                foreach (TileCtrl t in tileGroup.RandomTiles)
                {
                    if (t == centerTile)
                    {
                        continue;
                    }

                    GeneralTransform gt = centerTile.HexGrid.TransformToVisual(t.HexGrid);
                    Point pt = gt.TransformPoint(new Point(0, 0));
                    Task task = t.AnimateMoveTask(pt, ms, i++ * ms);
                    Canvas.SetZIndex(t, 95 - index++);
                    list.Add(task);
                }

                //for (int index = tileGroup.Start; index <= tileGroup.End; index++)
                //{
                //    if (index == middleIndex) continue;
                //    TileCtrl t = _currentHexPanel.TilesInIndexOrder[index];
                //    GeneralTransform gt = centerTile.HexGrid.TransformToVisual(t.HexGrid);
                //    Point pt = gt.TransformPoint(new Point(0, 0));
                //    Task task = t.AnimateMoveTask(pt, ms, i++ * ms);
                //    Canvas.SetZIndex(t, 95 - index);
                //    list.Add(task);
                //}

            }
            await Task.WhenAll(list);
            list.Clear();

            Random r = new Random(DateTime.Now.Millisecond);
            foreach (TileGroup tileGroup in _currentHexPanel.TileSets)
            {
                if (tileGroup.Randomize == false)
                {
                    continue;
                }

                foreach (TileCtrl t in tileGroup.RandomTiles)
                {
                    Task task = t.RotateTask(r.Next(1, 5) * 360, MainPage.GetAnimationSpeed(AnimationSpeed.Fast));
                    list.Add(task);
                }
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;

            foreach (TileGroup tileGroup in _currentHexPanel.TileSets)
            {
                if (tileGroup.Randomize == false)
                {
                    continue;
                }

                foreach (TileCtrl t in tileGroup.RandomTiles)
                {

                    t.ResetTileRotation();
                }
            }

            i = 0;
            foreach (TileGroup tileGroup in _currentHexPanel.TileSets)
            {
                if (tileGroup.Randomize == false)
                {
                    continue;
                }

                foreach (TileCtrl t in tileGroup.RandomTiles)
                {
                    Task task = t.AnimateMoveTask(new Point(0, 0), ms, i * ms);
                    i++;
                    list.Add(task);
                }
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            foreach (TileCtrl t in _currentHexPanel.TilesInIndexOrder)
            {
                if (t.ResourceType == ResourceType.Sea)
                {
                    continue;
                }

                TileOrientation orientation = TileOrientation.FaceUp;
                Task task = t.SetTileOrientation(orientation, MainPage.GetAnimationSpeed(AnimationSpeed.Fast), i * MainPage.GetAnimationSpeed(AnimationSpeed.Fast));
                i++;
                list.Add(task);
            }

            await Task.WhenAll(list);
            foreach (TileCtrl tile in _currentHexPanel.Tiles)
            {
                tile.ZIndex = -1;
            }

        }
        public async Task FancyHarborDistribution()
        {
            List<Task> list = new List<Task>();
            double ms = MainPage.GetAnimationSpeed(AnimationSpeed.Normal);
            int i = 0;
            int middleIndex = _currentHexPanel.Tiles.Count / 2;
            TileCtrl animationMiddle = _currentHexPanel.TilesInIndexOrder[middleIndex];
            foreach (Harbor harbor in _currentHexPanel.Harbors)
            {
                GeneralTransform gt = animationMiddle.TransformToVisual(harbor);
                Point pt = gt.TransformPoint(new Point(harbor.Width, harbor.Height));
                Task task = harbor.AnimateMoveTask(pt, ms, i * ms);
                list.Add(task);
            }



            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            Random r = new Random(DateTime.Now.Millisecond);
            ms = MainPage.GetAnimationSpeed(AnimationSpeed.Normal);

            foreach (Harbor h in _currentHexPanel.Harbors)
            {
                Task task = h.RotateTask(r.Next(1, 10) * 360, ms);
                list.Add(task);
            }

            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            ms = MainPage.GetAnimationSpeed(AnimationSpeed.Fast);
            foreach (Harbor h in _currentHexPanel.Harbors)
            {
                Task task = h.AnimateMoveTask(new Point(0, 0), ms, i * ms);
                i++;
                list.Add(task);

            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            ms = ms = MainPage.GetAnimationSpeed(AnimationSpeed.Normal);

            foreach (Harbor h in _currentHexPanel.Harbors)
            {
                Task task = h.SetOrientation(TileOrientation.FaceUp, ms, i * ms);
                i++;
                list.Add(task);
            }

            await Task.WhenAll(list);

        }

        public static LogList<int> GetRandomList(int max)
        {
            MersenneTwister twist = new MersenneTwister();
            LogList<int> randomIndeces = new LogList<int>();

            for (int i = 0; i <= max; i++)
            {
                randomIndeces.Add(i);

            }

            int temp = 0;
            for (int n = 0; n <= max; n++)
            {
                int k = twist.Next(n + 1);
                temp = randomIndeces[n];
                randomIndeces[n] = randomIndeces[k];
                randomIndeces[k] = temp;
            }

            return randomIndeces;

        }
        #region properties needed by MainPage

        public List<TileCtrl> AllTiles => _currentHexPanel.Tiles;
        public List<RoadCtrl> AllRoads => _currentHexPanel.Roads;

        public List<BuildingCtrl> AllBuildings => _currentHexPanel.Buildings;

        #endregion

        #region Random Number assignment

        public async Task AssignNumbers(TileGroup tileGroup, List<int> randomList)
        {
            if (tileGroup.Randomize == false)
            {
                return;
            }

            bool valid = false;
            int iterations = 0;
            List<int> RandomNumberSequence = randomList;
            do
            {

                int numberSeqCount = 0;

                if (RandomNumberSequence == null)
                {
                    RandomNumberSequence = GetRandomList(tileGroup.RandomTiles.Count - 1);
                }

                for (int i = 0; i < tileGroup.RandomTiles.Count; i++)
                {

                    TileCtrl t = tileGroup.RandomTiles[i];
                    t.HasBaron = false;
                    if (t.ResourceType != ResourceType.Desert)
                    {
                        int number = -1;
                        do
                        {
                            number = tileGroup.ValidNumbers[RandomNumberSequence[numberSeqCount++]];
                        } while (number == 7); // skip over all deserts

                        t.Number = number;
                    }
                    else
                    {
                        t.Number = 7;
                        if (_currentHexPanel.DesertTiles.Contains(t) == false)
                        {
                            _currentHexPanel.DesertTiles.Add(t);
                        }
                    }

                }
                iterations++;



                valid = IsValidNumberLayout();
                if (!valid)
                {
                    //
                    //  if this is a saved game, it will be valid.  if we got here, it means it is a new game - get better numbers!
                    RandomNumberSequence = null;

                }

                if (iterations > 1000)
                {
                    if (await StaticHelpers.AskUserYesNoQuestion("Tried to find a good number sequene 1000 times.  Continue?", "Continue", "Cancel") == true)
                    {
                        iterations = 0;
                    }
                    else
                    {
                        break;
                    }
                }

            } while (!valid);

            //
            //  when we get here, the random number lists are valid
            tileGroup.RandomNumbersList = RandomNumberSequence;


            //if (iterations > 50)
            //    this.TraceMessage($"tried {iterations} times to find a valid number sequence");

            await _gameCallback.AddLogEntry(null, GameState.Unknown, CatanAction.RandomizeTiles, true, LogType.Normal, _currentHexPanel.TileSets.IndexOf(tileGroup), RandomNumberSequence);
        }


        internal void Reset()
        {
            _currentHexPanel.Reset();
            _probabilities.Clear();
            RandomBoardSettings = new RandomBoardSettings();
            _ = ResetRandomGoldTiles();

        }



        private bool NextLowerRightIsRed(int row, int col, List<List<TileCtrl>> visualTiles)
        {
            if (col == visualTiles.Count - 1)
            {
                return false; // last column has no Next column
            }

            bool ret = false;

            bool beforeMiddle = (col < visualTiles.Count / 2);


            int number;
            if (beforeMiddle)
            {
                number = visualTiles.ElementAt(col + 1).ElementAt(row + 1).Number;
                ret = IsRed(number);
                return ret;
            }

            if (row > visualTiles.ElementAt(col + 1).Count - 1)
            {
                return false;
            }
            // we are at or past the middle
            number = visualTiles.ElementAt(col + 1).ElementAt(row).Number;   // row + 1 is always valid after the middle
            ret = IsRed(number);
            return ret;
        }

        private bool NextUpperRightIsRed(int row, int col, List<List<TileCtrl>> visualTiles)
        {
            if (col == visualTiles.Count - 1)
            {
                return false; // last column has no Next column
            }

            bool ret = false;

            bool beforeMiddle = (col < visualTiles.Count / 2);


            int number;
            if (beforeMiddle)
            {
                number = visualTiles.ElementAt(col + 1).ElementAt(row).Number;
                ret = IsRed(number);
                return ret;
            }

            if (row == 0)
            {
                return false;
            }

            // we are at or past the middle
            number = visualTiles.ElementAt(col + 1).ElementAt(row - 1).Number;   // row + 1 is always valid after the middle
            ret = IsRed(number);
            return ret;
        }
        private bool AboveTileIsRed(int row, int col, List<List<TileCtrl>> visualTiles)
        {
            if (row == 0)
            {
                return false;
            }

            int number = visualTiles.ElementAt(col).ElementAt(row - 1).Number;
            return IsRed(number);

        }
        private bool BelowTileIsRed(int row, int col, List<List<TileCtrl>> visualTiles)
        {
            if (row == visualTiles.ElementAt(col).Count - 1)
            {
                return false;
            }

            int number = visualTiles.ElementAt(col).ElementAt(row + 1).Number;
            return IsRed(number);

        }
        private bool IsValidNumberLayout()
        {
            List<List<TileCtrl>> visualTiles = _currentHexPanel.VisualTiles;

            //
            //  Need to check the last column to see if one red tile is below another
            for (int col = 0; col < visualTiles.Count; col++)
            {
                for (int row = 0; row < visualTiles.ElementAt(col).Count; row++)
                {
                    TileCtrl tile = visualTiles.ElementAt(col).ElementAt(row);
                    int number = tile.Number;
                    if (tile.ResourceType == ResourceType.GoldMine)
                    {
                        if (number == 8 || number == 6)
                        {
                            // this.TraceMessage($"Rejected layout because Gold had a {number}");
                            return false;
                        }
                    }
                    if (IsRed(number))
                    {


                        if (NextLowerRightIsRed(row, col, visualTiles))
                        {
                            return false;
                        }

                        if (NextUpperRightIsRed(row, col, visualTiles))
                        {
                            return false;
                        }

                        if (BelowTileIsRed(row, col, visualTiles))
                        {
                            return false;
                        }

                        // shouldn't need the below, as they are next to a tile that is above
                        //if (AboveTileIsRed(row, col, visualTiles))
                        //    return false;                        
                        //if (PreviousLowerLeftIsRed(row, col, visualTiles))
                        //    return false;
                        //if (PreviousUpperLeftIsRed(row, col, visualTiles))
                        //    return false;
                    }
                }
            }

            return true;

        }

        private bool IsRed(int i)
        {
            if (i == 6 || i == 8)
            {
                return true;
            }

            return false;
        }


        #endregion

        public RoadCtrl GetRoadAt(TileCtrl tile, RoadLocation roadLocation)
        {
            if (tile == null)
            {
                return null;
            }

            RoadKey key = new RoadKey(tile, roadLocation);
            _currentHexPanel.RoadKeyToRoadDictionary.TryGetValue(key, out RoadCtrl road);
            return road;
        }

        internal void CalculateAdjacentBuildings()
        {
            _currentHexPanel.FindAdjacentRoads();
        }

        internal bool GetBuilding(TileCtrl tile, BuildingLocation location, out BuildingCtrl control)
        {
            BuildingKey key = new BuildingKey(tile, location);
            return _currentHexPanel.BuildingKeyToBuildingCtrlDictionary.TryGetValue(key, out control);
        }


        internal void OnTest()
        {
            /*
            var cts = new CancellationTokenSource();
            cts.CancelAfter(TimeSpan.FromSeconds(30));

            var httpClient = new HttpClient();
            var resourceUri = new Uri("localhost:8080/roll/");

            try
            {
                HttpResponseMessage response = await httpClient.GetAsync(resourceUri, cts.Token);
            }
            catch (TaskCanceledException ex)
            {
                // Handle request being canceled due to timeout.
            }
            catch (HttpRequestException ex)
            {
                // Handle other possible exceptions.
            }
            */






        }

        public bool HasIslands => _currentHexPanel.HasIslands;


        public Island GetIsland(TileCtrl tile)
        {
            _currentHexPanel.TileToIslandDictionary.TryGetValue(tile, out Island island);
            return island; // can be null;            
        }

        /// <summary>
        ///     given a number (say 6) rutn the List of tiles that have that number
        /// </summary>
        /// <param name="val"></param>
        /// <returns></returns>
        readonly List<TileCtrl>[] _TilesWithNumbers = new List<TileCtrl>[13];
        internal IReadOnlyCollection<TileCtrl> GetTilesWithNumber(int val)
        {
            if (_TilesWithNumbers[val] == null)
            {
                _TilesWithNumbers[val] = new List<TileCtrl>();
                foreach (TileCtrl t in CurrentGame.Tiles)
                {

                    if (t.Number == val)

                    {
                        _TilesWithNumbers[val].Add(t);

                    }
                }
            }
            return _TilesWithNumbers[val];
        }

        /**
        *  flip tiles to facedown, change them to temp gold, and then flip them back up
        *  
        */

        public async Task SetRandomTilesToGold(IEnumerable<int> indeces)
        {
            if (indeces.Count() == 0) return;

            List<Task> taskList = new List<Task>();
            foreach (var idx in indeces)
            {

                TilesInIndexOrder[idx].SetTileOrientation(TileOrientation.FaceDown, taskList, MainPage.GetAnimationSpeed(AnimationSpeed.Fast));

            }


            await Task.WhenAll(taskList);
            taskList.Clear();
            foreach (var idx in indeces)
            {
                TilesInIndexOrder[idx].TemporarilyGold = true;
                TilesInIndexOrder[idx].SetTileOrientation(TileOrientation.FaceUp, taskList, MainPage.GetAnimationSpeed(AnimationSpeed.Normal));
            }

            await Task.WhenAll(taskList);
        }

        /// <summary>
        ///     This will go through all of the tiles and if they are temporarily gold
        ///     1. flip them face down in parallel
        ///     2. turn off the temp gold flag
        ///     3. flip them factup in parallel
        ///     
        ///     this has to be done in two loops because turning temp gold off has visual impact you shouldn't see with the tile faceup
        /// </summary>
        /// <returns></returns>
        public async Task ResetRandomGoldTiles()
        {
            List<Task> taskList = new List<Task>();
            foreach (var tile in TilesInIndexOrder)
            {
                if (tile.TemporarilyGold)
                {
                    tile.SetTileOrientation(TileOrientation.FaceDown, taskList, MainPage.GetAnimationSpeed(AnimationSpeed.Fast));
                }
            }

            if (taskList.Count == 0) return;
            await Task.WhenAll(taskList);
            taskList.Clear();
            foreach (var tile in TilesInIndexOrder)
            {
                if (tile.TemporarilyGold)
                {
                    tile.TemporarilyGold = false;
                    tile.SetTileOrientation(TileOrientation.FaceUp, taskList, MainPage.GetAnimationSpeed(AnimationSpeed.Fast));
                }
            }

            await Task.WhenAll(taskList);

        }

        readonly Random _randomForGold = new Random(DateTime.Now.Millisecond);
        /// <summary>
        ///     the "count < 5" clause is becuase I test with a lot of gold mines to see if the algo works.
        /// </summary>
        /// <param name="count"></param>
        /// <param name="exclude"></param>
        /// <returns></returns>
        public List<int> PickRandomTilesToBeGold(int count, List<int> exclude = null)
        {
            TileCtrl tile;
            List<int> randomTileIndices = new List<int>();
            for (; ; )
            {
                tile = TilesInIndexOrder[_randomForGold.Next(TilesInIndexOrder.Length)];
                if (tile.ResourceType == ResourceType.Desert)
                {
                    continue;
                }
                if (tile.ResourceType == ResourceType.GoldMine && count < 5)
                {
                    continue;
                }
                if (tile.TemporarilyGold)
                {
                    //  don't pick one that is already gold                    
                    //
                    continue;
                }
                if (randomTileIndices.Contains(tile.Index) && count < 5)
                {
                    //  don't pick the same one twice
                    //
                    continue;
                }
                if (exclude != null && count < 5 && exclude.Contains(tile.Index))
                {
                    //
                    //  don't pick anything that is excluded -- this lets us not pick the same gold tile twice in a row
                    continue;
                }

                randomTileIndices.Add(tile.Index);
                if (randomTileIndices.Count == count)
                {
                    return randomTileIndices;
                }

            }

        }
        public List<int> GetCurrentRandomGoldTiles()
        {
            List<int> ret = new List<int>();
            foreach (var tile in TilesInIndexOrder)
            {
                if (tile.TemporarilyGold)
                {
                    ret.Add(tile.Index);
                }
            }
            return ret;
        }
    }
}
