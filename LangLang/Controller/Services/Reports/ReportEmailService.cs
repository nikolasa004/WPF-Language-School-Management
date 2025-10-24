using System.Threading.Tasks;

namespace LangLang.Controller.Services.Reports
{
    internal static class ReportEmailService
    {
        public static async Task SendReportEmail(byte[] pdfBytes, string reportName)
        {
            await EmailService.SendReportAsync(
                smtpHost: "localhost",
                smtpPort: 1025,
                fromName: "Language App",
                fromEmail: "no-reply@local.test",
                toName: "Principal",
                toEmail: "principal@gmail.com",
                subject: "Generated report",
                bodyText: "Hi! Here is your generated report!",
                pdfBytes: pdfBytes,
                pdfFileName: reportName
            );
        }
    }
}