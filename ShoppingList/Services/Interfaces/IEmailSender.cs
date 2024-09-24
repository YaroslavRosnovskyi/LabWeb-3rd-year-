using LabWeb.DTOs;
using MimeKit;

namespace LabWeb.Services.Interfaces;

public interface IEmailSender
{
    Task SendEmail(Message message);
}