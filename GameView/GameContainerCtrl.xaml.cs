
using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Diagnostics;
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


    public class CatanGameCtrl : ICatanGameData
    {

        public Type ControlType { get; set; } = typeof(RegularGameCtrl);
        
        public string Description { get; set; } = "Regular";
        public int Index { get; set; } = -1;
        public CatanGames CatanGame { get; set; } = CatanGames.Regular;
        public CatanGameCtrl(Type type, CatanGames gameType, string s, int idx)
        {
            ControlType = type;
            Description = s;
            Index = idx;
            CatanGame = gameType;

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
        readonly List<CatanGameCtrl> _games = new List<CatanGameCtrl>()
        {
            new CatanGameCtrl (typeof (RegularGameCtrl), CatanGames.Regular,  "Regular", 0),
            new CatanGameCtrl (typeof (ExpansionCtrl), CatanGames.Expansion, "Expansion (5-6 Players)", 1),
            new CatanGameCtrl (typeof (Seafarers4PlayerCtrl), CatanGames.Seafarers, "Seafarers (4 Player)",2),
            //new CatanGameCtrl (typeof (FourIsland3Ctrl), "Four Islands (3 Player)", 3),

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
                CatanGameCtrl game = _games[index];
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



        public static readonly DependencyProperty GamesProperty = DependencyProperty.Register("Games", typeof(List<CatanGameCtrl>), typeof(CatanHexPanel), new PropertyMetadata(""));
        public static readonly DependencyProperty CurrentGameProperty = DependencyProperty.Register("CurrentGame", typeof(CatanGameCtrl), typeof(CatanHexPanel), new PropertyMetadata(null, OnCurrentGameChanged));

        private static void OnCurrentGameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            GameContainerCtrl container = d as GameContainerCtrl;
            container.SetGame(e.NewValue as CatanGameCtrl);
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
                    tile.TileOrientation = orientation;
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
        private void SetGame(CatanGameCtrl newGame)
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

        public CatanGameCtrl CurrentGame
        {
            get => (CatanGameCtrl)GetValue(CurrentGameProperty);
            set => SetValue(CurrentGameProperty, value);
        }
        public List<CatanGameCtrl> Games
        {
            get => (List<CatanGameCtrl>)GetValue(GamesProperty);
            set => SetValue(GamesProperty, value);
        }
        /// <summary>
        ///     Returns a *valid* Random board with no side affects. I random board looks like this
        ///     {
        ///    "TileGroupToRandomListsDictionary": {
        ///        "0": {
        ///            "TileList": [
        ///                <Random List of Ints, one for each Tile>
        ///            ],
        ///            "NumberList": [
        ///             <Random List of Ints, one for each Tile -- there are Catan rules that this list must follow such as "No two Red numbers next to each other">
        ///            ]
        ///    }
        ///    },
        ///    "RandomHarborTypeList": [
        ///      <Random List of Ints, one for each Harbor>
        ///    ]
        ///  }
        /// eg there is *one* HarborList per board and then each "TileGroup" (stored in a Dictionary<string, TileGroup> so that System.Text.Json can serialize it) has
        /// a collection of Tiles, each with a list on ints for both the Numbers and the Tiles, which give an index into a fixed array used to randomize the board.
        /// </summary>
        /// <returns></returns>
        public RandomBoardSettings GetRandomBoard()
        {
            var rbs = new RandomBoardSettings();
            //
            //  create a temporary board to do all our calculations.  We need to do this because they algo's here
            //  rely on the visual layout of the tiles to determine if the number layout is valid.  We want to do this
            //  off screen w/o impacting what the user is seeing in the game.



            CatanGameCtrl game = CurrentGame;




            for (int index = 0; index < game.HexPanel.TileSets.Count; index++)
            {
                TileGroup tileGroup = game.HexPanel.TileSets[index];
                var randomList = new RandomLists()
                {
                    TileList = GetRandomList(tileGroup.TilesToRandomize.Count - 1),       // RandomTiles is the list of tiles that should be randomized                    

                };
                tileGroup.Reset();

                tileGroup.TileAndNumberLists = randomList;
             //   this.TraceMessage($"Tiles Before Shuffle: {DumpTileList(tileGroup.TilesToRandomize)}");
                game.HexPanel.ShuffleTileGroup(tileGroup, randomList.TileList); // put the Tiles where they go
            //    this.TraceMessage($"Tiles After Shuffle: {DumpTileList(tileGroup.TilesToRandomize)}");
                randomList.NumberList = RandomAndValidNumberList(tileGroup); // since the tiles are where they go, we can now assign numbers

                rbs.TileGroupToRandomListsDictionary[index.ToString()] = randomList; // this is just the 
              //  this.TraceMessage($"RBS: {rbs}");
            }

            rbs.RandomHarborTypeList = GetRandomList(_currentHexPanel.Harbors.Count - 1);
            //this.TraceMessage($"Tiles: {CatanProxy.Serialize(rbs.TileGroupToRandomListsDictionary["0"].TileList)} Numbers: {CatanProxy.Serialize(rbs.TileGroupToRandomListsDictionary["0"].NumberList)}");
            return rbs;
        }
        private string DumpTileList(List<TileCtrl> list)
        {
            string s = "";
            foreach (var t in list)
            {
                s += $"{t},";
            }
            return s;
        }
        /// <summary>
        ///     Given a tileGroup (e.g. a set of Tiles completely surrounded by water (or a continuguous set of tiles like the standard board)
        ///     return a list of numbers that represent the random numbers for that TileGroup
        /// </summary>
        /// <param name="tileGroup"></param>
        /// <returns></returns>
        private List<int> RandomAndValidNumberList(TileGroup tileGroup)
        {


            bool valid = false;
            int iterations = 0;
            List<int> randomNumberSequence = new List<int>();
            
            while (!valid)
            {
                
                randomNumberSequence = GetRandomList(tileGroup.StartingTileNumbers.Count - 1); // this is the index *into* TileGroup.StartingTileNumbers
                _currentHexPanel.DesertTiles.Clear();
                _currentHexPanel.DesertTiles.AddRange(tileGroup.TilesToRandomize.FindAll(t => t.ResourceType == ResourceType.Desert));
                int startingDesertIndex = 0;
                int randomDesertIndex = 0;
                for (int i = 0; i < _currentHexPanel.DesertTiles.Count; i++)
                {
                    //
                    //  the index into the TileGroup.StartingTileNumbers that has the desert number (7)
                    startingDesertIndex = tileGroup.StartingTileNumbers.IndexOf(7, startingDesertIndex);

                    //
                    //  find the index in the random array that has that index
                    randomDesertIndex = randomNumberSequence.IndexOf(startingDesertIndex, randomDesertIndex);

                    // where is the Desert in the Randomtiles?

                    TileCtrl desertTile = _currentHexPanel.DesertTiles[i];
                    int desertTileIndex = tileGroup.TilesToRandomize.IndexOf(desertTile);

                    randomNumberSequence.Swap(desertTileIndex, randomDesertIndex);

                }

                for (int i = 0; i < tileGroup.TilesToRandomize.Count; i++)
                {

                    TileCtrl t = tileGroup.TilesToRandomize[i];
                    t.HasBaron = false;
                    int number = tileGroup.StartingTileNumbers[randomNumberSequence[i]];
                    if (number == 7)
                    {
                        Debug.Assert(t.ResourceType == ResourceType.Desert);

                    }
                    else
                    {
                        Debug.Assert(t.ResourceType != ResourceType.Desert);
                    }

                    t.Number = number;


                }
                iterations++;
                valid = IsValidNumberLayout();

            }
            
          //  this.TraceMessage($"Tiles: {DumpTileList(tileGroup.TilesToRandomize)} Numbers: {CatanProxy.Serialize(randomNumberSequence)}");
            return randomNumberSequence;
        }



        public async Task SetRandomCatanBoard(bool placeBaron, RandomBoardSettings randomBoard = null)
        {

            _currentHexPanel.DesertTiles.Clear();
            _probabilities.Clear();
            
            

            if (randomBoard == null)
            {
                randomBoard = GetRandomBoard();
            }
            //
            //  set the property
            RandomBoardSettings = randomBoard;


            for (int index = 0; index < _currentHexPanel.TileSets.Count; index++)
            {
                TileGroup tileGroup = _currentHexPanel.TileSets[index];
                if (tileGroup.Randomize == false) // if the set is a collection of non-playable tiles such as Sea tiles
                {
                    continue;
                }
                string key = index.ToString();
                for (int i = 0; i < tileGroup.TilesToRandomize.Count; i++)
                {
                    TileCtrl tile = tileGroup.TilesToRandomize[i];
                    int randomIndex = randomBoard.TileGroupToRandomListsDictionary[key].TileList[i];
                    tile.ResourceType = tileGroup.StartingResourceTypes[randomIndex];
                    randomIndex = randomBoard.TileGroupToRandomListsDictionary[key].NumberList[i];
                    tile.Number = tileGroup.StartingTileNumbers[randomIndex];
                }

                _currentHexPanel.DesertTiles.AddRange(tileGroup.TilesToRandomize.FindAll(t => t.ResourceType == ResourceType.Desert));

            }
            for (int i = 0; i < CurrentGame.HexPanel.Harbors.Count; i++)
            {
                CurrentGame.HexPanel.Harbors[i].HarborType = CurrentGame.HexPanel.StartingHarborTypes[randomBoard.RandomHarborTypeList[i]];
            }

            if (placeBaron)
            {
                await InitialPlaceBaron();
            }
            //
            //  update the Pips for each building as the numbers have changed
            foreach (BuildingCtrl building in AllBuildings)
            {
                int pips = 0;
                foreach (KeyValuePair<BuildingLocation, TileCtrl> kvp in building.BuildingToTileDictionary)
                {
                    pips += kvp.Value.Pips;

                }
                building.Pips = pips;
            }

            await _gameCallback.AddLogEntry(null, GameState.Unknown, CatanAction.RandomizeBoard, true, LogType.Normal, -1, this.RandomBoardSettings);

        }



        public async Task VisualShuffle(RandomBoardSettings rbs = null)
        {
            _currentHexPanel.PirateVisibility = Visibility.Collapsed;
            _currentHexPanel.BaronVisibility = Visibility.Collapsed;
            await SetRandomCatanBoard(false, rbs);
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
                    //
                    // if there are no deserts, the baron goes on a 12
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
                foreach (TileCtrl t in tileGroup.TilesToRandomize)
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

                foreach (TileCtrl t in tileGroup.TilesToRandomize)
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

                foreach (TileCtrl t in tileGroup.TilesToRandomize)
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

                foreach (TileCtrl t in tileGroup.TilesToRandomize)
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

        /// <summary>
        ///     given a randomList
        ///     1. make sure it is a valid list
        ///     2. assign the numbers to the tiles
        /// </summary>
        /// <param name="tileGroup"></param>
        /// <param name="randomList"></param>
        public void AssignNumbersToTileGroup(TileGroup tileGroup)
        {
            if (tileGroup.Randomize == false)
            {
                return;
            }

            _currentHexPanel.DesertTiles.Clear();

            for (int i = 0; i < tileGroup.TilesToRandomize.Count; i++)
            {

                TileCtrl t = tileGroup.TilesToRandomize[i];
                t.HasBaron = false;
                if (t.ResourceType == ResourceType.Desert)
                {
                    t.Number = 7;
                    if (_currentHexPanel.DesertTiles.Contains(t) == false) // we keep track of which Tile the desert is and here is where we set that.
                    {
                        _currentHexPanel.DesertTiles.Add(t);
                    }

                }
                else
                {
                    t.Number = tileGroup.StartingTileNumbers[tileGroup.TileAndNumberLists.NumberList[i]]; // StartingTileNumbers is the Array set at runtime that has all the number for this board.                        
                }

            }


            if (!IsValidNumberLayout())
            {
                this.TraceMessage($"Invalid! Tiles: {tileGroup.TileAndNumberLists}");
                throw new ArgumentException("You passed in an invalid Random list for the Catan Numbers.  Call GetRandomBoard() for your random settings!");

            }


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
        public List<int> CurrentRandomGoldTiles
        {
            get
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
}
