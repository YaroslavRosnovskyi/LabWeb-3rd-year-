using System;
using Microsoft.Azure.WebJobs;
using Microsoft.Azure.WebJobs.Host;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;

namespace CommonFunctions
{
    public class EmailFunction
    {
        public EmailFunction()
        {
            
        }

        [FunctionName("EmailFunction")]
        public void Run([ServiceBusTrigger("emailqueue", Connection = "az-service-bus")]string messageBody, ILogger log)
        {
            log.LogInformation($"C# ServiceBus queue trigger function processed message: {messageBody}");

            var emailMessage = JsonConvert.DeserializeObject<EmailServiceBusMessage>(messageBody);

            _emailClient.SendMessage()
        }
    }
}
