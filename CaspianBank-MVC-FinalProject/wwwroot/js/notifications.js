"use strict";

// Bildiriş zəngi və canlı bildirişlər (SignalR). Başlanğıc siyahı serverdən gəlir; bu fayl:
//  - zəngi açıb-bağlayır,
//  - API-nin hub-ına qoşulur (əsas JWT cookie-də gizli qalır, JS yalnız 2 dəqiqəlik hub bileti alır),
//  - yeni bildirişi sayğaca, zəngin siyahısına, bildirişlər səhifəsinə və qısa pop-up-a (toast) əlavə edir.
// Ikonlar və vaxt qaydası Helpers/NotificationUi.cs ilə eynidir.
(function () {
  const bell = document.querySelector("[data-bell]");
  if (!bell) return;

  const toggle = bell.querySelector("[data-bell-toggle]");
  const countEl = bell.querySelector("[data-bell-count]");
  const list = bell.querySelector("[data-bell-list]");
  const emptyEl = bell.querySelector("[data-bell-empty]");
  const liveEl = bell.querySelector("[data-bell-live]");
  const ticketUrl = bell.getAttribute("data-ticket-url");
  const pageList = document.querySelector("[data-note-page-list]");
  const MAX_IN_BELL = 6;

  const ICONS = {
    Loan: '<path d="M4 10l8-6 8 6"/><path d="M6 10v8h12v-8"/><path d="M10 18v-4h4v4"/>',
    Transfer: '<path d="M7 17 17 7"/><path d="M9 7h8v8"/>',
    Card: '<rect x="3" y="6" width="18" height="13" rx="2.5"/><path d="M3 10.5h18M7 15h3"/>',
    Staff: '<path d="M4 6h16v10H7l-3 3z"/>',
    Support: '<path d="M4 6h16v10H7l-3 3z"/>',
    Bill: '<path d="M13 3 5 14h6l-1 7 8-11h-6z"/>',
    Default: '<path d="M6 17h12l-1.5-2V11a4.5 4.5 0 0 0-9 0v4z"/><path d="M10 20a2 2 0 0 0 4 0"/>'
  };

  function esc(value) {
    const span = document.createElement("span");
    span.textContent = value == null ? "" : String(value);
    return span.innerHTML.replace(/"/g, "&quot;");
  }

  function ago(iso) {
    const then = new Date(iso);
    const seconds = (Date.now() - then.getTime()) / 1000;
    if (seconds < 45) return "just now";
    if (seconds < 3600) return Math.max(1, Math.round(seconds / 60)) + " min ago";
    if (seconds < 86400) return Math.floor(seconds / 3600) + " h ago";
    return then.toLocaleString("en-GB", { day: "2-digit", month: "short", hour: "2-digit", minute: "2-digit" });
  }

  function openUrl(n) {
    let url = "/App/Notifications/Open/" + n.id;
    if (n.linkUrl && n.linkUrl.charAt(0) === "/" && n.linkUrl.charAt(1) !== "/") {
      url += "?to=" + encodeURIComponent(n.linkUrl);
    }
    return url;
  }

  function noteHtml(n) {
    const icon = ICONS[n.category] || ICONS.Default;
    return '<a class="note' + (n.isRead ? "" : " is-unread") + '" href="' + esc(openUrl(n)) + '">'
      + '<span class="note-icon"><svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' + icon + "</svg></span>"
      + "<span>"
      + '<span class="spread"><span class="note-title">' + esc(n.title) + '</span><time datetime="' + esc(n.createdAt) + '">' + esc(ago(n.createdAt)) + "</time></span>"
      + '<span class="note-body">' + esc(n.message) + "</span>"
      + "</span></a>";
  }

  function setCount(count) {
    if (!countEl) return;
    countEl.textContent = count > 99 ? "99+" : String(count);
    countEl.hidden = count <= 0;
  }

  function prependTo(container, emptyNode, html, limit) {
    if (!container) return;
    if (emptyNode) emptyNode.hidden = true;
    container.insertAdjacentHTML("afterbegin", html);
    if (limit) {
      const notes = container.querySelectorAll(".note");
      for (let i = limit; i < notes.length; i++) notes[i].remove();
    }
  }

  function toast(n) {
    let root = document.querySelector(".toast-root");
    if (!root) {
      root = document.createElement("div");
      root.className = "toast-root";
      root.setAttribute("aria-live", "polite");
      document.body.appendChild(root);
    }
    const item = document.createElement("a");
    item.className = "toast toast-note";
    item.href = openUrl(n);
    item.innerHTML = "<strong>" + esc(n.title) + "</strong><span>" + esc(n.message) + "</span>";
    root.appendChild(item);
    setTimeout(() => {
      item.classList.add("is-out");
      setTimeout(() => item.remove(), 220);
    }, 7000);
  }

  function onNotification(payload) {
    const n = payload.notification;
    setCount(payload.unreadCount);
    prependTo(list, emptyEl, noteHtml(n), MAX_IN_BELL);
    prependTo(pageList, document.querySelector("[data-note-page-empty]"), noteHtml(n), 0);
    document.querySelectorAll("[data-notes-page] button[disabled]").forEach((button) => { button.disabled = false; });
    toast(n);
  }

  // ---------- zəngin açılıb-bağlanması ----------
  function closeBell() {
    bell.classList.remove("is-open");
    toggle.setAttribute("aria-expanded", "false");
  }

  toggle.addEventListener("click", (event) => {
    event.stopPropagation();
    const open = bell.classList.toggle("is-open");
    toggle.setAttribute("aria-expanded", String(open));
  });
  document.addEventListener("click", (event) => {
    if (!event.target.closest("[data-bell]")) closeBell();
  });
  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") closeBell();
  });

  // "5 min ago" yazıları köhnəlməsin
  setInterval(() => {
    document.querySelectorAll("time[datetime]").forEach((node) => { node.textContent = ago(node.getAttribute("datetime")); });
  }, 60000);

  // ---------- canlı bağlantı (SignalR) ----------
  function setLive(text) {
    if (liveEl) liveEl.textContent = text;
  }

  async function fetchTicket() {
    const response = await fetch(ticketUrl, { credentials: "same-origin", cache: "no-store" });
    if (!response.ok) throw new Error("ticket " + response.status);
    return response.json();
  }

  async function connect() {
    if (!window.signalR) {
      setLive("");
      return;
    }

    let first;
    try {
      first = await fetchTicket();
    } catch (error) {
      setLive("Offline");
      setTimeout(connect, 20000);
      return;
    }

    // Yenidən qoşulanda hər dəfə təzə bilet alınır (köhnəsi 2 dəqiqəyə bitir)
    let firstToken = first.token;
    const connection = new signalR.HubConnectionBuilder()
      .withUrl(first.hubUrl, {
        accessTokenFactory: async () => {
          if (firstToken) {
            const token = firstToken;
            firstToken = null;
            return token;
          }
          return (await fetchTicket()).token;
        },
        skipNegotiation: true,
        transport: signalR.HttpTransportType.WebSockets
      })
      .withAutomaticReconnect([0, 2000, 5000, 10000, 30000])
      .build();

    connection.on("notification", onNotification);
    connection.onreconnecting(() => setLive("Reconnecting…"));
    connection.onreconnected(() => setLive("Live"));
    connection.onclose(() => {
      setLive("Offline");
      setTimeout(connect, 20000);
    });

    try {
      await connection.start();
      setLive("Live");
    } catch (error) {
      setLive("Offline");
      setTimeout(connect, 20000);
    }
  }

  connect();
})();
