# 03 — Teknoloji Yığını ve Lisans

İlke: **çekirdekte .NET**, bütün bileşenler **izin verici (MIT / Apache-2.0)** açık kaynak. Hiçbir
bileşen kapalı ticari lisans veya copyleft (AGPL) yük getirmemeli. Lisanslar zamanla değişebilir;
bir bileşeni kilitlemeden önce güncel lisansını **doğrula**.

## Backend (.NET — çekirdek)
| Bileşen | Amaç | Lisans |
|---------|------|--------|
| .NET 8 (LTS) + ASP.NET Core | Web API + Blazor WASM host | MIT |
| Microsoft.Data.SqlClient | SQL Server erişimi | MIT |
| Dapper | Hafif, hızlı read mapping (EF'e gerek yok) | Apache-2.0 |
| SqlKata *(opsiyonel)* | Parametrik SQL üretimi (compiler yardımcısı) | MIT |
| System.Text.Json | Serializasyon (yerleşik) | MIT |
| ClosedXML | Excel dışa aktarma | MIT |

- **Raw SQL güvenliği:** birincil koruma DB tarafında salt-okunur login (`db_datareader`). İkincil
  koruma için T-SQL parser gerekirse **ScriptDom** değerlendirilebilir — güncel lisansını doğrula;
  gerekmiyorsa kural tabanlı whitelist yeter.
- **PDF dışa aktarma (gerekirse):** QuestPDF **Community lisansı belirli bir ciro eşiğinin altında
  ücretsiz**, üstünde ücretli — eşiği kontrol et; alternatif olarak izin verici bir kütüphane seç.
- ORM tercihi: ağır EF Core yerine **Dapper** (salt-okunur, sorgu odaklı iş için ideal ve hafif).

## Frontend (.NET — tek dil, tek host)
| Bileşen | Amaç | Lisans |
|---------|------|--------|
| Blazor WebAssembly (.NET 8) | UI (SPA) | MIT |
| MudBlazor | UI kit + grid (MudDataGrid) + sürükle-bırak (MudDropContainer) + KPI kartı | MIT |
| Blazor-ApexCharts | Grafikler / KPI görselleri | MIT |
| Component state + scoped/singleton service | State yönetimi (yerleşik) | — |

- **Grid:** MudBlazor'ın `MudDataGrid`'i. Ayrı bir grid kütüphanesi (TanStack vb.) gerekmez;
  React ekosistemine ait, bu yığında geçersiz.
- **Sürükle-bırak:** MudBlazor `MudDropContainer` / `MudDropZone`. Ayrı bir dnd kütüphanesi yok.
- **Grafik motoru bilinçli olarak UI kit'ten ayrı:** grafik Blazor-ApexCharts ile çizilir,
  MudBlazor'ın kendi chart bileşeni **kullanılmaz** (Report Definition → grafik sınırını izole tutmak için).
- **State:** ayrı bir state kütüphanesi yok; component state + DI ile scoped/singleton servisler yeter.
- Doğrulanmış repo sürümleri: `.NET 8.0`, `MudBlazor 8.11.0`, `Blazor-ApexCharts 6.1.0`.

## Uygulamanın kendi veritabanı (metadata + kayıtlı raporlar)
- SQL Server (zaten var) ya da PostgreSQL/SQLite. Metadata küçük; hangisi kolaysa. Logo DB'sinden
  **ayrı**. Logo DB'sine asla yazılmaz.

## ⚠️ Lisans mayınları — KULLANMA
- **AG Grid Enterprise** — Community MIT ama pivot/gruplama/entegre grafik/server-side row model gibi
  BI özellikleri Enterprise'da ve **ticari, geliştirici başına lisans**. Grid için MudDataGrid (MudBlazor) kullan.
- **Highcharts** — ticari kullanımda ücretsiz değil. Grafik için Blazor-ApexCharts kullan.
- **Telerik / DevExpress / Syncfusion / ComponentOne** (.NET UI/rapor) — hepsi ticari. Kullanma.
- **Metabase'i fork'lamak** — açık sürümü AGPL (copyleft, network kullanımı kaynak açma tetikler),
  kapalı ticari ürün için riskli. (Kendi motorumuzu yazdığımız için zaten gerekmiyor; not olarak dursun.)

## Neden kendi motorumuzu yazıyoruz (Superset/Cube fork yerine)
Superset/Cube Apache-2.0 (izin verici) ama Python/Node + Docker + cache + auth gibi geniş bir yüzey
getirir; çekirdek teknoloji .NET olacağı ve karmaşıklığı minimumda tutmak istediğimiz için, ince
kendi builder'ımızı yazmak daha sade. Asıl değer (Logo metadata modeli) zaten kendi şemamızda
ifade edilir. Bu bir mimari karar — bkz. `06-DECISION-LOG.md`.

## SQL Server hakkında
SQL Server müşterinin; biz lisanslamıyoruz (Logo zaten üstünde çalışıyor). Bizim tarafımızda
lisans yükü yok.