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
        public List<PlayerModel> AllPlayers { get; set; } = new List<PlayerModel>();
        public string DefaultPlayerName { get; set; } = "";
        public Settings Settings { get; set; } = new Settings();

        public ServiceState ServiceState { get; set; } = new ServiceState();
        #endregion
        public SavedState() { }

    }

}
