using SendGrid;

namespace SilverMenu.DependencyInjections.Email
{
    public interface IEmailSender
    {
        Task<Response> SendEmail(string emailTo, string userName, string content);
        Task<Response> SendEmailConfirmation(string emailTo, string userId, string code);
        Task<Response> SendPasswordResetEmail(string emailTo, string userId, string code);

    }
}
