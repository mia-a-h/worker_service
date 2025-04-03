using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace worker_service
{
    public class CoinPrices
    {
        [Key]
        public int Id { get; set; }

        public int CoinId { get; set; } //foreign key to Id in Coin

        [JsonProperty("symbol")]
        public string Symbol { get; set; }

        [JsonProperty("name")]
        public string Name { get; set; }

        [JsonProperty("image")]
        public string Image { get; set; }

        [JsonProperty("market_cap")]
        public decimal MarketCap { get; set; }

        [JsonProperty("market_cap_rank")]
        public int MarketCapRank { get; set; }

        [JsonProperty("total_supply")]
        public decimal TotalSupply { get; set; }

        [JsonProperty("price_change_24h")]
        public decimal PriceChange24h { get; set; }

        public decimal PriceInUsd { get; set; }
        public decimal PriceInEuro { get; set; }
        public decimal PriceInAed { get; set; }

        [JsonProperty("last_updated")]
        public DateTime LastUpdated { get; set; }

        public void SetPrices(CoinPrice price)
        {
            PriceInUsd = price.PriceInUsd;
            PriceInEuro = price.PriceInEuro;
            PriceInAed = price.PriceInAed;
        }

    }

    public class CoinPrice
    {
        [JsonProperty("usd")]
        public decimal PriceInUsd { get; set; }

        [JsonProperty("eur")]
        public decimal PriceInEuro { get; set; }

        [JsonProperty("aed")]
        public decimal PriceInAed { get; set; }
    }

}
