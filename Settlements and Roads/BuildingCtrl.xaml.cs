using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Threading.Tasks;
using Windows.ApplicationModel;
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
    public enum BuildingState { None, Build, Error, Pips, Settlement, City, NoEntitlement, Knight };

    public sealed partial class BuildingCtrl : UserControl
    {
        private static void BuildingStateChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuildingCtrl depPropClass = d as BuildingCtrl;
            BuildingState depPropValue = (BuildingState)e.NewValue;
            depPropClass.SetBuildingState(depPropClass, depPropValue);
        }

        private static void CurrentPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuildingCtrl depPropClass = d as BuildingCtrl;
            PlayerModel depPropValue = (PlayerModel)e.NewValue;
            depPropClass?.SetCurrentPlayer(depPropValue);
        }

        private static void PipGroupChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuildingCtrl depPropClass = d as BuildingCtrl;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetPipGroup(depPropValue);
        }

        private static void PipsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            BuildingCtrl depPropClass = d as BuildingCtrl;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetPips(depPropValue);
        }

        /// <summary>
        ///     When we enter a building, we check to see if there is nothing being shown
        ///     if so and if it is a valid building location, show the build ellipse
        ///     otherwise show the Error, which can indicate they don't have the entitlement to build.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Building_PointerEntered(object sender, PointerRoutedEventArgs e)
        {
            if (this.BuildingState == BuildingState.None && Callback != null)
            {
                this.BuildingState = Callback.ValidateBuildingLocation(this);
            }
        }

        /// <summary>
        ///  if we leave and it was an Error or Build, reset state to None
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Building_PointerExited(object sender, PointerRoutedEventArgs e)
        {
            if (this.BuildingState == BuildingState.Build || BuildingState == BuildingState.Error || BuildingState == BuildingState.NoEntitlement)
            {
                this.BuildingState = BuildingState.None;
            }

            if (MainPage.Current.CurrentGameState == GameState.PickingBoard && this.BuildingState == BuildingState.Pips)
            {
                this.BuildingState = BuildingState.None;
            }

            if (this.BuildingState == BuildingState.Knight && this.Owner == null)
            {
                this.BuildingState = BuildingState.None;
            }

        }

        /// <summary>
        ///     user clicked on a adjacent.  change the state to the new state and then update the BuildingState (which does the proper logging)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private async void Building_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            this.TraceMessage($"Building: {this}");
            //
            //  need to validate that the GameState is a valid state to change the state of a adjacent
            if (Callback == null) return;

            //
            //  if you have a City and the Wall entitlment, you can click on a city to protect it.
            if (this.BuildingState == BuildingState.City &&
                Callback.HasEntitlement(Entitlement.Wall) &&
                ( ( IGameController )Callback ).CurrentPlayer == Owner)
            {
                await ProtectCityLog.ProtectCity(Callback as IGameController, this);
                return;
            }

            if (this.BuildingState == BuildingState.City && Callback.HasEntitlement(Entitlement.DestroyCity))
            {
                await Callback.DestroyCity(this);
                return;
            }

            if (this.BuildingState == BuildingState.City && Callback.HasEntitlement(Entitlement.UpgradeToMetro) && !City.Metropolis)
            {

                await MetroTransitionLog.UpgradeCityLog(Callback as IGameController, this.Index);
                return;
            }

            if (this.BuildingState == BuildingState.Knight) // this is handled in KnightPointerUp so that we can do D&D
            {

                //if (this.Owner != null)
                //{
                //    // left click on an owned knight - no action
                //    return;
                //}
                //this.BuildingState = BuildingState.None;
                //await UpdateBuildingLog.UpdateBuildingState(Callback as IGameController, this, BuildingState.Knight, GameState.WaitingForNext);
                return;

            }

            bool valid = (bool)Callback?.BuildingStateChangeOk(this);
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

            var gameController = Callback as IGameController;
            await UpdateBuildingLog.UpdateBuildingState(gameController, this, newState, Callback.CurrentGameState);
        }

        private void OutputKeyInfo()
        {
            string s = "";
            foreach (BuildingKey key in Clones)
            {
                s += String.Format($"\n\tTile:{key.Tile} at {key.Location}");
            }
            s += "\n";
            this.TraceMessage(s);
        }

        private void SetBuildingState(BuildingCtrl ctrl, BuildingState value)
        {
        }

        private void SetCurrentPlayer(PlayerModel value)
        {
        }

        private void SetPipGroup(int value)
        {
        }

        private void SetPips(int value)
        {
        }

        internal void Reset()
        {
            Owner = null;
            this.BuildingState = BuildingState.None;
        }

        public List<BuildingCtrl> AdjacentBuildings { get; } = new List<BuildingCtrl>();

        // the harbor that is acquired when the user gets this adjacent
        public HarborCtrl AdjacentHarbor { get; set; } = null;

        public List<RoadCtrl> AdjacentRoads { get; } = new List<RoadCtrl>();

        public BuildingState BuildingState
        {
            get => ( BuildingState )GetValue(BuildingStateProperty);
            private set => SetValue(BuildingStateProperty, value);  // call ProtectCity instead
        }

        public void ResetTempBuildingState()
        {
            this.BuildingState = BuildingState.None;
        }

        public Dictionary<BuildingLocation, TileCtrl> BuildingToTileDictionary { get; set; } = new Dictionary<BuildingLocation, TileCtrl>();
        public IGameCallback Callback { get; internal set; }

        public PlayerModel CurrentPlayer
        {
            get => ( PlayerModel )GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }

        // the Index into the Settlement list owned by the HexPanel...so we can save it and set it later
        public int Index
        {
            get => ( int )GetValue(IndexProperty);
            set => SetValue(IndexProperty, value);
        }

        public bool IsCity => BuildingState == BuildingState.City;
        public bool IsSettlement => BuildingState == BuildingState.Settlement;
        public bool IsKnight => BuildingState == BuildingState.Knight;
        public Point LayoutPoint { get; set; }

        public PlayerModel Owner
        {
            get => ( PlayerModel )GetValue(OwnerProperty);
            set => SetValue(OwnerProperty, value);
        }

        public int PipGroup
        {
            get => ( int )GetValue(PipGroupProperty);
            set => SetValue(PipGroupProperty, value);
        }

        public int Pips
        {
            get => ( int )GetValue(PipsProperty);
            set => SetValue(PipsProperty, value);
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
                    case BuildingState.Knight:
                        return 0;
                    default:
                        this.TraceMessage($"You need to set a ScoreValue for BuildingState={BuildingState}.");
                        return 999;
                }
            }
        }

        public CompositeTransform Transform => ( CompositeTransform )this.RenderTransform;
        public static readonly DependencyProperty BuildingStateProperty = DependencyProperty.Register("BuildingState", typeof(BuildingState), typeof(BuildingCtrl), new PropertyMetadata(BuildingState.None, BuildingStateChanged));
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerModel), typeof(BuildingCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer, CurrentPlayerChanged));
        public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index", typeof(int), typeof(BuildingCtrl), new PropertyMetadata(0));
        public static readonly DependencyProperty OwnerProperty = DependencyProperty.Register("Owner", typeof(PlayerModel), typeof(BuildingCtrl), new PropertyMetadata(PlayerModel.DefaultPlayer));
        public static readonly DependencyProperty PipGroupProperty = DependencyProperty.Register("PipGroup", typeof(int), typeof(BuildingCtrl), new PropertyMetadata(0, PipGroupChanged));
        public static readonly DependencyProperty PipsProperty = DependencyProperty.Register("Pips", typeof(int), typeof(BuildingCtrl), new PropertyMetadata(27, PipsChanged));
        public static readonly DependencyProperty HighlightProperty = DependencyProperty.Register("Highlight", typeof(bool), typeof(BuildingCtrl), new PropertyMetadata(false));

        //
        //  this is bound in the HexPanel to MainPageModel.Current.MainPage via code behing
        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(BuildingCtrl), new PropertyMetadata(MainPageModel.Default, MainPageModelChanged));
        public MainPageModel MainPageModel
        {
            get => ( MainPageModel )GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }
        private static void MainPageModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as BuildingCtrl;
            var depPropValue = (MainPageModel)e.NewValue;
            depPropClass?.SetMainPageModel(depPropValue);
        }
        private void SetMainPageModel(MainPageModel value)
        {
           // this.TraceMessage("MainPageModel updated in buildingctrl");
        }
        public bool Highlight
        {
            get => ( bool )GetValue(HighlightProperty);
            set => SetValue(HighlightProperty, value);
        }

        //
        //  this the list of Tile/SettlmentLocations that are the same for this settlement
        public List<BuildingKey> Clones = new List<BuildingKey>();

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

        public void AddKey(TileCtrl tile, BuildingLocation loc)
        {
            BuildingKey key = new BuildingKey(tile, loc);
            foreach (BuildingKey clone in Clones)
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

        public Visibility BuildingStateToVisibility(BuildingState state, string match)
        {
            var controlState = (BuildingState)Enum.Parse(typeof(BuildingState), match, true);
            Contract.Assert(controlState != BuildingState.None);
            var vis = (state == controlState) ? Visibility.Visible : Visibility.Collapsed;
            if (DesignMode.DesignModeEnabled)
            {
                if (state == BuildingState.Knight)
                {
                    return Visibility.Visible;
                }
                else
                {
                    return Visibility.Collapsed;
                }
            }
            return vis;
        }

        public Brush PickPlayerBackground(PlayerModel owner, PlayerModel current)
        {
            if (owner != null)
            {
                return owner.BackgroundBrush;
            }
            if (current != null)
            {
                return current.BackgroundBrush;
            }

            var brush = ConverterGlobals.GetLinearGradientBrush(Colors.Purple, Colors.Black);
            return brush;
        }

        public void ShowBuildEllipse(bool canBuild = true, string colorAsString = "", string msg = "X")
        {
            _txtPipCount.Text = msg;

            this.BuildingState = BuildingState.Pips;
        }

        public KnightCtrl Knight
        {
            get
            {
                if (IsKnight)
                {
                    return CTRL_Knight;
                }
                else
                {
                    return null;
                }
            }
        }

        public CityCtrl City
        {
            get
            {
                if (IsCity)
                {
                    return CTRL_City;
                }
                else
                {
                    return null;
                }
            }
        }

        public override string ToString()
        {
            return String.Format($"Index={Index};State={BuildingState};Owner={Owner};Pips={Pips};PipGroup={PipGroup}");
        }

        /// <summary>
        ///     Sets the state of a building *directly* - eg. doesn't validate state transitions.
        ///     we need this functionality in Undo/Redo and ReplayLog
        /// </summary>
        /// <returns></returns>
        public async Task UpdateBuildingState(PlayerModel player, BuildingState oldState, BuildingState newState)
        {
            bool ret;
            switch (oldState)
            {
                case BuildingState.None:
                case BuildingState.Build:
                case BuildingState.Error:
                case BuildingState.Pips:
                    break;

                case BuildingState.Settlement:
                    ret = player.GameData.Settlements.Remove(this);
                    Contract.Assert(ret, "a settlement needs to be in the Settlements Collection");
                    break;

                case BuildingState.City:
                    ret = player.GameData.Cities.Remove(this);
                    Contract.Assert(ret, "a settlement needs to be in the Settlements Collection");
                    break;
                case BuildingState.Knight: // remove it and if we are supposed to, we'll add it later.  this means that the knight shows up in the collection and then leaves the collection when the mouse leaves
                    bool removed = player.GameData.CK_Knights.Remove(this.Knight);
                    this.Knight.KnightRank = KnightRank.Basic;
                    System.Diagnostics.Debug.Assert(removed, "if this fails, you probably have the player wrong.");
                    break;
                default:
                    break;
            }

            this.BuildingState = newState;

            switch (newState)
            {
                case BuildingState.Pips:
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
                case BuildingState.Knight:
                    Owner = player;
                    player.GameData.CK_Knights.Add(this.Knight);
                    break;
                default:
                    break;
            }
            if (AdjacentHarbor != null && GeneratesResources())
            {
                AdjacentHarbor.Owner = Owner;
            }
            // this.TraceMessage($"TotalKnightCount: {MainPage.Current.MainPageModel.TotalKnightRanks}");
            await Task.Delay(0);
        }

        private bool GeneratesResources()
        {
            return ( this.BuildingState == BuildingState.Settlement || this.BuildingState == BuildingState.City );
        }



        private async void OnUpgrade(object sender, RoutedEventArgs e)
        {
            await Callback.UpgradeKnight(this);
        }

        private async void OnActivate(object sender, RoutedEventArgs e)
        {
            await Callback.ActivateKnight(this, true);
        }


        private async void OnKnightGridPointerDown(object sender, PointerRoutedEventArgs e)
        {
            this.TraceMessage("START OnKnightGridPointerDown");
            if (this.BuildingState != BuildingState.Knight)
            {
                Debug.Assert(false, "How can we get this event from a non-knight?");
                return;
            }

            //
            //  the grid is the one that clicks, but we need to know the actual knight inside the Grid
            var knightClicked =  StaticHelpers.FindChildControl<KnightCtrl> ( ( Grid )sender );

            //
            //  if you clicked on somebody else's kight, you need to be in the DisplaceKnightMoveVictim state
            if (knightClicked.Owner != CurrentPlayer && Callback.CurrentGameState != GameState.DisplaceKnightMoveVictim)
            {
                this.TraceMessage($"Early return {this.Knight.Activated} {this.Owner} {Callback.CurrentGameState}");
                return;
            }

            //
            // return early if the knight isn't activated or if it is somebody else's night.
            // the exception is if we are displacing a knight, then we can move the displaced knight
            // we rely on the players to make sure that the right knight is moved
            if (!knightClicked.Activated && Callback.CurrentGameState != GameState.DisplaceKnightMoveVictim)
            {
                if (knightClicked.Owner == CurrentPlayer) // you should be clicking on somebody else's knight
                {
                    this.TraceMessage($"Early return {this.Knight.Activated} {this.Owner} {Callback.CurrentGameState}");
                    return;
                }

            }
            bool displacePhaseOne = false;
            Entitlement grantedEntitlement = Entitlement.Undefined;
            Entitlement[] entitlementToCheckFor = new Entitlement[] { Entitlement.MoveKnight, Entitlement.Intrigue, Entitlement.KnightDisplacement };
            foreach (var entitlement in entitlementToCheckFor)
            {
                if (Callback.HasEntitlement(entitlement))
                {
                    grantedEntitlement = entitlement;
                    break;
                }
            }

            if (grantedEntitlement == Entitlement.Undefined)
            {
                this.TraceMessage($"no appropriate entitlement for player {this.Owner}");
                return;
            }

            if (grantedEntitlement != Entitlement.MoveBaron)
            {
                if (( ( IGameController )Callback ).CurrentPlayer == this.Owner)
                {
                    displacePhaseOne = true;
                }
            }
            this.TraceMessage($"phase one?  {displacePhaseOne}");
            try
            {
                List<BuildingCtrl> targets;
                if (!displacePhaseOne) // only phase one looks for knights
                {
                    targets = GetConnectedBuildings(Entitlement.MoveKnight);
                }
                else
                {
                    targets = GetConnectedBuildings(grantedEntitlement);
                }

                var target =  await DragAndDropKnight(sender, e, targets);
                // in either case, you return here and the original knight is back where you started
                if (target != null)

                {
                    if (grantedEntitlement == Entitlement.MoveKnight)
                    {
                        await MoveKnightLog.PostLog(Callback as IGameController, this, target);
                    }

                    else
                    {
                        if (displacePhaseOne)
                        {
                            await DisplaceKnightLog.DisplaceKnightPhaseOne(Callback as IGameController, this, target, grantedEntitlement);
                        }
                        else
                        {
                            // it was somebody else's knight -- go to the end of phase 2
                            await DisplaceKnightLog.DisplaceKnightPhaseTwo(Callback as IGameController, this, target, grantedEntitlement);
                        }
                    }
                }
            }
            finally
            {
                e.Handled = true;
                this.TraceMessage("END OnKnightGridPointerDown");
            }





        }

        private async Task<BuildingCtrl> DragAndDropKnight(object sender, PointerRoutedEventArgs e, List<BuildingCtrl> targets)
        {

            BuildingCtrl target = null;

            void dragEnterHandler(UIElement control)
            {
                target = control as BuildingCtrl;
                target.Highlight = true;


            }
            void dragLeaveHandler(UIElement control)
            {
                target = control as BuildingCtrl;
                target.Highlight = false;
            }

            var dragHelper = new StaticHelpers.DragHelper();
            var zIndex = Canvas.GetZIndex(this);
            try
            {
                Canvas.SetZIndex(this, zIndex + 9999);
                e.Handled = true;
                dragHelper.DragEnter += dragEnterHandler;
                dragHelper.DragLeave += dragLeaveHandler;
                var exitPoint =   await dragHelper.DragAsync<BuildingCtrl>((FrameworkElement)sender, CTRL_Knight, e, targets);
                this.TraceMessage($"{exitPoint}");
            }
            finally
            {
                dragHelper.DragEnter -= dragEnterHandler;
                dragHelper.DragLeave -= dragLeaveHandler;
                MoveAsync(new Point(0, 0));
                targets.ForEach((b) => b.Highlight = false);
                Canvas.SetZIndex(this, zIndex);
            }


            return target;
        }


        //
        //  Gets all of the connected buildings to serve the "entitlement" purpose:
        //  
        public List<BuildingCtrl> GetConnectedBuildings(Entitlement entitlement)
        {
            var connectedBuildings = new HashSet<BuildingCtrl>(); // Holds buildings with no owner
            var exploredRoads = new HashSet<RoadCtrl>(); // Keeps track of explored roads to avoid loops
            var roadsToExplore = new Queue<RoadCtrl>(); // Queue to hold roads that are to be explored
            var startKnight = this;
            Debug.Assert(entitlement == Entitlement.MoveKnight || entitlement == Entitlement.KnightDisplacement || entitlement == Entitlement.Intrigue);

            // Enqueue the adjacent roads of the starting building that have the same owner
            foreach (var road in startKnight.AdjacentRoads)
            {
                if (road.Owner == startKnight.Owner)
                {
                    roadsToExplore.Enqueue(road);
                }
            }

            while (roadsToExplore.Count > 0)
            {
                var currentRoad = roadsToExplore.Dequeue();

                // If we've already explored this road, skip it
                if (exploredRoads.Contains(currentRoad))
                    continue;

                exploredRoads.Add(currentRoad);

                // Examine each building connected to the current road
                foreach (var building in currentRoad.AdjacentBuildings)
                {
                    // If the building has an owner that is not the startKnight's owner, skip this building and its roads
                    if (building.Owner != null && building.Owner != startKnight.Owner)
                    {
                        if (entitlement == Entitlement.MoveKnight) continue;

                        if (building.BuildingState == BuildingState.Knight && Owner != null)
                        {
                            if (connectedBuildings.Contains(building) == false)
                                connectedBuildings.Add(building);
                            continue;
                        }

                    }

                    // If the building has no owner, add it to the list of connected buildings
                    if (building.Owner == null)
                    {
                        if (entitlement == Entitlement.MoveKnight)
                            connectedBuildings.Add(building);
                    }

                    // Enqueue the connected roads for further exploration
                    // Only enqueue roads if the connected building has no owner or has the same owner as the startKnight
                    if (building.Owner == null || building.Owner == startKnight.Owner)
                    {
                        foreach (var connectedRoad in building.AdjacentRoads)
                        {
                            // Continue exploring only roads owned by startKnight
                            if (connectedRoad.Owner == startKnight.Owner && !exploredRoads.Contains(connectedRoad))
                            {
                                roadsToExplore.Enqueue(connectedRoad);
                            }
                        }
                    }
                }

                // Enqueue connected roads of the current road that have the same owner as the startKnight
                foreach (var road in currentRoad.AdjacentRoads)
                {
                    if (road.Owner == startKnight.Owner && !exploredRoads.Contains(road))
                    {
                        roadsToExplore.Enqueue(road);
                    }
                }
            }

            // Convert the hash set to a list before returning
            var buildings =  connectedBuildings.ToList();
            if (entitlement == Entitlement.KnightDisplacement) // e.g. if it is intrigue, do not filter
            {
                buildings = buildings.Where(k => k.IsKnight && k.Knight.KnightRank < startKnight.Knight.KnightRank)
                        .ToList();
            }
            return buildings;
        }



        public void MoveAsync(Point to)
        {
            // Transform the point to the appropriate coordinate system

            DA_X.To = to.X;
            DA_Y.To = to.Y;
            AnimateMove.Begin();
        }

        private async void OnKnighClicked(object sender, PointerRoutedEventArgs e)
        {
            //this.TraceMessage("Knight clicked");
            //
            //  because of the way events have been subscribed to, we need to return when
            //  we are doing drag and drop, which is driven by entitlements.
            if (Callback.HasEntitlement(Entitlement.MoveKnight)) return;
            if (Callback.HasEntitlement(Entitlement.KnightDisplacement)) return;

            if (this.BuildingState != BuildingState.Knight) return;

            //
            //  the BuildingState.Knight is overloaded - it shows the Knight that can be built
            //  and it is the state when it is actually built.  So in PointerEnter, if it is 
            //  a valid place for a knight, we set the state to Knight so the user knows there
            //  is something to click on.  When you leave it goes away -- but when you click on
            //  it, we need to reset the state so that the state machine is correct.


            await Callback.KnightLeftPointerPressed(this);
            e.Handled = true;



            if (this.Owner == null)
            {
                ResetTempBuildingState();
            }
        }
    }

    /// <summary>
    ///  This is for the "clones" support. the issue is that you can have multiple Tile/BuildingLocation pairs that map to the same
    ///  visual location for a Building.  We want to have exactly one building per location, so that datastructure allows us to find
    ///  duplicates and map the tuple (Tile,Location) to a unique Building
    /// </summary>
    public class BuildingKey
    {
        public BuildingLocation Location { get; set; }

        public TileCtrl Tile { get; set; }

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
}
