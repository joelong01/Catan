using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class GameViewControl : UserControl
    {

        GameType _gameType = GameType.Regular;


        private string _currentGameName = "";
        public GameViewControl()
        {
            this.InitializeComponent();

        }


        public int Rows
        {
            get
            {

                return (int)GetValue(RowsProperty);

            }
            set
            {
                if ((int)GetValue(RowsProperty) != value)
                    SetValue(RowsProperty, value);
            }
        }
        public int Columns
        {
            get
            {

                return (int)GetValue(ColumnsProperty);

            }
            set
            {
                if ((int)GetValue(ColumnsProperty) != value)
                    SetValue(ColumnsProperty, value);
            }
        }


        public bool EnableInput
        {
            get
            {
                return (bool)GetValue(EnableInputProperty);
            }
            set
            {
                if ((bool)GetValue(EnableInputProperty) != value)
                    SetValue(EnableInputProperty, value);
            }
        }
        // public string GameName { get; set; } = "Seafarers Game 1";
        public string NumberOfPlayers
        {
            get
            {
                return (string)GetValue(NumberOfPlayersProperty);
            }
            set
            {
                if ((string)GetValue(NumberOfPlayersProperty) != value)
                    SetValue(NumberOfPlayersProperty, value);
            }
        }

        public ObservableCollection<string> SavedGames
        {
            get
            {
                return _savedGameNames;
            }

            set
            {
                this.TraceMessage("If saved games don't show up it is NotifyPropertyChanged here...");
                _savedGameNames = value;
            }
        }
        public string SerializeGroupTilesAndHarbors()
        {
            string s = "";
            string nl = StaticHelpers.lineSeperator;
            string sep = StaticHelpers.listSeperator;
            for (int i = 0; i < _currentGame.TileGroups.Count; i++)
            {
                s += $"Group{i}Tiles={StaticHelpers.SerializeList<int>(_currentGame.TileGroups[i].RandomResourceTypeList, sep)}{nl}";
                s += $"Group{i}Harbors={StaticHelpers.SerializeList<int>(_currentGame.TileGroups[i].RandomHarborTypeList, sep)}{nl}";

            }

            return s;
        }

        public void RandomizeNumbers()
        {


        }

        private bool IsRed(int i)
        {
            if (i == 6 || i == 8)
                return true;

            return false;
        }

        private bool IsInvisible(int i)
        {
            if (i == 0 || i == 7)
                return true;
            return false;
        }

        private bool PreviousUpperLeftIsRed(int row, int col, List<List<TileCtrl>> visualTiles)
        {
            if (col == 0) return false;

            bool ret = false;

            bool beforeMiddle = (col < (int)(Columns / 2));
            bool atMiddle;

            if (Columns % 2 == 1)
                atMiddle = (col == ((int)(Columns / 2)));
            else
                atMiddle = (col == ((int)(Columns / 2) + 1));


            int number;
            if (beforeMiddle || atMiddle)
            {
                if (row == 0) return false;

                number = visualTiles.ElementAt(col - 1).ElementAt(row - 1).Number;
                ret = IsRed(number);
                return ret;
            }

            // we are after the middle
            number = visualTiles.ElementAt(col - 1).ElementAt(row).Number;
            ret = IsRed(number);
            return ret;
        }

        private bool PreviousLowerLeftIsRed(int row, int col, List<List<TileCtrl>> visualTiles)
        {
            if (col == 0) return false;

            bool ret = false;

            bool beforeMiddle = (col < (int)(Columns / 2));
            bool atMiddle;

            if (Columns % 2 == 1)
                atMiddle = (col == ((int)(Columns / 2)));
            else
                atMiddle = (col == ((int)(Columns / 2) + 1));


            int number;
            if (beforeMiddle || atMiddle)
            {
                if (row == visualTiles.ElementAt(col).Count - 1) return false;  // if it is the last tile before or at the middle, there is no lower left

                number = visualTiles.ElementAt(col - 1).ElementAt(row).Number;
                ret = IsRed(number);
                return ret;
            }

            // we are after the middle
            number = visualTiles.ElementAt(col - 1).ElementAt(row + 1).Number;   // row + 1 is always valid after the middle
            ret = IsRed(number);
            return ret;
        }

        private TileCtrl PreviousUpperLeft(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = _currentGame.VisualLayout();
            if (col == 0) return null;

            bool beforeMiddle = (col < (int)(Columns / 2));
            bool atMiddle;

            if (Columns % 2 == 1)
                atMiddle = (col == ((int)(Columns / 2)));
            else
                atMiddle = (col == ((int)(Columns / 2) + 1));



            if (beforeMiddle || atMiddle)
            {
                if (row == 0) return null;

                return visualTiles.ElementAt(col - 1).ElementAt(row - 1);


            }

            // we are after the middle
            return visualTiles.ElementAt(col - 1).ElementAt(row);
        }

        private TileCtrl PreviousLowerLeft(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = _currentGame.VisualLayout();
            if (col == 0) return null;

            bool beforeMiddle = (col < (int)(Columns / 2));
            bool atMiddle;

            if (Columns % 2 == 1)
                atMiddle = (col == ((int)(Columns / 2)));
            else
                atMiddle = (col == ((int)(Columns / 2) + 1));

            if (beforeMiddle || atMiddle)
            {
                if (row == visualTiles.ElementAt(col).Count - 1) return null;  // if it is the last tile before or at the middle, there is no lower left

                return visualTiles.ElementAt(col - 1).ElementAt(row);

            }

            // we are after the middle
            return visualTiles.ElementAt(col - 1).ElementAt(row + 1);   // row + 1 is always valid after the middle

        }

        private TileCtrl NextLowerRight(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = _currentGame.VisualLayout();

            if (col == visualTiles.Count - 1) return null;

            bool beforeMiddle = (col < (int)(Columns / 2));
            if (beforeMiddle)
            {
                return visualTiles.ElementAt(col + 1).ElementAt(row + 1);
            }
            if (row > visualTiles.ElementAt(col + 1).Count - 1) return null;

            return visualTiles.ElementAt(col + 1).ElementAt(row);
        }

        private TileCtrl NextUpperRight(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = _currentGame.VisualLayout();
            if (col == visualTiles.Count - 1) return null; // last column has no Next column
            bool beforeMiddle = (col < (int)(Columns / 2));
            if (beforeMiddle)
            {
                return visualTiles.ElementAt(col + 1).ElementAt(row);

            }

            if (row == 0) return null;

            // we are at or past the middle
            return visualTiles.ElementAt(col + 1).ElementAt(row - 1);   // row + 1 is always valid after the middle

        }
        private TileCtrl AboveTile(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = _currentGame.VisualLayout();
            if (row == 0) return null;

            return visualTiles.ElementAt(col).ElementAt(row - 1);

        }
        private TileCtrl BelowTile(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = _currentGame.VisualLayout();
            if (row == visualTiles.ElementAt(col).Count - 1) return null;

            return visualTiles.ElementAt(col).ElementAt(row + 1);

        }

        private bool NextLowerRightIsRed(int row, int col, List<List<TileCtrl>> visualTiles)
        {
            if (col == visualTiles.Count - 1) return false; // last column has no Next column

            bool ret = false;

            bool beforeMiddle = (col < (int)(Columns / 2));


            int number;
            if (beforeMiddle)
            {
                number = visualTiles.ElementAt(col + 1).ElementAt(row + 1).Number;
                ret = IsRed(number);
                return ret;
            }

            if (row > visualTiles.ElementAt(col + 1).Count - 1) return false;
            // we are at or past the middle
            number = visualTiles.ElementAt(col + 1).ElementAt(row).Number;   // row + 1 is always valid after the middle
            ret = IsRed(number);
            return ret;
        }

        private bool NextUpperRightIsRed(int row, int col, List<List<TileCtrl>> visualTiles)
        {
            if (col == visualTiles.Count - 1) return false; // last column has no Next column

            bool ret = false;

            bool beforeMiddle = (col < (int)(Columns / 2));


            int number;
            if (beforeMiddle)
            {
                number = visualTiles.ElementAt(col + 1).ElementAt(row).Number;
                ret = IsRed(number);
                return ret;
            }

            if (row == 0) return false;

            // we are at or past the middle
            number = visualTiles.ElementAt(col + 1).ElementAt(row - 1).Number;   // row + 1 is always valid after the middle
            ret = IsRed(number);
            return ret;
        }
        private bool AboveTileIsRed(int row, int col, List<List<TileCtrl>> visualTiles)
        {
            if (row == 0) return false;

            int number = visualTiles.ElementAt(col).ElementAt(row - 1).Number;
            return IsRed(number);

        }
        private bool BelowTileIsRed(int row, int col, List<List<TileCtrl>> visualTiles)
        {
            if (row == visualTiles.ElementAt(col).Count - 1) return false;

            int number = visualTiles.ElementAt(col).ElementAt(row + 1).Number;
            return IsRed(number);

        }
        private bool IsValidNumberLayout(OldCatanGame game)
        {
            List<List<TileCtrl>> visualTiles = game.VisualLayout();

            //
            //  Need to check the last column to see if one red tile is below another
            for (int col = 0; col < Columns; col++)
            {
                for (int row = 0; row < visualTiles.ElementAt(col).Count; row++)
                {
                    int number = visualTiles.ElementAt(col).ElementAt(row).Number;
                    if (IsRed(number))
                    {


                        if (NextLowerRightIsRed(row, col, visualTiles))
                            return false;
                        if (NextUpperRightIsRed(row, col, visualTiles))
                            return false;
                        if (BelowTileIsRed(row, col, visualTiles))
                            return false;

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



        public bool SetGroupsTilesAndHarbors(Dictionary<string, string> dataSection)
        {



            _currentGame.Randomize = false;
            for (int i = 0; i < _currentGame.TileGroups.Count; i++)
            {
                string key = $"Group{i}Tiles";

                _currentGame.TileGroups[i].RandomResourceTypeList = StaticHelpers.DeserializeList<int>(dataSection[key]);

                key = $"Group{i}Harbors";
                _currentGame.TileGroups[i].RandomHarborTypeList = StaticHelpers.DeserializeList<int>(dataSection[key]);
            }

            return true;

        }


        private int DesertCount(List<TileCtrl> tiles)
        {
            int deserts = 0;
            foreach (var tile in Tiles)
            {
                if (tile.ResourceType == ResourceType.Desert)
                {
                    deserts++;
                }
            }
            return deserts;
        }


        //
        //  if you want to set the tiles/harbors, get the current game, set Randomize to False, and then call this ShuffleReosurces()
        public void ShuffleResources()
        {
            foreach (TileGroup tileGroup in _currentGame.TileGroups)
            {
                if (tileGroup.Randomize == false) continue;
                int harborCount = 0;

                if (_currentGame.Randomize)
                {
                    tileGroup.RandomResourceTypeList = GetRandomList(tileGroup.Tiles.Count - 1);
                    if (tileGroup.Harbors.Count > 1)
                        tileGroup.RandomHarborTypeList = GetRandomList(tileGroup.Harbors.Count - 1);
                }

                for (int i = 0; i < tileGroup.Tiles.Count; i++)
                {

                    TileCtrl t = tileGroup.Tiles[i];
                    t.ResourceType = tileGroup.ResourceTypes[tileGroup.RandomResourceTypeList[i]];

                    foreach (Harbor h in t.VisibleHarbors)
                    {
                        h.HarborType = tileGroup.HarborTypes[tileGroup.RandomHarborTypeList[harborCount]];
                        harborCount++;
                    }
                }
            }

            AssignNumbers();

        }
        bool _useRandomNumbers = true;
        public bool UseRandomNumbers
        {
            get
            {
                return _useRandomNumbers;
            }
            set
            {
                if (_useRandomNumbers != value)
                {
                    _useRandomNumbers = value;
                    AssignNumbers();

                }
            }
        }

        public void AssignNumbers()
        {
            bool valid = false;
            int iterations = 0;
            do
            {

                foreach (TileGroup tileGroup in _currentGame.TileGroups)
                {
                    if (tileGroup.Randomize == false) continue;

                    int numberSeqCount = 0;
                    List<int> RandomNumberSequence;
                    if (_useRandomNumbers)
                        RandomNumberSequence = GetRandomList(tileGroup.ValidNumbers.Count - DesertCount(tileGroup.Tiles) - 1);
                    else
                    {
                        RandomNumberSequence = new List<int>();
                        for (int i = 0; i <= (tileGroup.ValidNumbers.Count - DesertCount(tileGroup.Tiles) - 1); i++)
                        {
                            RandomNumberSequence.Add(i); //do it in the order of the saved game
                        }
                    }
                    for (int i = 0; i < tileGroup.Tiles.Count; i++)
                    {

                        TileCtrl t = tileGroup.Tiles[i];
                        if (t.ResourceType != ResourceType.Desert)
                        {
                            t.Number = tileGroup.ValidNumbers[RandomNumberSequence[numberSeqCount++]];
                            t.Baron = false;
                        }
                        else
                        {
                            t.Number = 7;

                        }

                    }
                    iterations++;
                }


                valid = IsValidNumberLayout(_currentGame);

            } while (!valid);
            if (iterations > 200)
                this.TraceMessage($"tried {iterations} times to find a valid number sequence");
        }

        /// <summary>
        ///  Returns a list of intergers in "random" order from 0 to max with no duplicates
        /// </summary>
        /// <param name="max"></param>
        /// <returns></returns>

        private List<int> GetRandomList(int max)
        {
            //#if DEBUG
            //            Stopwatch timeToCreateList = Stopwatch.StartNew();
            //#endif
            MersenneTwister twist = new MersenneTwister();
            List<int> randomIndeces = new List<int>();

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
            //#if DEBUG
            //            timeToCreateList.Stop();
            //            this.TraceMessage($"GetRandomList took {new TimeSpan(timeToCreateList.ElapsedTicks).TotalSeconds:N6}s ({timeToCreateList.ElapsedTicks} ticks)");
            //#endif
            return randomIndeces;

            //List<int> ret = null;
            //ret = await GetRandomIntegers(max);


            //if (ret == null)
            //{
            //    ret = GetWithRandFunc(max);
            //}

            //return ret;
        }

        private async Task<List<int>> GetRandomIntegers(int count)
        {

            DateTime startTime = DateTime.Now;
            try
            {
                GetSeed();
                string uriShuffle = String.Format("https://www.random.org/sequences/?min=0&max={0}&col=1&format=plain&rnd=id.Catan:{1}", count, _seed);
                using (HttpClient httpClient = new HttpClient())
                {
                    var response = await httpClient.GetAsync(uriShuffle);
                    response.EnsureSuccessStatusCode();
                    string content = await response.Content.ReadAsStringAsync();
                    string[] sep = new string[] { "\n" };
                    string[] numbers = content.Split(sep, StringSplitOptions.RemoveEmptyEntries);

                    if (numbers.Count() - 1 != count)
                        return null;

                    int i = 0;
                    List<int> intList = new List<int>();
                    foreach (string s in numbers)
                    {
                        int val = Int32.Parse(s);
                        intList.Add(val);
                        i++;
                    }

                    return intList;
                }
            }
            catch (HttpRequestException)
            {
                // Debug.WriteLine("Exception calling randomon.org: count{0}", e.Message);
            }
            catch (Exception)
            {
                // Debug.WriteLine("Exception calling randomon.org: {0}", e.Message);
            }
            finally
            {
                // Debug.WriteLine("Finished calling random.org.  time: {0}ms", (DateTime.Now.Millisecond - startTime.Millisecond));
            }

            return null;

        }
        int _seed = 0;
        private List<int> GetWithRandFunc(int count)
        {
            GetSeed();

            Random r = new Random(_seed);

            List<int> list = new List<int>();
            list.Capacity = count;
            for (int i = 0; i <= count; i++)
            {
                list.Add(i);
            }

            for (int i = 0; i <= count; i++)
            {
                int to = r.Next(count);
                int temp = list[i];
                list[i] = list[to];
                list[to] = temp;
            }
            return list;
        }

        private void GetSeed()
        {

            _seed = (int)DateTime.Now.Ticks & 0x0000FFFF;


        }

        public TileCtrl DesertTile
        {
            get
            {
                foreach (TileCtrl tile in _currentGame.TilesByHexOrder)
                {
                    if (tile.Number == 7)
                        return tile;
                }

                return null;
            }

        }

        public async Task VisualShuffle()
        {
            await FancyTileDistribution();
            await FancyHarborDistribution();
        }

        public async Task FancyHarborDistribution()
        {
            List<Task> list = new List<Task>();
            double ms = MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.Normal);
            int i = 0;



            foreach (TileGroup tileGroup in _currentGame.TileGroups)
            {
                TileCtrl animationMiddle = tileGroup.Tiles[tileGroup.Tiles.Count / 2];

                for (i = 0; i < tileGroup.Harbors.Count; i++)
                {
                    int j = i + 1;
                    if (j == tileGroup.Harbors.Count) j = 0;

                    GeneralTransform gt = animationMiddle.TransformToVisual(tileGroup.Harbors[i]);
                    Point pt = gt.TransformPoint(new Point(tileGroup.Harbors[0].Width, tileGroup.Harbors[0].Height));
                    Task task = tileGroup.Harbors[i].AnimateMoveTask(pt, ms, i * ms);
                    list.Add(task);
                }
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            Random r = new Random(DateTime.Now.Millisecond);
            ms = MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.Normal);
            foreach (TileGroup tileGroup in _currentGame.TileGroups)
            {
                foreach (var h in tileGroup.Harbors)
                {
                    Task task = h.RotateTask(r.Next(1, 10) * 360, ms);
                    list.Add(task);
                }
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            ms = MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.Fast);
            foreach (TileGroup tileGroup in _currentGame.TileGroups)
            {
                foreach (var h in tileGroup.Harbors)
                {
                    Task task = h.AnimateMoveTask(new Point(0, 0), ms, i * ms);
                    i++;
                    list.Add(task);
                }
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            ms = ms = MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.Normal);
            foreach (TileGroup tileGroup in _currentGame.TileGroups)
            {
                foreach (var h in tileGroup.Harbors)
                {
                    Task task = h.SetOrientation(TileOrientation.FaceUp, ms, i * ms);
                    i++;
                    list.Add(task);
                }
            }

            await Task.WhenAll(list);

        }
        public async Task FancyTileDistribution()
        {


            List<Task> list = new List<Task>();
            double ms = MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.Fast); ;
            int i = 0;
            foreach (TileGroup tileGroup in _currentGame.TileGroups)
            {
                if (tileGroup.Randomize == false) continue;
                TileCtrl centerTile = tileGroup.Tiles.Last(); ;
                foreach (TileCtrl t in tileGroup.Tiles)
                {
                    GeneralTransform gt = centerTile.HexGrid.TransformToVisual(t.HexGrid);
                    Point pt = gt.TransformPoint(new Point(0, 0));
                    Task task = t.AnimateMoveTask(pt, ms, i * ms);
                    i++;
                    t.zIndex = 1000 - i;
                    list.Add(task);
                }
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            Random r = new Random(DateTime.Now.Millisecond);
            foreach (TileGroup tileGroup in _currentGame.TileGroups)
            {
                if (tileGroup.Randomize == false) continue;
                foreach (TileCtrl t in tileGroup.Tiles)
                {
                    Task task = t.RotateTask(r.Next(1, 5) * 360, MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.Fast));
                    list.Add(task);
                }
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;


            foreach (TileGroup tileGroup in _currentGame.TileGroups)
            {
                if (tileGroup.Randomize == false) continue;
                foreach (TileCtrl t in tileGroup.Tiles)
                {
                    t.ResetTileRotation();
                }
            }


            foreach (TileGroup tileGroup in _currentGame.TileGroups)
            {
                if (tileGroup.Randomize == false) continue;
                foreach (TileCtrl t in tileGroup.Tiles)
                {
                    Task task = t.AnimateMoveTask(new Point(0, 0), ms, i * ms);
                    i++;
                    list.Add(task);
                }
            }
            await Task.WhenAll(list);
            list.Clear();
            i = 0;
            foreach (TileGroup tileGroup in _currentGame.TileGroups)
            {
                if (tileGroup.Randomize == false) continue;

                foreach (TileCtrl t in tileGroup.Tiles)
                {
                    Task task = t.SetTileOrientation(TileOrientation.FaceUp, false, MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.Fast), i * MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.Fast));
                    i++;
                    list.Add(task);
                }
            }

            await Task.WhenAll(list);

        }

        public async Task FlipTiles()
        {
            TileOrientation orientation = TileOrientation.FaceUp;
            if (this.Tiles[0].TileOrientation == TileOrientation.FaceUp) orientation = TileOrientation.FaceDown;
            await FlipTiles(orientation);
        }

        public async Task FlipTiles(TileOrientation orientation, bool harborsToo = true)
        {
            double ms = MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.Fast);
            List<Task> list = new List<Task>();
            int i = 0;
            foreach (TileCtrl t in Tiles)
            {
                if (t.RandomTile == false) continue;
                Task task = t.SetTileOrientation(orientation, harborsToo, ms, i * ms);
                i++;
                list.Add(task);
            }



            await Task.WhenAll(list);
        }
        public void FlipAllTilesAsync(TileOrientation orientation, bool harborsToo = true)
        {
            double ms = MainPage.Current.MainPageModel.GetAnimationSpeed(AnimationSpeed.Fast);

            foreach (TileCtrl t in Tiles)
            {
                if (t.RandomTile == false) continue;
                t.SetTileOrientationAsync(orientation, harborsToo);
            }

        }

    }
}
