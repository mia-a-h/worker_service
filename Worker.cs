namespace worker_service
{
    public class Worker : BackgroundService
    {
        private readonly ILogger<Worker> _logger; 
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly CoinGeckoService _coinGeckoService;

        public Worker(ILogger<Worker> logger, CoinGeckoService coinGeckoService, IServiceScopeFactory scopeFactory)
        {
            _logger = logger;
            _scopeFactory = scopeFactory;
            _coinGeckoService = coinGeckoService;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                _logger.LogInformation("Worker running at: {time}", DateTimeOffset.Now);

                await UpdateCoinDataAsync();

                // Wait for 6 hours before fetching data again
                //await Task.Delay(TimeSpan.FromHours(6), stoppingToken);
                await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken);
            }
        }

        private async Task UpdateCoinDataAsync()
        {
            using (HttpClient client = new HttpClient())
            {
                var response = await client.GetAsync("https://httpbin.org/get");
                var content = await response.Content.ReadAsStringAsync();
                Console.WriteLine(content);  // Look for "User-Agent" in the output
            }

            try
            {
                var coins = await _coinGeckoService.GetCompleteCoinDataAsync();
                if (coins == null)
                {
                    _logger.LogWarning("No coin data fetched.");
                    return;
                }

                // Create a new scope
                using (var scope = _scopeFactory.CreateScope())
                {
                    var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                    foreach (var coin in coins)
                    {
                        var existingCoin = await dbContext.Coins.FindAsync(coin.Id);
                        if (existingCoin != null)
                        {
                            // Update existing record
                            existingCoin.MarketCap = coin.MarketCap;
                            existingCoin.MarketCapRank = coin.MarketCapRank;
                            existingCoin.TotalSupply = coin.TotalSupply;
                            existingCoin.PriceChange24h = coin.PriceChange24h;
                            existingCoin.PriceInUsd = coin.PriceInUsd;
                            existingCoin.PriceInEuro = coin.PriceInEuro;
                            existingCoin.PriceInAed = coin.PriceInAed;

                            dbContext.Coins.Update(existingCoin);
                        }
                        else
                        {
                            // Insert new record
                            dbContext.Coins.Add(coin);
                        }
                    }

                    await dbContext.SaveChangesAsync();
                }

                _logger.LogInformation("Coin data updated successfully.");
            }
            catch (Exception ex)
            {
                _logger.LogError("Error updating coin data: {message}", ex.Message);
            }
        }
    }
}