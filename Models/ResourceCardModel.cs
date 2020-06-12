﻿using System;
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
    /// <summary>
    ///     this class exists so that we can bind UI to a *list* of ResourceCards
    ///     It is born with 
    ///         - 0 dev cards
    ///         - Brick, Wood, wheat, Shee, Ore => all 0 count and facedown
    ///         - note: NO GOLDMINE.
    ///         
    /// </summary>
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

        public ResourceCardCollection()
        {
            //
            //  put the right resources in the collection and set them to 0 count
            this.Reset();
        }


        private void Add(ResourceType resource, bool countVisible = true)
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

        ResourceCardModel ModelForResource(ResourceType resourceType)
        {
            foreach (var model in this)
            {
                if (model.ResourceType == resourceType) return model;
            }

            throw new ArgumentException("bad resource type", nameof(resourceType));
        }

        public void AddResources(TradeResources tr)
        {
            foreach (var resType in tr.NonZeroResources)
            {
                var model = ModelForResource(resType);
                model.Count = tr.CountForResource(resType);
            }
        }

        public void AllDown()
        {
            this.ForEach((model) => model.Orientation = TileOrientation.FaceDown);
        }

        internal void RemoveGold()
        {
            for (int i = this.Count -1; i >= 0;  i--)
            {
                if (this[i].ResourceType == ResourceType.GoldMine)
                {
                    this.RemoveAt(i);
                    return;
                }
            }

        }

        public void AllUp()
        {
            this.ForEach((model) => model.Orientation = TileOrientation.FaceUp);
        }

       

        public void Reset()
        {
            this.Clear();
            this.Add(ResourceType.Wood);
            this.Add(ResourceType.Brick);
            this.Add(ResourceType.Wheat);
            this.Add(ResourceType.Sheep);
            this.Add(ResourceType.Ore);


          
        }

    

        internal void Shuffle()
        {
            Random rand = new Random((int)DateTime.Now.Ticks);
            for (int i = 0; i< this.Count; i++)
            {
                int index = rand.Next(this.Count);
                this.Swap(i, index);
            }
        }

        internal void AddGoldMine()
        {
            this.Add(ResourceType.GoldMine);
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

        private string _ownerName = null;

        private bool _readOnly = false;

        private ResourceType _resourceType = ResourceType.Back;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        [JsonIgnore]
        public PlayerModel Owner
        {
            get
            {
                return MainPage.Current.NameToPlayer(_ownerName);
            }
        }
        [JsonIgnore]
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
        [JsonIgnore]
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
        [JsonIgnore]
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
