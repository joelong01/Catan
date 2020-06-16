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
        public string Victim { get; set; }
        public TargetWeapon Weapon { get; set; }

        #endregion Properties
    }
}