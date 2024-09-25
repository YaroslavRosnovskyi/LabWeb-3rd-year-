using LabWeb.DTOs;
using LabWeb.Services.Interfaces;
using LabWeb.SettingOptions;
using Microsoft.Extensions.Options;
using MimeKit;

namespace LabWeb.Services
{
    public class EmailMessageSender : EmailSender, IEmailMessageSender
    {
        private readonly EmailConfiguration _emailConfig;
        private readonly IConfiguration _configuration;
        public EmailMessageSender(IOptions<EmailConfiguration> emailConfig, IConfiguration configuration) : base(emailConfig, configuration)
        {
            _emailConfig = emailConfig.Value;
            _configuration = configuration;
        }

        protected override MimeMessage CreateEmailMessage(Message message)
        {
            var emailMessage = new MimeMessage();

            var bodyBuilder = new BodyBuilder();
            bodyBuilder.HtmlBody = $"Registration was successful";

            emailMessage.From.Add(new MailboxAddress("LabWeb", _emailConfig.From));
            emailMessage.To.Add(message.To);
            emailMessage.Subject = message.Subject;
            emailMessage.Body = bodyBuilder.ToMessageBody();

            return emailMessage;
        }
    }
}
