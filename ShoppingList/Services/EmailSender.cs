using System.Net;
using LabWeb.Services.Interfaces;
using LabWeb.SettingOptions;
using Microsoft.Extensions.Options;
using MimeKit;
using MailKit.Net.Smtp;
using LabWeb.DTOs.ServiceBusDTO;

namespace LabWeb.Services;

public abstract class EmailSender : IEmailSender
{
    private readonly EmailConfiguration _emailConfig;
    private readonly IConfiguration _configuration;
    public EmailSender(IOptions<EmailConfiguration> emailConfig, IConfiguration configuration)
    {
        _emailConfig = emailConfig.Value;
        _configuration = configuration;
    }

    public async Task SendEmail(Message message)
    {
        var emailMessage = CreateEmailMessage(message);
        await Send(emailMessage);
    }

    protected abstract MimeMessage CreateEmailMessage(Message message);


    public async Task Send(MimeMessage mailMessage)
    {
        using var client = new SmtpClient();
        try
        {
            client.Connect(_emailConfig.SmtpServer, _emailConfig.Port, true);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            client.Authenticate(_emailConfig.UserName, _configuration["email-sender-password"]);

            await client.SendAsync(mailMessage);
        }
        finally
        {
            client.Disconnect(true);
            client.Dispose();
        }
    }


    
}