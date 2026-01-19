# SwapSmart – Akıllı Takas Platformu

## Proje Amacı
SwapSmart, kullanıcıların **para kullanmadan**, yalnızca **takas yöntemiyle**
eşyalarını değiştirebildiği, il/ilçe bazlı ve akıllı eşleştirme yapan
bir web uygulamasıdır.

Projenin amacı; klasik satış odaklı platformlardan farklı olarak,
karşılıklı değer değişimine dayalı, güvenli ve kullanıcı dostu
bir takas sistemi geliştirmektir.

---

## Hedef Kullanıcı Kitlesi
- Günlük hayatta eşya takası yapmak isteyen bireyler
- Öğrenciler
- Satış ve fiyat pazarlığıyla uğraşmak istemeyen kullanıcılar
- Aynı şehir veya ilçede takas yapmak isteyen kişiler

---

## Senaryo / Kullanım Amacı
Kullanıcı sisteme kayıt olurken telefon numarası ve bulunduğu il/ilçeyi belirtir.
Kayıt sonrası kullanıcı:

1. Takas etmek istediği eşyayı ilan olarak ekler.
2. İlan oluştururken:
   - Ürün fotoğrafı
   - Açıklama
   - Tahmini değer aralığı (min–max)
   - Takas etmek istediği ürün adı
   bilgilerini girer.
3. Sistem, karşılıklı uyumlu ilanları otomatik olarak kullanıcıya önerir.
4. Kullanıcı başka bir ilana takas teklifi gönderir.
5. İlan sahibi teklifi kabul veya reddeder.
6. Takas kabul edilirse, taraflar **WhatsApp üzerinden iletişime yönlendirilir**.
7. Takas tamamlandığında ilanlar kilitlenir ve süreç sonlandırılır.

Bu sayede kullanıcılar güvenli, kontrollü ve adil bir takas süreci yaşar.

---

## Kullanılan Teknolojiler
- **C#**
- **ASP.NET Core MVC**
- **Entity Framework Core**
- **SQLite**
- **ASP.NET Core Identity**
- **Bootstrap 5**

---

## Öne Çıkan Özellikler
- Satış ve fiyat içermeyen **sadece takas** sistemi
- İl / ilçe bazlı ilan filtreleme
- Tahmini değer aralığı ile adil takas
- Akıllı takas eşleştirme algoritması
- Güvenli WhatsApp yönlendirmesi
- Kontrollü CRUD işlemleri
- Rol ve yetki kontrolleri
- Modern ve responsive kullanıcı arayüzü

---

## MVC Yapısı
- **Models**
  - ApplicationUser
  - Item
  - ItemImage
  - TradeOffer
- **Controllers**
  - HomeController
  - ItemController
  - TradeController
  - ProfileController
- **Views**
  - Controller bazlı ayrılmış view yapısı
  - Partial view ve kart tabanlı tasarım

---

## Güvenlik ve İş Kuralları
- Kullanıcı yalnızca kendi ilanlarını güncelleyebilir veya silebilir.
- Aktif takas sürecindeki ilanlar değiştirilemez.
- Telefon numaraları herkese açık gösterilmez.
- WhatsApp iletişimi yalnızca takas kabul edildikten sonra açılır.
- Yetkisiz erişimler controller ve view seviyesinde engellenmiştir.

---

## Veritabanı
- **SQLite** kullanılmıştır.
- Entity Framework Core Code First yaklaşımı uygulanmıştır.
- Migration ile veritabanı otomatik oluşturulmaktadır.

---

## Kurulum ve Çalıştırma
1. Projeyi klonlayın.
2. Visual Studio veya uygun bir IDE ile açın.
3. `Update-Database` komutu ile veritabanını oluşturun.
4. Projeyi çalıştırın.
5. Tarayıcı üzerinden uygulamayı kullanmaya başlayın.

---

## Tanıtım Videosu
📺 **YouTube Video Linki:**  
https://youtu.be/xqRSe_J-hU4

---

## Not
Bu proje, Web Tabanlı Programlama dersi kapsamında
**MVC mimarisini, veritabanı entegrasyonunu ve gerçek hayat senaryosunu**
birlikte kullanacak şekilde **özgün olarak geliştirilmiştir**.
