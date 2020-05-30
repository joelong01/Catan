using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics.Contracts;
using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using Catan.Proxy;

namespace Catan10
{
    public class CatanGameInfo
    {
        public CatanGameInfo()
        {
        }

        public CatanGameInfo(CatanGameInfo info)
        {
            MaxRoads = info.MaxRoads;
            MaxCities = info.MaxCities;
            MaxSettlements = info.MaxSettlements;
            MaxResourceAllocated = info.MaxResourceAllocated;
            AllowShips = info.AllowShips;
            Knight = info.Knight;
            VictoryPoint = info.Knight;
            YearOfPlenty = info.VictoryPoint;
            RoadBuilding = info.YearOfPlenty;
            Monopoly = info.Monopoly;
        }

        public bool AllowShips { get; set; } = false;
        public RandomBoardSettings BoardSettings { get; set; } = new RandomBoardSettings();
        public CatanGames GameName { get; set; } = CatanGames.Regular;
        public GameType GameType { get; set; } = GameType.Regular;
        public int HarborCount { get; set; } = 9;
        public int Knight { get; set; } = 14;
        public int MaxCities { get; set; } = 4;
        public int MaxResourceAllocated { get; set; } = 19;
        public int MaxRoads { get; set; } = 15;
        public int MaxSettlements { get; set; } = 5;
        public int Monopoly { get; set; } = 2;

        public int RoadBuilding { get; set; } = 2;

        // for "Regular Game"
        public int TileCount { get; set; } = 19;

        // most aggregate resource per type
        public int VictoryPoint { get; set; } = 5;

        public int YearOfPlenty { get; set; } = 2;

        public static bool operator !=(CatanGameInfo a, CatanGameInfo b)
        {
            return !(a == b);
        }

        // for "Regular Game"
        public static bool operator ==(CatanGameInfo a, CatanGameInfo b)
        {
            if (a is null)
            {
                if (b is null)
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }

            if (b is null) // a is not null!
            {
                return false;
            }

            return
                (
                    a.MaxRoads == b.MaxRoads &&
                    a.MaxCities == b.MaxCities &&
                    a.MaxSettlements == b.MaxSettlements &&
                    a.Knight == b.Knight &&
                    a.MaxResourceAllocated == b.MaxResourceAllocated &&
                    a.AllowShips == b.AllowShips &&
                    a.VictoryPoint == b.VictoryPoint &&
                    a.YearOfPlenty == b.YearOfPlenty &&
                    a.RoadBuilding == b.RoadBuilding &&
                    a.Monopoly == b.Monopoly
                );
        }

        public override bool Equals(object obj)
        {
            return (CatanGameInfo)obj == this;
        }

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }
    }

    public class LogHeader
    {
        public LogHeader()
        {
            TypeName = this.GetType().FullName;
        }
        public GameState NewState { get; set; } = MainPage.Current.CurrentGameState;
        public GameState OldState { get; set; } = (MainPage.Current.MainPageModel.Log.PeekAction == null) ? GameState.WaitingForNewGame : MainPage.Current.MainPageModel.Log.PeekAction.NewState;

        public CatanAction Action { get; set; }
        public string SentBy { get; set; } = MainPage.Current.TheHuman?.PlayerName;
        public DateTime Time { get; set; } = DateTime.Now;
        public bool CanUndo { get; set; } = true;
        public CatanGames CatanGame { get; set; } = MainPage.Current.GameContainer.CurrentGame.CatanGame;

        [JsonIgnore]
        public LogHeader Previous { get; set; } = MainPage.Current.MainPageModel.Log.PeekAction; // for debugging convinience

        [JsonIgnore]
        public bool LocallyCreated
        {
            get
            {
                Contract.Assert(String.IsNullOrEmpty(SentBy) == false);
                Contract.Assert(MainPage.Current.TheHuman != null);
                return (MainPage.Current.TheHuman.PlayerName == SentBy);
            }
        }

        public Guid LogId { get; set; } = Guid.NewGuid();
        public LogType LogType { get; set; } = LogType.Normal;
       
        

        // if state changes, you have to set this
        

        public string TypeName { get; set; }

        public static LogHeader Deserialize(JsonElement element)
        {
            var typeName = element.GetProperty("typeName").GetString();
            Contract.Assert(!String.IsNullOrEmpty(typeName));
            Type type = Type.GetType(typeName);
            return JsonSerializer.Deserialize(element.ToString(), type, CatanProxy.GetJsonOptions()) as LogHeader;
        }

        /// <summary>
        ///     we have a unique GUID for each
        /// </summary>
        /// <returns></returns>
        public override int GetHashCode()
        {
            return LogId.GetHashCode();
        }

        public string Serialize()
        {
            return CatanProxy.Serialize<object>(this);
        }

        public override string ToString()
        {
            return $"[Type={TypeName}][Action={Action}][SentBy={SentBy}][OldState={OldState}][NewState={NewState}]";
        }
    }

    

    public class RandomBoardSettings
    {
        public RandomBoardSettings()
        {
        }

        public RandomBoardSettings(Dictionary<string, RandomLists> Tiles, List<int> Harbors)
        {
            RandomHarborTypeList = Harbors;
            TileGroupToRandomListsDictionary = Tiles;
        }

        //
        //  every Board has a random list of harbors
        public List<int> RandomHarborTypeList { get; set; } = null;

        //
        // every TileGroup has a list that says where to put the tiles
        // and another list that says what number to put on the tiles
        //
        // the int here is the TileGroup Index - System.Text.Json currently only Deserializes Dictionaries keyed by strings.
        //
        public Dictionary<string, RandomLists> TileGroupToRandomListsDictionary { get; set; } = new Dictionary<string, RandomLists>();

        public static RandomBoardSettings Deserialize(string saved)
        {
            return CatanProxy.Deserialize<RandomBoardSettings>(saved);
        }

        public string Serialize()
        {
            return CatanProxy.Serialize<RandomBoardSettings>(this);
        }

        public override string ToString()
        {
            return Serialize();
        }
    }

    public class RandomLists
    {
        public RandomLists()
        {
        }

        public RandomLists(string saved)
        {
            Deserialize(saved);
        }

        public RandomLists(List<int> tiles, List<int> numbers)
        {
            TileList = tiles;
            NumberList = numbers;
        }

        public List<int> NumberList { get; set; } = null;
        public List<int> TileList { get; set; } = null;

        public static RandomLists Deserialize(string saved)
        {
            return CatanProxy.Deserialize<RandomLists>(saved);
        }

        public string Serialize()
        {
            return CatanProxy.Serialize<RandomLists>(this);
        }
    }

 
}