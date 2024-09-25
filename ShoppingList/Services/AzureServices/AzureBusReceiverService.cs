using Azure.Messaging.ServiceBus;
using Elastic.Clients.Elasticsearch.Nodes;
using LabWeb.DTOs.ServiceBusDTO;
using LabWeb.Services.Interfaces;
using Newtonsoft.Json;

namespace LabWeb.Services.AzureServices;

public class AzureBusReceiverService : IHostedService
{
    private readonly IEmailMessageSender _emailSender;

    public AzureBusReceiverService(IEmailMessageSender emailSender)
    {
        _emailSender = emailSender;
        client = new ServiceBusClient(serviceBusConnectionString);
        processor = client.CreateProcessor(queueName, new ServiceBusProcessorOptions());
    }

    private const string serviceBusConnectionString =
        "Endpoint=sb://az-lab-web-bus.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=cb6AmJW64jnbZmYpMVjTYl3NBFKeXUodB+ASbHHH5NQ=";

    private const string queueName = "az-lab-web-queue";

    const int maxNumberOfMessages = 3;

    ServiceBusClient client;
    ServiceBusProcessor processor = default!;



    public async Task StartAsync(CancellationToken cancellationToken)
    {
        processor.ProcessMessageAsync += MessageHandler;
        processor.ProcessErrorAsync += ErrorHandler;

        await processor.StartProcessingAsync(cancellationToken);
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        await processor.StopProcessingAsync(cancellationToken);
        await client.DisposeAsync();
        await processor.DisposeAsync();
    }

    private async Task MessageHandler(ProcessMessageEventArgs args)
    {
        string body = args.Message.Body.ToString();
        var deserializedBody = JsonConvert.DeserializeObject<Message>(body);

        await _emailSender.SendEmail(deserializedBody);
        await args.CompleteMessageAsync(args.Message);
    }

    private Task ErrorHandler(ProcessErrorEventArgs args)
    {
        Console.WriteLine(args.Exception.ToString());
        return Task.CompletedTask;
    }


}