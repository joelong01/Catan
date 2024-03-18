using System;
using System.Collections.Generic;

namespace Catan10
{
    public class BaronModel
    {
        #region Properties

        public int PreviousTile { get; set; }
        public int ResourcesStolen { get; set; }
        public MoveBaronReason Reason { get; set; }
        public GameState StartingState { get; set; }
        public ResourceType StolenResource { get; set; }
        public int TargetTile { get; set; }
        public List<string> Victims { get; set; }
        public TargetWeapon Weapon { get; set; }
        public bool MainBaronHidden { get; set; }
        public Guid PreviousLargestArmyPlayerId { get; set; } = Guid.Empty;
        public Guid MovedBy { get; set; }
        public Guid PreviousMovedBy { get; set; }
        public override string ToString()
        {
            return $"{Weapon}-{base.ToString()}";
        }
        #endregion Properties
    }
}