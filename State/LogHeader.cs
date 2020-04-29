using Catan.Proxy;
using System.Text.Json.Serialization;

namespace Catan10
{

    public interface ILogHeader
    {
        int PlayerIndex { get; set; }
        string PlayerName { get; set; }
        GameState OldState { get; set; }
        GameState NewState { get; set; }
        CatanAction Action { get; set; }
    }



    public class LogHeader : ILogHeader
    {
        public int PlayerIndex { get; set; }
        public string PlayerName { get; set; }
        public GameState OldState { get; set; }
        public GameState NewState { get; set; }
        public CatanAction Action { get; set; }


        [JsonIgnore]
        public MainPage Page { get; internal set; } = null;
        [JsonIgnore]
        public PlayerModel Player { get; internal set; } = null;

        public LogHeader()
        {

        }


    }
}