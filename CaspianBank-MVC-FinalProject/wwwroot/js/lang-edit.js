"use strict";

// Admin redaktə səhifələrində dil seçimi: EN | AZ | RU.
//  - EN: səhifənin öz formasıdır (ingilis mətn bazadakı əsas mətndir).
//  - AZ / RU: eyni sahələrdə həmin dilin tərcüməsi göstərilir və yadda saxlanır. Tərcümə ingilis mətnə bağlıdır (açar = səhifə açılanda saxlanmış ingilis mətn),
//    ona görə burada yalnız mətn sahələri (data-translatable) göstərilir, şəkil və digər sahələr gizlədilir.
// Mətn sahəsinə data-translatable yazılır; səhifədə <div data-lang-tabs> olmalıdır.
(function () {
  const tabs = document.querySelector("[data-lang-tabs]");
  if (!tabs) return;

  const form = document.querySelector("main form[method='post']");
  if (!form) return;

  const fields = Array.from(form.querySelectorAll("[data-translatable]"));
  if (fields.length === 0) {
    tabs.hidden = true;
    return;
  }

  const t = (...args) => (window.cbt ? window.cbt(...args) : args[0]);
  const tokenInput = form.querySelector("input[name='__RequestVerificationToken']");
  const status = tabs.querySelector("[data-lang-status]");
  const buttons = Array.from(tabs.querySelectorAll("button[data-lang]"));
  const names = { az: "Azərbaycanca", ru: "Русский" };

  // Səhifə açılanda saxlanmış ingilis mətnlər: tərcümənin açarı bunlardır
  const originals = fields.map((field) => field.value);
  let drafts = null;          // EN rejimindən çıxanda yazılmış, hələ saxlanmamış mətnlər
  let current = "en";

  fields.forEach((field) => {
    const wrapper = field.closest(".field");
    if (wrapper) wrapper.classList.add("has-translatable");
  });

  function say(message, isError) {
    if (!status) return;
    status.textContent = message || "";
    status.classList.toggle("is-error", !!isError);
  }

  function post(url, body) {
    const data = new URLSearchParams();
    if (tokenInput) data.append("__RequestVerificationToken", tokenInput.value);
    Object.keys(body).forEach((key) => {
      const value = body[key];
      if (Array.isArray(value)) value.forEach((item) => data.append(key, item));
      else data.append(key, value);
    });
    return fetch(url, {
      method: "POST",
      credentials: "same-origin",
      headers: { "Content-Type": "application/x-www-form-urlencoded", "X-Requested-With": "XMLHttpRequest" },
      body: data.toString()
    });
  }

  function mark(language) {
    buttons.forEach((button) => button.classList.toggle("is-on", button.dataset.lang === language));
    form.classList.toggle("is-translating", language !== "en");
  }

  async function showEnglish() {
    fields.forEach((field, i) => {
      field.value = drafts ? drafts[i] : originals[i];
      field.placeholder = field.dataset.placeholder || "";
    });
    current = "en";
    mark("en");
    say("");
  }

  async function showLanguage(language) {
    if (current === "en") drafts = fields.map((field) => field.value);
    say(t("Loading..."));
    let dictionary = {};
    try {
      const response = await post("/Admin/ContentTranslations/Lookup", { language: language, keys: originals });
      if (response.ok) dictionary = await response.json();
      else throw new Error("lookup");
    } catch (error) {
      say(t("The translations can't be loaded right now. Please try again in a moment."), true);
      return;
    }
    fields.forEach((field, i) => {
      if (field.dataset.placeholder === undefined) field.dataset.placeholder = field.placeholder || "";
      field.value = dictionary[originals[i]] || "";
      field.placeholder = originals[i];
    });
    current = language;
    mark(language);
    say(t("Editing the {0} translation. The English text is shown as a hint. Leave a field empty to show the English text.", names[language]));
  }

  buttons.forEach((button) => {
    button.addEventListener("click", () => {
      const language = button.dataset.lang;
      if (language === current) return;
      if (language === "en") showEnglish();
      else showLanguage(language);
    });
  });

  // AZ/RU rejimində forma adi göndərilmir: tərcümələr ayrıca yazılır
  form.addEventListener("submit", async (event) => {
    if (current === "en") return;
    event.preventDefault();
    event.stopImmediatePropagation();

    const submit = form.querySelector("button[type='submit']");
    if (submit) submit.disabled = true;
    say(t("Saving..."));
    try {
      const response = await post("/Admin/ContentTranslations/Save", {
        language: current,
        keys: originals,
        values: fields.map((field) => field.value)
      });
      const result = await response.json();
      if (response.ok) say(result.message || t("The translation was saved."));
      else say((result.errors || []).join(" ") || t("Something went wrong. Please try again."), true);
    } catch (error) {
      say(t("Something went wrong. Please try again."), true);
    } finally {
      if (submit) submit.disabled = false;
    }
  }, true);
})();
