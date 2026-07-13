# 06 — Karar Kaydı (ADR)

Her mimari/ürün kararı buraya tek blok olarak eklenir. Format kısa: karar, gerekçe, alternatif,
durum. Yeni karar aldıkça en üste ekle.

> Şablon:
> ### ADR-XXX — [başlık]
> **Durum:** kabul / gözden geçiriliyor / iptal
> **Karar:** …
> **Gerekçe:** …
> **Alternatif(ler):** …

---

### ADR-001 — Ürün salt-okunur olacak
**Durum:** kabul
**Karar:** Ürün Logo'ya hiçbir koşulda yazmaz; yalnızca okur.
**Gerekçe:** Yazma, LObjects run-time lisansı ve destek yükü getirir. Salt-okunur → lisans derdi
yok, güvenlik basit, on-prem satış argümanı güçlü.
**Alternatif:** Yazma destekli (fiş oluşturma) — reddedildi, kapsam/lisans şişer.

### ADR-002 — Tek metadata modeli, sürüm katmanı yok
**Durum:** kabul
**Karar:** Go3/Tiger3/Enterprise için tek metadata modeli. Sürüme özel tablo haritası yok.
**Gerekçe:** Logo DB şeması tüm ürünlerde aynı. Üretim tabloları her sürümde var, yalnız Tiger3+
dolu olur (Go3'te boş döner, hata değil).
**Alternatif:** Sürüm başına fiziksel eşleme paketi — gereksiz, iptal.

### ADR-003 — Anchor tabanlı join, v1'de graf/BFS yok
**Durum:** kabul
**Karar:** Kullanıcı önce çıpa seçer; alanlar tanımlı ilişkilerle çıpaya 1-2 hop'ta bağlanır.
Yol yoksa reddedilir, tahmin edilmez.
**Gerekçe:** Genel graf çözücü işin korkutucu/karmaşık kısmı. Logo yıldız şemasına yakın; anchor
model ~%90'ı karşılar. Karmaşıklığı minimumda tutar.
**Alternatif:** Genel BFS yol çözücü — Faz 2'ye ertelendi.

### ADR-004 — Fan-trap v1'de engellenir, çözülmez
**Durum:** kabul
**Karar:** Farklı grain'lerden measure karışımı v1'de doğrudan engellenir + kullanıcı uyarılır.
**Gerekçe:** Yanlış toplanmış rakam ürünü öldürür. Akıllı çözüm (alt-sorgu) karmaşık; sınırlayarak
basit ve güvenli tutuyoruz.
**Alternatif:** Otomatik alt-sorgu çözümü — Faz 2.

### ADR-005 — Kendi motorumuzu yazıyoruz (Superset/Cube fork değil)
**Durum:** kabul
**Karar:** Semantik motor + SQL derleyici .NET'te kendimiz yazılır; hazır BI motoru fork'lanmaz.
**Gerekçe:** Çekirdek teknoloji .NET; hazır motorlar (Python/Node+Docker) gereksiz yüzey ve
karmaşıklık getirir. Değer zaten kendi metadata modelimizde. Çekirdek birkaç yüz satır.
**Alternatif:** Superset/Cube (Apache-2.0) embed/fork — reddedildi (stack uyumsuz, ağır).

### ADR-006 — Motor deterministik, AI'a bağımlı değil
**Durum:** kabul
**Karar:** Query builder ve SQL üretimi %100 deterministik kod. AI çekirdekte kullanılmaz.
**Gerekçe:** Doğruluk/tekrarlanabilirlik şart (fan-trap). AI ancak çok sonra, semantik katmana
bağlı opsiyonel "doğal dil sorgu" konforu olarak eklenebilir.

### ADR-007 — Tümüyle izin verici OSS yığın, .NET çekirdek
**Durum:** kabul
**Karar:** MIT/Apache-2.0 bileşenler. Grid ve UI=MudBlazor (MudDataGrid), grafik=ApexCharts.Blazor. AG Grid
Enterprise, Highcharts, ticari .NET UI kütüphaneleri ve AGPL fork'lar yasak. (Frontend teknoloji seçimi: bkz. ADR-009.)
**Gerekçe:** Lisans yükü/maliyet sıfır; ticarileştirme önünde engel yok.

### ADR-008 — İki-şerit çalışma modeli (token disiplini)
**Durum:** kabul
**Karar:** Tartışma/mimari/spec → Claude Projesi (beyin). Kütle kod → Cursor/Antigravity (kendi
sayaçları). Claude Code yalnızca zor motor kısmı için cerrahi.
**Gerekçe:** claude.ai + Claude Code aynı kullanım havuzunu paylaşır; kodu Claude Code'da yapmak
tartışma tokenlarını korumaz, aynı kovadan hızlı tüketir. Cursor/Antigravity ayrı sayaç.

###ADR-009 — Frontend Blazor WebAssembly (React/MVC reddedildi)
**Durum:** kabul
**Karar:** Frontend Blazor WebAssembly (.NET 8), ASP.NET Core host. UI kit MudBlazor, grafikler ApexCharts.Blazor (ikisi de MIT).
**Gerekçe:** MVC klasik server-render, drag-drop builder + anlık SQL önizleme modeline uymaz (elden JS'e döner). React ciddi değerlendirildi (dnd-kit/TanStack/ECharts daha cilalı) ama geliştiricinin React bilmemesi + doğruluk-kritik üründe tek-dil/tek-debugger'ın değeri belirleyici oldu: motor zaten C#, tüm yığın C# olunca hata ayıklama tanıdık. Builder UX'inde ücretsiz Blazor lib farkı Faz 0/1 için kabul edilebilir kozmetik maliyet.
Alternatif: React+TS SPA — lib'ler daha olgun ama öğrenme + debug vergisi solo/doğruluk-kritik bağlamda ağır bastı; MVC — builder'a mimari olarak uymadığı için elendi.

###ADR-010 — Syncfusion kullanılmaz (Community lisansı + ortaklık senaryosu dahil)
**Durum:** kabul
**Karar:** Syncfusion hiçbir katmanda kullanılmaz. Varsayılan izin verici (MIT) bileşenlerdir. Kapı tamamen kapalı değil ama açılması iki şarttan birine bağlı: (a) MudBlazor/ApexCharts'ın karşılayamadığı somut, kanıtlanmış bir teknik ihtiyaç doğması, veya (b) ortaklık kurulup lisans mülkiyetinin netleşmesi.
**Gerekçe:** Eldeki Community lisansı şirkete ait; ortaklık henüz kurulmadan prototipi bu lisansla üretmek IP sahipliğini bulandırır ve müzakere gücünü zayıflatır. Prototipin temiz/tümüyle geliştiriciye ait olması, ortaklık teklifinde en güçlü koz. Ürünün moat'ı UI component'i değil Logo semantik katmanı olduğundan, ticari suite değere dokunmaz. Ticari suite'e bağlanmak tek yönlü kapıdır (grid/chart kodu API'sine gömülür).
Alternatif: Syncfusion Community — mülkiyet, eşik ve müzakere riski nedeniyle reddedildi. Ortaklık kurulduktan sonra ortak kararla yeniden değerlendirilebilir.

### ADR-011 — AI ajan kural dosyaları: AGENTS.md tek kaynak + araç adaptörleri
Durum: kabul
Karar: Repo kökünde AGENTS.md (tek gerçeklik kaynağı) + GEMINI.md (Antigravity sertleştirme) +
CLAUDE.md (ince) + .cursor/rules/{00-core,10-engine,20-blazor}.mdc. Gemini için kurallar kısa/
imperatif tutulur, zorunlu build+tarayıcı self-check eklenir.
Gerekçe: Her araç kendi giriş dosyasını okur; tek kaynak kopya/çelişkiyi önler. Gemini uzun
dokümanı takip etmediği için ayrı sertleştirme katmanı + Definition of Done gerekir.
Alternatif: Tek CLAUDE.md/tek .cursorrules — araçlar arası taşınmaz, Gemini drift'ini çözmez; reddedildi.

###ADR-012 — App DB motoru: Microsoft SQL Server. 
**Durum:** kabul.
**Karar:** Metadata + kayıtlı raporlar için app DB olarak SQL Server kullanılır (Logo DB'sinden ayrı veritabanı, ayrı bağlantı dizesi). 
**Gerekçe:** Müşteride SQL Server zaten var → ek altyapı/lisans yok, prod-paritesi tam, Dapper+Microsoft.Data.SqlClient yığınıyla tek sürücü. 
Alternatif: SQLite/PostgreSQL — dev'de sade ama prod-paritesi ve tek-sürücü avantajını kaybediyor; reddedildi.

###ADR-013 (güncelleme) — Token dolgusu domain-teyitli. 
Firma D3 (001–999), dönem D2 (01–99); genişlikler const, doğrulanmış int'ten üretilir, parametre değil.


###ADR-014 — Alias standardı metadata'da veri. 
Her LogicalSource kısa, benzersiz, insan-okur bir Alias taşır (INV,CLC); 
compiler SQL'i bu alias'larla üretir (pozisyonel değil). 
Gerekçe: üretilen SQL'in Logo ile göz-doğrulaması + metadata=veri. Self-join (v1'de yok) için _2 son eki sonraya. Alternatif: pozisyonel t0/t1 — okunabilirlik/doğrulama zayıf, reddedildi.


###ADR-015 — Firma/dönem kaynağı: L_CAPIFIRM/L_CAPIPERIOD, salt-okunur, motordan bağımsız. 
Motor sadece TokenContext(int,int) alır. Numaralar Faz 0'da config'ten; Faz 1'de IFirmPeriodCatalog bu sistem tablolarından dinamik okur + UI seçici. Bu tablolar LogicalSource değil (sistem tablosu, tokensız). Gerekçe: dinamik + salt-okunur, kapsam disiplini korunur.