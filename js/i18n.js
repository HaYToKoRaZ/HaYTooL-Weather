import { translations } from "./translations.js";

const langConfigs = {
  tr: { flag: "assets/flags/tr.png", text: "TR" },
  en: { flag: "assets/flags/gb.png", text: "EN" },
  es: { flag: "assets/flags/es.png", text: "ES" },
  de: { flag: "assets/flags/de.png", text: "DE" },
  pt: { flag: "assets/flags/pt.png", text: "PT" },
  ar: { flag: "assets/flags/sa.png", text: "AR" },
  ru: { flag: "assets/flags/ru.png", text: "RU" }
};

let currentLang = "tr";

/**
 * Aktif arayüz dilini ve RTL/LTR yönünü günceller.
 * @param {string} lang - tr, en, es, de, pt, ar, ru
 */
export function setLanguage(lang) {
  if (!translations[lang]) return;
  currentLang = lang;

  // RTL/LTR Yönetimi (Arapça için RTL)
  if (lang === "ar") {
    document.documentElement.setAttribute("dir", "rtl");
    document.documentElement.setAttribute("lang", "ar");
  } else {
    document.documentElement.setAttribute("dir", "ltr");
    document.documentElement.setAttribute("lang", lang);
  }

  // Metin içeriklerini data-i18n anahtarlarına göre güncelle
  const langData = translations[lang];
  document.querySelectorAll("[data-i18n]").forEach(el => {
    const key = el.getAttribute("data-i18n");
    if (langData[key]) {
      el.textContent = langData[key];
    }
  });

  // Açılır listedeki aktif öğeyi güncelle
  document.querySelectorAll(".dropdown-item[data-lang]").forEach(item => {
    item.classList.toggle("active", item.getAttribute("data-lang") === lang);
  });

  // Seçili butonun bayrağını ve metnini güncelle
  const config = langConfigs[lang] || langConfigs.tr;
  const currentFlagEl = document.getElementById("currentLangFlag");
  if (currentFlagEl) currentFlagEl.src = config.flag;
  
  const currentTextEl = document.getElementById("currentLangText");
  if (currentTextEl) currentTextEl.textContent = config.text;

  localStorage.setItem("haytool_weather_lang", lang);
}

/**
 * Dil seçici açılır menüsünü ve tıklama dinleyicilerini başlatır.
 */
export function initLanguageSelector() {
  const langBtn = document.getElementById("langBtn");
  const langDropdown = document.getElementById("langDropdown");

  if (!langBtn || !langDropdown) return;

  langBtn.addEventListener("click", (e) => {
    e.stopPropagation();
    // Diğer açık menü varsa kapat
    const otherMenu = document.getElementById("themeDropdown");
    if (otherMenu) otherMenu.classList.remove("show");
    langDropdown.classList.toggle("show");
  });

  document.addEventListener("click", (e) => {
    if (!langBtn.contains(e.target) && !langDropdown.contains(e.target)) {
      langDropdown.classList.remove("show");
    }
  });

  langDropdown.addEventListener("click", (e) => {
    e.stopPropagation();
    const item = e.target.closest(".dropdown-item[data-lang]");
    if (item) {
      e.preventDefault();
      const lang = item.getAttribute("data-lang");
      if (lang) {
        setLanguage(lang);
        langDropdown.classList.remove("show");
      }
    }
  });

  // Kayıtlı dili veya varsayılan Türkçeyi yükle
  const savedLang = localStorage.getItem("haytool_weather_lang") || "tr";
  setLanguage(savedLang);
}

// Global window erişimi (inline tıklamalar ve her ortam için kesintisiz çalışma garantisi)
if (typeof window !== "undefined") {
  window.setLanguage = setLanguage;
  window.currentLang = currentLang;
}
