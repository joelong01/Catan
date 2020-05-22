﻿using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Threading.Tasks;
using Windows.Media.Audio;
using Windows.Networking.Sockets;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Shapes;




// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public class WSConnectInfo
    {
        public string GameId { get; set; }
        public string PlayerName { get; set; }
    }


    public sealed partial class MainPage : Page, ILog
    {


        private void InitTest()
        {

        }


        // int toggle = 0;
        private async void OnTest1(object sdr, RoutedEventArgs rea)
        {

            MainPageModel.PlayingPlayers.Clear();
            var joe = NameToPlayer("Joe");
            Contract.Assert(joe != null);
            TheHuman = joe;
            CurrentPlayer = joe;
            MainPageModel.AllPlayers.Remove(joe);
            MainPageModel.AllPlayers.Insert(0, joe);
            //
            //  delete alls games
            List<GameInfo> games = await Proxy.GetGames();
            games.ForEach(async (game) =>
            {
                var s = await Proxy.DeleteGame(game.Id);
                if (s == null)
                {
                    var ErrorMessage = CatanProxy.Serialize(Proxy.LastError, true);
                    this.TraceMessage(ErrorMessage);
                }
            });



            // create a new game
            GameInfo gameInfo = new GameInfo() { Id = Guid.NewGuid().ToString(), Name = "OnTest", Creator = CurrentPlayer.PlayerName };
            games = await Proxy.CreateGame(gameInfo);
            Contract.Assert(games != null);
            MainPageModel.GameInfo = gameInfo;
            //
            //  start the game
            await StartGameLog.StartGame(this, "Joe", 0);

            //
            //  add players
            foreach (var p in MainPageModel.AllPlayers)
            {
                await Proxy.JoinGame(gameInfo.Id, p.PlayerName);
                await AddPlayerLog.AddPlayer(this, p);

            }

            StartMonitoring();



        }
       
        private void OnTest2(object sdr, RoutedEventArgs rea)
        {

            _gameView.AllTiles.ForEach((tile) => tile.TileOrientation = TileOrientation.FaceUp);
            ValidateBuilding = false;
           
        }

       

        // Undo
        private async void OnTest3(object sdr, RoutedEventArgs rea)
        {
            var picker = new Windows.Storage.Pickers.FileOpenPicker();
            picker.ViewMode = Windows.Storage.Pickers.PickerViewMode.Thumbnail;
            picker.SuggestedStartLocation = Windows.Storage.Pickers.PickerLocationId.PicturesLibrary;
            picker.FileTypeFilter.Add(".log");
            Windows.Storage.StorageFile file = await picker.PickSingleFileAsync();
            if (file == null) return;

            string json = await FileIO.ReadTextAsync(file);
            List<CatanMessage> messages = CatanProxy.Deserialize<List<CatanMessage>>(json);
            foreach (var message in messages)
            {
                Type type = CurrentAssembly.GetType(message.DataTypeName);
                if (type == null) throw new ArgumentException("Unknown type!");
                LogHeader logHeader = JsonSerializer.Deserialize(message.Data.ToString(), type, CatanProxy.GetJsonOptions()) as LogHeader;
                message.Data = logHeader;
                Contract.Assert(logHeader != null, "All messages must have a LogEntry as their Data object!");
                
                ILogController logController = logHeader as ILogController;
                Contract.Assert(logController != null, "every LogEntry is a LogController!");
                switch (logHeader.LogType)
                {
                    case LogType.Normal:
                         await logController.Redo(this);
                        break;
                    case LogType.Undo:
                      //  await MainPageModel.Log.Undo(message);
                        break;
                    case LogType.Replay:

                      //  await MainPageModel.Log.Redo(message);
                        break;
                    case LogType.DoNotLog:
                    case LogType.DoNotUndo:
                    default:
                        throw new InvalidDataException("These Logtypes shouldn't be set in a service game");
                }

            }

            StartMonitoring();

        }

        private void VerifyRoundTrip<T>(T model)
        {
            //var options = new JsonSerializerOptions() { WriteIndented = true };
            //options.Converters.Add(new JsonStringEnumConverter());
            //var jsonString = JsonSerializer.Serialize<T>(model, options);
            //T newModel = JsonSerializer.Deserialize<T>(jsonString, options);
            //var newJsonString = JsonSerializer.Serialize<T>(newModel, options);
            ////   this.TraceMessage(newJsonString);
            //Debug.Assert(newJsonString == jsonString);

        }

        readonly Random testRandom = new Random();

        private void OnTestRegularGame(object sender, RoutedEventArgs e)
        {
            //AnimationSpeedBase = 10; // speed up the animations


            //await this.Reset();
            ////   await MainPageModel.Log.Init(CreateSaveFileName("Test Game"));

            //await SetStateAsync(null, GameState.WaitingForNewGame, true);
            //_gameView.CurrentGame = _gameView.Games[0];

            ////  SavedGames.Insert(0, MainPageModel.Log);
            //await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, 0);
            //List<PlayerModel> PlayerDataList = new List<PlayerModel>
            //{
            //    MainPageModel.AllPlayers[0],
            //    MainPageModel.AllPlayers[1],
            //    MainPageModel.AllPlayers[2],
            //    MainPageModel.AllPlayers[3]
            //};
            //await StartGame(PlayerDataList, 0);
            //await NextState(); // simluates pushing "Start"
            //CurrentPlayer = MainPageModel.PlayingPlayers[0];
            //await PickSettlementsAndRoads();


        }
        private async void OnTestExpansionGame(object sender, RoutedEventArgs e)
        {
            AnimationSpeedBase = 10; // speed up the animations
            RandomGoldTileCount = 3;
            await this.Reset();
            // await MainPageModel.Log.Init(CreateSaveFileName("Expansion Game"));
            await SetStateAsync(null, GameState.WaitingForNewGame, true);
            _gameView.CurrentGame = _gameView.Games[1];

            //   SavedGames.Insert(0, MainPageModel.Log);
            //   await AddLogEntry(null, GameState.GamePicked, CatanAction.SelectGame, true, LogType.Normal, 1);
            List<PlayerModel> PlayerDataList = new List<PlayerModel>
            {
                MainPageModel.AllPlayers[0],
                MainPageModel.AllPlayers[1],
                MainPageModel.AllPlayers[2],
                MainPageModel.AllPlayers[3],
                MainPageModel.AllPlayers[4],
            };
            await StartGame(PlayerDataList, 1);
            await NextState(); // simluates pushing "Start"
            CurrentPlayer = MainPageModel.PlayingPlayers[0];
            await PickSettlementsAndRoads();


        }
    }
}