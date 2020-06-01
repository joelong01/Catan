using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

// The User Control item template is documented at https://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public sealed partial class RollCtrl : UserControl
    {
        private static void OrientationChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var depPropClass = d as RollCtrl;
            var depPropValue = (TileOrientation)e.NewValue;
            depPropClass?.SetOrientation(depPropValue);
        }

        private void SetOrientation(TileOrientation orientation)
        {
            if (PlaneProjection_FaceDown.RotationY == 0 && orientation == TileOrientation.FaceUp)
            {
                FlipOpen.Begin();
            }
            else if (PlaneProjection_FaceDown.RotationY != 0 && orientation == TileOrientation.FaceDown)
            {
                FlipClose.Begin();
            }
        }

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(TileOrientation), typeof(RollCtrl), new PropertyMetadata(TileOrientation.FaceDown, OrientationChanged));
        public static readonly DependencyProperty RollProperty = DependencyProperty.Register("Roll", typeof(RollModel), typeof(RollCtrl), new PropertyMetadata(new RollModel()));

        public TileOrientation Orientation
        {
            get => (TileOrientation)GetValue(OrientationProperty);
            set => SetValue(OrientationProperty, value);
        }

        public RollModel Roll
        {
            get => (RollModel)GetValue(RollProperty);
            set => SetValue(RollProperty, value);
        }

        public RollCtrl()
        {
            this.InitializeComponent();
            FlipClose.Begin();
        }

        public Task GetFlipTask(TileOrientation orientation)
        {
            if (orientation == TileOrientation.FaceDown) return FlipClose.ToTask();
            if (orientation == TileOrientation.FaceUp) return FlipOpen.ToTask();
            throw new InvalidEnumArgumentException();
        }

        public void Randomize()
        {
            Roll.Randomize();
        }
    }

    public class RollModel : INotifyPropertyChanged
    {
        private int _diceOne = 2;

        private int _diceTwo = 5;

        private TileOrientation _Orientation = TileOrientation.FaceDown;

        private bool _selected = false;

        static private MersenneTwister Twist { get; } = new MersenneTwister((int)DateTime.Now.Ticks);

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        public int DiceOne
        {
            get
            {
                return _diceOne;
            }
            set
            {
                if (value != _diceOne)
                {
                    _diceOne = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Roll");
                }
            }
        }

        public int DiceTwo
        {
            get
            {
                return _diceTwo;
            }
            set
            {
                if (value != _diceTwo)
                {
                    _diceTwo = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Roll");
                }
            }
        }

        [JsonIgnore]
        public bool NotSelected => !Selected;

        public TileOrientation Orientation
        {
            get
            {
                return _Orientation;
            }
            set
            {
                if (_Orientation != value)
                {
                    _Orientation = value;
                    NotifyPropertyChanged();
                }
            }
        }

        [JsonIgnore]
        public int Roll => DiceOne + DiceTwo;

        public bool Selected
        {
            get
            {
                return _selected;
            }
            set
            {
                if (value != _selected)
                {
                    _selected = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("NotSelected");
                }
            }
        }

        public RollModel()
        {
        }

        public void Randomize()
        {
            DiceOne = Twist.Next(1, 7);
            DiceTwo = Twist.Next(1, 7);
            Selected = false;
        }

        public override string ToString()
        {
            return $"[Roll={Roll}][One={DiceOne}][Two={DiceTwo}][Selected={Selected}][Orientation={Orientation}]";
        }
    }
}
