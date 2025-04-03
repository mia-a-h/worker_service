using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.WebSockets;
using System.Text;
using System.Threading.Tasks;

namespace worker_service
{
    public class CoinGeckoService
    {
        private readonly HttpClient _httpClient;
        private readonly IServiceScopeFactory _scopeFactory;

        public CoinGeckoService(HttpClient httpClient, IServiceScopeFactory scopeFactory)
        {
            _httpClient = httpClient;
            _scopeFactory = scopeFactory;
        }

        public async Task<decimal?> GetExchangeRateAsync(string baseAsset, string quoteAsset)
        {
            var response = await _httpClient.GetAsync($"simple/price?ids={baseAsset}&vs_currencies={quoteAsset}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var json = JObject.Parse(content);

                var rate = json[baseAsset]?.Value<decimal?>($"{quoteAsset}");
                return rate;
            }

            return null;
        }

        public async Task<Dictionary<string, int>> GetCoinsLookupAsync()
        {
            // This method should query your Coins table and return a dictionary 
            // mapping API coin IDs (string) to your database coin IDs (int)
            using (var scope = _scopeFactory.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var coins = await dbContext.Coins
                    .FromSqlRaw("SELECT Id, CoinId FROM Coins")
                    .Select(c => new { c.Id, c.CoinId }).ToListAsync();

                return coins.ToDictionary(c => c.CoinId, c => c.Id);    //lookup dictionary
            }
        }

        public async Task<List<CoinPrices>> GetCoinMarketDataAsync()
        {
            var response = await _httpClient.GetAsync("coins/markets?vs_currency=usd");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: {content.Substring(0, 500)}"); // Print first 500 characters
                
                var apiCoins = JsonConvert.DeserializeObject<List<ApiCoinPrice>>(content);

                var coinsLookup = await GetCoinsLookupAsync();
                var result = new List<CoinPrices>();

                foreach(var apiCoin in apiCoins)
                {
                    if(coinsLookup.TryGetValue(apiCoin.CoinId, out var dbCoinId))
                    {
                        var dbCoin = new CoinPrices
                        {
                            CoinId = dbCoinId,
                            Symbol = apiCoin.Symbol,
                            Name = apiCoin.Name,
                            Image = apiCoin.Image,
                            MarketCap = apiCoin.MarketCap,
                            MarketCapRank = apiCoin.MarketCapRank,
                            TotalSupply = apiCoin.TotalSupply,
                            PriceChange24h = apiCoin.PriceChange24h,
                            LastUpdated = apiCoin.LastUpdated,
                        };
                        result.Add(dbCoin);
                    }
                    else
                    {
                        Console.WriteLine($"Warning: Coin {apiCoin.CoinId} not found in database");
                    }
                }

                return result;
            }
            else
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
            }

            return null;
        }

        public async Task<Dictionary<int, CoinPrice>> GetCoinPricesAsync(List<string> coinIds, List<string> currencies)
        {
            string ids = string.Join(",", coinIds);
            string vsCurrencies = string.Join(",", currencies);

            var response = await _httpClient.GetAsync($"simple/price?ids={ids}&vs_currencies={vsCurrencies}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var apiPrices = JsonConvert.DeserializeObject<Dictionary<string, CoinPrice>>(content);

                var coinsLookup = await GetCoinsLookupAsync();
                var dbPrices = new Dictionary<int, CoinPrice>();

                foreach (var apiCoin in apiPrices)
                {
                    if (coinsLookup.TryGetValue(apiCoin.Key, out var dbCoinId))
                    {
                        dbPrices[dbCoinId] = apiCoin.Value;
                    }
                }

                return dbPrices;
            }

            return null;

        }

        public async Task<List<CoinPrices>> GetCompleteCoinDataAsync()
        {
            var coins = await GetCoinMarketDataAsync();
            if (coins == null) return null;

            var coinsLookup = await GetCoinsLookupAsync();
            var apiCoinIds = coinsLookup.Keys.ToList();

            var currencies = new List<string>(){
                "usd",
                "aed",
                "eur"
            };

            var prices = await GetCoinPricesAsync(apiCoinIds, currencies);

            if (prices != null)
            {
                foreach (var coin in coins)
                {
                    if (prices.TryGetValue(coin.CoinId, out var price))
                    {
                        coin.SetPrices(price);
                    }
                }
            }

            return coins;
        }
    }
}
