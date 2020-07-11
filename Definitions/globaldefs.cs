using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Text.Json;
using System.Text.Json.Serialization;

using Catan.Proxy;
using Catan10.CatanService;

namespace Catan10
{
    public class LogHeader
    {
        #region Properties + Fields

        public CatanAction Action { get; set; }

        public bool CanUndo { get; set; } = true;

        public CatanGames CatanGame { get; set; } = MainPage.Current.GameContainer.CurrentGame.CatanGame;

        [JsonIgnore]
        public bool LocallyCreated
        {
            get
            {
                Contract.Assert(SentBy != null);
                Contract.Assert(MainPage.Current.TheHuman != null);
                return (MainPage.Current.TheHuman == SentBy);
            }
        }

        public Guid LogId { get; set; } = Guid.NewGuid();

        public LogType LogType { get; set; } = LogType.Normal;

        public GameState NewState { get; set; } = MainPage.Current.CurrentGameState;

        public GameState OldState { get; set; } = (MainPage.Current.MainPageModel.Log.PeekAction == null) ? GameState.WaitingForNewGame : MainPage.Current.MainPageModel.Log.PeekAction.NewState;

        [JsonIgnore]
        public LogHeader Previous { get; set; } = MainPage.Current.MainPageModel.Log.PeekAction;

        public PlayerModel SentBy { get; set; } = MainPage.Current.TheHuman;

        public DateTime CreatedTime { get; set; } = DateTime.Now;
        

        public string TypeName { get; set; }

        #endregion Properties + Fields



        #region Constructors

        public LogHeader()
        {
            TypeName = this.GetType().FullName;
        }

        #endregion Constructors

        #region Methods

        // for debugging convinience
        // if state changes, you have to set this
        public static LogHeader Deserialize(JsonElement element)
        {
            var typeName = element.GetProperty("typeName").GetString();
            Contract.Assert(!String.IsNullOrEmpty(typeName));
            Type type = Type.GetType(typeName);
            return JsonSerializer.Deserialize(element.ToString(), type, CatanSignalRClient.GetJsonOptions()) as LogHeader;
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
            return CatanSignalRClient.Serialize<object>(this);
        }

        public override string ToString()
        {
            return $"[Type={TypeName}][Action={Action}][SentBy={SentBy}][OldState={OldState}][NewState={NewState}]";
        }

        #endregion Methods
    }

    public class RandomBoardSettings
    {
        #region Properties + Fields

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

        #endregion Properties + Fields



        #region Constructors

        public RandomBoardSettings()
        {
        }

        public RandomBoardSettings(Dictionary<string, RandomLists> Tiles, List<int> Harbors)
        {
            RandomHarborTypeList = Harbors;
            TileGroupToRandomListsDictionary = Tiles;
        }

        #endregion Constructors

        #region Methods

        public static RandomBoardSettings Deserialize(string saved)
        {
            return CatanSignalRClient.Deserialize<RandomBoardSettings>(saved);
        }

        public string Serialize()
        {
            return CatanSignalRClient.Serialize<RandomBoardSettings>(this);
        }

        public override string ToString()
        {
            return Serialize();
        }

        #endregion Methods
    }

    public class RandomLists
    {
        #region Properties + Fields

        public List<int> NumberList { get; set; } = null;

        public List<int> TileList { get; set; } = null;

        #endregion Properties + Fields



        #region Constructors

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

        #endregion Constructors

        #region Methods

        public static RandomLists Deserialize(string saved)
        {
            return CatanSignalRClient.Deserialize<RandomLists>(saved);
        }

        public string Serialize()
        {
            return CatanSignalRClient.Serialize<RandomLists>(this);
        }

        #endregion Methods
    }
}