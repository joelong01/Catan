using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Windows.UI;
using Windows.UI.Xaml.Media;

namespace Catan10
{
    public class DevCardModel : INotifyPropertyChanged
    {
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private DevCardType _devCardType = DevCardType.None;

        private bool _played = false;

        #endregion Delegates + Fields + Events + Enums

        #region Properties

        public DevCardType DevCardType
        {
            get
            {
                return _devCardType;
            }
            set
            {
                if (_devCardType != value)
                {
                    _devCardType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool Played
        {
            get
            {
                return _played;
            }
            set
            {
                if (_played != value)
                {
                    _played = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public DevCardModel Self => this;

        #endregion Properties

        #region Methods

        public static Brush DevCardTypeToImage(DevCardType devCardType)
        {
            if (StaticHelpers.IsInVisualStudioDesignMode)
            {
                return new SolidColorBrush(Colors.AliceBlue);
            }

            string key = "DevCardType." + devCardType.ToString();
            if (devCardType == DevCardType.Back || devCardType == DevCardType.None)
            {
                return App.Current.Resources["DevCardType.Back"] as ImageBrush;
            }
            return App.Current.Resources[key] as ImageBrush;
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}