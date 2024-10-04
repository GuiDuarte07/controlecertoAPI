using ControleCerto.DTOs.User;
using ControleCerto.Models.Entities;
using ControleCerto.Services.Interfaces;
using ControleCerto.Utils;
using System.Net;
using System.Net.Mail;

namespace ControleCerto.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _configuration;
        private readonly ICacheService _cacheService;

        public EmailService(IConfiguration configuration, ICacheService cacheService)
        {
            _configuration = configuration;
            _cacheService = cacheService;
        }

        public void SendEmail(List<string> emailsTo, string subject, string body)
        {
            var mail = PrepareteMessage(emailsTo, subject, body);

            SendEmailBySmtp(mail);
        }

        public void SendConfirmationEmail(InfoUserResponse user)
        {
            string confirmEmailToken = RandomGenerate.Generate32BytesToken();

            string frontEndHost = _configuration.GetConnectionString("WebSiteUrl")!;
            string frontEndUrlPath = $"{frontEndHost}/confirm-email/{confirmEmailToken}";

            _cacheService.SetConfirmEmailTokenAsync(user.Email, confirmEmailToken);

            var htmlBody = $@"
            <html>
            <head>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f4f4f9;
                        color: #333;
                        margin: 0;
                        padding: 0;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #fff;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}
                    .header {{
                        text-align: center;
                        padding: 10px 0;
                        background-color: #4CAF50;
                        color: #fff;
                    }}
                    .content {{
                        padding: 20px;
                    }}
                    .button {{
                        display: block;
                        width: 200px;
                        margin: 20px auto;
                        padding: 10px 20px;
                        background-color: #4CAF50;
                        color: #fff;
                        text-align: center;
                        text-decoration: none;
                        border-radius: 5px;
                    }}
                    .footer {{
                        text-align: center;
                        padding: 10px 0;
                        font-size: 12px;
                        color: #999;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Confirmação de Email</h1>
                    </div>
                    <div class='content'>
                        <p>Olá {user.Name},</p>
                        <p>Obrigado por se registrar no Controle Certo! Por favor, confirme seu email clicando no botão abaixo:</p>
                        <a class='button' target='_blank' href='{frontEndUrlPath}'>Confirmar Email</a>
                        <p>Se você não se registrou em nosso site, por favor ignore este email.</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2024 Controle Certo. Todos os direitos reservados.</p>
                    </div>
                </div>
            </body>
            </html>";

            SendEmail([user.Email], "Confirme seu E-mail na Controle Certo", htmlBody);
        }

        public void SendForgotPasswordEmail(string email)
        {
            string forgotPasswordToken = RandomGenerate.Generate32BytesToken();

            string frontEndHost = _configuration.GetConnectionString("WebSiteUrl")!;
            string frontEndUrlPath = $"{frontEndHost}/forgot-password/{forgotPasswordToken}";

            _cacheService.SetForgotPasswordTokenAsync(email, forgotPasswordToken);

            var htmlBody = $@"
            <html>
            <head>
                <style>
                    body {{
                        font-family: Arial, sans-serif;
                        background-color: #f4f4f9;
                        color: #333;
                        margin: 0;
                        padding: 0;
                    }}
                    .container {{
                        max-width: 600px;
                        margin: 0 auto;
                        padding: 20px;
                        background-color: #fff;
                        box-shadow: 0 0 10px rgba(0, 0, 0, 0.1);
                    }}
                    .header {{
                        text-align: center;
                        padding: 10px 0;
                        background-color: #4CAF50;
                        color: #fff;
                    }}
                    .content {{
                        padding: 20px;
                    }}
                    .button {{
                        display: block;
                        width: 200px;
                        margin: 20px auto;
                        padding: 10px 20px;
                        background-color: #4CAF50;
                        color: #fff;
                        text-align: center;
                        text-decoration: none;
                        border-radius: 5px;
                    }}
                    .footer {{
                        text-align: center;
                        padding: 10px 0;
                        font-size: 12px;
                        color: #999;
                    }}
                </style>
            </head>
            <body>
                <div class='container'>
                    <div class='header'>
                        <h1>Redefinir senha</h1>
                    </div>
                    <div class='content'>
                        <p>Olá!</p>
                        <p>Uma solicitação de redefinição de senha foi enviada para o seu email.</p>
                        <a class='button' target='_blank' href='{frontEndUrlPath}'>Redefinir Senha</a>
                        <p>ATENÇÃO: Caso você não tenha feito essa solicitação, por favor, ignorar esse E-mail.</p>
                    </div>
                    <div class='footer'>
                        <p>&copy; 2024 Controle Certo. Todos os direitos reservados.</p>
                    </div>
                </div>
            </body>
            </html>
            ";

            SendEmail([email], "Redefinição de senha - Controle Certo", htmlBody);
        }

        private MailMessage PrepareteMessage(List<string> emailsTo, string subject, string body)
        {
            
            var mail = new MailMessage();
            mail.From = new MailAddress(_configuration.GetSection("MailConfig:Address").Value!, "Controle Certo");

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
            smtpClient.Timeout = 100000;
            smtpClient.UseDefaultCredentials = false;
            smtpClient.Credentials = new NetworkCredential(
                _configuration.GetSection("MailConfig:Address").Value!,
                _configuration.GetSection("MailConfig:Password").Value!
            );

            try
            {
                smtpClient.Send(mail);
                
            } catch(Exception error)
            {
                Console.WriteLine(error);
            } 
            finally
            {
                smtpClient.Dispose();
            }

            

        }
    }
}
