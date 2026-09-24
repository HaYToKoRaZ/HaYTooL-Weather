/**
 * HaYTooL Weather Website - Ana Uygulama Modülü
 * Modüler Mimari:
 *   - js/translations.js : 7 Dil Çeviri Veritabanı
 *   - js/i18n.js         : Dil Yönetimi ve Dinamik Arayüz Güncelleyici
 *   - js/simulator.js    : Canlı Windows Görev Çubuğu Simülatörü
 */

import { initLanguageSelector } from "./js/i18n.js";
import { initSimulator } from "./js/simulator.js";

document.addEventListener("DOMContentLoaded", () => {
  // 1. Çoklu dil modülünü başlat
  initLanguageSelector();

  // 2. İnteraktif görev çubuğu simülatörünü başlat
  initSimulator();
});
