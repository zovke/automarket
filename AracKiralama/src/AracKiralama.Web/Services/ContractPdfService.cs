using AracKiralama.Web.Data;
using AracKiralama.Web.Helpers;
using AracKiralama.Web.Models.Entities;
using AracKiralama.Web.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace AracKiralama.Web.Services;

public interface IContractPdfService
{
    Task<(byte[] Pdf, string FileName)?> GenerateAsync(int reservationId);
}

/// <summary>
/// Kiralama sözleşmesini PDF olarak üretir (QuestPDF kütüphanesi, Community lisansı).
/// Sayfa düzeni C# kodu ile tanımlanır: Header → Content → Footer.
/// </summary>
public class ContractPdfService(AppDbContext db, IOptions<SiteOptions> site) : IContractPdfService
{
    private const string Primary = "#2747D6";
    private const string Muted = "#6B7280";
    private const string LineColor = "#E5E7EB";

    public async Task<(byte[] Pdf, string FileName)?> GenerateAsync(int reservationId)
    {
        var r = await db.Reservations
            .Include(x => x.Customer)
            .Include(x => x.Vehicle).ThenInclude(v => v!.Brand)
            .Include(x => x.PickupBranch).Include(x => x.ReturnBranch)
            .Include(x => x.Extras).ThenInclude(e => e.Extra)
            .Include(x => x.DamageReports)
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Id == reservationId);
        if (r is null) return null;

        var s = site.Value;
        var pdf = Document.Create(doc => doc.Page(page =>
        {
            page.Size(PageSizes.A4);
            page.Margin(36);
            page.DefaultTextStyle(t => t.FontSize(9.5f).FontColor("#111827"));

            page.Header().Row(row =>
            {
                row.RelativeItem().Column(c =>
                {
                    c.Item().Text(t =>
                    {
                        t.Span(s.Name).FontSize(22).Bold().FontColor(Primary);
                        t.Span($"  {s.Tagline}").FontSize(11).FontColor(Muted);
                    });
                    c.Item().Text($"{s.Address}").FontColor(Muted);
                    c.Item().Text($"{s.Phone}  ·  {s.Email}").FontColor(Muted);
                });
                row.ConstantItem(170).AlignRight().Column(c =>
                {
                    c.Item().AlignRight().Text("ARAÇ KİRALAMA SÖZLEŞMESİ").Bold().FontSize(11);
                    c.Item().AlignRight().Text(r.Code).FontSize(16).Bold().FontColor(Primary);
                    c.Item().AlignRight().Text($"Düzenlenme: {Fmt.Date(DateTime.Now)}").FontColor(Muted);
                    c.Item().AlignRight().Text($"Durum: {r.Status.GetDisplayName()}").FontColor(Muted);
                });
            });

            page.Content().PaddingTop(16).Column(col =>
            {
                col.Spacing(14);

                col.Item().Row(row =>
                {
                    row.Spacing(14);
                    row.RelativeItem().Element(Box).Column(c =>
                    {
                        SectionTitle(c, "KİRACI");
                        Field(c, "Ad Soyad", r.Customer!.FullName);
                        Field(c, "T.C. Kimlik No", r.Customer.TcNo ?? "-");
                        Field(c, "Telefon", r.Customer.PhoneNumber ?? "-");
                        Field(c, "E-posta", r.Customer.Email ?? "-");
                        Field(c, "Ehliyet No", r.Customer.LicenseNumber ?? "-");
                        Field(c, "Ehliyet Tarihi", r.Customer.LicenseIssueDate is { } l ? Fmt.Date(l) : "-");
                    });
                    row.RelativeItem().Element(Box).Column(c =>
                    {
                        SectionTitle(c, "ARAÇ");
                        Field(c, "Araç", $"{r.Vehicle!.DisplayName} ({r.Vehicle.Year})");
                        Field(c, "Plaka", r.Vehicle.Plate);
                        Field(c, "Yakıt / Vites", $"{r.Vehicle.Fuel.GetDisplayName()} / {r.Vehicle.Transmission.GetDisplayName()}");
                        Field(c, "Renk", r.Vehicle.Color);
                        Field(c, "Teslim Km", r.StartKm is { } sk ? Fmt.Number(sk) : "-");
                        Field(c, "İade Km", r.EndKm is { } ek ? Fmt.Number(ek) : "-");
                    });
                });

                col.Item().Element(Box).Row(row =>
                {
                    row.RelativeItem().Column(c =>
                    {
                        SectionTitle(c, "ALIŞ");
                        c.Item().Text(r.PickupBranch!.Name).Bold();
                        c.Item().Text(Fmt.DateTime(r.StartDate));
                        c.Item().Text($"Yakıt: {r.FuelLevelOut?.GetDisplayName() ?? "-"}").FontColor(Muted);
                    });
                    row.ConstantItem(40).AlignCenter().AlignMiddle().Text("→").FontSize(18).FontColor(Muted);
                    row.RelativeItem().Column(c =>
                    {
                        SectionTitle(c, "İADE");
                        c.Item().Text(r.ReturnBranch!.Name).Bold();
                        c.Item().Text(Fmt.DateTime(r.EndDate));
                        c.Item().Text($"Gerçekleşen: {Fmt.DateTime(r.ActualReturnDate)}  ·  Yakıt: {r.FuelLevelIn?.GetDisplayName() ?? "-"}").FontColor(Muted);
                    });
                });

                col.Item().Element(Box).Column(c =>
                {
                    SectionTitle(c, "ÜCRET DÖKÜMÜ");
                    c.Item().Table(t =>
                    {
                        t.ColumnsDefinition(cd => { cd.RelativeColumn(4); cd.RelativeColumn(2); cd.RelativeColumn(2); });
                        void AddLine(string name, string detail, decimal amount, bool negative = false)
                        {
                            t.Cell().BorderBottom(0.5f).BorderColor(LineColor).PaddingVertical(4).Text(name);
                            t.Cell().BorderBottom(0.5f).BorderColor(LineColor).PaddingVertical(4).Text(detail).FontColor(Muted);
                            t.Cell().BorderBottom(0.5f).BorderColor(LineColor).PaddingVertical(4).AlignRight()
                                .Text((negative ? "- " : "") + Fmt.MoneyExact(amount));
                        }

                        AddLine("Kira bedeli", $"{r.TotalDays} gün × {Fmt.Money(r.DailyPrice)}", r.BaseTotal);
                        if (r.DiscountAmount > 0) AddLine("Uzun dönem indirimi", "", r.DiscountAmount, negative: true);
                        foreach (var e in r.Extras)
                            AddLine(e.Extra!.Name, $"{r.TotalDays} gün × {Fmt.Money(e.DailyPrice)}", e.Total);
                        if (r.OneWayFee > 0) AddLine("Farklı şubeye iade", "", r.OneWayFee);
                        if (r.LateFee > 0) AddLine("Geç iade ücreti", "", r.LateFee);
                        if (r.FuelFee > 0) AddLine("Eksik yakıt ücreti", "", r.FuelFee);
                        if (r.DamageFee > 0) AddLine("Hasar bedeli", string.Join(", ", r.DamageReports.Select(d => d.Description)), r.DamageFee);
                        if (r.CancellationFee > 0) AddLine("İptal ücreti", "", r.CancellationFee);

                        t.Cell().ColumnSpan(2).PaddingTop(6).Text("GENEL TOPLAM (KDV dahil)").Bold();
                        t.Cell().PaddingTop(6).AlignRight().Text(Fmt.MoneyExact(r.TotalPrice)).Bold().FontSize(12).FontColor(Primary);
                        t.Cell().ColumnSpan(2).Text("Depozito (iadede geri ödenir)").FontColor(Muted);
                        t.Cell().AlignRight().Text(Fmt.MoneyExact(r.Deposit)).FontColor(Muted);
                    });
                });

                col.Item().Column(c =>
                {
                    SectionTitle(c, "GENEL ŞARTLAR");
                    string[] terms =
                    [
                        "Araç, teslim alındığı yakıt seviyesiyle iade edilmelidir; eksik her çeyrek depo ücretlendirilir.",
                        "Planlanan iade saatinden sonra başlayan her gün için günlük ücretin 1,5 katı geç iade bedeli uygulanır.",
                        "Kiracı, trafik cezalarından ve kira süresince oluşan hasarlardan (kasko kapsamı dışındakiler) sorumludur.",
                        "Araç, sözleşmede adı geçmeyen kişilerce kullanılamaz ve yurt dışına çıkarılamaz.",
                        "Depozito, araç hasarsız ve eksiksiz iade edildiğinde kiracıya geri ödenir.",
                    ];
                    foreach (var (term, i) in terms.Select((x, i) => (x, i + 1)))
                        c.Item().PaddingBottom(2).Text($"{i}. {term}").FontColor(Muted);
                });

                col.Item().PaddingTop(20).Row(row =>
                {
                    row.Spacing(40);
                    row.RelativeItem().Column(c => Signature(c, "KİRAYA VEREN", s.Name));
                    row.RelativeItem().Column(c => Signature(c, "KİRACI", r.Customer!.FullName));
                });
            });

            page.Footer().AlignCenter().Text(t =>
            {
                t.DefaultTextStyle(x => x.FontColor(Muted).FontSize(8));
                t.Span($"{s.Name} {s.Tagline} · Bu belge {Fmt.DateTime(DateTime.Now)} tarihinde sistem tarafından üretilmiştir · Sayfa ");
                t.CurrentPageNumber();
            });
        })).GeneratePdf();

        return (pdf, $"Sozlesme-{r.Code}.pdf");
    }

    private static IContainer Box(IContainer c) => c.Border(0.75f).BorderColor(LineColor).CornerRadius(6).Padding(10);

    private static void SectionTitle(ColumnDescriptor c, string title)
        => c.Item().PaddingBottom(4).Text(title).FontSize(8).Bold().FontColor(Primary).LetterSpacing(0.05f);

    private static void Field(ColumnDescriptor c, string label, string value)
        => c.Item().Row(r =>
        {
            r.ConstantItem(90).Text(label).FontColor(Muted);
            r.RelativeItem().Text(value);
        });

    private static void Signature(ColumnDescriptor c, string title, string name)
    {
        c.Item().Height(40);
        c.Item().BorderTop(0.75f).BorderColor("#9CA3AF").PaddingTop(4).Text(title).Bold().FontSize(8);
        c.Item().Text(name).FontColor(Muted);
    }
}
