using Catan.Proxy;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Catan10
{


    /// <summary>
    ///     This class has all the data associated with changing a player.
    /// </summary>
    public class ChangedPlayerModel : LogHeader
    {
        public int From { get; set; } = -1;
        public int To { get; set; } = -1;
        public List<int> OldRandomGoldTiles { get; set; } = new List<int>();
        public List<int> NewRandomGoldTiles { get; set; } = new List<int>();
        public List<int> HighlightedTiles { get; set; } = new List<int>();
        public GameState OldGameState { get; set; }
        public ChangedPlayerModel() { }
        public ChangedPlayerModel(int oldIdx, int newIdx, GameState oldState, List<int> rgtOld, List<int> rgtNew, List<int> highlitedTiles)
        {
            From = oldIdx;
            To = newIdx;
            OldGameState = oldState; // new game state kept in LogEntry
            OldRandomGoldTiles = rgtOld;
            NewRandomGoldTiles = rgtNew;
            HighlightedTiles = highlitedTiles;
        }

        internal bool IsEqual(ChangedPlayerModel cpm)
        {
            if (cpm.From != this.From || cpm.To != this.To || cpm.OldGameState != this.OldGameState)
            {
                return false;
            }

            if (!ListsEqual(OldRandomGoldTiles, cpm.OldRandomGoldTiles))
                return false;

            if (!ListsEqual(NewRandomGoldTiles, cpm.NewRandomGoldTiles))
                return false;

            if (!ListsEqual(HighlightedTiles, cpm.HighlightedTiles))
                return false;

            return true;
        }

        private bool ListsEqual(List<int> l1, List<int> l2)
        {
            if (l1.Count != l2.Count)
                return false;

            for (int i = 0; i < l1.Count; i++)
            {
                if (l1[i] != l2[i])
                {
                    return false;
                }
            }

            return true;
        }
    }



    public class ChangedPlayerController
    {

        public static async Task<ChangedPlayerModel> ChangePlayer(MainPage page, int numberofPositions, GameState newState)
        {
            int from = page.MainPageModel.PlayingPlayers.IndexOf(page.CurrentPlayer);
            int to = page.GetNextPlayerPosition(numberofPositions);
            GameState oldState = page.NewGameState;



            ChangedPlayerModel model = new ChangedPlayerModel
            {
                Page = page,
                Player = page.CurrentPlayer,
                PlayerIndex = page.CurrentPlayer.AllPlayerIndex,
                PlayerName = page.CurrentPlayer.PlayerName,
                OldState = oldState,
                NewState = newState,
                Action = CatanAction.ChangePlayerAndSetState,
                From = from,
                To = to,
                OldRandomGoldTiles = page.GameContainer.GetCurrentRandomGoldTiles(),
                //  NewRandomGoldTiles filled in below
                //  Highlighted Tiles filled in below

            };



            // this controller is the one spot where the CurrentPlayer is changed.  it should update all the bindings
            // the setter will update all the associated state changes that happen when the CurrentPlayer
            // changes

            page.CurrentPlayer = page.MainPageModel.PlayingPlayers[to];

            //
            // always stop highlighted the roll when the player changes
            foreach (var tile in page.GameContainer.AllTiles)
            {
                if (tile.Highlighted)
                {
                    model.HighlightedTiles.Add(tile.Index);
                    tile.StopHighlightingTile();
                }

            }

            //
            //  in supplemental, we don't show random gold tiles
            if (newState == GameState.Supplemental)
            {
                await page.GameContainer.ResetRandomGoldTiles();
            }


            //
            // when we change player we optionally set tiles to be randomly gold - iff we are moving forward (not undo)
            // we need to check to make sure that we haven't already picked random goal tiles for this particular role.  the scenario is
            // we hit Next and are waiting for a role (and have thus picked random gold tiles) and then hit undo for some reason so that the
            // previous player can finish their turn.  when we hit Next again, we want the same tiles to be chosen to be gold.
            if ((newState == GameState.WaitingForRoll) || (newState == GameState.WaitingForNext))
            {
                int playerRoll = page.TotalRolls / page.MainPageModel.PlayingPlayers.Count;  // integer divide - drops remainder
                if (playerRoll == page.CurrentPlayer.GameData.GoldRolls.Count)
                {
                    model.NewRandomGoldTiles = page.GetRandomGoldTiles();
                    page.CurrentPlayer.GameData.GoldRolls.Add(model.NewRandomGoldTiles);
                }
                else
                {
                    Debug.Assert(page.CurrentPlayer.GameData.GoldRolls.Count > playerRoll);
                    //
                    //  we've already picked the tiles for this roll -- use them
                    model.NewRandomGoldTiles = page.CurrentPlayer.GameData.GoldRolls[playerRoll];
                }
                // this.TraceMessage($"[Player={CurrentPlayer} [PlayerRole={playerRoll}] [OldGoldTiles={StaticHelpers.SerializeList<int>(currentRandomGoldTiles)}] [NewGoldTiles={StaticHelpers.SerializeList<int>(newRandomGoldTiles)}]");
                await page.SetRandomTileToGold(model.NewRandomGoldTiles);
            }

            return model;

        }

        public static async Task<bool> Undo(MainPage page, ChangedPlayerModel changedPlayerModel)
        {
            page.CurrentPlayer = page.MainPageModel.PlayingPlayers[changedPlayerModel.From];

            if (changedPlayerModel.OldState == GameState.WaitingForNext)
            {
                if (changedPlayerModel.OldRandomGoldTiles != null)
                {
                    await page.SetRandomTileToGold(changedPlayerModel.OldRandomGoldTiles);
                }
                foreach (int idx in changedPlayerModel.HighlightedTiles)
                {
                    page.GameContainer.AllTiles[idx].HighlightTile(page.CurrentPlayer.GameData.BackgroundBrush);

                }
            }



            return true;
        }

        public static async Task<ChangedPlayerModel> Redo(MainPage page, ChangedPlayerModel changedPlayerModel)
        {
            var cpModel = await ChangePlayer(page, changedPlayerModel.To - changedPlayerModel.From, changedPlayerModel.NewState);
            if (!cpModel.IsEqual(changedPlayerModel))
            {
                throw new Exception("Redo should generate identical ChangedPlayerModels!");
            }

            return cpModel;
        }
    }
}