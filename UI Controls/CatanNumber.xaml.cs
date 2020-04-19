using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using Windows.UI;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Shapes;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace Catan10
{
    public enum NumberColorTheme { Light, Dark };
    public enum NumberStyle { Default, ResoureCount };
    public sealed partial class CatanNumber : UserControl, INotifyPropertyChanged
    {
        public int Probability { get; set; } = 0; // the number (divided by 36) that represents the probability of this number being rolled

        readonly SolidColorBrush _redBrush = StaticHelpers.GetResourceBrush("Red");
        readonly SolidColorBrush _blackBrush = StaticHelpers.GetResourceBrush("Black");
        readonly SolidColorBrush _whiteBrush = StaticHelpers.GetResourceBrush("White");
        NumberColorTheme myTheme = NumberColorTheme.Dark;
        public NumberStyle NumberStyle { get; set; } = NumberStyle.Default;
        bool _showEyes = false;
        public NumberColorTheme Theme
        {
            get => myTheme;
            set { myTheme = value; NotifyPropertyChanged(); }
        }
        public CatanNumber()
        {
            this.InitializeComponent();
        }

        public bool ShowEyes
        {
            get => _showEyes;
            set
            {
                _showEyes = value;
                _rectLeftEye.Visibility = Visibility.Collapsed;
                _rectRightEye.Visibility = Visibility.Collapsed;
                if ((Number == 3 || Number == 11) && _showEyes)
                {
                    _rectLeftEye.Visibility = Visibility.Visible;
                    _rectRightEye.Visibility = Visibility.Visible;
                }

            }
        }

        public bool HideSeven { get; set; } = true;

        public int Number
        {

            get => Convert.ToInt32(_txtNumber.Text);
            set
            {



                bool notifyChange = _txtNumber.Text != value.ToString();
                try
                {



                    _txtNumber.Text = value.ToString();

                    foreach (UIElement el in _oddGrid.Children)
                    {
                        el.Visibility = Visibility.Collapsed;
                        if (((FrameworkElement)el).Name == _txtNumber.Name)
                        {
                            this.TraceMessage("Found your damn bug!");
                        }
                    }

                    foreach (UIElement el in _evenGrid.Children)
                    {
                        el.Visibility = Visibility.Collapsed;
                        if (((FrameworkElement)el).Name == _txtNumber.Name)
                        {
                            this.TraceMessage("Found your damn bug!");
                        }
                    }

                    if (NumberStyle == NumberStyle.ResoureCount)
                    {
                        this.Visibility = Visibility.Visible;
                        // just take the number.  all the propability circles are hidden
                        return;
                    }


                    if (value == 6 || value == 8)
                    {
                        Probability = 5;
                        UseOddGrid(true);               // this might look funny since 8 and 6 aren't odd -- but the odds of getting them (5) is odd!
                        _txtNumber.Foreground = _redBrush;
                        SetEllipseColor(_redBrush);

                        foreach (UIElement el in _oddGrid.Children)
                        {
                            el.Visibility = Visibility.Visible;
                        }
                        return;
                    }

                    if (Theme == NumberColorTheme.Dark)
                    {
                        SetEllipseColor(_blackBrush);
                        _txtNumber.Foreground = _blackBrush;
                    }
                    else
                    {
                        SetEllipseColor(_whiteBrush);
                        _txtNumber.Foreground = _whiteBrush;
                    }

                    if (value == 2 || value == 12)
                    {
                        Probability = 1;
                        UseOddGrid(true);
                        _oddGrid.Children[2].Visibility = Visibility.Visible;

                    }

                    if (value == 3 || value == 11)
                    {
                        Probability = 2;
                        UseOddGrid(false);
                        _evenGrid.Children[1].Visibility = Visibility.Visible;
                        _evenGrid.Children[2].Visibility = Visibility.Visible;

                        if (_showEyes)
                        {
                            _rectLeftEye.Visibility = Visibility.Visible;
                            _rectRightEye.Visibility = Visibility.Visible;
                        }

                    }

                    if (value == 4 || value == 10)
                    {
                        Probability = 3;
                        UseOddGrid(true);
                        _oddGrid.Children[1].Visibility = Visibility.Visible;
                        _oddGrid.Children[2].Visibility = Visibility.Visible;
                        _oddGrid.Children[3].Visibility = Visibility.Visible;

                    }

                    if (value == 5 || value == 9)
                    {
                        Probability = 4;
                        UseOddGrid(false);
                        _evenGrid.Children[0].Visibility = Visibility.Visible;
                        _evenGrid.Children[1].Visibility = Visibility.Visible;
                        _evenGrid.Children[2].Visibility = Visibility.Visible;
                        _evenGrid.Children[3].Visibility = Visibility.Visible;

                    }

                    if (value == 0)
                    {
                        this.Visibility = Visibility.Collapsed;
                    }
                    if (value == 7 && HideSeven)
                    {
                        Probability = 6;
                        this.Visibility = Visibility.Collapsed;
                    }
                }
                catch (Exception)
                {

                }

                finally
                {
                    if (notifyChange)
                    {
                        NotifyPropertyChanged();
                    }
                }



            }

        }

        private void UseOddGrid(bool useOdd)
        {
            if (useOdd)
            {
                _oddGrid.Visibility = Visibility.Visible;
                _evenGrid.Visibility = Visibility.Collapsed;
            }
            else
            {
                _oddGrid.Visibility = Visibility.Collapsed;
                _evenGrid.Visibility = Visibility.Visible;
            }
        }

        private void SetEllipseColor(SolidColorBrush brush)
        {
            foreach (UIElement el in _oddGrid.Children)
            {
                if (el.GetType() == typeof(Ellipse))
                {
                    ((Ellipse)el).Fill = brush;
                }
            }

            foreach (UIElement el in _evenGrid.Children)
            {
                if (el.GetType() == typeof(Ellipse))
                {
                    ((Ellipse)el).Fill = brush;
                }
            }
        }
        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public event PropertyChangedEventHandler PropertyChanged;
    }
}
