using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;

using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Media;

namespace Catan10
{
    public class ResourceCardCollection : ObservableCollection<ResourceCardModel>
    {
        public static TradeResources ToTradeResources(IEnumerable<ResourceCardModel> resourceCards)
        {
            TradeResources tr = new TradeResources();
            foreach (var card in resourceCards)
            {
                tr.Add(card.ResourceType, 1);
            }
            return tr;
        }

        public void Add(ResourceType resource, bool countVisible = true)
        {
            ResourceCardModel model = new ResourceCardModel()
            {
                ResourceType = resource,
                Orientation = TileOrientation.FaceDown,
                CountVisible = countVisible
            };

            this.Add(model);
        }

        public void Add(DevCardType resource, bool countVisible = false)
        {
            ResourceCardModel model = new ResourceCardModel()
            {
                DevCardType = resource,
                CountVisible = countVisible
            };

            this.Add(model);
        }

        public void AddResources(TradeResources tr)
        {
            foreach (var res in this)
            {
                res.Count += tr.GetCount(res.ResourceType);
            }
        }

        public void AllDown()
        {
            this.ForEach((model) => model.Orientation = TileOrientation.FaceDown);
        }

        public void AllUp()
        {
            this.ForEach((model) => model.Orientation = TileOrientation.FaceUp);
        }

        public void InitalizeResources(TradeResources tr)
        {
            foreach (ResourceType resourceType in Enum.GetValues(typeof(ResourceType)))
            {
                int count = tr.GetCount(resourceType);
                for (int i = 0; i < count; i++)
                {
                    Add(resourceType, false);
                }
            }
        }

        public void InitWithAllResources()
        {
            this.Add(ResourceType.Wood);
            this.Add(ResourceType.Brick);
            this.Add(ResourceType.Wheat);
            this.Add(ResourceType.Sheep);
            this.Add(ResourceType.Ore);
        }
    }

    /// <summary>
    ///     This has all the data associated witht a ResourceCard
    /// </summary>
    public class ResourceCardModel : INotifyPropertyChanged
    {
        private int _count = 0;

        private bool _countVisible = true;

        private DevCardType _devCardType = DevCardType.None;

        private HarborType _harborType = HarborType.None;

        private Visibility _harborVisibility = Visibility.Collapsed;

        private TileOrientation _orientation = TileOrientation.FaceDown;

        private PlayerModel _owner = null;

        private bool _readOnly = false;

        private ResourceType _resourceType = ResourceType.Back;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        // the number of this type

        // do you want to show the Count in a UI?

        // DevCardType.Unknown means this is a ResourceType

        // ??

        // ??

        // should you show this face up or face down

        // who owns this (back pointer)

        public Brush BackBrush
        {
            get
            {
                if (DevCardType != DevCardType.None)
                {
                    return (Brush)App.Current.Resources["DevCardType.Back"];
                }

                return (Brush)App.Current.Resources["ResourceType.Back"];
            }
        }

        public int Count
        {
            get
            {
                return _count;
            }
            set
            {
                if (_count != value)
                {
                    _count = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public bool CountVisible
        {
            get
            {
                return _countVisible;
            }
            set
            {
                if (_countVisible != value)
                {
                    _countVisible = value;
                    NotifyPropertyChanged();
                }
            }
        }

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
                    NotifyPropertyChanged("BackBrush");
                    NotifyPropertyChanged("FrontBrush");
                }
            }
        }

        public Brush FrontBrush
        {
            get
            {
                if (DevCardType != DevCardType.None)
                {
                    return (Brush)App.Current.Resources["DevCardType." + DevCardType.ToString()];
                }

                if (ResourceType == ResourceType.None)
                {
                    return (Brush)App.Current.Resources["ResourceType.Back"];
                }

                return (Brush)App.Current.Resources["ResourceType." + ResourceType.ToString()];
            }
        }

        public Brush HarborBrush
        {
            get
            {
                if (HarborType != HarborType.None)
                {
                    return (Brush)App.Current.Resources["HarborType." + HarborType.ToString()];
                }

                return new SolidColorBrush(Colors.Transparent);
            }
        }

        public HarborType HarborType
        {
            get
            {
                return _harborType;
            }
            set
            {
                if (_harborType != value)
                {
                    _harborType = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public Visibility HarborVisibility
        {
            get
            {
                return _harborVisibility;
            }
            set
            {
                if (_harborVisibility != value)
                {
                    _harborVisibility = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public TileOrientation Orientation
        {
            get
            {
                return _orientation;
            }
            set
            {
                if (_orientation != value)
                {
                    _orientation = value;
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

        public bool ReadOnly
        {
            get
            {
                return _readOnly;
            }
            set
            {
                if (_readOnly != value)
                {
                    _readOnly = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public ResourceType ResourceType
        {
            get
            {
                return _resourceType;
            }
            set
            {
                if (_resourceType != value)
                {
                    _resourceType = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("BackBrush");
                    NotifyPropertyChanged("FrontBrush");
                }
            }
        }

        // ResourceType.None means it is a DevCard
        public ResourceCardModel()
        {
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public TileOrientation CountToOrientation(int count)
        {
            if (count > 0)
            {
                return TileOrientation.FaceUp;
            }

            return TileOrientation.FaceDown;
        }
    }
}
