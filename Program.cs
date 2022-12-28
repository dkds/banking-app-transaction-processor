using Microsoft.EntityFrameworkCore;
using Microsoft.Net.Http.Headers;
using TransactionProcessor.Data;

namespace TransactionProcessor
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);
            builder.Services.AddDbContext<TransactionProcessorContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("TransactionProcessorContext") ?? throw new InvalidOperationException("Connection string 'TransactionProcessorContext' not found.")));

            builder.Services.AddStackExchangeRedisCache(option =>
            {
                option.Configuration = builder.Configuration.GetConnectionString("Redis");
                option.InstanceName = "common-cache:";
            });
            // Add services to the container.

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            builder.Services.AddHttpClient("ApiManager", httpClient =>
            {
                var config = builder.Configuration.GetSection("ApiManager");
                httpClient.BaseAddress = new Uri(config["Location"] ?? throw new InvalidOperationException("ApiManger Location not found."));

                httpClient.DefaultRequestHeaders.Add(
                    "Ocp-Apim-Subscription-Key", config["SubscriptionKey"]);
            });

            var app = builder.Build();
            app.UsePathBase("/api");

            app.UseSwagger();
            app.UseSwaggerUI();

            app.UseHttpsRedirection();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}