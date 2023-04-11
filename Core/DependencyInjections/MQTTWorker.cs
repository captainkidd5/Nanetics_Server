using Microsoft.Extensions.Hosting;
using MQTTnet.Diagnostics;
using MQTTnet.Server;
using MQTTnet;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Core.DependencyInjections.MQTT;

namespace Core.DependencyInjections
{
    public class MQTTWorker : BackgroundService
    {
        private readonly IMQTTService _mQTTService;

        public MQTTWorker(IMQTTService mQTTService)
        {
            _mQTTService = mQTTService;
        }
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    if (!_mQTTService.IsServerRunning)
                        await _mQTTService.RunMinimalServer();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error starting MQTT Service: " + ex.ToString());
                }
                await Task.Delay(50000, stoppingToken);

            }

        }

    }
}
