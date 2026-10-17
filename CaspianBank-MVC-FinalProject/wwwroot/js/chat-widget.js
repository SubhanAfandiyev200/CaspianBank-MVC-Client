"use strict";

// Köməkçi paneli (hər səhifədə sağ aşağıdakı düymə). İndi dizayn önizləməsidir: yazılan mesaj görünür,
// amma cavab verən xidmət hələ qoşulmayıb. Real xidmət qoşulanda yalnız reply() dəyişəcək.
(function () {
  const root = document.querySelector("[data-chat-widget]");
  if (!root) return;

  const fab = root.querySelector("[data-cw-open]");
  const panel = root.querySelector(".cw-panel");
  const log = root.querySelector("[data-cw-log]");
  const suggest = root.querySelector("[data-cw-suggest]");
  const form = root.querySelector("[data-cw-form]");
  const input = root.querySelector("[data-cw-input]");
  const sendButton = form.querySelector(".cw-send");
  const greeting = root.getAttribute("data-greeting") || "Hi! How can I help?";

  let busy = false;

  function addMessage(text, isUser) {
    const row = document.createElement("div");
    row.className = "cw-msg" + (isUser ? " is-user" : "");
    const bubble = document.createElement("div");
    bubble.className = "cw-bubble";
    bubble.textContent = text;
    row.appendChild(bubble);
    log.appendChild(row);
    log.scrollTop = log.scrollHeight;
    return row;
  }

  function addTyping() {
    const row = document.createElement("div");
    row.className = "cw-msg";
    row.innerHTML = '<div class="cw-bubble"><span class="cw-typing" aria-label="The assistant is typing"><span></span><span></span><span></span></span></div>';
    log.appendChild(row);
    log.scrollTop = log.scrollHeight;
    return row;
  }

  function resetChat() {
    log.innerHTML = "";
    addMessage(greeting, false);
    suggest.hidden = false;
    input.value = "";
    autosize();
    busy = false;
    sendButton.disabled = false;
  }

  // Real cavab xidməti qoşulanda bu funksiya onu çağıracaq (Promise<string> qaytarmalıdır)
  function reply(question) {
    return new Promise((resolve) => {
      setTimeout(() => {
        resolve("I can't answer yet: the assistant is still a design preview. Soon you will be able to ask me about cards, transfers, fees and loans right here.");
      }, 900);
    });
  }

  async function send(text) {
    const question = text.trim();
    if (!question || busy) return;
    busy = true;
    sendButton.disabled = true;
    suggest.hidden = true;
    addMessage(question, true);
    input.value = "";
    autosize();

    const typing = addTyping();
    let answer;
    try {
      answer = await reply(question);
    } catch (error) {
      answer = "Something went wrong. Please try again.";
    }
    typing.remove();
    addMessage(answer, false);
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
    setTimeout(() => input.focus(), 300);
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

  suggest.addEventListener("click", (event) => {
    const chip = event.target.closest(".cw-chip");
    if (chip) send(chip.textContent);
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
