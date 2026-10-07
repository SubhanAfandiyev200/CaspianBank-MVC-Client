"use strict";

// App səhifələrinin ortaq davranışı: menyu, kart dəsti (deck), pəncərələr (modal), kopyalama, kart növü seçimi.
// (script.js / app.js saxta CaspianStore ilə işləyir və bu səhifələrdə yüklənmir)
(function () {
  // ---------- mobil menyu ----------
  const toggle = document.querySelector("[data-nav-toggle]");
  const bar = document.querySelector(".topnav");
  toggle?.addEventListener("click", () => {
    const open = bar.classList.toggle("is-open");
    toggle.setAttribute("aria-expanded", String(open));
    toggle.setAttribute("aria-label", open ? "Close menu" : "Open menu");
  });

  // ---------- pəncərələr ----------
  function openModal(modal) {
    if (!modal) return;
    modal.hidden = false;
    document.body.classList.add("modal-open");
    modal.querySelector("input, textarea, select, button")?.focus();
  }

  function closeModal(modal) {
    if (!modal) return;
    modal.hidden = true;
    if (!document.querySelector(".modal:not([hidden])")) document.body.classList.remove("modal-open");
  }

  document.addEventListener("click", (event) => {
    const opener = event.target.closest("[data-open-modal]");
    if (opener) {
      openModal(document.getElementById(opener.getAttribute("data-open-modal")));
      return;
    }
    if (event.target.closest("[data-close-modal]")) {
      closeModal(event.target.closest(".modal"));
    }
  });

  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") closeModal(document.querySelector(".modal:not([hidden])"));
  });

  // Server xəta qaytarıbsa (yanlış parol və s.) pəncərə yenidən açılır
  document.querySelectorAll(".modal[data-open-on-load='true']").forEach(openModal);

  // ---------- kopyalama ----------
  document.addEventListener("click", (event) => {
    const button = event.target.closest("[data-copy]");
    if (!button) return;
    const text = button.getAttribute("data-copy") || "";
    const done = () => {
      const old = button.textContent;
      button.textContent = "Copied";
      setTimeout(() => { button.textContent = old; }, 1400);
    };
    if (navigator.clipboard?.writeText) {
      navigator.clipboard.writeText(text).then(done).catch(() => {});
    }
  });

  // ---------- kart dəsti (üst-üstə yığılmış kartlar) ----------
  function mountDeck(stage) {
    const cards = [...stage.querySelectorAll(".plastic")];
    if (!cards.length) return;
    const dots = stage.parentElement.querySelector("[data-deck-dots]");
    let index = 0;
    let startX = 0;
    let moved = false;
    let tracking = false;

    function paint() {
      const count = cards.length;
      cards.forEach((card, i) => {
        let behind = i - index;
        if (behind < 0) behind += count;
        const visible = behind < Math.min(3, count);
        card.style.zIndex = String(20 - behind);
        card.style.opacity = visible ? "1" : "0";
        card.style.pointerEvents = behind === 0 ? "auto" : "none";
        card.style.transform = visible
          ? "translate(" + behind * 16 + "px, " + behind * -14 + "px) scale(" + (1 - behind * 0.045) + ")"
          : "translate(28px, -32px) scale(0.86)";
        card.toggleAttribute("aria-hidden", behind !== 0);
      });
      if (dots) {
        dots.innerHTML = cards.map((_, i) =>
          '<button type="button" class="deck-dot' + (i === index ? " is-active" : "") + '" data-deck-go="' + i + '" aria-label="Card ' + (i + 1) + '"></button>'
        ).join("");
      }
    }

    function step(delta) {
      index = (index + delta + cards.length) % cards.length;
      paint();
    }

    stage.addEventListener("pointerdown", (event) => {
      if (event.button !== 0) return;
      startX = event.clientX;
      tracking = true;
      moved = false;
    });
    stage.addEventListener("pointermove", (event) => {
      if (tracking && Math.abs(event.clientX - startX) > 40) moved = true;
    });
    stage.addEventListener("pointerup", (event) => {
      if (!tracking) return;
      tracking = false;
      if (!moved) return;
      const dx = event.clientX - startX;
      if (dx <= -40) step(1);
      else if (dx >= 40) step(-1);
    });
    stage.addEventListener("pointercancel", () => { tracking = false; });
    // Sürüşdürmə kartın linkini işə salmasın
    stage.addEventListener("click", (event) => {
      if (!moved) return;
      event.preventDefault();
      event.stopPropagation();
      moved = false;
    }, true);
    stage.addEventListener("keydown", (event) => {
      if (event.key === "ArrowRight") step(1);
      if (event.key === "ArrowLeft") step(-1);
    });
    stage.parentElement.addEventListener("click", (event) => {
      if (event.target.closest("[data-deck-prev]")) step(-1);
      if (event.target.closest("[data-deck-next]")) step(1);
      const dot = event.target.closest("[data-deck-go]");
      if (dot) {
        index = Number(dot.getAttribute("data-deck-go")) || 0;
        paint();
      }
    });
    paint();
  }

  document.querySelectorAll("[data-deck]").forEach(mountDeck);

  // ---------- kart şəkilli seçim (select[data-card-pick]) ----------
  function esc(value) {
    const span = document.createElement("span");
    span.textContent = value == null ? "" : String(value);
    return span.innerHTML.replace(/"/g, "&quot;");
  }

  // Kiçik kart: şəkil bazadakı dizayndan, mətn rəngləri növün sinfindən (CSS)
  function thumb(option) {
    const tier = option.getAttribute("data-tier") || "standard";
    const design = option.getAttribute("data-design") || "";
    const last4 = option.getAttribute("data-last4") || "";
    const style = design ? ' style="background-image:url(\'' + esc(design) + '\');background-size:cover;background-position:center"' : "";
    return '<span class="card-thumb-frame" aria-hidden="true"><span class="plastic plastic-' + esc(tier.toLowerCase()) + ' card-thumb-scale"' + style + '>'
      + '<span class="plastic-art" aria-hidden="true"></span><span class="plastic-shine" aria-hidden="true"></span>'
      + '<div class="plastic-top"><span class="plastic-brand"><span class="plastic-mark" aria-hidden="true"></span><span class="plastic-bank">Caspian</span></span>'
      + '<span class="plastic-tier">' + esc(tier) + '</span></div>'
      + '<div class="plastic-mid"><span class="plastic-chip" aria-hidden="true"></span></div>'
      + '<p class="plastic-number">•••• ' + esc(last4) + '</p>'
      + '<div class="plastic-bottom"><span class="plastic-issuer">Caspian Bank</span></div></span></span>';
  }

  function mountCardPicks() {
    const chevron = '<svg class="card-pick-chevron" width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><path d="M4 6l4 4 4-4"/></svg>';

    document.querySelectorAll("select[data-card-pick]").forEach((select) => {
      const wrap = document.createElement("div");
      wrap.className = "card-pick";
      select.parentNode.insertBefore(wrap, select);
      wrap.appendChild(select);
      select.classList.add("card-pick-native");
      select.tabIndex = -1;

      const button = document.createElement("button");
      button.type = "button";
      button.className = "card-pick-btn";
      button.setAttribute("aria-haspopup", "listbox");
      button.setAttribute("aria-expanded", "false");
      const menu = document.createElement("div");
      menu.className = "card-pick-menu";
      menu.hidden = true;
      menu.setAttribute("role", "listbox");
      wrap.append(button, menu);

      function row(option, withChevron) {
        const tier = option.getAttribute("data-tier") || option.text;
        const last4 = option.getAttribute("data-last4");
        return thumb(option)
          + '<span class="card-pick-copy"><strong>' + esc(tier) + '</strong>' + (last4 ? '<span>•••• ' + esc(last4) + '</span>' : "") + '</span>'
          + '<span class="card-pick-bal">' + esc(option.getAttribute("data-money") || "") + '</span>'
          + (withChevron ? chevron : "");
      }

      function sync() {
        const current = select.options[select.selectedIndex] || null;
        button.innerHTML = current ? row(current, true) : '<span class="card-pick-copy"><strong>Choose a card</strong></span>' + chevron;
        menu.innerHTML = [...select.options].map((option) =>
          '<button type="button" class="card-pick-option' + (option === current ? " is-selected" : "") + '" data-value="' + esc(option.value) + '" role="option" aria-selected="' + (option === current) + '">' + row(option, false) + '</button>'
        ).join("");
      }

      function close() {
        wrap.classList.remove("is-open");
        menu.hidden = true;
        button.setAttribute("aria-expanded", "false");
      }

      button.addEventListener("click", () => {
        const opening = !wrap.classList.contains("is-open");
        document.querySelectorAll(".card-pick.is-open").forEach((pick) => pick.querySelector(".card-pick-btn")?.click());
        if (opening) {
          wrap.classList.add("is-open");
          menu.hidden = false;
          button.setAttribute("aria-expanded", "true");
        }
      });

      menu.addEventListener("click", (event) => {
        const choice = event.target.closest("[data-value]");
        if (!choice) return;
        select.value = choice.getAttribute("data-value");
        select.dispatchEvent(new Event("change", { bubbles: true }));
        close();
        sync();
      });

      document.addEventListener("click", (event) => { if (!wrap.contains(event.target)) close(); });
      document.addEventListener("keydown", (event) => { if (event.key === "Escape") close(); });
      select._pickSync = sync;
      new MutationObserver(sync).observe(select, { childList: true });
      sync();
    });
  }

  mountCardPicks();

  // ---------- add-card: növ seçimi ----------
  const tiers = document.getElementById("tiers");
  const form = document.getElementById("tier-form");
  if (tiers && form) {
    const input = document.getElementById("tier-input");
    const select = document.getElementById("fee-card");
    const heading = document.getElementById("tier-heading");
    const copy = document.getElementById("tier-copy");
    const error = document.getElementById("fee-error");
    const submit = document.getElementById("tier-submit");
    let fee = 0;

    function money(value) {
      return value.toLocaleString("en-GB", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " AZN";
    }

    // Balans kifayət etmirsə düymə söndürülür (yoxlama əsasən serverdədir, bu yalnız rahatlıq üçündür)
    function checkBalance() {
      const option = select.options[select.selectedIndex];
      const balance = option ? Number(option.getAttribute("data-balance")) : 0;
      const enough = !!option && balance >= fee;
      error.hidden = enough;
      submit.disabled = !enough;
    }

    tiers.addEventListener("click", (event) => {
      const button = event.target.closest("[data-tier]");
      if (!button) return;
      tiers.querySelectorAll(".tier").forEach((tier) => tier.classList.toggle("is-selected", tier === button));
      fee = Number(button.getAttribute("data-fee")) || 0;
      input.value = button.getAttribute("data-name");
      heading.textContent = button.getAttribute("data-name");
      copy.textContent = fee > 0
        ? "The opening fee is " + money(fee) + ", taken from the card you choose."
        : "There is no opening fee for this card. Confirm to add it.";
      form.hidden = false;
      checkBalance();
      form.scrollIntoView({ behavior: "smooth", block: "nearest" });
    });

    select.addEventListener("change", checkBalance);
  }

  // ---------- top-up ----------
  function luhn(digits) {
    if (digits.length < 13 || digits.length > 19) return false;
    let sum = 0;
    for (let i = 0; i < digits.length; i++) {
      let d = Number(digits[digits.length - 1 - i]);
      if (i % 2 === 1) { d *= 2; if (d > 9) d -= 9; }
      sum += d;
    }
    return sum % 10 === 0;
  }

  function brandOf(digits) {
    if (/^4/.test(digits)) return "Visa";
    if (/^(5[1-5]|2(2[2-9]|[3-6]\d|7[01]|720))/.test(digits)) return "Mastercard";
    if (/^3[47]/.test(digits)) return "Amex";
    return "";
  }

  const topUpForm = document.querySelector("[data-topup-form]");
  const numberInput = document.querySelector("[data-card-number]");
  const expiryInput = document.querySelector("[data-expiry]");
  const cvvInput = document.querySelector("[data-cvv]");
  const amountInput = document.getElementById("topup-amount");

  numberInput?.addEventListener("input", (event) => {
    const digits = event.target.value.replace(/\D/g, "").slice(0, 19);
    event.target.value = digits.replace(/(.{4})/g, "$1 ").trim();
    const tag = topUpForm?.querySelector("[data-brand]");
    if (tag) tag.textContent = brandOf(digits);
  });

  // Hazır məbləğ düymələri
  topUpForm?.querySelectorAll("[data-amount]").forEach((chip) => {
    chip.addEventListener("click", () => {
      amountInput.value = chip.getAttribute("data-amount");
      topUpForm.querySelectorAll(".chip").forEach((c) => c.classList.toggle("is-active", c === chip));
    });
  });
  amountInput?.addEventListener("input", () => {
    topUpForm.querySelectorAll(".chip").forEach((c) => c.classList.remove("is-active"));
  });

  // Demo kart (test rejimi): müəllim/istifadəçi doğru nömrə tapmaq məcburiyyətində qalmasın
  document.querySelector("[data-demo-card]")?.addEventListener("click", () => {
    const next = new Date();
    const year = String((next.getFullYear() + 3) % 100).padStart(2, "0");
    numberInput.value = "4111 1111 1111 1111";
    numberInput.dispatchEvent(new Event("input"));
    expiryInput.value = "12/" + year;
    cvvInput.value = "123";
    if (!amountInput.value) amountInput.value = "100";
  });

  function fieldError(input, message) {
    const field = input.closest(".field");
    field.querySelector(".field-error")?.remove();
    if (!message) return;
    const p = document.createElement("p");
    p.className = "field-error";
    p.textContent = message;
    field.appendChild(p);
  }

  // Göndərməzdən əvvəl sahə-sahə yoxlama (server də yenidən yoxlayır)
  topUpForm?.addEventListener("submit", (event) => {
    const amount = Number(String(amountInput.value).replace(",", "."));
    const digits = numberInput.value.replace(/\D/g, "");
    const expiry = expiryInput.value;
    const m = /^(\d{2})\/(\d{2})$/.exec(expiry);
    let expiryOk = false;
    if (m && Number(m[1]) >= 1 && Number(m[1]) <= 12) {
      const end = new Date(2000 + Number(m[2]), Number(m[1]), 0, 23, 59, 59);
      expiryOk = end >= new Date();
    }
    const checks = [
      [amountInput, Number.isFinite(amount) && amount >= 1 && amount <= 5000 && Math.round(amount * 100) / 100 === amount, "Enter an amount between 1 and 5,000 AZN."],
      [numberInput, luhn(digits), "Enter a valid card number."],
      [expiryInput, expiryOk, "Enter the expiry as MM/YY. It must not have passed."],
      [cvvInput, /^\d{3,4}$/.test(cvvInput.value), "Enter the 3 or 4 digit CVV."]
    ];
    let ok = true;
    checks.forEach(([input, valid, message]) => {
      fieldError(input, valid ? "" : message);
      if (!valid) ok = false;
    });
    if (!ok) event.preventDefault();
  });

  document.querySelector("[data-expiry]")?.addEventListener("input", (event) => {
    const digits = event.target.value.replace(/\D/g, "").slice(0, 4);
    event.target.value = digits.length > 2 ? digits.slice(0, 2) + "/" + digits.slice(2) : digits;
  });

  document.querySelector("[data-cvv]")?.addEventListener("input", (event) => {
    event.target.value = event.target.value.replace(/\D/g, "").slice(0, 4);
  });


  // ---------- köçürmə səhifəsi ----------
  const transferForm = document.getElementById("transfer-form");
  if (transferForm) {
    const byId = (id) => document.getElementById(id);
    const fromSel = byId("from-card");
    const toSel = byId("to-card");
    const modeInput = byId("mode-input");
    const ownField = byId("own-field");
    const otherField = byId("other-field");
    const ownBtn = byId("to-own");
    const otherBtn = byId("to-other");
    const amountInput = byId("amount");
    const numberInput = byId("to-number");
    const hint = byId("limit-hint");
    const cashbackHint = byId("cashback-hint");
    const ownEmpty = byId("own-empty");
    const reviewBtn = byId("review-btn");
    const payCard = byId("pay-card");
    const meterFill = byId("meter-fill");
    const meterText = byId("meter-text");
    const destinations = [...toSel.options].map((o) => o.cloneNode(true));
    let mode = transferForm.getAttribute("data-mode") === "other" ? "other" : "own";

    const round2 = (n) => Math.round(n * 100) / 100;
    const money = (n) => n.toLocaleString("en-GB", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " AZN";
    const fromOption = () => fromSel.options[fromSel.selectedIndex];
    const isCashback = () => (fromOption()?.getAttribute("data-tier") || "").toLowerCase() === "cashback";

    // Göndərən kartı hədəf siyahısından çıxarır (eyni karta köçürmək olmaz)
    function rebuildDestinations() {
      const keep = toSel.value || toSel.getAttribute("data-selected");
      toSel.innerHTML = "";
      destinations.filter((o) => o.value !== fromSel.value).forEach((o) => toSel.appendChild(o.cloneNode(true)));
      if ([...toSel.options].some((o) => o.value === keep)) toSel.value = keep;
      toSel._pickSync?.();
    }

    function setMode(next) {
      // Cashback kartı yalnız öz kartlarına göndərə bilir
      mode = isCashback() ? "own" : next;
      modeInput.value = mode;
      ownBtn.classList.toggle("is-active", mode === "own");
      otherBtn.classList.toggle("is-active", mode === "other");
      otherBtn.disabled = isCashback();
      cashbackHint.hidden = !isCashback();
      ownField.hidden = mode !== "own";
      otherField.hidden = mode !== "other";
      paint();
    }

    function parseAmount() {
      const n = Number(String(amountInput.value).replace(/,/g, ".").trim());
      return Number.isFinite(n) && n > 0 ? round2(n) : null;
    }

    // Komissiya hər köçürməyə tətbiq olunur: göndərən kartın limitindən yuxarı olan hissəyə (server də eyni hesablayır)
    function commissionFor(amount, limit, pct) {
      return amount > limit ? round2((amount - limit) * pct / 100) : 0;
    }

    // Komissiya daxil olmaqla balansa sığan ən böyük məbləğ
    function maxAmount(balance, limit, pct) {
      const rate = pct / 100;
      if (balance <= limit) return Math.floor(balance * 100) / 100;
      return Math.floor(((balance + limit * rate) / (1 + rate)) * 100) / 100;
    }

    function paintNumber(digits) {
      const box = byId("live-number");
      const chars = Array.from({ length: 16 }, (_, i) => digits[i] || "•");
      if (box.childElementCount !== 16) {
        box.innerHTML = chars.map((c, i) => '<span class="pay-pan-digit' + (i > 0 && i % 4 === 0 ? " pay-pan-gap" : "") + '"></span>').join("");
      }
      chars.forEach((c, i) => {
        const span = box.children[i];
        span.textContent = c;
        span.classList.toggle("is-on", c !== "•");
      });
    }

    function paint() {
      const from = fromOption();
      const tier = from ? from.getAttribute("data-tier") : "";
      const limit = from ? Number(from.getAttribute("data-limit")) : 0;
      const pct = from ? Number(from.getAttribute("data-pct")) : 0;
      const balance = from ? Number(from.getAttribute("data-balance")) : 0;
      const amount = parseAmount();

      // Önizləmə kartı göndərənin kartı kimi görünür (növü və bazadakı dizaynı)
      if (from && payCard) {
        payCard.className = "plastic plastic-" + tier.toLowerCase();
        const design = from.getAttribute("data-design");
        payCard.style.backgroundImage = design ? "url('" + design + "')" : "";
        payCard.style.backgroundSize = "cover";
        payCard.style.backgroundPosition = "center";
        byId("live-tier").textContent = tier;
      }

      const ownTargets = toSel.options.length;
      let digits = "";
      let holder = "Recipient";
      if (mode === "own") {
        const dest = toSel.options[toSel.selectedIndex];
        const last4 = dest ? dest.getAttribute("data-last4") : "";
        digits = last4 ? "••••••••••••" + last4 : "";
        holder = dest ? "Your " + dest.getAttribute("data-tier") + " card" : "Your card";
      } else {
        digits = numberInput.value.replace(/\D/g, "").slice(0, 16);
      }
      paintNumber(digits);
      byId("live-holder").textContent = holder;

      // Öz kartlarım rejimində başqa kart yoxdursa izah və düymənin söndürülməsi
      const noTarget = mode === "own" && ownTargets === 0;
      ownEmpty.hidden = !(mode === "own" && ownTargets === 0);
      reviewBtn.disabled = noTarget;

      const fee = amount === null ? 0 : commissionFor(amount, limit, pct);
      const shown = amount === null ? 0 : amount;
      byId("live-amount").textContent = money(shown);
      byId("live-order-amount").textContent = money(shown);
      byId("live-fee").textContent = money(fee);
      byId("live-total").textContent = money(amount === null ? 0 : round2(amount + fee));
      byId("live-from").textContent = from ? tier + " •••• " + from.getAttribute("data-last4") : "—";

      // Limit göstəricisi: məbləğ komissiyasız limitə nisbətən nə qədərdir
      if (limit > 0) {
        const ratio = Math.min(1, shown / limit);
        meterFill.style.width = (ratio * 100) + "%";
        meterFill.classList.toggle("is-over", shown > limit);
        meterText.textContent = shown > limit
          ? "Above the " + money(limit) + " commission-free limit"
          : "Commission-free up to " + money(limit);
      } else {
        meterFill.style.width = "0%";
        meterFill.classList.remove("is-over");
        meterText.textContent = from ? tier + " transfers have no commission." : "";
      }

      hint.textContent = !from ? "" : limit > 0
        ? "Commission-free up to " + money(limit) + ". " + String(pct) + "% applies to the part above that. Available: " + money(balance) + "."
        : "No commission on " + tier + " transfers. Available: " + money(balance) + ".";
    }

    fromSel.addEventListener("change", () => { rebuildDestinations(); setMode(mode); });
    toSel.addEventListener("change", paint);
    ownBtn.addEventListener("click", () => setMode("own"));
    otherBtn.addEventListener("click", () => setMode("other"));
    amountInput.addEventListener("input", paint);
    numberInput.addEventListener("input", () => {
      numberInput.value = numberInput.value.replace(/\D/g, "").slice(0, 16).replace(/(\d{4})(?=\d)/g, "$1 ").trim();
      paint();
    });

    // Hazır məbləğlər və "Max"
    transferForm.querySelectorAll("[data-quick]").forEach((chip) => {
      chip.addEventListener("click", () => {
        amountInput.value = chip.getAttribute("data-quick");
        paint();
      });
    });
    byId("amount-max")?.addEventListener("click", () => {
      const from = fromOption();
      if (!from) return;
      const max = maxAmount(Number(from.getAttribute("data-balance")), Number(from.getAttribute("data-limit")), Number(from.getAttribute("data-pct")));
      amountInput.value = max > 0 ? String(max) : "";
      paint();
    });

    rebuildDestinations();
    setMode(mode);
  }

  // ---------- təsdiq pəncərəsi (SweetAlert tipli) ----------
  // data-confirm-text olan düymə basılanda əvvəl "Are you sure?" soruşulur; "Yes" olarsa forma həmin düymə ilə göndərilir
  function confirmDialog(options) {
    return new Promise((resolve) => {
      const wrap = document.createElement("div");
      wrap.className = "swal-backdrop";
      wrap.innerHTML =
        '<div class="swal" role="alertdialog" aria-modal="true" aria-labelledby="swal-title" aria-describedby="swal-text">'
        + '<div class="swal-icon" aria-hidden="true">?</div>'
        + '<h2 id="swal-title"></h2>'
        + '<p id="swal-text"></p>'
        + '<div class="swal-actions">'
        + '<button type="button" class="swal-btn swal-cancel"></button>'
        + '<button type="button" class="swal-btn swal-confirm"></button>'
        + '</div></div>';
      wrap.querySelector("#swal-title").textContent = options.title;
      wrap.querySelector("#swal-text").textContent = options.text;
      const cancel = wrap.querySelector(".swal-cancel");
      const confirm = wrap.querySelector(".swal-confirm");
      cancel.textContent = options.no;
      confirm.textContent = options.yes;
      document.body.appendChild(wrap);
      document.body.classList.add("modal-open");
      confirm.focus();

      function close(result) {
        document.removeEventListener("keydown", onKey);
        wrap.classList.add("is-closing");
        setTimeout(() => {
          wrap.remove();
          if (!document.querySelector(".modal:not([hidden])")) document.body.classList.remove("modal-open");
        }, 160);
        resolve(result);
      }
      function onKey(event) {
        if (event.key === "Escape") close(false);
      }
      document.addEventListener("keydown", onKey);
      cancel.addEventListener("click", () => close(false));
      confirm.addEventListener("click", () => close(true));
      wrap.addEventListener("click", (event) => { if (event.target === wrap) close(false); });
    });
  }

  document.addEventListener("click", async (event) => {
    const button = event.target.closest("[data-confirm-text]");
    if (!button || button.dataset.confirmed === "1") return;
    event.preventDefault();
    const form = button.form;
    const ok = await confirmDialog({
      title: button.getAttribute("data-confirm-title") || "Are you sure?",
      text: button.getAttribute("data-confirm-text") || "",
      yes: button.getAttribute("data-confirm-yes") || "Yes",
      no: button.getAttribute("data-confirm-no") || "Cancel"
    });
    if (ok && form) {
      button.dataset.confirmed = "1";
      form.requestSubmit(button);
    }
  });

  // Təsdiq düyməsi iki dəfə basılmasın (server də eyni sorğunu təkrar köçürmür, amma düymə dərhal söndürülür)
  document.querySelectorAll("form").forEach((form) => {
    form.addEventListener("submit", (event) => {
      const submitter = event.submitter;
      if (submitter && submitter.hasAttribute("data-once")) setTimeout(() => { submitter.disabled = true; }, 0);
    });
  });

  // ---------- qəbz: PDF kimi saxla (çap pəncərəsi) və paylaş ----------
  document.getElementById("rc-print")?.addEventListener("click", () => window.print());
  document.getElementById("rc-share")?.addEventListener("click", async (event) => {
    const button = event.currentTarget;
    const text = button.getAttribute("data-share") || "";
    try {
      if (navigator.share) {
        await navigator.share({ title: "Caspian Bank receipt", text });
      } else if (navigator.clipboard?.writeText) {
        await navigator.clipboard.writeText(text);
        const old = button.textContent;
        button.textContent = "Copied";
        setTimeout(() => { button.textContent = old; }, 1400);
      }
    } catch (error) {
      /* istifadəçi paylaşmanı ləğv etdi */
    }
  });

  // ---------- FİN: yalnız hərf/rəqəm, böyük hərflə ----------
  const fin = document.getElementById("fin");
  fin?.addEventListener("input", () => {
    fin.value = fin.value.replace(/[^A-Za-z0-9]/g, "").toUpperCase().slice(0, 7);
  });
})();
