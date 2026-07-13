# GEMINI.md — Antigravity İçin Sert Kurallar (En Yüksek Öncelik)

> Bu dosya Antigravity'de en yüksek önceliğe sahiptir. `AGENTS.md`'yi **ezmez, pekiştirir.**
> Tam kural seti için `AGENTS.md`'yi oku. Aşağıdakiler ihlal edilmesi en pahalı olan çekirdektir.
> Kısa tutulmuştur çünkü uzun dokümanlarda kaybolma eğilimin var; **bu 10 kuralı ezberle.**

## ÇALIŞMADAN ÖNCE
- Görevi anlamadıysan **kod yazma, önce sor.** Varsayım üretme, uydurma.
- Repo kökündeki `AGENTS.md`'yi oku ve ona uy. Çelişki olursa AGENTS.md kazanır.

## MUTLAK KURALLAR (ASLA çiğneme)
1. **SALT-OKUNUR.** Logo DB'sine ASLA yazma (INSERT/UPDATE/DELETE/DDL yok). Sadece SELECT.
2. **PARAMETRELİ SQL.** Değeri ASLA string ile SQL'e gömme. Token'lar resolver'dan, değerler parametre.
3. **ANCHOR-ONLY JOIN.** Çıpa dışı alan tanımlı ilişkiyle 1-2 hop'ta bağlanmalı. Yol yoksa
   "birleştirilemiyor" de. **Graf/BFS join YAZMA. Tahmin YOK.**
4. **FAN-TRAP: ENGELLE, ÇÖZME.** Farklı grain measure'larını toplama; tespit et, uyar, dur.
5. **SADECE MIT/Apache-2.0 paket.** AG Grid Enterprise, Highcharts, Syncfusion, Telerik,
   DevExpress, EF Core ASLA. Grid = MudDataGrid, grafik = Blazor-ApexCharts.
6. **YENİ BAĞIMLILIK EKLEME.** Gerekliyse önce sor.
7. **SÜRÜMLER SABİT:** .NET 8.0, MudBlazor 8.11.0, Blazor-ApexCharts 6.1.0, Dapper. Değiştirme.
8. **METADATA = VERİ.** Tablo/kolon/ilişkiyi C# içine gömme; DB satırı olarak tanımla.
9. **KAPSAM ÇİTİ.** v1'e ait olmayan özellik ekleme (hesaplanan metrik, doğal dil sorgu,
   akıllı fan-trap, ekstra grafik). Şüphedeysen: EKLEME.
10. **UYDURMA.** Emin olmadığın Logo tablo/kolon adını `// TODO: DB'de doğrula` diye işaretle.

## FRONTEND
- Inline style YASAK → CSS class. jQuery YASAK → Blazor/MudBlazor.
- Marka renkleri: `--brand-primary: #1A2B47`, `--brand-gold: #B89968`. Başka ana renk ekleme.
- `async/await` her yerde. Magic string yok → `const`.

## BİTİRMEDEN ÖNCE — KENDİNİ DOĞRULA (zorunlu)
Bir görevi "bitti" demeden önce:
1. **`dotnet build` çalıştır — hatasız geçmeli.** Geçmiyorsa bitmedi.
2. Yukarıdaki 10 mutlak kuraldan hiçbirini çiğnemediğini **tek tek kontrol et.**
3. Frontend işiyse tarayıcıda aç, çalıştığını **gözle doğrula** (Antigravity tarayıcısını kullan).
4. Prompt'taki kabul kriteri sağlanıyor mu? Örnek girdi → beklenen çıktı tutuyor mu?
5. Bir madde bile başarısızsa: **düzelt, sonra bildir.** "Muhtemelen çalışır" deme.
