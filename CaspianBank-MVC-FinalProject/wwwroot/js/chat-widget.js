"use strict";

// "Help" paneli (hər səhifədə sağ aşağıdakı düymə). Yazılan mesaj (istəsə bir şəkillə) /Help/Send ilə API-yə gedir və işçilərə bildiriş düşür.
// Qeydiyyatsız ziyarətçi email yazır (cavab ora gedir); daxil olmuş müştəridə email sahəsi yoxdur (cavab bildirişlərinə düşür).
// AI köməkçi qoşulanda yalnız send() içindəki göndərmə hissəsi dəyişəcək.
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
  const sendButton = form.querySelector(".cw-send");
  const greeting = root.getAttribute("data-greeting") || "Hi! How can we help?";
  const defaultNote = fileNote.textContent;
  const maxImageBytes = 5 * 1024 * 1024;

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

  function resetChat() {
    log.innerHTML = "";
    addMessage(greeting, false);
    input.value = "";
    resetImage();
    autosize();
    busy = false;
    sendButton.disabled = false;
  }

  // Mesajı API-yə göndərir; uğurlu olanda təsdiq mətnini, olmayanda xətanı qaytarır
  async function deliver(text) {
    const data = new FormData();
    const token = form.querySelector("input[name='__RequestVerificationToken']");
    if (token) data.append("__RequestVerificationToken", token.value);
    if (email) data.append("email", email.value.trim());
    data.append("message", text);
    if (image.files.length > 0) data.append("image", image.files[0]);

    const response = await fetch("/Help/Send", { method: "POST", credentials: "same-origin", body: data });
    let body = null;
    try { body = await response.json(); } catch (error) { /* gövdə yoxdur */ }
    if (!response.ok) {
      const errors = body && Array.isArray(body.errors) && body.errors.length ? body.errors.join(" ") : cbt("Something went wrong. Please try again.");
      return { ok: false, text: errors };
    }
    return { ok: true, text: body && body.isGuest
      ? cbt("Thanks! Your message was sent. Our team will reply to your email.")
      : cbt("Thanks! Your message was sent. You will get the answer in your notifications.") };
  }

  async function send(text) {
    const question = text.trim();
    if (!question || busy) return;
    if (email && email.value.trim() === "") {
      addMessage(cbt("Please write your email so we can reply."), false, true);
      email.focus();
      return;
    }
    busy = true;
    sendButton.disabled = true;
    addMessage(question, true);

    const typing = addTyping();
    let result;
    try {
      result = await deliver(question);
    } catch (error) {
      result = { ok: false, text: cbt("Something went wrong. Please try again.") };
    }
    typing.remove();
    addMessage(result.text, false, !result.ok);
    if (result.ok) {
      input.value = "";
      resetImage();
      autosize();
    }
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
    setTimeout(() => (email && email.value === "" ? email : input).focus(), 300);
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
