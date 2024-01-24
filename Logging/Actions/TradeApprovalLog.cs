using System.Diagnostics.Contracts;
using System.Threading.Tasks;

namespace Catan10
{
    public class TradeApprovalChangedLog : LogHeader, ILogController
    {
        #region Properties

        public bool ApprovalValue { get; set; }
        public PlayerModel Approver { get; set; }
        public TradeOffer TradeOffer { get; set; }

        #endregion Properties

        #region Methods

        public static Task ToggleTrade(IGameController gameController, TradeOffer offer, bool approval, PlayerModel approver)
        {
            TradeApprovalChangedLog logHeader = new TradeApprovalChangedLog()
            {
                TradeOffer = offer,
                CanUndo = false,
                Approver = approver,
                ApprovalValue = approval,
                Action = CatanAction.ChangedTradeApproval
            };

            return gameController.PostMessage(logHeader, ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            TradeOffer localOffer = gameController.TheHuman.GameData.Trades.FindTradeByValue(this.TradeOffer);
            if (localOffer == null)
            {
                this.TraceMessage($"Couldn't find LocalOffer: {TradeOffer} on {gameController.TheHuman.PlayerName}'s machine.");
                return;
            }
            if (localOffer.Partner.Player == Approver)
            {
                localOffer.Partner.Approved = ApprovalValue;
            }
            else
            {
                Contract.Assert(localOffer.Owner.Player == Approver);
                localOffer.Owner.Approved = ApprovalValue;
            }

            if (localOffer.Owner.Approved && localOffer.Partner.Approved)
            {
                //
                //
                if (!this.TradeOffer.Owner.Player.GameData.Resources.CurrentResources.CanAfford(this.TradeOffer.Owner.Resources))
                {
                    await MainPage.Current.ShowErrorMessage($"{this.TradeOffer.Owner.Player.PlayerName} is a bad person.\n\nThey approved a trade for resources they do not have.\n\nShame.\n\n", "Catan 10", "");
                    return;
                }
                if (!this.TradeOffer.Partner.Player.GameData.Resources.CurrentResources.CanAfford(this.TradeOffer.Partner.Resources))
                {
                    await MainPage.Current.ShowErrorMessage($"{this.TradeOffer.Partner.Player.PlayerName} is a bad person.\n\nThey approved a trade for resources they do not have.\n\nShame.\n\n", "Catan 10", "");
                    return;
                }

                //
                //  clear the resources this turn
                this.TradeOffer.Owner.Player.GameData.Resources.ResourcesThisTurn.Reset();
                this.TradeOffer.Partner.Player.GameData.Resources.ResourcesThisTurn.Reset();

                //
                //  take away resources
                this.TradeOffer.Partner.Player.GameData.Resources.GrantResources(this.TradeOffer.Partner.Resources.GetNegated());
                this.TradeOffer.Owner.Player.GameData.Resources.GrantResources(this.TradeOffer.Owner.Resources.GetNegated());

                //
                //  grant the resources
                this.TradeOffer.Owner.Player.GameData.Resources.GrantResources(this.TradeOffer.Partner.Resources);
                this.TradeOffer.Partner.Player.GameData.Resources.GrantResources(this.TradeOffer.Owner.Resources);
            }
        }

        public Task Replay (IGameController gameController)
        {
            return Do(gameController);
        }

        public async Task Redo(IGameController gameController)
        {
             await DefaultTask;
        }

        public async Task Undo(IGameController gameController)
        {
             await DefaultTask;
        }

        #endregion Methods
    }
}