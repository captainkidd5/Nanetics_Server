using System.Net.Mail;
using System.Text.Encodings.Web;
using System.Web;
using Microsoft.AspNetCore.Mvc;
using SendGrid;
using SendGrid.Helpers.Mail;
using SendGrid.Helpers.Mail.Model;

namespace Api.DependencyInjections.Email
{
    public class EmailSender : IEmailSender
    {
        private readonly IConfiguration _config;

        //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

        public EmailSender(IConfiguration config)
        {
            _config = config;
        }


        public async Task<Response> SendEmail(string emailTo, string userName, string content)
        {


            var apiKey = Environment.GetEnvironmentVariable(_config.GetSection("SendGrid").GetSection("Key").Value);
            var client = new SendGridClient(apiKey);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("waiikipomm@gmail.com", "Waiiki"),
                Subject = "Brocab - Email Confirmation",
                PlainTextContent = content,
            };
            msg.AddTo(new EmailAddress(emailTo, userName));
            Response response = await client.SendEmailAsync(msg);

            return response;

        }

        public async Task<Response> SendEmailConfirmation(string emailTo, string userId, string code)
        {

            string sendGrid = _config.GetValue<string>("SendGrid:Key");
            var client = new SendGridClient(sendGrid);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("waiikipomm@gmail.com", "Waiiki"),
                Subject = "Api - Email Confirmation",
                HtmlContent = $"Here is your confirmation code: {code}.\n" +
                        $"This code will expire in 12 hours."
            };
            msg.AddTo(new EmailAddress(emailTo, emailTo));
            Response response = await client.SendEmailAsync(msg);

            return response;
        }

        public async Task<Response> SendPasswordResetEmail(string emailTo, string userId, string passwordResetToken)
        {
            string sendGrid = _config.GetValue<string>("SendGrid:Key");
            var client = new SendGridClient(sendGrid);
            var msg = new SendGridMessage()
            {
                From = new EmailAddress("waiikipomm@gmail.com", "Waiiki"),
                Subject = "Api - Reset Password",
                HtmlContent = $"<p>Here is your password reset link:</p> <a href='http://localhost:3000/reset-password?email={emailTo}&token={HttpUtility.UrlEncode(passwordResetToken)}'>Reset Password</a>.\n" +
                        $"<p>This code will expire in 12 hours.</p><strong> If you did not request a password change, you should ignore this email.</strong>"
            };
            msg.AddTo(new EmailAddress(emailTo, emailTo));
            Response response = await client.SendEmailAsync(msg);

            return response;
        }
    }
}
