using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Windows.ApplicationModel.DataTransfer;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The Content Dialog item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{
    public sealed partial class NewGameDlg : ContentDialog
    {
        Dictionary<PlayerPosition, Grid> PositionToGridDictionary = new Dictionary<PlayerPosition, Grid>();
        Dictionary<PlayerPosition, PlayerData> PositionToPlayerDataDictionary = new Dictionary<PlayerPosition, PlayerData>();

        public static readonly DependencyProperty PlayerDataProperty = DependencyProperty.Register("PlayerData", typeof(ObservableCollection<PlayerData>), typeof(NewGameDlg), new PropertyMetadata(new ObservableCollection<PlayerData>()));
        public static readonly DependencyProperty GamesProperty = DependencyProperty.Register("Games", typeof(ObservableCollection<CatanGame>), typeof(NewGameDlg), new PropertyMetadata(new ObservableCollection<CatanGame>()));
        public static readonly DependencyProperty SelectedGameProperty = DependencyProperty.Register("SelectedGame", typeof(CatanGame), typeof(NewGameDlg), new PropertyMetadata(null));
        public static readonly DependencyProperty SelectedPlayersProperty = DependencyProperty.Register("SelectedPlayers", typeof(ObservableCollection<PlayerData>), typeof(NewGameDlg), new PropertyMetadata(new ObservableCollection<PlayerData>()));
        public static readonly DependencyProperty SaveFileNameProperty = DependencyProperty.Register("SaveFileName", typeof(string), typeof(NewGameDlg), new PropertyMetadata(""));
        public string SaveFileName
        {
            get => (string)GetValue(SaveFileNameProperty);
            set => SetValue(SaveFileNameProperty, value);
        }

        public ObservableCollection<PlayerData> SelectedPlayers
        {
            get => (ObservableCollection<PlayerData>)GetValue(SelectedPlayersProperty);
            set => SetValue(SelectedPlayersProperty, value);
        }

        public CatanGame SelectedGame
        {
            get => (CatanGame)GetValue(SelectedGameProperty);
            set => SetValue(SelectedGameProperty, value);
        }

        public ObservableCollection<CatanGame> Games
        {
            get => (ObservableCollection<CatanGame>)GetValue(GamesProperty);
            set => SetValue(GamesProperty, value);
        }

        public int SelectedIndex => Games.IndexOf(SelectedGame);

        public ObservableCollection<PlayerPickerItemCtrl> Players { get; set; } = new ObservableCollection<PlayerPickerItemCtrl>();
        public List<PlayerPickerItemCtrl> GamePlayers { get; } = new List<PlayerPickerItemCtrl>();
        public List<PlayerData> PlayerDataList { get; } = new List<PlayerData>();


        public NewGameDlg()
        {
            this.InitializeComponent();
        }
        public NewGameDlg(IList<PlayerData> playerData, IList<CatanGame> games)
        {
            this.InitializeComponent();
            Players.Clear();
            GamePlayers.Clear();
            foreach (PlayerData p in playerData)
            {
                PlayerPickerItemCtrl ctrl = new PlayerPickerItemCtrl(p);
                if (p.PlayerName == "Joe")
                {
                    ctrl.Position = PlayerPosition.BottomLeft;
                    BottomLeft.Children.Add(ctrl);
                    SelectedPlayers.Add(p);
                }
                else if (p.PlayerName == "Dodgy")
                {
                    TopLeft.Children.Add(ctrl);
                    SelectedPlayers.Add(p);
                    ctrl.Position = PlayerPosition.TopLeft;
                }
                else if (p.PlayerName == "Doug")
                {
                    TopRight.Children.Add(ctrl);
                    SelectedPlayers.Add(p);
                    ctrl.Position = PlayerPosition.TopRight;
                }
                else if (p.PlayerName == "Robert")
                {
                    BottomRight.Children.Add(ctrl);
                    SelectedPlayers.Add(p);
                    ctrl.Position = PlayerPosition.BottomRight;
                }
                else
                {
                    Players.Add(ctrl);
                }
                ctrl.DragStarting += PlayerItemCtrl_DragStarting;
                ctrl.FirstChanged += PlayerItemCtrl_FirstChanged;
            }
            Games.Clear();
            foreach (CatanGame g in games)
            {
                Games.Add(g);
                if (g.Description.Contains("Regular"))
                {
                    SelectedGame = g;

                }
            }



            //  
            // the positions of the grids are in their .Names
            //
            //  the ORDER is semantic to the game -- it is used in the OnOk handler
            //
            PositionToGridDictionary[PlayerPosition.BottomLeft] = BottomLeft;
            PositionToGridDictionary[PlayerPosition.Left] = Left;
            PositionToGridDictionary[PlayerPosition.TopLeft] = TopLeft;
            PositionToGridDictionary[PlayerPosition.TopRight] = TopRight;
            PositionToGridDictionary[PlayerPosition.Right] = Right;
            PositionToGridDictionary[PlayerPosition.BottomRight] = BottomRight;




        }

        private void PlayerItemCtrl_FirstChanged(PlayerPickerItemCtrl sender, bool val)
        {
            if (!val)
            {
                return; // don't care if somebody turns OFF the first player
            }

            // find the player who used to be first, and turn that off

            foreach (PlayerPickerItemCtrl player in GamePlayers)
            {
                if (player != sender && (bool)player.IsFirst)
                {
                    player.IsFirst = false;
                }
            }
            foreach (PlayerPickerItemCtrl player in Players)
            {
                if (player != sender && (bool)player.IsFirst)
                {
                    player.IsFirst = false;
                }
            }

        }

        private void PlayerItemCtrl_DragStarting(UIElement sender, DragStartingEventArgs args)
        {

            PlayerPickerItemCtrl player = sender as PlayerPickerItemCtrl;
            int index = Players.IndexOf(player);
            args.Data.SetText($"{index}:{player.Position}");
            this.TraceMessage($"Dragging {player.PlayerName} Index:{index} Position:{player.Position}");
        }

        private void OnOk(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {

            if (SaveFileName == "")
            {
                return;
            }

            //
            //  we have to sort playing players so that they are correct by position
            GamePlayers.Clear();

            foreach (PlayerPosition pos in PositionToGridDictionary.Keys)
            {
                PlayerPickerItemCtrl item = ((PlayerPickerItemCtrl)PositionToGridDictionary[pos].Children[0]);
                if (item != null)
                {
                    GamePlayers.Add(item);
                    item.PlayerData.PlayerPosition = item.Position;
                }
            }

            int count = 0;

            //
            //  now we are in the right order, but we don't necessarily start in the right spot
            while (GamePlayers[0].IsFirst == false && count < GamePlayers.Count)
            {
                PlayerPickerItemCtrl first = GamePlayers[0];
                GamePlayers.RemoveAt(0);
                GamePlayers.Add(first); // move it to the end
                count++;
            }



            foreach (PlayerPickerItemCtrl player in GamePlayers)
            {
                PlayerDataList.Add(player.PlayerData);
            }


        }

        private void OnCancel(ContentDialog sender, ContentDialogButtonClickEventArgs args)
        {
        }



        private void SetThickness(object target, double thickness)
        {
            if (target.GetType() == typeof(Grid))
            {
                ((Grid)target).BorderThickness = new Thickness(thickness);
            }
            else if (target.GetType() == typeof(GridView))
            {
                ((GridView)target).BorderThickness = new Thickness(thickness);
            }
        }

        private void Grid_DragEnter(object target, DragEventArgs e)
        {
            SetThickness(target, 3);
        }
        private void Grid_DragLeave(object target, DragEventArgs e)
        {
            SetThickness(target, 1);

        }

        private async void Grid_Drop(object target, DragEventArgs e)
        {
            //
            //  we marshal the index of the PlayerPickerItemCtrl when it starts dragging
            //
            if (e.DataView.Contains(StandardDataFormats.Text))
            {
                string tag = await e.DataView.GetTextAsync();
                string[] tokens = tag.Split(new char[] { ':' }, StringSplitOptions.RemoveEmptyEntries);
                if (tokens.Count() != 2)
                {
                    return;
                }

                if (target.GetType() == typeof(Grid))
                {
                    Grid dropTarget = target as Grid;
                    PlayerPickerItemCtrl player = null;
                    int index = Int32.Parse(tokens[0]);
                    PlayerPosition position = (PlayerPosition)Enum.Parse(typeof(PlayerPosition), tokens[1]);
                    while (dropTarget.Children.Count > 0)
                    {
                        //
                        //  if you replace a player, the old player goes back into the gridview
                        PlayerPickerItemCtrl child = (PlayerPickerItemCtrl)dropTarget.Children[0];
                        dropTarget.Children.RemoveAt(0);
                        Players.Add(child);
                        GamePlayers.Remove(child);
                        child.Position = PlayerPosition.None;
                    }

                    if (position == PlayerPosition.None) // coming from the ListView and dropping in a Grid
                    {
                        if (index < 0 || index >= Players.Count)
                        {
                            return;
                        }

                        player = Players[index];
                        Players.RemoveAt(index);
                        GamePlayers.Add(player);
                    }
                    else
                    {
                        Grid grid = PositionToGridDictionary[position];
                        player = grid.Children[0] as PlayerPickerItemCtrl;
                        grid.Children.Clear();
                    }


                    // it is already in PlayingPlayers...the changes its parent and position...
                    if (player == null)
                    {
                        return;
                    }
                    //
                    //  the name of the grids are their positions... :P
                    position = (PlayerPosition)Enum.Parse(typeof(PlayerPosition), dropTarget.Name);
                    player.Position = position;
                    dropTarget.Children.Add(player);
                    return;
                }

                if (target.GetType() == typeof(GridView))
                {
                    GridView dropTarget = target as GridView;
                    PlayerPickerItemCtrl player = null;
                    PlayerPosition position = (PlayerPosition)Enum.Parse(typeof(PlayerPosition), tokens[1]);
                    if (position != PlayerPosition.None) // None if we are just rearranging
                    {
                        Grid grid = PositionToGridDictionary[position];
                        player = grid.Children[0] as PlayerPickerItemCtrl;
                        grid.Children.Clear();
                        GamePlayers.Remove(player);
                        Players.Add(player);
                        player.Position = PlayerPosition.None;
                    }
                }



            }
        }

        private void Grid_DragOver(object sender, DragEventArgs e)
        {

            e.AcceptedOperation = Windows.ApplicationModel.DataTransfer.DataPackageOperation.Move;
            e.DragUIOverride.IsGlyphVisible = false;
            e.DragUIOverride.IsCaptionVisible = false;
        }

        private void OnGameChanged(object sender, SelectionChangedEventArgs e)
        {
            if (SelectedGame == null)
            {
                return;
            }

            DateTime dt = DateTime.Now;

            string ampm = dt.TimeOfDay.TotalMinutes > 720 ? "PM" : "AM";
            string min = dt.TimeOfDay.Minutes.ToString().PadLeft(2, '0');

            SaveFileName = String.Format($"{dt.TimeOfDay.Hours % 12}.{min} {ampm} - {SelectedGame.Description}");
        }
    }
}
