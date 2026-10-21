"use strict";

// Aylıq köçürmə formu: "öz kartıma" / "başqa karta" seçimi və kart nömrəsinin 4-4 qruplanması.
// Qaydalar (alıcı, limit, komissiya) API-dədir; burada yalnız göstəriş var.
(function () {
  const form = document.getElementById("recurring-form");
  if (!form) return;

  const modeInput = document.getElementById("rec-mode");
  const ownBtn = document.getElementById("rec-own-btn");
  const otherBtn = document.getElementById("rec-other-btn");
  const ownField = document.getElementById("rec-own-field");
  const otherField = document.getElementById("rec-other-field");
  const number = document.getElementById("rec-number");
  const amount = document.getElementById("rec-amount");
  if (!modeInput || !ownBtn || !otherBtn) return;

  function setMode(mode) {
    modeInput.value = mode;
    ownBtn.classList.toggle("is-active", mode === "own");
    otherBtn.classList.toggle("is-active", mode === "other");
    ownField.hidden = mode !== "own";
    otherField.hidden = mode !== "other";
  }

  ownBtn.addEventListener("click", () => { if (!ownBtn.disabled) setMode("own"); });
  otherBtn.addEventListener("click", () => setMode("other"));

  number?.addEventListener("input", () => {
    number.value = number.value.replace(/\D/g, "").slice(0, 16).replace(/(\d{4})(?=\d)/g, "$1 ").trim();
  });

  // Məbləğ: yalnız rəqəm və bir ayırıcı, ayırıcıdan sonra ən çox 2 rəqəm
  amount?.addEventListener("input", () => {
    let value = amount.value.replace(/[^0-9.,]/g, "");
    const sep = value.search(/[.,]/);
    if (sep !== -1) {
      value = value.slice(0, sep + 1) + value.slice(sep + 1).replace(/[.,]/g, "").slice(0, 2);
    }
    if (value !== amount.value) amount.value = value;
  });

  setMode(modeInput.value === "own" ? "own" : "other");
})();
