using System;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.OpenApi.Models;
using PlatformService.AsyncDataService;
using PlatformService.Data;
using PlatformService.SyncDataService.Http;
using Serilog;

namespace PlatformService
{
    public class Startup
    {
        public IConfiguration Configuration { get; }
        private readonly IWebHostEnvironment _environment;

        public Startup(IConfiguration configuration, IWebHostEnvironment environment)
        {
            Configuration = configuration;
            _environment = environment;
        }

        public void ConfigureServices(IServiceCollection services)
        {
            var logger = new LoggerConfiguration()
                                .Enrich.FromLogContext()
                                .WriteTo.Console()
                                .CreateLogger();

            services
               .AddLogging(loggingBuilder =>
               {
                   loggingBuilder
                   .ClearProviders()
                   .AddConsole()
                   .AddSerilog(logger, dispose: true);
               });

            services.AddControllers();

            if (_environment.IsProduction())
            {
                Console.WriteLine($"--> Using Postgres DB");
                services.AddDbContext<AppDbContext>(options => options.UseNpgsql(Configuration.GetConnectionString("PlatformConnection")));
            }
            else
            {
                Console.WriteLine($"--> Using InMem DB");
                services.AddDbContext<AppDbContext>(options => options.UseInMemoryDatabase("InMen"));
            }

            services.AddTransient<AppDbContext>();

            services.AddHttpClient<ICommandDataClient, HttpCommandDataClient>();

            services.AddSingleton<IMessageBusClient, MessageBusClient>();

            services.AddScoped<IPlatformRepository, PlatformRepository>();
            services.AddAutoMapper(AppDomain.CurrentDomain.GetAssemblies());

            Console.WriteLine($"Platform endpoint -> {Configuration["CommandService"]}");

            services.AddSwaggerGen(c =>
            {
                c.SwaggerDoc("v1", new OpenApiInfo { Title = "PlatformService", Version = "v1" });
            });
        }

        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
                app.UseSwagger();
                app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "PlatformService v1"));
            }

            app.UseRouting();
            app.UseAuthorization();
            app.PrePopulation(env.IsProduction());

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });
        }
    }
}
