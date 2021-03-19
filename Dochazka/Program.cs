using System;
using ContactManager.Data;
using Dochazka.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Dochazka
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = CreateHostBuilder(args).Build();

            using (var scope = host.Services.CreateScope())
            {
                var services = scope.ServiceProvider;
                var logger = services.GetRequiredService<ILogger<Program>>();

                try
                {
                    var context = services.GetRequiredService<ApplicationDbContext>();
                    context.Database.Migrate();

                    // requires using Microsoft.Extensions.Configuration;
                    var config = host.Services.GetRequiredService<IConfiguration>();
                    
                    // test configuration service:
                    logger.LogWarning("TestKey: {TestValue}", config["TestKey"]);

                    // Set password with the Secret Manager tool, dotnet user-secrets set SeedUserPW <pw>                    
                    var testUserPw = config["SeedUserPW"];
                    
                    // Seed database
                    SeedData.Initialize(services, testUserPw).Wait();                    
                }
                catch (Exception ex)
                {                    
                    logger.LogError(ex, "An error occurred seeding the DB.");
                }                
            }            

            host.Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureLogging(logging =>
                {
                    logging.ClearProviders();
                    logging.AddConsole();
                })
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>();
                });
    }
}
