using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Collections.Generic;
using System.IO;

namespace LangLang.Controller.Services
{
    internal class PDFService
    {
        public static byte[] GenerateCenteredReport(string title, IEnumerable<string> lines)
        {
            QuestPDF.Settings.License = LicenseType.Community;

            using var document = new MemoryStream();
            Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);


                    page.DefaultTextStyle(x => x.FontSize(12));

                    page.Content().AlignMiddle().AlignCenter().Column(col =>
                    {
                        col.Spacing(12);

                        col.Item().Text(title).FontSize(32).SemiBold().AlignCenter();

                        foreach (var line in lines)
                        {
                            col.Item().Text(line).FontSize(16).AlignCenter();
                        }
                    });
                });
            })
            .GeneratePdf(document);

            return document.ToArray();
        }
    }
}
