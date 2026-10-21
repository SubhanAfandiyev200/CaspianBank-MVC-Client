"use strict";

// Kredit səhifələri: aylıq ödənişin canlı göstəricisi, "Other" izahı, ödəniş növü seçimi.
// Burada yalnız göstəriş hesablanır; həqiqi hesab, qaydalar və yoxlamalar API-dədir.
(function () {
  function toNumber(text) {
    const value = parseFloat(String(text || "").trim().replace(/\s/g, "").replace(",", "."));
    return Number.isFinite(value) ? value : NaN;
  }

  function money(value) {
    return value.toLocaleString("en-US", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " AZN";
  }

  // Annuitet: API-dəki LoanEngine.MonthlyPayment ilə eyni düstur
  function monthlyPayment(principal, annualRate, months) {
    const monthly = annualRate / 12 / 100;
    if (monthly === 0) return principal / months;
    const growth = Math.pow(1 + monthly, months);
    return principal * monthly * growth / (growth - 1);
  }

  // ---------- müraciət forması ----------
  const form = document.getElementById("loan-form");
  if (form) {
    const rate = parseFloat(form.dataset.rate || "0");
    const min = parseFloat(form.dataset.min || "0");
    const max = parseFloat(form.dataset.max || "0");
    const amount = document.getElementById("loan-amount");
    const term = document.getElementById("loan-term");
    const purpose = document.getElementById("loan-purpose");
    const noteField = document.getElementById("loan-note-field");
    const monthlyOut = document.getElementById("loan-monthly");
    const noteOut = document.getElementById("loan-quote-note");

    function paintQuote() {
      if (!amount || !term || !monthlyOut || !noteOut) return;
      const principal = toNumber(amount.value);
      const months = parseInt(term.value, 10);
      if (!Number.isFinite(principal) || principal <= 0 || !months) {
        monthlyOut.textContent = "—";
        noteOut.textContent = cbt("Enter an amount to see the monthly payment at {0}% a year.", rate);
        return;
      }
      if (principal < min || principal > max) {
        monthlyOut.textContent = "—";
        noteOut.textContent = cbt("The amount must be between {0} and {1}.", money(min), money(max));
        return;
      }
      const payment = monthlyPayment(principal, rate, months);
      monthlyOut.textContent = money(payment);
      noteOut.textContent = cbt("Over {0} months you repay about {1} in total ({2}% a year). The bank may approve a smaller amount.", months, money(payment * months), rate);
    }

    amount?.addEventListener("input", paintQuote);
    term?.addEventListener("change", paintQuote);
    paintQuote();

    // "Other" seçiləndə izah sahəsi görünür
    function paintNote() {
      if (!purpose || !noteField) return;
      noteField.hidden = purpose.value !== "Other";
    }
    purpose?.addEventListener("change", paintNote);
    paintNote();
  }

  // ---------- ödəniş forması ----------
  const pay = document.getElementById("pay-form");
  if (pay) {
    const typeInput = document.getElementById("pay-type");
    const submit = document.getElementById("pay-submit");
    const amountInput = document.getElementById("pay-amount");
    const partInput = document.getElementById("pay-part");
    const partialNote = document.getElementById("pay-partial-note");
    const buttons = [...pay.querySelectorAll("[data-pay-type]")];
    const panels = [...pay.querySelectorAll("[data-pay-panel]")];

    const outstanding = parseFloat(pay.dataset.outstanding || "0");
    const rate = parseFloat(pay.dataset.rate || "0");
    const minPercent = parseFloat(pay.dataset.minPercent || "0");
    const reduction = parseFloat(pay.dataset.reduction || "0");
    const minRate = parseFloat(pay.dataset.minRate || "0");
    const minEarly = parseFloat(pay.dataset.minEarly || "0");
    const dueText = pay.dataset.due || "0.00";
    const payoffText = pay.dataset.payoff || "0.00";
    const baseNote = partialNote ? partialNote.textContent : "";

    function paint() {
      const type = typeInput.value;
      buttons.forEach((button) => button.classList.toggle("is-active", button.dataset.payType === type));
      panels.forEach((panel) => { panel.hidden = panel.dataset.payPanel !== type; });

      let label = "Pay";
      let text = "";
      if (type === "Installment") {
        const part = toNumber(partInput ? partInput.value : "");
        const due = parseFloat(dueText.replace(/,/g, ""));
        if (Number.isFinite(part) && part > 0 && part < due) {
          label = cbt("Pay {0}", money(part));
          text = cbt("{0} will be taken from your card. You still owe {1} of this payment.", money(part), money(due - part));
        } else {
          label = cbt("Pay {0} AZN", dueText);
          text = cbt("{0} AZN will be taken from your card.", dueText);
        }
      } else if (type === "FullEarly") {
        label = cbt("Pay off {0} AZN", payoffText);
        text = cbt("{0} AZN will be taken from your card and the loan will be closed.", payoffText);
      } else {
        const value = toNumber(amountInput ? amountInput.value : "");
        if (Number.isFinite(value) && value > 0) {
          label = cbt("Pay {0}", money(value));
          text = cbt("{0} will be taken from your card and your monthly payment goes down.", money(value));
        } else {
          label = cbt("Pay early");
          text = cbt("The amount you enter will be taken from your card.");
        }
      }
      submit.textContent = label;
      submit.setAttribute("data-confirm-text", text);
      submit.removeAttribute("data-confirmed");
      delete submit.dataset.confirmed;
    }

    // Hissəvi ödənişdə faizin düşüb-düşməyəcəyi göstərilir
    function paintPartial() {
      if (!amountInput || !partialNote) return;
      const value = toNumber(amountInput.value);
      if (!Number.isFinite(value) || value <= 0) {
        partialNote.textContent = baseNote;
      } else if (value < minEarly) {
        partialNote.textContent = cbt("The smallest early payment is {0}.", money(minEarly));
      } else if (value >= outstanding) {
        partialNote.textContent = cbt("That is more than what is left. Use \"Pay off in full\" instead.");
      } else if (value >= outstanding * minPercent / 100) {
        const next = Math.max(minRate, rate - reduction);
        partialNote.textContent = next < rate
          ? cbt("Your rate will drop from {0}% to {1}%.", rate, next)
          : cbt("Your rate is already the lowest it can be ({0}%).", minRate);
      } else {
        partialNote.textContent = cbt("Your monthly payment will go down. Pay {0} or more to lower the rate too.", money(outstanding * minPercent / 100));
      }
      paint();
    }

    buttons.forEach((button) => {
      button.addEventListener("click", () => {
        if (button.disabled) return;
        typeInput.value = button.dataset.payType;
        paint();
      });
    });
    amountInput?.addEventListener("input", paintPartial);
    partInput?.addEventListener("input", paint);
    paint();
  }
})();
