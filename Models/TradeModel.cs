using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text.Json;
using System.Text.Json.Serialization;
using Catan.Proxy;

namespace Catan10
{
    public class PlayerModelConverter : JsonConverter<PlayerModel>
    {
        #region Methods

        public override PlayerModel Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        {
            if (typeToConvert == typeof(string))
            {
                string playerName = reader.GetString();
                return MainPage.Current.NameToPlayer(playerName);
            }

            return null;
        }

        public override void Write(Utf8JsonWriter writer, PlayerModel player, JsonSerializerOptions options)
        {
            writer.WriteStringValue(player.PlayerName);
        }

        #endregion Methods
    }

    public class PlayerTradeTracker : INotifyPropertyChanged
    {
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        bool _inTrade = false;
        private Guid _playerId;
        private string playerName;
        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public bool InTrade
        {
            get
            {
                return _inTrade;
            }
            set
            {
                if (_inTrade != value)
                {
                    _inTrade = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public Guid PlayerIdentifier
        {
            get
            {
                return _playerId;
            }
            set
            {
                if (value != _playerId)
                {
                    _playerId = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public string PlayerName
        {
            get => playerName;
            set
            {
                if (playerName != value)
                {
                    playerName = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public override string ToString()
        {
            return $"[InTrade={InTrade}][Name={PlayerName}]";
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion Properties
    }
    public class TradeOffer : INotifyPropertyChanged
    {
        #region Constructors + Destructors

        public TradeOffer()
        {
        }

        #endregion Constructors + Destructors

        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private TradeResources _desire = new TradeResources();

        private TradeResources _offer = new TradeResources();

        private PlayerModel _owner = MainPage.Current?.TheHuman;
        private bool _ownerApproved = false;
        private bool _partnerApproved = false;
        ObservableCollection<PlayerTradeTracker> _tradePartners = new ObservableCollection<PlayerTradeTracker>();

        public ObservableCollection<PlayerTradeTracker> TradePartners
        {
            get
            {
                return _tradePartners;
            }
            set
            {
                if (_tradePartners != value)
                {
                    _tradePartners = value;
                    NotifyPropertyChanged();
                }
            }
        }

        /// <summary>
        ///     This addes PlayingPlayers to the PotentialTradePartners along with a bool to indicate they are being offered the trade
        /// </summary>
        /// <param name="players"></param>
        public void AddPotentialTradingPartners(ObservableCollection<PlayerModel> players)
        {
            TradePartners.Clear();
            foreach (var player in players)
            {
                //
                //  6/22 - while you can't trade with yourself, we need "self" in the 
                //         list so that we can get the Owner picture
                //  


                PlayerTradeTracker tracker = new PlayerTradeTracker()
                {
                    PlayerName = player.PlayerName,
                    InTrade = false,
                    PlayerIdentifier = player.PlayerIdentifier
                };
                TradePartners.Add(tracker);
            }
        }
        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public TradeResources Desire
        {
            get
            {
                return _desire;
            }
            set
            {
                if (_desire != value)
                {
                    _desire = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TradeResources Offer
        {
            get
            {
                return _offer;
            }
            set
            {
                if (_offer != value)
                {
                    _offer = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public PlayerModel Owner
        {
            get
            {
                return _owner;
            }
            set
            {
                if (_owner != value)
                {
                    _owner = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool OwnerApproved
        {
            get
            {
                return _ownerApproved;
            }
            set
            {
                if (_ownerApproved != value)
                {
                    _ownerApproved = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool PartnerApproved
        {
            get
            {
                return _partnerApproved;
            }
            set
            {
                if (_partnerApproved != value)
                {
                    _partnerApproved = value;
                    NotifyPropertyChanged();
                }
            }
        }


        #endregion Properties

        #region Methods

        public static TradeOffer Deserialze(string json)
        {
            var jsonOptions = CatanProxy.GetJsonOptions();
            jsonOptions.Converters.Add(new PlayerModelConverter());
            return JsonSerializer.Deserialize<TradeOffer>(json, jsonOptions);
        }

        public string Serialize()
        {
            var jsonOptions = CatanProxy.GetJsonOptions();
            jsonOptions.Converters.Add(new PlayerModelConverter());
            return JsonSerializer.Serialize(this, jsonOptions);
        }
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods


    }
}