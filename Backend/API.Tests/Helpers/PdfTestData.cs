using QuestPDF.Fluent;
using QuestPDF.Infrastructure;

namespace API.Tests.Helpers;

public static class PdfTestData
{
    static PdfTestData()
    {
        QuestPDF.Settings.License = LicenseType.Community;
    }

    public static byte[] CreateCvPdfBytes() =>
        Document.Create(doc =>
        {
            doc.Page(page =>
            {
                page.Content().Text("Senior Software Engineer with C# and .NET experience for CV extraction.");
            });
        }).GeneratePdf();
}
