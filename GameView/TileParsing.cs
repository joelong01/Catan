using System.Collections.Generic;
using System.Linq;

using Windows.UI.Xaml.Controls;

namespace Catan10
{
    public partial class CatanHexPanel : Canvas
    {
        #region Methods

        private TileCtrl AboveTile(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = this.VisualTiles;
            if (row == 0) return null;

            return visualTiles.ElementAt(col).ElementAt(row - 1);
        }

        private TileCtrl BelowTile(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = this.VisualTiles;
            if (row >= visualTiles.ElementAt(col).Count - 1) return null;

            return visualTiles.ElementAt(col).ElementAt(row + 1);
        }

        private TileCtrl NextLowerRight(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = this.VisualTiles;

            if (col == visualTiles.Count - 1) return null;

            bool beforeMiddle = (col < (int)(visualTiles.Count / 2));
            if (beforeMiddle)
            {
                return visualTiles.ElementAt(col + 1).ElementAt(row + 1);
            }
            if (row > visualTiles.ElementAt(col + 1).Count - 1) return null;

            return visualTiles.ElementAt(col + 1).ElementAt(row);
        }

        private TileCtrl NextUpperRight(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = this.VisualTiles;
            if (col == visualTiles.Count - 1) return null; // last column has no Next column
            bool beforeMiddle = (col < (int)(visualTiles.Count / 2));
            if (beforeMiddle)
            {
                return visualTiles.ElementAt(col + 1).ElementAt(row);
            }

            if (row == 0) return null;

            // we are at or past the middle
            return visualTiles.ElementAt(col + 1).ElementAt(row - 1);   // row + 1 is always valid after the middle
        }

        private TileCtrl PreviousLowerLeft(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = this.VisualTiles;
            if (col == 0) return null;

            bool beforeMiddle = (col < (int)(visualTiles.Count / 2));
            bool atMiddle;

            if (visualTiles.Count % 2 == 1)
                atMiddle = (col == ((int)(visualTiles.Count / 2)));
            else
                atMiddle = (col == ((int)(visualTiles.Count / 2) + 1));

            if (beforeMiddle || atMiddle)
            {
                if (row == visualTiles.ElementAt(col).Count - 1) return null;  // if it is the last tile before or at the middle, there is no lower left

                return visualTiles.ElementAt(col - 1).ElementAt(row);
            }

            // we are after the middle
            return visualTiles.ElementAt(col - 1).ElementAt(row + 1);   // row + 1 is always valid after the middle
        }

        private TileCtrl PreviousUpperLeft(int row, int col)
        {
            List<List<TileCtrl>> visualTiles = this.VisualTiles;
            if (col == 0) return null;

            bool beforeMiddle = (col < (int)(visualTiles.Count / 2));
            bool atMiddle;

            if (visualTiles.Count % 2 == 1)
                atMiddle = (col == ((int)(visualTiles.Count / 2)));
            else
                atMiddle = (col == ((int)(visualTiles.Count / 2) + 1));

            if (beforeMiddle || atMiddle)
            {
                if (row == 0) return null;

                return visualTiles.ElementAt(col - 1).ElementAt(row - 1);
            }

            // we are after the middle
            return visualTiles.ElementAt(col - 1).ElementAt(row);
        }

        public TileCtrl GetAdjacentTile(TileCtrl tile, TileLocation adjacentLocation)
        {
            return GetAdjacentTile(tile.Row, tile.Col, adjacentLocation);
        }

        public TileCtrl GetAdjacentTile(int row, int col, TileLocation adjacentLocation)
        {
            switch (adjacentLocation)
            {
                case TileLocation.TopRight:
                    return NextUpperRight(row, col);

                case TileLocation.TopLeft:
                    return PreviousUpperLeft(row, col);

                case TileLocation.BottomRight:
                    return NextLowerRight(row, col);

                case TileLocation.BottomLeft:
                    return PreviousLowerLeft(row, col);

                case TileLocation.Top:
                    return AboveTile(row, col);

                case TileLocation.Bottom:
                    return BelowTile(row, col);

                default:
                    break;
            }

            return null;
        }

        #endregion Methods
    }
}
