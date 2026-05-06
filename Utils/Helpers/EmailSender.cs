using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Options;
using System.Net;
using System.Net.Mail;
using Utils.Models;

namespace Utils.Helpers
{
    public class EmailSender : IEmailSender
    {
        private readonly EmailSettings settings;

        public EmailSender(IOptions<EmailSettings> options)
        {
            settings = options.Value;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // BURASI ÇOK KRİTİK: Hatayı ekranda patlatması için try-catch ekliyoruz
            try
            {
                using var client = new SmtpClient(settings.Host, settings.Port)
                {
                    Credentials = new NetworkCredential(settings.UserName, settings.Password),
                    EnableSsl = settings.EnableSSL,
                    DeliveryMethod = SmtpDeliveryMethod.Network,
                    UseDefaultCredentials = false,
                    Timeout = 10000 // 10 saniye sonra vazgeç
                };

                using var mailMessage = new MailMessage
                {
                    From = new MailAddress(settings.UserName, settings.DisplayName),
                    Subject = subject,
                    Body = htmlMessage,
                    IsBodyHtml = true
                };

                mailMessage.To.Add(email);

                await client.SendMailAsync(mailMessage);

                // Gönderim başarılıysa konsola yaz
                Console.WriteLine(">>>>>> E-POSTA BAŞARIYLA GÖNDERİLDİ: " + email);
            }
            catch (Exception ex)
            {
                // HATA VARSA BURADA DURDURUP HATAYI GÖRECEĞİZ
                Console.WriteLine("!!!!!! E-POSTA HATASI !!!!!!");
                Console.WriteLine("Mesaj: " + ex.Message);
                if (ex.InnerException != null)
                    Console.WriteLine("İç Hata: " + ex.InnerException.Message);

                // Hatayı fırlatıyoruz ki uygulama "Başarılı" deyip geçmesin, hata sayfasını görelim
                throw new Exception($"SMTP Gönderim Hatası: {ex.Message}", ex);
            }
        }
    }
}