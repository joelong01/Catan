using Catan.Proxy;

namespace Catan10
{
    public class CatanServiceHelpers
    {
        #region Methods

        static public LogHeader DeserializeLogHeader(string json)
        {
            LogHeader logHeader = CatanProxy.Deserialize<LogHeader>(json);
            if (logHeader == null) return null;
            switch (logHeader.Action)
            {
                case CatanAction.Rolled:
                    break;

                case CatanAction.ChangedState:
                    break;

                case CatanAction.ChangedPlayer:
                    break;

                case CatanAction.Dealt:
                    break;

                case CatanAction.CardsLost:
                    break;

                case CatanAction.CardsLostToSeven:
                    break;

                case CatanAction.MissedOpportunity:
                    break;

                case CatanAction.DoneSupplemental:
                    break;

                case CatanAction.DoneResourceAllocation:
                    break;

                case CatanAction.RolledSeven:
                    break;

                case CatanAction.MovingBaron:
                    break;

                case CatanAction.UpdatedRoadState:
                    break;

                case CatanAction.UpdateBuildingState:
                    break;

                case CatanAction.AssignedPirateShip:
                    break;

                case CatanAction.AddPlayer:
                    return CatanProxy.Deserialize<AddPlayerLog>(json);

                case CatanAction.SelectGame:
                    break;

                case CatanAction.InitialAssignBaron:
                    break;

                case CatanAction.None:
                    break;

                case CatanAction.SetFirstPlayer:
                    break;

                case CatanAction.RoadTrackingChanged:
                    break;

                case CatanAction.AddResourceCount:
                    break;

                case CatanAction.ChangedPlayerProperty:
                    break;

                case CatanAction.SetRandomTileToGold:
                    break;

                case CatanAction.ChangePlayerAndSetState:
                    break;

                case CatanAction.Started:
                    break;

                case CatanAction.RandomizeBoard:
                    break;

                default:
                    break;
            }

            return null;
        }

        #endregion Methods
    }
}
