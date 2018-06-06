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
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Pickers;
using Windows.Storage.Streams;
using Windows.System;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Navigation;

namespace Catan10
{
    public sealed partial class MainPage : Page
    {

        private const int SMALLEST_STATE_COUNT = 1; // game starts with NewGame and then Deal

        private async Task PlayerControl_OnEnter(object sender, GameTrackerEventArgs e)
        {
            await ProcessEnter(e.Player, e.Input);
        }

        private async Task PlayerControl_OnUndo(object sender, EventArgs e)
        {
            if (_log.Count < SMALLEST_STATE_COUNT) // the games starts with 5 states that can't be undone
                return;

            LogEntry state = _log.Last();

            try
            {
                for (int i = _log.Count - 1; i >= SMALLEST_STATE_COUNT - 1; i--)
                {
                    switch (_log[i].Action)
                    {
                        case Action.Rolled:
                            _gameTracker.PopRoll();
                            _log.RemoveAt(i);
                            return;
                        case Action.Targetted:
                            _log[i].Player.TimesTargeted--;
                            _log[i].Player.CardsLostToMonopoly--;
                            _log.RemoveAt(i); // throw away this entry
                            return;
                        case Action.LostCardsToMonopoly:
                            _log[i].Player.CardsLostToMonopoly -= _log[i].Number;
                            _log.RemoveAt(i); // throw away this entry
                            return;
                        case Action.ChangedState:
                            _log.RemoveAt(i);
                            break;
                        case Action.FinishedLostCardToSeven:
                            {
                                _log.RemoveAt(i); // the log entry that has the Action == FinishedLostCard
                                i--;
                                do
                                {
                                    _log.RemoveAt(i);
                                    i--;
                                } while (_log[i].Action != Action.LostCardsToMonopoly);
                                _log[i].Player.CardsLostToMonopoly -= _log[i].Number;
                                do
                                {
                                    _log.RemoveAt(i);
                                    i--;
                                } while (_log[i].GameState == GameState.LostToCardsLikeMonopoly);
                                return;
                            }
                        case Action.FinishedAllPlayersLostCardsToMonopoly:
                            {
                                // get rid of the entries until we find the ones that tell us how many cards the layer lost
                                do
                                {
                                    _log.RemoveAt(i);
                                    i--;
                                } while (_log[i].Action != Action.LostCardsToMonopoly);

                                // refund the lost cards
                                do
                                {
                                    _log[i].Player.CardsLostToMonopoly -= _log[i].Number;
                                    _log.RemoveAt(i);
                                    i--;

                                } while (_log[i].Action == Action.LostCardsToMonopoly);


                                do
                                {
                                    _log.RemoveAt(i);
                                    i--;
                                } while (_log[i].GameState == GameState.AllPlayersLostCardsToMonopoly);


                                return;

                            }
                        case Action.MoveToNextPlayer:
                            if (_log[i].GameState == GameState.AllocateResource)
                            {
                                _log.RemoveAt(i);
                                await _gameTracker.AnimateOnePlayerCounterClockwise();
                                _allocateResourceCounter--;
                            }
                            return;
                        case Action.MoveToPrevPlayer:
                            if (_log[i].GameState == GameState.AllocateResource)
                            {
                                _log.RemoveAt(i);
                                await _gameTracker.AnimateOnePlayerClockwise();
                                _allocateResourceCounter++;
                            }
                            return;
                        case Action.ChangePlayer:
                            await _gameTracker.AnimateOnePlayerCounterClockwise();
                            _log.RemoveAt(i);
                            break;
                        case Action.Dealing:
                            await Reshuffle();
                            // don't take out the log line because Reshuffle doesn't log...
                            // put the state back reguardless if they really reshuffled or not
                            await SetStateAsync(_gameTracker.CurrentPlayer, GameState.WaitingForStart, Action.ChangedState, false);
                            await ProcessEnter(null, "");
                            break;
                        default:
                            break;
                    }

                }
            }
            finally
            {
                UpdateChart();
                _gameTracker.SetState(_log.Last().GameState);
                await OnSave();
            }
        }




        //
        //  player targeted is NOT a state transition -- all we do is record
        //  that the player was targetted and update the UI -- so we add the state 
        //  as whatever it was last
        private async Task OnPlayerTargeted(object sender, GameTrackerEventArgs e)
        {
            e.Player.TimesTargeted++;
            e.Player.CardsLostToMonopoly++;
            AddLogEntry(e.Player, _log.Last().GameState, Action.Targetted, 1);

            await OnSave();

        }

        private async Task OnPlayerLostCards(object sender, GameTrackerEventArgs e)
        {
            if (e.Player == null) // many players targetted
            {

                foreach (CatanPlayer player in _gameTracker.Players)
                {
                    string input = await _gameTracker.ShowAndWait(player, "Cards Lost", "0");
                    int cardsLost = 0;
                    if (Int32.TryParse(input, out cardsLost))
                    {
                        _gameTracker.CurrentPlayer.CardsLostToMonopoly += cardsLost;
                        await OnSave();
                        AddLogEntry(_gameTracker.CurrentPlayer, GameState.AllPlayersLostCardsToMonopoly, Action.LostCardsToMonopoly, cardsLost);
                    }
                }

                //
                //  go ask the players who the cards were taken from
                StartIteration(GameState.AllPlayersLostCardsToMonopoly, 0);
            }
            else
            {
                //
                // e.Player is the target, not the originator.
                await AnimateToPlayer(e.Player, GameState.LostToCardsLikeMonopoly);
                await SetStateAsync(e.Player, GameState.LostToCardsLikeMonopoly, Action.ChangedState, false);
            }
            await OnSave();


        }

        private async Task PlayerControl_OnWinner(object sender, GameTrackerEventArgs e)
        {
            this.TraceMessage("This code doesn't work");
            
            await OnWin();

        }

        private async Task OnNewGame(object sender, EventArgs e)
        {
            await OnNewGame();
        }

        private async Task OnWin()
        {

            var ret = await StaticHelpers.AskUserYesNoQuestion(String.Format($"Did {_gameTracker.CurrentPlayerName} really win?"), "Yes", "No");
            if (ret == true)
            {
                try
                {
                    await _gameTracker.PlayerWon();
                    await SetStateAsync(State.Player, GameState.WaitingForNewGame, Action.ChangedState, true);
                }
                catch (Exception e)
                {
                    MessageDialog dlg = new MessageDialog(String.Format($"Error in OnWin\n{e.Message}"));
                    await dlg.ShowAsync();
                }
            }
        }
        private async Task PlayerControl_OnChangeGame(object sender, ChangeGameEventArgs e)
        {
            if (State.GameState == GameState.WaitingForNewGame)
            {
                await _gameView.SaveDefaultGamesLocally();
                await _gameView.LoadGame(e.File);
                return;
              
            }
            if (await StaticHelpers.AskUserYesNoQuestion("Are you sure you want to change the game?  The board will reshuffle and you won't be able to get back to this game.", "Yes", "No"))
            {
                await _gameView.LoadGame(e.File);
                await ShuffleResources();
                await VisualShuffle();

            }

        }
    }
}
