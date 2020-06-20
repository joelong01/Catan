using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Catan10
{
    public class TradeOffer : INotifyPropertyChanged
    {


        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private TradeResources _desire = null;

        private TradeResources _offer = null;

        private PlayerModel _owner = null;
        private bool _ownerApproved = false;
        private bool _partnerApproved = false;
        private PlayerModel _tradePartner = null;

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

        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods


    }
}