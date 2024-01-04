using System.Drawing;
using System.Security.AccessControl;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Markup;
using Windows.UI.Xaml.Media;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    [ContentProperty(Name = "Child")]
    public sealed partial class ResourceCardCtrl : UserControl
    {
        private static void OrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as ResourceCardCtrl;

            depPropClass?.SetOrientation(( TileOrientation )e.OldValue, ( TileOrientation )e.NewValue);
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
        private void SetOrientation(TileOrientation oldValue, TileOrientation newValue)
        {
            if (oldValue == newValue) return;

            if (newValue == TileOrientation.FaceUp)
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
            this.TraceMessage($"{resourceCard}");
            switch (resourceCard.ResourceType)
            {
                case ResourceType.Science:
                    Child = new ScienceCtrl()
                    {
                        Stroke = new SolidColorBrush(Windows.UI.Colors.White),
                        StrokeThickness = 10.0,
                        Width = 67,
                        Height = 100
                    };
                    break;
                case ResourceType.Trade:
                    Child = new TradeCtrl()
                    {
                        Width = 67,
                        Height = 100
                    };
                    break;
                case ResourceType.Politics:
                    Child = new PoliticsCtrl()
                    {
                        Stroke = new SolidColorBrush(Windows.UI.Colors.White),
                        StrokeThickness = 10.0,
                        Width = 67,
                        Height = 100
                    };
                    break;
                default:
                    Child = null;
                    string key = "ResourceType." + resourceCard.ResourceType.ToString();
                    var brush = ( Brush )App.Current.Resources[key];
                    var rectangle = new Windows.UI.Xaml.Shapes.Rectangle
                    {
                        Fill = brush,
                        Width=67,
                        Height=100
                    };
                    Child = rectangle;
                    break;

            }

        }

        private void Text_GotFocus(object sender, RoutedEventArgs e)
        {
        }

        public MainPageModel MainPageModel
        {
            get => ( MainPageModel )GetValue(MainPageModelProperty);
            set => SetValue(MainPageModelProperty, value);
        }

        public TileOrientation Orientation
        {
            get => ( TileOrientation )GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public ResourceCardModel ResourceCardModel
        {
            get => ( ResourceCardModel )GetValue(ResourceCardModelProperty);
            set => SetValue(ResourceCardModelProperty, value);
        }

        public static readonly DependencyProperty MainPageModelProperty = DependencyProperty.Register("MainPageModel", typeof(MainPageModel), typeof(ResourceCardCtrl), new PropertyMetadata(null));

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(TileOrientation), typeof(ResourceCardCtrl), new PropertyMetadata(TileOrientation.FaceDown, OrientationChanged));

        public static readonly DependencyProperty ResourceCardModelProperty = DependencyProperty.Register("ResourceCardModel", typeof(ResourceCardModel), typeof(ResourceCardCtrl), new PropertyMetadata(null, ResourceCardModelChanged));
        public static readonly DependencyProperty ChildProperty = DependencyProperty.Register(nameof(Child), typeof(UIElement), typeof(ResourceCardCtrl), new PropertyMetadata(null));
        public UIElement Child
        {
            get { return ( UIElement )GetValue(ChildProperty); }
            set { SetValue(ChildProperty, value); }
        }
        public ResourceCardCtrl()
        {
            this.InitializeComponent();
        }

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
