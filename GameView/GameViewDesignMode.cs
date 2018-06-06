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
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
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
    public sealed partial class GameViewControl : UserControl, IGameViewCallback
    {
        TileCtrl _selectedTile = null;
        int _tileIndex = 0;

        #region DEPENDENCY PROPERTIES
        //  NotifyPropertyChanged("CurrentIndex");
        public static readonly DependencyProperty CurrentIndexProperty = DependencyProperty.Register("CurrentIndex", typeof(int), typeof(GameViewControl), new PropertyMetadata(0));
        //  NotifyPropertyChanged("CurrentzIndex");
        public static readonly DependencyProperty CurrentzIndexProperty = DependencyProperty.Register("CurrentzIndex", typeof(int), typeof(GameViewControl), new PropertyMetadata(0));
        //  NotifyPropertyChanged("CurrentNumber");
        public static readonly DependencyProperty CurrentNumberProperty = DependencyProperty.Register("CurrentNumber", typeof(int), typeof(GameViewControl), new PropertyMetadata(2));
        //  NotifyPropertyChanged("UseClassic");
        public static readonly DependencyProperty UseClassicProperty = DependencyProperty.Register("UseClassic", typeof(bool), typeof(GameViewControl), new PropertyMetadata(true));
        ////  NotifyPropertyChanged("Index");
        //public static readonly DependencyProperty IndexProperty = DependencyProperty.Register("Index", typeof(int), typeof(GameViewControl), new PropertyMetadata(false));
        //  NotifyPropertyChanged("NumberOfPlayers");
        public static readonly DependencyProperty NumberOfPlayersProperty = DependencyProperty.Register("NumberOfPlayers", typeof(string), typeof(GameViewControl), new PropertyMetadata("3-5"));

        //  NotifyPropertyChanged("CurrentResourceType");
        public static readonly DependencyProperty CurrentResourceTypeProperty = DependencyProperty.Register("CurrentResourceType", typeof(ResourceType), typeof(GameViewControl), new PropertyMetadata(ResourceType.None));
        //  NotifyPropertyChanged("CurrentHarborType");
        public static readonly DependencyProperty CurrentHarborTypeProperty = DependencyProperty.Register("CurrentHarborType", typeof(HarborType), typeof(GameViewControl), new PropertyMetadata(HarborType.None));
        //  NotifyPropertyChanged("CurrentHarborLocation");
        public static readonly DependencyProperty CurrentHarborLocationProperty = DependencyProperty.Register("CurrentHarborLocation", typeof(HarborLocation), typeof(GameViewControl), new PropertyMetadata(HarborLocation.None));
        //  NotifyPropertyChanged("CurrentTileOrientation");
        public static readonly DependencyProperty CurrentTileOrientationProperty = DependencyProperty.Register("CurrentTileOrientation", typeof(TileOrientation), typeof(GameViewControl), new PropertyMetadata(TileOrientation.None));
        //  NotifyPropertyChanged("EnableInput");
        public static readonly DependencyProperty EnableInputProperty = DependencyProperty.Register("EnableInput", typeof(bool), typeof(GameViewControl), new PropertyMetadata(false));
        //NotifyPropertyChanged("CurrentGame");
        public static readonly DependencyProperty CurrentGameNameProperty = DependencyProperty.Register("CurrentGame", typeof(string), typeof(GameViewControl), new PropertyMetadata(""));
        //NotifyPropertyChanged("Rows");
        public static readonly DependencyProperty RowsProperty = DependencyProperty.Register("Rows", typeof(int), typeof(GameViewControl), new PropertyMetadata(0));
        //NotifyPropertyChanged("Columns");
        public static readonly DependencyProperty ColumnsProperty = DependencyProperty.Register("Columns", typeof(int), typeof(GameViewControl), new PropertyMetadata(0));
        //NotifyPropertyChanged("GameName");
        public static readonly DependencyProperty GameNameProperty = DependencyProperty.Register("GameName", typeof(string), typeof(GameViewControl), new PropertyMetadata(""));
        public static readonly DependencyProperty TileGroupAsStringProperty = DependencyProperty.Register("TileGroupAsString", typeof(string), typeof(GameViewControl), new PropertyMetadata(""));
        public static readonly DependencyProperty SetOrderProperty = DependencyProperty.Register("SetOrder", typeof(bool?), typeof(GameViewControl), new PropertyMetadata(false));
        public static readonly DependencyProperty TileGroupsProperty = DependencyProperty.Register("TileGroups", typeof(string), typeof(GameViewControl), new PropertyMetadata("All"));
        public static readonly DependencyProperty HarborLocationsAsTextProperty = DependencyProperty.Register("HarborLocationsAsText", typeof(string), typeof(GameViewControl), new PropertyMetadata("[Location=None]"));
        public static readonly DependencyProperty CurrentGameTypeProperty = DependencyProperty.Register("CurrentGameType", typeof(GameType), typeof(GameViewControl), new PropertyMetadata(GameType.Regular));

        #endregion
        public string TileGroupAsString
        {
            get
            {
                if (_currentGame != null)
                    return _currentGame.TileGroupsAsString;
                else
                    return "";
            }
            set
            {
                if (_currentGame == null)
                    return;

             //   if (value != _currentGame.TileGroupsAsString)
                {
                    _currentGame.TileGroupsAsString = value;
                    SetValue(TileGroupAsStringProperty, value);
                }
            }
        }

        public OldCatanGame CurrentGame
        {
            get
            {
                return _currentGame;
            }
            set
            {
                if (value != _currentGame)
                {
                    _currentGame = value;
                    SetCurrentGame(value);
                }
            }
        }

        public Array AllGameTypes
        {
            get
            {
                return Enum.GetValues(typeof(GameType));
            }
        }

        public GameType CurrentGameType
        {
            get
            {
                return (GameType)GetValue(CurrentGameTypeProperty);
            }
            set
            {
                if (value != CurrentGameType)
                {
                    SetValue(CurrentGameTypeProperty, value);
                    if (_currentGame != null)
                    {
                        _currentGame.GameType = value;
                    }

                }
            }
        }

        public bool UseClassic
        {
            get
            {
                return (bool)GetValue(UseClassicProperty);
            }

            set
            {

                if ((bool)GetValue(UseClassicProperty) != value)
                {
                    SetValue(UseClassicProperty, value);                   
                }
            }
        }

        bool _designModeSet = false;

        internal void SetDesignMode(bool enterDesignMode)
        {
            _designModeSet = enterDesignMode;
            _currentGame.DesignMode = enterDesignMode;            
            foreach (TileGroup tg in _currentGame.TileGroups)
            {
                foreach (TileCtrl tile in tg.Tiles)
                {
                    tile.ShowIndex = enterDesignMode;                                           
                    tile.SetTileOrientationAsync(TileOrientation.FaceUp, true);
                }
            }

            if (enterDesignMode)
            {
                CreateDesignMenus();
            
            }
            else
            {
                if (_selectedTile != null)
                {
                    OnClose();
                }
            }


        }

        internal void SetDesignModeAsync(bool enterDesignMode)
        {
            _designModeSet = enterDesignMode;
            _currentGame.DesignMode = enterDesignMode;
            foreach (TileGroup tg in _currentGame.TileGroups)
            {
                foreach (TileCtrl tile in tg.Tiles)
                {
                    tile.ShowIndex = enterDesignMode;
                   // tile.GameViewCallback = (this as IGameViewCallback);
                }
            }

            if (enterDesignMode)
            {
                CreateDesignMenus();
                FlipAllTilesAsync(TileOrientation.FaceUp);

            }
            else
            {
                if (_selectedTile != null)
                {
                    OnClose();
                }
            }
        }
    

        private void GameView_KeyUp(CoreWindow sender, KeyEventArgs args)
        {
            object o = FocusManager.GetFocusedElement();
            if (o.GetType() == typeof(TileCtrl))
            {
                Debug.WriteLine($"Focused Element: {o.ToString()} Tile.Index={((TileCtrl)o).Index} Key:{args.VirtualKey}");
            }
            else
            {
                Debug.WriteLine($"Focused Element: {FocusManager.GetFocusedElement()} Type={o.GetType()} Key:{args.VirtualKey}");
            }
        }

        private void Tile_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            //e.Handled = false;
            //TileCtrl tile = sender as TileCtrl;
            //if (StaticHelpers.ExcludeCommonKeys(e))
            //    return;

            //// Debug.WriteLine($"[Tile_KeyUp] Tile.ResourceType={tile.ResourceType} tile.Index={tile.Index} Key={e.Key}");
            //if (e.Key == VirtualKey.Down)
            //{
            //    int idx = _hexPanel.Children.IndexOf((TileCtrl)sender);
            //    idx++;
            //    if (idx == _hexPanel.Children.Count) idx = 0;
            //    //  Debug.WriteLine($"OldTile.Index={((TileCtrl)sender).Index} Key={e.Key} NewTile.Index={(TileCtrl)_hexPanel.Children[idx]} idx={idx}");
            //    SelectTile((TileCtrl)_hexPanel.Children[idx]);
            //    e.Handled = true;
            //    return;
            //}

            //if (e.Key == VirtualKey.Up)
            //{
            //    int idx = _hexPanel.Children.IndexOf((TileCtrl)sender);
            //    idx--;
            //    if (idx < 0) idx = _hexPanel.Children.Count - 1;
            //    SelectTile((TileCtrl)_hexPanel.Children[idx]);
            //    e.Handled = true;
            //    return;
            //}

            return;
        }

        private void Tile_DoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {
            TileCtrl tile = sender as TileCtrl;

            if (tile.TileOrientation == TileOrientation.FaceUp)
                tile.TileOrientation = TileOrientation.FaceDown;
            else
                tile.TileOrientation = TileOrientation.FaceUp;
        }



        private void CreateDesignMenus()
        {
            MenuFromEnum(_menuResourceType, typeof(ResourceType), Menu_OnResourceTypeClicked);
            MenuFromEnum(_menuHarborType, typeof(HarborType), Menu_OnHarborTypeClicked);
            //    MenuFromEnum(_menuHarborLocation, typeof(HarborLocation), Menu_OnHarborLocationClicked);
            MenuFromEnum(_menuNumber, typeof(ValidNumbers), Menu_OnSetNumber);
            FixupHarborLocationTag();
        }

        private void FixupHarborLocationTag()
        {
            foreach (ToggleMenuFlyoutItem item in _menuHarborLocation.Items)
            {
                if (item.Tag.GetType() == typeof(string))
                {
                    HarborLocation location = (HarborLocation)Enum.Parse(typeof(HarborLocation), (string)item.Tag);
                    item.Tag = location;
                }
            }
        }

        private void Menu_OnSetNumber(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem item = sender as ToggleMenuFlyoutItem;

            _selectedTile.Number = (int)(ValidNumbers)item.Tag;
        }

        private void Menu_OnHarborLocationClicked(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem item = sender as ToggleMenuFlyoutItem;
            HarborLocation location = (HarborLocation)item.Tag;
            if (item.IsChecked)
            {
                _selectedTile.AddHarborLocation(location);
                _selectedTile.Harbor?.SetOrientationAsync(TileOrientation.FaceUp);
            }
            else
                _selectedTile.RemoveHarborLocation(location);

            //
            //  the UI will only remember the last one we selected...for now
            SetValue(CurrentHarborLocationProperty, location);

            //  ForceHexPanelUpdate();
        }


        private void Menu_OnHarborTypeClicked(object sender, RoutedEventArgs e)
        {

            //  this.TraceMessage("You need to make it so that this menu pops when you right click on a harbor...double click should cycle through the options");

            ToggleMenuFlyoutItem item = sender as ToggleMenuFlyoutItem;
            _selectedTile.SetHarborType(_clickedHarberLocation, (HarborType)item.Tag);
        }

        private void Menu_OnResourceTypeClicked(object sender, RoutedEventArgs e)
        {
            ToggleMenuFlyoutItem item = sender as ToggleMenuFlyoutItem;

            _selectedTile.ResourceType = (ResourceType)item.Tag;
        }

        private void MenuFromEnum(MenuFlyoutSubItem menu, Type type, RoutedEventHandler onClicked)
        {
            menu.Items.Clear();
            foreach (var v in Enum.GetValues(type))
            {
                ToggleMenuFlyoutItem item = new ToggleMenuFlyoutItem();

                item.Text = v.ToString();
                item.Tag = v;
                item.Click += onClicked;
                menu.Items.Add(item);
            }
        }



        private void SelectTile(TileCtrl newTile)
        {

            if (_selectedTile != null)
            {
                bool UnSelected = (newTile == _selectedTile);
                _selectedTile.Selected = false; // you can select a different one w/o closing the first one
                // boost below so you can see the whole focus rect - otherwise the overlappy rectangles make it confusing where mouse events go
                // the UI hides the boost from the UI. Deboost here
                int zIndex = Canvas.GetZIndex(_selectedTile);
                Canvas.SetZIndex(_selectedTile, zIndex - ZINDEX_BOOST);
                _selectedTile = null;
                if (UnSelected) return;
            }

            if (newTile == null)
            {
                //
                //  can we get into this state?
                Debug.Assert(false, "SelectTile(null) called");
                return;
            }

            newTile.Selected = true;
            _selectedTile = newTile;

            Canvas.SetZIndex(_selectedTile, Canvas.GetZIndex(_selectedTile) + ZINDEX_BOOST);
            //  Debug.WriteLine($"[GameView::SelectTile] Tile.ResourceType={newTile.ResourceType} tile.Index={newTile.Index}");


            EnableInput = true;
            UpdateBindings();



        }

        private void UpdateBindings()
        {
            SetValue(CurrentNumberProperty, _selectedTile.Number);
            SetValue(CurrentResourceTypeProperty, _selectedTile.ResourceType);
            SetValue(CurrentHarborTypeProperty, _selectedTile.HarborType);
            SetValue(CurrentHarborLocationProperty, _selectedTile.HarborLocation);
            SetValue(CurrentTileOrientationProperty, _selectedTile.TileOrientation);
            SetValue(CurrentzIndexProperty, Canvas.GetZIndex(_selectedTile) - ZINDEX_BOOST);
            SetValue(CurrentIndexProperty, _selectedTile.Index);
            SetValue(HarborLocationsAsTextProperty, _selectedTile.HarborLocationsAsShortText);


        }


        private void OnHarborMenuOpening(object sender, object e)
        {
            HarborType current = _selectedTile.GetHarborTypeAtLocation(_clickedHarberLocation);

            foreach (var v in _menuHarborType.Items)
            {
                ToggleMenuFlyoutItem item = v as ToggleMenuFlyoutItem;
                item.IsChecked = (current == ((HarborType)item.Tag));
            }
        }

        private void OnMenuOpening(object sender, object e)
        {
            foreach (var v in _menuResourceType.Items)
            {
                ToggleMenuFlyoutItem item = v as ToggleMenuFlyoutItem;
                if (((ResourceType)item.Tag) == _selectedTile.ResourceType)
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
            }

            foreach (var v in _menuHarborLocation.Items)
            {
                ToggleMenuFlyoutItem item = v as ToggleMenuFlyoutItem;
                if (_selectedTile.IsHarborVisible((HarborLocation)item.Tag))
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;

                }
            }


            foreach (var v in _menuNumber.Items)
            {
                ToggleMenuFlyoutItem item = v as ToggleMenuFlyoutItem;
                if ((int)((ValidNumbers)item.Tag) == _selectedTile.Number)
                {
                    item.IsChecked = true;
                }
                else
                {
                    item.IsChecked = false;
                }
            }
        }

        private void ForceHexPanelUpdate()
        {
            if (_selectedTile == null)
                return;

          //  _hexPanel.Update(new Size(_hexPanel.ActualWidth, _hexPanel.ActualHeight));
        }

        private async void Menu_MakeFaceDown(object sender, RoutedEventArgs e)
        {
            await this.FlipTiles(TileOrientation.FaceDown);
        }

        private async void Menu_MakeFaceUp(object sender, RoutedEventArgs e)
        {
            await this.FlipTiles(TileOrientation.FaceUp);
        }

        public string CurrentGameName
        {
            get
            {
                if (GetValue(CurrentGameNameProperty) != null)
                    return (string)GetValue(CurrentGameNameProperty).ToString();
                else
                    return "";
            }

            set
            {
                if (_currentGameName != value)
                {
                    _currentGameName = value;
                    SetValue(CurrentGameNameProperty, value);
                    SetCurrentGame(_currentGameName);                           
                }
            }
        }

        

        public Array AllResourceTypes
        {
            get
            {
                return Enum.GetValues(typeof(ResourceType));
            }
        }
        public ResourceType CurrentResourceType
        {
            get
            {
                if (_selectedTile != null)
                {

                    ResourceType type = (ResourceType)GetValue(CurrentResourceTypeProperty);
                    this.Assert(_selectedTile.ResourceType == type, "Something funky going on in your properties...");
                    return type;
                }

                this.TraceMessage("_selectedTile = null.  Returning Sheep for resource type");
                return ResourceType.Sheep; ;
            }
            set
            {
                if (_selectedTile != null)
                {
                    if (_selectedTile.ResourceType != value)
                    {
                        _selectedTile.ResourceType = value;
                        SetValue(CurrentResourceTypeProperty, value);
                    }

                }
            }
        }

        public Array AllHarborTypes
        {
            get
            {
                return Enum.GetValues(typeof(HarborType));
            }
        }
        public HarborType CurrentHarborType
        {
            get
            {
                //  this.TraceMessage("Harbortype broken here too");
                if (_selectedTile != null)
                {
                    if (_selectedTile.HarborLocation == HarborLocation.None)
                        return HarborType.None;

                    HarborType type = (HarborType)GetValue(CurrentHarborTypeProperty);
                    this.Assert(type == _selectedTile.HarborType, "bad prop");
                    return type;
                }

                return HarborType.None;
            }
            set
            {
                if (_selectedTile != null)
                {
                    if (_selectedTile.HarborType != value)
                    {
                        _selectedTile.HarborType = value;
                        SetValue(CurrentHarborTypeProperty, value);
                    }
                }
            }
        }
        List<CheckBox> _checkBoxList = new List<CheckBox>();
        public Array AllHarborLocations
        {
            get
            {
                _checkBoxList.Clear();
                foreach (var value in Enum.GetValues(typeof(HarborLocation)))
                {
                    CheckBox cb = new CheckBox();
                    cb.Content = value.ToString();
                    cb.IsChecked = true;
                    cb.Checked += HarborLocation_Checked;
                    cb.Unchecked += HarborLocation_Checked;
                    cb.Tag = value;
                    _checkBoxList.Add(cb);

                }
                return _checkBoxList.ToArray();
            }
        }

        public string HarborLocationsAsText
        {
            get
            {
                if (_selectedTile != null)
                {
                    return (string)GetValue(HarborLocationsAsTextProperty);
                }
                return "<none>";

            }
            set
            {
                if (value != (string)GetValue(HarborLocationsAsTextProperty))
                {
                    SetValue(HarborLocationsAsTextProperty, value);
                }
            }
        }


        public string TileGroupsAsString
        {
            get
            {
                return (string)GetValue(TileGroupsProperty);
            }
            set
            {
                if (value != (string)GetValue(TileGroupsProperty))
                {
                    SetValue(TileGroupsProperty, value);
                }
            }
        }


        private bool _stopCheckBoxRecursion = false;
        private void HarborLocation_Checked(object sender, RoutedEventArgs e)
        {
            CheckBox cb = sender as CheckBox;
            if (_stopCheckBoxRecursion) return;
            try
            {
                _stopCheckBoxRecursion = true;

                HarborLocation location = (HarborLocation)cb.Tag;

                if (location == HarborLocation.None)
                {

                    for (int i = 1; i < _checkBoxList.Count; i++) // skip the first one
                    {
                        _checkBoxList[i].IsChecked = false;
                    }
                }
                else
                {
                    _checkBoxList[0].IsChecked = false;
                }



                if (_selectedTile != null)
                {
                    if ((bool)cb.IsChecked)
                    {
                        _selectedTile.AddHarborLocation(location);
                    }
                    else
                    {
                        _selectedTile.RemoveHarborLocation(location);
                    }

                    HarborLocationsAsText = _selectedTile.HarborLocationsAsShortText;

                }
            }
            finally
            {
                _stopCheckBoxRecursion = false;
            }
        }

        public TileCtrl SelectedTile
        {
            get
            {
                return _selectedTile;
            }
        }



        public HarborLocation CurrentHarborLocations
        {
            get
            {

                if (_selectedTile != null)
                {
                    return (HarborLocation)GetValue(CurrentHarborLocationProperty);
                }

                return HarborLocation.None;
            }
            set
            {
                if (_selectedTile != null)
                {
                    if (_selectedTile.HarborLocation != value)
                    {
                        _selectedTile.HarborLocation = value;
                        SetValue(CurrentHarborLocationProperty, value);
                        ForceHexPanelUpdate();
                    }

                }
            }
        }

        public Array TileOrientations
        {
            get
            {
                return Enum.GetValues(typeof(TileOrientation));
            }
        }

        public TileOrientation CurrentTileOrientation
        {
            get
            {
                if (_selectedTile != null)
                {
                    return (TileOrientation)GetValue(CurrentTileOrientationProperty);
                }

                return TileOrientation.None;
            }
            set
            {
                if (_selectedTile != null)
                {
                    if (_selectedTile.TileOrientation != value)
                    {

                        _selectedTile.SetTileOrientationAsync(value, true);                        
                        SetValue(CurrentTileOrientationProperty, value);
                    }

                }
            }
        }

        public int CurrentNumber
        {
            get
            {

                if (_selectedTile != null)
                {
                    return (int)GetValue(CurrentNumberProperty);
                }              
                return 13;
            }
            set
            {
                if (_selectedTile != null)
                {
                    if (_selectedTile.Number != value)
                    {
                        try
                        {
                            _selectedTile.Number = value;
                            SetValue(CurrentNumberProperty, value);
                        }
                        catch
                        {

                        }

                    }
                }


            }
        }
        private const int ZINDEX_BOOST = 20;
        public int CurrentzIndex
        {
            get
            {
                if (_selectedTile != null)
                {
                    return (int)GetValue(CurrentzIndexProperty);
                }

                return -2;
            }
            set
            {
                if (_selectedTile != null)
                {
                    if ((int)GetValue(CurrentzIndexProperty) != value)
                    {
                        Canvas.SetZIndex(_selectedTile, value + ZINDEX_BOOST);
                        SetValue(CurrentzIndexProperty, value);
                    }

                }
            }
        }

        public int CurrentIndex
        {
            get
            {
                if (_selectedTile != null)
                    return (int)GetValue(CurrentIndexProperty);

                return -2;
            }
            set
            {
                if (_selectedTile != null)
                {
                    if (_selectedTile.Index != value)
                    {
                        SetValue(CurrentIndexProperty, value);
                        _selectedTile.Index = value;
                    }

                }
            }
        }



        public void CreateBoard()
        {
            //  if (Rows * Columns == 0)
            //    return;

            //_hexPanel.Children.Clear();
            //_hexPanel.Columns = Columns;
            //_hexPanel.Rows = Rows;
            //_hexPanel.NormalHeight = 96;
            //_hexPanel.NormalWidth = 110;

            //int rowsToLeft = Columns / 2;
            //int total = 0;
            //for (int i = 1; i <= rowsToLeft; i++)
            //{
            //    total += Rows - i;
            //}
            //total = total * 2 + Rows;
            
            //TileGroup tileGroup = new TileGroup();

            //Random r = new Random((int)DateTime.Now.Ticks);
            //for (int i = 0; i < total; i++)
            //{
            //    TileCtrl tile = new TileCtrl();
            //    tile.TileOrientation = TileOrientation.FaceUp; 
            //    tile.HarborLocation = HarborLocation.None;
            //    tile.ResourceType = (ResourceType)r.Next(1, 6);
            //    tile.UseClassic = UseClassic;
            //    tile.Number = r.Next(2, 12);
            //    tile.Index = i;
            //    tile.zIndex = 0;
            //    tileGroup.Tiles.Add(tile);
            //    _hexPanel.Children.Add(tile);


            //}


            //OldCatanGame game = new OldCatanGame();
            
            //game.Columns = Columns;
            //game.Rows = Rows;
            //game.TileGroups.Add(tileGroup);
            //game.GameName = GameName;
            //game.NumberOfPlayers = NumberOfPlayers;
            //game.TileGroupsAsString = String.Format($"0-{total}.True");
            //_games.Add(game);
            //SavedGames.Add(GameName);
            //SetCurrentGame(game);


            //SetDesignMode(true);

        }

        internal void DeleteCurrentGame(string currentGameAsString)
        {
            if (_currentGame.GameName != currentGameAsString)
                return;
        }



        public void OnClose()
        {
            if (_selectedTile != null)
            {
                _selectedTile.Selected = false;
                _selectedTile = null;
            }
        }

        //public new void KeyUp(KeyRoutedEventArgs e)
        //{
        //    throw new NotImplementedException();
        //}

        //public void TileSelected(TileCtrl tile)
        //{

        //    SelectTile(tile);
        //}

        //public void UpdateHexLayout()
        //{
        //    ForceHexPanelUpdate();
        //}

        public void OnGridLeftTapped(TileCtrl tile, TappedRoutedEventArgs e)
        {
            if (!_designModeSet) return;

            SelectTile(tile);

            if ((bool)SetOrder == true)
            {
                tile.Index = _tileIndex++;
                if (_tileIndex == this.TotalTiles) _tileIndex = 0;

                //for (int i = _tileIndex; i<_tiles.Count; i++)
                //{
                //    _tiles[i].Index = i;
                //}

            }



        }

        public int TotalTiles
        {
            get
            {
                return _currentGame.TilesByHexOrder.Count;

                //int total = 0;
                //foreach (TileGroup tg in _currentGame.TileGroups)
                //{
                //    total += tg.Tiles.Count;
                //}
                //return total;
            }
        }

        
        public void OnGridRightTapped(TileCtrl tile, RightTappedRoutedEventArgs rte)
        {
             
            if (_designModeSet)
            {
                if (tile != _selectedTile)
                    SelectTile(tile);

                _menuMain.ShowAt(tile, rte.GetPosition(tile));
            }
           

            
        }

       

        public void OnTileDoubleTapped(object sender, DoubleTappedRoutedEventArgs e)
        {


        }
        HarborLocation _clickedHarberLocation = HarborLocation.None;
        public void OnHarborRightTapped(TileCtrl tile, HarborLocation location, RightTappedRoutedEventArgs e)
        {
            if (!_designModeSet) return;

            if (tile != _selectedTile)
                SelectTile(tile);

            _clickedHarberLocation = location;
            _menuHarbor.ShowAt(tile, e.GetPosition(tile));
        }
    }
}