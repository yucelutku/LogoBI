# 07 — Görev Panosu (Task Board)

Basit kanban. İş bitince satırı `## Bitti`'ye taşı. Her görev küçük ve uçtan uca test edilebilir olsun.
Etiketler: `[A]` = beyin/Claude Projesi işi, `[B]` = Cursor/Antigravity kod işi, `[CC]` = Claude Code
(cerrahi, zor motor).

## Backlog (sıradaki fazlar)
- [ ] Ham SQL modu + salt-okunur login + whitelist `[CC]`
- [ ] Rapor kaydet/yükle (JSON, app DB) `[B]`
- [ ] 2. ve 3. grafik türü `[B]`
- [ ] Grain Guard uyarı UX'i (motorda hazır, UI'da gösterimi) `[B]`
- [ ] DATE_ (int YYYYMMDD) tarih biçimlendirme/dönüşümü `[B]`
- [ ] Prod: Logo DB için db_datareader-only login (sa yerine) `[B]`
- [ ] Faz 2: graf join çözücü tasarımı `[A]`
- [ ] Faz 2: fan-trap akıllı çözümü (alt-sorgu) tasarımı `[A]`

## To Do (Faz 1 — güvenilir prototip)
- [ ] Web API katmanı: POST /api/report/preview + GET /api/metadata/{sources,fields,relationships} `[B]`
- [ ] Frontend iskelet: Blazor WASM + MudBlazor, kaynak/alan listesi, MudDropContainer sürükle alanı `[B]`
- [ ] MudDataGrid ile önizleme render (API'den) `[B]`
- [ ] Tek KPI kartı componenti (MudBlazor card) `[B]`
- [ ] Firma/dönem seçici: IFirmPeriodCatalog (L_CAPIFIRM/L_CAPIPERIOD, salt-okunur) + UI `[B]`
- [ ] Kullanıcı filtreleri (Filter → WHERE, parametreli) `[CC]`

## In Progress
- [ ] (buraya taşı)

## Bitti
- [x] Ürün fikri, konumlanma, moat netleşti `[A]`
- [x] Mimari çekirdek kararları (ADR-001…011) `[A]`
- [x] Teknoloji yığını + lisans temizliği belirlendi `[A]`
- [x] Proje doküman seti üretildi `[A]`
- [x] AI ajan kural dosyaları (AGENTS/CLAUDE/GEMINI) üretildi `[A]`
- [x] App DB şeması: LogicalSource/Field/Relationship + Alias kolonu `[B]`
- [x] Tohum metadata: Faturalar, Cari, Stok Hareketleri (gerçek fiziksel adlarla doğrulandı) `[B]`
- [x] Report Definition (AST) modeli `[B]`
- [x] TokenResolver (firma D3 / dönem D2) `[CC]`
- [x] Anchor join resolver (tek çıpa, 1 hop, tahmin yok) `[CC]`
- [x] SQL Compiler (SELECT/FROM/LEFT JOIN/WHERE + token + default_filters) `[CC]`
- [x] Measure + GROUP BY + TOP N `[CC]`
- [x] Grain Guard (fan-trap engelle + uyar) `[CC]`
- [x] Executor (salt-okunur, TOP 100, timeout, generic QueryResult) `[B]`
- [x] Config: isimli Logo bağlantıları + aktif seçici (çok DB'ye hazır) `[B]`
- [x] Konsol smoke-test: uçtan uca gerçek DEVA verisi `[B]`
- [x] **FAZ 0 ÇIKIŞ EŞİĞİ: measure toplamı gerçek Logo ekranıyla TUTTU (firma 126/dönem 01)** `[B]`