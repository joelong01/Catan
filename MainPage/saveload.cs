using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.AccessCache;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;


namespace Catan10
{

    public sealed partial class MainPage : Page
    {


        public const string PlayerDataFile = "players.data";
        private const string SERIALIZATION_VERSION = "3";
       

        private string GetSaveString()
        {
            string s = "";
            return s;
/*
            string nl = StaticHelpers.lineSeperator;

            

            if (_displayName == "")
                GetSaveFileName(true); // sets display name

            s += "[Data]" + nl;
            s += StaticHelpers.SetValue("Version", SERIALIZATION_VERSION);
            s += StaticHelpers.SetValue("DisplayName", _displayName);
            s += _gameView.SerializeGroupTilesAndHarbors();            
            s += String.Format($"GameState={State.GameState}{nl}");
            s += String.Format($"Turn={CurrentPlayer?.PlayerName}{nl}");
            s += String.Format($"GameName={_gameView?.GameName}{nl}");
            s += String.Format($"Players={StaticHelpers.SerializeListWithProperty<PlayerView>(PlayingPlayers, "PlayerName")}{nl}");

            s += nl;
            s += SaveGameState();

            s += "[Log]" + nl;

            // s += StaticHelpers.SerilizeListToSection<LogEntry>(_log, "LogEntry");
            return s;
 */

        }

        internal string SaveGameState()
        {
            return "not implemented.  sorry";

           // string s = "";
            //foreach (PlayerView p in PlayingPlayers)
            //{
            //    s += String.Format($"[{p.PlayerName}]{StaticHelpers.lineSeperator}");
            //    s += p.GameSerialize();
            //    s += StaticHelpers.lineSeperator;
            //}

           // return s;
        }

        string _displayName = "";
        public static readonly string SAVED_GAME_EXTENSION = ".SavedCatanGame";

        public List<TileCtrl> Tiles
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<RoadCtrl> Roads
        {
            get
            {
                throw new NotImplementedException();
            }
        }

        public List<SettlementCtrl> Settlements
        {
            get
            {
                throw new NotImplementedException();
            }
        }

       
      

        private async Task OnSave()
        {
           try
            {

                string saveString = GetSaveString();
                if (saveString == "")
                    return;

                _saveFileName =  GetSaveFileName(false);


                var folder = await StaticHelpers.GetSaveFolder();
                var option = CreationCollisionOption.OpenIfExists;
                var file = await folder.CreateFileAsync(_saveFileName, option);
                await FileIO.AppendTextAsync(file, saveString);


            }
            catch (Exception exception)
            {

                string s = StaticHelpers.GetErrorMessage($"Error saving to file {_saveFileName}", exception);
                await ShowErrorText(s);

            }

        }

       

        private async Task<bool> LoadCatanFile(StorageFile file)
        {
            var loadGameString = await FileIO.ReadTextAsync(file);

            return await ParseAndLoadGame(loadGameString);
        }

        private async Task ShowErrorText(string s)
        {
            MessageDialog dlg = new MessageDialog(s);
            await dlg.ShowAsync();
        }

        //
        // Parse and load the saved game file (read as one big string)
        //  I did it this way (try/catch around each section) so that in a pinch I can see where the error is and maybe fix it by hand.
        //

       
      

      

        /*
           [Data]
         Version=3
         ShuffleTiles=17,9,0,11,3,13,5,15,7,18,4,2,10,1,8,14,12,16,6,
         ShuffleHarbors=6,2,3,1,5,4,0,
         GameType=Regular
         GameState=WaitingForNext
         GameFile=Regular
         Turn=Dodgy
         Players=Chris,Joe,Dodgy,Doug

         [Chris]            
         CardsLostToSeven=3
         TimesTargetd=1
         CardsLost=1
         Rolls=12,5,6
         TotalTime=00:00:12.6600888
        
        .
        .
        .

        */

        //private async Task parsePlayers(Dictionary<string, string> sections, Dictionary<string, string> DataSection)
        //{

        //    string currentPlayer = DataSection["Turn"];


        //    List<string> gamePlayers = StaticHelpers.DeserializeList<string>(DataSection["Players"]);

        //    //
        //    //  load my players from the file
        //    ObservableCollection<CatanPlayer> players = await MainPage.LoadPlayers(MainPage.PlayerDataFile);

        //    //
        //    //   data that contains the player names and the rolls
        //    //     Dictionary<string, string> playerData = StaticHelpers.DeserializeDictionary(sections["Players"]);

        //    ObservableCollection<CatanPlayer> currentPlayers = new ObservableCollection<CatanPlayer>();


        //    List<int> allrolls = new List<int>();
        //    bool found = false;
        //    foreach (string name in gamePlayers)
        //    {
        //        // find the player with the same name -- NOTE: NO DUPLICATE NAMES!!
        //        found = false;
        //        foreach (CatanPlayer cp in players)
        //        {
        //            if (cp.PlayerName == name)
        //            {
        //                string serializedPlayerState = sections[name];
        //                cp.DeserializeGame(serializedPlayerState);
        //                currentPlayers.Add(cp);
        //                allrolls.AddRange(cp.Rolls);
        //                found = true;
        //                break;
        //            }
        //        }

        //        if (!found)
        //        {
        //            await ShowErrorText($"This saved game references Player {name} that no longer exists.  Add the player back (same name!) to open this file.");
        //            throw new InvalidDataException();
        //        }
        //    }

        //    throw new NotImplementedException();
            
        //    //_gameTracker.Players = currentPlayers;
        //    //if (State.GameState != GameState.WaitingForStart)
        //    //{
        //    //    await _gameTracker.SetCurrentPlayer(currentPlayer);
        //    //    _gameTracker.CurrentPlayer.StartTimer();
        //    //}

        //    //_gameTracker.AllRolls = allrolls;
        //    // UpdateChart();


        //}
        /*
                  
        [Data]
         Version=3
         ShuffleTiles=17,9,0,11,3,13,5,15,7,18,4,2,10,1,8,14,12,16,6,
         ShuffleHarbors=6,2,3,1,5,4,0,
         GameType=Regular
         GameState=WaitingForNext
         GameFile=Regular
         Turn=Dodgy
         Players=Chris,Joe,Dodgy,Doug

         [Chris]            
         CardsLostToSeven=3
         TimesTargetd=1
         CardsLost=1
         Rolls=12,5,6
         TotalTime=00:00:12.6600888

     */
        private async Task<Dictionary<string, string>> parseData(Dictionary<string, string> sections)
        {
            Dictionary<string, string> Data = StaticHelpers.DeserializeDictionary(sections["Data"]);
            if (Data == null)
                throw new InvalidDataException("There is no [Data] Section");

            if (Data["Version"] != SERIALIZATION_VERSION)
                throw new InvalidDataException("Bad Version number");

          //  string currentPlayer = "";


            await SetStateAsync(null, (GameState)GameState.Parse(typeof(GameState), Data["GameState"]), CatanAction.ChangedState, false);

            try
            {
                this.TraceMessage("TODO: Implement...");
                //currentPlayer = Data["Turn"];
                //if (!_gameView.SetCurrentGame(Data["GameName"]))
                //{
                //    await StaticHelpers.ShowErrorText($"The Game '{Data["GameName"]} wasn't found");
                //    return null;
                //}
                //_gameView.SetGroupsTilesAndHarbors(Data);                
                // _gameView.ShuffleResources();
                //await _gameView.FlipTiles(TileOrientation.FaceUp, true);
            }

            catch
            {
                // means we saved before we had anybody start
            }


            return Data;
        }

      

        //public async static Task<ObservableCollection<CatanPlayer>> LoadPlayers(string fileName)
        //{

        //    ObservableCollection<CatanPlayer> retList = new ObservableCollection<CatanPlayer>();
        //    try
        //    {

        //        var folder = await StaticHelpers.GetSaveFolder();
        //        var playersDictionary = await StaticHelpers.LoadSectionsFromFile(folder, fileName);

        //        CompositeTransform ct = new CompositeTransform();
        //        ct.ScaleX = 1.0;
        //        ct.ScaleY = 1.0;

        //        foreach (var kvp in playersDictionary)
        //        {

        //            CatanPlayer p = new CatanPlayer();
        //            p.Width = 100;
        //            p.Height = 100;
        //            p.CardsLostToMonopoly = 0;
        //            p.TimesTargeted = 0;
        //            p.Deserialize(kvp.Value, false);
        //            p.RenderTransform = ct;
        //            p.SetupTransform2(220, 0, 30, 250);                  
        //            try
        //            {
        //                await p.LoadImage(); ;
        //            }
        //            catch
        //            {
        //                string s = String.Format($"{kvp.Key} has a bad picture set.  Right click on portait to update it.");
        //                MessageDialog dlg = new MessageDialog(s);
        //                await dlg.ShowAsync();

        //            }

        //            retList.Add(p);

        //        }
        //    }
        //    catch
        //    {

        //    }

        //    return retList;

        //}

       

        public static async Task<bool> DeletePlayers(string fileName)
        {

            var folder = Windows.Storage.ApplicationData.Current.LocalFolder;
            var file = await folder.CreateFileAsync(fileName, CreationCollisionOption.ReplaceExisting);
            string toWrite = "";
            await FileIO.WriteTextAsync(file, toWrite);
            return true;
        }
    }
}
