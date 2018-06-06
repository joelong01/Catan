using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.UI.Xaml.Shapes;

namespace Catan10
{
    //
    // this contains all the state for a Game object -- 
    //  game serialization and deserialization happens here

    public class OldCatanGame
    {
        public int MaxRoads { get; set; } = 15;
        public int MaxCities { get; set; } = 4;
        public int MaxShips { get; set; } = 0;
        public int MaxSettlements { get; set; } = 5;
        public bool DesignMode = false;
        public int Rows { get; set; }
        public int Columns { get; set; }
        public string RowsPerColumn { get; set; }
        public double  TileGap { get; set; }
        public string GameName { get; set; }
        public string NumberOfPlayers { get; set; }
        public bool UsesClassicTiles { get; set; } = true;
        public bool Randomize { get; set; } = true;
        public GameType GameType { get; set; } = GameType.Regular;
        public string TileGroupsAsString { get; set; }
        public List<ResourceType> ResourceTypes { get; internal set; }
        int _groupCount = 0;

        Dictionary<string, string> Sections { get; set; } = null;

        public List<TileGroup> TileGroups { get; set; } = new List<TileGroup>();
        public List<Harbor> Harbors { get; set; } = new List<Harbor>();

        List<TileCtrl> _tilesByHexOrder = new List<TileCtrl>();
        private Dictionary<RoadKey, Road> _roadKeyToRoad = new Dictionary<RoadKey, Road>(new RoadKeyComparer());
        private Dictionary<Polygon, Road> _roadSegmentToRoad = new Dictionary<Polygon, Road>();
        public Dictionary<RoadKey, Road> RoadKeyToRoadDictionary
        {
            get
            {

                return _roadKeyToRoad;
            }
        }

        public Dictionary<Polygon, Road> RoadSegmentToRoadDictionary
        {
            get
            {

                return _roadSegmentToRoad;
            }
        }
        public List<TileCtrl> TilesByHexOrder
        {
            get
            {
                if (_tilesByHexOrder.Count > 0) return _tilesByHexOrder;

                foreach (TileGroup tg in TileGroups)
                {
                    _tilesByHexOrder.AddRange(tg.Tiles);
                }

                _tilesByHexOrder.Sort(delegate (TileCtrl t1, TileCtrl t2)
                {
                    return t1.HexOrder - t2.HexOrder;
                });

                return _tilesByHexOrder;
            }
            set
            {
                _tilesByHexOrder = value;
            }
        }

        public override string ToString()
        {
            return String.Format($"{GameName}: {base.ToString()}");
        }

       

        public int GroupCount
        {
            get
            {
                if (Serializing)
                    return _groupCount;

                return TileGroups.Count;
            }
            set
            {
                if (!Serializing) return;

                _groupCount = value; // deserialize will call this 
            }
        }

        private string[] SerializedProperties = new string[] {"MaxRoads", "MaxSettlements", "MaxCities", "MaxShips", "Rows", "Columns", "GameName", "NumberOfPlayers", "UsesClassicTiles", "Randomize", "GameType", "TileGroupsAsString",
                                                                "ResourceTypes", "GroupCount", "TileGap", "RowsPerColumn"};

        public string Serialize()
        {
            Serializing = true;
            _groupCount = TileGroups.Count();
            string nl = StaticHelpers.lineSeperator;
            string s = "[View]" + nl;
            s += StaticHelpers.SerializeObject<OldCatanGame>(this, SerializedProperties, false);
            s += nl;
            for (int i = 0; i < GroupCount; i++)
            {
                s += String.Format($"[TileGroup {i}]{nl}");
                s += TileGroups[i].Serialize(i);
                s += nl;
            }
            Serializing = false;
            return s;
        }

        //
        //  this only deserializes the [View] section of the file, and in particular doesn't create any XAML objects..
        public bool FastDeserialize(string savedGame)
        {
            this.Serializing = true;


            try
            {
                this.Sections = StaticHelpers.GetSections(savedGame);
                if (this.Sections == null)
                {
                    this.Error = String.Format($"Error parsing the file into sections.\nThere are no sections.  Please load a valid .catangame file.");
                    return false;
                }
            }
            catch (Exception e)
            {
                this.Error = e.Message;
                return false;
            }

            StaticHelpers.DeserializeObject<OldCatanGame>(this, this.Sections["View"], false);
            this.Serializing = false;
            return true;
        }

        public bool LoadRestOfGame()
        {
            this.Serializing = true;
            for (int groupCount = 0; groupCount < this.GroupCount; groupCount++)
            {
                TileGroup tg = new TileGroup();
                string tgAsString = this.Sections[$"TileGroup {groupCount}"];
                tg.Deserialize(tgAsString, this.Sections, groupCount);
                this.TileGroups.Add(tg);
            }
            
            this.Serializing = false;
            return true;
        }

        static public OldCatanGame Deserialize(string savedGame)
        {

            OldCatanGame game = new OldCatanGame();
            game.Serializing = true;
            Dictionary<string, string> sections = null;
            try
            {
                sections = StaticHelpers.GetSections(savedGame);
                if (sections == null)
                {
                    game.Error = String.Format($"Error parsing the file into sections.\nThere are no sections.  Please load a valid .catangame file.");
                    return game;
                }
            }
            catch (Exception e)
            {
                game.Error = e.Message;
                return game;
            }

            StaticHelpers.DeserializeObject<OldCatanGame>(game, sections["View"], false);
            for (int groupCount = 0; groupCount < game.GroupCount; groupCount++)
            {
                TileGroup tg = new TileGroup();
                string tgAsString = sections[$"TileGroup {groupCount}"];
                tg.Deserialize(tgAsString, sections, groupCount);
                game.TileGroups.Add(tg);
            }
            game.Serializing = false;
            return game;
        }

        public string Error { get; internal set; }
        public bool Serializing { get; set; }
        

        List<List<TileCtrl>> _elementList = null;
        public List<List<TileCtrl>> VisualLayout()
        {
            if (_elementList != null)
                return _elementList;

            _elementList = new List<List<TileCtrl>>();
            int count = 0;

            if (this.Rows * this.Columns == 0)
                return _elementList;



            int middleCol = this.Columns / 2;
            int rowCountForCol = this.Rows - middleCol; // the number of rows in the first column
            int currentCol = middleCol;


            for (int col = 0; col < this.Columns; col++)
            {
                if (count == this.TilesByHexOrder.Count)
                    break;

                List<TileCtrl> innerList = new List<TileCtrl>();
                for (int row = 0; row < rowCountForCol; row++)
                {
                    if (count == this.TilesByHexOrder.Count)
                        break;


                    innerList.Add(this.TilesByHexOrder[count] as TileCtrl);

                    count++;
                }
                _elementList.Add(innerList);

                if (col < middleCol) // if you are less then the middle, move the column up, otherwise move it down
                {
                    rowCountForCol++;
                }
                else
                {
                    rowCountForCol--;

                }


            }
            return _elementList;


        }




    }

    public class RoadKeyComparer : IEqualityComparer<RoadKey>
    {
        public bool Equals(RoadKey x, RoadKey y)
        {
            if (x.Tile.Index == y.Tile.Index)
            {
                if (x.RoadLocation == y.RoadLocation)
                {
                    return true;
                }
            }
            return false;
        }

        public int GetHashCode(RoadKey obj)
        {
            return obj.Tile.GetHashCode() * 17 + obj.RoadLocation.GetHashCode();
        }
    }

    public class RoadKey 
    {
        public TileCtrl Tile { get; set; }
        public RoadLocation RoadLocation { get; set; }
        public RoadKey(TileCtrl tile, RoadLocation loc)
        {
            Tile = tile;
            RoadLocation = loc;
        }

        public override string ToString()
        {
            return String.Format($"{Tile} {Tile.Index} {RoadLocation}");
        }
    }

   
}
