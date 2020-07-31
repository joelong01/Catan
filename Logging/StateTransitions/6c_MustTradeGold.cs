using System;
using System.Diagnostics;
using System.Threading.Tasks;

using Catan.Proxy;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

namespace Catan10
{
    /// <summary>
    ///     set when one player has a Gold tile rolled.  transitions to Waiting for Next when *all gold tiles* have been traded in.
    /// </summary>
    public class MustTradeGold : LogHeader, ILogController
    {
        #region Methods

        public static async Task PostMessage(IGameController gameController)
        {
            MustTradeGold logHeader = new MustTradeGold()
            {
                CanUndo = false
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            int goldCards = gameController.TheHuman.GameData.Resources.CurrentResources.GetCount(ResourceType.GoldMine);
            if (goldCards > 0)
            {
                Debug.Assert(VisualTreeHelper.GetOpenPopups(Window.Current).Count == 0); // we shouldn't have any popups at this time.
                ResourceCardCollection destination = new ResourceCardCollection(false);
                TradeResources tr = new TradeResources()
                {
                    Wood = goldCards,
                    Brick = goldCards,
                    Wheat = goldCards,
                    Ore = goldCards,
                    Sheep = goldCards
                };
                ResourceCardCollection source = ResourceCardCollection.Flatten(tr);

                string c = goldCards > 1 ? "cards" : "card";

                TakeCardDlg dlg = new TakeCardDlg()
                {
                    To = gameController.TheHuman,
                    From = gameController.MainPageModel.Bank,
                    SourceOrientation = TileOrientation.FaceUp,
                    HowMany = goldCards,
                    Source = source,
                    Instructions = $"Take {goldCards} {c} from the bank.",
                    Destination = destination,
                    
                };

                var ret = await dlg.ShowAsync();
                if (ret != ContentDialogResult.Primary)
                {
                    await MainPage.Current.ShowErrorMessage("Why did you click Cancel?  I'll pick a random resource for you.  No undo.\n\n", "Catan", "");
                    Random random = new Random((int)DateTime.Now.Ticks);
                    int idx = random.Next(source.Count);
                    destination.Add(source[idx]);
                }

                var picked = ResourceCardCollection.ToTradeResources(dlg.Destination);
                picked.AddResource(ResourceType.GoldMine, -goldCards);
                await TradeGold.PostTradeMessage(gameController, picked);
            }
        }
        /// <summary>
        ///     this is bad -- here we do nothing unless this is the *last* message...then we have to call Do()
        ///     need a way to figure out if it is the last message....for now I'll just assume we don't end up 
        ///     here.  ugh.
        /// </summary>
        /// <param name="gameController"></param>
        /// <returns></returns>

        public Task Replay (IGameController gameController)
        {
            return Task.CompletedTask; //TODO: Fix this by calling Do() if this is the last message we are replaying.
        }

        public Task Redo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        public Task Undo(IGameController gameController)
        {
            throw new NotImplementedException();
        }

        #endregion Methods
    }
}