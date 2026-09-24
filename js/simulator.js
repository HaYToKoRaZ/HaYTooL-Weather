const weatherDataMap = {
  sunny: { icon: "☀️", temp: "24°", label: "Sunny / Güneşli" },
  cloudy: { icon: "⛅", temp: "19°", label: "Partly Cloudy / Parçalı Bulutlu" },
  rainy: { icon: "🌧️", temp: "14°", label: "Rainy / Yağmurlu" },
  snowy: { icon: "❄️", temp: "-2°", label: "Snowy / Karlı" },
  storm: { icon: "⛈️", temp: "17°", label: "Thunderstorm / Fırtına" }
};

/**
 * Görev çubuğu simülasyonundaki ikon, derece ve boyutları günceller.
 */
export function updateSimulator() {
  const weatherTypeSelect = document.getElementById("simWeatherSelect");
  const iconScaleInput = document.getElementById("simIconScale");
  const textScaleInput = document.getElementById("simTextScale");

  if (!weatherTypeSelect || !iconScaleInput || !textScaleInput) return;

  const weatherType = weatherTypeSelect.value;
  const iconScale = iconScaleInput.value;
  const textScale = textScaleInput.value;

  const data = weatherDataMap[weatherType] || weatherDataMap.sunny;
  const iconEl = document.getElementById("simTrayIcon");
  const tempEl = document.getElementById("simTrayTemp");

  if (iconEl) {
    iconEl.textContent = data.icon;
    iconEl.style.transform = `scale(${iconScale / 100})`;
  }

  if (tempEl) {
    tempEl.textContent = data.temp;
    tempEl.style.fontSize = `${(textScale / 100) * 1.15}rem`;
  }
}

/**
 * Görev çubuğunda canlı saati ve tarihi günceller.
 */
export function updateSimulatorClock() {
  const clockEl = document.getElementById("simClock");
  if (!clockEl) return;
  const now = new Date();
  const timeStr = now.toLocaleTimeString([], { hour: '2-digit', minute: '2-digit' });
  const dateStr = now.toLocaleDateString([], { day: '2-digit', month: '2-digit', year: 'numeric' });
  clockEl.innerHTML = `${timeStr}<br>${dateStr}`;
}

/**
 * Görev çubuğu simülatör olay dinleyicilerini başlatır.
 */
export function initSimulator() {
  const weatherSelect = document.getElementById("simWeatherSelect");
  const iconScale = document.getElementById("simIconScale");
  const textScale = document.getElementById("simTextScale");

  if (weatherSelect) weatherSelect.addEventListener("change", updateSimulator);
  if (iconScale) iconScale.addEventListener("input", updateSimulator);
  if (textScale) textScale.addEventListener("input", updateSimulator);

  updateSimulator();
  updateSimulatorClock();
  setInterval(updateSimulatorClock, 1000);
}
