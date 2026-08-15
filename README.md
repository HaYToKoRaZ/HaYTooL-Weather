# 🌤️ HaYTooL Weather

<div align="center">
  <p><strong>Windows Sistem Tepsisi (Görev Çubuğu) Dinamik Hava Durumu Uygulaması</strong></p>
  <p><em>Lightweight, Dynamic Weather Application for Windows System Tray</em></p>
  <p>👨‍💻 <strong>Geliştirici / Developer:</strong> HaYTo</p>
  <p>🌐 <strong>GitHub Repository:</strong> <a href="https://github.com/HaYToKoRaZ/HaYTooL-Weather">HaYToKoRaZ/HaYTooL-Weather</a></p>
</div>

---

## 🇹🇷 Türkçe Açıklama

**HaYTooL Weather**, Windows bildirim alanında (saatin yanında) sessizce çalışan, anlık sıcaklık derecesini ve hava durumu sembolünü dinamik olarak tepsi simgesi üzerinde gösteren ultra hafif, modern ve yerel bir hava durumu yazılımıdır.

### ✨ Öne Çıkan Özellikler
- **Dinamik Çift Alanlı Tepsi İkonu:** Saatin hemen yanında sol tarafta hava durumu ikonu (güneş, yağmur, kar, fırtına vb.), sağ tarafında ise dev sıcaklık derecesi (örn: `24°`) net ve yüksek çözünürlükte gösterilir.
- **Özelleştirilebilir Görünüm & Boyut:** Simge boyutu, rakam boyutu, özel arka plan rengi ve şeffaflık/opaklık oranları kaydırıcı çubuklarla (%40 - %200) canlı olarak ayarlanabilir.
- **Canlı Görev Çubuğu Simülasyonu:** Ayarlar panelinde saatin yanında simgelerin nasıl duracağını canlı olarak önizleyebilirsiniz.
- **Varsayılan Ayarlar:** İlk açılışta otomatik olarak **İstanbul, Türkiye** konumuyla gelir ve her **6 saatte bir** güncellenir.
- **Kolay Konum Değiştirme:** Kontrol Merkezi üzerinden Türkiye'nin 81 ili, tüm ilçeleri ve dünya şehirleri anında aranıp seçilebilir; 7 dilin başkentleri 1 tıkla anında seçilebilir.
- **Detaylı Hava Kartı:** Tıklandığında açılan modern kart ile nem, rüzgar, yağış ihtimali ve 7 günlük tahminleri görüntüler.
- **Taşınabilir (Portable) `.ini` Yapılandırması:** Tüm ayarlar exe ile aynı dizindeki `HaYTooLWeather.ini` dosyasında saklanır.
- **7 Dil Desteği:** Türkçe (`tr`), English (`en`), Español (`es`), Deutsch (`de`), Português (`pt`), العربية (`ar`), Русский (`ru`).
- **Sıfır Telemetri:** Hiçbir veri toplanmaz, %100 gizlilik odaklıdır.

### 🚀 Çalıştırma & Derleme
```bash
# Projeyi derleme
dotnet build

# Bağımsız tekil exe üretme
dotnet publish -c Release -r win-x64 --self-contained false -p:PublishSingleFile=true -o ./dist
```

---

## 🇬🇧 English Description

**HaYTooL Weather** is an ultra-lightweight, modern weather application that runs in the Windows Notification Area (System Tray, next to the clock) displaying live temperature and weather condition icons dynamically.

### ✨ Key Features
- **Dynamic Dual Tray Icons:** High-DPI rendered weather condition icon on the left and large temperature digits on the right.
- **Customizable Appearance & Size:** Individual slider controls for symbol size, font size, custom background colors, and opacity levels (%40 - %200).
- **Live Taskbar Preview Simulator:** Real-time preview inside the Control Center showing how icons look on your actual taskbar.
- **Default Location & Frequency:** Starts with **Istanbul, Turkey** by default with a **6-hour** automatic update interval.
- **Instant Location Search:** Search and switch between provinces, districts, and global cities via the integrated search engine and capital city chips.
- **Detailed Weather Card:** Click to view a sleek popup with humidity, wind speed, precipitation chance, and 7-day forecast.
- **Portable `.ini` Configuration:** Settings stored in `HaYTooLWeather.ini` in the executable folder.
- **7 Languages Supported:** TR, EN, ES, DE, PT, AR, RU.
- **Zero Telemetry:** 100% private, no tracking.

---

## 📬 İletişim / Contact
- **Geliştirici / Developer:** HaYTo
- **E-posta / Email:** [korazhayto@gmail.com](mailto:korazhayto@gmail.com)
- **X (Twitter):** [https://x.com/HaYTo](https://x.com/HaYTo)
- **GitHub:** [https://github.com/HaYToKoRaZ/HaYTooL-Weather](https://github.com/HaYToKoRaZ/HaYTooL-Weather)
- **Lisans / License:** MIT License
