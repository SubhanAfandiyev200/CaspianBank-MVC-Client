/* Placeholder customer and operations data. Replace CaspianStore reads with the Web API. */
(function () {
  const knownEmail = "leyla.mammadova@gmail.com";

  const limits = {
    main: { name: "Main", openingFee: 0, cashbackRate: 0, transferLimit: 1000, commissionRate: 0.005 },
    cashback: { name: "Cashback", openingFee: 0, cashbackRate: 0.015, transferLimit: 1000, commissionRate: 0.005 },
    standard: { name: "Standard", openingFee: 0, cashbackRate: 0.005, transferLimit: 500, commissionRate: 0.01 },
    silver: { name: "Silver", openingFee: 15, cashbackRate: 0.01, transferLimit: 2000, commissionRate: 0.006 },
    gold: { name: "Gold", openingFee: 40, cashbackRate: 0.015, transferLimit: 10000, commissionRate: 0.003 }
  };

  function hoursAgo(hours) {
    return new Date(Date.now() - hours * 3600000).toISOString();
  }

  function daysAgo(days, hour) {
    const dt = new Date();
    dt.setDate(dt.getDate() - days);
    dt.setHours(hour, 15, 0, 0);
    return dt.toISOString();
  }

  function build() {
    const profile = {
      firstName: "Leyla",
      lastName: "Mammadova",
      email: knownEmail,
      phone: "+994 50 312 44 18",
      dob: "1994-06-12",
      fin: "5A2K7LM",
      registered: "2023-04-18"
    };

    const cards = [
      { id: "c1", tier: "main", label: "Main", number: "453212344291", cvv: "246", balance: 4280.5, expiry: "09/28", holder: "LEYLA MAMMADOVA", accountNo: "CB-4000-4291", blocked: false },
      { id: "c2", tier: "cashback", label: "Cashback", number: "453298118834", cvv: "519", balance: 640.2, expiry: "09/28", holder: "LEYLA MAMMADOVA", accountNo: "CB-4000-8834", blocked: false },
      { id: "c3", tier: "gold", label: "Gold", number: "453200111102", cvv: "804", balance: 12450, expiry: "03/29", holder: "LEYLA MAMMADOVA", accountNo: "CB-4000-1102", blocked: false }
    ];

    const rawTx = [
      { id: "tx1", cardId: "c1", description: "Bravo Hypermarket", type: "expense", amount: 46.8, category: "groceries", at: hoursAgo(3) },
      { id: "tx2", cardId: "c1", description: "Salary payment", type: "income", amount: 3200, category: "income", at: hoursAgo(6) },
      { id: "tx3", cardId: "c2", description: "Bolt ride", type: "expense", amount: 8.4, category: "transport", at: daysAgo(2, 19) },
      { id: "tx4", cardId: "c1", description: "Azərişıq electricity", type: "expense", amount: 42.15, category: "utilities", at: daysAgo(2, 11) },
      { id: "tx5", cardId: "c2", description: "Cashback reward", type: "income", amount: 6.4, category: "cashback", at: daysAgo(3, 8) },
      { id: "tx6", cardId: "c3", description: "SOCAR Petroleum", type: "expense", amount: 70, category: "fuel", at: daysAgo(5, 18) },
      { id: "tx7", cardId: "c3", description: "Kontakt Home", type: "expense", amount: 259.99, category: "shopping", at: daysAgo(8, 15) },
      { id: "tx8", cardId: "c1", description: "Transfer to Nigar Aliyeva", type: "expense", amount: 150, category: "transfer", at: daysAgo(9, 12) },
      { id: "tx9", cardId: "c1", description: "Azərsu water", type: "expense", amount: 18.6, category: "utilities", at: daysAgo(12, 9) },
      { id: "tx10", cardId: "c2", description: "Baku Metro", type: "expense", amount: 12, category: "transport", at: daysAgo(14, 8) },
      { id: "tx11", cardId: "c1", description: "Transfer from Rashad Huseynov", type: "income", amount: 500, category: "transfer", at: daysAgo(18, 16) },
      { id: "tx12", cardId: "c2", description: "Azercell", type: "expense", amount: 25, category: "telecom", at: daysAgo(20, 13) },
      { id: "tx13", cardId: "c3", description: "Port Baku Mall", type: "expense", amount: 186.4, category: "shopping", at: daysAgo(36, 17) },
      { id: "tx14", cardId: "c1", description: "Bolmart", type: "expense", amount: 95, category: "groceries", at: daysAgo(110, 12) },
      { id: "tx15", cardId: "c3", description: "Annual Gold service", type: "expense", amount: 40, category: "fee", at: new Date(new Date().getFullYear() - 1, 11, 20, 10, 0).toISOString() }
    ];

    const running = Object.fromEntries(cards.map((card) => [card.id, card.balance]));
    const transactions = rawTx
      .slice()
      .sort((a, b) => new Date(b.at) - new Date(a.at))
      .map((tx) => {
        const copy = { ...tx, balanceAfter: running[tx.cardId] };
        const signed = tx.type === "income" ? tx.amount : -tx.amount;
        running[tx.cardId] = Math.round((running[tx.cardId] - signed) * 100) / 100;
        return copy;
      });

    const notifications = [
      { id: "n1", type: "payment", title: "Payment sent", body: "42.15 AZN paid to Azərişıq.", at: hoursAgo(2), read: false },
      { id: "n2", type: "alert", title: "Low balance", body: "Your Cashback card is below 1,000.00 AZN.", at: hoursAgo(5), read: false },
      { id: "n3", type: "loan", title: "Loan application received", body: "Your request for 12,000.00 AZN is pending review.", at: hoursAgo(26), read: false },
      { id: "n4", type: "support", title: "Support replied", body: "We answered “Cashback for August”.", at: hoursAgo(50), read: true },
      { id: "n5", type: "income", title: "Transfer received", body: "500.00 AZN from Rashad Huseynov.", at: hoursAgo(80), read: true },
      { id: "n6", type: "card", title: "Gold card is active", body: "Your Gold card ending 1102 is ready to use.", at: hoursAgo(240), read: true }
    ];

    const recurring = [
      { id: "b1", provider: "Azərişıq", reference: "10492811", amount: 42.15, next: daysAgo(-10, 9), status: "Active" },
      { id: "b2", provider: "Azərsu", reference: "883120", amount: 18.6, next: daysAgo(-10, 9), status: "Active" },
      { id: "b3", provider: "Azercell", reference: "+994 50 111 22 33", amount: 25, next: daysAgo(-3, 9), status: "Failed" },
      { id: "b4", provider: "CityNet", reference: "BN-22910", amount: 30, next: daysAgo(-10, 9), status: "Paused" }
    ];

    const loans = [
      { id: "l1", applicant: "Leyla Mammadova", userId: "u1", amount: 12000, purpose: "Education", term: 36, status: "Pending", submitted: daysAgo(2, 11) },
      { id: "l2", applicant: "Leyla Mammadova", userId: "u1", amount: 5000, purpose: "Home renovation", term: 24, status: "Approved", submitted: daysAgo(50, 10) },
      { id: "l3", applicant: "Leyla Mammadova", userId: "u1", amount: 2000, purpose: "Medical", term: 12, status: "Declined", submitted: daysAgo(190, 14) }
    ];

    const tickets = [
      {
        id: "t1",
        userId: "u1",
        name: "Leyla Mammadova",
        email: knownEmail,
        subject: "Cashback for August",
        message: "I don't see cashback posted for supermarket purchases in August.",
        status: "Answered",
        created: daysAgo(9, 14),
        reply: "Cashback for August was posted on 2 September to your Cashback card, ending 8834.",
        repliedAt: daysAgo(8, 16)
      },
      {
        id: "t2",
        userId: "u1",
        name: "Leyla Mammadova",
        email: knownEmail,
        subject: "SWIFT transfer timing",
        message: "How long does an incoming SWIFT transfer take to appear?",
        status: "Pending",
        created: daysAgo(1, 9),
        reply: "",
        repliedAt: null
      }
    ];

    const users = [
      { id: "u1", name: "Leyla Mammadova", email: knownEmail, phone: "+994 50 312 44 18", registered: "2023-04-18", status: "Active", sent: 18420, received: 24600, spent: 16340 },
      { id: "u2", name: "Rashad Huseynov", email: "rashad.huseynov@gmail.com", phone: "+994 55 201 90 14", registered: "2024-01-09", status: "Active", sent: 9200, received: 14110, spent: 8750 },
      { id: "u3", name: "Nigar Aliyeva", email: "nigar.aliyeva@mail.ru", phone: "+994 70 555 12 40", registered: "2022-11-02", status: "Active", sent: 22150, received: 19840, spent: 17660 },
      { id: "u4", name: "Elvin Qasimov", email: "elvin.qasimov@gmail.com", phone: "+994 51 440 18 77", registered: "2025-06-21", status: "Restricted", sent: 4300, received: 2100, spent: 3980 },
      { id: "u5", name: "Aysel Karimova", email: "aysel.karimova@gmail.com", phone: "+994 50 888 21 03", registered: "2024-09-30", status: "Active", sent: 7600, received: 5400, spent: 6900 },
      { id: "u6", name: "Tural Ismayilov", email: "tural.ismayil@outlook.com", phone: "+994 55 673 00 19", registered: "2021-02-14", status: "Active", sent: 31200, received: 28750, spent: 24110 },
      { id: "u7", name: "Sevinc Mammadli", email: "sevinc.m@gmail.com", phone: "+994 70 214 66 52", registered: "2025-12-01", status: "Active", sent: 2800, received: 9600, spent: 4100 },
      { id: "u8", name: "Kamran Bayramov", email: "kamran.b@gmail.com", phone: "+994 51 909 33 28", registered: "2026-02-17", status: "Restricted", sent: 1500, received: 800, spent: 1320 }
    ];

    const adminLoans = [
      ...loans,
      { id: "l4", applicant: "Rashad Huseynov", userId: "u2", amount: 8000, purpose: "Car repair", term: 18, status: "Pending", submitted: daysAgo(1, 15) },
      { id: "l5", applicant: "Tural Ismayilov", userId: "u6", amount: 20000, purpose: "Business stock", term: 48, status: "Approved", submitted: daysAgo(20, 11) },
      { id: "l6", applicant: "Sevinc Mammadli", userId: "u7", amount: 3500, purpose: "Travel", term: 12, status: "Declined", submitted: daysAgo(15, 10) }
    ];

    const reports = [
      { id: "r1", reporter: "Aysel Karimova", userId: "u5", reference: "TX-204918", reason: "POS charge in Ganja while the card was with me in Baku.", at: daysAgo(1, 18), status: "Pending", reviewed: false },
      { id: "r2", reporter: "Elvin Qasimov", userId: "u4", reference: "TX-198332", reason: "Three contactless payments I do not recognise.", at: daysAgo(6, 13), status: "Action Taken", reviewed: true },
      { id: "r3", reporter: "Nigar Aliyeva", userId: "u3", reference: "TX-188104", reason: "Duplicate supermarket charge on the same afternoon.", at: daysAgo(12, 9), status: "Dismissed", reviewed: true }
    ];

    const audit = [
      { id: "a1", at: hoursAgo(1), user: "Leyla Mammadova", action: "Sign-in", details: "Web banking session opened" },
      { id: "a2", at: hoursAgo(3), user: "Leyla Mammadova", action: "Bill payment", details: "42.15 AZN to Azərişıq" },
      { id: "a3", at: hoursAgo(6), user: "System", action: "Transfer", details: "Salary payment 3,200.00 AZN to Main · 4291" },
      { id: "a4", at: daysAgo(1, 16), user: "Nigar Aliyeva", action: "Support reply", details: "Ticket t2 still waiting. Replied on t1." },
      { id: "a5", at: daysAgo(2, 11), user: "Leyla Mammadova", action: "Loan submitted", details: "12,000.00 AZN · 36 months · Education" },
      { id: "a6", at: daysAgo(4, 15), user: "Nigar Aliyeva", action: "Loan decision", details: "Approved 20,000.00 AZN for Tural Ismayilov" },
      { id: "a7", at: daysAgo(6, 13), user: "Nigar Aliyeva", action: "Account restricted", details: "Elvin Qasimov restricted after report TX-198332" },
      { id: "a8", at: daysAgo(9, 12), user: "Leyla Mammadova", action: "Transfer", details: "150.00 AZN to Nigar Aliyeva" },
      { id: "a9", at: daysAgo(12, 9), user: "Nigar Aliyeva", action: "Suspicious report", details: "Dismissed TX-188104" },
      { id: "a10", at: daysAgo(40, 10), user: "Leyla Mammadova", action: "Card issued", details: "Gold card ending 1102" }
    ];

    return {
      profile,
      cards,
      transactions,
      notifications,
      recurring,
      loans,
      tickets,
      users,
      adminLoans,
      reports,
      audit
    };
  }

  window.CaspianDemo = { knownEmail, limits, build };
})();

/* Shared UI for Caspian Bank. Pages open as views; forms do not change data. */
(function () {
  document.addEventListener("submit", (event) => {
    event.preventDefault();
    event.stopPropagation();
  }, true);

  const KEY = "caspian.v1";
  const FLOW = "caspian.flow";
  const UI_KEY = "caspian.ui";

  function readUi() {
    try { return JSON.parse(localStorage.getItem(UI_KEY) || "null"); }
    catch (e) { return null; }
  }

  function writeUi(data) {
    localStorage.setItem(UI_KEY, JSON.stringify(data));
  }

  (function applyCardCopy() {
    const saved = readUi();
    const cards = saved && saved.cards;
    if (!cards || !window.CaspianDemo) return;
    Object.keys(cards).forEach((key) => {
      const rule = window.CaspianDemo.limits[key];
      const over = cards[key];
      if (!rule || !over) return;
      if (over.name) rule.name = over.name;
      ["openingFee", "cashbackRate", "transferLimit", "commissionRate"].forEach((field) => {
        if (over[field] != null && over[field] !== "" && !Number.isNaN(Number(over[field]))) rule[field] = Number(over[field]);
      });
    });
  })();

  const ICONS = {
    eye: '<path d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6S2 12 2 12z"/><circle cx="12" cy="12" r="3"/>',
    eyeOff: '<path d="M3 3l18 18"/><path d="M10.6 6.2A10.7 10.7 0 0 1 12 6c6.5 0 10 6 10 6a18 18 0 0 1-3.2 4.2"/><path d="M6.1 6.1C3.6 7.8 2 12 2 12s3.5 6 10 6c1.6 0 3-.3 4.3-.9"/><path d="M9.9 9.9a3 3 0 0 0 4.2 4.2"/>',
    payment: '<rect x="3" y="6" width="18" height="12" rx="2"/><path d="M3 10h18"/>',
    alert: '<path d="M12 3l9 16H3z"/><path d="M12 9v4"/><path d="M12 16h.01"/>',
    loan: '<path d="M4 10l8-6 8 6"/><path d="M6 10v8h12v-8"/><path d="M10 18v-4h4v4"/>',
    support: '<path d="M4 6h16v10H7l-3 3z"/>',
    income: '<path d="M12 19V5"/><path d="M6 11l6-6 6 6"/>',
    card: '<rect x="3" y="6" width="18" height="12" rx="2"/><path d="M3 10h18"/><path d="M7 15h4"/>',
    cart: '<circle cx="9" cy="20" r="1"/><circle cx="17" cy="20" r="1"/><path d="M3 4h2l2.2 11h11.3l2-7H7"/>',
    zap: '<path d="M13 2L4 14h7l-1 8 9-12h-7z"/>',
    bus: '<rect x="4" y="4" width="16" height="13" rx="2"/><path d="M4 13h16"/><path d="M7 20h.01M17 20h.01"/><path d="M7 17v3M17 17v3"/>',
    arrows: '<path d="M7 7h11l-3-3"/><path d="M17 17H6l3 3"/>',
    bag: '<path d="M6 8h12l-1 12H7z"/><path d="M9 8V7a3 3 0 0 1 6 0v1"/>',
    fuel: '<path d="M5 21V5h9v16"/><path d="M14 8h2l3 3v6a2 2 0 0 1-4 0"/>',
    cashback: '<path d="M4 12a8 8 0 1 0 2.3-5.6"/><path d="M4 4v5h5"/>',
    fee: '<rect x="3" y="6" width="18" height="12" rx="2"/><path d="M12 9v6"/><path d="M9.5 10.5a2 2 0 0 1 2-1.5h1a2 2 0 0 1 0 4h-1a2 2 0 0 0 0 4h1a2 2 0 0 0 2-1.5"/>',
    phone: '<rect x="8" y="3" width="8" height="18" rx="2"/><path d="M11 18h2"/>',
    plus: '<path d="M12 5v14M5 12h14"/>'
  };

  function icon(name) {
    return `<svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">${ICONS[name] || ICONS.card}</svg>`;
  }

  function esc(value) {
    return String(value ?? "").replace(/[&<>"']/g, (ch) => ({
      "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
    }[ch]));
  }

  function formatMoney(amount) {
    const n = Math.abs(Number(amount) || 0);
    return n.toLocaleString("en-GB", { minimumFractionDigits: 2, maximumFractionDigits: 2 }) + " AZN";
  }

  function formatPercent(rate) {
    const n = rate * 100;
    return (Number.isInteger(n) ? n.toFixed(0) : n.toFixed(1)) + "%";
  }

  function formatDate(iso) {
    return new Date(iso).toLocaleDateString("en-GB", { day: "2-digit", month: "short", year: "numeric" });
  }

  function formatDateTime(iso) {
    const d = new Date(iso);
    return formatDate(iso) + ", " + d.toLocaleTimeString("en-GB", { hour: "2-digit", minute: "2-digit" });
  }

  function maskEmail(email) {
    const [user, domain] = String(email).split("@");
    if (!domain) return email;
    return user.slice(0, 1) + "****@" + domain;
  }

  function maskCard(number) {
    return "•••• •••• •••• " + String(number).slice(-4);
  }

  function formatCard(number) {
    return String(number || "").replace(/\D/g, "").replace(/(.{4})/g, "$1 ").trim();
  }

  function cardCvv(card) {
    if (card?.cvv) return String(card.cvv);
    const digits = String(card?.number || "");
    let hash = 7;
    for (let i = 0; i < digits.length; i += 1) hash = (hash * 33 + digits.charCodeAt(i)) % 900;
    return String(100 + hash);
  }

  function copyText(text, label) {
    const done = () => toast((label || "Value") + " copied.");
    const fallback = () => {
      const area = document.createElement("textarea");
      area.value = text;
      area.setAttribute("readonly", "");
      area.style.position = "fixed";
      area.style.left = "-999px";
      document.body.appendChild(area);
      area.select();
      document.execCommand("copy");
      area.remove();
      done();
    };
    if (navigator.clipboard?.writeText) {
      navigator.clipboard.writeText(text).then(done).catch(fallback);
    } else fallback();
  }

  function initials(name) {
    return String(name || "")
      .split(/\s+/)
      .slice(0, 2)
      .map((part) => part[0] || "")
      .join("")
      .toUpperCase();
  }

  function uid(prefix) {
    return prefix + "-" + Math.random().toString(36).slice(2, 7) + Date.now().toString(36).slice(-3);
  }

  function amountHtml(type, amount) {
    const cls = type === "income" ? "amount-in" : "amount-out";
    const sign = type === "income" ? "+" : "−";
    return `<span class="${cls} num">${sign}${formatMoney(amount)}</span>`;
  }

  function badge(status) {
    const map = {
      Active: "success",
      Approved: "success",
      Answered: "success",
      Pending: "pending",
      Paused: "neutral",
      Failed: "danger",
      Declined: "danger",
      Restricted: "danger",
      "Action Taken": "info",
      Dismissed: "neutral",
      Blocked: "danger",
      Cancelled: "neutral"
    };
    const kind = map[status] || "neutral";
    return `<span class="badge badge-${kind}">${esc(status)}</span>`;
  }

  function categoryIcon(category) {
    const map = {
      groceries: "cart",
      utilities: "zap",
      transport: "bus",
      transfer: "arrows",
      shopping: "bag",
      fuel: "fuel",
      income: "income",
      cashback: "cashback",
      fee: "fee",
      telecom: "phone",
      loan: "loan",
      card: "card"
    };
    return icon(map[category] || "card");
  }

  function getFlow() {
    try { return JSON.parse(sessionStorage.getItem(FLOW) || "null"); }
    catch { return null; }
  }

  function setFlow(flow) {
    sessionStorage.setItem(FLOW, JSON.stringify(flow));
  }

  function seed() {
    const data = window.CaspianDemo.build();
    data.session = {
      email: data.profile.email,
      name: data.profile.firstName + " " + data.profile.lastName,
      mode: "existing"
    };
    return data;
  }

  function getState() {
    const raw = sessionStorage.getItem(KEY);
    if (raw) {
      try {
        const state = JSON.parse(raw);
        if (state && state.session && Array.isArray(state.cards) && state.cards.length) return state;
      } catch (e) { /* replace with the demo view */ }
    }
    const data = seed();
    sessionStorage.setItem(KEY, JSON.stringify(data));
    return data;
  }

  function save(state) {
    sessionStorage.setItem(KEY, JSON.stringify(state));
  }

  function update() {
    return getState();
  }

  function logAudit(state, action, details) {
    state.audit.unshift({
      id: uid("a"),
      at: new Date().toISOString(),
      user: state.session?.name || "System",
      action,
      details
    });
  }

  function pushNote(state, note) {
    state.notifications.unshift({
      id: uid("n"),
      read: false,
      at: new Date().toISOString(),
      ...note
    });
  }

  function establish(mode, profile) {
    update((state) => {
      if (mode === "existing") {
        const fresh = window.CaspianDemo.build();
        state.profile = fresh.profile;
        state.cards = fresh.cards;
        state.transactions = fresh.transactions;
        state.notifications = fresh.notifications;
        state.recurring = fresh.recurring;
        state.loans = fresh.loans;
        state.tickets = fresh.tickets;
        state.session = {
          email: fresh.profile.email,
          name: fresh.profile.firstName + " " + fresh.profile.lastName,
          mode: "existing"
        };
        logAudit(state, "Sign-in", "Web banking session opened");
      } else {
        state.profile = profile;
        state.cards = [];
        state.transactions = [];
        state.notifications = [{
          id: uid("n"),
          type: "support",
          title: "Welcome to Caspian Bank",
          body: "Add your first cards to start banking.",
          at: new Date().toISOString(),
          read: false
        }];
        state.recurring = [];
        state.loans = [];
        state.tickets = [];
        state.session = {
          email: profile.email,
          name: profile.firstName + " " + profile.lastName,
          mode: "new"
        };
        logAudit(state, "Sign-in", "New customer account created");
      }
    });
  }

  function toast(message, type) {
    const root = document.getElementById("toast-root");
    if (!root) return;
    const el = document.createElement("div");
    el.className = "toast" + (type === "error" ? " toast-error" : " toast-success");
    el.setAttribute("role", "status");
    el.textContent = message;
    root.appendChild(el);
    setTimeout(() => {
      el.classList.add("is-out");
      setTimeout(() => el.remove(), 220);
    }, 4200);
  }

  function setBusy(button, busy) {
    if (!button) return;
    button.disabled = busy;
    button.classList.toggle("is-busy", busy);
  }

  async function request(url, options) {
    // TODO: replace this delay with the Web API call:
    // return fetch(url, { headers: { "Content-Type": "application/json", "Accept": "application/json" }, ...options });
    await new Promise((resolve) => setTimeout(resolve, 380));
    return { ok: true, url, options };
  }

  function setFieldError(input, message) {
    const field = input.closest(".field") || input.parentElement;
    let hint = field.querySelector(".field-error");
    if (!message) {
      input.removeAttribute("aria-invalid");
      if (hint) hint.remove();
      return;
    }
    input.setAttribute("aria-invalid", "true");
    if (!hint) {
      hint = document.createElement("p");
      hint.className = "field-error";
      field.appendChild(hint);
    }
    hint.textContent = message;
  }

  function clearErrors(form) {
    form.querySelectorAll("[aria-invalid]").forEach((el) => el.removeAttribute("aria-invalid"));
    form.querySelectorAll(".field-error").forEach((el) => el.remove());
  }

  const MONTHS = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
  const MONTHS_SHORT = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sept", "Oct", "Nov", "Dec"];
  const WHEEL_ITEM = 36;

  function parsePhoneDigits(raw) {
    let digits = String(raw || "").replace(/\D/g, "");
    if (digits.startsWith("994") && digits.length > 9) digits = digits.slice(3);
    while (digits.startsWith("0") && digits.length > 1) digits = digits.slice(1);
    if (digits === "0") return "0";
    return digits.slice(0, 9);
  }

  function formatNational(digits) {
    if (!digits || digits === "0") return digits || "";
    const parts = [digits.slice(0, 2), digits.slice(2, 5), digits.slice(5, 7), digits.slice(7, 9)].filter(Boolean);
    return parts.join(" ");
  }

  function canonicalPhone(raw) {
    const digits = parsePhoneDigits(raw);
    if (!/^\d{9}$/.test(digits)) return "";
    return "+994 " + formatNational(digits);
  }

  function bindPhone(input) {
    if (!input || input.dataset.phoneBound) return;
    input.dataset.phoneBound = "1";
    input.setAttribute("inputmode", "numeric");
    if (!input.closest(".phone-wrap")) {
      const wrap = document.createElement("div");
      wrap.className = "phone-wrap";
      const prefix = document.createElement("span");
      prefix.className = "phone-prefix";
      prefix.textContent = "+994";
      input.parentNode.insertBefore(wrap, input);
      wrap.appendChild(prefix);
      wrap.appendChild(input);
      input.classList.remove("input");
      input.classList.add("phone-input");
      const field = wrap.closest(".field");
      if (field && !field.querySelector(".field-hint")) {
        const hint = document.createElement("p");
        hint.className = "field-hint";
        hint.textContent = "Type 012 345 67 89 or 123456789.";
        field.appendChild(hint);
      }
    }
    if (!input.placeholder || input.placeholder.includes("994")) input.placeholder = "12 345 67 89";
    const apply = () => {
      const caret = input.selectionStart ?? input.value.length;
      const digitsBefore = input.value.slice(0, caret).replace(/\D/g, "").length;
      const rawDigits = input.value.replace(/\D/g, "");
      let removed = 0;
      if (rawDigits.startsWith("994") && rawDigits.length > 9) removed = 3;
      else if (rawDigits.startsWith("0") && rawDigits.length > 1) removed = 1;
      const digits = parsePhoneDigits(input.value);
      const next = formatNational(digits);
      if (input.value !== next) {
        input.value = next;
        let seen = 0;
        const target = Math.max(0, digitsBefore - removed);
        let pos = next.length;
        if (target > 0) {
          pos = 0;
          for (let i = 0; i < next.length; i += 1) {
            if (/\d/.test(next[i])) seen += 1;
            pos = i + 1;
            if (seen >= target) break;
          }
        } else pos = 0;
        input.setSelectionRange(pos, pos);
      }
    };
    input.addEventListener("input", apply);
    input.addEventListener("blur", () => {
      if (parsePhoneDigits(input.value).length !== 9) {
        if (input.value === "0") input.value = "";
      }
    });
  }

  function bindFin(input) {
    if (!input || input.dataset.finBound) return;
    input.dataset.finBound = "1";
    input.addEventListener("input", () => {
      const caret = input.selectionStart ?? input.value.length;
      const next = input.value.toUpperCase().replace(/[^A-Z0-9]/g, "").slice(0, 7);
      if (input.value !== next) {
        input.value = next;
        const pos = Math.min(caret, next.length);
        input.setSelectionRange(pos, pos);
      }
    });
  }

  function parseIsoDate(value) {
    const match = /^(\d{4})-(\d{2})-(\d{2})$/.exec(value || "");
    if (!match) return null;
    return { y: Number(match[1]), m: Number(match[2]), d: Number(match[3]) };
  }

  function toIsoDate(part) {
    return part.y + "-" + String(part.m).padStart(2, "0") + "-" + String(part.d).padStart(2, "0");
  }

  function dateLabel(value) {
    const part = parseIsoDate(value);
    if (!part) return "Select date";
    return part.d + " " + MONTHS_SHORT[part.m - 1] + " " + part.y;
  }

  function daysInMonth(year, month) {
    return new Date(year, month, 0).getDate();
  }

  function dateBounds(input) {
    const min = parseIsoDate(input.min) || { y: 1920, m: 1, d: 1 };
    const today = new Date();
    const max = parseIsoDate(input.max) || { y: today.getFullYear() + 1, m: 12, d: 31 };
    return { min, max };
  }

  function compareDate(a, b) {
    return (a.y - b.y) || (a.m - b.m) || (a.d - b.d);
  }

  function clampDate(part, min, max) {
    const dim = daysInMonth(part.y, part.m);
    const next = { y: part.y, m: part.m, d: Math.min(part.d, dim) };
    if (compareDate(next, min) < 0) return { ...min };
    if (compareDate(next, max) > 0) return { ...max };
    return next;
  }

  function watchInputValue(input, onChange) {
    const desc = Object.getOwnPropertyDescriptor(HTMLInputElement.prototype, "value");
    Object.defineProperty(input, "value", {
      configurable: true,
      get() { return desc.get.call(this); },
      set(value) {
        desc.set.call(this, value);
        onChange();
      }
    });
  }

  function closeOpenWheels(except) {
    document.querySelectorAll(".wheel-date.is-open").forEach((wrap) => {
      if (wrap !== except) wrap._closeWheel?.(true);
    });
  }

  function bindWheelDate(input) {
    if (!input || input.dataset.wheelBound || input.closest(".wheel-date")) return;
    input.dataset.wheelBound = "1";
    const wrap = document.createElement("div");
    wrap.className = "wheel-date";
    input.parentNode.insertBefore(wrap, input);
    wrap.appendChild(input);
    input.classList.add("wheel-native");
    input.tabIndex = -1;
    input.setAttribute("aria-hidden", "true");

    const button = document.createElement("button");
    button.type = "button";
    button.className = "input wheel-date-btn";
    const syncButton = () => {
      const empty = !input.value;
      button.textContent = dateLabel(input.value);
      button.classList.toggle("is-empty", empty);
    };
    syncButton();
    wrap.appendChild(button);

    const pop = document.createElement("div");
    pop.className = "wheel-pop";
    pop.hidden = true;
    pop.setAttribute("role", "dialog");
    pop.setAttribute("aria-label", "Choose a date");
    pop.innerHTML = `
      <div class="wheel-frame">
        <div class="wheel-highlight" aria-hidden="true"></div>
        <div class="wheel" data-part="day" tabindex="0" role="listbox" aria-label="Day"></div>
        <div class="wheel" data-part="month" tabindex="0" role="listbox" aria-label="Month"></div>
        <div class="wheel" data-part="year" tabindex="0" role="listbox" aria-label="Year"></div>
      </div>
      <button type="button" class="btn btn-primary btn-block wheel-done">Done</button>`;
    document.body.appendChild(pop);

    const columns = {
      day: pop.querySelector('[data-part="day"]'),
      month: pop.querySelector('[data-part="month"]'),
      year: pop.querySelector('[data-part="year"]')
    };
    let draft = null;
    let syncing = false;
    let moved = false;

    function itemsFor(part) {
      const bounds = dateBounds(input);
      if (part === "year") {
        const list = [];
        for (let year = bounds.min.y; year <= bounds.max.y; year += 1) list.push({ value: year, label: String(year) });
        return list;
      }
      if (part === "month") {
        return MONTHS.map((label, index) => ({ value: index + 1, label }));
      }
      const count = daysInMonth(draft.y, draft.m);
      const list = [];
      for (let day = 1; day <= count; day += 1) list.push({ value: day, label: String(day) });
      return list;
    }

    function paintColumn(part, keepScroll) {
      const col = columns[part];
      const items = itemsFor(part);
      const current = draft[part === "year" ? "y" : part === "month" ? "m" : "d"];
      let index = items.findIndex((item) => item.value === current);
      if (index < 0) index = items.length - 1;
      col.innerHTML = `<div class="wheel-pad"></div>` + items.map((item, i) =>
        `<div class="wheel-item${i === index ? " is-active" : ""}" role="option" data-i="${i}" data-value="${item.value}">${esc(item.label)}</div>`
      ).join("") + `<div class="wheel-pad"></div>`;
      if (!keepScroll) col.scrollTop = index * WHEEL_ITEM;
    }

    let ignoreUntil = 0;

    function paint(resetScroll) {
      syncing = true;
      ignoreUntil = performance.now() + 180;
      paintColumn("day", !resetScroll);
      paintColumn("month", !resetScroll);
      paintColumn("year", !resetScroll);
      requestAnimationFrame(() => { syncing = false; });
    }

    function readColumn(part) {
      const col = columns[part];
      const index = Math.max(0, Math.round(col.scrollTop / WHEEL_ITEM));
      const item = col.querySelectorAll(".wheel-item")[index];
      col.querySelectorAll(".wheel-item").forEach((el, i) => el.classList.toggle("is-active", i === index));
      return item ? Number(item.dataset.value) : null;
    }

    function applyScroll(part) {
      if (syncing || !draft) return;
      const value = readColumn(part);
      if (value == null) return;
      if (part === "year") draft.y = value;
      if (part === "month") draft.m = value;
      if (part === "day") draft.d = value;
      const bounds = dateBounds(input);
      const next = clampDate(draft, bounds.min, bounds.max);
      const dayCount = daysInMonth(draft.y, draft.m);
      const rebuildDay = part !== "day" && (next.d !== draft.d || draft.d > dayCount || columns.day.querySelectorAll(".wheel-item").length !== dayCount);
      draft = next;
      if (rebuildDay) {
        syncing = true;
        paintColumn("day", false);
        requestAnimationFrame(() => { syncing = false; });
      }
    }

    Object.entries(columns).forEach(([part, col]) => {
      let timer = 0;
      col.addEventListener("scroll", () => {
        if (syncing || performance.now() < ignoreUntil) return;
        moved = true;
        window.clearTimeout(timer);
        timer = window.setTimeout(() => applyScroll(part), 80);
      });
      col.addEventListener("click", (event) => {
        const item = event.target.closest(".wheel-item");
        if (!item) return;
        col.scrollTo({ top: Number(item.dataset.i) * WHEEL_ITEM, behavior: "smooth" });
      });
      col.addEventListener("keydown", (event) => {
        if (event.key !== "ArrowUp" && event.key !== "ArrowDown") return;
        event.preventDefault();
        const delta = event.key === "ArrowDown" ? WHEEL_ITEM : -WHEEL_ITEM;
        col.scrollTo({ top: col.scrollTop + delta, behavior: "smooth" });
      });
    });

    function place() {
      const rect = button.getBoundingClientRect();
      const width = Math.max(rect.width, 280);
      let left = rect.left;
      if (left + width > window.innerWidth - 8) left = window.innerWidth - width - 8;
      left = Math.max(8, left);
      let top = rect.bottom + 6;
      if (top + 248 > window.innerHeight) top = Math.max(8, rect.top - 254);
      pop.style.left = left + "px";
      pop.style.top = top + "px";
      pop.style.width = width + "px";
    }

    function open() {
      closeOpenWheels(wrap);
      const bounds = dateBounds(input);
      const parsed = parseIsoDate(input.value);
      if (parsed) draft = clampDate(parsed, bounds.min, bounds.max);
      else if (input.id === "dob") {
        const base = { y: bounds.max.y - 25, m: bounds.max.m, d: bounds.max.d };
        draft = clampDate(base, bounds.min, bounds.max);
      } else draft = clampDate(bounds.max, bounds.min, bounds.max);
      moved = false;
      wrap.classList.add("is-open");
      pop.hidden = false;
      button.setAttribute("aria-expanded", "true");
      place();
      paint(true);
      columns.day.focus();
    }

    function close(commit) {
      if (!wrap.classList.contains("is-open")) return;
      Object.keys(columns).forEach((part) => applyScroll(part));
      wrap.classList.remove("is-open");
      pop.hidden = true;
      button.setAttribute("aria-expanded", "false");
      if (commit && draft && (moved || commit === "done")) {
        const value = toIsoDate(clampDate(draft, dateBounds(input).min, dateBounds(input).max));
        if (input.value !== value) {
          input.value = value;
          input.dispatchEvent(new Event("input", { bubbles: true }));
          input.dispatchEvent(new Event("change", { bubbles: true }));
        }
      }
      syncButton();
    }

    pop._wrap = wrap;
    wrap._wheelPop = pop;
    wrap._closeWheel = close;
    button.addEventListener("click", () => {
      if (wrap.classList.contains("is-open")) close(moved);
      else open();
    });
    pop.querySelector(".wheel-done").addEventListener("click", () => close("done"));
    watchInputValue(input, syncButton);

    if (!bindWheelDate.listening) {
      bindWheelDate.listening = true;
      window.addEventListener("resize", () => {
        document.querySelectorAll(".wheel-pop:not([hidden])").forEach((panel) => panel._place?.());
      });
      window.addEventListener("scroll", (event) => {
        if (event.target?.closest?.(".wheel")) return;
        document.querySelectorAll(".wheel-pop:not([hidden])").forEach((panel) => panel._place?.());
      }, true);
    }
    pop._place = () => { if (button.isConnected) place(); };
  }

  function enhanceFields(root) {
    if (!root) return;
    root.querySelectorAll("input[data-phone], #phone").forEach(bindPhone);
    root.querySelectorAll("input[data-fin]").forEach(bindFin);
    root.querySelectorAll('input[type="date"]').forEach(bindWheelDate);
  }

  function mountFieldObserver() {
    enhanceFields(document);
    const observer = new MutationObserver((records) => {
      records.forEach((record) => {
        record.addedNodes.forEach((node) => {
          if (node.nodeType !== 1) return;
          if (node.matches?.("input")) enhanceFields(node.parentElement || node);
          else enhanceFields(node);
        });
      });
      document.querySelectorAll(".wheel-pop").forEach((panel) => {
        if (panel._wrap && !panel._wrap.isConnected) panel.remove();
      });
    });
    observer.observe(document.body, { childList: true, subtree: true });
    document.addEventListener("click", (event) => {
      document.querySelectorAll(".wheel-date.is-open").forEach((wrap) => {
        if (wrap.contains(event.target) || wrap._wheelPop?.contains(event.target)) return;
        wrap._closeWheel?.(true);
      });
    });
  }

  function closeDropdowns() {
    document.querySelectorAll("[data-dropdown].is-open").forEach((dd) => {
      dd.classList.remove("is-open");
      dd.querySelector("[data-dropdown-toggle]")?.setAttribute("aria-expanded", "false");
    });
  }

  function openModal(id) {
    const modal = document.getElementById(id);
    if (!modal) return;
    modal.hidden = false;
    document.body.classList.add("modal-open");
    const focus = modal.querySelector("input, textarea, select, button");
    focus?.focus();
  }

  function closeModal(modal) {
    if (!modal) return;
    modal.hidden = true;
    if (!document.querySelector(".modal:not([hidden])")) document.body.classList.remove("modal-open");
  }

  function confirmDialog(opts) {
    return new Promise((resolve) => {
      document.getElementById("modal-confirm")?.remove();
      const wrap = document.createElement("div");
      wrap.className = "modal";
      wrap.id = "modal-confirm";
      wrap.innerHTML = `
        <div class="modal-backdrop" data-cancel></div>
        <div class="modal-dialog" role="dialog" aria-modal="true" aria-labelledby="confirm-title">
          <h2 id="confirm-title">${esc(opts.title)}</h2>
          <p class="lede">${esc(opts.message)}</p>
          <div class="modal-actions">
            <button type="button" class="btn btn-secondary" data-cancel>${esc(opts.cancelText || "Cancel")}</button>
            <button type="button" class="btn ${opts.danger ? "btn-danger" : "btn-primary"}" data-ok>${esc(opts.confirmText || "Confirm")}</button>
          </div>
        </div>`;
      document.body.appendChild(wrap);
      document.body.classList.add("modal-open");
      const finish = (value) => {
        wrap.remove();
        if (!document.querySelector(".modal:not([hidden])")) document.body.classList.remove("modal-open");
        resolve(value);
      };
      wrap.querySelector("[data-ok]").addEventListener("click", () => finish(true));
      wrap.querySelectorAll("[data-cancel]").forEach((el) => el.addEventListener("click", () => finish(false)));
    });
  }

  function noteHtml(note) {
    return `<button type="button" class="note${note.read ? "" : " is-unread"}" data-note="${esc(note.id)}">
      <span class="note-icon">${icon(note.type === "card" ? "card" : note.type)}</span>
      <span>
        <span class="spread"><span class="note-title">${esc(note.title)}</span><time>${esc(formatDateTime(note.at))}</time></span>
        <span class="note-body">${esc(note.body)}</span>
      </span>
    </button>`;
  }

  function adminDesk(state) {
    const items = [];
    (state.adminLoans || []).filter((loan) => loan.status === "Pending").forEach((loan) => {
      items.push({
        type: "loan",
        title: "Loan waiting",
        body: loan.applicant + " · " + formatMoney(loan.amount),
        at: loan.submitted,
        href: "admin-loans.html"
      });
    });
    (state.reports || []).filter((report) => report.status === "Pending").forEach((report) => {
      items.push({
        type: "alert",
        title: "Report waiting",
        body: report.reporter + " · " + report.reference,
        at: report.at,
        href: "admin-reports.html"
      });
    });
    (state.tickets || []).filter((ticket) => ticket.status === "Pending").forEach((ticket) => {
      items.push({
        type: "support",
        title: "Message waiting",
        body: ticket.name + " · " + ticket.subject,
        at: ticket.created,
        href: "admin-contact-tickets.html"
      });
    });
    return items.sort((a, b) => new Date(b.at) - new Date(a.at));
  }

  function renderNavNotes() {
    const list = document.getElementById("nav-notes");
    if (!list) return;
    const state = getState();
    const badgeEl = document.getElementById("nav-note-count");
    if (document.body.classList.contains("body-admin")) {
      const items = adminDesk(state);
      if (badgeEl) {
        badgeEl.textContent = String(items.length);
        badgeEl.hidden = items.length === 0;
      }
      const mark = document.getElementById("nav-mark-read");
      if (mark) mark.hidden = true;
      list.innerHTML = items.length
        ? items.map((item) => `<a class="note is-unread" href="${item.href}">
            <span class="note-icon">${icon(item.type)}</span>
            <span>
              <span class="spread"><span class="note-title">${esc(item.title)}</span><time>${esc(formatDateTime(item.at))}</time></span>
              <span class="note-body">${esc(item.body)}</span>
            </span>
          </a>`).join("")
        : `<div class="empty"><h3>Nothing waiting</h3><p>Loans, reports, and messages are clear.</p></div>`;
      return;
    }
    if (!state.session) {
      if (badgeEl) badgeEl.hidden = true;
      list.innerHTML = `<div class="empty"><h3>Sign in required</h3><p>Notifications are available after you sign in.</p></div>`;
      return;
    }
    const unread = state.notifications.filter((n) => !n.read).length;
    if (badgeEl) {
      badgeEl.textContent = String(unread);
      badgeEl.hidden = unread === 0;
    }
    const items = state.notifications.slice(0, 5);
    list.innerHTML = items.length
      ? items.map(noteHtml).join("")
      : `<div class="empty"><h3>No notifications</h3><p>You're up to date.</p></div>`;
  }

  function refreshChrome() {
    const state = getState();
    const signedIn = document.getElementById("profile-signed-in");
    const signedOut = document.getElementById("profile-signed-out");
    if (signedIn && signedOut) {
      signedIn.hidden = !state.session;
      signedOut.hidden = !!state.session;
    }
    if (state.session) {
      const nameEl = document.querySelector("[data-user-name]");
      if (nameEl) nameEl.textContent = state.session.name;
      const av = document.querySelector("[data-user-initials]");
      if (av) av.textContent = initials(state.session.name);
    }
    renderNavNotes();
  }

  function markNote(id) {
    update((state) => {
      const note = state.notifications.find((n) => n.id === id);
      if (note) note.read = true;
    });
    refreshChrome();
    document.dispatchEvent(new CustomEvent("caspian:notes"));
  }

  function markAllNotes() {
    update((state) => state.notifications.forEach((n) => { n.read = true; }));
    refreshChrome();
    document.dispatchEvent(new CustomEvent("caspian:notes"));
  }

  function waveSvg() {
    return `<svg class="plastic-wave" viewBox="0 0 24 24" width="18" height="18" fill="none" stroke="currentColor" stroke-width="1.7" stroke-linecap="round" aria-hidden="true"><path d="M8 8.2c1.6 1.5 1.6 6.1 0 7.6"/><path d="M11.2 5.6c2.6 2.4 2.6 10.4 0 12.8"/><path d="M14.4 3.2c3.6 3.4 3.6 14.2 0 17.6"/></svg>`;
  }

  function plasticInner(parts) {
    return `<span class="plastic-art" aria-hidden="true"></span>
      <span class="plastic-shine" aria-hidden="true"></span>
      <div class="plastic-top">
        <span class="plastic-brand"><span class="plastic-mark" aria-hidden="true"></span><span class="plastic-bank">Caspian</span></span>
        <span class="plastic-tier">${esc(parts.tierLabel || "")}</span>
        ${waveSvg()}
      </div>
      <div class="plastic-mid"><span class="plastic-chip" aria-hidden="true"></span>${parts.secret || ""}</div>
      ${parts.numberHtml}
      <div class="plastic-bottom">${parts.bottom}</div>`;
  }

  function plasticPreview(tier, label) {
    return `<div class="plastic plastic-mini plastic-${esc(tier)}" aria-hidden="true">${plasticInner({
      tierLabel: label,
      numberHtml: `<p class="plastic-number">•••• ••••</p>`,
      bottom: `<span class="plastic-issuer">Caspian Bank</span>`
    })}</div>`;
  }

  function plasticHtml(card, opts) {
    const options = opts || {};
    const large = options.large ? " plastic-lg" : "";
    const blocked = card.blocked ? " is-blocked" : "";
    const href = options.link === false ? "" : ` href="card-detail.html?id=${esc(card.id)}"`;
    const tag = options.link === false ? "div" : "a";
    const pan = formatCard(card.number);
    const cvv = cardCvv(card);
    const last4 = String(card.number || "").slice(-4);
    const expiry = `<div><span class="plastic-k">Valid</span><span class="plastic-v">${esc(card.expiry)}</span></div>`;
    const balance = `<div><span class="plastic-k">Balance</span><span class="plastic-v num">${formatMoney(card.balance)}</span></div>`;
    const number = options.large
      ? `<p class="plastic-number"><span class="plastic-pan-full">${esc(pan)}</span></p>`
      : `<p class="plastic-number">•••• ${esc(last4)}</p>`;
    const secret = options.large
      ? `<span class="plastic-cvv"><span class="plastic-k">CVV</span><span class="plastic-v">${esc(cvv)}</span></span>`
      : "";
    const holder = options.large
      ? `<div><span class="plastic-k">Cardholder</span><span class="plastic-v">${esc(card.holder)}</span></div>`
      : `<span class="plastic-issuer">Caspian Bank</span>`;
    const rule = window.CaspianDemo && window.CaspianDemo.limits[card.tier];
    const tierLabel = (rule && rule.name) || card.label;
    return `<${tag} class="plastic plastic-${esc(card.tier)}${large}${blocked}" data-card-id="${esc(card.id)}"${href}>${plasticInner({
      tierLabel,
      numberHtml: number,
      secret,
      bottom: `${holder}${expiry}${balance}`
    })}</${tag}>`;
  }

  function cardThumb(card, size) {
    const row = size === "row" ? " is-row" : "";
    if (!card) {
      return `<span class="card-thumb-frame card-thumb-all${row}" aria-hidden="true"><span></span><span></span></span>`;
    }
    const rule = window.CaspianDemo && window.CaspianDemo.limits[card.tier];
    const tierLabel = (rule && rule.name) || card.label || "";
    const last4 = String(card.number || "").slice(-4);
    return `<span class="card-thumb-frame${row}" aria-hidden="true"><span class="plastic plastic-${esc(card.tier || "standard")} card-thumb-scale">${plasticInner({
      tierLabel,
      numberHtml: `<p class="plastic-number">•••• ${esc(last4)}</p>`,
      bottom: `<span class="plastic-issuer">Caspian</span>`
    })}</span></span>`;
  }

  function mountCardPicks() {
    const chevron = `<svg class="card-pick-chevron" width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><path d="M4 6l4 4 4-4"/></svg>`;
    document.querySelectorAll("select[data-card-pick]").forEach((select) => {
      if (select.dataset.pickReady) return;
      select.dataset.pickReady = "1";
      const wrap = document.createElement("div");
      wrap.className = "card-pick";
      select.parentNode.insertBefore(wrap, select);
      wrap.appendChild(select);
      select.classList.add("card-pick-native");
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

      function findCard(id) {
        if (!id || id === "all") return null;
        return CaspianStore.get().cards.find((card) => card.id === id) || null;
      }
      function rowHtml(option, withChevron) {
        const card = findCard(option.value);
        const bits = option.text.split("·").map((part) => part.trim());
        const title = card ? card.label : bits[0];
        const meta = card ? "•••• " + String(card.number || "").slice(-4) : (bits[1] || "");
        const extra = card ? formatMoney(card.balance) : (bits[2] || "");
        return `${cardThumb(card)}${meta || extra ? `<span class="card-pick-copy"><strong>${esc(title)}</strong>${meta ? `<span>${esc(meta)}</span>` : ""}</span>` : `<span class="card-pick-copy"><strong>${esc(title)}</strong></span>`}${extra ? `<span class="card-pick-bal">${esc(extra)}</span>` : ""}${withChevron ? chevron : ""}`;
      }
      function sync() {
        const option = select.options[select.selectedIndex] || null;
        button.innerHTML = option
          ? rowHtml(option, true)
          : `<span class="card-pick-copy"><strong>Choose a card</strong></span>${chevron}`;
        menu.innerHTML = [...select.options].map((opt) =>
          `<button type="button" class="card-pick-option${opt === option ? " is-selected" : ""}" data-value="${esc(opt.value)}" role="option" aria-selected="${opt === option}">${rowHtml(opt, false)}</button>`
        ).join("");
      }
      function close() {
        wrap.classList.remove("is-open");
        menu.hidden = true;
        button.setAttribute("aria-expanded", "false");
      }
      function open() {
        document.querySelectorAll(".card-pick.is-open").forEach((pick) => {
          if (pick === wrap) return;
          pick.classList.remove("is-open");
          const other = pick.querySelector(".card-pick-menu");
          const toggle = pick.querySelector(".card-pick-btn");
          if (other) other.hidden = true;
          if (toggle) toggle.setAttribute("aria-expanded", "false");
        });
        wrap.classList.add("is-open");
        menu.hidden = false;
        button.setAttribute("aria-expanded", "true");
      }
      button.addEventListener("click", () => {
        if (wrap.classList.contains("is-open")) close();
        else open();
      });
      menu.addEventListener("click", (event) => {
        const choice = event.target.closest("[data-value]");
        if (!choice) return;
        select.value = choice.getAttribute("data-value");
        select.dispatchEvent(new Event("change", { bubbles: true }));
        close();
        sync();
      });
      select.addEventListener("focus", open);
      new MutationObserver(sync).observe(select, { childList: true });
      sync();
    });
    if (!mountCardPicks.bound) {
      mountCardPicks.bound = true;
      document.addEventListener("click", (event) => {
        document.querySelectorAll(".card-pick.is-open").forEach((pick) => {
          if (pick.contains(event.target)) return;
          pick.classList.remove("is-open");
          const menu = pick.querySelector(".card-pick-menu");
          const toggle = pick.querySelector(".card-pick-btn");
          if (menu) menu.hidden = true;
          if (toggle) toggle.setAttribute("aria-expanded", "false");
        });
      });
      document.addEventListener("keydown", (event) => {
        if (event.key !== "Escape") return;
        document.querySelectorAll(".card-pick.is-open").forEach((pick) => {
          pick.classList.remove("is-open");
          const menu = pick.querySelector(".card-pick-menu");
          const toggle = pick.querySelector(".card-pick-btn");
          if (menu) menu.hidden = true;
          if (toggle) toggle.setAttribute("aria-expanded", "false");
        });
      });
    }
  }

  function mountDeck(stage) {
    if (!stage || stage.dataset.deckReady) return;
    stage.dataset.deckReady = "1";
    const cards = [...stage.querySelectorAll(".plastic")];
    if (!cards.length) return;
    let index = 0;
    let startX = 0;
    let moved = false;
    const dots = stage.parentElement.querySelector("[data-deck-dots]");

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
          ? `translate(${behind * 16}px, ${behind * -14}px) scale(${1 - behind * 0.045})`
          : "translate(28px, -32px) scale(0.86)";
        card.toggleAttribute("aria-hidden", behind !== 0);
      });
      if (dots) {
        dots.innerHTML = cards.map((_, i) =>
          `<button type="button" class="deck-dot${i === index ? " is-active" : ""}" data-deck-go="${i}" aria-label="Card ${i + 1}"></button>`
        ).join("");
      }
      stage.dispatchEvent(new CustomEvent("deck:change", { bubbles: true, detail: { index, id: cards[index].dataset.cardId } }));
    }

    function step(delta) {
      index = (index + delta + cards.length) % cards.length;
      paint();
    }

    let tracking = false;

    stage.addEventListener("pointerdown", (event) => {
      if (event.button !== 0) return;
      startX = event.clientX;
      tracking = true;
      moved = false;
    });
    stage.addEventListener("pointermove", (event) => {
      if (!tracking) return;
      if (Math.abs(event.clientX - startX) > 40) moved = true;
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

    stage.addEventListener("click", (event) => {
      if (!moved) return;
      event.preventDefault();
      event.stopPropagation();
      moved = false;
    });
    stage.addEventListener("keydown", (event) => {
      if (event.key === "ArrowRight") step(1);
      if (event.key === "ArrowLeft") step(-1);
    });
    stage.parentElement.addEventListener("click", (event) => {
      if (event.target.closest("[data-deck-prev]")) step(-1);
      if (event.target.closest("[data-deck-next]")) step(1);
      const dot = event.target.closest("[data-deck-go]");
      if (!dot) return;
      index = Number(dot.getAttribute("data-deck-go")) || 0;
      paint();
    });
    paint();
  }

  function emptyHtml(title, text, actionHtml) {
    return `<div class="empty">
      <div class="empty-mark"><span class="plus"></span></div>
      <h3>${esc(title)}</h3>
      <p>${esc(text)}</p>
      ${actionHtml || ""}
    </div>`;
  }

  function enhanceTable(table) {
    if (!table || table.dataset.enhanced === "1") return;
    table.dataset.enhanced = "1";
    table.querySelectorAll("th[data-sort]").forEach((th) => {
      if (!th.hasAttribute("aria-sort")) th.setAttribute("aria-sort", "none");
      th.addEventListener("click", () => {
        const type = th.dataset.type || "text";
        const dir = th.getAttribute("aria-sort") === "ascending" ? "descending" : "ascending";
        table.querySelectorAll("th[data-sort]").forEach((other) => {
          if (other !== th) other.setAttribute("aria-sort", "none");
        });
        th.setAttribute("aria-sort", dir);
        const tbody = table.tBodies[0];
        const rows = [...tbody.querySelectorAll("tr")].filter((row) => !row.classList.contains("empty-row"));
        const idx = th.cellIndex;
        rows.sort((a, b) => {
          const va = a.cells[idx]?.dataset.value ?? a.cells[idx]?.textContent.trim() ?? "";
          const vb = b.cells[idx]?.dataset.value ?? b.cells[idx]?.textContent.trim() ?? "";
          let cmp = 0;
          if (type === "number") cmp = parseFloat(va) - parseFloat(vb);
          else if (type === "date") cmp = new Date(va) - new Date(vb);
          else cmp = va.localeCompare(vb, undefined, { numeric: true, sensitivity: "base" });
          return dir === "ascending" ? cmp : -cmp;
        });
        rows.forEach((row) => tbody.appendChild(row));
      });
    });
  }

  function initOtp(root) {
    const inputs = [...root.querySelectorAll("input")];
    const land = (input) => {
      input.classList.remove("is-typed");
      if (!input.value) return;
      void input.offsetWidth;
      input.classList.add("is-typed");
    };
    inputs.forEach((input, index) => {
      input.addEventListener("input", () => {
        input.value = input.value.replace(/\D/g, "").slice(-1);
        land(input);
        if (input.value && inputs[index + 1]) inputs[index + 1].focus();
      });
      input.addEventListener("keydown", (event) => {
        if (event.key === "Backspace" && !input.value && inputs[index - 1]) {
          inputs[index - 1].focus();
        }
        if (event.key === "ArrowLeft" && inputs[index - 1]) inputs[index - 1].focus();
        if (event.key === "ArrowRight" && inputs[index + 1]) inputs[index + 1].focus();
      });
      input.addEventListener("paste", (event) => {
        const text = (event.clipboardData.getData("text") || "").replace(/\D/g, "").slice(0, inputs.length);
        if (!text) return;
        event.preventDefault();
        [...text].forEach((ch, i) => { if (inputs[i]) inputs[i].value = ch; });
        text.split("").forEach((_, i) => {
          window.setTimeout(() => { if (inputs[i]) land(inputs[i]); }, i * 70);
        });
        inputs[Math.min(text.length, inputs.length) - 1].focus();
      });
    });
  }

  function readOtp(root) {
    return [...root.querySelectorAll("input")].map((input) => input.value).join("");
  }

  function commission(card, amount) {
    const rule = window.CaspianDemo.limits[card.tier];
    if (!rule || amount <= rule.transferLimit) return 0;
    return Math.round((amount - rule.transferLimit) * rule.commissionRate * 100) / 100;
  }

  function limitSentence(tier) {
    const rule = window.CaspianDemo.limits[tier];
    if (!rule) return "";
    return `${rule.name} transfers are commission-free up to ${formatMoney(rule.transferLimit)}. ${formatPercent(rule.commissionRate)} applies above that amount.`;
  }

  function inRange(iso, from, to) {
    const d = new Date(iso);
    if (from) {
      const start = new Date(from);
      start.setHours(0, 0, 0, 0);
      if (d < start) return false;
    }
    if (to) {
      const end = new Date(to);
      end.setHours(23, 59, 59, 999);
      if (d > end) return false;
    }
    return true;
  }

  function periodBounds(period) {
    const now = new Date();
    const end = now.toISOString().slice(0, 10);
    if (period === "day") return { from: end, to: end };
    if (period === "week") {
      const start = new Date(now);
      start.setDate(start.getDate() - 6);
      return { from: start.toISOString().slice(0, 10), to: end };
    }
    if (period === "month") {
      const start = new Date(now.getFullYear(), now.getMonth(), 1);
      return { from: start.toISOString().slice(0, 10), to: end };
    }
    if (period === "year") return { from: `${now.getFullYear()}-01-01`, to: end };
    return { from: "", to: "" };
  }

  function downloadText(filename, text) {
    const blob = new Blob(["\uFEFF" + text], { type: "text/csv;charset=utf-8" });
    const url = URL.createObjectURL(blob);
    const a = document.createElement("a");
    a.href = url;
    a.download = filename;
    document.body.appendChild(a);
    a.click();
    a.remove();
    URL.revokeObjectURL(url);
  }

  function toCsv(rows) {
    return rows.map((row) => row.map((cell) => `"${String(cell ?? "").replaceAll('"', '""')}"`).join(",")).join("\r\n");
  }

  function cardLabel(state, cardId) {
    const card = state.cards.find((item) => item.id === cardId);
    return card ? `${card.label} · •••• ${card.number.slice(-4)}` : "Card";
  }

  let reportTx = null;

  function openReport(tx) {
    reportTx = tx;
    const summary = document.getElementById("report-summary");
    const formView = document.getElementById("report-form-view");
    const done = document.getElementById("report-done");
    const reason = document.getElementById("report-reason");
    if (!summary || !tx) return;
    const state = getState();
    summary.innerHTML = `
      <div><dt>Description</dt><dd>${esc(tx.description)}</dd></div>
      <div><dt>Date</dt><dd>${esc(formatDateTime(tx.at))}</dd></div>
      <div><dt>Card</dt><dd>${esc(cardLabel(state, tx.cardId))}</dd></div>
      <div><dt>Amount</dt><dd>${esc(formatMoney(tx.amount))}</dd></div>`;
    if (reason) {
      reason.value = "";
      setFieldError(reason, "");
    }
    if (formView) formView.hidden = false;
    if (done) done.hidden = true;
    openModal("report-modal");
  }

  function submitReport() {
    return;
    const reason = document.getElementById("report-reason");
    const text = reason.value.trim();
    if (text.length < 10) {
      setFieldError(reason, "Describe what looked wrong, in at least 10 characters.");
      return;
    }
    setFieldError(reason, "");
    const button = document.getElementById("report-submit");
    setBusy(button, true);
    const tx = reportTx;
    request("/api/reports", { method: "POST", body: JSON.stringify({ transactionId: tx.id, reason: text }) })
      .then(() => {
        const ref = uid("RP").toUpperCase();
        update((state) => {
          state.reports.unshift({
            id: uid("r"),
            reporter: state.session.name,
            userId: "u1",
            reference: tx.id.toUpperCase(),
            reason: text,
            at: new Date().toISOString(),
            status: "Pending",
            reviewed: false
          });
          logAudit(state, "Suspicious report", `${tx.description} · ${formatMoney(tx.amount)}`);
          pushNote(state, { type: "alert", title: "Report received", body: `Reference ${ref}. Our fraud team will review it.` });
        });
        document.getElementById("report-form-view").hidden = true;
        document.getElementById("report-done").hidden = false;
        document.getElementById("report-ref").textContent = "Reference " + ref;
        refreshChrome();
      })
      .finally(() => setBusy(button, false));
  }

  function mountPasswords() {
    document.querySelectorAll("[data-password]").forEach((input) => {
      if (input.dataset.ready === "1") return;
      input.dataset.ready = "1";
      const wrap = document.createElement("div");
      wrap.className = "input-wrap";
      input.parentNode.insertBefore(wrap, input);
      wrap.appendChild(input);
      const btn = document.createElement("button");
      btn.type = "button";
      btn.className = "icon-btn";
      btn.setAttribute("aria-label", "Show password");
      btn.innerHTML = icon("eye");
      wrap.appendChild(btn);
      btn.addEventListener("click", () => {
        const show = input.type === "password";
        input.type = show ? "text" : "password";
        btn.setAttribute("aria-label", show ? "Hide password" : "Show password");
        btn.innerHTML = icon(show ? "eyeOff" : "eye");
      });
    });
  }

  function pageKey() {
    const file = (location.pathname.split("/").pop() || "welcome.html").split("?")[0].toLowerCase();
    if (file === "add-card.html" || file === "card-detail.html") return "app.html";
    if (file === "report-transaction.html") return "transaction-history.html";
    return file;
  }

  function setActiveNav(_pathname, animate) {
    const key = pageKey();
    document.querySelectorAll(".nav-link").forEach((link) => {
      if (link.hasAttribute("data-admin-mode")) return;
      const href = link.getAttribute("href") || "";
      const linkKey = (href.split("#")[0].split("?")[0].split("/").pop() || "").toLowerCase();
      const active = linkKey === key;
      link.classList.toggle("is-active", active);
      if (active) link.setAttribute("aria-current", "page");
      else link.removeAttribute("aria-current");
    });
    moveIndicator(animate);
  }

  function indicatorBox(link) {
    const nav = link.closest(".main-nav");
    const navRect = nav.getBoundingClientRect();
    const linkRect = link.getBoundingClientRect();
    const pad = 8;
    const left = Math.round(linkRect.left + pad);
    const right = Math.round(linkRect.right - pad);
    return {
      x: left - navRect.left,
      w: Math.max(16, right - left),
      top: (Math.round(navRect.bottom) - 2) - navRect.top
    };
  }

  function placeIndicator(indicator, box, top) {
    indicator.style.width = box.w + "px";
    indicator.style.height = "2px";
    indicator.style.top = top + "px";
    indicator.style.bottom = "auto";
    indicator.style.transform = "translate3d(" + box.x + "px, 0, 0)";
  }

  function moveIndicator(animate) {
    const nav = document.querySelector(".main-nav");
    if (nav && !nav.querySelector(".nav-indicator")) {
      const mark = document.createElement("span");
      mark.className = "nav-indicator";
      mark.setAttribute("aria-hidden", "true");
      nav.prepend(mark);
    }
    const indicator = nav?.querySelector(".nav-indicator");
    const active = nav?.querySelector(".nav-link.is-active");
    if (!nav || !indicator || !active) return;
    const box = indicatorBox(active);
    let from = null;
    if (animate) {
      try { from = JSON.parse(sessionStorage.getItem("caspian.nav.mark") || "null"); }
      catch { from = null; }
    }
    sessionStorage.removeItem("caspian.nav.mark");
    if (from && Number.isFinite(from.x) && Number.isFinite(from.w)) {
      indicator.classList.remove("is-live");
      placeIndicator(indicator, from, box.top);
      indicator.getBoundingClientRect();
      indicator.classList.add("is-live");
      placeIndicator(indicator, box, box.top);
      return;
    }
    const live = indicator.classList.contains("is-live");
    indicator.classList.remove("is-live");
    placeIndicator(indicator, box, box.top);
    indicator.getBoundingClientRect();
    if (live || animate) indicator.classList.add("is-live");
  }

  function navMarkRemember(nav) {
    if (!nav || nav.dataset.markReady) return;
    nav.dataset.markReady = "1";
    nav.addEventListener("click", (event) => {
      const link = event.target.closest("a.nav-link");
      if (!link || link.classList.contains("is-active")) return;
      const current = nav.querySelector(".nav-link.is-active");
      if (!current) return;
      sessionStorage.setItem("caspian.nav.mark", JSON.stringify(indicatorBox(current)));
    });
  }

  function canSoftNav(url) {
    if (!document.querySelector("#content.page") || !document.querySelector(".main-nav")) return false;
    const path = url.pathname.replace(/\/$/, "") || "/";
    if (path.startsWith("admin-overview.html")) return false;
    if (path.startsWith("/Account") && path !== "app.html") return false;
    return true;
  }

  function shouldSoftNav() {
    return false;
  }

  let navToken = 0;

  async function runPageScripts(doc) {
    const sources = [...doc.querySelectorAll("script[src]")]
      .map((script) => script.getAttribute("src") || "")
      .filter((src) => src.includes("/js/pages/"));
    for (const src of sources) {
      const response = await fetch(src);
      if (!response.ok) throw new Error(src);
      (0, eval)(await response.text());
    }
  }

  async function softGo(url, historyMode) {
    const token = ++navToken;
    const parsed = new URL(url, location.origin);
    const nextUrl = parsed.pathname + parsed.search + parsed.hash;
    setActiveNav(parsed.pathname, true);
    try {
      const response = await fetch(nextUrl, { headers: { Accept: "text/html" } });
      if (!response.ok) throw new Error("status");
      const html = await response.text();
      if (token !== navToken) return;
      const doc = new DOMParser().parseFromString(html, "text/html");
      const nextMain = doc.querySelector("#content");
      const main = document.querySelector("#content");
      if (!nextMain || !main) throw new Error("shell");
      if (historyMode === "push") history.pushState({ caspian: 1 }, "", nextUrl);
      else if (historyMode === "replace") history.replaceState({ caspian: 1 }, "", nextUrl);
      document.title = doc.title;
      main.innerHTML = nextMain.innerHTML;
      window.scrollTo(0, 0);
      closeDropdowns();
      const bar = document.querySelector(".topnav");
      bar?.classList.remove("is-open");
      document.querySelector("[data-nav-toggle]")?.setAttribute("aria-expanded", "false");
      mountPasswords();
      await runPageScripts(doc);
    } catch {
      if (token === navToken) location.href = nextUrl;
    }
  }

  function mountNav() {
    const toggle = document.querySelector("[data-nav-toggle]");
    const bar = document.querySelector(".topnav");
    toggle?.addEventListener("click", () => {
      const open = bar.classList.toggle("is-open");
      toggle.setAttribute("aria-expanded", String(open));
      toggle.setAttribute("aria-label", open ? "Close menu" : "Open menu");
    });
    const arriving = !!sessionStorage.getItem("caspian.nav.mark");
    setActiveNav(location.pathname, arriving);
    navMarkRemember(document.querySelector(".main-nav"));
    window.addEventListener("resize", () => moveIndicator(false));
    document.fonts?.ready.then(() => {
      const indicator = document.querySelector(".nav-indicator");
      if (indicator && !indicator.classList.contains("is-live")) moveIndicator(false);
    });
    window.addEventListener("hashchange", () => setActiveNav(location.pathname, true));
    window.addEventListener("popstate", () => setActiveNav(location.pathname, true));
  }

  function mountAdminShell() {
    if (!document.body.classList.contains("body-admin")) return;
    const header = document.querySelector(".topnav");
    if (!header || document.querySelector(".admin-side")) return;

    const file = (location.pathname.split("/").pop() || "").split("?")[0].toLowerCase();
    const bankPages = [
      ["admin-overview.html", "Dashboard"],
      ["admin-users.html", "Users"],
      ["admin-loans.html", "Loans"],
      ["admin-reports.html", "Reports"],
      ["admin-contact-tickets.html", "Contact Tickets"],
      ["admin-audit-log.html", "Audit Log"]
    ];
    const uiPages = [
      ["admin-app.html", "App"],
      ["admin-home.html", "Home"],
      ["admin-cards.html", "Cards"]
    ];
    const uiFiles = uiPages.map(([href]) => href);
    const bankFiles = bankPages.map(([href]) => href);
    let mode = uiFiles.includes(file) ? "ui" : "bank";

    const side = document.createElement("aside");
    side.className = "admin-side";
    side.setAttribute("aria-label", "Sections");
    header.insertAdjacentElement("afterend", side);

    function remember() {
      try { sessionStorage.setItem("caspian.admin.mode", mode); }
      catch (e) { /* the view still updates */ }
    }

    function renderSide() {
      const pages = mode === "ui" ? uiPages : bankPages;
      const eyebrow = mode === "ui" ? "UI" : "Bank";
      side.innerHTML = `<p class="eyebrow">${eyebrow}</p>` + pages.map(([href, label]) =>
        `<a href="${href}" class="${href === file ? "is-on" : ""}">${esc(label)}</a>`
      ).join("");
    }

    function renderHeader() {
      document.querySelectorAll("[data-admin-mode]").forEach((button) => {
        const on = button.getAttribute("data-admin-mode") === mode;
        button.classList.toggle("is-active", on);
        if (on) button.setAttribute("aria-current", "page");
        else button.removeAttribute("aria-current");
      });
      moveIndicator(false);
    }

    function render() {
      remember();
      document.body.classList.remove("admin-ui");
      renderSide();
      renderHeader();
    }

    header.addEventListener("click", (event) => {
      const button = event.target.closest("[data-admin-mode]");
      if (!button) return;
      const next = button.getAttribute("data-admin-mode") === "ui" ? "ui" : "bank";
      if (next === "ui" && !uiFiles.includes(file)) {
        try { sessionStorage.setItem("caspian.admin.mode", "ui"); } catch (e) { /* still navigate */ }
        location.href = "admin-app.html";
        return;
      }
      if (next === "bank" && !bankFiles.includes(file)) {
        try { sessionStorage.setItem("caspian.admin.mode", "bank"); } catch (e) { /* still navigate */ }
        location.href = "admin-overview.html";
        return;
      }
      mode = next;
      render();
    });

    render();
  }

  function mountGlobal() {
    mountPasswords();
    mountNav();
    mountAdminShell();
    refreshChrome();
    document.querySelectorAll("[data-otp]").forEach(initOtp);
    mountCardPicks();
    document.querySelectorAll("table.data-table").forEach(enhanceTable);
    mountFieldObserver();

    document.addEventListener("click", (event) => {
      const link = event.target.closest("a[href]");
      if (link && shouldSoftNav(event, link)) {
        event.preventDefault();
        softGo(link.href, "push");
        return;
      }

      const logout = event.target.closest("[data-logout]");
      if (logout) {
        const raw = sessionStorage.getItem(KEY);
        if (raw) {
          try {
            const state = JSON.parse(raw);
            state.session = null;
            sessionStorage.setItem(KEY, JSON.stringify(state));
          } catch { sessionStorage.removeItem(KEY); }
        }
        sessionStorage.removeItem(FLOW);
      }

      const toggle = event.target.closest("[data-dropdown-toggle]");
      if (toggle) {
        const dd = toggle.closest("[data-dropdown]");
        const willOpen = !dd.classList.contains("is-open");
        closeDropdowns();
        if (willOpen) {
          dd.classList.add("is-open");
          toggle.setAttribute("aria-expanded", "true");
        }
        return;
      }

      const copyBtn = event.target.closest("[data-copy]");
      if (copyBtn) {
        event.preventDefault();
        copyText(copyBtn.getAttribute("data-copy"), copyBtn.getAttribute("data-copy-label") || "Value");
        return;
      }

      const noteBtn = event.target.closest("[data-note]");
      if (noteBtn) {
        markNote(noteBtn.getAttribute("data-note"));
        return;
      }

      if (event.target.closest("#nav-mark-read")) return;

      if (!event.target.closest("[data-dropdown]")) closeDropdowns();

      const opener = event.target.closest("[data-open-modal]");
      if (opener) openModal(opener.getAttribute("data-open-modal"));

      if (event.target.closest("[data-close-modal]")) {
        closeModal(event.target.closest(".modal"));
      }

      const report = event.target.closest("[data-report]");
      if (report) {
        const id = report.getAttribute("data-report");
        if (id) location.href = "report-transaction.html?id=" + encodeURIComponent(id);
      }
    });

    document.addEventListener("keydown", (event) => {
      if (event.key !== "Escape") return;
      closeDropdowns();
      const confirmBox = document.getElementById("modal-confirm");
      if (confirmBox) {
        confirmBox.querySelector("[data-cancel]")?.click();
        return;
      }
      document.querySelectorAll(".modal:not([hidden])").forEach(closeModal);
    });

    document.getElementById("report-submit")?.addEventListener("click", submitReport);
    mountReportPage();
  }

  function mountReportPage() {
    const page = document.getElementById("report-page");
    if (!page) return;
    const id = new URLSearchParams(location.search).get("id") || "";
    const tx = getState().transactions.find((item) => item.id === id);
    const summary = document.getElementById("report-summary");
    if (!tx || !summary) {
      page.innerHTML = emptyHtml("Transaction not found", "Go back to your history and choose a transaction.", '<a class="btn btn-secondary" href="transaction-history.html">Back to history</a>');
      return;
    }
    reportTx = tx;
    const state = getState();
    summary.innerHTML = `
      <div><dt>Description</dt><dd>${esc(tx.description)}</dd></div>
      <div><dt>Date</dt><dd>${esc(formatDateTime(tx.at))}</dd></div>
      <div><dt>Card</dt><dd>${esc(cardLabel(state, tx.cardId))}</dd></div>
      <div><dt>Amount</dt><dd>${esc(formatMoney(tx.amount))}</dd></div>`;
  }

  window.CaspianStore = {
    get: getState,
    update,
    establish,
    logAudit,
    pushNote,
    getFlow,
    setFlow
  };

  window.BankUI = {
    icon,
    esc,
    formatMoney,
    formatPercent,
    canonicalPhone,
    formatDate,
    formatDateTime,
    maskEmail,
    maskCard,
    formatCard,
    cardCvv,
    initials,
    uid,
    amountHtml,
    badge,
    categoryIcon,
    toast,
    setBusy,
    request,
    setFieldError,
    clearErrors,
    openModal,
    closeModal,
    confirm: confirmDialog,
    plasticHtml,
    plasticPreview,
    cardThumb,
    mountDeck,
    emptyHtml,
    enhanceTable,
    initOtp,
    readOtp,
    commission,
    limitSentence,
    inRange,
    periodBounds,
    downloadText,
    toCsv,
    cardLabel,
    refreshChrome,
    markAllNotes,
    noteHtml,
    openReport
  };

  if (document.readyState === "loading") {
    document.addEventListener("DOMContentLoaded", mountGlobal);
  } else {
    mountGlobal();
  }
})();
