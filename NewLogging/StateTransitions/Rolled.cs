using Catan.Proxy;

using System;
using System.Collections.Generic;
using System.Diagnostics;

namespace Catan10
{


    /// <summary>
    ///     This class has all the data associated with a roll -- when a roll happens, we adjust both player stats and global stats.
    /// </summary>
    public class RolledModel : LogHeader
    {

        public int Rolled { get; set; } = -1;
        //
        //  there is a limitation in the system.text.json serializer in 3.0 where it will only serialize Dictionary<string, TValue>
        public Dictionary<string, PlayerRollData> PlayerToResources { get; set; }
        public RolledModel() { }
        public RolledModel(int n)
        {
            Rolled = n;
        }
        //
        //  compares 2 RolledModel objects for identity.  used for testing Redo.
        public bool Equals(RolledModel rolledModel)
        {
            if ( SentBy != rolledModel.SentBy || OldState != rolledModel.OldState ||
                Action != rolledModel.Action || Rolled != rolledModel.Rolled || PlayerToResources.Count != rolledModel.PlayerToResources.Count)
            {
                return false;
            }

            foreach (KeyValuePair<string, PlayerRollData> kvp in PlayerToResources)
            {
                bool exists = rolledModel.PlayerToResources.TryGetValue(kvp.Key, out PlayerRollData rollData);
                if (!exists) return false;
                if (kvp.Value.MaxRollNoResources != rolledModel.PlayerToResources[kvp.Key].MaxRollNoResources) return false;
                if (kvp.Value.ResourceList.Count != rollData.ResourceList.Count) return false;
                for (int i = 0; i < rollData.ResourceList.Count; i++)
                {
                    if (kvp.Value.ResourceList[i].Value != rollData.ResourceList[i].Value || kvp.Value.ResourceList[i].ResourceType != rollData.ResourceList[i].ResourceType ||
                        kvp.Value.ResourceList[i].BlockedByBaron != rollData.ResourceList[i].BlockedByBaron)
                    {
                        return false;
                    }
                }

            }

            return true;
        }
    }

    /// <summary>
    ///     this keeps track of the resources that are acquired because of the roll. there is a list of these in the PlayerRollData
    /// </summary>
    public class RollResourcesModel
    {
        public ResourceType ResourceType { get; set; }
        public int Value { get; set; }
        public bool BlockedByBaron { get; set; }
        public RollResourcesModel() { } // needed for deserialization
        public RollResourcesModel(ResourceType type, int value, bool blocked)
        {
            ResourceType = type;
            Value = value;
            BlockedByBaron = blocked;
        }
    }

    //
    //  this is the data about the player that we collect because a roll happened.
    //  in particular, there was no way to infer what the max roll with no resources should be without logging it 
    //  because we might have a tie (e.g. if the value for MaxRollsNoResources was 10 and we got an Undo where the current value was 10, 
    //  they might previously have had 10)
    public class PlayerRollData
    {
        public int MaxRollNoResources { get; set; } = 0;
        public List<RollResourcesModel> ResourceList { get; set; } = new List<RollResourcesModel>();
    }


    /// <summary>
    ///     Exposes a set of APIs that handle a roll (Do, Redo, Undo)
    /// </summary>
   
}
