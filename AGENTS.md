

# AGENTS.md — Proje Kural Seti (Tek Gerçeklik Kaynağı)

Bu dosya bu repoda çalışan **tüm AI ajanları** (Antigravity, Cursor, Claude Code) için
bağlayıcı kuraldır. Cursor için `.cursor/rules/*.mdc`, Antigravity için `GEMINI.md`,
Claude Code için `CLAUDE.md` bu dosyayı işaret eder / tamamlar. Çelişki olursa **bu dosya kazanır**.

---

## 1. Ne inşa ediyoruz (bağlam)

Logo ERP için **salt-okunur, semantik katman tabanlı self-service BI** ürünü. Değer sürükle-bırak
arayüzü değil, **Logo şemasının hazır modellenmiş olmasıdır** (LOGICALREF/CLIENTREF ilişkileri,
firma/dönem tablo ekleri, iptal/statü flag'leri, borç-alacak işareti, measure/dimension ayrımı).
Bu ürünün moat'ıdır; her karar bunu korumalı.

Motor **%100 deterministik koddur, AI'a bağımlı değildir.** Bir rakam yanlışsa ürün ölür;
**doğruluğu her zaman hıza tercih et.**

---

## 2. ASLA ÇİĞNENMEYEN KURALLAR (non-negotiable)

- **SALT-OKUNUR.** Logo DB'sine hiçbir koşulda INSERT/UPDATE/DELETE/DDL yazma. Ürün Logo'ya yazmaz.
- **Fan-trap (grain) karıştırma.** Farklı grain'lerden (ör. `invoice_header` 1 satır, `invoice_line`
  N satır) measure'ları toplama. v1'de bunu **ÇÖZME, ENGELLE** ve kullanıcıyı uyar.
- **Join = yalnız anchor modeli.** Kullanıcı önce çıpayı seçer; her ek alan tanımlı `Relationship`
  kayıtları üzerinden çıpaya 1-2 hop'ta bağlanmalı. Yol yoksa **"birleştirilemiyor" de, TAHMİN ETME.**
  v1'de genel graf/BFS join çözücü **YAZMA.**
- **SQL parametreli.** Değerleri asla string interpolation ile SQL'e gömme. `{FIRMA}`/`{DONEM}`
  token'ları resolver'dan gelen değerlerle basılır; kullanıcı değerleri parametre olur.
- **Ham SQL modu iki katman korumalı:** salt-okunur DB login (`db_datareader`) + tek `SELECT`
  whitelist. `INSERT/UPDATE/DELETE/EXEC/DROP/ALTER/MERGE/;` reddedilir.
- **Sadece izin verici OSS.** MIT / Apache-2.0. Kapalı ticari veya AGPL bileşen YASAK (bkz. §4).
- **Metadata = veri, kod değil.** Hiçbir tablo/kolon/ilişki C# içine gömülmez; app DB'sindeki
  `LogicalSource` / `Field` / `Relationship` tablolarında satır olarak durur.

---

## 3. Teknoloji yığını (sürümler sabit)

**Backend (.NET çekirdek):**
- .NET 8.0 (LTS) + ASP.NET Core — tek uygulama hem Web API hem Blazor WASM host
- Microsoft.Data.SqlClient (SQL erişimi), **Dapper** (read mapping — EF Core KULLANMA)
- SqlKata (opsiyonel, parametrik SQL yardımcısı), System.Text.Json, ClosedXML (Excel export)

**Frontend (.NET — tek dil):**
- Blazor WebAssembly (.NET 8)
- **MudBlazor 8.11.0** — UI kit + grid (`MudDataGrid`) + drag-drop (`MudDropContainer`) + KPI kartı
- **Blazor-ApexCharts 6.1.0** — grafikler (MudBlazor'ın kendi chart bileşenini KULLANMA)
- State: component state + scoped/singleton DI servisleri (ayrı state kütüphanesi YOK)

**App DB:** metadata + kayıtlı raporlar Logo DB'sinden **ayrı** bir veritabanında; Logo'ya yazılmaz.

---

## 4. YASAKLI BİLEŞENLER (lisans mayınları)

Aşağıdakileri **hiçbir katmanda önerme, import etme, kod üretme:**
- **AG Grid Enterprise** — Enterprise özellikleri ticari. Grid = `MudDataGrid`.
- **Highcharts** — ticari. Grafik = Blazor-ApexCharts.
- **Telerik / DevExpress / Syncfusion / ComponentOne** — hepsi ticari .NET UI/rapor suite'i.
- **AGPL fork'lar** (ör. Metabase açık sürümü) — copyleft riski.
- **EF Core** — bu salt-okunur/sorgu-odaklı iş için ağır; Dapper kullan.

Yeni bağımlılık eklemeden önce dur ve sor. Varsayılan cevap: **ekleme.**

---

## 5. Motor doğruluğu (kritik davranışlar)

- **SQL Compiler:** Report Definition (AST) → parametrik T-SQL. SELECT (measure'lara varsayılan agg),
  FROM (çıpa + token), LEFT JOIN (yalnız resolver'ın verdiği ilişkiler), WHERE (kullanıcı filtreleri
  **+ her kaynağın zorunlu `default_filters`'ı**, ör. `CANCELLED = 0`), measure+dimension varsa
  otomatik GROUP BY.
- **Grain Guard:** farklı grain measure karışımı tespit edilince sorguyu üretme, nazik uyarı döndür.
- **Executor:** salt-okunur bağlantı, önizlemede `TOP N` + query timeout + debounce.
- Logo fiziksel tablo/kolon adları (`INVOICE`, `CLCARD`, `NETTOTAL`, `CLIENTREF`, `LOGICALREF` ...)
  **yaygın kullanımdan varsayımdır; gerçek DB'de doğrulanmalı.** Emin değilsen `// TODO: DB'de doğrula`
  yaz, uydurma.

---

## 6. Kapsam disiplini (v1'de gevşetme)

**v1'DE YOK / SONRAYA:**
- Genel graf/BFS join çözücü → anchor + 1-2 hop ile kal
- Fan-trap'in akıllı çözümü (alt-sorguda önce topla) → v1'de sadece engelle
- Hesaplanan metrik / ifade dili → measure'lar sabit
- AI/doğal dil sorgu → motor deterministik
- 3'ten fazla grafik türü → 2-3 ile sınırla

Her özellik önerisinde sor: **"Bu v1'e mi ait, sonraya mı?"** Varsayılan: en küçük uçtan uca dilim.
Bir dilim doğrulanmadan (rakam Logo ekranıyla tutmadan) bir sonrakine geçme. UI eklemek yanlış
rakamı düzeltmez.

---

## 7. Kod hijyeni

- `async/await` her I/O'da. Magic string yok — sabitler `const`.
- Kısa, tek sorumluluklu sınıf/metod. Gereksiz soyutlama katmanı ekleme (her abstraction yerini
  hak etmeli).
- Marka renkleri (frontend): `--brand-primary: #1A2B47` (lacivert), `--brand-gold: #B89968` (altın).
  Başka ana renk ekleme.

---

## 8. DEFINITION OF DONE (bitmeden "bitti" deme)

Bir görevi tamamlandı saymadan önce **hepsini doğrula:**
1. `dotnet build` **hatasız** geçiyor.
2. §2'deki hiçbir kural çiğnenmedi (salt-okunur, parametreli SQL, anchor-only, grain guard,
   izin verici OSS, §4 yasaklıları yok).
3. Yeni bağımlılık eklenmediyse — eklendiyse §4'e göre izin verici mi ve gerçekten gerekli mi?
4. Kapsam çiti aşılmadı (§6 — v1'e ait olmayan bir şey eklenmedi).
5. Kabul kriteri (prompt'ta verilen örnek girdi → beklenen çıktı) sağlanıyor.

Bir madde sağlanmıyorsa görev **bitmemiştir**; düzelt, sonra bildir.

##9. DETAY BİLGİ
Detaylı tasarım/mimari dokümanları `docs/` altındadır (01–08). Derin bağlam gerektiğinde
ilgili dosyayı referans al (ör. metadata detayı için `docs/04-METADATA-MODEL.md`).