using System.Threading.Tasks;

namespace Core.DependencyInjections.MQTT
{
    public interface IMQTTService
    {
        bool IsServerRunning { get; }

        public Task RunMinimalServer();

        public Task ForceDisconnectingClient();

        public Task PublishMessageFromBroker();

        public Task RunServerWithLogging();

        public Task ValidatingConnections();
    }
}
