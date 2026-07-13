# 01 — Ürün Özeti (Product Brief)

## Problem
Logo kullanan KOBİ'lerde rapor ihtiyacı sürekli ama iki uç var: Logo Mind Insight (Qlik altyapılı)
ve Power BI güçlü ama **pahalı, kurulumu ağır ve SQL/modelleme bilgisi isteyen** profesyonel
araçlar. Sıradan iş kullanıcısı (patron, muhasebeci, satış müdürü) kendi raporunu kuramıyor;
her rapor için bir geliştiriciye/IT'ye gidiyor.

Asıl zorluk SQL **sözdizimi** değil, Logo'nun **şemasını bilmek**: hangi tablo neye karşılık gelir,
LOGICALREF/CLIENTREF ile nasıl join'lenir, iptal/statü flag'leri, borç-alacak işareti, firma/dönem
tablo ekleri. Generic BI araçları bu modeli kullanıcıya kurdurur — ki bu da tam olarak yapılamayan şey.

## Çözüm
Logo şemasını **hazır modellenmiş** getiren, salt-okunur bir self-service BI ürünü. Kullanıcı
"Faturalar", "Cari", "Stok Hareketleri" gibi mantıksal kaynakları görür; kolonları sürükleyip
bırakır; arkada doğru SQL anlık üretilir; sonucu grid, KPI kartı ve grafik olarak bir panele
tasarlar. İsteyen kullanıcı için **ham SQL modu** da vardır.

## Hedef kullanıcı
- **Birincil:** Logo kullanan KOBİ'de rapor tüketen iş kullanıcısı (patron/finans/satış).
- **İkincil:** Logo çözüm ortakları ve bu firmalara hizmet veren mali müşavirler/geliştiriciler.

## Değer önermesi
- Kurulumu hafif, fiyatı düşük, **kutudan çıkar çıkmaz Logo'da çalışır**.
- SQL bilmeyen kişi kendi raporunu kurar → IT/geliştirici kuyruğu biter.
- Salt-okunur olduğu için Logo verisine risk yok; on-prem çalışır, veri şirkette kalır.

## Moat (savunulabilirlik)
Ürünün kopyalanamayan kısmı arayüz değil, **önceden kurulmuş Logo semantik katmanıdır**:
ilişkiler, iş kuralları, measure/dimension ayrımı, varsayılan filtreler. Bu, Looker'ın LookML'i
veya Superset'in dataset'lerinin Logo'ya özel, hazır gelen halidir. Rakip generic araçlar bu
modeli müşteriye kurdurur; biz hazır veririz.

## Konumlanma
- **Logo Mind Insight'a karşı:** biz daha ucuz, daha basit, ayrı sunucu/16GB RAM istemeyen,
  iş kullanıcısına dönük tarafı hedefliyoruz. Özellik genişliğinde değil, sadelik + Logo-native
  uyumda yarışıyoruz.
- **Power BI'a karşı:** onda modeli kullanıcı kurar; bizde hazır gelir.
- **Global self-service BI (Metabase/Superset/Cube) karşısında:** onlar generic; bizim farkımız
  Logo'ya özel hazır semantik katman.

## Non-goals (kesinlikle yapmıyoruz)
- **Saha satış / plasiyer otomasyonu** (mobil sipariş, rota, telefondan bakiye) — kapsam dışı.
- **Logo'ya yazma** (fiş/kart oluşturma) — salt-okunur kalıyoruz, LObjects'e girmiyoruz.
- **Pazaryeri / e-ticaret entegrasyonu** — doymuş kırmızı okyanus, girmiyoruz.
- **Sürüme özel şema mantığı** — Logo DB şeması tüm ürünlerde aynı, gerek yok.
- v1'de: keyfi graf join'leri, otomatik fan-trap çözümü, hesaplanan metrik dili — sonraya.

## Başarı ölçütü (erken)
İlk gerçek müşterinin gerçek Logo DB'sinde, ürettiğimiz rakamların **Logo ekranındaki rakamla
birebir tutması**. Rakam güveni = ürünün olması/olmaması.
