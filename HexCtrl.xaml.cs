using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    

    public sealed partial class HexCtrl : UserControl, INotifyPropertyChanged
    {
        ResourceType myType = ResourceType.None;
        SolidColorBrush _brRegular = new SolidColorBrush(Colors.BurlyWood);
        SolidColorBrush _brHighlight = new SolidColorBrush(Colors.Red);
        public event PropertyChangedEventHandler PropertyChanged;
        bool _useClassic = false;
        HarborLocation _harborLocation = HarborLocation.None;
     


        public HexCtrl()
        {
            this.InitializeComponent();
            _polygon.Stroke = _brRegular;
          
        }

        public ResourceType ResourceType
        {
            get
            {
                return myType;
            }
            set
            {


                myType = value;
                string bitmapPath = "ms-appx:Assets/back.jpg";

                switch (value)
                {
                    case ResourceType.Sheep:
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old sheep.png" : "ms-appx:Assets/sheep.jpg";
                        break;
                    case ResourceType.Wood:
                        //bitmapPath = "ms-appx:Assets/wood.jpg";
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old wood.png" : "ms-appx:Assets/wood.jpg";
                        break;
                    case ResourceType.Ore:
                        //bitmapPath = "ms-appx:Assets/ore.jpg";
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old ore.png" : "ms-appx:Assets/ore.jpg";
                        break;
                    case ResourceType.Wheat:
                        //bitmapPath = "ms-appx:Assets/wheat.jpg";
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old wheat.png" : "ms-appx:Assets/wheat.jpg";
                        break;
                    case ResourceType.Brick:
                        //bitmapPath = "ms-appx:Assets/brick.jpg";
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old brick.png" : "ms-appx:Assets/brick.jpg";
                        break;
                    case ResourceType.None:
                        bitmapPath = "ms-appx:Assets/back.jpg";
                        break;
                    case ResourceType.Desert:
                        //bitmapPath = "ms-appx:Assets/desert.jpg";
                        bitmapPath = _useClassic ? "ms-appx:Assets/Old Visuals/old desert.png" : "ms-appx:Assets/desert.jpg";
                        break;
                    default:
                        break;

                }
                BitmapImage bitmapImage = new BitmapImage(new Uri(bitmapPath, UriKind.RelativeOrAbsolute));
                _brush.ImageSource = bitmapImage;
                _brush.Stretch = Stretch.UniformToFill;
                _polygon.UpdateLayout();
                NotifyPropertyChanged();
            }

        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }
        }

        public bool Highlight
        {
            get
            {
                return (_polygon.Stroke == _brHighlight);
            }
            set
            {
                if (value)
                {
                    _polygon.Stroke = _brHighlight;
                }
                else
                {
                    _polygon.Stroke = _brRegular;
                }

            }
        }

        public bool UseClassic
        {
            get
            {
                return _useClassic;
            }

            set
            {
                _useClassic = value;
                ResourceType = myType; // will change images if needed
            }
        }

        public HarborLocation HarborLocation
        {
            get
            {
                return _harborLocation;
            }

            set
            {
                _harborLocation = value;

            }
        }

        
        TileOrientation _orientation = TileOrientation.FaceUp;       
        public TileOrientation Orientation
        {
            get { return _orientation; }
            set
            {

                _orientation = value;
                if (_orientation == TileOrientation.FaceUp)
                {
                    _transform.RotationY = 0;
                    
                    
                }
                else
                {
                    _transform.RotationY = -90;
                }

            }
        }

        public Task FlipTask(double to, double animationTimeInMs,  double startAfter = 0)
        {
            _daFlip.To = to;
            _daFlip.BeginTime = TimeSpan.FromMilliseconds(startAfter);
            _daFlip.Duration = TimeSpan.FromMilliseconds(animationTimeInMs);
            return _sbFlip.ToTask();

        }
    }
}
