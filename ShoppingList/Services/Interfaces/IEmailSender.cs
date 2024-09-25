using LabWeb.DTOs.ServiceBusDTO;
using MimeKit;

namespace LabWeb.Services.Interfaces;

public interface IEmailSender
{
    Task SendEmail(Message message);
}