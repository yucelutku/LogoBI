# 08 — Prompt Playbook

Bu Projede (beyin) üretilen spec/prompt'ları **Cursor/Antigravity/Claude Code**'a taşımak için
kalıplar. Amaç: pahalı iteratif üretimi başka sayaçta yapmak, ucuz spec üretimini burada tutmak.

## İki-şerit kuralı (hangi iş nereye)
- **Şerit A — burada (Claude Projesi):** tartışma, mimari, metadata tasarımı, spec/prompt üretimi.
- **Şerit B — Cursor / Antigravity:** kütle kod üretimi (UI, grid, grafik, boilerplate, plumbing).
  Cursor içinde Claude modelini seçebilirsin; kaliteyi kaybetmezsin, bedeli ayrı sayaçta ödersin.
- **`[CC]` — Claude Code (cerrahi):** yalnızca zor motor kısmı (join resolver, SQL compiler,
  fan-trap guard, ham SQL güvenliği). Kalite farkının önemli olduğu, havuz bütçesi olan anlarda.

## İyi prompt'un kuralları (kendi kendine yeten olmalı)
1. **Bağlam:** hangi dosya/katman, hangi teknoloji (.NET 8 / ASP.NET Core / Blazor WASM + MudBlazor).
2. **Girdi/çıktı sözleşmesi:** fonksiyon imzası, DTO şekli, dönen tip.
3. **Kısıtlar:** salt-okunur, parametreli SQL, anchor-only join, fan-trap engelle, izin verici OSS.
4. **Kabul kriteri:** ne olursa "doğru" sayılır (örnek girdi → beklenen SQL/çıktı).
5. **Kapsam çiti:** neyi YAPMA (graf join yok, fan-trap çözme yok, yeni bağımlılık ekleme yok).

## Şablon A — Motor parçası prompt'u (`[CC]` için)
```
Bağlam: [proje] .NET 8 / ASP.NET Core. Katman: SQL Compiler.
Görev: Report Definition (AST) alıp parametrik T-SQL üret.
Sözleşme: girdi = { anchor, fields[], filters[], groupings[] }; çıktı = { sql, parameters }.
Kısıtlar: parametreli; FROM'da {FIRMA}/{DONEM} token'ları resolver'dan gelen değerlerle basılır;
her kaynağın default_filters'ı WHERE'e zorunlu eklenir; measure+dimension varsa GROUP BY üret;
JOIN yalnızca resolver'ın verdiği ilişkilerden LEFT JOIN.
YAPMA: string interpolation ile değer gömme; graf join; birden çok statement.
Kabul: [örnek AST] verildiğinde tam olarak [beklenen SQL] üretmeli.
```

## Şablon B — UI/feature prompt'u (Şerit B için)
```
Bağlam: Blazor WebAssembly (.NET 8), ASP.NET Core host. MudBlazor (UI/grid/drag-drop/KPI),
ApexCharts.Blazor (grafik). State: component state + scoped/singleton service.
Görev: [component] — [ne yapmalı].
Kısıtlar: yalnızca izin verici (MIT/Apache) paketler; Syncfusion/Telerik/DevExpress YOK;
grafik ApexCharts.Blazor ile (MudBlazor chart'ı kullanma); API sözleşmesi = [endpoint + istek/yanıt şekli].
Kabul: [kullanıcı etkileşimi] → [beklenen davranış].
YAPMA: yeni ağır bağımlılık; ticari component suite; tasarımı aşırı karmaşıklaştırma.
```

## Şablon C — Hata düzeltme prompt'u
```
Bağlam: [dosya/fonksiyon]. Beklenen: [x]. Gözlenen: [y]. Tekrar üretme: [adım].
Kısıt: davranışı değiştirme, yalnızca hatayı gider; testleri koru.
```

## Dolu örnek — ilk uçtan uca dilim (`[CC]`)
```
Bağlam: .NET 8 / ASP.NET Core, Dapper, Microsoft.Data.SqlClient. Salt-okunur Logo SQL Server.
Görev: Anchor join resolver + SQL compiler'ın en küçük hali.
Senaryo: anchor = "Faturalar". fields = ["Faturalar.NUMBER", "Cari.CODE"].
Metadata (verili): 
  Faturalar → LG_{FIRMA}_{DONEM}_INVOICE, scope=period, default_filters "CANCELLED=0"
  Cari → LG_{FIRMA}_CLCARD, scope=firm
  Relationship: Faturalar.CLIENTREF → Cari.LOGICALREF (one_to_many, left)
firma=... dönem=... token değerleri parametre olarak verilecek.
Beklenen SQL (birebir):
  SELECT INV.NUMBER, CLC.CODE
  FROM LG_{F}_{D}_INVOICE INV
  LEFT JOIN LG_{F}_CLCARD CLC ON CLC.LOGICALREF = INV.CLIENTREF
  WHERE INV.CANCELLED = 0
Kısıtlar: token'lar resolver'dan; değerler parametreli; tek SELECT.
YAPMA: graf join; STLINE/başka grain ekleme; string ile değer gömme.
Kabul: yukarıdaki fields için tam bu SQL üretilmeli; Cari yerine farklı bir kaynak ilişkisiz
ise "birleştirilemiyor" hatası dönmeli.
```

## Prompt üretim akışı (pratikte)
1. Burada (beyin) özelliği/motoru tartış, karara bağla, gerekirse ADR ekle.
2. Bana "bunun Cursor/Claude Code prompt'unu üret" de → yukarıdaki şablonlardan doldururum.
3. Prompt'u ilgili araca taşı, kodu orada ürettir.
4. Sonucu buraya özetle getir (kod dökme); bir sonraki dilime geç.
