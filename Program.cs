using Microsoft.EntityFrameworkCore.SqlServer;
using Microsoft.EntityFrameworkCore;

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
                    // Add the connection string for SQL Server (make sure to add it in appsettings.json)
                    var connectionString = context.Configuration.GetConnectionString("CryptoDbConnection");

                    // Add AppDbContext to the services with SQL Server configuration
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseSqlServer(connectionString));

                    services.AddHttpClient<CoinGeckoService>(client =>
                    {
                        client.BaseAddress = new Uri("https://api.coingecko.com/api/v3/");
                        client.DefaultRequestHeaders.Add("User-Agent", "MyCryptoWorkerService/1.0");
                    });

                    services.AddHostedService<Worker>();
                })
                .Build();

            host.Run();
        }
    }
}