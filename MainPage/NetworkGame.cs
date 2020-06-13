using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Reflection;
using System.Text.Json;
using System.Threading.Tasks;

using Catan.Proxy;
using Microsoft.AspNetCore.SignalR.Client;
using Windows.Networking.Sockets;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page
    {
        #region Properties + Fields 

        private Dictionary<string, GameInfo> KnownGames = new Dictionary<string, GameInfo>();

        #endregion Properties + Fields 

        #region Methods

        private async Task DeleteAllGames()
        {
            if (MainPageModel.CatanService == null) return;

            if (MainPageModel.Settings.IsHomegrownGame)
            {

                List<GameInfo> games = await MainPageModel.CatanService.GetAllGames();
                if (games != null)
                {
                    games.ForEach(async (game) =>
                    {
                        await MainPageModel.CatanService.DeleteGame(game.Id, TheHuman.PlayerName);                        
                    });
                }
            }
        }

        private PlayerModel FindPlayerByName(ICollection<PlayerModel> playerList, string playerName)
        {
            foreach (var player in playerList)
            {
                if (player.PlayerName == playerName)
                {
                    return player;
                }
            }
            return null;
        }

       

       

        private async void OnDeleteAllGames(object sender, RoutedEventArgs e)
        {
            await DeleteAllGames();
        }

        private void OnNewNetworkGame(object sender, RoutedEventArgs e)
        {
            OnStartDefaultNetworkGame(sender, e);

            //await PickDefaultUser();
            //if (TheHuman == null)
            //{
            //    bool ret = await PickDefaultUser();
            //    if (!ret) return;
            //};

            //var proxy = MainPageModel.Proxy;

            //var existingGames = await proxy.GetGames();
            //ServiceGameDlg dlg = new ServiceGameDlg(TheHuman, MainPageModel.AllPlayers, existingGames)
            //{
            //    HostName = proxy.HostName
            //};
            //// this.TraceMessage($"Human={TheHuman}");
            //await dlg.ShowAsync();
            //if (dlg.IsCanceled) return;

            //if (dlg.SelectedGame == null)
            //{
            //    dlg.ErrorMessage = "Pick a game. Stop messing around Dodgy!";
            //    return;
            //}

            //MainPageModel.ServiceGameInfo = dlg.SelectedGame;
            //await _rollControl.Reset();
            //MainPageModel.GameStartedBy = NameToPlayer(dlg.SelectedGame.Creator);
            //await NewGameLog.NewGame(this, TheHuman.PlayerName, 0);

            //StartMonitoring();
        }



        private async void OnStartDefaultNetworkGame(object sender, RoutedEventArgs e)
        {
            if (TheHuman == null)
            {
                await PickDefaultUser();
            }
            if (TheHuman == null) return;
            
            GameNameDlg dlg = new GameNameDlg() { MainPageModel = MainPageModel, Player = TheHuman };
            
            var ret = await dlg.ShowAsync();
            if (ret != ContentDialogResult.Primary  || String.IsNullOrEmpty(MainPageModel.Settings.DefaultGameName))
            {
                return;
            }

            MainPageModel.ServiceGameInfo = MainPageModel.DefaultGame; // the dialog will set the chosen name here
            MainPageModel.ServiceGameInfo.Name = MainPageModel.Settings.DefaultGameName;
            MainPageModel.ServiceGameInfo.Creator = TheHuman.PlayerName;

            if (MainPageModel.CatanService != null)
            {
                MainPageModel.CatanService.OnBroadcastMessageReceived -= Service_OnBroadcastMessageReceived;
                MainPageModel.CatanService.OnGameCreated -= Service_OnGameCreated;
                MainPageModel.CatanService.OnGameDeleted -= Service_OnGameDeleted;
                MainPageModel.CatanService.OnPrivateMessage -= Service_OnPrivateMessage;
                
                MainPageModel.CatanService = null;
            }

            if (MainPageModel.Settings.IsSignalRGame)
            {
                MainPageModel.CatanService = new CatanSignalRClient();                                
            }
            else if (MainPageModel.Settings.IsHomegrownGame)
            {
                MainPageModel.CatanService = new CatanRestService();
            }

            MainPageModel.CatanService.OnBroadcastMessageReceived += Service_OnBroadcastMessageReceived;
            MainPageModel.CatanService.OnGameCreated += Service_OnGameCreated;
            MainPageModel.CatanService.OnGameDeleted += Service_OnGameDeleted;
            MainPageModel.CatanService.OnPrivateMessage += Service_OnPrivateMessage;
            MainPageModel.CatanService.OnGameJoined += Service_OnGameJoined;


            MainPageModel.PlayingPlayers.Clear();
            MainPageModel.Log = new Log(this);

            await MainPageModel.CatanService.Initialize(MainPageModel.Settings.HostName);
            await CreateOrJoinGame(MainPageModel.CatanService, MainPageModel.ServiceGameInfo, TheHuman.PlayerName);
            await MainPageModel.CatanService.StartConnection(MainPageModel.ServiceGameInfo, TheHuman.PlayerName);


            //
            //  start the game
            await NewGameLog.NewGame(this, TheHuman.PlayerName, 0);
            //
            //  add the human to the game
            
            await AddPlayerLog.AddPlayer(this, TheHuman);
            
        }

        private void Service_OnGameJoined(GameInfo gameInfo, string playerName)
        {
            this.TraceMessage($"{gameInfo}:{playerName}");
        }

        private async Task CreateOrJoinGame(ICatanService gameService, GameInfo gameInfo, string me)
        {
            var games = await gameService.GetAllGames();            
            bool exists = false;
            
            foreach (var game in games)
            {
                if (game.Id == gameInfo.Id)
                {
                    exists = true;

                    if (game.Creator == me)
                    {
                        await gameService.DeleteGame(gameInfo.Id, me);
                        exists = false;
                        break;
                    }
                }
            }
            if (!exists)
            {
                await gameService.CreateGame(gameInfo);
            }
            else
            {
                await gameService.LeavGame(gameInfo, me);
            }
            GameInfo serverGameInfo =  await gameService.JoinGame(gameInfo, me);
            gameInfo.Creator = serverGameInfo.Creator;
                
        }

        private async Task ProcessMessage(CatanMessage message)
        {
            LogHeader logHeader = message.Data as LogHeader;
            ILogController logController = logHeader as ILogController;
            Contract.Assert(logController != null, "every LogEntry is a LogController!");
            switch (message.ActionType)
            {
                case ActionType.Normal:
                    //
                    //  5/20/2020: we are logging first so that the Do() method has the correct current state of the system
                    //             instead of the *anticipated* correct state.
                    if (logHeader.LogType != LogType.DoNotLog)
                    {
                        await Log.PushAction(logHeader);
                    }
                    await logController.Do(this);
                    break;

                case ActionType.Undo:
                    await logController.Undo(this);
                    await Log.Undo(logHeader);
                    break;

                case ActionType.Redo:
                    await logController.Redo(this);
                    await Log.Redo(logHeader);
                    break;

                case ActionType.Replay:
                    Contract.Assert(false, "You haven't implemented this yet!");
                    break;

                default:
                    break;
            }
            //
            //  done processing release calling thread - if we end up doing an ACK message, we'd do this after counting up the number of 
            //  ACKS to the number of clients
            ////
            //MessageCompletionDictionary[logHeader.LogId].SetResult(null);
            //MessageCompletionDictionary.Remove(logHeader.LogId);
        }

        private Task ReplayGame(GameInfo game, string playerName)
        {
            this.TraceMessage("You need to build this...");
            return Task.CompletedTask;
            //   var messages = await Proxy.
        }

        private async void Service_OnBroadcastMessageReceived(CatanMessage message)
        {
          //  this.TraceMessage($"{TheHuman.PlayerName}: {message.From} - {message.MessageType} : {message.Data}");
            MainPageModel.Log.RecordMessage(message);
            await ProcessMessage(message);
        }

        private async void Service_OnGameCreated(GameInfo gameInfo, string playerName)
        {
            if (MainPageModel.Settings.AutoJoinGames)
            {
                if (TheHuman == null)
                {
                    bool ret = await PickDefaultUser();
                    if (!ret) return;
                }
                if (playerName != TheHuman.PlayerName)
                {
                    await MainPageModel.CatanService.JoinGame(gameInfo, TheHuman.PlayerName);
                    await AddPlayerLog.AddPlayer(this, TheHuman);
                }
            }
        }

        private async void Service_OnGameDeleted(GameInfo gameInfo)
        {
            if (gameInfo.Creator == TheHuman.PlayerName) return; // don't reset games started by me!

            if (CurrentGameState != GameState.Uninitialized && CurrentGameState != GameState.WaitingForNewGame)
            {
                await this.Reset();
            }
        }
        private void Service_OnPrivateMessage(CatanMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion Methods

        //private async void StartMonitoring()
        //{
        //    var proxy = MainPageModel.Proxy;
        //    var gameId = MainPageModel.ServiceGameInfo.Id;

        //    var players = await proxy.GetPlayers(gameId);
        //    Contract.Assert(players.Contains(TheHuman.PlayerName), "You need to join the game before you can monitor it!");

        //    while (true)
        //    {
        //        List<CatanMessage> messages;
        //        try
        //        {
        //            messages = await proxy.Monitor(gameId, TheHuman.PlayerName);
        //        }
        //        catch (Exception e)
        //        {
        //            this.TraceMessage($"{e}");
        //            return;
        //        }
        //        foreach (var message in messages)
        //        {
        //            Type type = CurrentAssembly.GetType(message.DataTypeName);
        //            if (type == null) throw new ArgumentException("Unknown type!");
        //            string json = message.Data.ToString();
        //            LogHeader logHeader = JsonSerializer.Deserialize(json, type, CatanProxy.GetJsonOptions()) as LogHeader;
        //            IMessageDeserializer deserializer = logHeader as IMessageDeserializer;
        //            if (deserializer != null)
        //            {
        //                logHeader = deserializer.Deserialize(json);
        //            }
        //            message.Data = logHeader;
        //            MainPageModel.Log.RecordMessage(message);
        //            Contract.Assert(logHeader != null, "All messages must have a LogEntry as their Data object!");

        //            await ProcessMessage(message);
        //        }
        //    }
        //}

       
    }
}
