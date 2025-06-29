namespace Calmative.Server.API.Services
{
    public class EmailService : IEmailService
    {
        private readonly ILogger<EmailService> _logger;
        private readonly IConfiguration _configuration;

        public EmailService(ILogger<EmailService> logger, IConfiguration configuration)
        {
            _logger = logger;
            _configuration = configuration;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            var smtpHost = _configuration["EmailSettings:SmtpHost"];
            var smtpPort = _configuration.GetValue<int>("EmailSettings:SmtpPort", 0);
            var username = _configuration["EmailSettings:Username"];
            var password = _configuration["EmailSettings:Password"];
            var from = _configuration["EmailSettings:From"] ?? username;

            if (string.IsNullOrWhiteSpace(smtpHost) || smtpPort == 0)
            {
                // Fallback to logging only
                _logger.LogInformation("--- NEW EMAIL (LOG ONLY) ---");
                _logger.LogInformation("To: {To}", to);
                _logger.LogInformation("Subject: {Subject}", subject);
                _logger.LogInformation("Body: {Body}", body);
                _logger.LogInformation("--- END EMAIL ---");
                return;
            }

            try
            {
                using var client = new System.Net.Mail.SmtpClient(smtpHost, smtpPort)
                {
                    EnableSsl = _configuration.GetValue<bool>("EmailSettings:EnableSsl", true),
                    Credentials = new System.Net.NetworkCredential(username, password)
                };

                var mail = new System.Net.Mail.MailMessage(from, to, subject, body) { IsBodyHtml = true };
                await client.SendMailAsync(mail);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to send email via SMTP. Falling back to log.");
                _logger.LogInformation("--- NEW EMAIL (LOG ONLY) ---");
                _logger.LogInformation("To: {To}", to);
                _logger.LogInformation("Subject: {Subject}", subject);
                _logger.LogInformation("Body: {Body}", body);
                _logger.LogInformation("--- END EMAIL ---");
            }
        }
    }
} 