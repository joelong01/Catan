using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

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

        private void CreateAndConfigureProxy()
        {
            using (new FunctionTimer("Initializing MainPageModel"))
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
            }
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
            return action;
        }

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

        private async void OnNewNetworkGame(object sender, RoutedEventArgs e)
        {
            if (TheHuman == null)
            {
                bool ret = await PickDefaultUser();
                if (!ret) return;
            };

            CreateAndConfigureProxy();
            await MainPageModel.CatanService.Initialize(MainPageModel.Settings.HostName);
            await MainPageModel.CatanService.StartConnection(MainPageModel.ServiceGameInfo, TheHuman.PlayerName);

            List<GameInfo> games = await MainPageModel.CatanService.GetAllGames();
            ServiceGameDlg dlg = new ServiceGameDlg(TheHuman, MainPageModel.AllPlayers, games, MainPageModel.CatanService)
            {
                HostName = MainPageModel.Settings.HostName
            };
            // this.TraceMessage($"Human={TheHuman}");
            await dlg.ShowAsync();
            if (dlg.IsCanceled) return;

            if (dlg.SelectedGame == null)
            {
                dlg.ErrorMessage = "Pick a game. Stop messing around Dodgy!";
                return;
            }

            MainPageModel.ServiceGameInfo = dlg.SelectedGame;
            await NewGameLog.JoinOrCreateGame(this, MainPageModel.ServiceGameInfo.Creator, 0, CatanAction.GameJoined);
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

                    GameNameDlg dlg = new GameNameDlg() { MainPageModel = MainPageModel, Player = TheHuman };

                    var ret = await dlg.ShowAsync();
                    if (ret != ContentDialogResult.Primary || String.IsNullOrEmpty(MainPageModel.Settings.DefaultGameName))
                    {
                        return;
                    }

                    CreateAndConfigureProxy();

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

                    await NewGameLog.JoinOrCreateGame(this, MainPageModel.ServiceGameInfo.Creator, 0, action);

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

        private async void Service_OnGameDeleted(Guid id, string by)
        {
            if (by == TheHuman.PlayerName) return; // don't reset games started by me!

            if (MainPageModel.ServiceGameInfo.Id != id) return;

            // uh oh -- deleting my game

            if (CurrentGameState != GameState.Uninitialized && CurrentGameState != GameState.WaitingForNewGame)
            {
                await this.Reset();
            }
        }

        private void Service_OnGameJoined(GameInfo gameInfo, string playerName)
        {
            this.TraceMessage($"{gameInfo}:{playerName}");
        }

        private void Service_OnPrivateMessage(CatanMessage message)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}