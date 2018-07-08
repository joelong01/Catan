﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Search;
using Windows.Storage.Streams;
using Windows.UI;
using Windows.UI.Popups;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Navigation;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=234238

namespace Catan10
{

    public sealed partial class MainPage : Page
    {

        public static readonly DependencyProperty StateDescriptionProperty = DependencyProperty.Register("StateDescription", typeof(string), typeof(MainPage), new PropertyMetadata("Hit Start"));
        public string StateDescription
        {
            get{return (string)GetValue(StateDescriptionProperty);}
            set{SetValue(StateDescriptionProperty, value);}
        }
        public static readonly DependencyProperty GameStateProperty = DependencyProperty.Register("GameState", typeof(GameState), typeof(MainPage), new PropertyMetadata(GameState.WaitingForNewGame));
        public static readonly DependencyProperty CurrentPlayerProperty = DependencyProperty.Register("CurrentPlayer", typeof(PlayerData), typeof(MainPage), new PropertyMetadata(null, CurrentPlayerChanged));
        public PlayerData CurrentPlayer
        {
            get => (PlayerData)GetValue(CurrentPlayerProperty);
            set => SetValue(CurrentPlayerProperty, value);
        }
        private static void CurrentPlayerChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as MainPage;
            var depPropValue = (PlayerData)e.NewValue;
            depPropClass?.SetCurrentPlayer(depPropValue);
        }
        private void SetCurrentPlayer(PlayerData player)
        {
            //
            //  the next player can always play a baron once
            player.GameData.PlayedKnightThisTurn = false;
            player.GameData.MovedBaronAfterRollingSeven = null;

            UpdateTurnFlag();

            _stopWatchForTurn.TotalTime = TimeSpan.FromSeconds(0);
            _stopWatchForTurn.StartTimer();

            if (GameState == GameState.AllocateResourceForward || GameState == GameState.AllocateResourceReverse)
            {

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                HideAllPipEllipses();
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                _showPipGroupIndex = 0;

            }

            // tell all the Buildings that the CurrentPlayer has changed
            foreach (var building in _gameView.AllBuildings)
            {
                building.CurrentPlayer = player;
            }
        }
        #region RollProperties
        public static readonly DependencyProperty TotalRollsProperty = DependencyProperty.Register("TotalRolls", typeof(int), typeof(MainPage), new PropertyMetadata(0));
        public static readonly DependencyProperty TwoPercentProperty = DependencyProperty.Register("TwoPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty ThreePercentProperty = DependencyProperty.Register("ThreePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty FourPercentProperty = DependencyProperty.Register("FourPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty FivePercentProperty = DependencyProperty.Register("FivePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty SixPercentProperty = DependencyProperty.Register("SixPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty SevenPercentProperty = DependencyProperty.Register("SevenPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty EightPercentProperty = DependencyProperty.Register("EightPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty NinePercentProperty = DependencyProperty.Register("NinePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty TenPercentProperty = DependencyProperty.Register("TenPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty ElevenPercentProperty = DependencyProperty.Register("ElevenPercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));
        public static readonly DependencyProperty TwelvePercentProperty = DependencyProperty.Register("TwelvePercent", typeof(string), typeof(MainPage), new PropertyMetadata("0 (0%)"));



        public string TwelvePercent
        {
            get
            {
                return (string)GetValue(TwelvePercentProperty);
            }
            set
            {
                SetValue(TwelvePercentProperty, value);
            }
        }
        public string ElevenPercent
        {
            get
            {
                return (string)GetValue(ElevenPercentProperty);
            }
            set
            {
                SetValue(ElevenPercentProperty, value);
            }
        }
        public string TenPercent
        {
            get
            {
                return (string)GetValue(TenPercentProperty);
            }
            set
            {
                SetValue(TenPercentProperty, value);
            }
        }

        public string NinePercent
        {
            get
            {
                return (string)GetValue(NinePercentProperty);
            }
            set
            {
                SetValue(NinePercentProperty, value);
            }
        }
        public string EightPercent
        {
            get
            {
                return (string)GetValue(EightPercentProperty);
            }
            set
            {
                SetValue(EightPercentProperty, value);
            }
        }
        public string SevenPercent
        {
            get
            {
                return (string)GetValue(SevenPercentProperty);
            }
            set
            {
                SetValue(SevenPercentProperty, value);
            }
        }
        public string SixPercent
        {
            get
            {
                return (string)GetValue(SixPercentProperty);
            }
            set
            {
                SetValue(SixPercentProperty, value);
            }
        }
        public string FivePercent
        {
            get
            {
                return (string)GetValue(FivePercentProperty);
            }
            set
            {
                SetValue(FivePercentProperty, value);
            }
        }
        public string FourPercent
        {
            get
            {
                return (string)GetValue(FourPercentProperty);
            }
            set
            {
                SetValue(FourPercentProperty, value);
            }
        }
        public string ThreePercent
        {
            get
            {
                return (string)GetValue(ThreePercentProperty);
            }
            set
            {
                SetValue(ThreePercentProperty, value);
            }
        }
        public string TwoPercent
        {
            get
            {
                return (string)GetValue(TwoPercentProperty);
            }
            set
            {
                SetValue(TwoPercentProperty, value);
            }
        }
        public int TotalRolls
        {
            get
            {
                return (int)GetValue(TotalRollsProperty);
            }
            set
            {
                SetValue(TotalRollsProperty, value);
            }
        }
        #endregion
       
    }
}