using FundManagerStateMachine.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.DependencyInjection;

class Program
{

    static void Main(string[] args)
    {
        // Build the configuration from appsettings.json
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory()) // Ensure the appsettings.json file is found
            .AddJsonFile("appsettings.json", optional: false, reloadOnChange: true)
            .Build();

        var host = Host.CreateDefaultBuilder(args)
            .ConfigureServices((context, services) =>
            {
                // Configure the DbContext with connection string from appsettings.json
                services.AddDbContext<StateMachineDbContext>(options =>
                    options.UseSqlServer(configuration.GetConnectionString("FMConnection")));

                // Register your other services, if any
            })
            .Build();

        using (var scope = host.Services.CreateScope())
        {
            var dbContext = scope.ServiceProvider.GetRequiredService<StateMachineDbContext>();

            dbContext.Database.EnsureCreated(); // Create the database if it doesn't exist
            Console.WriteLine("Database created or already exists.");

            // Test changing states

            // test changing states with related states
        }

    }
}