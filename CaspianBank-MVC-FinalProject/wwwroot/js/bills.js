"use strict";

// Ödənişlər səhifəsi: təchizatçı seçimi, nömrə/kod axtarışı (JSON), məbləğ qısa yolları və təsdiq mətni.
// Burada yalnız göstəriş və sürətli yoxlama var; operator, abunəçi, məbləğ və cashback qaydaları API-dədir.
(function () {
  const form = document.getElementById("bill-form");
  if (!form) return;

  const providerInput = document.getElementById("bill-provider");
  const tiles = [...form.querySelectorAll("[data-provider]")];
  const identifier = document.getElementById("bill-identifier");
  const identifierLabel = document.getElementById("bill-identifier-label");
  const identifierHint = document.getElementById("bill-identifier-hint");
  const checkButton = document.getElementById("bill-check");
  const lookupError = document.getElementById("bill-lookup-error");
  const found = document.getElementById("bill-found");
  const payStep = document.getElementById("bill-pay-step");
  const amountField = document.getElementById("bill-amount-field");
  const amountInput = document.getElementById("bill-amount");
  const chipsBox = document.getElementById("bill-chips");
  const amountHint = document.getElementById("bill-amount-hint");
  const cardSelect = document.getElementById("bill-card");
  const cashbackNote = document.getElementById("bill-cashback-note");
  const recurringBox = document.getElementById("bill-recurring");
  const recurringText = document.getElementById("bill-recurring-text");
  const submit = document.getElementById("bill-submit");
  const token = form.querySelector('input[name="__RequestVerificationToken"]');

  const LOAN = "Loan";
  let current = null; // axtarışın son uğurlu nəticəsi

  function toNumber(text) {
    const value = parseFloat(String(text || "").trim().replace(/\s/g, "").replace(",", "."));
    return Number.isFinite(value) ? value : NaN;
  }

  function money(value) {
    return Number(value).toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " AZN";
  }

  function plain(value) {
    return Number(value).toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 });
  }

  // "2026-10" -> "Oct 2026"
  function periodText(period) {
    const parts = String(period || "").split("-");
    if (parts.length !== 2) return period || "";
    const months = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
    return (months[parseInt(parts[1], 10) - 1] || parts[1]) + " " + parts[0];
  }

  function el(tag, className, text) {
    const node = document.createElement(tag);
    if (className) node.className = className;
    if (text !== undefined && text !== null) node.textContent = text;
    return node;
  }

  function activeTile() {
    return tiles.find((tile) => tile.classList.contains("is-active")) || null;
  }

  function isLoan() {
    const tile = activeTile();
    return !!tile && tile.dataset.category === LOAN;
  }

  // ---------- təchizatçı seçimi ----------
  function selectProvider(tile) {
    tiles.forEach((item) => {
      const on = item === tile;
      item.classList.toggle("is-active", on);
      item.setAttribute("aria-pressed", on ? "true" : "false");
    });
    providerInput.value = tile.dataset.provider;
    identifierLabel.textContent = tile.dataset.label || "Number";
    identifier.placeholder = tile.dataset.hint || "";
    identifier.setAttribute("inputmode", tile.dataset.category === LOAN ? "text" : "numeric");

    if (recurringText) {
      recurringText.textContent = tile.dataset.category === LOAN
        ? "Pay this loan automatically every month. The installment is taken from the card you selected on its due date, the last day of the month, so there is never a late fee."
        : "Make this a monthly payment. It runs on the 1st, from the card you selected.";
    }

    if (tile.dataset.category === "Mobile") {
      identifierHint.textContent = tile.dataset.name + " numbers start with " + (tile.dataset.prefixes || "") + ". Example: " + tile.dataset.hint;
    } else if (tile.dataset.category === LOAN) {
      identifierHint.textContent = "The payment code is on the loan page. Example: " + tile.dataset.hint;
    } else {
      // "10 digits" -> "10-digit"
      const digits = (tile.dataset.hint || "").replace(/(\d+)\s*digits?/i, "$1-digit");
      identifierHint.textContent = "Enter the " + digits + " subscriber code.";
    }
  }

  function clearResult() {
    current = null;
    found.hidden = true;
    found.textContent = "";
    payStep.hidden = true;
  }

  function showError(text) {
    lookupError.textContent = text;
    lookupError.hidden = !text;
  }

  // ---------- axtarış ----------
  async function lookup() {
    clearResult();
    showError("");

    const text = identifier.value.trim();
    if (!text) {
      showError(isLoan() ? "Enter the loan code." : "Enter the number or subscriber code.");
      identifier.focus();
      return;
    }

    checkButton.disabled = true;
    checkButton.textContent = "Checking…";
    try {
      const body = new URLSearchParams();
      body.set("__RequestVerificationToken", token ? token.value : "");
      body.set("providerId", providerInput.value);
      body.set("identifier", text);

      const response = await fetch(form.dataset.lookupUrl, {
        method: "POST",
        credentials: "same-origin",
        headers: { "Content-Type": "application/x-www-form-urlencoded" },
        body
      });
      if (response.status === 401) {
        window.location.reload();
        return;
      }
      if (!response.ok) {
        showError("We can't check this right now. Please try again.");
        return;
      }

      const data = await response.json();
      if (!data.ok) {
        showError(data.error || "We can't check this right now. Please try again.");
        return;
      }
      render(data.lookup, false);
    } catch (error) {
      showError("We can't reach the service. Please try again.");
    } finally {
      checkButton.disabled = false;
      checkButton.textContent = "Check";
    }
  }

  // ---------- nəticə kartı ----------
  function row(list, term, value, className) {
    const wrap = el("div");
    wrap.append(el("dt", null, term), el("dd", className || "", value));
    list.append(wrap);
  }

  function render(data, keepAmount) {
    current = data;
    found.textContent = "";

    // Sahədə API-nin tanıdığı yazılış görünsün (055 123 45 67, KR-2026-000123)
    if (data.identifier) identifier.value = data.identifier;

    const head = el("div", "bill-found-head");
    if (data.logoUrl) {
      const logo = el("img", "provider-logo");
      logo.src = data.logoUrl;
      logo.alt = "";
      logo.width = 32;
      logo.height = 32;
      head.append(logo);
    } else {
      head.append(el("span", "provider-mark", "KR"));
    }
    const who = el("div", "bill-found-who");
    who.append(el("strong", null, data.subscriberName), el("span", "num", data.providerName + " · " + data.identifier));
    head.append(who);

    const mobile = data.category === "Mobile";
    let badge;
    if (data.debt > 0) {
      badge = el("span", "badge badge-danger", (data.category === LOAN ? "Due " : "Debt ") + money(data.debt));
    } else if (mobile) {
      badge = el("span", "badge badge-neutral", "Balance " + money(data.credit));
    } else if (data.credit > 0) {
      badge = el("span", "badge badge-success", "Prepaid " + money(data.credit));
    } else {
      badge = el("span", "badge badge-success", "No debt");
    }
    head.append(badge);
    found.append(head);

    const list = el("dl", "detail-list");
    if (data.packageName) {
      row(list, "Package", data.packageName);
    }
    if (data.monthlyFee) {
      row(list, "Monthly fee", money(data.monthlyFee), "num");
    }
    if (data.lastChargePeriod && data.lastChargeAmount !== null && data.lastChargeAmount !== undefined) {
      const usage = data.lastChargeUsage && data.unitName ? plain(data.lastChargeUsage) + " " + data.unitName + " · " : "";
      row(list, "Last bill", periodText(data.lastChargePeriod) + " · " + usage + money(data.lastChargeAmount), "num");
    }
    if (!mobile && data.credit > 0 && data.debt <= 0) {
      row(list, "Prepaid balance", money(data.credit), "num");
    }
    if (list.children.length > 0) found.append(list);
    found.hidden = false;

    setupPay(data, keepAmount);
  }

  // ---------- ödəniş addımı ----------
  function setupPay(data, keepAmount) {
    const fixed = !!data.fixedAmount;
    amountField.hidden = fixed;
    amountInput.disabled = fixed;

    if (!keepAmount) {
      amountInput.value = !fixed && data.debt > 0 ? String(data.debt.toFixed(2)) : "";
    }

    chipsBox.textContent = "";
    if (!fixed) {
      const amounts = [];
      if (data.debt > 0) amounts.push({ value: data.debt, label: "Debt " + plain(data.debt) });
      [5, 10, 20, 50].forEach((value) => {
        if (value >= data.minPayment && value <= data.maxPayment && !amounts.some((item) => Math.abs(item.value - value) < 0.005)) {
          amounts.push({ value, label: String(value) });
        }
      });
      amounts.forEach((item) => {
        const chip = el("button", "chip", item.label);
        chip.type = "button";
        chip.addEventListener("click", () => {
          amountInput.value = item.value.toFixed(2);
          paintPay();
        });
        chipsBox.append(chip);
      });
      amountHint.textContent = "Between " + plain(data.minPayment) + " and " + plain(data.maxPayment) + " AZN.";
    } else {
      amountHint.textContent = "";
    }

    payStep.hidden = false;
    paintPay();
  }

  function cardTier() {
    if (!cardSelect) return "";
    const option = cardSelect.options[cardSelect.selectedIndex];
    return option ? option.getAttribute("data-tier") || "" : "";
  }

  function paintPay() {
    if (!current || !submit) return;

    const fixed = !!current.fixedAmount;
    const value = fixed ? current.debt : toNumber(amountInput.value);
    const valid = Number.isFinite(value) && value > 0;

    if (cashbackNote) cashbackNote.hidden = cardTier().toLowerCase() !== "cashback";

    // Seçilmiş məbləğə uyğun chip işıqlanır
    [...chipsBox.children].forEach((chip) => {
      const chipValue = toNumber(chip.textContent.replace("Debt ", ""));
      chip.classList.toggle("is-active", valid && Math.abs(chipValue - value) < 0.005);
    });

    if (!valid) {
      submit.textContent = "Pay";
      submit.setAttribute("data-confirm-text", "");
      return;
    }

    submit.textContent = "Pay " + money(value);
    let text = money(value) + " will be taken from your card";
    if (current.category === LOAN) {
      text += " and paid to loan " + current.identifier + ".";
    } else if (current.category === "Mobile") {
      text += " and added to the balance of " + current.identifier + ".";
    } else if (current.debt > 0) {
      text += " and paid to " + current.providerName + " for " + current.subscriberName + ".";
    } else {
      text += " and added to the balance of " + current.subscriberName + " at " + current.providerName + ".";
    }
    if (recurringBox && recurringBox.checked) {
      text += current.category === LOAN ? " It will repeat on the last day of every month, when the installment is due." : " It will repeat every month on the 1st.";
    }
    submit.setAttribute("data-confirm-text", text);
    submit.removeAttribute("data-confirmed");
    delete submit.dataset.confirmed;
  }

  // ---------- hadisələr ----------
  tiles.forEach((tile) => {
    tile.addEventListener("click", () => {
      if (tile.classList.contains("is-active")) return;

      // Mobil operatordan operatora keçəndə nömrə qalır (səhv operator seçilmiş ola bilər), qalan hallarda kod başqa təchizatçıya aid deyil
      const previous = activeTile();
      const keepNumber = !!previous && previous.dataset.category === "Mobile" && tile.dataset.category === "Mobile";
      selectProvider(tile);
      if (!keepNumber) identifier.value = "";
      clearResult();
      showError("");
      identifier.focus();
    });
  });

  // Nömrə dəyişəndə köhnə nəticə etibarsızdır: yenidən "Check" lazımdır
  identifier.addEventListener("input", () => {
    if (current) clearResult();
    showError("");
  });
  identifier.addEventListener("keydown", (event) => {
    if (event.key === "Enter") {
      event.preventDefault();
      lookup();
    }
  });
  checkButton.addEventListener("click", lookup);
  amountInput?.addEventListener("input", paintPay);
  cardSelect?.addEventListener("change", paintPay);
  recurringBox?.addEventListener("change", paintPay);

  // Axtarışsız və ya düzgün məbləğsiz təsdiq pəncərəsi açılmasın
  submit?.addEventListener("click", (event) => {
    if (!current) {
      event.preventDefault();
      event.stopPropagation();
      showError("Check the number first.");
      return;
    }
    if (!current.fixedAmount) {
      const value = toNumber(amountInput.value);
      if (!Number.isFinite(value) || value <= 0) {
        event.preventDefault();
        event.stopPropagation();
        amountHint.textContent = "Enter an amount.";
        amountInput.focus();
      }
    }
  }, true);

  form.addEventListener("keydown", (event) => {
    if (event.key === "Enter" && event.target === amountInput) {
      event.preventDefault();
      submit?.click();
    }
  });

  // ---------- açılış ----------
  const selectedTile = tiles.find((tile) => tile.dataset.provider === providerInput.value) || tiles[0];
  if (selectedTile) selectProvider(selectedTile);

  // Ödəniş uğursuz olub səhifə yenidən açılıbsa: əvvəlki nəticə və yazılan məbləğ qalır
  if (form.dataset.initial) {
    try {
      render(JSON.parse(form.dataset.initial), true);
    } catch (error) {
      clearResult();
    }
  }
})();
