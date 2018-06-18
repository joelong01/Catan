using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public delegate void TitleBarClickedHandler(PlayerView playerView);
    public enum PlayerViewOrientation { Bottom, Right, Top, Left };
    public sealed partial class PlayerView : UserControl, INotifyPropertyChanged
    {
        
        public TitleBarClickedHandler TitleBarClicked;

        PlayerData _data = null;

        // State I need from outside PlayerState to update the UI
        // MainView needs to set these whenever the game changes
        public int MaxRoads { get; set; } = 15;
        public int MaxSettlements { get; set; } = 5;
        public int MaxCities { get; set; } = 4;
        private Dictionary<Island, int> _islands = new Dictionary<Island, int>();

        public PlayerPosition Position { get; set; } = PlayerPosition.BottomLeft;

        public bool IsOpen { get; set; } = false;

        //when you add a new property to the UI you need to
        //  1.  Add a DependencyProperty
        //  2.  Add a property for it
        //  3.  Update SetViewData so that the UI updates

        #region DependencyProperties

        public static readonly DependencyProperty GamesWonProperty = DependencyProperty.Register("GamesWon", typeof(int), typeof(PlayerView), new PropertyMetadata(0));
        public static readonly DependencyProperty GamesPlayedProperty = DependencyProperty.Register("GamesPlayed", typeof(int), typeof(PlayerView), new PropertyMetadata(0));        
        public static readonly DependencyProperty CardsLostToSevenProperty = DependencyProperty.Register("CardsLostToSeven", typeof(int), typeof(PlayerView), new PropertyMetadata(0));
        public static readonly DependencyProperty TimesTargetedProperty = DependencyProperty.Register("TimesTargeted", typeof(int), typeof(PlayerView), new PropertyMetadata(0));
        public static readonly DependencyProperty CardsLostToMonopolyProperty = DependencyProperty.Register("CardsLost", typeof(int), typeof(PlayerView), new PropertyMetadata(0));
        public static readonly DependencyProperty CardsLostToBaronProperty = DependencyProperty.Register("CardsLostToBaron", typeof(int), typeof(PlayerView), new PropertyMetadata(0));
        
        //
        //  these impact the Score
        public static readonly DependencyProperty ScoreProperty = DependencyProperty.Register("Score", typeof(int), typeof(PlayerView), new PropertyMetadata(0, ScoreChanged));
        public static readonly DependencyProperty LargestArmyProperty = DependencyProperty.Register("LargestArmy", typeof(bool), typeof(PlayerView), new PropertyMetadata(false, LargestArmyChanged));
        
        public static readonly DependencyProperty IslandsProperty = DependencyProperty.Register("Islands", typeof(int), typeof(PlayerView), new PropertyMetadata(0, IslandsChanged));
        public static readonly DependencyProperty CitiesPlayedProperty = DependencyProperty.Register("CitiesPlayed", typeof(int), typeof(PlayerView), new PropertyMetadata(0, CitiesPlayedChanged));
        public static readonly DependencyProperty SettlementsPlayedProperty = DependencyProperty.Register("SettlementsPlayed", typeof(int), typeof(PlayerView), new PropertyMetadata(0, SettlementsPlayedChanged));
        public static readonly DependencyProperty ImageFileNameProperty = DependencyProperty.Register("ImageFileName", typeof(string), typeof(PlayerView), new PropertyMetadata("ms-appx:///Assets/guest.jpg"));
        public static readonly DependencyProperty TotalTimeProperty = DependencyProperty.Register("TotalTime", typeof(TimeSpan), typeof(PlayerView), new PropertyMetadata(TimeSpan.FromSeconds(0)));
        public static readonly DependencyProperty SettlementStateProperty = DependencyProperty.Register("SettlementState", typeof(string), typeof(PlayerView), new PropertyMetadata("0 of 5"));
        public static readonly DependencyProperty CityStateProperty = DependencyProperty.Register("CityState", typeof(string), typeof(PlayerView), new PropertyMetadata("0 of 4"));
        public static readonly DependencyProperty RoadStateProperty = DependencyProperty.Register("RoadState", typeof(string), typeof(PlayerView), new PropertyMetadata("0 of 15"));
        public static readonly DependencyProperty ForegroundColorProperty = DependencyProperty.Register("Foreground", typeof(Color), typeof(PlayerView), new PropertyMetadata(Colors.Black, ForegroundColorChanged));
        public static readonly DependencyProperty FillColorProperty = DependencyProperty.Register("Background", typeof(Color), typeof(PlayerView), new PropertyMetadata(Colors.Black, FillColorChanged));
        public static readonly DependencyProperty PlayerNameProperty = DependencyProperty.Register("PlayerName", typeof(string), typeof(PlayerView), new PropertyMetadata("Nameless", PlayerNameChanged));
        public static readonly DependencyProperty KnightsPlayedProperty = DependencyProperty.Register("KnightsPlayed", typeof(int), typeof(PlayerView), new PropertyMetadata(0, KnightsPlayedChanged));
        public static readonly DependencyProperty RoadsPlayedProperty = DependencyProperty.Register("RoadsPlayed", typeof(int), typeof(PlayerView), new PropertyMetadata(0, RoadsPlayedChanged));
        public static readonly DependencyProperty ResourcesAcquiredProperty = DependencyProperty.Register("ResourcesAcquired", typeof(int), typeof(PlayerView), new PropertyMetadata(0, ResourcesAcquiredChanged));
        public static readonly DependencyProperty LongestRoadProperty = DependencyProperty.Register("LongestRoad", typeof(int), typeof(PlayerView), new PropertyMetadata(0, LongestRoadChanged));
        public static readonly DependencyProperty HasLongestRoadProperty = DependencyProperty.Register("HasLongestRoad", typeof(bool), typeof(PlayerView), new PropertyMetadata(false, HasLongestRoadChanged));
        public static readonly DependencyProperty PlayerDataProperty = DependencyProperty.Register("PlayerData", typeof(PlayerData), typeof(PlayerView), new PropertyMetadata(null, PlayerDataChanged));
        public PlayerData PlayerData
        {
            get { return (PlayerData)GetValue(PlayerDataProperty); }
            set { SetValue(PlayerDataProperty, value); }
        }
        private static void PlayerDataChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            PlayerData depPropValue = (PlayerData)e.NewValue;
            depPropClass.SetPlayerData(depPropValue);
        }
        private void SetPlayerData(PlayerData value)
        {
            _data = value;
            UpdateView();
        }

        public bool HasLongestRoad
        {
            get { return (bool)GetValue(HasLongestRoadProperty); }
            set { SetValue(HasLongestRoadProperty, value); }
        }
        private static void HasLongestRoadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            bool depPropValue = (bool)e.NewValue;
            depPropClass.SetHasLongestRoad(depPropValue);
        }
        private void SetHasLongestRoad(bool value)
        {
            _data.GameData.HasLongestRoad = value;
            UpdateScore();
           
        }

        public int LongestRoad
        {
            get { return (int)GetValue(LongestRoadProperty); }
            set { SetValue(LongestRoadProperty, value); }
        }
        private static void LongestRoadChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetLongestRoad(depPropValue);
        }
        private void SetLongestRoad(int value)
        {
            _data.GameData.LongestRoad = value;
           
        }


        public int ResourcesAcquired
        {
            get { return (int)GetValue(ResourcesAcquiredProperty); }
            set { SetValue(ResourcesAcquiredProperty, value); }
        }
        private static void ResourcesAcquiredChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetResourcesAcquired(depPropValue);
        }
        private void SetResourcesAcquired(int value)
        {
            _data.GameData.ResourcesAcquired = value;
           
        }

        public int RoadsPlayed
        {
            get { return (int)GetValue(RoadsPlayedProperty); }
            set { SetValue(RoadsPlayedProperty, value); }
        }
        private static void RoadsPlayedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetRoadsPlayed(depPropValue);
        }
        private void SetRoadsPlayed(int value)
        {
            _data.GameData.RoadsPlayed = value;
           
        }

        public int KnightsPlayed
        {
            get { return (int)GetValue(KnightsPlayedProperty); }
            set { SetValue(KnightsPlayedProperty, value); }
        }
        private static void KnightsPlayedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetKnightsPlayed(depPropValue);
        }
        private void SetKnightsPlayed(int value)
        {
            _data.GameData.KnightsPlayed = value;
          
        }


        public string PlayerName
        {
            get { return (string)GetValue(PlayerNameProperty); }
            set { SetValue(PlayerNameProperty, value); }
        }
        private static void PlayerNameChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            string depPropValue = (string)e.NewValue;
            depPropClass.SetPlayerName(depPropValue);
        }
        private void SetPlayerName(string value)
        {
            if (_data != null)
            {
                _data.PlayerName = value;
            }
        }

        public Color FillColor
        {
            get { return (Color)GetValue(FillColorProperty); }
            set { SetValue(FillColorProperty, value); }
        }
        private static void FillColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetFillColor(depPropValue);
        }
        private void SetFillColor(Color color)
        {

            _data.GameData.FillColor = color;
            _data.Background = color;
           
            Color c = Colors.HotPink;
            if (StaticHelpers.BackgroundToForegroundColorDictionary.TryGetValue(color, out c) == true)
            {
                ForegroundColor = c;
                
            }

        }

        public Color ForegroundColor
        {
            get { return (Color)GetValue(ForegroundColorProperty); }
            set { SetValue(ForegroundColorProperty, value); }
        }
        private static void ForegroundColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            Color depPropValue = (Color)e.NewValue;
            depPropClass.SetForegroundColor(depPropValue);
        }
        private void SetForegroundColor(Color color)
        {
            _data.GameData.ForegroundColor = color;
            _data.Foreground = color;
        }

        

        public int CitiesPlayed
        {
            get { return (int)GetValue(CitiesPlayedProperty); }
            set { SetValue(CitiesPlayedProperty, value); }
        }
        private static void CitiesPlayedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetCitiesPlayed(depPropValue);
        }
        private void SetCitiesPlayed(int newVal)
        {
            _data.GameData.CitiesPlayed = newVal;
            CityState = String.Format($"{newVal} of {MaxCities}");
            UpdateScore();
        }

        public int SettlementsPlayed
        {
            get { return (int)GetValue(SettlementsPlayedProperty); }
            set { SetValue(SettlementsPlayedProperty, value); }
        }
        private static void SettlementsPlayedChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetSettlementsPlayed(depPropValue);
        }
        private void SetSettlementsPlayed(int value)
        {
            _data.GameData.SettlementsPlayed = value;
            SettlementState = String.Format($"{value} of {MaxSettlements}");
            UpdateScore();
        }



        public int Islands
        {
            get { return (int)GetValue(IslandsProperty); }
            set { SetValue(IslandsProperty, value); }
        }
        private static void IslandsChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetIslands(depPropValue);
        }
        private void SetIslands(int newVal)
        {
            UpdateScore();
        }

      

        public bool LargestArmy
        {
            get { return (bool)GetValue(LargestArmyProperty); }
            set { SetValue(LargestArmyProperty, value); }
        }
        private static void LargestArmyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            bool depPropValue = (bool)e.NewValue;
            depPropClass.SetLargestArmy(depPropValue);
        }
        private void SetLargestArmy(bool val)
        {
            _data.GameData.LargestArmy = val;
            UpdateScore();
           
        }

        public int Score
        {
            get { return (int)GetValue(ScoreProperty); }
            set { SetValue(ScoreProperty, value); }
        }
        private static void ScoreChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            PlayerView depPropClass = d as PlayerView;
            int depPropValue = (int)e.NewValue;
            depPropClass.SetScore(depPropValue);
        }
        private void SetScore(int newScore)
        {
            _data.GameData.Score = newScore;
           
        }

        public string SettlementState
        {
            get
            {
                return (string)GetValue(SettlementStateProperty);
            }
            set
            {
                if (value != SettlementState)
                {
                    SetValue(SettlementStateProperty, value);
                }
            }
        }

        public string CityState
        {
            get
            {
                return (string)GetValue(CityStateProperty);
            }
            set
            {
                if (value != CityState)
                {
                    SetValue(CityStateProperty, value);
                }
            }
        }

        public string RoadState
        {
            get
            {
                return (string)GetValue(RoadStateProperty);
            }
            set
            {
                if (value != RoadState)
                {
                    SetValue(RoadStateProperty, value);
                }
            }
        }

        public TimeSpan TotalTime
        {
            get
            {
                return (TimeSpan)GetValue(TotalTimeProperty);
            }
            set
            {

                _data.GameData.TotalTime = value;
                SetValue(TotalTimeProperty, value);
            }
        }



        public string ImageFileName
        {
            get
            {
                return (string)GetValue(ImageFileNameProperty);
            }
            set
            {
                if (value != ImageFileName)
                {
                    _data.ImageFileName = value;
                    SetValue(ImageFileNameProperty, value);
                }
            }
        }
        public int GamesWon
        {
            get
            {
                return (int)GetValue(GamesWonProperty);
            }
            set
            {
                if (value != GamesWon)
                {
                    _data.GamesWon = value;
                    SetValue(GamesWonProperty, value);
                }
            }
        }
        public int GamesPlayed
        {
            get
            {
                return (int)GetValue(GamesPlayedProperty);
            }
            set
            {
                if (value != GamesPlayed)
                {
                    _data.GamesPlayed = value;
                    SetValue(GamesPlayedProperty, value);
                }
            }
        }
      

      
        public int CardsLostToBaron
        {
            get
            {
                return (int)GetValue(CardsLostToBaronProperty);
            }
            set
            {
                if (value != CardsLostToBaron)
                {
                    _data.GameData.CardsLostToBaron = value;
                    SetValue(CardsLostToBaronProperty, value);
                }
            }
        }
        public int CardsLostToMonopoly
        {
            get
            {
                return (int)GetValue(CardsLostToMonopolyProperty);
            }
            set
            {
                if (value != CardsLostToMonopoly)
                {
                    _data.GameData.CardsLost = value;
                    SetValue(CardsLostToMonopolyProperty, value);
                }
            }
        }
        public int TimesTargeted
        {
            get
            {
                return (int)GetValue(TimesTargetedProperty);
            }
            set
            {
                if (value != TimesTargeted)
                {
                    if (_data != null)
                    {
                        _data.GameData.TimesTargeted = value;

                        SetValue(TimesTargetedProperty, value);
                    }
                    
                }
            }
        }
       
        public int CardsLostToSeven
        {
            get
            {
                return (int)GetValue(CardsLostToSevenProperty);
            }
            set
            {
                if (value != CardsLostToSeven)
                {
                    _data.GameData.CardsLostToSeven = value;
                    SetValue(CardsLostToSevenProperty, value);
                }
            }
        }





        #endregion

        public double TitlebarHeight
        {
            get
            {
                return LayoutRoot.RowDefinitions[0].ActualHeight;
            }
        }


        



     

        public void AddIsland(Island island)
        {

            if (_islands.TryGetValue(island, out int refCount))
            {
                refCount++;
            }
            else
            {
                refCount = 1;
            }
            _islands[island] = refCount;
            UpdateScore();
        }

        public void RemoveIsland(Island island)
        {
            int refCount = _islands[island];
            refCount--;
            if (refCount == 0)
            {
                _islands.Remove(island);

            }
            else
            {
                _islands[island] = refCount;
            }
            UpdateScore();
        }
        private void UpdateScore()
        {
            int score = CitiesPlayed * 2 + SettlementsPlayed;

            score += HasLongestRoad ? 2 : 0;
            score += LargestArmy ? 2 : 0;

            Islands = _islands.Count;

            score += _islands.Count;

            Score = score;
        }

        public string ColorAsString
        {
            get
            {
                return StaticHelpers.ColorToStringDictionary[FillColor];
            }
        }


        public void UpdateView()
        {
            //
            //  PlayerData
            this.PlayerName = PlayerData.PlayerName;
            this.GamesWon = PlayerData.GamesWon;
            this.GamesPlayed = PlayerData.GamesPlayed;
            this.ImageFileName = PlayerData.ImageFileName;
            this.FillColor = StaticHelpers.StringToColorDictionary[PlayerData.ColorAsString];

            //
            //  GameData
            this.Score = PlayerData.GameData.Score;
            this.RoadsPlayed = PlayerData.GameData.RoadsPlayed;
            this.CitiesPlayed = PlayerData.GameData.CitiesPlayed;
            this.SettlementsPlayed = PlayerData.GameData.SettlementsPlayed;
            this.KnightsPlayed = PlayerData.GameData.KnightsPlayed;
            this.CardsLostToBaron = PlayerData.GameData.CardsLostToBaron;
            this.CardsLostToMonopoly = PlayerData.GameData.CardsLost;
            this.TimesTargeted = PlayerData.GameData.TimesTargeted;
            this.ResourcesAcquired = PlayerData.GameData.ResourcesAcquired;
            this.CardsLostToSeven = PlayerData.GameData.CardsLostToSeven;
            this.HasLongestRoad = PlayerData.GameData.HasLongestRoad;

        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));

        }
        public PlayerGameData PlayerGameData
        {
            get
            {
                return PlayerData?.GameData;
            }
        }

        public ObservableCollection<RoadCtrl> Roads
        {
            get
            {
                return PlayerData.GameData.Roads;
            }
        }

        public ObservableCollection<BuildingCtrl> Settlements
        {
            get
            {
                return PlayerData.GameData.Buildings;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public PlayerViewOrientation Orientation { get; set; } = PlayerViewOrientation.Bottom;
        public int Index { get; set; } = 0;
      
        public PlayerView()
        {
            this.InitializeComponent();

        }



        public override string ToString()
        {
            return String.Format($"Index={Index};{PlayerName}={StaticHelpers.ColorToStringDictionary[FillColor]}");
        }
        private void OnTitleTapped(object sender, TappedRoutedEventArgs e)
        {

            TitleBarClicked?.Invoke(this);

            //ToggleShow();
        }

        public void Close()
        {
            switch (Orientation)
            {
                case PlayerViewOrientation.Bottom:
                    _daShowY.To = this.ActualHeight - LayoutRoot.RowDefinitions[0].ActualHeight;
                    _daShowX.To = 0;
                    break;
                case PlayerViewOrientation.Right:
                    _daShowX.To = -LayoutRoot.RowDefinitions[0].ActualHeight;
                    _daShowY.To = 0;
                    break;
                case PlayerViewOrientation.Top:
                    _daShowY.To = LayoutRoot.RowDefinitions[0].ActualHeight;
                    _daShowX.To = 0;
                    break;
                case PlayerViewOrientation.Left:
                    _daShowX.To = LayoutRoot.RowDefinitions[0].ActualHeight;
                    _daShowY.To = 0;
                    break;
                default:
                    break;
            }
            _sbShow.Begin();
        }
        public void Open()
        {
            //if (!IsOpen)
            //{
            //    TitleBarClicked?.Invoke(this);
            //}
        }

        public void AnimatePosition(double x, double y)
        {
            _daShowX.To = x;
            _daShowY.To = y;
            _sbShow.Begin();
        }

     

        internal void RemoveRoad(RoadCtrl road)
        {
            _data.GameData.Roads.Remove(road);
            RoadsPlayed--;
        }

        internal void AddRoad(RoadCtrl road)
        {
            _data.GameData.Roads.Add(road);
            RoadsPlayed++;
        }

        Dictionary<ResourceType, ResourceCount> _dictResourceCount = new Dictionary<ResourceType, ResourceCount>();
        internal void UpdateResourceCount(ResourceType resourceType, BuildingState buildingState, bool hasBaron, bool undo)
        {
            if (buildingState == BuildingState.None)
            {
                throw new InvalidDataException("the settlement type shouldn't be None!");
            }

            if (_dictResourceCount.TryGetValue(resourceType, out ResourceCount resCount) == false)
            {
                resCount = new ResourceCount();
                _dictResourceCount[resourceType] = resCount;
            }

            int value = 0;
            if (buildingState == BuildingState.Settlement)
                value = 1;
            else if (buildingState == BuildingState.City)
                value = 2;

            if (undo) value = -value;

            if (hasBaron)
            {
                resCount.Lost += value;
                CardsLostToBaron += value;
            }
            else
            {
                resCount.Acquired += value;
                ResourcesAcquired += value;
            }

            string s = hasBaron ? "Lost" : "Gained";

            this.TraceMessage($"{this.PlayerName} {s} {value} of {resourceType} ");
        }

        internal void AddSettlement(BuildingCtrl settlement)
        {
            
            this.PlayerGameData.Buildings.Add(settlement);
            UpdateSettlementCounts();

        }
        internal void RemoveSettlement(BuildingCtrl settlement)
        {           
            this.PlayerGameData.Buildings.Remove(settlement);
            UpdateSettlementCounts();
        }

        internal void AddCity(BuildingCtrl settlement)
        {           
            this.PlayerGameData.Buildings.Add(settlement);
            UpdateSettlementCounts();

        }
        internal void RemoveCity(BuildingCtrl settlement)
        {
            
            this.PlayerGameData.Buildings.Remove(settlement);
            UpdateSettlementCounts();
        }

        private void UpdateSettlementCounts()
        {
            int Cities = 0;
            int Settlements = 0;

            foreach (BuildingCtrl s in this.PlayerGameData.Buildings)
            {
                if (s.BuildingState == BuildingState.City)
                {
                    Cities++;
                }
                else if (s.BuildingState == BuildingState.Settlement)
                {
                    Settlements++;
                }
            }
            CitiesPlayed = Cities;
            SettlementsPlayed = Settlements;

        }

        public void SetCardCountForAction(CatanAction action, int delta)
        {
            switch (action)
            {
                case CatanAction.Rolled:
              case CatanAction.ChangedState:              
                case CatanAction.ChangedPlayer:                
                case CatanAction.Dealt:
                    this.Assert(false, "Bad action in SetCardCount");
                    break;
                case CatanAction.CardsLost:
                    this.CardsLostToMonopoly += delta;
                    break;
                case CatanAction.CardsLostToSeven:
                    this.CardsLostToSeven += delta;
                    break;
                case CatanAction.MissedOpportunity:
                    this.CardsLostToBaron += delta;
                    break;
                default:
                    break;
            }
        }

        public void StopTimer()
        {
            _stopWatch.StopTimer();
        }


        public void StartTimer()
        {
            _stopWatch.StartTimer();

        }
    }

    class ResourceCount
    {
        public int Acquired { get; set; } = 0;
        public int Lost { get; set; } = 0;

        public override string ToString()
        {
            return String.Format($"Acquired:{Acquired} Lost:{Lost}");
        }
    }
}
