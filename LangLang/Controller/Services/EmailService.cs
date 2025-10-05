using MailKit.Security;
using MimeKit;
using System.Threading.Tasks;
using MailKit.Net.Smtp;

namespace LangLang.Controller.Services
{
    public class EmailService
    {
        public static async Task SendReportAsync(
        string smtpHost, int smtpPort,
        string fromName, string fromEmail,
        string toName, string toEmail,
        string subject, string bodyText,
        byte[] pdfBytes, string pdfFileName = "report.pdf")
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(fromName, fromEmail));
            message.To.Add(new MailboxAddress(toName, toEmail));
            message.Subject = subject;

            var builder = new BodyBuilder
            {
                TextBody = bodyText
            };

            builder.Attachments.Add(pdfFileName, pdfBytes, new ContentType("application", "pdf"));
            message.Body = builder.ToMessageBody();

            using var client = new SmtpClient();

            await client.ConnectAsync(smtpHost, smtpPort, SecureSocketOptions.None);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
    }
}
