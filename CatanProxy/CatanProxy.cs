using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Net.Http;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace Catan.Proxy
{
    /// <summary>
    ///     A proxy shared by client and service.  This is in both projects and can be found in https://github.com/joelong/catan
    /// </summary>
    public class CatanProxy : IDisposable
    {


        public HttpClient Client { get; set; } = new HttpClient();
        private CancellationTokenSource _cts = new CancellationTokenSource(TimeSpan.FromDays(1));
        public string HostName { get; set; } // "http://localhost:50919";
        public CatanResult LastError { get; set; } = null;

        public Task<PlayerResources> RefundEntitlement(string gameName, PurchaseLog log)
        {
            if (String.IsNullOrEmpty(gameName))
            {
                throw new Exception("names can't be null or empty");
            }
            if (log is null)
            {
                throw new Exception("Purchase Log cannot be null in RefundEntitlment");
            }
            string url = $"{HostName}/api/catan/purchase/refund/{gameName}";

            return Post<PlayerResources>(url, Serialize<PurchaseLog>(log));
        }
        public Task<PlayerResources> BuyEntitlement(string gameName, string playerName, Entitlement entitlement)
        {
            if (String.IsNullOrEmpty(gameName) || String.IsNullOrEmpty(playerName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/purchase/{gameName}/{playerName}/{entitlement}";

            return Post<PlayerResources>(url, null);
        }

        public Task<List<string>> CreateGame(string gameName, GameInfo gameInfo)
        {
            if (String.IsNullOrEmpty(gameName))
            {
                throw new Exception("names can't be null or empty");
            }
            if (gameInfo == null)
            {
                throw new Exception("gameInfo can't be null");
            }
            string url = $"{HostName}/api/catan/game/create/{gameName}";

            return Post<List<string>>(url, CatanProxy.Serialize<GameInfo>(gameInfo));
        }

        public string LastErrorString { get; set; } = "";
        public CatanProxy()
        {
            Client.Timeout = TimeSpan.FromHours(5);
        }

        public Task<PlayerResources> JoinGame(string gameName, string playerName)
        {

            if (String.IsNullOrEmpty(gameName) || String.IsNullOrEmpty(playerName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/game/joingame/{gameName}/{playerName}";

            return Post<PlayerResources>(url, null);

        }



        public Task<PlayerResources> GetResources(string gameName, string playerName)
        {
            if (String.IsNullOrEmpty(gameName) || String.IsNullOrEmpty(playerName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/resource/{gameName}/{playerName}";
            return Get<PlayerResources>(url);
        }



        public Task<PlayerResources> DevCardPurchase(string gameName, string playerName)
        {
            if (String.IsNullOrEmpty(gameName) || String.IsNullOrEmpty(playerName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/devcard/{gameName}/{playerName}";

            return Post<PlayerResources>(url, null);
        }
        public Task<PlayerResources> PlayYearOfPlenty(string gameName, string playerName, TradeResources tr)
        {

            if (String.IsNullOrEmpty(gameName) || String.IsNullOrEmpty(playerName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/devcard/play/yearofplenty/{gameName}/{playerName}";

            return Post<PlayerResources>(url, Serialize(tr));
        }

        public Task<PlayerResources> PlayKnight(string gameName, string playerName)
        {
            if (String.IsNullOrEmpty(gameName) || String.IsNullOrEmpty(playerName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/devcard/play/knight/{gameName}/{playerName}";

            return Post<PlayerResources>(url, null);
        }

        public Task<PlayerResources> PlayRoadBuilding(string gameName, string playerName)
        {
            if (String.IsNullOrEmpty(gameName) || String.IsNullOrEmpty(playerName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/devcard/play/roadbuilding/{gameName}/{playerName}";

            return Post<PlayerResources>(url, null);
        }
        public Task<PlayerResources> PlayMonopoly(string gameName, string playerName, ResourceType resourceType)
        {
            if (String.IsNullOrEmpty(gameName) || String.IsNullOrEmpty(playerName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/devcard/play/monopoly/{gameName}/{playerName}/{resourceType}";

            return Post<PlayerResources>(url, null);
        }
        public Task<List<string>> GetGames()
        {
            string url = $"{HostName}/api/catan/game";

            return Get<List<string>>(url);

        }
        public Task<List<string>> GetUsers(string gameName)
        {
            if (String.IsNullOrEmpty(gameName))
            {
                throw new Exception("names can't be null or empty");
            }

            string url = $"{HostName}/api/catan/game/users/{gameName}";

            return Get<List<string>>(url);

        }

        public Task<PlayerResources> TradeGold(string gameName, string playerName, TradeResources tradeResources)
        {
            if (String.IsNullOrEmpty(playerName) || String.IsNullOrEmpty(playerName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/resource/tradegold/{gameName}/{playerName}";
            return Post<PlayerResources>(url, Serialize(tradeResources));
        }

        public Task<GameInfo> GetGameInfo(string gameName)
        {
            if (String.IsNullOrEmpty(gameName))
            {
                throw new Exception("names can't be null or empty");
            }

            string url = $"{HostName}/api/catan/game/gameInfo/{gameName}";

            return Get<GameInfo>(url);

        }



        public Task<CatanResult> DeleteGame(string gameName)
        {

            if (String.IsNullOrEmpty(gameName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/game/{gameName}";

            return Delete<CatanResult>(url);


        }

        public Task StartGame(string gameName)
        {
            if (String.IsNullOrEmpty(gameName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/game/start/{gameName}";
            return Post<string>(url, null);
        }

        public async Task<List<ServiceLogRecord>> Monitor(string gameName, string playerName)
        {
            if (String.IsNullOrEmpty(gameName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/monitor/{gameName}/{playerName}";
            string json = await Get<string>(url);

            ServiceLogCollection serviceLogCollection = CatanProxy.Deserialize<ServiceLogCollection>(json);
            List<ServiceLogRecord> records = ParseServiceLogRecord(serviceLogCollection);
            //Debug.WriteLine($"[Game={gameName}] [Player={playerName}] [LogCount={logList.Count}]");
            return records;
        }

        private List<ServiceLogRecord> ParseServiceLogRecord(ServiceLogCollection serviceLogCollection)
        {
            List<ServiceLogRecord> records = new List<ServiceLogRecord>();
            foreach (var rec in serviceLogCollection.LogRecords)
            {
                //
                //  we have to Deserialize to the Header to find out what kind of object we have - this means we double Deserialize the object
                //  we could avoid this by serializing a list of Actions to matach 
                ServiceLogRecord logEntry = CatanProxy.Deserialize<ServiceLogRecord>(rec.ToString());
                switch (logEntry.Action)
                {
                    case ServiceAction.Refund:
                    case ServiceAction.Purchased:
                        PurchaseLog purchaseLog = CatanProxy.Deserialize<PurchaseLog>(rec.ToString());
                        ParseCatanRequest(purchaseLog.UndoRequest);
                        records.Add(purchaseLog);
                        break;
                    case ServiceAction.UserRemoved:
                    case ServiceAction.GameDeleted:
                    case ServiceAction.PlayerAdded:
                    case ServiceAction.GameStarted:
                        GameLog gameLog = CatanProxy.Deserialize<GameLog>(rec.ToString());
                        records.Add(gameLog);
                        break;
                    case ServiceAction.PlayedYearOfPlenty:
                    case ServiceAction.TradeGold:
                    case ServiceAction.GrantResources:
                    case ServiceAction.ReturnResources:
                        ResourceLog resourceLog = CatanProxy.Deserialize<ResourceLog>(rec.ToString());
                        records.Add(resourceLog);
                        break;
                    case ServiceAction.TakeCard:
                        TakeLog takeLog = CatanProxy.Deserialize<TakeLog>(rec.ToString());
                        records.Add(takeLog);
                        break;
                    case ServiceAction.MeritimeTrade:
                        MeritimeTradeLog mtLog = CatanProxy.Deserialize<MeritimeTradeLog>(rec.ToString());
                        records.Add(mtLog);
                        break;
                    case ServiceAction.UpdatedTurn:
                        TurnLog tLog = CatanProxy.Deserialize<TurnLog>(rec.ToString());
                        records.Add(tLog);
                        break;
                    case ServiceAction.PlayedMonopoly:
                    case ServiceAction.LostToMonopoly:
                        MonopolyLog mLog = CatanProxy.Deserialize<MonopolyLog>(rec.ToString());
                        records.Add(mLog);
                        break;
                    case ServiceAction.PlayedKnight:
                    case ServiceAction.PlayedRoadBuilding:
                        records.Add(logEntry);
                        break;
                    case ServiceAction.GameCreated:
                    case ServiceAction.Undefined:
                    case ServiceAction.TradeResources:

                    default:
                        throw new Exception($"{logEntry.Action} has no Deserializer! logEntry: {logEntry}");
                }
            }
            return records;
        }

        public async Task<List<ServiceLogRecord>> GetAllLogs(string gameName, string playerName, int startAt)
        {
            if (String.IsNullOrEmpty(gameName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/monitor/logs/{gameName}/{playerName}/{startAt}";
            string json = await Get<string>(url);
            if (String.IsNullOrEmpty(json)) return null;

            ServiceLogCollection serviceLogCollection = CatanProxy.Deserialize<ServiceLogCollection>(json);
            List<ServiceLogRecord> records = ParseServiceLogRecord(serviceLogCollection);
            return records;



        }

        /// <summary>
        ///     Takes resources (Ore, Wheat, etc.) from global pool and assigns to playerName
        /// </summary>
        /// <param name="gameName"></param>
        /// <param name="playerName"></param>
        /// <param name="resources"></param>
        /// <returns></returns>

        public Task<PlayerResources> GrantResources(string gameName, string playerName, TradeResources resources)
        {
            if (String.IsNullOrEmpty(gameName) || String.IsNullOrEmpty(playerName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/resource/grant/{gameName}/{playerName}";
            var body = CatanProxy.Serialize<TradeResources>(resources);
            return Post<PlayerResources>(url, body);
        }
        public Task<PlayerResources> ReturnResource(string gameName, string playerName, TradeResources resources)
        {
            if (String.IsNullOrEmpty(gameName) || String.IsNullOrEmpty(playerName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/resource/return/{gameName}/{playerName}";
            var body = CatanProxy.Serialize<TradeResources>(resources);
            return Post<PlayerResources>(url, body);
        }

        public Task<PlayerResources> UndoGrantResource(string gameName, ResourceLog lastLogRecord)
        {
            if (lastLogRecord is null)
            {
                throw new Exception("log record can't be null");
            }
            if (String.IsNullOrEmpty(gameName) || String.IsNullOrEmpty(lastLogRecord.PlayerName))
            {
                throw new Exception("names can't be null or empty");
            }

            string url = $"{HostName}/api/catan/resource/undo/{gameName}";
            var body = CatanProxy.Serialize<ResourceLog>(lastLogRecord);
            return Post<PlayerResources>(url, body);
        }
        public Task<List<PlayerResources>> Trade(string gameName, string fromPlayer, TradeResources from, string toPlayer, TradeResources to)
        {
            if (String.IsNullOrEmpty(fromPlayer) || String.IsNullOrEmpty(toPlayer) || String.IsNullOrEmpty(gameName))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/api/catan/resource/trade/{gameName}/{fromPlayer}/{toPlayer}";
            var body = CatanProxy.Serialize<TradeResources[]>(new TradeResources[] { from, to });
            return Post<List<PlayerResources>>(url, body);
        }


        private async Task<T> Get<T>(string url)
        {


            if (String.IsNullOrEmpty(url))
            {
                throw new Exception("the URL can't be null or empty");
            }



            LastError = null;
            LastErrorString = "";
            string json = "";
            try
            {
                var response = await Client.GetAsync(url, _cts.Token);
                if (response.IsSuccessStatusCode)
                {
                    json = await response.Content.ReadAsStringAsync();

                    if (typeof(T) == typeof(string))
                    {
                        T workaround = (T)(object)json;
                        return workaround;
                    }
                    T obj = CatanProxy.Deserialize<T>(json);
                    return obj;
                }
                else
                {
                    Debug.WriteLine($"Error grom GetAsync: {response} {Environment.NewLine} {response.ReasonPhrase}");
                }


            }
            catch (HttpRequestException)
            {
                // see if there is a Catan Exception

                LastErrorString = json;
                try
                {
                    LastError = ParseCatanResult(json);
                }
                catch
                {
                    return default;
                }

            }
            catch (Exception e)
            {
                LastErrorString = json + e.ToString();
                return default;
            }
            return default;
        }



        private CatanResult ParseCatanResult(string json)
        {
            if (String.IsNullOrEmpty(json)) return null;

            var result = CatanProxy.Deserialize<CatanResult>(json);
            ParseCatanRequest(result.CantanRequest);

            return result;
        }

        /// <summary>
        ///     System.Text.Json does not do polymorphic Deserialization. So we serialize the object and its type.  here 
        ///     we switch on the type and then covert the JSON returned by ASP.net to string and then deserialize it into the 
        ///     right type.
        /// </summary>
        /// <param name="unparsedRequest"></param>
        private void ParseCatanRequest(CatanRequest unparsedRequest)
        {
            if (unparsedRequest == null) return;
            switch (unparsedRequest.BodyType)
            {
                case BodyType.TradeResources:
                    unparsedRequest.Body = CatanProxy.Deserialize<TradeResources>(unparsedRequest.Body.ToString());
                    break;
                case BodyType.GameInfo:
                    unparsedRequest.Body = CatanProxy.Deserialize<GameInfo>(unparsedRequest.Body.ToString());
                    break;
                case BodyType.TradeResourcesList:
                    unparsedRequest.Body = CatanProxy.Deserialize<TradeResources[]>(unparsedRequest.Body.ToString());
                    break;
                case BodyType.None:
                default:
                    break;
            }
        }

        private async Task<T> Delete<T>(string url)
        {

            if (String.IsNullOrEmpty(url))
            {
                throw new Exception("the URL can't be null or empty");
            }



            LastError = null;
            LastErrorString = "";

            try
            {

                var response = await Client.DeleteAsync(url, _cts.Token);
                var json = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    T obj = CatanProxy.Deserialize<T>(json);
                    return obj;
                }
                else
                {
                    LastErrorString = await response.Content.ReadAsStringAsync();
                    try
                    {
                        LastError = CatanProxy.Deserialize<CatanResult>(LastErrorString);
                        return default;
                    }
                    catch
                    {
                        return default;
                    }

                }
            }
            catch (Exception e)
            {
                // see if there is a Catan Exception
                LastErrorString = e.ToString();
                return default;
            }
        }

        public Task<T> PostUndoRequest<T>(CatanRequest undoRequest)
        {
            if (undoRequest == null || String.IsNullOrEmpty(undoRequest.Url))
            {
                throw new Exception("names can't be null or empty");
            }
            string url = $"{HostName}/{undoRequest.Url}";
            string body = CatanProxy.Serialize<object>(undoRequest.Body);                        
            return Post<T>(url, body);
        }
        
        private async Task<T> Post<T>(string url, string body)
        {

            if (String.IsNullOrEmpty(url))
            {
                throw new Exception("the URL can't be null or empty");
            }



            LastError = null;
            LastErrorString = "";

            try
            {
                HttpResponseMessage response;
                if (body != null)
                {
                    response = await Client.PostAsync(url, new StringContent(body, Encoding.UTF8, "application/json"), _cts.Token);
                }
                else
                {
                    response = await Client.PostAsync(url, new StringContent("", Encoding.UTF8, "application/json"));
                }

                string json = await response.Content.ReadAsStringAsync();
                if (response.IsSuccessStatusCode)
                {
                    if (typeof(T) == typeof(string))
                    {
                        T workaround = (T)(object)json;
                        return workaround;
                    }

                    T obj = CatanProxy.Deserialize<T>(json);
                    return obj;
                }
                else
                {
                    LastErrorString = json;
                    LastError = ParseCatanResult(json);
                    return default;


                }
            }
            catch (Exception e)
            {
                // see if there is a Catan Exception
                LastErrorString = e.ToString();
                return default;
            }
        }

        public void CancelAllRequests()
        {
            _cts.Cancel();
        }

        public void Dispose()
        {
            CancelAllRequests();
            Client.Dispose();
        }
        public static JsonSerializerOptions GetJsonOptions(bool indented = false)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                WriteIndented = indented

            };
            options.Converters.Add(new JsonStringEnumConverter());
            return options;
        }
        static public string Serialize<T>(T obj, bool indented = false)
        {
            if (obj == null) return null;
            return JsonSerializer.Serialize<T>(obj, GetJsonOptions(indented));
        }
        static public T Deserialize<T>(string json)
        {
            var options = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
            };
            options.Converters.Add(new JsonStringEnumConverter());
            return JsonSerializer.Deserialize<T>(json, options);
        }
    }
}
