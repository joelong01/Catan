using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Search;
using Windows.System;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class DesignerPage : Page, INotifyPropertyChanged

    {


        bool _initializing = true;

        public int Rows { get; set; } = 0;
        public int Columns { get; set; } = 0;
        public int NumberOfTiles { get; set; } = 6;
        public bool EnableInput { get; set; } = false;
        public string GameName { get; set; } = "Seafarers Game 1";
        public string NumberOfPlayers { get; set; } = "3-4";

        private string _currentGame = "";
        private ObservableCollection<string> _savedGameNames = new ObservableCollection<string>();
        private List<StorageFile> _savedFiles = null;

        public HarborLocation[] HarborLocations { get; } = { HarborLocation.Bottom, HarborLocation.BottomLeft, HarborLocation.BottomRight, HarborLocation.None, HarborLocation.Top, HarborLocation.TopLeft, HarborLocation.TopRight };
        public ResourceType[] ResourceTypes { get; } = { ResourceType.Sheep, ResourceType.Wood, ResourceType.Ore, ResourceType.Wheat, ResourceType.Brick, ResourceType.Desert, ResourceType.Back, ResourceType.None, ResourceType.Sea };
        public TileOrientation[] TileOrientations { get; } = { TileOrientation.FaceDown, TileOrientation.FaceUp };
        public HarborType[] HarborTypes = { HarborType.Sheep, HarborType.Wood, HarborType.Ore, HarborType.Wheat, HarborType.Brick, HarborType.ThreeForOne };

        public event PropertyChangedEventHandler PropertyChanged;



        TileCtrl _currentTile = null;
        TileCtrl _tileNull = null;

        public DesignerPage()
        {
            this.InitializeComponent();
            this.Loaded += DesignerPage_Loaded;


            Rows = 7;
            Columns = 7;

            int i = 0;
            foreach (var child in _hexPanel.Children)
            {
                child.PointerPressed += Tile_PointerPressed;
                ((TileCtrl)child).Harbor?.SetOrientationAsync(TileOrientation.FaceUp, Double.MaxValue);
                ((TileCtrl)child).Index = i++;
            }

            CurrentTile = (TileCtrl)_hexPanel.Children[0];


            _tileNull = new TileCtrl();  // a tile that makes the binding work that isn't in the Panel and isn't visible
            _tileNull.Visibility = Visibility.Collapsed;


        }


        private async void DesignerPage_Loaded(object sender, RoutedEventArgs e)
        {
            ApplicationView.GetForCurrentView().TryEnterFullScreenMode();
            await SaveDefaultGamesLocally();
            var files = await GetSavedFiles();
            _savedFiles = new List<StorageFile>();

            foreach (var file in files)
            {
                _savedGameNames.Add(file.DisplayName);
                _savedFiles.Add(file);
            }

            if (_savedFiles.Count > 0)
            {
                CurrentGame = _savedGameNames[0];

                NotifyPropertyChanged("SavedGames");
                NotifyPropertyChanged("CurrentGame");

            }


            _initializing = false;
        }

        public static async Task<IReadOnlyList<StorageFile>> GetSavedFiles()
        {
            
            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new[] { ".catangame" });
            queryOptions.FolderDepth = FolderDepth.Shallow;
            var folder = ApplicationData.Current.LocalFolder;
            var query = folder.CreateFileQueryWithOptions(queryOptions);
            var files = await query.GetFilesAsync();            
            return files;
        }

        /// <summary>
        ///  I have the Regular and Expansion games embedded as resource files.  this will update them everytime the app starts.
        /// </summary>
        /// <returns></returns>
        private async Task SaveDefaultGamesLocally()
        {
            string game = "";
            Assembly assembly = Assembly.Load(new AssemblyName("Catan Game Board"));
            var folder = ApplicationData.Current.LocalFolder;
            var option = CreationCollisionOption.ReplaceExisting;
            char[] sep = new char[] { '.' };

            var resourceNames = assembly.GetManifestResourceNames();
            string dt = await ReadTextFromResource(assembly, "BuildDate.dat");
            DateTime buildTime = DateTime.Parse(dt);
            foreach (var name in assembly.GetManifestResourceNames())
            {
                using (var stream = assembly.GetManifestResourceStream(name))
                {
                    
                    string[] tokens = name.Split(sep);
                    int count = tokens.Count();
                    string fn = tokens[count - 2] + "." + tokens[count - 1];
                    if (tokens[count - 1] != "catangame")
                        continue;

                    //
                    //  Check to see if it is there.  if it is, don't mess with it.  Who knows what happened?
                    IStorageItem isiLocal = await folder.TryGetItemAsync(fn);
                    if (isiLocal != null)
                    {
                        if (isiLocal.DateCreated > buildTime)
                        {
                            continue;
                        }
                    }
                    //
                    //  not there -- copy it from the resources
                    using (var reader = new StreamReader(stream))
                    {
                        game = await reader.ReadToEndAsync();
                        
                        var file = await folder.CreateFileAsync(fn, option);
                        await FileIO.WriteTextAsync(file, game);
                    }
                }
            }
  
        }

        private async Task<string> ReadTextFromResource(Assembly assembly, string name)
        {
            string resourcePrefix = "Catan10.Assets.Games.";
            if (!name.StartsWith(resourcePrefix))
                name = resourcePrefix + name;

            string txt = "";
            using (var stream = assembly.GetManifestResourceStream(name))
            {
                using (var reader = new StreamReader(stream))
                {
                    txt = await reader.ReadToEndAsync();                  
                }
            }

            return txt;
        }

        public TileCtrl CurrentTile
        {
            get
            {
                return _currentTile;
            }

            set
            {
                try
                {
                    if (value == null && _currentTile == null)
                    {
                        return;
                    }



                    if (value == null && _currentTile != null)
                    {
                        TileSelected(_currentTile, false);
                        return;
                    }

                    // value != null

                    if (_currentTile == value)
                    {
                        //DeSelect Current
                        TileSelected(_currentTile, false);
                        _currentTile = _tileNull;
                        return;
                    }

                    if (_currentTile != _tileNull  && _currentTile != null)
                    {
                        TileSelected(_currentTile, false);
                    }


                    TileSelected(value, true);
                    _currentTile = value;
                }
                catch
                {

                }
                finally
                {
                    NotifyPropertyChanged("EnableInput");
                    NotifyPropertyChanged("TileOrientation");
                    NotifyPropertyChanged("HarborType");
                    NotifyPropertyChanged("HarborLocation");
                    NotifyPropertyChanged("zIndex");
                    NotifyPropertyChanged("ResourceType");
                    NotifyPropertyChanged("Number");
                    NotifyPropertyChanged("UseClassic");
                    NotifyPropertyChanged("Index");
                    NotifyPropertyChanged("NumberOfPlayers");
                }
            }

        }



        public ResourceType ResourceType
        {
            get
            {
                if (CurrentTile == null)
                    return ResourceType.Back;

                return CurrentTile.ResourceType;
            }
            set
            {
                if (CurrentTile != null)
                    CurrentTile.ResourceType = value;
            }
        }

        public TileOrientation TileOrientation
        {
            get
            {
                if (CurrentTile == null)
                    return TileOrientation.FaceDown;

                return CurrentTile.TileOrientation;
            }
            set
            {
                if (CurrentTile != null)
                {
                    CurrentTile.TileOrientation = value;
                    if (CurrentTile.Harbor != null)
                        CurrentTile.Harbor.Orientation = value;
                }
            }
        }
        public HarborLocation HarborLocation
        {
            get
            {
                if (CurrentTile == null)
                    return HarborLocation.None;

                return CurrentTile.HarborLocation;
            }
            set
            {
                if (CurrentTile == null)
                    return;

                if (CurrentTile.HarborLocation == value)
                    return;

                if (CurrentTile != null)
                {
                    CurrentTile.HarborLocation = value;
                    if (!_initializing)
                        UpdateTile();
                }
            }
        }

        public HarborType HarborType
        {
            get
            {
                if (CurrentTile == null)
                    return HarborType.Wheat;

                return CurrentTile.HarborType;
            }
            set
            {
                if (CurrentTile != null)
                {
                    CurrentTile.HarborType = value;
                }
            }
        }
        public int zIndex
        {
            get
            {
                if (CurrentTile == null)
                    return -2;

                return Canvas.GetZIndex(CurrentTile);
            }
            set
            {
                if (CurrentTile != null)
                    Canvas.SetZIndex(CurrentTile, value);
            }
        }

        public int Number
        {
            get
            {
                if (CurrentTile == null)
                    return -2;

                return CurrentTile.Number;
            }
            set
            {
                if (CurrentTile != null)
                    CurrentTile.Number = value;
            }
        }

        public int Index
        {
            get
            {
                if (CurrentTile == null)
                    return -2;

                return CurrentTile.Index;
            }
            set
            {
                if (CurrentTile != null)
                    CurrentTile.Index = value;
            }
        }

        public string CurrentGame
        {
            get
            {
                return _currentGame;
            }

            set
            {
                _currentGame = value;
                if (value != null)
                {
#pragma warning disable 4014
                    Dispatcher.RunAsync(CoreDispatcherPriority.Normal, async () =>
                    {
                        await LoadCurrentFile();
                    });
#pragma warning restore
                }


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
                _savedGameNames = value;
            }
        }

        private void FaceUp_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void UseClassic_Checked(object sender, RoutedEventArgs e)
        {

        }

        private void SetOrder_Checked(object sender, RoutedEventArgs e)
        {
            _index = 0;
        }

        private async void OnSave(object sender, RoutedEventArgs e)
        {


            try
            {

                string saveString = GetSaveString();
                if (saveString == "")
                    return;


                var folder = ApplicationData.Current.LocalFolder;
                var option = CreationCollisionOption.ReplaceExisting;
                var file = await folder.CreateFileAsync(GameName + ".catangame", option);
                await FileIO.WriteTextAsync(file, saveString);

                if (CurrentGame != GameName)
                {
                    _savedGameNames.Add(GameName);
                    _savedFiles.Add(file);
                    CurrentGame = GameName;
                    NotifyPropertyChanged("SavedGames");
                    NotifyPropertyChanged("CurrentGame");
                }


            }
            catch (Exception exception)
            {

                string s = StaticHelpers.GetErrorMessage($"Error saving to file {GameName}", exception);
                MessageDialog dlg = new MessageDialog(s);
                await dlg.ShowAsync();

            }

        }

        string SERIALIZATION_VERSION = "1.0";

        private string GetSaveString()
        {
            string nl = StaticHelpers.lineSeperator;
            string s = "[Game]" + nl;

            s += StaticHelpers.SetValue("Rows", Rows);
            s += StaticHelpers.SetValue("Columns", Columns);
            s += StaticHelpers.SetValue("GameName", GameName);

            List<int> indeces = new List<int>();
            foreach (TileCtrl tile in _hexPanel.Children)
            {
                indeces.Add(tile.Index);
            }

            s += StaticHelpers.SetValue("TileIndexArray", StaticHelpers.SerializeList<int>(indeces));
            s += StaticHelpers.SetValue("TileCount", _hexPanel.Children.Count);
            s += StaticHelpers.SetValue("NumberOfPlayers", NumberOfPlayers);
            s += StaticHelpers.SetValue("Version", SERIALIZATION_VERSION);
            s += StaticHelpers.SetValue("UseClassic", "True");
            s += StaticHelpers.SetValue("Randomize", "True");
            s += StaticHelpers.SetValue("GameType", GameType.SupplementalBuildPhase);
            s += nl;
            int count = 0;
            foreach (TileCtrl tile in _hexPanel.Children)
            {
                s += String.Format($"[Tile {count.ToString()}]\n{tile.Serialize(false)}\n");
                count++;
            }

            return s;
        }

        

        private StorageFile GetCurrentFile()
        {
            int idx = _savedGameNames.IndexOf(_currentGame);
            StorageFile file = _savedFiles[idx];
            return file;
        }

        private async Task LoadCurrentFile()
        {
            StorageFile file = GetCurrentFile();
            try
            {
                using (var stream = await file.OpenStreamForReadAsync())
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        string savedGame = streamReader.ReadToEnd();
                        string error = ParseAndLoadGame(savedGame);
                        if (error != "")
                        {
                            await DeleteCurrentGame();
                        }


                    }
                }
            }
            catch
            {
                if (await StaticHelpers.AskUserYesNoQuestion($"Error Loading file {file.DisplayName}.\n\nDelete it?", "Yes", "No") == true)
                {
                    int idx = _savedFiles.IndexOf(file);
                    _savedFiles.RemoveAt(idx);
                    _savedGameNames.RemoveAt(idx);
                    await file.DeleteAsync();

                }
            }
        }

        private string GetValue(Dictionary<string, string> dict, string key)
        {

            string s = "";

            if (dict.TryGetValue(key, out s))
                return s;

            string error = String.Format($"Key {key} not found.");
            throw new Exception(error);

        }



        private string ParseAndLoadGame(string savedGame)
        {
            Dictionary<string, string> sections = null;
            string error = "";
            try
            {
                sections = StaticHelpers.GetSections(savedGame);
                if (sections == null)
                {
                    error = String.Format($"Error parsing the file into sections.\nThere are no sections.  Please load a valid .catangame file.");
                    return error;
                }
            }
            catch (Exception e)
            {
                return String.Format($"Error parsing the file into sections.\n{e.Message}");

            }
            int tileCount = 0;
            Dictionary<string, string> Game = StaticHelpers.DeserializeDictionary(sections["Game"]);
            try
            {
                _hexPanel.DisableLayout = true; ;
                Rows = Int32.Parse(GetValue(Game, "Rows"));
                NotifyPropertyChanged("Rows");
                _hexPanel.Rows = Rows;
                Columns = Int32.Parse(GetValue(Game, "Columns"));
                NotifyPropertyChanged("Columns");
                _hexPanel.Columns = Columns;
                GameName = GetValue(Game, "GameName");
                NotifyPropertyChanged("GameName");
                NumberOfPlayers = GetValue(Game, "NumberOfPlayers");
                NotifyPropertyChanged("NumberOfPlayers");
                tileCount = Int32.Parse(GetValue(Game, "TileCount"));
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                _hexPanel.DisableLayout = false;
            }

            _hexPanel.Children.Clear();


            for (int i = 0; i < tileCount; i++)
            {
                TileCtrl tile = new TileCtrl();
                tile.PointerPressed += Tile_PointerPressed;
                string s = (string)sections["Tile " + i.ToString()];
                tile.Deserialize(s, false);
                if (tile.Harbor != null)
                    tile.Harbor.SetOrientationAsync(TileOrientation.FaceUp, Double.MaxValue);
                _hexPanel.Children.Add(tile);
            }

            CurrentTile = (TileCtrl)_hexPanel.Children[0];
            _hexPanel.DisableLayout = false; ;
            _hexPanel.UpdateLayout();
            return "";
        }
        private async void CreateBoard(object sender, RoutedEventArgs e)
        {
            if (Rows * Columns == 0)
                return;

            _hexPanel.Children.Clear();
            _hexPanel.Columns = Columns;
            _hexPanel.Rows = Rows;
            _hexPanel.NormalHeight = 96;
            _hexPanel.NormalWidth = 110;

            int rowsToLeft = Columns / 2;
            int total = 0;
            for (int i = 1; i <= rowsToLeft; i++)
            {
                total += Rows - i;
            }

            total = total * 2 + Rows;
            List<Task> taskList = new List<Task>();
            for (int i = 0; i < total; i++)
            {
                TileCtrl tile = new TileCtrl();
                tile.PointerPressed += Tile_PointerPressed;
                tile.TileOrientation = TileOrientation.FaceUp;
                tile.SetTileOrientation(TileOrientation.FaceUp, taskList, 0);
                tile.HarborLocation = HarborLocation.None;
                tile.ShowIndex = true;
                if (tile.Harbor != null)
                    tile.Harbor.SetOrientationAsync(TileOrientation.FaceUp, 10);
                _hexPanel.Children.Add(tile);


            }

            await Task.WhenAll(taskList);

            CurrentTile = _hexPanel.Children[0] as TileCtrl;


        }

        int _index = 0;

        private void Tile_PointerPressed(object sender, PointerRoutedEventArgs e)
        {
            CurrentTile = sender as TileCtrl;
            if ((bool)_chkSetOrder.IsChecked == true)
            {
                CurrentTile.ShowIndex = true;
                CurrentTile.Index = _index++;
                if (_index == _hexPanel.Children.Count)
                {
                    _index = 0;
                }
               
            }
        }
        private void TileSelected(TileCtrl newSelectedTile, bool selected)
        {


            newSelectedTile.Selected = selected;
            EnableInput = selected;

        }

        private void TextBox_GotFocus(object sender, RoutedEventArgs e)
        {
            TextBox tb = sender as TextBox;
            tb.SelectAll();
        }

        private void TextBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (CurrentTile == null)
                return;

            try
            {
                int n = Int32.Parse(_txtNumber.Text);
                CurrentTile.Number = n;
            }
            catch { }
            CurrentTile.UpdateLayout();
        }

        private void ComboBox_SelectedChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CurrentTile == null)
                return;


        }


        private void UpdateTile()
        {
            if (CurrentTile == _tileNull)
                return;

            int idx = _hexPanel.Children.IndexOf(CurrentTile);
            if (idx == -1)
                return;

            _hexPanel.DisableLayout = true;
            _hexPanel.Children.Remove(CurrentTile);
            TileCtrl ctrl = new TileCtrl();
            try
            {
                //
                //  I use this so that I go through the same path as I do when I save to disc...
                string s = CurrentTile.Serialize(false);
                ctrl.Deserialize(s, false);
                ctrl.PointerPressed += Tile_PointerPressed;
            }
            catch
            {

            }
            finally
            {
                _hexPanel.DisableLayout = false;
            }

            _hexPanel.Children.Insert(idx, ctrl);
            CurrentTile = ctrl;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }

        private void HexPanel_KeyUp(object sender, KeyRoutedEventArgs e)
        {
            if (e.Key == VirtualKey.Left)
            {

            }
            else if (e.Key == VirtualKey.Down)
            {
                int idx = _hexPanel.Children.IndexOf(CurrentTile);
                if (idx != -1)
                {
                    idx++;
                    CurrentTile = (TileCtrl)_hexPanel.Children[idx];
                }

            }
            else if (e.Key == VirtualKey.Right)
            {

            }
            else if (e.Key == VirtualKey.Up)
            {

            }
        }

        private async void OnDelete(object sender, RoutedEventArgs e)
        {
            try
            {
                ((Button)sender).IsEnabled = false;
                await DeleteCurrentGame();
            }
            catch
            {


            }
            finally
            {
                ((Button)sender).IsEnabled = true;
            }
        }

        private async Task DeleteCurrentGame()
        {
            StorageFile file = GetCurrentFile();
            if (await StaticHelpers.AskUserYesNoQuestion($"Delete \"{CurrentGame}\"?", "Yes", "No") == true)
            {

                
                int idx = _savedFiles.IndexOf(file);
                _savedFiles.RemoveAt(idx);
                _savedGameNames.RemoveAt(idx);
                await file.DeleteAsync();
                if (_savedGameNames.Count > 0)
                {
                    _currentGame = _savedGameNames[0];                    
                    NotifyPropertyChanged("CurrentGame");

                }

            }
        }

        private void OnClose(object sender, RoutedEventArgs e)
        {
            if (Frame.CanGoBack)
            {

                Frame.GoBack();
            }
            else
            {
                this.Frame.Navigate(typeof(MainPage));
            }
        }
    }
}
