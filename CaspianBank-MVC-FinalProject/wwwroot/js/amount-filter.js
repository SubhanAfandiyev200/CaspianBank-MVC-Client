"use strict";

// Məbləğ sahələrinə (inputmode="decimal") yalnız rəqəm və bir ondalıq ayırıcı yazılır, vergüldən sonra ən çox 2 rəqəm.
// Capture ilə sənədin üstündə dinlənir: səhifənin öz "input" dinləyiciləri artıq təmizlənmiş dəyəri görür
(function () {
  document.addEventListener("input", function (event) {
    const field = event.target;
    if (!(field instanceof HTMLInputElement) || field.getAttribute("inputmode") !== "decimal") return;

    let cleaned = field.value.replace(",", ".").replace(/[^0-9.]/g, "");
    const dot = cleaned.indexOf(".");
    if (dot >= 0) {
      cleaned = cleaned.slice(0, dot + 1) + cleaned.slice(dot + 1).replace(/\./g, "").slice(0, 2);
    }
    if (cleaned !== field.value) field.value = cleaned;
  }, true);
})();
