"use strict";

// Ağ/qara tema düyməsi. Seçim brauzerdə saxlanılır (localStorage); seçilməyibsə sistemin temasına baxılır.
// İlk rəng <head>-dəki kiçik skriptlə qoyulur (səhifə yanıb-sönməsin), bu fayl yalnız düymələri işlədir.
(function () {
  const KEY = "cb-theme";
  const root = document.documentElement;

  function current() {
    return root.getAttribute("data-theme") === "dark" ? "dark" : "light";
  }

  function apply(theme) {
    root.setAttribute("data-theme", theme);
    const meta = document.querySelector('meta[name="theme-color"]');
    if (meta) meta.setAttribute("content", theme === "dark" ? "#0d1310" : "#0F5C3F");
    document.querySelectorAll("[data-theme-toggle]").forEach((button) => {
      button.setAttribute("aria-pressed", theme === "dark" ? "true" : "false");
      button.setAttribute("title", theme === "dark" ? button.getAttribute("data-label-light") || "Light theme" : button.getAttribute("data-label-dark") || "Dark theme");
    });
  }

  document.addEventListener("click", (event) => {
    const button = event.target.closest("[data-theme-toggle]");
    if (!button) return;
    const next = current() === "dark" ? "light" : "dark";
    try { localStorage.setItem(KEY, next); } catch (e) { /* saxlanmasa da tema dəyişir */ }
    apply(next);
  });

  // Dil menyusu kənara klikləndə bağlanır
  document.addEventListener("click", (event) => {
    document.querySelectorAll("details.lang-switch[open]").forEach((menu) => {
      if (!menu.contains(event.target)) menu.removeAttribute("open");
    });
  });

  apply(current());
})();
