# 02 — Mimari (Architecture)

Çekirdek ilke: **karmaşıklığı minimumda tut, metadata'yı kod değil veri yap, motorun doğruluğundan
taviz verme.** Motor tamamen deterministiktir; AI'a bağımlı değildir.

## Sistem görünümü (yüksek seviye)

```
[Blazor WASM SPA] ──►  [ASP.NET Core Web API]  ──►  [Logo SQL Server (SALT-OKUNUR login)]
  builder UI            │  Semantic Engine
  grid/KPI/chart        │   ├─ Metadata Store (app DB)
  dashboard canvas      │   ├─ Report Definition (AST)
                        │   ├─ Join Resolver (anchor)
                        │   ├─ SQL Compiler (+firma/dönem, zorunlu filtreler)
                        │   ├─ Grain Guard (fan-trap engelle)
                        │   └─ Executor (TOP-N önizleme, timeout)
                        └─ Saved Reports/Dashboards (app DB, JSON)
```

- **On-prem**,  tek ASP.NET Core uygulaması hem API'yi hem Blazor WASM SPA'yı servis eder. Windows'ta (Kestrel/IIS) çalışır; veri şirkette kalır.
- Uygulamanın kendi verisi (metadata + kayıtlı raporlar) **ayrı bir app veritabanında** durur;
  Logo DB'sine asla yazılmaz.

## 1. Metadata Store (semantik katman) — ürünün kalbi
Tabloları, kolonları ve ilişkileri **veri olarak** tutar (kod değil). Böylece yeni tablo/ilişki
eklemek = satır eklemek, recompile yok. Ayrıntılı şema: `04-METADATA-MODEL.md`. Üç ana varlık:
- **LogicalSource** (mantıksal kaynak): görünen ad ("Faturalar"), fiziksel desen
  (`LG_{FIRMA}_{DONEM}_INVOICE`), kapsam (firma/dönem), **grain** etiketi.
- **Field** (kolon): kaynak, fiziksel kolon, görünen ad, tip, format, **rol (dimension/measure)**,
  varsayılan agregasyon.
- **Relationship** (ilişki): from-kaynak, to-kaynak, join kolonları, kardinalite (1-1 / 1-N), join tipi.

## 2. Report Definition (AST)
Kullanıcının builder'da kurduğu rapor, sunucuya **bildirimsel bir nesne** olarak gider:
seçili alanlar, filtreler, gruplama, sıralama, seçili çıpa, grafik/panel yapılandırması.
SQL string'i istemci ASLA göndermez; sunucu üretir. (Ham SQL modu ayrı yoldan gider, aşağıda.)

## 3. Join Resolver — anchor (çıpa) modeli
- Kullanıcı önce **ana kaynağı (anchor)** seçer (ör. "Faturalar").
- Sürüklenen her ek alan, tanımlı `Relationship` kayıtları üzerinden çıpaya **geri dönebilen bir yola**
  sahip olmalı (tipik 1-2 hop). Yol varsa `LEFT JOIN` zinciri kurulur.
- Yol yoksa alan reddedilir: "bu alan Faturalar ile henüz birleştirilemiyor." **Tahmin yok.**
- v1'de genel graf/BFS **yazılmaz**. Logo raporlaması yıldız şemasına yakındır (başlık→satır→kartlar),
  bu yüzden anchor+1-2 hop işlerin ~%90'ını karşılar. Graf çözücü sonraki faza ertelenir.

## 4. SQL Compiler
Report Definition → parametrik T-SQL. Sorumlulukları:
- SELECT: seçili alanlar (measure'lara varsayılan agregasyon).
- FROM: çıpa kaynağı; fiziksel tablo adındaki `{FIRMA}`/`{DONEM}` token'ları **çalışma anında** basılır.
- JOIN: resolver'ın döndürdüğü ilişkilerden LEFT JOIN.
- WHERE: kullanıcı filtreleri **+ zorunlu varsayılanlar** (aktif firma/dönem, `CANCELLED`/`STATUS`
  gibi iptal/statü filtreleri). Zorunlu filtreler metadata'da kaynak başına tanımlanır.
- GROUP BY: measure + dimension karıştığında otomatik.
- Hepsi **parametreli** (SQL injection'a kapalı). İç üretimde SqlKata gibi hafif bir builder
  kullanılabilir; zorunlu değil, birkaç yüz satırlık bir sınıf yeter.

## 5. Grain Guard (fan-trap koruması) — v1'de ENGELLE, çözme
- Her `LogicalSource` bir **grain** taşır (ör. `invoice_header`, `invoice_line`).
- Kullanıcı farklı grain'lerden measure karıştırırsa (INVOICE net tutar + STLINE miktar toplamı),
  toplama **çarpar** → yanlış rakam. v1'de bu kombinasyon **doğrudan engellenir**, kullanıcıya
  nazik uyarı gösterilir.
- Akıllı çözüm (önce alt-sorguda topla, sonra join'le) sonraki faza. Sorunu *çözmeyerek* değil,
  *sınırlayarak* basit tutuyoruz.

## 6. Executor
- **Salt-okunur** bağlantı (bkz. güvenlik). Önizlemede `TOP N` (ör. 100) + debounce (her sürüklemede
  sorgu atma) + query timeout. Production DB'yi yormamak esas.
- Sonuç bir tablo (kolon meta + satırlar) olarak döner; grid/KPI/grafik bunu tüketir.

## 7. Ham SQL modu
- Ayrı bir editör; kullanıcı doğrudan SELECT yazar.
- Güvenlik **iki katman**:
  1. **Birincil:** ayrı bir Logo DB login'i, yalnızca `db_datareader` yetkili (DDL/DML fiziksel
     olarak imkânsız). Lisans/parser'a bağlı değil, en sağlamı budur.
  2. **İkincil:** çalıştırmadan önce statement whitelist — yalnızca tek `SELECT`; `INSERT/UPDATE/
     DELETE/EXEC/DROP/ALTER/MERGE/;` reddedilir. T-SQL parser (ör. ScriptDom — güncel lisansını
     doğrula) veya basit kural tabanlı kontrol.
- Timeout ve satır limiti burada da uygulanır.

## 8. Dashboard / Rapor Tasarımcısı (frontend)
-Blazor WASM üzerinde canvas: componentler grid (MudDataGrid), KPI kartı, grafik (ApexCharts.Blazor) (v1'de 2-3 grafik türü).
-Sürükle-bırak MudBlazor (MudDropContainer) ile; her component bir Report Definition'a (veya ham SQL'e) bağlıdır.
-Panel düzeni + component yapılandırması JSON olarak app DB'de saklanır (kaydet/yükle).
-Grafik motoru (ApexCharts) UI kit'ten bilinçli ayrı; Report Definition → grafik sınırında izole.
## 9. Firma / dönem çözümü
- Fiziksel tablo adı `LG_{FIRMA}_{DONEM}_...`. Bu bir *sürüm* değil *kurulum* değişkeni.
- Küçük bir "token resolver": aktif firma/dönem seçiminden gerçek numaraları üretir ve compiler'a
  verir. Firma seviyesindeki tablolarda (ör. CLCARD → `LG_{FIRMA}_CLCARD`) dönem eki yoktur;
  metadata'daki kapsam alanı bunu belirler.
- Çok firmalı/çok dönemli raporlama bu sayede bedava gelir (opsiyonel bir seçici).

## Güvenlik / operasyon ilkeleri
- Logo DB'sine **yalnızca salt-okunur** login ile bağlan. Ürün Logo'ya hiçbir koşulda yazmaz.
- Bütün Logo sorguları parametreli; ham SQL modunda çift katman koruma.
- On-prem; veri dışarı çıkmaz (KOBİ için satış argümanı).
- Not: Logo DB'sine doğrudan SQL okuma, Logo'nun destek/garanti açısından gri alanıdır; salt-okunur
  ve yalnızca raporlama olduğumuz için risk düşük, ama müşteriye bunu şeffaf anlat.

## Genişletilebilirlik
- Yeni tablo/ilişki = metadata satırı. Şema tüm ürünlerde aynı olduğu için **bir kez eklenen tanım
  tüm müşterileri kapsar**. Logo bir kolon eklerse tek satırla absorbe edilir; recompile yok.
