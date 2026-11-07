"use strict";

// Sağ aşağıdakı pəncərə. İki rejim, hər birinin öz söhbəti var (biri digərinə keçəndə itmir):
//  - "ai": daxil olmuş istifadəçi AI köməkçiyə (Gemini) sual verir, /Assistant/Ask -> api/assistant.
//  - "team": mesaj (istəsə bir şəkillə) işçilərə gedir, /Help/Send -> api/help. Qeydiyyatsız ziyarətçi yalnız bunu görür və email yazır;
//    cavab emailə, daxil olmuş müştəriyə isə bildirişlərinə gəlir.
// Söhbətlər brauzerin sessiya yaddaşında (sessionStorage) saxlanır: səhifələr arasında keçəndə qalır, tab bağlananda silinir.
// AI cavab verərkən belə rejimlər arasında keçmək olar: hər rejimin öz "gözləyir" vəziyyəti var.
(function () {
  const root = document.querySelector("[data-chat-widget]");
  if (!root) return;

  const fab = root.querySelector("[data-cw-open]");
  const panel = root.querySelector(".cw-panel");
  const aiLog = root.querySelector("[data-cw-log]");
  const form = root.querySelector("[data-cw-form]");
  const input = root.querySelector("[data-cw-input]");
  const email = root.querySelector("[data-cw-email]");
  const image = root.querySelector("[data-cw-image]");
  const fileNote = root.querySelector("[data-cw-file]");
  const aiNote = root.querySelector("[data-cw-ai-note]");
  const tabs = Array.from(root.querySelectorAll("[data-cw-mode]"));
  const teamOnly = Array.from(root.querySelectorAll("[data-cw-team-only]"));
  const sendButton = form.querySelector(".cw-send");
  const signedIn = root.getAttribute("data-signed-in") === "true";
  const greetings = {
    ai: root.getAttribute("data-greeting-ai") || root.getAttribute("data-greeting") || "Hi! How can I help?",
    team: root.getAttribute("data-greeting") || "Hi! How can we help?"
  };
  const defaultNote = fileNote.textContent;
  const maxImageBytes = 2 * 1024 * 1024;   // API-nin şəkil limiti ilə eynidir
  const maxHistory = 10;
  const maxEntries = 40;
  const storagePrefix = "cw:" + (signedIn ? "in:" : "out:");

  // Hər rejimin öz söhbət sahəsi: AI üçün səhifədəki, komanda üçün yanına əlavə olunan
  const teamLog = document.createElement("div");
  teamLog.className = aiLog.className;
  teamLog.setAttribute("role", "log");
  teamLog.setAttribute("aria-live", "polite");
  aiLog.after(teamLog);

  const state = {
    ai: { log: aiLog, entries: [], history: [], busy: false, draft: "" },
    team: { log: teamLog, entries: [], history: [], busy: false, draft: "" }
  };
  let mode = "ai";
  // Daxil olmuş müştəridə "Komandaya yaz" söhbəti serverdən gəlir (işçinin cavabları da orada): 10 saniyədən bir yenilənir
  const serverTeam = signedIn && tabs.length > 0;
  let lastTeamSnapshot = "";

  function save(name) {
    try {
      const s = state[name];
      sessionStorage.setItem(storagePrefix + name, JSON.stringify({ entries: s.entries.slice(-maxEntries), history: s.history.slice(-maxHistory) }));
    } catch (error) { /* saxlama bağlıdırsa vacib deyil */ }
  }

  function load(name) {
    try {
      const raw = sessionStorage.getItem(storagePrefix + name);
      if (!raw) return false;
      const saved = JSON.parse(raw);
      if (!Array.isArray(saved.entries) || saved.entries.length === 0) return false;
      state[name].entries = saved.entries;
      state[name].history = Array.isArray(saved.history) ? saved.history : [];
      return true;
    } catch (error) {
      return false;
    }
  }

  function draw(name, text, isUser, isError, imageUrl) {
    const row = document.createElement("div");
    row.className = "cw-msg" + (isUser ? " is-user" : "");
    const bubble = document.createElement("div");
    bubble.className = "cw-bubble" + (isError ? " is-error" : "");
    bubble.textContent = text;
    if (imageUrl) {
      const link = document.createElement("a");
      link.href = imageUrl;
      link.target = "_blank";
      link.rel = "noopener";
      const picture = document.createElement("img");
      picture.src = imageUrl;
      picture.alt = "";
      picture.className = "cw-image";
      link.appendChild(picture);
      bubble.appendChild(link);
    }
    row.appendChild(bubble);
    state[name].log.appendChild(row);
    state[name].log.scrollTop = state[name].log.scrollHeight;
    return row;
  }

  function addMessage(name, text, isUser, isError) {
    state[name].entries.push({ text: text, isUser: !!isUser, isError: !!isError });
    save(name);
    return draw(name, text, isUser, isError);
  }

  function addTyping(name) {
    const row = document.createElement("div");
    row.className = "cw-msg";
    row.innerHTML = '<div class="cw-bubble"><span class="cw-typing" aria-label="Sending"><span></span><span></span><span></span></span></div>';
    state[name].log.appendChild(row);
    state[name].log.scrollTop = state[name].log.scrollHeight;
    return row;
  }

  function resetImage() {
    image.value = "";
    fileNote.textContent = defaultNote;
  }

  function showMode() {
    const sub = root.querySelector("[data-cw-sub]");
    if (sub) {
      const text = sub.querySelector("[data-cw-sub-text]");
      if (text) text.textContent = mode === "ai" ? sub.dataset.subAi : sub.dataset.subTeam;
    }
    tabs.forEach((tab) => tab.classList.toggle("is-on", tab.dataset.cwMode === mode));
    teamOnly.forEach((node) => { node.hidden = mode !== "team"; });
    if (aiNote) aiNote.hidden = mode !== "ai";
    if (email) email.hidden = mode !== "team";
    state.ai.log.hidden = mode !== "ai";
    state.team.log.hidden = mode !== "team";
    input.value = state[mode].draft;
    sendButton.disabled = state[mode].busy;
    autosize();
  }

  function render(name) {
    state[name].log.innerHTML = "";
    state[name].entries.forEach((entry) => draw(name, entry.text, entry.isUser, entry.isError, entry.imageUrl));
  }

  // Seçilmiş rejimin söhbətini təmizləyir (digərinə toxunmur)
  function resetChat() {
    const s = state[mode];
    s.entries = [];
    s.history = [];
    s.draft = "";
    s.log.innerHTML = "";
    try { sessionStorage.removeItem(storagePrefix + mode); } catch (error) { /* vacib deyil */ }
    addMessage(mode, greetings[mode], false);
    resetImage();
    showMode();
  }

  async function syncTeam() {
    if (!serverTeam || state.team.busy) return;
    try {
      const response = await fetch("/Help/Conversation", { credentials: "same-origin", cache: "no-store" });
      if (!response.ok) return;
      const messages = await response.json();
      const snapshot = JSON.stringify(messages);
      if (snapshot === lastTeamSnapshot) return;
      lastTeamSnapshot = snapshot;
      state.team.entries = [{ text: greetings.team, isUser: false, isError: false }].concat(messages.map((m) => ({ text: m.body, isUser: !m.isStaff, isError: false, imageUrl: m.imageUrl || null })));
      render("team");
      state.team.log.scrollTop = state.team.log.scrollHeight;
    } catch (error) { /* şəbəkə yoxdursa növbəti yoxlamada */ }
  }

  function csrf() {
    const token = form.querySelector("input[name='__RequestVerificationToken']");
    return token ? token.value : "";
  }

  async function readJson(response) {
    try { return await response.json(); } catch (error) { return null; }
  }

  function errorText(body) {
    return body && Array.isArray(body.errors) && body.errors.length ? body.errors.join(" ") : cbt("Something went wrong. Please try again.");
  }

  // AI rejimi: sual API-yə gedir, cavab gəlir; söhbət tarixçəsi (son 10 mesaj) kontekst üçün göndərilir
  async function askAi(text) {
    const data = new FormData();
    data.append("__RequestVerificationToken", csrf());
    data.append("message", text);
    data.append("history", JSON.stringify(state.ai.history.slice(-maxHistory)));

    const response = await fetch("/Assistant/Ask", { method: "POST", credentials: "same-origin", body: data });
    const body = await readJson(response);
    if (!response.ok) return { ok: false, text: errorText(body) };

    state.ai.history.push({ role: "user", text: text });
    state.ai.history.push({ role: "model", text: body.reply });
    return { ok: true, text: body.reply };
  }

  // Komanda rejimi: mesaj (və şəkil) işçilərə gedir
  async function writeToTeam(text, file) {
    const data = new FormData();
    data.append("__RequestVerificationToken", csrf());
    if (email) data.append("email", email.value.trim());
    data.append("message", text);
    if (file) data.append("image", file);

    const response = await fetch("/Help/Send", { method: "POST", credentials: "same-origin", body: data });
    const body = await readJson(response);
    if (!response.ok) return { ok: false, text: errorText(body) };
    return { ok: true, text: body && body.isGuest
      ? cbt("Thanks! Your message was sent. Our team will reply to your email.")
      : cbt("Thanks! Your message was sent. You will get the answer in your notifications.") };
  }

  async function send(text) {
    const name = mode;
    const s = state[name];
    const question = text.trim();
    if (!question || s.busy) return;
    if (name === "team" && email && email.value.trim() === "") {
      addMessage(name, cbt("Please write your email so we can reply."), false, true);
      email.focus();
      return;
    }

    const file = name === "team" && image.files.length > 0 ? image.files[0] : null;
    s.busy = true;
    sendButton.disabled = true;
    addMessage(name, question, true);
    s.draft = "";
    input.value = "";
    autosize();

    const typing = addTyping(name);
    let result;
    try {
      result = name === "ai" ? await askAi(question) : await writeToTeam(question, file);
    } catch (error) {
      result = { ok: false, text: cbt("Something went wrong. Please try again.") };
    }
    typing.remove();
    addMessage(name, result.text, false, !result.ok);
    if (result.ok && name === "team") resetImage();
    s.busy = false;
    if (name === "team" && serverTeam) {
      lastTeamSnapshot = "";
      syncTeam();
    }
    if (mode === name) {
      sendButton.disabled = false;
      input.focus();
    }
  }

  function autosize() {
    // border-box: kənar xətti (2px) ölçüyə əlavə olunur; 120px-dən uzun olanda scroll çıxır
    input.style.height = "auto";
    input.style.height = Math.min(input.scrollHeight + 2, 120) + "px";
    input.style.overflowY = input.scrollHeight + 2 > 120 ? "auto" : "hidden";
  }

  function open() {
    root.classList.add("is-open");
    root.classList.remove("has-new");
    fab.setAttribute("aria-expanded", "true");
    panel.setAttribute("aria-hidden", "false");
    // Panelin keçid animasiyası bitəndən sonra yazı sahəsinə fokus
    setTimeout(() => (mode === "team" && email && email.value === "" ? email : input).focus(), 300);
    state[mode].log.scrollTop = state[mode].log.scrollHeight;
    syncTeam();
  }

  function close() {
    if (!root.classList.contains("is-open")) return;
    root.classList.remove("is-open");
    fab.setAttribute("aria-expanded", "false");
    panel.setAttribute("aria-hidden", "true");
    fab.focus();
  }

  // Canlı bildiriş (SignalR): işçi cavab yazan kimi söhbət dərhal yenilənir. Pəncərə bağlıdırsa düymədə nöqtə görünür
  document.addEventListener("caspian:notification", (event) => {
    const note = event.detail || {};
    if (!serverTeam || !String(note.linkUrl || "").startsWith("/App/Support/Ticket/")) return;
    lastTeamSnapshot = "";
    syncTeam();
    if (!root.classList.contains("is-open")) root.classList.add("has-new");
  });

  fab.addEventListener("click", open);
  root.querySelectorAll("[data-cw-close]").forEach((node) => node.addEventListener("click", close));
  root.querySelector("[data-cw-reset]").addEventListener("click", resetChat);
  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") close();
  });

  // Rejimlər arasında keçid: AI cavab verərkən də mümkündür, hər rejimin söhbəti saxlanır
  tabs.forEach((tab) => {
    tab.addEventListener("click", () => {
      const next = tab.dataset.cwMode;
      if (next === mode) return;
      state[mode].draft = input.value;
      mode = next;
      showMode();
      state[mode].log.scrollTop = state[mode].log.scrollHeight;
      if (mode === "team") syncTeam();
      input.focus();
    });
  });

  input.addEventListener("input", () => { state[mode].draft = input.value; autosize(); });

  image.addEventListener("change", () => {
    const file = image.files[0];
    if (!file) { fileNote.textContent = defaultNote; return; }
    if (file.size > maxImageBytes) {
      image.value = "";
      fileNote.textContent = cbt("The image must be 2 MB or smaller.");
      return;
    }
    fileNote.textContent = file.name;
  });

  form.addEventListener("submit", (event) => {
    event.preventDefault();
    send(input.value);
  });
  // Enter göndərir, Shift+Enter yeni sətir
  input.addEventListener("keydown", (event) => {
    if (event.key === "Enter" && !event.shiftKey) {
      event.preventDefault();
      send(input.value);
    }
  });

  // Səhifənin ən aşağısında footerin alt zolağı görünəndə düymə yuxarı qalxır: sosial ikonlar sağda öz yerində qalır, düymə onların üstündə dayanır
  const footerBar = document.querySelector(".footer-bottom, .site-footer-inner");
  if (footerBar && "IntersectionObserver" in window) {
    new IntersectionObserver((items) => {
      items.forEach((item) => root.classList.toggle("cw-lift", item.isIntersecting));
    }, { threshold: 0 }).observe(footerBar);
  }

  // Başlanğıc: saxlanmış söhbət varsa o, yoxdursa salamlama
  ["ai", "team"].forEach((name) => {
    if (!(name === "team" && serverTeam) && load(name)) {
      render(name);
    } else {
      addMessage(name, greetings[name], false);
    }
  });
  showMode();
  syncTeam();
  setInterval(() => { if (root.classList.contains("is-open")) syncTeam(); }, 10000);
})();
