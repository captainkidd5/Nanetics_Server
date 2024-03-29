﻿using MQTTnet.Diagnostics;
using MQTTnet.Server;
using MQTTnet;
using MQTTnet.Protocol;

namespace Core.DependencyInjections.MQTT
{
    public class MQTTService : IMQTTService
    {
        public bool IsServerRunning { get; private set; } = false;

        public async Task RunMinimalServer()
        {
            /*
             * This sample starts a simple MQTT server which will accept any TCP connection.
             */

            var mqttFactory = new MqttFactory();

            // The port for the default endpoint is 1883.
            // The default endpoint is NOT encrypted!
            // Use the builder classes where possible.
            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            // The port can be changed using the following API (not used in this example).
            // new MqttServerOptionsBuilder()
            //     .WithDefaultEndpoint()
            //     .WithDefaultEndpointPort(1234)
            //     .Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                try
                {
                    await mqttServer.StartAsync();
                    IsServerRunning = true;
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Unable to start MQTT Server: {ex}");
                }



                // Stop and dispose the MQTT server if it is no longer needed!
                await mqttServer.StopAsync();
            }
        }
        public async Task ForceDisconnectingClient()
        {
            /*
             * This sample will disconnect a client.
             *
             * See _Run_Minimal_Server_ for more information.
             */

            using (var mqttServer = await StartMqttServer())
            {
                // Let the client connect.
                await Task.Delay(TimeSpan.FromSeconds(5));

                // Now disconnect the client (if connected).
                var affectedClient = (await mqttServer.GetClientsAsync()).FirstOrDefault(c => c.Id == "MyClient");
                if (affectedClient != null)
                {
                    await affectedClient.DisconnectAsync();
                }
            }
        }

        public async Task PublishMessageFromBroker()
        {
            /*
             * This sample will publish a message directly at the broker.
             *
             * See _Run_Minimal_Server_ for more information.
             */

            using (var mqttServer = await StartMqttServer())
            {
                // Create a new message using the builder as usual.
                var message = new MqttApplicationMessageBuilder().WithTopic("HelloWorld").WithPayload("Test").Build();

                // Now inject the new message at the broker.
                await mqttServer.InjectApplicationMessage(
                    new InjectedMqttApplicationMessage(message)
                    {
                        SenderClientId = "SenderClientId"
                    });
            }
        }


        public async Task RunServerWithLogging()
        {
            /*
             * This sample starts a simple MQTT server and prints the logs to the output.
             *
             * IMPORTANT! Do not enable logging in live environment. It will decrease performance.
             *
             * See sample "Run_Minimal_Server" for more details.
             */

            var mqttFactory = new MqttFactory(new ConsoleLogger());

            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                await mqttServer.StartAsync();

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                // Stop and dispose the MQTT server if it is no longer needed!
                await mqttServer.StopAsync();
            }
        }

        public async Task ValidatingConnections()
        {
            /*
             * This sample starts a simple MQTT server which will check for valid credentials and client ID.
             *
             * See _Run_Minimal_Server_ for more information.
             */

            var mqttFactory = new MqttFactory();

            var mqttServerOptions = new MqttServerOptionsBuilder().WithDefaultEndpoint().Build();

            using (var mqttServer = mqttFactory.CreateMqttServer(mqttServerOptions))
            {
                // Setup connection validation before starting the server so that there is 
                // no change to connect without valid credentials.
                mqttServer.ValidatingConnectionAsync += e =>
                {
                    if (e.ClientId != "ValidClientId")
                    {
                        e.ReasonCode = MqttConnectReasonCode.ClientIdentifierNotValid;
                    }

                    if (e.UserName != "ValidUser")
                    {
                        e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    }

                    if (e.Password != "SecretPassword")
                    {
                        e.ReasonCode = MqttConnectReasonCode.BadUserNameOrPassword;
                    }

                    return Task.CompletedTask;
                };

                await mqttServer.StartAsync();

                Console.WriteLine("Press Enter to exit.");
                Console.ReadLine();

                await mqttServer.StopAsync();
            }
        }

        async Task<MqttServer> StartMqttServer()
        {
            var mqttFactory = new MqttFactory();

            // Due to security reasons the "default" endpoint (which is unencrypted) is not enabled by default!
            var mqttServerOptions = mqttFactory.CreateServerOptionsBuilder().WithDefaultEndpoint().Build();
            var server = mqttFactory.CreateMqttServer(mqttServerOptions);
            await server.StartAsync();
            return server;
        }

        class ConsoleLogger : IMqttNetLogger
        {
            readonly object _consoleSyncRoot = new();

            public bool IsEnabled => true;

            public void Publish(MqttNetLogLevel logLevel, string source, string message, object[]? parameters, Exception? exception)
            {
                var foregroundColor = ConsoleColor.White;
                switch (logLevel)
                {
                    case MqttNetLogLevel.Verbose:
                        foregroundColor = ConsoleColor.White;
                        break;

                    case MqttNetLogLevel.Info:
                        foregroundColor = ConsoleColor.Green;
                        break;

                    case MqttNetLogLevel.Warning:
                        foregroundColor = ConsoleColor.DarkYellow;
                        break;

                    case MqttNetLogLevel.Error:
                        foregroundColor = ConsoleColor.Red;
                        break;
                }

                if (parameters?.Length > 0)
                {
                    message = string.Format(message, parameters);
                }

                lock (_consoleSyncRoot)
                {
                    Console.ForegroundColor = foregroundColor;
                    Console.WriteLine(message);

                    if (exception != null)
                    {
                        Console.WriteLine(exception);
                    }
                }
            }
        }
    }
}
