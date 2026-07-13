# 07 — Görev Panosu (Task Board)

Basit kanban. İş bitince satırı `## Bitti`'ye taşı. Her görev küçük ve uçtan uca test edilebilir olsun.
Etiketler: `[A]` = beyin/Claude Projesi işi, `[B]` = Cursor/Antigravity kod işi, `[CC]` = Claude Code
(cerrahi, zor motor).

## Backlog (sıradaki fazlar)
- [ ] Firma/dönem seçici UI `[B]`
- [ ] Ham SQL modu + salt-okunur login + whitelist `[CC]`
- [ ] Rapor kaydet/yükle (JSON, app DB) `[B]`
- [ ] 2. ve 3. grafik türü `[B]`
- [ ] Grain Guard uyarı UX'i `[B]`
- [ ] Faz 2: graf join çözücü tasarımı `[A]`
- [ ] Faz 2: fan-trap akıllı çözümü (alt-sorgu) tasarımı `[A]`

## To Do (Faz 0 — çalışan demo)
- [ ] App DB şeması: LogicalSource / Field / Relationship tabloları `[B]`
- [ ] Tohum metadata: Faturalar, Cari, Stok Hareketleri, Malzeme `[A]` (tasarım) → `[B]` (seed script)
- [ ] Report Definition (AST) modeli (C# DTO'lar) `[A]`→`[B]`
- [ ] Anchor join resolver — tek çıpa, tek ilişki (Faturalar→Cari) `[CC]`
- [ ] SQL Compiler — SELECT/FROM/LEFT JOIN/WHERE + firma-dönem token `[CC]`
- [ ] Executor — salt-okunur bağlantı, TOP 100, timeout `[B]`
- [ ] Frontend iskelet: Blazor WASM + MudBlazor, kaynak listesi, MudDropContainer sürükle alanı `[B]`
- [ ] MudDataGrid ile grid render `[B]`
- [ ] Tek KPI kartı componenti (MudBlazor card) `[B]`
- [ ] Uçtan uca: "Fatura No" + "Cari Kodu" sürükle → grid'de doğru satırlar `[CC]`

## In Progress
- [ ] (buraya taşı)

## Bitti
- [x] Ürün fikri, konumlanma, moat netleşti `[A]`
- [x] Mimari çekirdek kararları (ADR-001…008) `[A]`
- [x] Teknoloji yığını + lisans temizliği belirlendi `[A]`
- [x] Proje doküman seti üretildi `[A]`

## Notlar
- Faz 0 çıkış eşiği: uçtan uca döngü dönüyor.
- Faz 1 çıkış eşiği (kritik): çıktı rakamı **gerçek Logo ekranıyla birebir**.
- Bir dilim doğrulanmadan bir sonrakine geçme; UI eklemek yanlış rakamı düzeltmez.
