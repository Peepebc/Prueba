using ProyectoGrado.Models;
namespace ProyectoGrado.Services
{
    public interface IEmailService
    {

        void SendEmail(EmailDTO request);
    }
}
