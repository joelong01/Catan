using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;

namespace Catan10
{
    public sealed partial class NewMainPage : Page
    {
        private async void OnPointerPressed(object sender, PointerRoutedEventArgs e)
        {
            if (e.GetCurrentPoint(ControlGrid).Position.Y < ControlGrid.RowDefinitions[0].ActualHeight)
            {
                Point pt = await StaticHelpers.DragAsync(ControlGrid, e);
            }
            
        }

       

        private void OnNextPlayer(object sender, RoutedEventArgs e)
        {

        }

        private void OnUndo(object sender, RoutedEventArgs e)
        {

        }
        public ObservableCollection<CatanPlayer> CatanPlayers { get; set; } = new ObservableCollection<CatanPlayer>();
        private async void OnNewGame(object sender, RoutedEventArgs e)
        {
            if (CatanPlayers.Count == 0)
            {
                CompositeTransform ct = new CompositeTransform();
                ct.ScaleX = 1.0;
                ct.ScaleY = 1.0;
                int n = 0;
                foreach (PlayerData data in PlayerData)
                {

                    CatanPlayer p = new CatanPlayer();
                    p.Width = 100;
                    p.Height = 100;
                    p.CardsLostToMonopoly = 0;
                    p.TimesTargeted = 0;
                    p.GamesPlayed = data.GamesPlayed.ToString();
                    p.GamesWon = data.GamesWon.ToString();
                    p.ImageFileName = data.ImageFileName;                    
                    p.Index = n.ToString();
                    n++;
                    p.RenderTransform = ct;
                    p.SetupTransform2(220, 0, 30, 250);
                    CatanPlayers.Add(p);
                }
                CatanNewGameDlg dlg = new CatanNewGameDlg(CatanPlayers, _gameView.SavedGames);
                ContentDialogResult result = await dlg.ShowAsync();
                if (dlg.GamePlayers.Count < 3 && result == ContentDialogResult.Primary)
                {
                    string content = String.Format($"You must pick at least 3 players to play the game.  Click New Game again and click on \"+\" to add players.");
                    MessageDialog msgDlg = new MessageDialog(content);
                    await msgDlg.ShowAsync();
                    return;
                }

                if (result != ContentDialogResult.Secondary)
                {
                    _gameView.SetCurrentGame(dlg.SelectedGame);
                    _gameView.ShuffleResources();
                 
                }
            }
        }

        private void OnViewSettings(object sender, RoutedEventArgs e)
        {

        }

        private void OnWinner(object sender, RoutedEventArgs e)
        {

        }

    
        

        
    }
}
