"use strict";

// "Contact us" səhifəsindəki AI söhbəti (yuxarıdakı böyük pəncərə). Sağ aşağıdakı pəncərənin AI bölməsi ilə EYNİ söhbəti göstərir:
// ikisi də sessionStorage-dakı "cw:in:ai" açarını oxuyur və yazır (səhifə yenilənəndə biri digərinin yazdığını görür).
(function () {
  const root = document.querySelector("[data-support-chat]");
  if (!root) return;

  const log = root.querySelector("[data-sc-log]");
  const form = root.querySelector("[data-sc-form]");
  const input = root.querySelector("[data-sc-input]");
  const button = root.querySelector("[data-sc-send]");
  const chips = Array.from(root.querySelectorAll("[data-sc-chip]"));
  const greeting = root.getAttribute("data-greeting") || "";
  const storageKey = "cw:in:ai";
  const maxHistory = 10;
  const maxEntries = 40;

  let entries = [];
  let history = [];
  let busy = false;

  function load() {
    try {
      const saved = JSON.parse(sessionStorage.getItem(storageKey) || "null");
      if (saved && Array.isArray(saved.entries) && saved.entries.length > 0) {
        entries = saved.entries;
        history = Array.isArray(saved.history) ? saved.history : [];
        return;
      }
    } catch (error) { /* saxlama bağlıdırsa vacib deyil */ }
    entries = [{ text: greeting, isUser: false, isError: false }];
    history = [];
  }

  function save() {
    try {
      sessionStorage.setItem(storageKey, JSON.stringify({ entries: entries.slice(-maxEntries), history: history.slice(-maxHistory) }));
    } catch (error) { /* vacib deyil */ }
  }

  function draw(entry) {
    const row = document.createElement("div");
    row.className = "chat-msg" + (entry.isUser ? " is-user" : "");
    const bubble = document.createElement("div");
    bubble.className = "chat-bubble";
    bubble.style.whiteSpace = "pre-wrap";
    if (entry.isError) bubble.classList.add("is-error");
    bubble.textContent = entry.text;
    row.appendChild(bubble);
    log.appendChild(row);
    log.scrollTop = log.scrollHeight;
    return row;
  }

  function add(text, isUser, isError) {
    const entry = { text: text, isUser: !!isUser, isError: !!isError };
    entries.push(entry);
    save();
    return draw(entry);
  }

  function csrf() {
    const token = form.querySelector("input[name='__RequestVerificationToken']");
    return token ? token.value : "";
  }

  async function ask(question) {
    const data = new FormData();
    data.append("__RequestVerificationToken", csrf());
    data.append("message", question);
    data.append("history", JSON.stringify(history.slice(-maxHistory)));
    const response = await fetch("/Assistant/Ask", { method: "POST", credentials: "same-origin", body: data });
    let body = null;
    try { body = await response.json(); } catch (error) { /* gövdə yoxdur */ }
    if (!response.ok) {
      return { ok: false, text: body && Array.isArray(body.errors) && body.errors.length ? body.errors.join(" ") : cbt("Something went wrong. Please try again.") };
    }
    history.push({ role: "user", text: question });
    history.push({ role: "model", text: body.reply });
    return { ok: true, text: body.reply };
  }

  async function send(text) {
    const question = (text || "").trim();
    if (!question || busy) return;
    busy = true;
    button.disabled = true;
    chips.forEach((chip) => { chip.disabled = true; });
    add(question, true);
    input.value = "";

    const typing = document.createElement("div");
    typing.className = "chat-msg";
    typing.innerHTML = '<div class="chat-bubble"><span class="cw-typing"><span></span><span></span><span></span></span></div>';
    log.appendChild(typing);
    log.scrollTop = log.scrollHeight;

    let result;
    try {
      result = await ask(question);
    } catch (error) {
      result = { ok: false, text: cbt("Something went wrong. Please try again.") };
    }
    typing.remove();
    add(result.text, false, !result.ok);
    busy = false;
    button.disabled = false;
    chips.forEach((chip) => { chip.disabled = false; });
    input.focus();
  }

  chips.forEach((chip) => chip.addEventListener("click", () => send(chip.textContent)));
  form.addEventListener("submit", (event) => {
    event.preventDefault();
    send(input.value);
  });

  load();
  entries.forEach(draw);
})();
