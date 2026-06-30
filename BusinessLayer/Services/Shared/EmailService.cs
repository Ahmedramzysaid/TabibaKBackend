using DomainLayer.Interfaces;
using MailKit.Net.Smtp;
using MailKit.Security;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using MimeKit;

namespace BusinessLayer.Services;

public class EmailService : IEmailService
{
    private readonly IConfiguration _configuration;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration configuration, ILogger<EmailService> logger)
    {
        _configuration = configuration;
        _logger = logger;
    }

    public async Task<bool> SendEmailAsync(string toEmail, string subject, string body, bool isHtml = true)
    {
        var host = _configuration["Email:SmtpHost"];
        var port = _configuration.GetValue<int>("Email:SmtpPort", 587);
        var user = _configuration["Email:SmtpUser"];
        var password = _configuration["Email:SmtpPassword"];
        var fromEmail = _configuration["Email:FromEmail"] ?? user;
        var fromName = _configuration["Email:FromName"] ?? "Tabibak Clinic";

        if (string.IsNullOrEmpty(host) || string.IsNullOrEmpty(toEmail))
        {
            _logger.LogWarning("Email not sent: SmtpHost or ToEmail missing. Configure Email section in appsettings.");
            return false;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(MailboxAddress.Parse(toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder();
            if (isHtml)
                builder.HtmlBody = body;
            else
                builder.TextBody = body;
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();
            var useSsl = _configuration.GetValue<bool>("Email:UseSsl", true);
            var secureSocketOptions = useSsl ? SecureSocketOptions.StartTls : SecureSocketOptions.None;
            await client.ConnectAsync(host, port, secureSocketOptions);

            if (!string.IsNullOrEmpty(user) && !string.IsNullOrEmpty(password))
                await client.AuthenticateAsync(user, password);

            await client.SendAsync(message);
            await client.DisconnectAsync(true);
            _logger.LogInformation("Email sent to {ToEmail}, subject: {Subject}", toEmail, subject);
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {ToEmail}", toEmail);
            return false;
        }
    }
}
