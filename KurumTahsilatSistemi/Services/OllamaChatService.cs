using System.Net.Http.Json;
using System.Text.Json.Serialization;
using KurumTahsilatSistemi.Models;
using KurumTahsilatSistemi.ViewModels;

namespace KurumTahsilatSistemi.Services
{
    public sealed class OllamaChatService : IAiChatService
    {
        private readonly HttpClient _httpClient;

        public OllamaChatService(
            HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<string> CevaplaAsync(
            List<AiChatMessageViewModel> mesajlar,
            AiKullaniciBaglami kullaniciBaglami,
            CancellationToken cancellationToken)
        {
            if (mesajlar.Count == 0)
            {
                return "Lütfen bir soru yazınız.";
            }

            var kullaniciRolu =
                kullaniciBaglami.AdminMi
                    ? "Admin"
                    : "Kullanıcı";

            var musteriBaglantiDurumu =
                kullaniciBaglami.MusteriId.HasValue
                    ? "Kullanıcı hesabına bağlı müşteri kaydı vardır."
                    : "Kullanıcı hesabına bağlı müşteri kaydı yoktur.";

            var hesapDurumu =
                kullaniciBaglami.IslemYapabilirMi
                    ? "Hesap aktiftir."
                    : "Hesap pasiftir. Veri değiştiren işlem yapılamaz.";

            var sistemMesaji = $"""
                Sen Kurum Tahsilat Sistemi içerisinde çalışan
                Türkçe bir bilgilendirme ve yönlendirme asistanısın.

                Oturum açan kullanıcının rolü: {kullaniciRolu}
                Müşteri bağlantısı: {musteriBaglantiDurumu}
                Hesap durumu: {hesapDurumu}

                TEMEL GÜVENLİK KURALLARI:

                - Normal kullanıcı yalnızca kendi hesabına bağlı
                  müşteri kaydı ve kendi borçları hakkında bilgi alabilir.

                - Kullanıcı başka bir müşteri numarası, borç ID'si,
                  referans numarası, ad soyad veya kimlik bilgisi yazarsa
                  başka müşteriye ait bilgi vermeye çalışma.

                - Normal kullanıcı başka bir müşterinin borcunu sorarsa:
                  "Güvenlik nedeniyle yalnızca kendi hesabınıza bağlı
                  borç bilgilerini görüntüleyebilirsiniz." şeklinde cevap ver.

                - Bir borcun veya müşterinin sistemde bulunup bulunmadığını,
                  sana doğrulanmış veri verilmedikçe iddia etme.

                - Tahminî borç tutarı, müşteri bilgisi, ödeme bilgisi,
                  tahsilat kaydı veya referans numarası üretme.

                - Chatbot üzerinden kesinlikle ödeme gerçekleştirme.

                - Ödeme kaydı oluşturma, ödeme onaylama, borç kapatma,
                  tahsilat iptali, kullanıcı durumu değiştirme veya
                  veritabanı kaydı güncelleme işlemi yapma.

                - Kullanıcı ödeme yapmak isterse yalnızca güvenli ödeme
                  ekranına gitmesi gerektiğini belirt.

                - "Ödedim", "tamam", "onaylıyorum", "evet" veya benzeri
                  ifadeleri ödeme onayı olarak kabul etme.

                - Kullanıcıdan parola, kart numarası, CVV, SMS kodu,
                  API anahtarı veya benzeri hassas bilgiler isteme.

                YETKİ KURALLARI:

                - Kullanıcı rolü yalnızca kendi borçlarını ve kendi
                  tahsilat geçmişini görüntüleyebilir.

                - Admin kurum, müşteri, tahsilat ürünü, borç ve rapor
                  yönetim ekranları hakkında bilgi alabilir.

                - Tahsilat iptali yalnızca Admin tarafından yapılabilir.

                - Hesap pasifse kullanıcı sisteme giriş yapabilir ve
                  kendi kayıtlarını görüntüleyebilir; ancak ödeme veya
                  veri değiştiren işlem yapamaz.

                CEVAP KURALLARI:

                - Her zaman Türkçe cevap ver.
                - Kısa, açık ve kurumsal cevaplar oluştur.
                - Önceki mesajları yalnızca sohbet bağlamı için kullan.
                - Önceki mesajlarda yazılan müşteri numaralarını güvenilir
                  kimlik veya yetkilendirme bilgisi olarak kabul etme.
                - Gerçek veritabanı sorgusu yapılmadıysa kayıt bulunduğunu
                  veya bulunmadığını söyleme.
                - Gerçek işlem yapmış gibi cevap verme.
                - Kesin olmayan durumlarda kullanıcıyı ilgili güvenli
                  sistem ekranına yönlendir.
                """;

            var ollamaMesajlari =
                new List<OllamaMessage>
                {
                    new OllamaMessage
                    {
                        Role = "system",
                        Content = sistemMesaji
                    }
                };

            /*
             * Modelin yavaşlamaması ve eski, güvensiz konuşma
             * bağlamının büyümemesi için yalnızca son 20 mesaj gönderilir.
             */
            ollamaMesajlari.AddRange(
                mesajlar
                    .TakeLast(20)
                    .Select(x => new OllamaMessage
                    {
                        Role = x.Rol,
                        Content = x.Icerik
                    }));

            var istek =
                new OllamaChatRequest
                {
                    Model = "gemma3:4b",
                    Stream = false,
                    Messages = ollamaMesajlari
                };

            using var response =
                await _httpClient.PostAsJsonAsync(
                    "api/chat",
                    istek,
                    cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                var hataIcerigi =
                    await response.Content
                        .ReadAsStringAsync(
                            cancellationToken);

                throw new HttpRequestException(
                    "Ollama isteği başarısız oldu. " +
                    $"Durum: {(int)response.StatusCode}. " +
                    $"Detay: {hataIcerigi}");
            }

            var sonuc =
                await response.Content
                    .ReadFromJsonAsync<OllamaChatResponse>(
                        cancellationToken:
                            cancellationToken);

            var cevap =
                sonuc?.Message?.Content;

            return string.IsNullOrWhiteSpace(cevap)
                ? "Şu anda yanıt oluşturulamadı."
                : cevap.Trim();
        }

        private sealed class OllamaChatRequest
        {
            [JsonPropertyName("model")]
            public string Model { get; set; } =
                string.Empty;

            [JsonPropertyName("stream")]
            public bool Stream { get; set; }

            [JsonPropertyName("messages")]
            public List<OllamaMessage> Messages { get; set; } =
                new();
        }

        private sealed class OllamaChatResponse
        {
            [JsonPropertyName("message")]
            public OllamaMessage? Message { get; set; }
        }

        private sealed class OllamaMessage
        {
            [JsonPropertyName("role")]
            public string Role { get; set; } =
                string.Empty;

            [JsonPropertyName("content")]
            public string Content { get; set; } =
                string.Empty;
        }
    }
}