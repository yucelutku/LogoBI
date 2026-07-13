# CLAUDE.md — Claude Code

**Tek gerçeklik kaynağı repo kökündeki `AGENTS.md`'dir. Önce onu oku ve tüm kurallarına uy.**
Bu dosya yalnızca Claude Code'a özel kısa notlar ekler; kuralları tekrarlamaz.

## Bu repodaki rolün
Sen **cerrahi motor işi** içinsin: join resolver, SQL compiler, Grain Guard, ham SQL güvenliği.
Kütle UI/boilerplate işi başka araçlarda yapılır — sana verilen görev tipik olarak doğruluk-kritik,
küçük ve iyi tanımlıdır. Kapsamını **verilen görevle sınırla**, spontane refactor/genişletme yapma.

## Çalışma tarzı
- Değiştirmeden önce ilgili dosyayı oku; mevcut deseni koru.
- Motor kodu **%100 deterministik** olmalı — AI/rastgelelik yok, aynı girdi aynı SQL'i üretmeli.
- SQL üretiminde: parametreli, anchor-only join, zorunlu `default_filters`, grain guard (bkz. AGENTS.md §2, §5).
- Bir davranışı doğrulamak için küçük birim testi yaz; kabul kriterini (örnek AST → beklenen SQL)
  test olarak sabitle.
- Emin olmadığın Logo tablo/kolon davranışını uydurma; `// TODO: DB'de doğrula` işaretle ve söyle.

## Bitirmeden önce
`dotnet build` hatasız + varsa testler yeşil + AGENTS.md §8 Definition of Done sağlanmış olmalı.
