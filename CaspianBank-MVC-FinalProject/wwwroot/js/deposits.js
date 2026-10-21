"use strict";

// Depozit səhifəsi: variant seçimi və faizin canlı göstəricisi. Burada yalnız göstəriş hesablanır;
// həqiqi hesab, hədlər və yoxlamalar API-dədir (Domain/Deposits/DepositRules.cs ilə eyni düstur).
(function () {
  const form = document.getElementById("deposit-form");
  if (!form) return;

  const planInput = document.getElementById("dep-plan");
  if (!planInput) return;

  const amountInput = document.getElementById("dep-amount");
  const amountBox = document.getElementById("dep-amount-box");
  const errorEl = document.getElementById("dep-error");
  const limitsEl = document.getElementById("dep-limits");
  const sourceSel = document.getElementById("dep-source");
  const submit = document.getElementById("dep-submit");
  const plans = [...form.querySelectorAll("[data-plan]")];

  const round2 = (n) => Math.round((n + Number.EPSILON) * 100) / 100;
  const money = (n) => n.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " AZN";

  function currentPlan() {
    return plans.find((button) => button.dataset.plan === planInput.value) || plans[0];
  }

  function parseAmount() {
    const n = Number(String(amountInput.value).replace(/,/g, ".").trim());
    return Number.isFinite(n) && n > 0 ? round2(n) : null;
  }

  // Məbləğ: yalnız rəqəm və bir ayırıcı, ayırıcıdan sonra ən çox 2 rəqəm
  function cleanAmount() {
    let value = amountInput.value.replace(/[^0-9.,]/g, "");
    const sep = value.search(/[.,]/);
    if (sep !== -1) {
      value = value.slice(0, sep + 1) + value.slice(sep + 1).replace(/[.,]/g, "").slice(0, 2);
    }
    value = value.replace(/^0+(?=\d)/, "");
    if (value.startsWith(".") || value.startsWith(",")) value = "0" + value;
    if (value !== amountInput.value) amountInput.value = value;
  }

  function addMonths(date, months) {
    const result = new Date(date.getTime());
    const day = result.getDate();
    result.setMonth(result.getMonth() + months);
    if (result.getDate() !== day) result.setDate(0);
    return result;
  }

  function paint() {
    const plan = currentPlan();
    plans.forEach((button) => button.classList.toggle("is-active", button === plan));
    const months = Number(plan.dataset.months);
    const rate = Number(plan.dataset.rate);
    const min = Number(plan.dataset.min);
    const max = Number(plan.dataset.max);
    const amount = parseAmount();
    const from = sourceSel.options[sourceSel.selectedIndex];
    const balance = from ? Number(from.getAttribute("data-balance")) : 0;

    limitsEl.textContent = cbt("From {0} to {1} for this term. Available on the card: {2}.", money(min), money(max), money(balance));

    const interest = amount === null ? 0 : round2(amount * rate / 100 * months / 12);
    document.getElementById("dep-interest").textContent = money(interest);
    document.getElementById("dep-total").textContent = money(amount === null ? 0 : round2(amount + interest));
    const end = addMonths(new Date(), months);
    document.getElementById("dep-date").textContent = end.toLocaleDateString("en-GB", { day: "2-digit", month: "short", year: "numeric" });

    let problem = "";
    if (amount !== null && amount < min) problem = cbt("The smallest deposit for this term is {0}.", money(min));
    else if (amount !== null && amount > max) problem = cbt("The largest deposit for this term is {0}.", money(max));
    else if (amount !== null && amount > balance) problem = cbt("Not enough money on this card.");
    errorEl.hidden = !problem;
    errorEl.textContent = problem;
    amountBox.classList.toggle("is-error", !!problem);

    submit.disabled = amount === null || !!problem;
    submit.textContent = amount === null ? cbt("Open deposit") : cbt("Open deposit · {0}", money(amount));
    submit.setAttribute("data-confirm-text",
      money(amount === null ? 0 : amount) + " will leave your card and stay locked for " + months + " months. "
      + "You will get " + money(amount === null ? 0 : round2(amount + interest)) + " on " + end.toLocaleDateString("en-GB", { day: "2-digit", month: "short", year: "numeric" })
      + ". You can't take it out earlier.");
    delete submit.dataset.confirmed;
  }

  plans.forEach((button) => {
    button.addEventListener("click", () => {
      planInput.value = button.dataset.plan;
      paint();
    });
  });
  amountInput.addEventListener("input", () => { cleanAmount(); paint(); });
  sourceSel.addEventListener("change", paint);

  cleanAmount();
  paint();
})();
