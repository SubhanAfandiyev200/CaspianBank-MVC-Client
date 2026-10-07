"use strict";

// Admin panelinin ortaq davranışı: mobil menyu və bildiriş zəngi (açılan panel)
(function () {
  // ---------- mobil menyu ----------
  const toggle = document.querySelector("[data-nav-toggle]");
  const bar = document.querySelector(".topnav");
  toggle?.addEventListener("click", () => {
    const open = bar.classList.toggle("is-open");
    toggle.setAttribute("aria-expanded", String(open));
    toggle.setAttribute("aria-label", open ? "Close menu" : "Open menu");
  });

  // ---------- açılan panellər (bildirişlər) ----------
  function closeDropdowns(except) {
    document.querySelectorAll("[data-dropdown].is-open").forEach((dropdown) => {
      if (dropdown === except) return;
      dropdown.classList.remove("is-open");
      dropdown.querySelector("[data-dropdown-toggle]")?.setAttribute("aria-expanded", "false");
    });
  }

  document.addEventListener("click", (event) => {
    const button = event.target.closest("[data-dropdown-toggle]");
    if (button) {
      const dropdown = button.closest("[data-dropdown]");
      const open = !dropdown.classList.contains("is-open");
      closeDropdowns(dropdown);
      dropdown.classList.toggle("is-open", open);
      button.setAttribute("aria-expanded", String(open));
      return;
    }
    if (!event.target.closest("[data-dropdown]")) closeDropdowns();
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") closeDropdowns();
  });
})();
