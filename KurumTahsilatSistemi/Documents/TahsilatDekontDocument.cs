using System.Globalization;
using KurumTahsilatSistemi.Models;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace KurumTahsilatSistemi.Documents
{
    public class TahsilatDekontDocument : IDocument
    {
        private readonly Tahsilat _tahsilat;

        public TahsilatDekontDocument(
            Tahsilat tahsilat)
        {
            _tahsilat = tahsilat;
        }

        public DocumentMetadata GetMetadata()
        {
            return DocumentMetadata.Default;
        }

        public void Compose(
            IDocumentContainer container)
        {
            container.Page(page =>
            {
                page.Size(PageSizes.A4);
                page.Margin(35);

                page.DefaultTextStyle(x =>
                    x.FontFamily(Fonts.Arial)
                     .FontSize(10));

                page.Header()
                    .Element(HeaderOlustur);

                page.Content()
                    .PaddingVertical(20)
                    .Element(IcerikOlustur);

                page.Footer()
                    .Element(FooterOlustur);
            });
        }

        private void HeaderOlustur(
            IContainer container)
        {
            container
                .BorderBottom(2)
                .BorderColor("#123B6D")
                .PaddingBottom(15)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Column(column =>
                        {
                            column.Item()
                                .Text("KURUM TAHSİLAT")
                                .FontSize(21)
                                .Bold()
                                .FontColor("#123B6D");

                            column.Item()
                                .PaddingTop(4)
                                .Text("Yönetim Sistemi")
                                .FontSize(10)
                                .FontColor("#667085");
                        });

                    row.RelativeItem()
                        .AlignRight()
                        .Column(column =>
                        {
                            column.Item()
                                .AlignRight()
                                .Text("TAHSİLAT DEKONTU")
                                .FontSize(16)
                                .Bold()
                                .FontColor("#123B6D");

                            column.Item()
                                .PaddingTop(4)
                                .AlignRight()
                                .Text(
                                    $"Belge Tarihi: " +
                                    $"{DateTime.Now:dd.MM.yyyy HH:mm}")
                                .FontSize(9)
                                .FontColor("#667085");
                        });
                });
        }

        private void IcerikOlustur(
            IContainer container)
        {
            var borc = _tahsilat.Borc;
            var urun = borc?.TahsilatUrunu;
            var kurum = urun?.Kurum;

            var kalanTutar = borc is null
                ? 0
                : Math.Max(
                    0,
                    borc.BorcTutari -
                    borc.OdenenTutar);

            container.Column(column =>
            {
                column.Spacing(16);

                column.Item()
                    .Element(c =>
                        BolumOlustur(
                            c,
                            "İşlem Bilgileri",
                            new[]
                            {
                                (
                                    "İşlem Numarası",
                                    _tahsilat.IslemNo
                                ),
                                (
                                    "İşlem Tarihi",
                                    _tahsilat.IslemTarihi
                                        .ToString(
                                            "dd.MM.yyyy HH:mm:ss")
                                ),
                                (
                                    "İşlem Durumu",
                                    _tahsilat.IslemDurumu
                                ),
                                (
                                    "Ödeme Kanalı",
                                    _tahsilat.OdemeKanali
                                )
                            }));

                column.Item()
                    .Element(c =>
                        BolumOlustur(
                            c,
                            "Kurum ve Borç Bilgileri",
                            new[]
                            {
                                (
                                    "Kurum",
                                    kurum?.KurumAdi ?? "-"
                                ),
                                (
                                    "Kurum Kodu",
                                    kurum?.KurumKodu ?? "-"
                                ),
                                (
                                    "Tahsilat Ürünü",
                                    urun?.UrunAdi ?? "-"
                                ),
                                (
                                    "Ürün Kodu",
                                    urun?.UrunKodu ?? "-"
                                ),
                                (
                                    "Referans Numarası",
                                    borc?.ReferansNo ?? "-"
                                ),
                                (
                                    "Son Ödeme Tarihi",
                                    borc is null
                                        ? "-"
                                        : borc.SonOdemeTarihi
                                            .ToString(
                                                "dd.MM.yyyy")
                                )
                            }));

                column.Item()
                    .Element(c =>
                        BolumOlustur(
                            c,
                            "Müşteri Bilgileri",
                            new[]
                            {
                                (
                                    "Müşteri",
                                    borc?.MusteriAdi ?? "-"
                                ),
                                (
                                    "Müşteri Numarası",
                                    borc?.MusteriNo ?? "-"
                                )
                            }));

                column.Item()
                    .Background("#123B6D")
                    .Padding(20)
                    .Row(row =>
                    {
                        row.RelativeItem()
                            .Column(left =>
                            {
                                left.Item()
                                    .Text(
                                        "Bu İşlemde Ödenen Tutar")
                                    .FontColor("#DCE8F5");

                                left.Item()
                                    .PaddingTop(5)
                                    .Text(
                                        ParaFormatla(
                                            _tahsilat.OdenenTutar))
                                    .FontSize(24)
                                    .Bold()
                                    .FontColor(Colors.White);
                            });

                        row.RelativeItem()
                            .AlignRight()
                            .Column(right =>
                            {
                                right.Item()
                                    .AlignRight()
                                    .Text("Borç Durumu")
                                    .FontColor("#DCE8F5");

                                right.Item()
                                    .PaddingTop(5)
                                    .AlignRight()
                                    .Text(
                                        borc?.Durum ?? "-")
                                    .FontSize(14)
                                    .Bold()
                                    .FontColor(Colors.White);
                            });
                    });

                column.Item()
                    .Element(c =>
                        BolumOlustur(
                            c,
                            "Borç Özeti",
                            new[]
                            {
                                (
                                    "Toplam Borç",
                                    ParaFormatla(
                                        borc?.BorcTutari ?? 0)
                                ),
                                (
                                    "Toplam Ödenen",
                                    ParaFormatla(
                                        borc?.OdenenTutar ?? 0)
                                ),
                                (
                                    "Kalan Borç",
                                    ParaFormatla(
                                        kalanTutar)
                                )
                            }));

                column.Item()
                    .Background("#F8FAFC")
                    .Border(1)
                    .BorderColor("#E2E8F0")
                    .Padding(12)
                    .Text(
                        "Bu dekont elektronik ortamda " +
                        "oluşturulmuştur. İşlem numarası " +
                        "üzerinden doğrulanabilir.")
                    .FontSize(9)
                    .FontColor("#667085");
            });
        }

        private void BolumOlustur(
            IContainer container,
            string baslik,
            IEnumerable<(string Etiket, string Deger)> bilgiler)
        {
            container
                .Border(1)
                .BorderColor("#DCE4EE")
                .Column(column =>
                {
                    column.Item()
                        .Background("#F8FAFC")
                        .Padding(12)
                        .Text(baslik)
                        .Bold()
                        .FontSize(12)
                        .FontColor("#123B6D");

                    column.Item()
                        .Padding(14)
                        .Element(c =>
                            BilgiTablosu(
                                c,
                                bilgiler));
                });
        }

        private void BilgiTablosu(
            IContainer container,
            IEnumerable<(string Etiket, string Deger)> bilgiler)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn();
                    columns.RelativeColumn();
                });

                foreach (var bilgi in bilgiler)
                {
                    table.Cell()
                        .PaddingVertical(8)
                        .PaddingRight(10)
                        .BorderBottom(1)
                        .BorderColor("#EEF2F6")
                        .Column(column =>
                        {
                            column.Item()
                                .Text(bilgi.Etiket)
                                .FontSize(8)
                                .FontColor("#667085");

                            column.Item()
                                .PaddingTop(3)
                                .Text(bilgi.Deger)
                                .SemiBold()
                                .FontColor("#1D2939");
                        });
                }
            });
        }

        private void FooterOlustur(
    IContainer container)
        {
            container
                .BorderTop(1)
                .BorderColor("#DCE4EE")
                .PaddingTop(10)
                .Row(row =>
                {
                    row.RelativeItem()
                        .Text("Kurum Tahsilat Sistemi")
                        .FontSize(8)
                        .FontColor("#667085");

                    row.RelativeItem()
                        .AlignRight()
                        .DefaultTextStyle(style =>
                            style
                                .FontSize(8)
                                .FontColor("#667085"))
                        .Text(text =>
                        {
                            text.Span("Sayfa ");
                            text.CurrentPageNumber();
                            text.Span(" / ");
                            text.TotalPages();
                        });
                });
        }

        private static string ParaFormatla(
            decimal tutar)
        {
            return tutar.ToString(
                       "N2",
                       new CultureInfo("tr-TR")) +
                   " TL";
        }
    }
}