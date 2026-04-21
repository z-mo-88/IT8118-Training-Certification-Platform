using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace TrainingSystem.MVC.Services
{
    public class CertificatePdfService
    {
        public byte[] Generate(
            string traineeName,
            string trackName,
            string reference,
            string issuedDate,
            string instructorName,
            string duration)
        {
            var imagePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "trainingsystemicon.png");
            var imageData = File.Exists(imagePath) ? File.ReadAllBytes(imagePath) : null;

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.Background("#fdfaf3");

                    page.Content()
                        .Border(8)
                        .BorderColor("#c9a646")
                        .Padding(30)
                        .Column(col =>
                        {
                            col.Spacing(20);

                            if (imageData != null)
                            {
                                col.Item()
                                   .AlignCenter()
                                   .Height(60)
                                   .Image(imageData);
                            }

                            col.Item().AlignCenter().Text("CERTIFICATE")
                                .FontSize(42).Bold().FontColor("#0a1f5c");

                            col.Item().AlignCenter().Text("OF COMPLETION")
                                .FontSize(18).SemiBold();

                            col.Item().PaddingTop(15).AlignCenter()
                                .Text("This certifies that")
                                .FontSize(14);

                            col.Item().AlignCenter().Text(traineeName)
                                .FontSize(32).Bold().FontColor("#0a1f5c");

                            col.Item().AlignCenter().Text("has successfully completed")
                                .FontSize(14);

                            col.Item().AlignCenter().Text(trackName)
                                .FontSize(26).Bold().FontColor("#c9a646");

                            col.Item().PaddingTop(10).AlignCenter()
                                .Text($"Instructor: {instructorName}")
                                .FontSize(12);

                            col.Item().AlignCenter()
                                .Text($"Duration: {duration}")
                                .FontSize(12);

                            col.Item().PaddingTop(25).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("Date Issued").FontSize(12);
                                    c.Item().Text(issuedDate).Bold();
                                });

                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text("Certificate ID").FontSize(12);
                                    c.Item().Text(reference).Bold();
                                });
                            });

                            col.Item().PaddingTop(40).Row(row =>
                            {
                                row.RelativeItem().Column(c =>
                                {
                                    c.Item().Text("______________________");
                                    c.Item().Text("Program Director").FontSize(12);
                                });

                                row.RelativeItem().AlignRight().Column(c =>
                                {
                                    c.Item().Text("Training System Institute");
                                    c.Item().Text("Official Certificate").FontSize(12);
                                });
                            });

                            col.Item().PaddingTop(10).AlignCenter()
                                .Text("Verify this certificate using the Certificate ID.")
                                .FontSize(10).Italic().FontColor(Colors.Grey.Medium);
                        });
                });
            }).GeneratePdf();
        }
    }
}