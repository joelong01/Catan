using System.Collections.Generic;

namespace Catan10
{
    public class ServiceState
    {
        public string HostName { get; set; } = "https://localhost:5000";
        public string DefaultUser { get; set; } = "";
        public ServiceState() { }
    }


    public class SavedState
    {
        #region properties
        public List<PlayerModel> Players { get; set; } = new List<PlayerModel>();
        public string DefaultPlayerName { get; set; } = "";
        public Settings Settings { get; set; } = new Settings();

        public ServiceState ServiceState { get; set; } = new ServiceState();
        #endregion
        public SavedState() { }

    }

}
