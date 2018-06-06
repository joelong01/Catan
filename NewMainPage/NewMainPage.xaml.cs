using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class NewMainPage : Page
    {
        public const string PlayerDataFile = "players.data";
        private const string SERIALIZATION_VERSION = "3";
        public List<PlayerData> PlayerData { get; set; } = new List<PlayerData>();
        

        public NewMainPage()
        {
            this.InitializeComponent();
        }
        protected override async void OnNavigatedTo(NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
            var args = e.Parameter as Windows.ApplicationModel.Activation.IActivatedEventArgs;
           

            if (e.NavigationMode == NavigationMode.New)
            {
                await Initialize();
                ApplicationView.GetForCurrentView().TryEnterFullScreenMode();

            }

        }

        private async Task Initialize()
        {
            _progress.IsActive = true;
            _progress.Visibility = Visibility.Visible;
            await _gameView.LoadAllGames();
            _progress.Visibility = Visibility.Collapsed;
            await LoadPlayerData();
            _progress.IsActive = false;
            
        }

        

        private async Task LoadPlayerData()
        {
            
            try
            {

                var folder = await StaticHelpers.GetSaveFolder();
                var playersDictionary = await StaticHelpers.LoadSectionsFromFile(folder, PlayerDataFile);


                foreach (var kvp in playersDictionary)
                {

                    PlayerData p = new PlayerData();
                  
                    p.Deserialize(kvp.Value, false);
                  
                    //try
                    //{
                    //    await p.LoadImage(); ;
                    //}
                    //catch
                    //{
                    //    string s = String.Format($"{kvp.Key} has a bad picture set.  Right click on portait to update it.");
                    //    MessageDialog dlg = new MessageDialog(s);
                    //    await dlg.ShowAsync();

                    //}

                    PlayerData.Add(p);

                }
            }
            catch
            {

            }
        }
    }


}
