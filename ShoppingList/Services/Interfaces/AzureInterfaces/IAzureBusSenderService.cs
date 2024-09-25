using LabWeb.DTOs.ServiceBusDTO;

namespace LabWeb.Services.Interfaces.AzureInterfaces
{
    public interface IAzureBusSenderService
    {
        Task Send(Message message);
    }
}