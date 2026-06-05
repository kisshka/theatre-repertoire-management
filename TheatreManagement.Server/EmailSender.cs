using Domain.Entities;
using Microsoft.AspNetCore.Identity;

namespace TheatreManagement.Server
{
    public class MyEmailSender : IEmailSender<User>
    {
        public Task SendConfirmationLinkAsync(User user, string email, string confirmationLink)
            => Task.CompletedTask;

        public Task SendPasswordResetLinkAsync(User user, string email, string resetLink)
        {
            // Здесь будет ваша логика отправки письма
            Console.WriteLine($"Ссылка для сброса пароля: {resetLink}");
            return Task.CompletedTask;
        }

        public Task SendPasswordResetCodeAsync(User user, string email, string resetCode)
            => Task.CompletedTask;
    }
}
