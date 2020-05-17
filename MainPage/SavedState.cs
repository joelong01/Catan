using System.Collections.Generic;

namespace Catan10
{
    public class ServiceState
    {
        public string HostName { get; set; } = "http://192.168.1.128:5000";
        public string DefaultUser { get; set; } = "";
        public ServiceState() { }
    }


    public class SavedState
    {
        #region properties
        public string TheHuman { get; set; } = "";
        public ServiceState ServiceState { get; set; } = new ServiceState();

        public List<PlayerModel> AllPlayers { get; set; } = new List<PlayerModel>();
        
        public Settings Settings { get; set; } = new Settings();

        
        #endregion
        public SavedState() { }

    }

}
