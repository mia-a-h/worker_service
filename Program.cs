namespace worker_service
{
    public class Program
    {
        public static void Main(string[] args)
        {
            IHost host = Host.CreateDefaultBuilder(args)
                .ConfigureServices((context, services) =>
                {
                    //string apiKey = context.Configuration["CoinApi:ApiKey"];

                    services.AddHttpClient<CoinGeckoService>(client =>
                    {
                        client.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
                    });

                    services.AddHostedService<Worker>();
                })
                .Build();

            host.Run();
        }
    }
}