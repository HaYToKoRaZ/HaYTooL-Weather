/**
 * HaYTooL Weather Website - Ana Uygulama Modülü
 * Modüler Mimari:
 *   - js/translations.js : 7 Dil Çeviri Veritabanı
 *   - js/i18n.js         : Dil Yönetimi ve Dinamik Arayüz Güncelleyici
 *   - js/simulator.js    : Canlı Windows Görev Çubuğu Simülatörü
 */

import { initLanguageSelector } from "./js/i18n.js";
import { initSimulator } from "./js/simulator.js";
import { initThemeSelector } from "./js/theme.js";

function startApp() {
  try {
    initLanguageSelector();
    initThemeSelector();
    initSimulator();
  } catch (err) {
    console.error("HaYTooL Weather init error:", err);
  }
}

if (document.readyState === "loading") {
  document.addEventListener("DOMContentLoaded", startApp);
} else {
  startApp();
}

