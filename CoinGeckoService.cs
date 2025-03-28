using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace worker_service
{
    public class CoinGeckoService
    {
        private readonly HttpClient _httpClient;

        public CoinGeckoService(HttpClient httpClient)
        {
            _httpClient = httpClient;
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

        public async Task<List<Coin>> GetCoinMarketDataAsync()
        {
            var response = await _httpClient.GetAsync("coins/markets?vs_currency=usd");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"API Response: {content.Substring(0, 500)}"); // Print first 500 characters
                return JsonConvert.DeserializeObject<List<Coin>>(content);
            }
            else
            {
                Console.WriteLine($"API Error: {response.StatusCode}");
            }

            return null;
        }

        public async Task<Dictionary<string, CoinPrice>> GetCoinPricesAsync(List<string> coinIds, List<string> currencies)
        {
            string ids = string.Join(",", coinIds);
            string vsCurrencies = string.Join(",", currencies);

            var response = await _httpClient.GetAsync($"simple/price?ids={ids}&vs_currencies={vsCurrencies}");

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                return JsonConvert.DeserializeObject<Dictionary<string, CoinPrice>>(content);
            }

            return null;

        }

        public async Task<List<Coin>> GetCompleteCoinDataAsync()
        {
            var coins = await GetCoinMarketDataAsync();
            if (coins == null) return null;

            var coinIds = coins.Select(c => c.Id).ToList();
            var currencies = new List<string>(){
                "usd",
                "aed",
                "eur"
            };

            var prices = await GetCoinPricesAsync(coinIds, currencies);

            if (prices != null)
            {
                foreach (var coin in coins)
                {
                    if (prices.TryGetValue(coin.Id, out var price))
                    {
                        coin.SetPrices(price);
                    }
                }
            }

            return coins;
        }
    }
}
