# 04 — Metadata Modeli (ürünün kalbi)

Bu doküman semantik katmanın veri şemasını tanımlar. **Kural: her şey veri, hiçbir tablo/ilişki
C# içine gömülmez.** Yeni tablo/ilişki eklemek = buradaki tablolara satır eklemek. Şema tüm Logo
ürünlerinde aynı olduğu için bir kez eklenen tanım tüm müşterileri kapsar.

> Aşağıdaki fiziksel Logo tablo/kolon adları yaygın kullanımdan örnektir; **gerçek DB'de
> doğrulanmalıdır** (bazı adlar sürüm/yapılandırmaya göre teyit ister).

## Varlık 1 — LogicalSource (mantıksal kaynak)
Kullanıcının gördüğü "Faturalar", "Cari", "Stok Hareketleri" gibi kaynaklar.

| Alan | Açıklama |
|------|----------|
| `id` | benzersiz anahtar |
| `display_name` | görünen ad ("Faturalar") |
| `physical_pattern` | fiziksel desen, token'lı: `LG_{FIRMA}_{DONEM}_INVOICE` |
| `scope` | `firm` (yalnız firma eki) \| `period` (firma+dönem eki) |
| `grain` | grain etiketi: `invoice_header`, `invoice_line`, `card` ... |
| `default_filters` | zorunlu WHERE parçaları (ör. `CANCELLED = 0`) |
| `is_hidden` | listelemede gizle (ör. Go3'te boş kalan üretim kaynağı) — opsiyonel |

## Varlık 2 — Field (kolon)
| Alan | Açıklama |
|------|----------|
| `id` | benzersiz anahtar |
| `source_id` | bağlı LogicalSource |
| `physical_column` | fiziksel kolon (ör. `NUMBER`, `NETTOTAL`, `CLIENTREF`) |
| `display_name` | görünen ad ("Fatura No", "Net Tutar") |
| `data_type` | `string` \| `int` \| `decimal` \| `date` \| `bool` |
| `format` | görüntü formatı (ör. `#,##0.00`, `dd.MM.yyyy`) |
| `role` | **`dimension`** \| **`measure`** |
| `default_agg` | measure ise: `sum` \| `avg` \| `count` \| `min` \| `max` |
| `is_hidden` | teknik kolonları (ör. LOGICALREF) kullanıcıdan gizle |

## Varlık 3 — Relationship (ilişki)
| Alan | Açıklama |
|------|----------|
| `id` | benzersiz anahtar |
| `from_source_id` | kaynak tablo |
| `to_source_id` | hedef tablo |
| `from_column` / `to_column` | join kolonları |
| `cardinality` | `one_to_one` \| `one_to_many` |
| `join_type` | v1'de `left` |

## Somut örnek (ilk dilim için tohum veri)

**LogicalSource'lar**
- `Faturalar` → `LG_{FIRMA}_{DONEM}_INVOICE`, scope `period`, grain `invoice_header`,
  default_filters `CANCELLED = 0`
- `Cari` → `LG_{FIRMA}_CLCARD`, scope `firm`, grain `card`
- `Stok Hareketleri` → `LG_{FIRMA}_{DONEM}_STLINE`, scope `period`, grain `invoice_line`
- `Malzeme (Stok Kartı)` → `LG_{FIRMA}_ITEMS`, scope `firm`, grain `card`

**Field örnekleri**
- Faturalar: `NUMBER`→"Fatura No" (dimension), `DATE_`→"Tarih" (dimension),
  `NETTOTAL`→"Net Tutar" (measure/sum), `CLIENTREF`→gizli (join için)
- Cari: `CODE`→"Cari Kodu" (dimension), `DEFINITION_`→"Cari Ünvan" (dimension),
  `LOGICALREF`→gizli
- Stok Hareketleri: `AMOUNT`→"Miktar" (measure/sum), `LINENET`→"Satır Net" (measure/sum),
  `STOCKREF`→gizli, `INVOICEREF`→gizli

**Relationship örnekleri**
- Faturalar.`CLIENTREF` → Cari.`LOGICALREF` (one_to_many, left)
- Stok Hareketleri.`INVOICEREF` → Faturalar.`LOGICALREF` (one_to_many, left)
- Stok Hareketleri.`STOCKREF` → Malzeme.`LOGICALREF` (one_to_many, left)

## Çalışma örneği (mantığın kanıtı)
1. Çıpa = **Faturalar**. Kullanıcı "Fatura No" sürükler →
   `SELECT INV.NUMBER FROM LG_{F}_{D}_INVOICE INV WHERE INV.CANCELLED = 0`
2. Yanına "Cari Kodu" sürükler → resolver `CLIENTREF→LOGICALREF` ilişkisini bulur →
   `SELECT INV.NUMBER, CLC.CODE FROM LG_{F}_{D}_INVOICE INV
    LEFT JOIN LG_{F}_CLCARD CLC ON CLC.LOGICALREF = INV.CLIENTREF WHERE INV.CANCELLED = 0`
3. Kullanıcı "Miktar" (STLINE, grain `invoice_line`) eklemeye çalışır → **Grain Guard devreye girer**:
   `invoice_header` + `invoice_line` measure karışımı fan-trap riski → v1'de engellenir + uyarı.

## Yeni tablo/ilişki ekleme prosedürü (genişletilebilirlik)
1. `LogicalSource` satırı ekle (desen, scope, grain, zorunlu filtreler).
2. Gösterilecek `Field` satırlarını ekle (rol/agg/format ile), teknik kolonları `is_hidden`.
3. Çıpalara bağlanabilmesi için gerekli `Relationship` satırlarını ekle.
4. Bitti — kod değişmez, tüm müşteriler aynı anda kapsanır.

## Kural özetleri (motorun uyacağı)
- `is_hidden` alanlar kullanıcı listesinde görünmez ama join için kullanılabilir.
- measure + dimension seçilince compiler otomatik `GROUP BY` üretir.
- Her kaynağın `default_filters`'ı her sorguya zorunlu eklenir (iptal/statü).
- Farklı `grain` measure karışımı v1'de engellenir.
