using System;
using System.ComponentModel;
using System.Runtime.CompilerServices;

using Catan.Proxy;

using Windows.UI.Xaml.Media;

namespace Catan10
{
    public class GridPosition : INotifyPropertyChanged
    {
        private double _scaleX = 1.0;
        private double _scaleY = 1.0;
        private double _translateX = 0.0;
        private double _translateY = 0.0;

        private void NotifyPropertyChanged([CallerMemberName] String propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        public double ScaleX
        {
            get
            {
                return _scaleX;
            }
            set
            {
                if (value != _scaleX)
                {
                    _scaleX = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double ScaleY
        {
            get
            {
                return _scaleY;
            }
            set
            {
                if (value != _scaleY)
                {
                    _scaleY = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double TranslateX
        {
            get
            {
                return _translateX;
            }
            set
            {
                value = (double)(int)value;

                if (value != _translateX)
                {
                    _translateX = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public double TranslateY
        {
            get
            {
                return _translateY;
            }
            set
            {
                value = (double)(int)value;
                if (value != _translateY)
                {
                    _translateY = value;
                    NotifyPropertyChanged();
                }
            }
        }

        public GridPosition(double X, double Y, double scaleX = 1.0, double scaleY = 1.0)
        {
            TranslateX = X;
            TranslateY = Y;
            ScaleX = scaleX;
            ScaleY = scaleY;
        }

        public GridPosition()
        {
        }

        public GridPosition(CompositeTransform ct)
        {
            ScaleX = ct.ScaleX;
            ScaleY = ct.ScaleY;
            TranslateX = ct.TranslateX;
            TranslateY = ct.TranslateY;
        }

        public event PropertyChangedEventHandler PropertyChanged;

        new public virtual bool Equals(Object obj)
        {
            if (obj.GetType() != typeof(GridPosition)) return false;
            var gp = obj as GridPosition;
            if (gp.TranslateX == this.TranslateX &&
                gp.TranslateY == this.TranslateY &&
                gp.ScaleX == this.ScaleX &&
                gp.ScaleY == this.ScaleY)
            {
                return true;
            }
            return false;
        }

        public string Serialize()
        {
            return CatanProxy.Serialize(this);
        }

        public override string ToString()
        {
            return Serialize();
        }
    }
}
