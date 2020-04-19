using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI;

namespace Catan10
{
    public class SettlementData
    {
        public TileCtrl Tile { get; set; }
        public PlayerView Owner { get; set; }
        public SettlementLocation Location { get; set; }
        public SettlementCtrl Settlement { get; set; }
        public SettlementData(TileCtrl t, SettlementCtrl setttlement)
        {
            Tile = t;
            Location = setttlement.SettlementLocation;
            Settlement = setttlement;
        }

        public override string ToString()
        {
            if (Owner == null)
                return String.Format($"[{Tile}.{Location}.NoOwner]");
            else
                return String.Format($"[{Tile}.{Location}.{Owner.PlayerName}]");
        }


        static public bool Equivalent(SettlementData sd1, SettlementData sd2)
        {
            if (sd1.Tile.GetHashCode() == sd2.Tile.GetHashCode())
            {
                if (sd1.Location == sd2.Location)
                {
                    return true;
                }
            }
            return false;
        }
    }

  

    //
    //  Settlement overlap tiles in Catan and so a "Settlement" can be viewed as being "on" up to 3 different tiles.
    //  we need a definitive way of saying "is there a settlement at this location" (Tile, SettlementLocation) and get the right
    //  answer, even if the user started by clicking on a different SettlementCtrl in a different TileCtrl a different SettlementLcoation causing the same Settlement to 
    //public class Settlement
    //{
    //    public List<SettlementData> Settlements { get; set; } = new List<SettlementData>(); // 1-3 settlements in this visual location

    //    public bool Contains(SettlementData data)
    //    {
    //        foreach (var s in Settlements)
    //        {
    //            if (SettlementData.Equivalent(s, data))
    //                return true;
    //        }

    //        return false;
    //    }

    //    public Color Color
    //    {
    //        get
    //        {
    //            if (Settlements.Count > 0)
    //            {
    //                return Settlements[0].Settlement.Color;
    //            }

    //            return Colors.HotPink;
    //        }
    //        set
    //        {
    //            foreach (var s in Settlements)
    //            {
    //                s.Owner.Color = value;
    //                s.Settlement.Color = value;
    //            }
    //        }
    //    }
    //    public PlayerView Owner
    //    {
    //        get
    //        {
    //            if (Settlements.Count > 0)
    //            {
    //                return Settlements[0].Owner;
    //            }
    //            return null;
    //        }
    //        set
    //        {


    //            foreach (var s in Settlements)
    //            {
    //                s.Owner = value;
    //                if (value == null)
    //                {
    //                    s.Tile.OwnedSettlements.Remove(this);
    //                    s.Settlement.Show(SettlementType.None);
                        

    //                }
    //                else
    //                {
    //                    s.Tile.OwnedSettlements.Add(this);
    //                    s.Settlement.Color = s.Owner.Color;
    //                }
    //            }

    //        }
    //    }

    //    public SettlementType SettlementType
    //    {
    //        get
    //        {
    //            return Settlements[0].Settlement.SettlementType;
    //        }
    //        set
    //        {
    //            if (value != SettlementType)
    //            {
    //                foreach (var s in Settlements)
    //                {
    //                    s.Settlement.SettlementType = value;
    //                }
    //            }
    //        }
    //    }

    //    public override string ToString()
    //    {
    //        string s = "";
    //        foreach (var data in Settlements)
    //        {
    //            s += data.ToString() + ":";
    //        }

    //        return s;
    //    }
    //}
}
