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

            return gameController.PostMessage(logHeader, Catan.Proxy.ActionType.Normal);
        }

        public async Task Do(IGameController gameController)
        {
            TradeOffer localOffer = gameController.TheHuman.GameData.Trades.FindTradeByValue(this.TradeOffer);
            if (localOffer == null)
            {
                await StaticHelpers.ShowErrorText("Sorry, your offer no longer exists.\nLooks like somebody beat you to it.", "Catan Trades");
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
                if (!this.TradeOffer.Owner.Player.GameData.Resources.Current.CanAfford(this.TradeOffer.Owner.Resources))
                {
                    await StaticHelpers.ShowErrorText($"{this.TradeOffer.Owner.Player.PlayerName} is a bad person.\n\nThey approved a trade for resources they do not have.\n\nShame.", "Catan 10");
                    return;
                }
                if (!this.TradeOffer.Partner.Player.GameData.Resources.Current.CanAfford(this.TradeOffer.Partner.Resources))
                {
                    await StaticHelpers.ShowErrorText($"{this.TradeOffer.Owner.Player.PlayerName} is a bad person.\n\nThey approved a trade for resources they do not have.\n\nShame.", "Catan 10");
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

        public Task Redo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        public Task Undo(IGameController gameController)
        {
            return Task.CompletedTask;
        }

        #endregion Methods
    }
}