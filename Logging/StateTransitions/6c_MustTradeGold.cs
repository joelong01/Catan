using System;
using System.Threading.Tasks;

using Catan.Proxy;



using Windows.UI.Xaml.Controls;

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
            };

            await gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            int goldCards = gameController.TheHuman.GameData.Resources.Current.GetCount(ResourceType.GoldMine);
            if (goldCards > 0)
            {
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
                    await StaticHelpers.ShowErrorText("Why did you click Cancel?  I'll pick a random resource for you.  No undo.", "Catan");
                    Random random = new Random((int)DateTime.Now.Ticks);
                    int idx = random.Next(source.Count);
                    destination.Add(source[idx]);
                }

                var picked = ResourceCardCollection.ToTradeResources(dlg.Destination);
                await TradeGold.PostTradeMessage(gameController, picked);
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

        #endregion Methods
    }
}