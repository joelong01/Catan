using System;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class ResourceCardCtrl : UserControl
    {
        private static void OrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ResourceCardCtrl;
            var depPropValue = (TileOrientation)e.NewValue;
            depPropClass?.SetOrientation(depPropValue);
        }

        //    if (e.NewItems != null && e.NewItems.Count == 1)
        //    {
        //        HarborType type = ((Harbor)e.NewItems[0]).HarborType;
        //        if (StaticHelpers.HarborTypeToResourceType(type) == ResourceType)
        //        {
        //            // Debug.WriteLine($" =======> Making {type} Visible for ResourceType {ResourceType}");
        //            HarborVisibility = Visibility.Visible;
        //        }
        //    }
        //    if (e.OldItems != null && e.OldItems.Count == 1)
        //    {
        //        HarborType type = ((Harbor)e.OldItems[0]).HarborType;
        //        if (StaticHelpers.HarborTypeToResourceType(type) == ResourceType)
        //        {
        //            // Debug.WriteLine($" =======> Making {type} Collapsed for ResourceType {ResourceType}");
        //            HarborVisibility = Visibility.Collapsed;
        //        }
        //    }
        //}
        private static void ResourceCardModelChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ResourceCardCtrl;
            var depPropValue = (ResourceCardModel)e.NewValue;
            depPropClass?.SetResourceCardModel(depPropValue);
        }

        //private void OwnedHarbors_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        //{
        //    //  left here for debugging purposes
        //    //
        //    //  ObservableCollection<Harbor> col = (ObservableCollection<Harbor>)sender;
        private void SetOrientation(TileOrientation orientation)
        {
            if (orientation == TileOrientation.FaceUp)
            {
                FlipOpen.Begin();
            }
            else
            {
                FlipClose.Begin();
            }
        }

        private void SetResourceCardModel(ResourceCardModel resourceCard)
        {
        }

        private void Text_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        public ResourceCardCtrl()
        {
            this.InitializeComponent();
        }

        public MainPageModel MainPageModel
        {
            get => (MainPageModel)GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }

        public TileOrientation Orientation
        {
            get => (TileOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public ResourceCardModel ResourceCardModel
        {
            get => (ResourceCardModel)GetValue(ResourceCardModelProperty);
            set => SetValue(ResourceCardModelProperty, value);
        }

        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(ResourceCardCtrl), new PropertyMetadata(null));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(TileOrientation), typeof(ResourceCardCtrl), new PropertyMetadata(TileOrientation.FaceDown, OrientationChanged));

        public static readonly DependencyProperty ResourceCardModelProperty = DependencyProperty.Register("ResourceCardModel", typeof(ResourceCardModel), typeof(ResourceCardCtrl), new PropertyMetadata(null, ResourceCardModelChanged));

        /// <summary>
        ///     a XAML x:Bind called function to set the visibility of the harbor based on the resource type of the model
        /// </summary>
        /// <returns></returns>
        //
        public bool HarborTypeOwned()
        {
            ResourceType resType = ResourceCardModel.ResourceType;
            return false;
        }
    }
}
