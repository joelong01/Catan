using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Catan10
{
    public class TradeOffer : INotifyPropertyChanged
    {
        public TradeOffer()
        {
            
        }
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private Offer _owner = new Offer();
        private Offer _partner = new Offer();

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public Offer Owner
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

        public Offer Partner
        {
            get
            {
                return _partner;
            }
            set
            {
                if (_partner != value)
                {
                    _partner = value;
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

    public class Offer : INotifyPropertyChanged
    {
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private bool _approved = false;
        
        private TradeResources _resources = new TradeResources();

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        PlayerModel _player = null;

        public bool Approved
        {
            get
            {
                return _approved;
            }
            set
            {
                if (_approved != value)
                {
                    _approved = value;
                    NotifyPropertyChanged();
                }
            }
        }
        public PlayerModel Player
        {
            get
            {
                return _player;
            }
            set
            {
                if (_player != value)
                {
                    _player = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TradeResources Resources
        {
            get
            {
                return _resources;
            }
            set
            {
                if (_resources != value)
                {
                    _resources = value;
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