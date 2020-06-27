﻿using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

using Windows.UI.Xaml.Controls;

namespace Catan10
{
    public class PlayerHasTooManyCards : LogHeader, ILogController
    {
        public static Task PostMessage(IGameController gameController)
        {
            Contract.Assert(gameController.CurrentGameState == GameState.WaitingForNext);
            var logHeader = new PlayerHasTooManyCards() { CanUndo = false };
            return gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            
            if (gameController.TheHuman.GameData.Resources.Current.Count > 7)
            {
                //
                //  we explicity discard before we target! -- this is *not* about the Baron, so don't move it.
                //  this is about having too many cards
                //


                Contract.Assert(gameController.TheHuman.GameData.Resources.ResourcesThisTurn.Count == 0);
                int loss = (int)gameController.TheHuman.GameData.Resources.Current.Count / 2;
                TradeResources lost = new TradeResources();
                ResourceCardCollection source = new ResourceCardCollection(false);
                source.AddResources(gameController.TheHuman.GameData.Resources.Current);
                TakeCardDlg dlg = new TakeCardDlg()
                {
                    To = gameController.MainPageModel.Bank,
                    From = gameController.TheHuman,
                    SourceOrientation = TileOrientation.FaceUp,
                    HowMany = loss,
                    Source = source,
                    Destination = new ResourceCardCollection(),
                    Instructions = $"Give {loss} cards to the bank."
                };
                var ret = await dlg.ShowAsync();
                if (ret == ContentDialogResult.Primary)
                {
                    lost = ResourceCardCollection.ToTradeResources(dlg.Destination);
                }
                else
                {
                    await StaticHelpers.ShowErrorText($"Since you cancelled out of the dialog (I assume it was you Dodgy) the game will now pick {loss} random cards.\nNo Undo. Live and learn.", "Catan");
                    var list = gameController.TheHuman.GameData.Resources.Current.ToList();
                    Random rand = new Random((int)DateTime.Now.Ticks);

                    for (int i = 0; i < loss; i++)
                    {
                        int index = rand.Next(list.Count);
                        lost.AddResource(list[index], 1);
                        list.RemoveAt(index);
                    }
                }

                Contract.Assert(lost.Count == loss);
                await LoseCardsToSeven.PostMessage(gameController, lost);
             
            }

        }

        public Task Redo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }
    }
}