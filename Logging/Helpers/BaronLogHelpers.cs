﻿using System.Diagnostics.Contracts;
using System.Threading.Tasks;

using Catan.Proxy;

namespace Catan10
{
    public class BaronModel
    {
        public int PreviousTile { get; set; }
        public int TargetTile { get; set; }
        public string Victim { get; set; }
        public TargetWeapon Weapon { get; set; }
    }
}
