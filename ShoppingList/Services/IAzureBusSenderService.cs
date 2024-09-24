using LabWeb.DTOs;

namespace LabWeb.Services
{
    public interface IAzureBusSenderService
    {
        Task Send(Message message);
    }
}