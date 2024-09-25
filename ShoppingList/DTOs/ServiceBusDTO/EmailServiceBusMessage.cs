using LabWeb.Helpers.JsonConverters;
using MimeKit;
using Newtonsoft.Json;

namespace LabWeb.DTOs.ServiceBusDTO
{
    public class Message
    {
        [JsonConverter(typeof(MailboxAddressConverter))]
        public MailboxAddress To { get; set; }
        public string Subject { get; set; }
        public string Content { get; set; }
        public Message(string to, string subject, string content)
        {
            To = new MailboxAddress("email", to);
            Subject = subject;
            Content = content;
        }
    }
}
