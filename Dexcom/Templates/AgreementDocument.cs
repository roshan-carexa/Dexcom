using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Dexcom.Templates;

public class AgreementDocument : IDocument
{
    public string UserName { get; }

    public AgreementDocument(string username)
    {
        UserName = username;
    }

    public void Compose(IDocumentContainer container)
    {
        container.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(2, Unit.Centimetre);
            page.PageColor("#F8FAFC");

            page.DefaultTextStyle(x =>
                x.FontSize(12)
                 .FontFamily("Arial"));

            // HEADER
            page.Header()
                .PaddingBottom(20)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("SERVICE AGREEMENT")
                                .FontSize(28)
                                .Bold()
                                .FontColor("#1E3A8A");

                            column.Item()
                                .PaddingTop(5)
                                .Text($"Generated on {DateTime.Now:dd MMMM yyyy}")
                                .FontSize(10)
                                .FontColor(Colors.Grey.Darken1);
                        });

                    row.ConstantItem(80)
                        .Height(80)
                        .Background("#1E3A8A")
                        .AlignCenter()
                        .AlignMiddle()
                        .Text("AG")
                        .FontColor(Colors.White)
                        .FontSize(30)
                        .Bold();
                });

            // CONTENT
            page.Content()
                .PaddingVertical(10)
                .Column(column =>
                {
                    // Agreement Card
                    column.Item()
                        .Background(Colors.White)
                        .Border(1)
                        .BorderColor("#E2E8F0")
                        .Padding(25)
                        .Column(content =>
                        {
                            content.Item()
                                .Text("Agreement Terms")
                                .FontSize(22)
                                .Bold()
                                .FontColor("#0F172A");

                            content.Item()
                                .PaddingTop(15)
                                .Text(text =>
                                {
                                    text.Span("This agreement is entered into between ")
                                        .FontColor("#475569");

                                    text.Span(UserName)
                                        .SemiBold()
                                        .FontColor("#1D4ED8");

                                    text.Span(" and the organization. By signing this document, the participant agrees to all terms and conditions outlined below.")
                                        .FontColor("#475569");
                                });

                            content.Item()
                                .PaddingTop(20)
                                .Text(Placeholders.LoremIpsum())
                                .LineHeight(1.6f)
                                .FontColor("#334155");

                            content.Item()
                                .PaddingTop(20)
                                .Text(Placeholders.LoremIpsum())
                                .LineHeight(1.6f)
                                .FontColor("#334155");
                        });

                    // Signature Section
                    column.Item()
                        .PaddingTop(40)
                        .AlignRight()
                        .Width(260)
                        .Background(Colors.White)
                        .Border(1)
                        .BorderColor("#CBD5E1")
                        .Padding(20)
                        .Column(signature =>
                        {
                            signature.Item()
                                .Text("DIGITAL SIGNATURE")
                                .FontSize(10)
                                .SemiBold()
                                .LetterSpacing(1.5f)
                                .FontColor("#64748B");

                            signature.Item()
                                .PaddingTop(15)
                                .LineHorizontal(1)
                                .LineColor("#94A3B8");

                            signature.Item()
                                .PaddingTop(10)
                                .AlignCenter()
                                .Text(UserName)
                                .FontSize(22)
                                .Italic()
                                .SemiBold()
                                .FontColor("#0F172A");

                            signature.Item()
                                .PaddingTop(10)
                                .AlignCenter()
                                .Text($"Electronically signed on {DateTime.Now:dd MMM yyyy}")
                                .FontSize(10)
                                .FontColor("#64748B");
                        });
                });
            
            // FOOTER
            page.Footer()
                .PaddingTop(15)
                .AlignCenter()
                .Text(text =>
                {
                    text.Span("Page ");
                    text.CurrentPageNumber();
                    text.Span(" of ");
                    text.TotalPages();
                });
        });
    }
}