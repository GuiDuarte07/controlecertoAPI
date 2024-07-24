namespace Finantech.Services.Interfaces
{
    public interface IEmailService
    {
        public void SendEmail(List<string> emailsTo, string subject, string body);
    }
}
