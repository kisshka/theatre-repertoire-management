using MimeKit;
using MailKit.Net.Smtp;
using System.Threading.Tasks;
using MimeKit;
using Microsoft.AspNetCore.Identity;
using Domain.Entities;
using DocumentFormat.OpenXml.Vml;
using Microsoft.Extensions.Options;

namespace TheatreManagement.Server
{
    public class EmailSender : IEmailSender<User>
    {
        private readonly EmailService _emailService;

        public EmailSender(EmailService emailService)
        {
            _emailService = emailService;
        }

        public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
            => Task.CompletedTask;

        public async Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
        {
            var subject = "Восстановление пароля";
            var userFullName = $"{user.Surname} {user.Name} {user.FatherName}";
            var body = $@"
            <h2>Здравствуйте, {user.UserName}!</h2>
            <p>Вы запросили восстановление пароля.</p>
            <p>Ваш код для сброса пароля:</p>
            <h3 style='background: #f0f0f0; padding: 10px; font-family: monospace;'>{resetCode}</h3>
            <p>Введите этот код на странице восстановления пароля.</p>
            <p>Если вы не запрашивали восстановление, просто проигнорируйте это письмо.</p>
        ";

            await _emailService.SendEmailAsync(email, subject, body);
            Console.WriteLine($"Письмо с кодом отправлено на {email}");
        }

        public async Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
        {
            var subject = "Восстановление пароля";
            var userFullName = $"{user.Surname} {user.Name} {user.FatherName}";
            var body = $@"
            <h2>Здравствуйте, {user.UserName}!</h2>
            <p>Вы запросили восстановление пароля.</p>
            <p>Перейдите по ссылке для сброса пароля:</p>
            <h3 style='background: #f0f0f0; padding: 10px; font-family: monospace;'>{resetLink}</h3>
            <p>Если вы не запрашивали восстановление, просто проигнорируйте это письмо.</p>
        ";

            await _emailService.SendEmailAsync(email, subject, body);
            Console.WriteLine($"Письмо с кодом отправлено на {email}");
        }
    }

    public class EmailService
    {
        private readonly EmailSettings _emailSettings;

        public EmailService(IOptions<EmailSettings> emailSettings)
        {
            _emailSettings = emailSettings.Value;
        }

        public async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(_emailSettings.SenderName, _emailSettings.SenderEmail));
            message.To.Add(new MailboxAddress("", toEmail));
            message.Subject = subject;
            message.Body = new TextPart("html") { Text = body };

            using var client = new SmtpClient();
            await client.ConnectAsync(_emailSettings.SmtpServer, _emailSettings.Port, true);
            await client.AuthenticateAsync(_emailSettings.SenderEmail, _emailSettings.Password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }

    public class EmailSettings
    {
        public string SmtpServer { get; set; }
        public int Port { get; set; }
        public string SenderEmail { get; set; }
        public string SenderName { get; set; }
        public string Password { get; set; }
    }
}