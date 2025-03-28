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
                return json["rate"]?.Value<decimal>();
            }
            return null;
        }
    }
}
