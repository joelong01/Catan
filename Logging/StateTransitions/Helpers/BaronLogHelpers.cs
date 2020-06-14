﻿namespace Catan10
{
    public class BaronModel
    {
        public int PreviousTile { get; set; }
        public int TargetTile { get; set; }
        public string Victim { get; set; }
        public TargetWeapon Weapon { get; set; }
        public ResourceType StolenResource { get; set; }
        public MoveBaronReason Reason { get; set; }
    }
}