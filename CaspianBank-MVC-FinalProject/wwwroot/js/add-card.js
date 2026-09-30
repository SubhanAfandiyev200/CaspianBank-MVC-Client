(function () {
  const root = document.getElementById("add-root");
  const state = CaspianStore.get();
  const firstTime = state.cards.length === 0;

  function round2(n) { return Math.round(n * 100) / 100; }

  function pan() {
    let value = "4532";
    for (let i = 0; i < 12; i += 1) value += Math.floor(Math.random() * 10);
    return value;
  }

  function expiry() {
    const date = new Date();
    date.setFullYear(date.getFullYear() + 4);
    return String(date.getMonth() + 1).padStart(2, "0") + "/" + String(date.getFullYear()).slice(-2);
  }

  function makeCard(tier, label, holder) {
    const number = pan();
    return {
      id: BankUI.uid("c"),
      tier,
      label,
      number,
      cvv: String(100 + Math.floor(Math.random() * 900)),
      balance: 0,
      expiry: expiry(),
      holder,
      accountNo: "CB-4000-" + number.slice(-4),
      blocked: false
    };
  }

  if (firstTime) {
    document.getElementById("add-title").textContent = "Your first cards";
    document.getElementById("add-lead").textContent = "We'll open a Main card for everyday spending and a Cashback card that returns a portion of eligible purchases. Both are free. Confirm your identity to continue.";
    const mainName = CaspianDemo.limits.main.name;
    const cashName = CaspianDemo.limits.cashback.name;
    root.innerHTML = `
      <div class="starter-pair">
        ${BankUI.plasticPreview("main", mainName)}
        ${BankUI.plasticPreview("cashback", cashName)}
      </div>
      <form id="first-form" class="surface form-stack narrow" novalidate>
        <div class="form-row">
          <div class="field">
            <label for="given">Name</label>
            <input class="input" id="given" value="${BankUI.esc(state.profile.firstName || "")}" autocomplete="given-name" />
          </div>
          <div class="field">
            <label for="surname">Surname</label>
            <input class="input" id="surname" value="${BankUI.esc(state.profile.lastName || "")}" autocomplete="family-name" />
          </div>
        </div>
        <div class="field">
          <label for="fin">FIN / ID code</label>
          <input class="input" id="fin" data-fin maxlength="7" autocapitalize="characters" spellcheck="false" placeholder="7 characters" />
        </div>
        <button class="btn btn-primary" type="submit">Create my cards</button>
      </form>`;

    document.getElementById("first-form").addEventListener("submit", async (event) => {
      event.preventDefault();
      const form = event.target;
      const given = document.getElementById("given");
      const surname = document.getElementById("surname");
      const fin = document.getElementById("fin");
      BankUI.clearErrors(form);
      let ok = true;
      if (!given.value.trim()) { BankUI.setFieldError(given, "Enter your name."); ok = false; }
      if (!surname.value.trim()) { BankUI.setFieldError(surname, "Enter your surname."); ok = false; }
      if (!/^[A-Za-z0-9]{7}$/.test(fin.value.trim())) {
        BankUI.setFieldError(fin, "Enter the 7-character FIN code.");
        ok = false;
      }
      if (!ok) return;
      const button = form.querySelector("button[type=submit]");
      BankUI.setBusy(button, true);
      // TODO: POST /api/cards/starter  { name, surname, fin }
      await BankUI.request("/api/cards/starter", { method: "POST", body: JSON.stringify({ fin: fin.value.trim() }) });
      const holder = (given.value.trim() + " " + surname.value.trim()).toUpperCase();
      CaspianStore.update((next) => {
        next.profile.firstName = given.value.trim();
        next.profile.lastName = surname.value.trim();
        next.profile.fin = fin.value.trim().toUpperCase();
        next.session.name = given.value.trim() + " " + surname.value.trim();
        next.cards = [makeCard("main", "Main", holder), makeCard("cashback", "Cashback", holder)];
        CaspianStore.logAudit(next, "Card issued", "Main and Cashback cards opened");
        CaspianStore.pushNote(next, {
          type: "card",
          title: "Your cards are ready",
          body: "Main and Cashback cards have been added to your account."
        });
      });
      BankUI.toast("Your Main and Cashback cards are ready.");
      location.href = "app.html";
    });
    return;
  }

  document.getElementById("add-title").textContent = "Choose a tier";
  document.getElementById("add-lead").textContent = "The opening fee is charged to a card you already hold.";
  const order = ["standard", "silver", "gold"];
  root.innerHTML = `
    <div class="tier-grid" id="tiers">
      ${order.map((key) => {
        const rule = CaspianDemo.limits[key];
        return `<button type="button" class="tier" data-tier="${key}">
          ${BankUI.plasticPreview(key, rule.name)}
          <h3>${BankUI.esc(rule.name)}</h3>
          <p class="tier-fee">${BankUI.formatMoney(rule.openingFee)}<span>Opening fee</span></p>
          <ul>
            <li>Cashback ${BankUI.formatPercent(rule.cashbackRate)}</li>
            <li>Transfer limit ${BankUI.formatMoney(rule.transferLimit)} before commission</li>
            <li>${BankUI.formatPercent(rule.commissionRate)} commission beyond that limit</li>
          </ul>
        </button>`;
      }).join("")}
    </div>
    <form id="tier-form" class="surface form-stack narrow mt-5" hidden novalidate>
      <h2 id="tier-heading">Confirm</h2>
      <p class="lede" id="tier-copy"></p>
      <div class="field">
        <label for="fee-card">Pay from</label>
        <select class="select" id="fee-card"></select>
      </div>
      <button class="btn btn-primary" type="submit">Confirm and add card</button>
    </form>`;

  const form = document.getElementById("tier-form");
  const select = document.getElementById("fee-card");
  let selected = "";

  function fundingOptions() {
    const cards = CaspianStore.get().cards.filter((card) => !card.blocked);
    select.innerHTML = cards.map((card) =>
      `<option value="${card.id}">${BankUI.esc(card.label)} · •••• ${card.number.slice(-4)} · ${BankUI.formatMoney(card.balance)}</option>`
    ).join("");
  }

  document.getElementById("tiers").addEventListener("click", (event) => {
    const button = event.target.closest("[data-tier]");
    if (!button) return;
    selected = button.getAttribute("data-tier");
    document.querySelectorAll(".tier").forEach((tier) => tier.classList.toggle("is-selected", tier === button));
    const rule = CaspianDemo.limits[selected];
    document.getElementById("tier-heading").textContent = rule.name;
    document.getElementById("tier-copy").textContent = rule.openingFee > 0
      ? `The opening fee is ${BankUI.formatMoney(rule.openingFee)}, taken from the card you choose.`
      : "There is no opening fee for Standard. Confirm to add the card.";
    fundingOptions();
    form.hidden = false;
  });

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    if (!selected) return;
    const rule = CaspianDemo.limits[selected];
    const current = CaspianStore.get();
    const source = current.cards.find((card) => card.id === select.value);
    if (!source) {
      BankUI.toast("Choose a card to charge.", "error");
      return;
    }
    if (source.balance < rule.openingFee) {
      BankUI.toast("That card doesn't have enough for the opening fee.", "error");
      return;
    }
    const button = form.querySelector("button[type=submit]");
    BankUI.setBusy(button, true);
    // TODO: POST /api/cards  { tier, fundingCardId }
    await BankUI.request("/api/cards", { method: "POST", body: JSON.stringify({ tier: selected, fundingCardId: source.id }) });
    CaspianStore.update((next) => {
      const fund = next.cards.find((card) => card.id === source.id);
      const holder = (next.profile.firstName + " " + next.profile.lastName).toUpperCase();
      const created = makeCard(selected, rule.name, holder);
      if (rule.openingFee > 0) {
        fund.balance = round2(fund.balance - rule.openingFee);
        next.transactions.unshift({
          id: BankUI.uid("tx"),
          cardId: fund.id,
          description: rule.name + " card opening fee",
          type: "expense",
          amount: rule.openingFee,
          category: "fee",
          at: new Date().toISOString(),
          balanceAfter: fund.balance
        });
      }
      next.cards.push(created);
      CaspianStore.logAudit(next, "Card issued", rule.name + " card ending " + created.number.slice(-4));
      CaspianStore.pushNote(next, {
        type: "card",
        title: rule.name + " card is active",
        body: "Card ending " + created.number.slice(-4) + " has been added."
      });
    });
    BankUI.toast(rule.name + " card added.");
    location.href = "app.html";
  });
})();
