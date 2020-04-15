using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Controls;

namespace Catan10
{
    public class ServiceState
    {
        public string HostName { get; set; } = "https://localhost:5000";
        public string DefaultUser { get; set; } = "";
    }


    public class SavedState
    {
        #region properties
        public List<PlayerModel> Players { get; set; } = new List<PlayerModel>();

        public Settings Settings { get; set; } = new Settings();
        
        public ServiceState ServiceState { get; set; } = new ServiceState();
        #endregion


    }

}
