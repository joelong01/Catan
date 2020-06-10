using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.Json.Serialization;

namespace Catan10
{
    public class TileGroup
    {
        private readonly string[] SerializedProperties = new string[] { "Start", "End", "Randomize", "ResourceTypes", "NumberSequence", "HarborTypes", "RandomResourceTypeList", "RandomHarborTypeList", "TileCount" };

        private int _cols = 0;

        private int _rows = 0;

        public List<List<TileData>> _tilesInVisualOrder { get; private set; } = new List<List<TileData>>();

        public List<TileCtrl> AllTiles { get; set; } = new List<TileCtrl>();

        public List<TileData> AllTilesData { get; set; } = new List<TileData>();

        public int Cols
        {
            get
            {
                return _cols;
            }
            set
            {
                if (value > _cols) _cols = value;
            }
        }

        public int DesertCount { get; internal set; }

        public List<int> DesertTileIndices { get; set; } = new List<int>();

        public int End { get; set; }

        public List<TileCtrl> OriginalNonSeaTiles { get; set; } = new List<TileCtrl>();

        public List<TileData> OriginalNonSeaTilesData { get; set; } = new List<TileData>();

        public bool Randomize { get; set; }

        public int Rows
        {
            get
            {
                return _rows;
            }
            set
            {
                if (value > _rows) _rows = value;
            }
        }

        public int Start { get; set; }

        public List<ResourceType> StartingResourceTypes { get; set; } = new List<ResourceType>();

        public List<int> StartingTileNumbers { get; set; } = new List<int>();

        public RandomLists TileAndNumberLists { get; set; }

        //
        //  All the tiles in the Tilegroup -- including Sea Tiles that won't be randomized
        //
        //  the set of Tiles that particpate in Randomization and Shuffling
        public List<TileData> TileDataToRandomize { get; set; } = new List<TileData>();

        [JsonIgnore]
        public List<List<TileData>> TilesInVisualOrder
        {
            get
            {
                if (_tilesInVisualOrder.Count == 0)
                {
                    //
                    //  i don't know if the ordering is right and it is important
                    //  that it is, so I'll construct an array and then add them
                    //  to the list

                    TileData[,] tilesInvisualOrder = null;
                    tilesInvisualOrder = new TileData[Cols, Rows];
                    AllTilesData.ForEach((tile) =>
                   {
                       tilesInvisualOrder[tile.Col, tile.Row] = tile;
                   });

                    for (int col = 0; col < Cols; col++)
                    {
                        List<TileData> columns = new List<TileData>();
                        _tilesInVisualOrder.Add(columns);
                        for (int row = 0; row < Rows; row++)
                        {
                            TileData tileData = tilesInvisualOrder[col, row];
                            Contract.Assert(tileData != null);
                            columns.Add(tileData);
                        }
                    }
                }
                return _tilesInVisualOrder;
            }
        }

        public List<TileCtrl> TilesToRandomize { get; set; } = new List<TileCtrl>();

        public TileGroup()
        {
        }

        public TileGroup(string s)
        {
            string[] tokens = s.Split(new char[] { '-', '.' }, StringSplitOptions.RemoveEmptyEntries);
            Start = int.Parse(tokens[0]);
            End = int.Parse(tokens[1]);
            Randomize = bool.Parse(tokens[2]);
        }

        public static List<TileGroup> BuildList(string s)
        {
            List<TileGroup> list = new List<TileGroup>();
            string[] tokens = s.Split(new char[] { ',' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (string token in tokens)
            {
                TileGroup tg = new TileGroup(token);
                list.Add(tg);
            }

            return list;
        }

        public void Deserialize(string s, Dictionary<string, string> sections, int groupIndex)
        {
            //StaticHelpers.DeserializeObject<TileGroup>(this, s, "=", "|");
            //for (int i = 0; i < AllTiles.Count; i++)
            //{
            //    string serilizedTile = sections[$"Tile {i}.{groupIndex}"];
            //    TileCtrl tile = new TileCtrl();
            //    tile.Deserialize(serilizedTile, false);
            //    AllTiles.Add(tile);
            //    if (tile.RandomGoldEligible == false)
            //    {
            //        tile.TileOrientation = TileOrientation.FaceUp;
            //    }
            //    // Harbors.AddRange(tile.VisibleHarbors);
            //}
        }

        public void Reset()
        {
            for (int i = 0; i < TilesToRandomize.Count - 1; i++)
            {
                TilesToRandomize[i].ResourceType = StartingResourceTypes[i];
                TilesToRandomize[i].Number = StartingTileNumbers[i];
            }
        }

        public string Serialize(int groupIndex)
        {
            //string s = "";
            //string nl = StaticHelpers.lineSeperator;
            //s = StaticHelpers.SerializeObject<TileGroup>(this, SerializedProperties, "=", "|");
            //s += nl;
            //for (int i = 0; i < AllTiles.Count; i++)
            //{
            //    s += string.Format($"[Tile {i}.{groupIndex}]{nl}");
            //    s += AllTiles[i].Serialize(false);
            //    s += nl;
            //}

            //return s;
            return "oops";
        }

        public override string ToString()
        {
            return string.Format($"{Start}-{End}.{Randomize}");
        }
    }
}
