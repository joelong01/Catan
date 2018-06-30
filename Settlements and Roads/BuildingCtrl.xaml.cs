using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{


    /// <summary>
    ///     The states that a building can be in
    /// </summary>
    public enum BuildingState { None, Build, Error, Pips, Settlement, City };

    public sealed partial class BuildingCtrl : UserControl
    {
        public int Index { get; set; } = -1; // the Index into the Settlement list owned by the HexPanel...so we can save it and set it later


        SolidColorBrush _brush = new SolidColorBrush(Colors.Blue);
        public Dictionary<BuildingLocation, TileCtrl> BuildingToTileDictionary { get; set; } = new Dictionary<BuildingLocation, TileCtrl>();

        public List<RoadCtrl> AdjacentRoads { get; } = new List<RoadCtrl>();

        public List<BuildingCtrl> AdjacentBuildings { get; } = new List<BuildingCtrl>();

        //
        //  this the list of Tile/SettlmentLocations that are the same for this settlement
        public List<BuildingKey> Clones = new List<BuildingKey>();
        public Point LayoutPoint { get; set; }
        public CompositeTransform Transform { get { return (CompositeTransform)this.RenderTransform; } }

        public IGameCallback Callback { get; internal set; }


        public static readonly DependencyProperty BuildingStateProperty = DependencyProperty.Register("BuildingState", typeof(BuildingState), typeof(BuildingCtrl), new PropertyMetadata(BuildingState.None, BuildingStateChanged));
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerData), typeof(BuildingCtrl), new PropertyMetadata(null, CurrentPlayerChanged));
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerData), typeof(BuildingCtrl), new PropertyMetadata(null));
        public static readonly DependencyProperty PipGroupProperty = DependencyProperty.Register("PipGroup", typeof(int), typeof(BuildingCtrl), new PropertyMetadata(0, PipGroupChanged));
        public static readonly DependencyProperty PipsProperty = DependencyProperty.Register("Pips", typeof(int), typeof(BuildingCtrl), new PropertyMetadata(27, PipsChanged));
        public int Pips
        {
            get { return (int)GetValue(PipsProperty); }
            set { SetValue(PipsProperty, value); }
        }
        private static void PipsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuildingCtrl depPropClass = d as BuildingCtrl;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetPips(depPropValue);
        }
        private void SetPips(int value)
        {

        }




        public int PipGroup
        {
            get { return (int)GetValue(PipGroupProperty); }
            set { SetValue(PipGroupProperty, value); }
        }
        private static void PipGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuildingCtrl depPropClass = d as BuildingCtrl;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetPipGroup(depPropValue);
        }
        private void SetPipGroup(int value)
        {

        }


        public PlayerData Owner
        {
            get => (PlayerData)GetValue(OwnerProperty);
            set => SetValue(OwnerProperty, value);
        }
        public PlayerData CurrentPlayer
        {
            get => (PlayerData)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }
        private static void CurrentPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as BuildingCtrl;
            var depPropValue = (PlayerData)e.NewValue;
            depPropClass?.SetCurrentPlayer(depPropValue);
        }
        private void SetCurrentPlayer(PlayerData value)
        {

        }

        public BuildingState BuildingState
        {
            get { return (BuildingState)GetValue(BuildingStateProperty); }
            private set { SetValue(BuildingStateProperty, value); } // call UpdateBuildingState instead
        }
        private static void BuildingStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuildingCtrl depPropClass = d as BuildingCtrl;
            BuildingState depPropValue = (BuildingState)e.NewValue;
            depPropClass.SetBuildingState(depPropValue);
        }

        private void SetBuildingState(BuildingState value)
        {

        }







        internal void Reset()
        {
            Owner = null;
            this.BuildingState = BuildingState.None;

        }

        public override string ToString()
        {
            return String.Format($"Index={Index};State={BuildingState};Owner={Owner};Pips={Pips};PipGroup={PipGroup}");
        }


        public bool IsCity
        {
            get
            {
                return BuildingState == BuildingState.City;
            }
        }

        public bool IsSettlement
        {
            get
            {
                return BuildingState == BuildingState.Settlement;
            }
        }

        public BuildingCtrl()
        {
            this.InitializeComponent();




            this.Width = 30;
            this.Height = 30;
            this.BuildingState = BuildingState.None;
            //this..Show(BuildingState.Settlement);
            Canvas.SetZIndex(this, 20);
            this.RenderTransformOrigin = new Point(.5, .5);
            this.HorizontalAlignment = HorizontalAlignment.Left;
            this.VerticalAlignment = VerticalAlignment.Top;
            CompositeTransform transform = new CompositeTransform
            {
                ScaleX = 1.0,
                ScaleY = 1.0
            };

            this.RenderTransform = transform;



        }


        private void OutputKeyInfo()
        {
            string s = "";
            foreach (var key in Clones)
            {
                s += String.Format($"\n\tTile:{key.Tile} at {key.Location}");
            }
            s += "\n";
            this.TraceMessage(s);
        }


        /// <summary>
        ///     When we enter a building, we check to see if there is nothing being shown
        ///     if so and if it is a valid building location, show the build ellipse
        ///     otherwise show the Error
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Building_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (this.BuildingState == BuildingState.None)
            {

                Tuple<bool, bool> validate = Callback?.IsValidBuildingLocation(this);
                if (validate.Item1) // it is a valid location
                {
                    this.BuildingState = BuildingState.Build;
                }
                else if (validate.Item2) // it is not a valid location and we should show an error
                {
                    this.BuildingState = BuildingState.Error;

                }
            }

        }
        /// <summary>
        ///     user clicked on a building.  change the state to the new state and then update the BuildingState (which does the proper logging)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Building_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            //
            //  need to validate that the GameState is a valid state to change the state of a building

            bool valid = (bool)Callback?.BuildingStateChangedOk(this);
            if (!valid)
            {
                return;
            }



            BuildingState oldState = this.BuildingState;
            BuildingState newState = BuildingState.None;
            switch (oldState)
            {
                case BuildingState.Error: // do nothing
                case BuildingState.None: // do nothing
                    return;
                case BuildingState.Pips: // Pips and build transition to Settlement
                case BuildingState.Build:
                    oldState = BuildingState.None; // when you have Pips and then Undo it, go back to None;
                    newState = BuildingState.Settlement;
                    break;
                case BuildingState.Settlement: //transition to City
                    newState = BuildingState.City;
                    break;
                case BuildingState.City: // transtion to Build
                    newState = BuildingState.Build;
                    break;
                default:
                    break;
            }

            await UpdateBuildingState(oldState, newState, LogType.Normal);


        }

        /// <summary>
        ///     Sets the state of a building *directly* - eg. doesn't validate state transitions.
        ///     we need this functionality in Undo/Redo and ReplayLog
        /// </summary>
        /// <param name="building"></param>
        /// <param name="oldState"></param>
        /// <param name="newState"></param>
        /// <param name="logType"></param>
        /// <returns></returns>
        public async Task UpdateBuildingState(BuildingState oldState, BuildingState newState, LogType logType)
        {
            PlayerData player = CurrentPlayer;

            //
            //  remove everything -- we will add it back below
            player.GameData.Cities.Remove(this);
            player.GameData.Settlements.Remove(this);
            this.BuildingState = newState;


            switch (newState)
            {
                case BuildingState.Pips:
                    //    Owner = player;
                    break;

                //
                //  work done above                    
                case BuildingState.None:
                case BuildingState.Build:
                    Owner = null;
                    break;
                case BuildingState.Settlement:
                    Owner = player;
                    player.GameData.Settlements.Add(this);
                    break;
                case BuildingState.City:
                    Owner = player;
                    player.GameData.Cities.Add(this);
                    break;
                default:
                    break;
            }

            await Callback?.BuildingStateChanged(this, oldState, logType);
        }

        /// <summary>
        ///  if we leave and it was an Error or Build, reset state to None
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Building_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (this.BuildingState == BuildingState.Build || BuildingState == BuildingState.Error)
            {
                this.BuildingState = BuildingState.None;
            }
        }
        public void ShowBuildEllipse(bool canBuild = true, string colorAsString = "", string msg = "X")
        {
            _txtPipCount.Text = msg;

            this.BuildingState = BuildingState.Pips;

        }


        public int ScoreValue
        {
            get
            {
                switch (BuildingState)
                {
                    case BuildingState.None:
                        return 0;
                    case BuildingState.Settlement:
                        return 1;
                    case BuildingState.City:
                        return 2;
                    default:
                        return 999;

                }
            }
        }

        public void AddKey(TileCtrl tile, BuildingLocation loc)
        {
            BuildingKey key = new BuildingKey(tile, loc);
            foreach (var clone in Clones)
            {
                //
                //  need to do this by value because .Contains looks for the same pointer value
                if (clone.Tile.Index == tile.Index && clone.Location == loc)
                {
                    // this.TraceMessage($"{tile} @ {loc} already in Clones list!");
                    return;
                }
            }

            Clones.Add(key);

        }


    }

    public class KeyComparer : IEqualityComparer<BuildingKey>
    {
        //
        //  Note:  Once the board is created, we never change the Tiles or the Location of a Settlement...
        public bool Equals(BuildingKey x, BuildingKey y)
        {
            if (x.Tile.Index == y.Tile.Index)
            {
                if (x.Location == y.Location)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetHashCode(BuildingKey obj)
        {
            return obj.Tile.GetHashCode() * 17 + obj.Location.GetHashCode();
        }
    }

    /// <summary>
    ///  This is for the "clones" support. the issue is that you can have multiple Tile/BuildingLocation pairs that map to the same
    ///  visual location for a Building.  We want to have exactly one building per location, so that datastructure allows us to find
    ///  duplicates and map the tuple (Tile,Location) to a unique Building
    /// </summary>
    public class BuildingKey
    {
        public TileCtrl Tile { get; set; }

        public BuildingLocation Location { get; set; }

        public BuildingKey(TileCtrl t, BuildingLocation loc)
        {
            Tile = t;
            Location = loc;
        }

        public override string ToString()
        {
            return String.Format($"[{Tile}. IDX={Tile.Index} @ {Location}]");
        }


    }

}
