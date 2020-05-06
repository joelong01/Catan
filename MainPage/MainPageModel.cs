﻿using Catan.Proxy;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

namespace Catan10
{
    public class ServiceData
    {
        public CatanProxy Proxy { get; } = new CatanProxy() { HostName = "http://192.168.1.128:5000" };
        public string HostName
        {
            get
            {
                return Proxy.HostName;
            }
            set
            {

                Proxy.HostName = value;

            }
        }

        public SessionInfo SessionInfo { get; set; }
        

        public ServiceData()
        {
            Proxy.HostName = HostName;
        }
    }

    public class MainPageModel : INotifyPropertyChanged
    {
        public ServiceData ServiceData { get; } = new ServiceData();
        bool _EnableUiInteraction = true;
        Log _Log = new Log();
        private bool _isServiceGame = false;

        public bool IsServiceGame
        {
            get => _isServiceGame;
            set
            {
                if (value != _isServiceGame)
                {
                    _isServiceGame = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public MainPageModel()
        {
            _Log.PropertyChanged += Log_PropertyChanged;
        }

        private void Log_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            // this.TraceMessage($"Log_PropertyChanged: [Property={e.PropertyName}] [GameState={_Log.GameState}");
            if (e.PropertyName == "GameState")
            {

                NotifyPropertyChanged("EnableNextButton");
                NotifyPropertyChanged("EnableRedo");
            }


        }

        private ObservableCollection<PlayerModel> _PlayingPlayers = new ObservableCollection<PlayerModel>();
        [JsonIgnore]
        public ObservableCollection<PlayerModel> PlayingPlayers
        {
            get
            {
                return _PlayingPlayers;
            }
            set
            {
                if (_PlayingPlayers != value)
                {
                    _PlayingPlayers = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public PlayerModel GameStartedBy { get; internal set; }

  
        public bool EnableUiInteraction
        {
            get
            {
                return _EnableUiInteraction;
            }
            set
            {
                if (_EnableUiInteraction != value)
                {
                    _EnableUiInteraction = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("EnableRedo");
                    NotifyPropertyChanged("EnableNextButton");
                }
            }
        }


        public bool EnableNextButton
        {
            get
            {
                GameState state = _Log.GameState;
                return (EnableUiInteraction && (state == GameState.WaitingForNewGame || state == GameState.WaitingForNext || state == GameState.WaitingForStart ||
                        state == GameState.DoneSupplemental || state == GameState.Supplemental || state == GameState.AllocateResourceForward || state == GameState.AllocateResourceReverse ||
                        state == GameState.DoneResourceAllocation));

            }
        }




        public bool EnableRedo
        {
            get
            {
                return (Log.UndoCount > 0 && EnableUiInteraction);
            }
        }

      

        [JsonIgnore]
        public Log Log
        {
            get
            {
                return _Log;
            }
            set
            {
                if (_Log != value)
                {
                    _Log = value;
                    NotifyPropertyChanged();
                }
            }
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}