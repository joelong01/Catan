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
        #region Delegates + Fields + Events + Enums

        public static readonly DependencyProperty OrientationProperty = DependencyProperty.Register("Orientation", typeof(TileOrientation), typeof(RollCtrl), new PropertyMetadata(TileOrientation.FaceDown, OrientationChanged));

        public static readonly DependencyProperty RollProperty = DependencyProperty.Register("Roll", typeof(RollModel), typeof(RollCtrl), new PropertyMetadata(new RollModel()));

        #endregion Delegates + Fields + Events + Enums

        #region Properties

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

        #endregion Properties

        #region Constructors + Destructors

        public RollCtrl()
        {
            this.InitializeComponent();
            FlipClose.Begin();
        }

        #endregion Constructors + Destructors

        #region Methods

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

        #endregion Methods
    }

    public class RollModel : INotifyPropertyChanged
    {
        #region Delegates + Fields + Events + Enums

        public event PropertyChangedEventHandler PropertyChanged;

        private int _diceOne = -1;
        private int _diceTwo = -1;
        private TileOrientation _Orientation = TileOrientation.FaceDown;
        private bool _selected = false;

        #endregion Delegates + Fields + Events + Enums

        #region Properties

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

        static private MersenneTwister Twist { get; } = new MersenneTwister((int)DateTime.Now.Ticks);

        #endregion Properties

        #region Constructors + Destructors

        public RollModel()
        {
        }

        #endregion Constructors + Destructors

        #region Methods

        public void Randomize()
        {
            DiceOne = Twist.Next(1, 7);
            DiceTwo = Twist.Next(1, 7);
            Selected = false;
        }

        public override string ToString()
        {
            return $"[Selected={Selected}][Roll={Roll}][One={DiceOne}][Two={DiceTwo}][Orientation={Orientation}]";
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion Methods
    }
}