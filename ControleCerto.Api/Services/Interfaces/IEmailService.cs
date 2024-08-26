using ControleCerto.DTOs.User;
using ControleCerto.Models.Entities;

namespace ControleCerto.Services.Interfaces
{
    public interface IEmailService
    {
        public void SendEmail(List<string> emailsTo, string subject, string body);
        public void SendConfirmationEmail(InfoUserResponse email);
        public void SendForgotPasswordEmail(string email);
    }
}
