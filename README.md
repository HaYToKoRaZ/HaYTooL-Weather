# 🌤️ HaYTooL Weather

<div align="center">
  <p><strong>Lightweight, Dynamic Weather Application for Windows System Tray</strong></p>
  <p><em>Windows Sistem Tepsisi (Görev Çubuğu) Dinamik Hava Durumu Uygulaması</em></p>
</div>

---

## 🇬🇧 English Description

**HaYTooL Weather** is an ultra-lightweight, modern native desktop application that runs silently in the Windows Notification Area (System Tray, next to the taskbar clock), dynamically rendering live temperature and weather condition symbols.

### ✨ Key Features
- **Dynamic Dual Tray Icons:** High-DPI rendered weather condition icon on the left (sun, rain, snow, storm, etc.) and large temperature digits on the right (e.g., `24°`).
- **Fully Customizable Appearance & Scale:** Independent sliders (%40 - %200) for weather icon scale, temperature text size, custom background colors, and opacity levels (%0 - %100).
- **Live Taskbar Preview Simulator:** Real-time taskbar preview inside the Control Center showing how icons look on your actual taskbar.
- **Default Location & Interval:** Starts with **Istanbul, Turkey** by default with a **6-hour** automatic update interval.
- **Instant Location Search & Quick Capitals:** Search provinces, districts, and worldwide cities or select capital cities representing 7 languages with 1 click.
- **Detailed Weather Card:** Click tray icon to open a sleek dark glass popup with humidity, wind speed, precipitation probability, and a 7-day forecast with direct web links.
- **Portable `.ini` Configuration:** All settings are stored in `HaYTooLWeather.ini` located in the executable folder.
- **7 Languages Supported:** English (`en`), Turkish (`tr`), Spanish (`es`), German (`de`), Portuguese (`pt`), Arabic (`ar`), Russian (`ru`).
- **HaYTooL PoRTaL:** Discover and access all HaYTooL ecosystem desktop applications at [https://haytokoraz.github.io/](https://haytokoraz.github.io/).
- **Zero Telemetry:** 100% private, zero tracking, zero telemetry.

### 🚀 Build & Run Locally
```bash
# Build the project
dotnet build

# Publish single-file standalone executable
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./dist
```

---

## 🇹🇷 Türkçe Açıklama

**HaYTooL Weather**, Windows bildirim alanında (saatin yanında) sessizce çalışan, anlık sıcaklık derecesini ve hava durumu sembolünü dinamik olarak tepsi simgesi üzerinde gösteren ultra hafif, modern ve yerel bir hava durumu yazılımıdır.

### ✨ Öne Çıkan Özellikler
- **Dinamik Çift Alanlı Tepsi İkonu:** Saatin hemen yanında sol tarafta hava durumu ikonu (güneş, yağmur, kar, fırtına vb.), sağ tarafında ise dev sıcaklık derecesi (örn: `24°`) net ve yüksek çözünürlükte gösterilir.
- **Özelleştirilebilir Görünüm & Boyut:** Simge boyutu, rakam boyutu, özel arka plan rengi ve şeffaflık/opaklık oranları kaydırıcı çubuklarla (%40 - %200) canlı olarak ayarlanabilir.
- **Canlı Görev Çubuğu Simülasyonu:** Ayarlar panelinde saatin yanında simgelerin nasıl duracağını canlı olarak önizleyebilirsiniz.
- **Varsayılan Ayarlar:** İlk açılışta otomatik olarak **İstanbul, Türkiye** konumuyla gelir ve her **6 saatte bir** güncellenir.
- **Kolay Konum Değiştirme:** Kontrol Merkezi üzerinden Türkiye'nin 81 ili, tüm ilçeleri ve dünya şehirleri anında aranıp seçilebilir; 7 dilin başkentleri 1 tıkla anında seçilebilir.
- **Detaylı Hava Kartı:** Tıklandığında açılan modern kart ile nem, rüzgar, yağış ihtimali, 7 günlük tahminler ve web bağlantısını görüntüler.
- **Taşınabilir (Portable) `.ini` Yapılandırması:** Tüm ayarlar exe ile aynı dizindeki `HaYTooLWeather.ini` dosyasında saklanır.
- **7 Dil Desteği:** Türkçe (`tr`), English (`en`), Español (`es`), Deutsch (`de`), Português (`pt`), العربية (`ar`), Русский (`ru`).
- **🌐 HaYTooL PoRTaL:** Tüm HaYTooL masaüstü uygulamalarını tek bir merkezden keşfedin ve indirin: [https://haytokoraz.github.io/](https://haytokoraz.github.io/)
- **Sıfır Telemetri:** Hiçbir veri toplanmaz, %100 gizlilik odaklıdır.

---

## 🌐 HaYTooL Ecosystem / Ekosistem
- **HaYTooL PoRTaL:** [https://haytokoraz.github.io/](https://haytokoraz.github.io/)

---

## 📬 Contact & Support / İletişim & Destek
- **Developer / Geliştirici:** HaYTo
- **HaYTooL PoRTaL:** [https://haytokoraz.github.io/](https://haytokoraz.github.io/)
- **Email / E-posta:** [korazhayto@gmail.com](mailto:korazhayto@gmail.com)
- **X (Twitter):** [https://x.com/HaYTo](https://x.com/HaYTo)
- **GitHub:** [https://github.com/HaYToKoRaZ/HaYTooL-Weather](https://github.com/HaYToKoRaZ/HaYTooL-Weather)
- **License / Lisans:** MIT License
