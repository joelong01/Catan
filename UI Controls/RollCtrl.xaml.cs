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

    }

    public class RollModel : INotifyPropertyChanged
    {


        public event PropertyChangedEventHandler PropertyChanged;

        private int _redDie = -1;
        private int _whiteDie = -1;
        private TileOrientation _Orientation = TileOrientation.FaceDown;
        private bool _selected = false;
        private SpecialDice _specialDice = SpecialDice.None;



        private int _roll = -1;
        public int Roll
        {
            get
            {
                return _roll;
            }
            set
            {
                if (value != _roll)
                {
                    _roll = value;
                    NotifyPropertyChanged();
                }
            }
        }
      
        public SpecialDice SpecialDice
        {
            get
            {
                return _specialDice;
            }
            set
            {
                if (value != _specialDice)
                {
                    _specialDice = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public int RedDie
        {
            get
            {
                return _redDie;
            }
            set
            {
                if (value != _redDie)
                {
                    _redDie = value;
                    NotifyPropertyChanged();
                    NotifyPropertyChanged("Roll");
                }
            }
        }

        public int WhiteDie
        {
            get
            {
                return _whiteDie;
            }
            set
            {
                if (value != _whiteDie)
                {
                    _whiteDie = value;
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



        public RollModel()
        {
        }

   


        public void Randomize()
        {
            RedDie = Twist.Next(1, 7);
            WhiteDie = Twist.Next(1, 7);
            Selected = false;
        }

        public override string ToString()
        {
            return $"[Selected={Selected}][Roll={Roll}][Red={RedDie}][White={WhiteDie}][Special={SpecialDice}][Orientation={Orientation}]";
        }

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

    }
}