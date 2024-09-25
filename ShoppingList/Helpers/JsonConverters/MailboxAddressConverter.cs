using MimeKit;
using Newtonsoft.Json;

namespace LabWeb.Helpers.JsonConverters
{
    public class MailboxAddressConverter : JsonConverter<MailboxAddress>
    {
        public override void WriteJson(JsonWriter writer, MailboxAddress value, JsonSerializer serializer)
        {
            writer.WriteValue(value.Address); 
        }

        public override MailboxAddress ReadJson(JsonReader reader, Type objectType, MailboxAddress existingValue, bool hasExistingValue, JsonSerializer serializer)
        {
            string email = (string)reader.Value!;
            return new MailboxAddress("email", email);  
        }
    }
}
