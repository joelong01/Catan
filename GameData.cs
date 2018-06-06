using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Catan10
{
    public interface IGameData
    {
        int[] NumberSequence { get; }
        ResourceType[] Resources { get; }
        HarborType[] HarborTypes { get; }
        int[] TileOrder { get; }
        HarborLocation[] HarborLocations { get; }
        int Columns { get; }
        int Rows { get; }
        GameType GameType { get; }
    }

    public class SavedGame : IGameData
    {
        public SavedGame(string savedGame)
        {

        }
        private string ParseAndLoadGame(string savedGame)
        {
            Dictionary<string, string> sections = null;
            string error = "";
            try
            {
                sections = StaticHelpers.GetSections(savedGame);
                if (sections == null)
                {
                    error = String.Format($"Error parsing the file into sections.\nThere are no sections.  Please load a valid .catangame file.");
                    return error;
                }
            }
            catch (Exception e)
            {
                return String.Format($"Error parsing the file into sections.\n{e.Message}");

            }
            int tileCount = 0;
            Dictionary<string, string> Game = StaticHelpers.DeserializeDictionary(sections["Game"]);

           

            try
            {

                Rows = Int32.Parse(GetValue(Game, "Rows"));
                Columns = Int32.Parse(GetValue(Game, "Columns"));
                GameName = GetValue(Game, "GameName");
               
                NumberOfPlayers = GetValue(Game, "NumberOfPlayers");                
                tileCount = Int32.Parse(GetValue(Game, "TileCount"));
            }
            catch (Exception ex)
            {
                return ex.Message;
            }
            finally
            {
                
            }

            //List<HarborLocation> harborLocations = new List<HarborLocation>();
            //List<HarborType> harbortypes = new List<HarborType>();
            //List<ResourceType> resourceTypes = new List<ResourceType>();
            //List<int> TileOrder = new List<int>();


            for (int i = 0; i < tileCount; i++)
            {
                
                
                string section = (string)sections["Tile " + i.ToString()];
                Dictionary<string, string> values = StaticHelpers.DeserializeDictionary(section);

                
                
            }

           
            return "";
        }

        private string GetValue(Dictionary<string, string> dict, string key)
        {

            string s = "";

            if (dict.TryGetValue(key, out s))
                return s;

            string error = String.Format($"Key {key} not found.");
            throw new Exception(error);

        }

        public string GameName {get;set;}
        public int Columns { get; set; } = 0;
        
        public GameType GameType { get; set; } = GameType.Saved;
        public string NumberOfPlayers { get; set; }


        public HarborLocation[] HarborLocations { get; set; }


        public HarborType[] HarborTypes { get; set; }
       

        public int[] NumberSequence { get; set; }
        

        public ResourceType[] Resources { get; set; }
       
        public int Rows { get; set; }
        

        public int[] TileOrder { get; set; }
        
    }

    public class RegularGameData : IGameData
    {
        int[] _numberSequenceRegularGame = { 5, 2, 6, 3, 8, 10, 9, 12, 11, 4, 8, 10, 9, 4, 5, 6, 3, 11 };
        ResourceType[] _resurcesRegularGame = { ResourceType.Brick, ResourceType.Desert, ResourceType.Wood, ResourceType.Wood, ResourceType.Wood,
                                      ResourceType.Sheep, ResourceType.Ore, ResourceType.Ore, ResourceType.Sheep, ResourceType.Sheep,
                                      ResourceType.Wheat, ResourceType.Brick, ResourceType.Brick, ResourceType.Wheat, ResourceType.Wood,
                                      ResourceType.Ore, ResourceType.Sheep, ResourceType.Wheat, ResourceType.Wheat };


        HarborType[] _harborTypesRegularGame = { HarborType.Wheat, HarborType.Sheep, HarborType.Ore, HarborType.Brick, HarborType.Wood, HarborType.ThreeForOne, HarborType.ThreeForOne, HarborType.ThreeForOne, HarborType.ThreeForOne };

        int[] _tileOrderRegularGame = { 11, 15, 18, 17, 16, 12, 7, 3, 0, 1, 2, 6, 10, 14, 13, 8, 4, 5, 9 };

        HarborLocation[] _harborLocations = {HarborLocation.TopLeft, HarborLocation.BottomLeft, HarborLocation.None, HarborLocation.Top, HarborLocation.None, HarborLocation.None, HarborLocation.BottomLeft, HarborLocation.None, HarborLocation.None,
                                            HarborLocation.None, HarborLocation.None, HarborLocation.Bottom, HarborLocation.Top, HarborLocation.None,  HarborLocation.None, HarborLocation.BottomRight, HarborLocation.TopRight, HarborLocation.None, HarborLocation.TopRight };



        public int[] NumberSequence
        {
            get
            {
                return _numberSequenceRegularGame;
            }

        }

        public ResourceType[] Resources
        {
            get
            {
                return _resurcesRegularGame;
            }

        }

        public HarborType[] HarborTypes
        {
            get
            {
                return _harborTypesRegularGame;
            }


        }

        public int[] TileOrder
        {
            get
            {
                return _tileOrderRegularGame;
            }

        }

        public HarborLocation[] HarborLocations
        {
            get
            {
                return _harborLocations;
            }


        }

        public int Columns
        {
            get
            {
                return 5;
            }
        }

        public int Rows
        {
            get
            {
                return 5;
            }
        }

        public GameType GameType
        {
            get
            {
                return GameType.Regular;
            }
        }
    }

    public class ExpansionGameData : IGameData
    {
        int[] _numberSequence = { 8, 11, 11, 10, 6, 3, 8, 4, 8, 2, 5, 4, 6, 3, 9, 5, 4, 9, 5, 9, 10, 11, 12, 10, 2, 6, 12, 3 };
        ResourceType[] _resources = { ResourceType.Wood,
                                      ResourceType.Ore,
                                      ResourceType.Brick,
                                      ResourceType.Brick,
                                      ResourceType.Brick,
                                      ResourceType.Brick,
                                      ResourceType.Wheat,
                                      ResourceType.Wood,
                                      ResourceType.Ore,
                                      ResourceType.Wheat,
                                      ResourceType.Wheat,
                                      ResourceType.Wheat,
                                      ResourceType.Wheat,
                                      ResourceType.Wheat,
                                      ResourceType.Sheep,
                                      ResourceType.Brick,
                                      ResourceType.Desert,
                                      ResourceType.Ore,
                                      ResourceType.Sheep,
                                      ResourceType.Sheep,
                                      ResourceType.Wood,
                                      ResourceType.Ore,
                                      ResourceType.Sheep,
                                      ResourceType.Sheep,
                                      ResourceType.Wood,
                                      ResourceType.Sheep,
                                      ResourceType.Desert,
                                      ResourceType.Wood,
                                      ResourceType.Ore,
                                      ResourceType.Wood  };

        HarborType[] _harborTypes = { HarborType.Sheep, HarborType.ThreeForOne, HarborType.Wood, HarborType.Brick,
                                      HarborType.Sheep, HarborType.ThreeForOne, HarborType.ThreeForOne, HarborType.ThreeForOne,
                                      HarborType.Wheat, HarborType.Ore, HarborType.ThreeForOne };

        int[] _tileOrderExpansionGame = { 17, 22, 26, 29, 28, 27, 23, 18, 12, 7, 3, 0, 1, 2, 6, 11, 16, 21, 25, 24, 19, 13, 8, 4, 5, 10, 15, 20, 14, 9 };

        HarborLocation[] _harborLocations = {
                                                HarborLocation.None,
                                                HarborLocation.TopLeft,
                                                HarborLocation.BottomLeft,
                                                HarborLocation.TopLeft,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.Bottom,
                                                HarborLocation.Top,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.TopRight,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.Bottom,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.None,
                                                HarborLocation.Bottom,
                                                HarborLocation.Top,
                                                HarborLocation.TopRight,
                                                HarborLocation.BottomRight
                                             };

        

        public int[] NumberSequence
        {
            get
            {
                return _numberSequence;
            }

        }

        public ResourceType[] Resources
        {
            get
            {
                return _resources;
            }

        }

        public HarborType[] HarborTypes
        {
            get
            {
                return _harborTypes;
            }


        }

        public int[] TileOrder
        {
            get
            {
                return _tileOrderExpansionGame;
            }

        }

        public HarborLocation[] HarborLocations
        {
            get
            {
                return _harborLocations;
            }


        }

        public int Columns
        {
            get
            {
                return 7;
            }
        }

        public int Rows
        {
            get
            {
                return 6;
            }
        }
        public GameType GameType
        {
            get
            {
                return GameType.SupplementalBuildPhase;
            }
        }
    }
}
