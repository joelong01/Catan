using System;
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
    }

    public class TradeOffer : INotifyPropertyChanged
    {
        public TradeOffer() 
        {
           
        }
        
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private TradeResources _desire = new TradeResources();

        private TradeResources _offer = new TradeResources();

        private PlayerModel _owner = MainPage.Current?.TheHuman;
        private bool _ownerApproved = false;
        private bool _partnerApproved = false;
        private PlayerModel _tradePartner = null;
        ObservableCollection<PlayerModel> _tradePartners = new ObservableCollection<PlayerModel>();
        public ObservableCollection<PlayerModel> TradePartners
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

        public PlayerModel TradePartner
        {
            get
            {
                return _tradePartner;
            }
            set
            {
                if (_tradePartner != value)
                {
                    _tradePartner = value;
                    NotifyPropertyChanged();
                }
            }
        }

        #endregion Properties

        #region Methods

        public string Serialize()
        {
            var jsonOptions = CatanProxy.GetJsonOptions();
            jsonOptions.Converters.Add(new PlayerModelConverter());
            return JsonSerializer.Serialize(this, jsonOptions);
        }

        public static TradeOffer Deserialze(string json)
        {
            var jsonOptions = CatanProxy.GetJsonOptions();
            jsonOptions.Converters.Add(new PlayerModelConverter());
            return JsonSerializer.Deserialize<TradeOffer>(json, jsonOptions);
        }

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods


    }
}