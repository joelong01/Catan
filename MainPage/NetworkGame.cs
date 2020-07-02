using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.Contracts;
using System.Reflection.Metadata.Ecma335;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.UI.WindowManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

using static Catan10.StaticHelpers;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class MainPage : Page
    {
        #region Properties + Fields

        private Dictionary<string, GameInfo> KnownGames = new Dictionary<string, GameInfo>();

        #endregion Properties + Fields

        #region Methods

        private async Task CreateAndConfigureProxy()
        {
            try
            {
                using (new FunctionTimer("CreateAndConfigureProxy"))
                {
                    MainPageModel.IsGameStarted = false;
                    MainPageModel.ServiceGameInfo = MainPageModel.DefaultGame; // the dialog will set the chosen name here
                    MainPageModel.ServiceGameInfo.Name = MainPageModel.Settings.DefaultGameName;
                    MainPageModel.ServiceGameInfo.Creator = TheHuman.PlayerName;
                    MainPageModel.IsServiceGame = true;

                    if (MainPageModel.CatanService != null)
                    {
                        MainPageModel.CatanService.OnBroadcastMessageReceived -= Service_OnBroadcastMessageReceived;
                        MainPageModel.CatanService.OnGameCreated -= Service_OnGameCreated;
                        MainPageModel.CatanService.OnGameDeleted -= Service_OnGameDeleted;
                        MainPageModel.CatanService.OnPrivateMessage -= Service_OnPrivateMessage;
                        await MainPageModel.CatanService.DisposeAsync();
                        MainPageModel.CatanService = null;
                    }

                    MainPageModel.CatanService = new CatanSignalRClient();

                    //if (MainPageModel.Settings.IsSignalRGame)
                    //{
                    //    MainPageModel.CatanService = new CatanSignalRClient();
                    //}
                    //else if (MainPageModel.Settings.IsHomegrownGame)
                    //{
                    //    MainPageModel.CatanService = new CatanRestService();
                    //}

                    MainPageModel.CatanService.OnBroadcastMessageReceived += Service_OnBroadcastMessageReceived;
                    MainPageModel.CatanService.OnGameCreated += Service_OnGameCreated;
                    MainPageModel.CatanService.OnGameDeleted += Service_OnGameDeleted;
                    MainPageModel.CatanService.OnPrivateMessage += Service_OnPrivateMessage;
                    MainPageModel.CatanService.OnGameJoined += Service_OnGameJoined;

                    await MainPageModel.CatanService.Initialize(MainPageModel.Settings.HostName);
                    await MainPageModel.CatanService.StartConnection(MainPageModel.ServiceGameInfo, TheHuman.PlayerName);
                }
            }
            catch (Exception e)
            {

                string Title = "Catan Connnection Error";
                string Message = $"Error connecting to the Catan Service.\n\nOnly Local Games allowed.\n";
                string ExtendedMessage = e.ToString();
                await this.ShowErrorMessage(Message, Title, ExtendedMessage);
                this.MainPageModel.Settings.IsLocalGame = true;
            }

            // await StaticHelpers.ShowErrorText($"Error connecting to the Catan Service.\nOnly Local Games allowed.", "Connection Error");
            
            
        }

        public async Task ShowErrorMessage(string message, string caption, string extended)
        {
            if (TheHuman == null)
            {
                bool ret = await PickDefaultUser();
                if (!ret) return;
                    
            }

            var dlg = new ErrorDlg()
            {
                Title = caption,
                Background = TheHuman.BackgroundBrush,
                Foreground = TheHuman.ForegroundBrush,
                Message = message,
                ExtendedMessage = extended
            };
            await dlg.ShowAsync();
        }

        private async Task<CatanAction> CreateOrJoinGame(ICatanService gameService, GameInfo gameInfo, string me)
        {
            List<GameInfo> games;
            using (new FunctionTimer("GetAllGames"))
            {
                games = await gameService.GetAllGames();
            }

            bool exists = false;
            CatanAction action = CatanAction.GameJoined;

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
                action = CatanAction.GameCreated;
            }
            else
            {
                await gameService.LeaveGame(gameInfo, me);
            }
            GameInfo serverGameInfo = await gameService.JoinGame(gameInfo, me);
            gameInfo.Creator = serverGameInfo.Creator;
            this.TraceMessage($"Game {gameInfo.Name} created by {gameInfo.Creator} joined by {me}");
            return action;
        }

        private async Task DeleteAllGames()
        {
            if (MainPageModel.CatanService == null) return;

            if (MainPageModel.Settings.IsServiceGame)
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

        private async void OnNewNetworkGame(object sender, RoutedEventArgs e)
        {
            if (TheHuman == null)
            {
                bool ret = await PickDefaultUser();
                if (!ret) return;
            };
            MainPageModel.Settings.IsLocalGame = false;
            List<GameInfo> games = await MainPageModel.CatanService.GetAllGames();
            ServiceGameDlg dlg = new ServiceGameDlg(TheHuman, MainPageModel.AllPlayers, games, MainPageModel.CatanService)
            {
                HostName = MainPageModel.Settings.HostName
            };
            await dlg.ShowAsync();
            if (dlg.IsCanceled) return;

            if (dlg.SelectedGame == null)
            {
                dlg.ErrorMessage = "Pick a game. Stop messing around Dodgy!";
                return;
            }

            //MainPageModel.ServiceGameInfo = dlg.SelectedGame;
            //await NewGameLog.JoinOrCreateGame(this, MainPageModel.ServiceGameInfo.Creator, 0, CatanAction.GameJoined);
        }

        private async void OnStartDefaultNetworkGame(object sender, RoutedEventArgs e)
        {
            try
            {
                using (new FunctionTimer("OnStartDefaultNetworkGame"))
                {
                    if (CurrentGameState != GameState.WaitingForNewGame)
                    {
                        this.TraceMessage($"State={CurrentGameState} so rejecting call.  call EndGame() first.");
                        return;
                    }

                    if (TheHuman == null)
                    {
                        await PickDefaultUser();
                    }
                    if (TheHuman == null) return;

                    //JoinGameDlg dlg = new JoinGameDlg(new List<string>() { "DefaultNetworkGame" }) { MainPageModel = MainPageModel,  };

                    //var ret = await dlg.ShowAsync();
                    //if (ret != ContentDialogResult.Primary || String.IsNullOrEmpty(MainPageModel.Settings.DefaultGameName))
                    //{
                    //    return;
                    //}

                    await CreateAndConfigureProxy();

                    using (new FunctionTimer("MainPageModel.CatanService.Initialize"))
                    {
                        await MainPageModel.CatanService.Initialize(MainPageModel.Settings.HostName);
                    }

                    CatanAction action;
                    using (new FunctionTimer("CreateOrJoinGame"))
                    {
                        action = await CreateOrJoinGame(MainPageModel.CatanService, MainPageModel.ServiceGameInfo, TheHuman.PlayerName);
                    }
                    using (new FunctionTimer("StartConnection"))
                    {
                        await MainPageModel.CatanService.StartConnection(MainPageModel.ServiceGameInfo, TheHuman.PlayerName);
                    }

                    //
                    //  note that we join or create the game and this returns when the message is sent, not when the message is processed
                    //

                    await NewGameLog.JoinOrCreateGame(this, MainPageModel.ServiceGameInfo, action);

                    //
                    //  6/16/2020: If there is an issue connecting to the service we call AddPlayer() before creating the game
                    //             moved to the JoinOrCreate.Do method
                }
            }
            catch (Exception exception)
            {
                this.TraceMessage($"Exception starting game: {exception}");
            }
        }

        private async Task ProcessMessage(CatanMessage message)
        {
            LogHeader logHeader = message.Data as LogHeader;
            var latency = (DateTime.Now - logHeader.CreatedTime).Milliseconds;
            NameToPlayer(logHeader.SentBy).ServiceLatency = latency;
            //this.TraceMessage($"Latency: {latency}");
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
                    using (new FunctionTimer($"calling {message.DataTypeName}"))
                    {
                        await logController.Do(this);
                    }

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
            if (message.From == TheHuman.PlayerName)
            {
                MainPageModel.UnprocessedMessages--;
            }
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

            if (message.ActionType == ActionType.Retry)
            {
                for (int i=MainPageModel.Log.MessageLog.Count - 1; i>=0; i--)                
                {
                    var loggedMessage = MainPageModel.Log.MessageLog[i];
                    if (loggedMessage.MessageId == message.MessageId)
                    {
                        this.TraceMessage($"Found the message in the message log.  {message}");
                        this.TraceMessage($"IsRecordedMessage = {MainPageModel.Log.IsMessageRecorded(message)}");
                        return;
                    }
                }
            }


            MainPageModel.Log.RecordMessage(message);
            await ProcessMessage(message);

        }

        private async void Service_OnGameCreated(GameInfo gameInfo, string playerName)
        {
            this.TraceMessage($"{gameInfo} playerName={playerName}");

            if (TheHuman.PlayerName == gameInfo.Creator)
            {
                await NewGameLog.JoinOrCreateGame(this, gameInfo, CatanAction.GameCreated); // the local action to join as the service is already created
            }
            else
            {
                if (!MainPageModel.Settings.AutoRespond)
                {
                    bool yes = await StaticHelpers.AskUserYesNoQuestion($"{gameInfo.Creator} started a game named {gameInfo.Name}.\n\nWould you like to join it?", "Yes!", "No");
                    if (!yes) return;
                }
                //
                //  send a message to the service to Join the game
                await NewGameLog.JoinOrCreateGame(this, gameInfo, CatanAction.GameJoined); // the local action to join as the service is already created
                await MainPageModel.CatanService.JoinGame(gameInfo, TheHuman.PlayerName);   // join the service Hub for this group -- will cause the client events to fire
            }

        }

        private async void Service_OnGameDeleted(Guid id, string by)
        {
            this.TraceMessage($"{id} playerName={by}");
            if (MainPageModel == null || MainPageModel.ServiceGameInfo == null) return;


            if (MainPageModel.ServiceGameInfo.Id != id) return;

            // uh oh -- deleting my game

            if (CurrentGameState != GameState.Uninitialized && CurrentGameState != GameState.WaitingForNewGame)
            {
                await this.Reset();
            }
        }

        /// <summary>
        ///     Message from the service saying somebody has join the game...this does the local work to set the game up
        ///     1. if the game hasn't be started, start it
        ///     2. find all the players currently added and add them.
        /// </summary>
        /// <param name="gameInfo"></param>
        /// <param name="playerName"></param>
        private async void Service_OnGameJoined(GameInfo gameInfo, string playerName)
        {
            //
            //  am I already in a game?
            if (CurrentGameState == GameState.WaitingForNewGame)
            {
                //
                //  this sets the client up to start a game
                await NewGameLog.JoinOrCreateGame(this, gameInfo, CatanAction.GameJoined); // the local action to join as the service is already created
            }

            if (CurrentGameState != GameState.WaitingForPlayers)
            {
                Debug.Assert(false);// how did we get here?
                return;
            }


            //
            //  ask the service for who has joined -- this will now include the current player because of the previous call
            List<string> players = await MainPageModel.CatanService.GetAllPlayerNames(gameInfo.Id);

            //
            //  add the players locally to the game
            foreach (var name in players)
            {
                if (!IsInGame(name))
                {
                    await AddPlayerLog.AddPlayer(this, name);
                }
            }


        }

        private bool IsInGame(string name)
        {
            foreach (var player in MainPageModel.PlayingPlayers)
            {
                if (player.PlayerName == name)
                    return true;
            }
            return false;
        }
        private async void Service_OnPrivateMessage(CatanMessage message)
        {
            MainPageModel.Log.RecordMessage(message);
            await ProcessMessage(message);
        }

        #endregion Methods
    }
}