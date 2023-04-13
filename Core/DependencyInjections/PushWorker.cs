using Contracts.MobileNotifications;
using DatabaseServices;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Models.Authentication;
using Models.Phones;
using Models.Logging;
using NodaTime.TimeZones;
using Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Core.DependencyInjections
{
    public class PushWorker : BackgroundService
    {
        private readonly ILogger<LoggerWorker> _logger;
        private readonly IServiceScopeFactory serviceProvider;

        private PushApiClient _pushApiClient;
        public PushWorker(ILogger<LoggerWorker> logger, IServiceScopeFactory serviceProvider)
        {
            _logger = logger;
            this.serviceProvider = serviceProvider;
            _pushApiClient = new PushApiClient();
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

                        var users = context.Users.Include("Devices").Include("Words");
                        PushTicketRequest ptRequest = new PushTicketRequest()
                        {
                            PushTo = new List<string>(),
                            PushData = new { some = "some data" },
                            PushTitle = "Sup Broski",
                            PushBody = "It's time to learn",
                            PushTTL = 5,
                            PushExpiration = 5,
                            PushPriority = "normal",
                            PushSubTitle = "yo",
                            PushSound = "default",
                            PushBadgeCount = 1,


                        };
                        foreach (var user in users)
                        {
                            //foreach(Device device in user.Devices)
                            //{
                            //    ptRequest.PushTo.Add(device.ExpoPushToken.ToString());


                            //    TimeZoneInfo tzi = GetTimeZoneInfoForTzdbId(device.TimeZone);
                            //    DateTime date = TimeZoneInfo.ConvertTimeFromUtc(DateTime.UtcNow, tzi);
                            //    //Only push notifications at 9 oclock
                            //    if (date.Hour == 9 && date.Minute == 0)
                            //    {
                            //        PushTicketResponse response = await _pushApiClient.PushSendAsync(ptRequest);
                            //        if (response.PushTicketErrors != null)
                            //        {
                            //            Console.WriteLine("test");
                            //        }

                                 
                            //    }
                            //}
                        }

                        await context.SaveChangesAsync();

                    }

                }
                catch (Exception ex)
                {
                    Console.WriteLine(ex);
                }
                //every minute
                await Task.Delay(60000, stoppingToken);
            }
        }

        

        private TimeZoneInfo GetTimeZoneInfoForTzdbId(string tzdbId)
        {
            var mappings = TzdbDateTimeZoneSource.Default.WindowsMapping.MapZones;
            var map = mappings.FirstOrDefault(x =>
                x.TzdbIds.Any(z => z.Equals(tzdbId, StringComparison.OrdinalIgnoreCase)));
            return map == null ? null : TimeZoneInfo.FindSystemTimeZoneById(map.WindowsId);
        }
       

    }
}
