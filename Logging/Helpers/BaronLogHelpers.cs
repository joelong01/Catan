using System.Collections.Generic;

namespace Catan10
{
    public class BaronModel
    {
        #region Properties

        public int PreviousTile { get; set; }
        public MoveBaronReason Reason { get; set; }
        public GameState StartingState { get; set; }
        public ResourceType StolenResource { get; set; }
        public int TargetTile { get; set; }
        public List<string> Victims { get; set; }
        public TargetWeapon Weapon { get; set; }
        public bool MainBaronHidden { get; set; }

        #endregion Properties
    }
}