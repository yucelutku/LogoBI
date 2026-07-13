# 05 — Yol Haritası ve Kapsam

İlke: **acımasız kapsam disiplini.** Genelleştirme kuyruğu sonsuz. Her iş için "bu faza mı ait?"
sorusunu geç. Önce en küçük uçtan uca dilim çalışsın, sonra aynı kalıbı tekrarla.

## Faz 0 — Çalışan demo (odaklı ~1 hafta / bir hafta sonu)
Amaç: "çalışıyor" dedirtmek; henüz gerçek müşteri değil.
- Tek çıpa: **Faturalar**. Elle girilmiş metadata: 3-4 kaynak (Faturalar, Cari, Stok Hareketleri, Malzeme).
- Sürükle → anlık SQL → grid. Tek KPI kartı.
- Fatura→Cari join çalışıyor (tek ilişki, uçtan uca).
- **Çıkış eşiği:** ürettiğimiz rakam bir test DB'sinde mantıklı; döngü uçtan uca dönüyor.

## Faz 1 — Güvenilir prototip (odaklı 2-4 hafta / akşam-hafta sonu ~1-2 ay)
Amaç: gerçek müşterinin gerçek Logo DB'sinde gösterilebilir.
- Metadata **veri** haline geldi (kod değil); yeni kaynak eklemek satır eklemek.
- **Anchor + 1-2 hop** join resolver.
- **Firma/dönem token** çözümü + seçici.
- **Grain Guard** (fan-trap engelle + uyar).
- **Ham SQL modu** (salt-okunur login + whitelist).
- 2-3 grafik/KPI türü + rapor **kaydet/yükle** (JSON).
- **Çıkış eşiği (kritik):** çıktı rakamları **gerçek Logo ekranıyla birebir tutuyor**.

## Faz 2 — Genişleme (prototip doğrulandıktan sonra)
Yalnızca gerçek ihtiyaç netleştikçe, teker teker:
- Fan-trap'in **akıllı çözümü** (alt-sorguda önce topla, sonra join).
- **Graf/BFS** join çözücü (keyfi tablolar arası yol).
- Hesaplanan metrik / basit ifade dili.
- Daha fazla grafik türü, drill-down, zamanlanmış rapor/e-posta.
- Çok firmalı konsolide raporlar.
- Rol/izin (row-level security) — kim hangi kaynağı görür.

## Kapsam disiplini kuralları (v1'de gevşetme)
- Fan-trap'i **çözme, engelle.**
- Genel graf join **yazma**, anchor+1-2 hop ile kal.
- Grafik türünü **2-3** ile sınırla.
- Hesaplanan metrik dili **yok** (measure'lar sabit).
- AI/doğal dil sorgu **yok** (motor deterministik; AI en fazla çok sonra opsiyonel konfor katmanı).

## İş/dağıtım tarafı (paralel, sonraya bilgi olarak)
- Satış: doğrudan + Logo iş ortağı/bayi resell + (olgunlaşınca) Logo Ekosistem vitrini.
- Model: aylık abonelik (tek seferlik lisans değil), on-prem kurulum.
- Bu satırlar ürün faz eşiklerini beklemeden düşünülebilir ama v1 önceliği üründür.

## Genel çalışma sırası (her dilim için aynı kalıp)
1. Metadata şemasını/tohum veriyi hazırla.
2. En küçük uçtan uca SELECT'i üret (çıpa + gerekli join).
3. Grid'e bas, rakamı Logo ile karşılaştır.
4. Doğ­ruysa bir sonraki kaynağı/özelliği aynı kalıpla ekle. Yanlışsa durup düzelt — UI ekleme.
