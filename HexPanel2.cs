using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace Catan10
{
    public class HexPanel2 : Canvas
    {

        

        public bool DisableLayout { get; set; } = false;
        List<List<UIElement>> _childList;

        public int Rows { get; set; }
        
        public int Columns { get; set; }    
        public double NormalWidth { get; set; }
        public double NormalHeight { get; set; }
        int _tileCount = 0;
        public int TileCount
        {
            get
            {
                return _tileCount;
            }
            set
            {
                _tileCount = value;
                AddChildren();
            }
        } 
        

        public HexPanel2()
        {
            
           
        }

        private void AddChildren()
        {
            if (!StaticHelpers.IsInVisualStudioDesignMode)
            {
                this.Children.Clear();
                for (int i = 0; i < _tileCount; i++)
                {
                    TileCtrl tile = new TileCtrl();
                    tile.Number = 0;
                    tile.ResourceType = ResourceType.Sea;
                    tile.HarborLocation = HarborLocation.None;
                    tile.SetTileOrientationAsync(TileOrientation.FaceUp, Double.MaxValue, 0);
                    tile.ShowIndex = false;

                    this.Children.Add(tile);
                }
            }
        }

        private List<List<UIElement>> BuildElementList(Size availableSize)
        {
            List<List<UIElement>> elementList = new List<List<UIElement>>();
            int count = 0;

            if (Rows * Columns == 0)
                return elementList;

            int middleCol = Columns / 2;
            int rowCountForCol = Rows - middleCol; // the number of rows in the first column
            int currentCol = middleCol;


            for (int col = 0; col < Columns; col++)
            {
                if (count == Children.Count)
                    break;

                List<UIElement> innerList = new List<UIElement>();
                for (int row = 0; row < rowCountForCol; row++)
                {
                    if (count == Children.Count)
                        break;

                    Children[count].Measure(availableSize);
                    innerList.Add(Children[count]);

                    count++;
                }
                elementList.Add(innerList);

                if (col < middleCol) // if you are less then the middle, move the column up, otherwise move it down
                {
                    rowCountForCol++;
                }
                else
                {
                    rowCountForCol--;

                }


            }
            return elementList;

        }

        /// <summary>
        ///     We have to add Width if there is a harbor, but if there are two harbors in the same column, we should add the width only once
        /// </summary>
        /// <param name="availableSize"></param>
        /// <returns></returns>
        protected override Size MeasureOverride(Size availableSize)
        {
            if (DisableLayout) return availableSize;

            Size maxSize = new Size();


            _childList = BuildElementList(availableSize);

            foreach (List<UIElement> childColumn in _childList)
            {
                Size size = ColumnSize(childColumn);
                if (size.Width > NormalWidth)
                {
                    if (childColumn == _childList.First())
                    {
                        maxSize.Width += size.Width;
                    }
                    else if (childColumn == _childList.Last())
                    {
                        maxSize.Width += size.Width;
                    }
                    else
                    {
                        maxSize.Width += NormalWidth;
                    }
                }
                else
                {
                    maxSize.Width += NormalWidth;
                }

                if (size.Height > maxSize.Height)
                    maxSize.Height = size.Height;
            }

            maxSize.Width = maxSize.Width - (_childList.Count - 1) * NormalWidth * .25;

            return maxSize;
        }



        // Arrange the child elements to their final position
        protected override Size ArrangeOverride(Size finalSize)
        {
            if (Children.Count == 0)
                return finalSize;

            if (DisableLayout) return finalSize;

            double height = (finalSize.Height + NormalHeight) / (double)Rows;
            double width = height * (NormalWidth / NormalHeight);
            
            double left = -width * .25;
            double top = -height * .5;

            int total = 0;

            for (int col = 0; col < Columns; col++)
            {
                for (int row = 0; row < Rows; row++)
                {
                    UIElement tile = Children[total];
                    tile.Arrange(new Rect(0, 0, tile.DesiredSize.Width, tile.DesiredSize.Height));
                    total++;
                    if (total == Children.Count)
                        return finalSize;

                    Canvas.SetLeft(tile, left);
                    Canvas.SetTop(tile, top);
                    top += NormalHeight;
                }
                left += width * 0.75;
                if (col % 2 == 0)
                    top = 0;
                else
                    top = -height * 0.5;
            }

            return finalSize;
        }

        private bool ColumnHasHasLeftHarbors(List<UIElement> childColumn)
        {
            foreach (var child in childColumn)
            {
                TileCtrl tile = child as TileCtrl;
                if (tile.HarborLocation == HarborLocation.TopLeft || tile.HarborLocation == HarborLocation.BottomLeft)
                    return true;
            }

            return false;
        }

        private bool ColumnHasHasRightHarbors(List<UIElement> childColumn)
        {
            foreach (var child in childColumn)
            {
                TileCtrl tile = child as TileCtrl;
                if (tile.HarborLocation == HarborLocation.TopRight || tile.HarborLocation == HarborLocation.BottomRight)
                    return true;
            }

            return false;
        }

        private Size ColumnSize(List<UIElement> list)
        {
            Size size = new Size();
            foreach (var v in list)
            {
                size.Height += v.DesiredSize.Height;
                if (size.Width < v.DesiredSize.Width)
                {
                    size.Width = v.DesiredSize.Width;
                }
            }

            return size;
        }
    }


}
