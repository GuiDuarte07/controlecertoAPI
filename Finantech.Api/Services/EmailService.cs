using Finantech.Services.Interfaces;
using System.Net;
using System.Net.Mail;

namespace Finantech.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;

        public EmailService(IConfiguration configuration)
        {
            _configuration = configuration;
        }

        public void SendEmail(List<string> emailsTo, string subject, string body)
        {
            var mail = PrepareteMessage(emailsTo, subject, body);

            SendEmailBySmtp(mail);
        }

        private MailMessage PrepareteMessage(List<string> emailsTo, string subject, string body)
        {
            var mail = new MailMessage();
            mail.From = new MailAddress(_configuration.GetConnectionString("MailAddress")!, "Controle Certo");

            mail.Subject = subject;
            mail.Body = body;
            mail.IsBodyHtml = true;

            foreach(var email in emailsTo)
            {
                mail.To.Add(email);
            }

            return mail;
        }

        private void SendEmailBySmtp(MailMessage mail)
        {
            SmtpClient smtpClient = new SmtpClient();
            smtpClient.Host = "smtp.gmail.com";
            smtpClient.Port = 587;
            smtpClient.EnableSsl = true;
            smtpClient.Timeout = 5000;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(
                _configuration.GetConnectionString("MailAddress"),
                _configuration.GetConnectionString("MailPassword")
            );

            smtpClient.Send(mail);
            smtpClient.Dispose();

        }
    }
}
