using System.Net.Mail;
using System.Net;
using OTP.Interfaces;

namespace OTP.Services
{
    public class EmailService : IEmailService
    {
        private readonly IConfiguration _config;

        public EmailService(IConfiguration config)
        {
            _config = config;
        }

        public async Task SendOtpAsync(string toEmail, string otp)
        {
            var fromEmail = _config["EmailSettings:FromEmail"];
            var appPassword = _config["EmailSettings:AppPassword"];

            var smtpClient = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                Credentials = new NetworkCredential(fromEmail, appPassword),
                EnableSsl = true,
            };

            var message = new MailMessage(fromEmail, toEmail)
            {
                Subject = "Your OTP Code",
                Body = $"Your OTP is: {otp}",
                IsBodyHtml = false,
            };

            await smtpClient.SendMailAsync(message);
        }
    }
}
