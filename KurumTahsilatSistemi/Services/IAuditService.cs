public interface IAuditService
{
    Task LogAsync(
        string islemTipi,
        string aciklama,
        string durum = "Başarılı",
        string? kullanici = null);
}