using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Utils.Models;
using Microsoft.Extensions.Logging; // ✅ Logger eklendi

namespace Utils.Helpers
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings settings;
        private readonly ILogger<EmailSender> _logger; // ✅ Console.WriteLine yerine Logger kullanımı profesyoneldir

        public EmailSender(IOptions<EmailSettings> options, ILogger<EmailSender> logger)
        {
            settings = options.Value;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // E-posta adresi boşsa boşuna işlem yapma
            if (string.IsNullOrEmpty(email)) return;

            try
            {
                string safeDisplayName = string.IsNullOrWhiteSpace(settings.DisplayName) || settings.DisplayName.Contains("@")
                    ? "StayHub Bildirim"
                    : settings.DisplayName;

                using var client = new SmtpClient(settings.Host, settings.Port)
                {
                    Credentials = new NetworkCredential(settings.UserName, settings.Password),
                    EnableSsl = settings.EnableSSL,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 20000 // ✅ 10 saniye bazen SMTP sunucuları için kısa gelebilir, 20 yaptık.
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(settings.UserName, safeDisplayName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);

                _logger.LogInformation(">>>>>> E-POSTA BAŞARIYLA GÖNDERİLDİ: {Email}", email);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "!!!!!! E-POSTA GÖNDERİM HATASI: {Email} !!!!!!", email);

                // ⚠️ KRİTİK: Buradaki 'throw'u kaldırıyoruz veya kontrollü fırlatıyoruz. 
                // Çünkü mail gitmedi diye rezervasyonun kaydedilmemesini istemeyiz.
                // Log tutmak yeterlidir.
            }
        }
    }
}