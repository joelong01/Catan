using System;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace Catan10.CatanServiceTest
{
    class ServiceClient
    {
        HttpClient _client = new HttpClient();
        readonly string _serverUri = "http://localhost:54362/";
        public async Task<int> CreateGame()
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            FormUrlEncodedContent content = new FormUrlEncodedContent(values);
            string url = _serverUri + "api/StartGame";
            HttpResponseMessage response = await _client.PostAsync(url, content);
            string responseString = await response.Content.ReadAsStringAsync();
            int gameId = Int32.Parse(responseString);
            return gameId;

        }

        public async Task<bool> AddPlayer(int gameId, string name)
        {
            Dictionary<string, string> values = new Dictionary<string, string>();
            FormUrlEncodedContent content = new FormUrlEncodedContent(values);
            string url = String.Format($"{_serverUri}/api/AddPlayer/gameID={gameId}/name={name}");
            HttpResponseMessage response = await _client.PostAsync(url, content);
            if (response.StatusCode == System.Net.HttpStatusCode.OK)
            {
                return true;
            }

            return false;
        }


    }
}
