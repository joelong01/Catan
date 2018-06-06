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
    public class HexPanel : Canvas
    {

        private int _rowCount = 0;
        private int _colCount = 0;
        private double _normalWidth = 0;
        private double _normalHeight = 0;


        public bool DisableLayout { get; set; } = false;
        List<List<TileCtrl>> _childList;

        public int Rows
        {
            get
            {
                return _rowCount;
            }

            set
            {
                _rowCount = value;
            }
        }

        public int Columns
        {
            get
            {
                return _colCount;
            }

            set
            {
                _colCount = value;
            }
        }

        public double NormalWidth
        {
            get
            {
                return _normalWidth;
            }

            set
            {
                _normalWidth = value;
            }
        }

        public double NormalHeight
        {
            get
            {
                return _normalHeight;
            }

            set
            {
                _normalHeight = value;
            }
        }

        public HexPanel()
        {

        }

        private List<List<TileCtrl>> BuildElementList(Size availableSize)
        {
            List<List<TileCtrl>> elementList = new List<List<TileCtrl>>();
            int count = 0;

            if (_rowCount * _colCount == 0)
                return elementList;

            int middleCol = _colCount / 2;
            int rowCountForCol = _rowCount - middleCol; // the number of rows in the first column
            int currentCol = middleCol;


            for (int col = 0; col < _colCount; col++)
            {
                if (count == Children.Count)
                    break;

                List<TileCtrl> innerList = new List<TileCtrl>();
                for (int row = 0; row < rowCountForCol; row++)
                {
                    if (count == Children.Count)
                        break;

                    Children[count].Measure(availableSize);
                    innerList.Add(Children[count] as TileCtrl);

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

            foreach (List<TileCtrl> childColumn in _childList)
            {
                Size size = ColumnSize(childColumn);
                if (size.Width > _normalWidth)
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
                        maxSize.Width += _normalWidth;
                    }
                }
                else
                {
                    maxSize.Width += _normalWidth;
                }

                if (size.Height > maxSize.Height)
                    maxSize.Height = size.Height;
            }

            maxSize.Width = maxSize.Width - (_childList.Count - 1) * _normalWidth * .25;

            return maxSize;
        }



        // Arrange the child elements to their final position
        protected override Size ArrangeOverride(Size finalSize)
        {
            return Update(finalSize);
        }
        internal Size Update2(Size finalSize)
        {
            if (Children.Count == 0)
                return finalSize;

            if (DisableLayout) return finalSize;

            

            Point center = new Point(finalSize.Width / 2.0, finalSize.Height / 2.0);




            //
            //  the middle column is assumed to be the tallest column --
            //
            //  if there is a harbor on the top, we have to move everything else down to line up
            //  if there is a harbor on the bottom, we have to move everything up to line up
            //  if there are harbors in both positions, they cancel each other out
            //

            int middleIndex = _childList.Count / 2;
            List<TileCtrl> middleColumn = _childList[middleIndex];
            double HexHeight = middleColumn[0].HexHeight;
            double HexWidth = middleColumn[0].HexWidth;




            double heightOffset = middleColumn.First().TopHarborHeight;
            heightOffset -= middleColumn.Last().BottomHarborHeight;



            double totalHeight = middleColumn.Count * HexHeight;
            totalHeight += middleColumn.First().TopHarborHeight; // 0 if collapsed
            totalHeight += middleColumn.Last().BottomHarborHeight;



            double top = center.Y - totalHeight * 0.5;
            double left = center.X - HexWidth * 0.5;

            foreach (TileCtrl tile in middleColumn)
            {
                tile.Arrange(new Rect(0, 0, tile.DesiredSize.Width, tile.DesiredSize.Height));
                Canvas.SetLeft(tile, left);
                Canvas.SetTop(tile, top);
                top += HexHeight;
            }

            top = 0;
            left = 0;

            for (int i = 0; i < _childList.Count; i++)
            {
                if (i == middleIndex) continue;

                List<TileCtrl> columns = _childList[i];
                if (columns == _childList.First())
                {

                    totalHeight = columns.Count * HexHeight;
                    totalHeight += columns.First().TopHarborHeight; // 0 if collapsed
                    totalHeight += columns.Last().BottomHarborHeight;
                    top = center.Y - ((totalHeight + heightOffset) * 0.5);
                }
                else
                {                    
                    top = Canvas.GetTop(_childList[i - 1].First()); // the top of the previous columnd
                    if (i<middleIndex)
                        top -= (HexHeight + columns.First().TopHarborHeight);                                            
                    else
                        top += (HexHeight + columns.First().TopHarborHeight);
                }
                

                foreach (TileCtrl tile in columns)
                {
                    double localleft = left;
                    tile.Arrange(new Rect(0, 0, tile.DesiredSize.Width, tile.DesiredSize.Height));
                    if (ColumnHasHasLeftHarbors(columns) && columns == _childList.First() && !tile.HasLeftHarbor)
                    {
                        localleft = left + 27;
                    }
                    Canvas.SetLeft(tile, localleft);
                    Canvas.SetTop(tile, top);
                    top += HexHeight;
                }

                if (ColumnHasHasLeftHarbors(columns) && columns == _childList.First())
                {
                    left += 27;
                }

                left += HexWidth * .75;

              
            }





            return finalSize;
        }

        public long TileXOffset(TileCtrl tile, int column)
        {
            List<TileCtrl> columns = _childList.ElementAt(column);
            if (ColumnHasHasLeftHarbors(columns)  && tile.HasLeftHarbor)
                return 27;

            return 0;
        }

        internal Size Update(Size finalSize)
        {
            if (Children.Count == 0)
                return finalSize;

            if (DisableLayout) return finalSize;

            Point center = new Point(finalSize.Width / 2.0, finalSize.Height / 2.0);
            double left = 0;



            //
            //  the middle column is assumed to be the tallest column --
            //
            //  if there is a harbor on the top, we have to move everything else down to line up
            //  if there is a harbor on the bottom, we have to move everything up to line up
            //  if there are harbors in both positions, they cancel each other out
            //
            List<TileCtrl> middleColumn = _childList[_childList.Count / 2];
            Size middleSize = ColumnSize(middleColumn);
            double heightOffset = middleColumn.First().TopHarborHeight;
            heightOffset -= middleColumn.Last().BottomHarborHeight;

            foreach (List<TileCtrl> childColumn in _childList)
            {


                Size columnSize = ColumnSize(childColumn);
                //
                //  if it is the middle column, we have to use the columnSize because it is the tallest column.
                //  if there is a "Bottom" harbor on any other column, we can ignore its size because it just hangs off into space.
                //  if there is a Top harbor, we have to move the Top up

                double normalColumnHeight = _normalHeight * childColumn.Count;
                double top = center.Y - normalColumnHeight * 0.5;



                //
                //  we have a harbor on top or bottom
                if (childColumn.First().HasTopHarbor && (childColumn != middleColumn))
                {
                    TileCtrl t = childColumn.First() as TileCtrl;                    
                    top -= t.TopHarborHeight; //extraheight is negative if the harbor is on the bottom
                }

                if (childColumn != middleColumn)
                {
                    top += heightOffset * .5;
                }

                //
                //  middle columns *center* (if the harbor is on the bottom, we don't center it, which is why this is different(
                if (childColumn == middleColumn)
                {
                    top = center.Y - columnSize.Height * 0.5;
                }
                bool columnHasLeftHarbor = ColumnHasHasLeftHarbors(childColumn);
                bool columnHasRightHarbor = ColumnHasHasRightHarbors(childColumn);

                foreach (TileCtrl tile in childColumn)
                {


                    tile.Arrange(new Rect(0, 0, tile.DesiredSize.Width, tile.DesiredSize.Height));

                    double tileLeft = left;
                    if (tile.DesiredSize.Width == _normalWidth && columnSize.Width != NormalWidth && childColumn == _childList.First())
                    {
                        // this means there is something sticking off to the left and the current tile is just a plain one
                        if (columnHasLeftHarbor)
                        {
                            if (!columnHasRightHarbor)
                            {
                                tileLeft += columnSize.Width - _normalWidth;
                            }
                            else
                            {
                                // both left and right harbors
                                tileLeft += (columnSize.Width - _normalWidth) * .5;
                            }
                        }

                        
                    }
                    else if (tile.DesiredSize.Width != _normalWidth && childColumn != _childList.First())
                    {
                        //  I need to know if the harbor is on the left or the right of the child

                        if (tile.HasLeftHarbor) // what happens if there is also a right harbor??
                        {
                            tileLeft = tileLeft - (tile.DesiredSize.Width - _normalWidth);
                        }
                    }
                    else if (childColumn == _childList.First() && tile.DesiredSize.Width > _normalWidth && columnHasLeftHarbor && tile != childColumn.First())
                    {
                        if (tile.HasRightHarbord && !tile.HasLeftHarbor)
                        {
                            tileLeft = tileLeft + (tile.DesiredSize.Width - _normalWidth);
                        }

                    }

                    // adjust Top for the current one if the Harbor is on the top
                    if (tile.IsHarborVisible(HarborLocation.Top))
                    {
                        if (tile != childColumn.First())
                        {
                            // we have to move it up so that the harbor goes into the tile above the current one
                            top -= (tile.DesiredSize.Height - _normalHeight);
                        }
                        


                    }

                    if (tile.IsHarborVisible(HarborLocation.Top) && tile.IsHarborVisible(HarborLocation.Bottom))
                    {
                        
                            //
                            //  put too high -- the bottom harbor hangs over
                            top += (tile.DesiredSize.Height - _normalHeight) * 0.5;
                        
                    }

                    if (tile == childColumn.First())
                    {
                        if (tile.IsHarborVisible(HarborLocation.TopLeft) && tile.IsHarborVisible(HarborLocation.TopRight))
                        {
                            tileLeft += (tile.DesiredSize.Width - _normalWidth) * .5;
                        }
                    }


                    //string s = ($"Layout: Index={tile.Index} Column={_childList.IndexOf(childColumn)} Row={childColumn.IndexOf(tile)} ");
                    //s += ($"HarborLocation: {tile.HarborLocation} DesiredSize={tile.DesiredSize} OldLeft:{ Canvas.GetLeft(tile)} OldTop:{Canvas.GetTop(tile)} ");
                    //s += $"NewLeft:{tileLeft} newTop:{top}";
                    //this.TraceMessage(s);




                    Canvas.SetTop(tile, top);
                    Canvas.SetLeft(tile, tileLeft);

                    //  
                    //  we are going down the columns, so increment top to be at the bottom of the current tile
                    if (tile.IsHarborVisible(HarborLocation.Top))
                    {
                        if (!tile.IsHarborVisible(HarborLocation.Bottom))
                        {
                            // if there is a Top harbor, the buttom is the full height
                            top += tile.Height;
                        }
                        else
                        {
                            top += NormalHeight + (tile.DesiredSize.Height - _normalHeight) * 0.5; ;
                        }
                    }
                    else
                    {
                        top += NormalHeight;
                    }


                }
                if (childColumn == _childList.First())
                {
                    if (columnHasLeftHarbor)
                    {
                        if (!columnHasRightHarbor)
                            left += (columnSize.Width - _normalWidth) + _normalWidth * 0.75; //the full width of the harbor + .75 of the Hex
                        else
                            left += (columnSize.Width - _normalWidth) * .5 + _normalWidth * 0.75; //the full width of ONE of the harbors + .75 of the Hex
                    }
                    else
                    {
                        left += _normalWidth * 0.75;
                    }
                }
                else
                {
                    left += _normalWidth * 0.75;
                }
            }

            return finalSize;
        }

        private bool ColumnHasHasLeftHarbors(List<TileCtrl> childColumn)
        {
            foreach (var child in childColumn)
            {
                TileCtrl tile = child as TileCtrl;
                if (tile.IsHarborVisible(HarborLocation.TopLeft) || tile.IsHarborVisible(HarborLocation.BottomLeft))
                    return true;
            }

            return false;
        }

        private bool ColumnHasHasRightHarbors(List<TileCtrl> childColumn)
        {
            foreach (var child in childColumn)
            {
                TileCtrl tile = child as TileCtrl;
                if (tile.IsHarborVisible(HarborLocation.TopRight) || tile.IsHarborVisible(HarborLocation.BottomRight))
                    return true;
            }

            return false;
        }

        //
        //  the top and the bottom only count for height if it is the first or last tile
        private Size ColumnSize(List<TileCtrl> list)
        {
            Size size = new Size();
            foreach (var tile in list)
            {
                size.Height += tile.DesiredSize.Height;
                if (size.Width < tile.DesiredSize.Width)
                {
                    size.Width = tile.DesiredSize.Width;
                }
            }

            return size;
        }
    }


}
