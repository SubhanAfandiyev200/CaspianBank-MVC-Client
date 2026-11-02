"use strict";

// Sağ aşağıdakı pəncərə. İki rejim:
//  - "ai": daxil olmuş istifadəçi AI köməkçiyə (Gemini) sual verir, /Assistant/Ask -> api/assistant. Söhbət yalnız brauzerdə (yaddaşda) saxlanır.
//  - "team": mesaj (istəsə bir şəkillə) işçilərə gedir, /Help/Send -> api/help. Qeydiyyatsız ziyarətçi yalnız bu rejimi görür və email yazır;
//    cavab emailə, daxil olmuş müştəriyə isə bildirişlərinə gəlir.
(function () {
  const root = document.querySelector("[data-chat-widget]");
  if (!root) return;

  const fab = root.querySelector("[data-cw-open]");
  const panel = root.querySelector(".cw-panel");
  const log = root.querySelector("[data-cw-log]");
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
  const greetingTeam = root.getAttribute("data-greeting") || "Hi! How can we help?";
  const greetingAi = root.getAttribute("data-greeting-ai") || greetingTeam;
  const defaultNote = fileNote.textContent;
  const maxImageBytes = 5 * 1024 * 1024;
  const maxHistory = 10;

  let mode = signedIn ? "ai" : "team";
  let history = [];
  let busy = false;

  function addMessage(text, isUser, isError) {
    const row = document.createElement("div");
    row.className = "cw-msg" + (isUser ? " is-user" : "");
    const bubble = document.createElement("div");
    bubble.className = "cw-bubble" + (isError ? " is-error" : "");
    bubble.textContent = text;
    row.appendChild(bubble);
    log.appendChild(row);
    log.scrollTop = log.scrollHeight;
    return row;
  }

  function addTyping() {
    const row = document.createElement("div");
    row.className = "cw-msg";
    row.innerHTML = '<div class="cw-bubble"><span class="cw-typing" aria-label="Sending"><span></span><span></span><span></span></span></div>';
    log.appendChild(row);
    log.scrollTop = log.scrollHeight;
    return row;
  }

  function resetImage() {
    image.value = "";
    fileNote.textContent = defaultNote;
  }

  function applyMode() {
    tabs.forEach((tab) => tab.classList.toggle("is-on", tab.dataset.cwMode === mode));
    teamOnly.forEach((node) => { node.hidden = mode !== "team"; });
    if (aiNote) aiNote.hidden = mode !== "ai";
    if (email) email.hidden = mode !== "team";
  }

  function resetChat() {
    log.innerHTML = "";
    history = [];
    addMessage(mode === "ai" ? greetingAi : greetingTeam, false);
    input.value = "";
    resetImage();
    applyMode();
    autosize();
    busy = false;
    sendButton.disabled = false;
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
    data.append("history", JSON.stringify(history.slice(-maxHistory)));

    const response = await fetch("/Assistant/Ask", { method: "POST", credentials: "same-origin", body: data });
    const body = await readJson(response);
    if (!response.ok) return { ok: false, text: errorText(body) };

    history.push({ role: "user", text: text });
    history.push({ role: "model", text: body.reply });
    return { ok: true, text: body.reply };
  }

  // Komanda rejimi: mesaj (və şəkil) işçilərə gedir
  async function writeToTeam(text) {
    const data = new FormData();
    data.append("__RequestVerificationToken", csrf());
    if (email) data.append("email", email.value.trim());
    data.append("message", text);
    if (image.files.length > 0) data.append("image", image.files[0]);

    const response = await fetch("/Help/Send", { method: "POST", credentials: "same-origin", body: data });
    const body = await readJson(response);
    if (!response.ok) return { ok: false, text: errorText(body) };
    return { ok: true, text: body && body.isGuest
      ? cbt("Thanks! Your message was sent. Our team will reply to your email.")
      : cbt("Thanks! Your message was sent. You will get the answer in your notifications.") };
  }

  async function send(text) {
    const question = text.trim();
    if (!question || busy) return;
    if (mode === "team" && email && email.value.trim() === "") {
      addMessage(cbt("Please write your email so we can reply."), false, true);
      email.focus();
      return;
    }
    busy = true;
    sendButton.disabled = true;
    addMessage(question, true);
    input.value = "";
    autosize();

    const typing = addTyping();
    let result;
    try {
      result = mode === "ai" ? await askAi(question) : await writeToTeam(question);
    } catch (error) {
      result = { ok: false, text: cbt("Something went wrong. Please try again.") };
    }
    typing.remove();
    addMessage(result.text, false, !result.ok);
    if (result.ok && mode === "team") resetImage();
    busy = false;
    sendButton.disabled = false;
    input.focus();
  }

  function autosize() {
    // border-box: kənar xətti (2px) ölçüyə əlavə olunur; 120px-dən uzun olanda scroll çıxır
    input.style.height = "auto";
    input.style.height = Math.min(input.scrollHeight + 2, 120) + "px";
    input.style.overflowY = input.scrollHeight + 2 > 120 ? "auto" : "hidden";
  }

  function open() {
    root.classList.add("is-open");
    fab.setAttribute("aria-expanded", "true");
    panel.setAttribute("aria-hidden", "false");
    // Panelin keçid animasiyası bitəndən sonra yazı sahəsinə fokus
    setTimeout(() => (mode === "team" && email && email.value === "" ? email : input).focus(), 300);
  }

  function close() {
    if (!root.classList.contains("is-open")) return;
    root.classList.remove("is-open");
    fab.setAttribute("aria-expanded", "false");
    panel.setAttribute("aria-hidden", "true");
    fab.focus();
  }

  fab.addEventListener("click", open);
  root.querySelectorAll("[data-cw-close]").forEach((node) => node.addEventListener("click", close));
  root.querySelector("[data-cw-reset]").addEventListener("click", resetChat);
  document.addEventListener("keydown", (event) => {
    if (event.key === "Escape") close();
  });

  tabs.forEach((tab) => {
    tab.addEventListener("click", () => {
      if (tab.dataset.cwMode === mode || busy) return;
      mode = tab.dataset.cwMode;
      resetChat();
    });
  });

  image.addEventListener("change", () => {
    const file = image.files[0];
    if (!file) { fileNote.textContent = defaultNote; return; }
    if (file.size > maxImageBytes) {
      image.value = "";
      fileNote.textContent = cbt("The image must be 5 MB or smaller.");
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
  input.addEventListener("input", autosize);

  resetChat();
})();
