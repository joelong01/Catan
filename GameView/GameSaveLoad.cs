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
        private ObservableCollection<string> _savedGameNames = new ObservableCollection<string>();  // the list of games put into the combobox
        private Dictionary<string, StorageFile> _savedStoragefiles = new Dictionary<string, StorageFile>();
        private ObservableCollection<OldCatanGame> _games = new ObservableCollection<OldCatanGame>();
        private OldCatanGame _currentGame = null;

        
        public string CurrentFileName
        {
            get
            {
                return _currentGame.GameName + ".catangame";
            }
        }

        //
        //  opens and parses the file into a Game object
        public async Task<OldCatanGame> LoadGame(StorageFile file)
        {
            string error = "";
            OldCatanGame game = null;
            try
            {

                using (var stream = await file.OpenStreamForReadAsync())
                {
                    using (var streamReader = new StreamReader(stream))
                    {
                        string savedGame = streamReader.ReadToEnd();
                        game = new OldCatanGame();
                       
                        game.FastDeserialize(savedGame);

                        if (game.Error != null)
                        {
                            await DeleteCurrentGame(file, error);

                        }

                    }
                }
            }
            catch (Exception e)
            {
                CurrentGameName = "";
                if (await StaticHelpers.AskUserYesNoQuestion($"Error Loading file {file.DisplayName}.\n\nDelete it?\nException: {e.Message}\n\n{e.InnerException}\n\n{e.StackTrace}", "Yes", "No") == true)
                {
                    await file.DeleteAsync();

                }
            }

            return game;
        }
        public async Task<StorageFile> Save()
        {
            try
            {
                string saveString = _currentGame.Serialize();
                if (saveString == "")
                    return null;

                string fileName = GameName + ".catangame";


                var folder = await StaticHelpers.GetSaveFolder();
                IStorageItem isiLocal = await folder.TryGetItemAsync(fileName);
                if (isiLocal != null)
                {
                    var ret = await StaticHelpers.AskUserYesNoQuestion($"Overwrite {fileName}?", "yes", "no");
                    if (!ret)
                        return null;
                }

                var option = CreationCollisionOption.ReplaceExisting;
                var file = await folder.CreateFileAsync(fileName, option);
                await FileIO.WriteTextAsync(file, saveString);

                if (CurrentGameName != GameName)
                {
                    _savedGameNames.Add(GameName);
                    _savedStoragefiles.Add(GameName, file);
                    CurrentGameName = GameName;
                }

                return file;

            }
            catch (Exception exception)
            {

                string s = StaticHelpers.GetErrorMessage($"Error saving to file {GameName}", exception);
                MessageDialog dlg = new MessageDialog(s);
                await dlg.ShowAsync();
            }
            return null;
        }
        public bool Randomize
        {
            get
            {
                if (_currentGame != null)
                {
                    return _currentGame.Randomize;
                }
                return true;
            }
            set
            {

                if (_currentGame != null)
                    _currentGame.Randomize = value;

            }
        }

        /// <summary>
        ///  I have the Regular and Expansion games embedded as resource files.  this will update them everytime the app starts.
        /// </summary>
        /// <returns></returns>
        public async Task SaveDefaultGamesLocally()
        {
            string game = "";
            Assembly assembly = Assembly.Load(new AssemblyName("Catan Game Board"));
            var folder = await StaticHelpers.GetSaveFolder();
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
        public async Task LoadAllGames()
        {

            await SaveDefaultGamesLocally();
            
            IReadOnlyList<StorageFile> files = await GetSavedFilesInternal();
            foreach (StorageFile file in files)
            {
                OldCatanGame game = await LoadGame(file);
                if (game.Error == null)
                    _games.Add(game);               
             
            }

            CurrentGameName = "Regular";
        }

        public ObservableCollection<string> GetSavedGameNames()
        {
            ObservableCollection<string> fileNames = new ObservableCollection<string>();
            foreach (OldCatanGame game in _games)
            {
                fileNames.Add(game.GameName);
            }

            return fileNames;
        }


        public async Task<IReadOnlyList<StorageFile>> GetSavedFilesInternal()
        {
            var queryOptions = new QueryOptions(CommonFileQuery.DefaultQuery, new[] { ".catangame" });
            queryOptions.FolderDepth = FolderDepth.Shallow;
            var folder = await StaticHelpers.GetSaveFolder();
            var query = folder.CreateFileQueryWithOptions(queryOptions);
            var files = await query.GetFilesAsync();
            _savedGameNames.Clear();
            _savedStoragefiles.Clear();
            foreach (var f in files)
            {
                _savedGameNames.Add(f.DisplayName);
                _savedStoragefiles.Add(f.DisplayName, f);
            }
            
            return files;
        }

        public bool SetCurrentGame(string gameName)
        {
            foreach (OldCatanGame game in _games)
            {
                if (game.GameName == gameName)
                {
                    SetCurrentGame(game);
                    return true;
                }
            }

            return false;
        }

        public void SetCurrentGame(OldCatanGame game)
        {
            //_hexPanel.Children.Clear();
            //_hexPanel.Rows = game.Rows;
            //_hexPanel.Columns = game.Columns;
            //_hexPanel.NormalHeight = 96;
            //_hexPanel.NormalWidth = 110;
           
            //if (game.TilesByHexOrder.Count == 0)
            //{
            //    game.LoadRestOfGame();
            //}
        
            //foreach (TileCtrl tile in game.TilesByHexOrder)
            //{
            //    _hexPanel.Children.Add(tile);
            //    if (_designModeSet)
            //        tile.SetTileOrientationAsync(TileOrientation.FaceUp, true);
            //}
            ////
            ////  TODO: set the harbors

            //_currentGame = game;
            //this.Rows = game.Rows;
            //this.Columns = game.Columns;
            //this.GameName = game.GameName;
            //this.NumberOfPlayers = game.NumberOfPlayers;
            //this.UseClassic = game.UsesClassicTiles;
            //this.Randomize = game.Randomize;
            //this.GameType = game.GameType;            
            //this.TileGroupAsString = game.TileGroupsAsString;
            //this.CurrentGameName = game.GameName;


            
            //SetDesignModeAsync(_designModeSet);
            //_hexPanel.UpdateLayout();

            //BuildRoadDictionary();
            
        }


        private async Task DeleteCurrentGame(StorageFile file, string message)
        {
            if (await StaticHelpers.AskUserYesNoQuestion($"Error loading \"{file.DisplayName}\"\n\nError: {message}\n\nDelete \"{file.DisplayName}\" ", "Yes", "No") == true)
            {
                await file.DeleteAsync();
            }
        }
    }

 

   
}