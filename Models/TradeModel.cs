using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using Catan.Proxy;
using Catan10.CatanService;

namespace Catan10
{
   
    public class PlayerTradeTracker : INotifyPropertyChanged
    {
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _inTrade = false;
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

    public class OldTradeOffer : INotifyPropertyChanged
    {

        #region Constructors + Destructors

        public OldTradeOffer()
        {
        }

        #endregion Constructors + Destructors

        PlayerModel _singlePartner = null;
        
        /// <summary>
        ///     when a message is recieved we zero out the list of partners and add this for the one partner that owns/approves the trade
        ///     used for databinding
        /// </summary>
        public PlayerModel SinglePartner
        {
            get
            {
                return _singlePartner;
            }
            set
            {
                if (_singlePartner != value)
                {
                    _singlePartner = value;
                    NotifyPropertyChanged();
                }
            }
        }

        string _singlePartnerName = "";
        public string SinglePartnerName
        {
            get
            {
                return _singlePartnerName;
            }
            set
            {
                if (_singlePartnerName != value)
                {
                    _singlePartnerName = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private TradeResources _desire = new TradeResources();

        private TradeResources _offer = new TradeResources();

        private PlayerModel _owner = MainPage.Current?.TheHuman;
        private bool _ownerApproved = false;
        private bool _partnerApproved = false;
        private ObservableCollection<PlayerTradeTracker> _tradePartners = new ObservableCollection<PlayerTradeTracker>();
        string _ownerName = "";
        public string OwnerName
        {
            get
            {
                return _ownerName;
            }
            set
            {
                if (_ownerName != value)
                {
                    _ownerName = value;
                    NotifyPropertyChanged();
                }
            }
        }

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
        ///     This adds PlayingPlayers to the PotentialTradePartners along with a bool to indicate they are being offered the trade
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
        [JsonIgnore]
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
                    OwnerName = value.PlayerName;
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
            
            return JsonSerializer.Deserialize<TradeOffer>(json, CatanSignalRClient.GetJsonOptions());
        }

        public string Serialize()
        {            
            return JsonSerializer.Serialize(this, CatanSignalRClient.GetJsonOptions());
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}