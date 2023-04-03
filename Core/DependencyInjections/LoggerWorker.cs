using DatabaseServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models.Logging;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DependencyInjections
{
    public class LoggerWorker : BackgroundService
    {
        private readonly ILogger<LoggerWorker> _logger;
        private readonly IServiceScopeFactory serviceProvider;


        public LoggerWorker(ILogger<LoggerWorker> logger, IServiceScopeFactory serviceProvider)
        {
            _logger = logger;
            this.serviceProvider = serviceProvider;

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    using (var scope = serviceProvider.CreateScope()) // this will use `IServiceScopeFactory` internally
                    {
                        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                        await CheckLogs(context);

                        await context.SaveChangesAsync();

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                //every five mins
                await Task.Delay(50000, stoppingToken);
            }
        }

        private async Task CheckLogs(AppDbContext context)
        {
            int maxLogs = 10000;

            IQueryable<Log> all = context.Logs;
            int count = all.Count();
            List<Log> logsToRemove = await context.Logs.Where(
                x => x.Level.Equals(LogLevel.Information.ToString())).ToListAsync();
            context.Logs.RemoveRange(logsToRemove);
            // Get the users for the specified page
            List<Log> logs = await context.Logs.OrderByDescending(log => log.TimeStamp).Skip(maxLogs)
                .ToListAsync();
           context.Logs.RemoveRange(logs);

            await context.SaveChangesAsync();

        }





    }
}
