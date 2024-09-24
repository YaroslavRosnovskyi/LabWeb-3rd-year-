using Azure.Messaging.ServiceBus;
using LabWeb.DTOs;
using Newtonsoft.Json;

namespace LabWeb.Services
{
    public class AzureBusSenderService : IAzureBusSenderService
    {
        private const string serviceBusConnectionString =
            "Endpoint=sb://az-lab-web-bus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=cb6AmJW64jnbZmYpMVjTYl3NBFKeXUodB+ASbHHH5NQ=";

        private const string queueName = "az-lab-web-queue";

        ServiceBusClient client;
        ServiceBusSender sender;

        public AzureBusSenderService()
        {
            client = new ServiceBusClient(serviceBusConnectionString);
            sender = client.CreateSender(queueName);
        }

        public async Task Send(Message message)
        {
            var serializedMessage = JsonConvert.SerializeObject(message);

            await sender.SendMessageAsync(new ServiceBusMessage(serializedMessage));
        }

    }
}
