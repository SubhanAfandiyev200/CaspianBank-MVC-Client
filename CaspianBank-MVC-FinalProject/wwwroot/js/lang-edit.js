"use strict";

// Admin yaratma və redaktə səhifələrində dil seçimi: EN | AZ | RU.
//  - EN: səhifənin öz sahələridir (ingilis mətn bazadakı əsas mətndir).
//  - AZ / RU: eyni sahələrdə həmin dilin mətni yazılır (redaktədə mövcud tərcümə gətirilir). Tərcümə ingilis mətnə bağlıdır,
//    ona görə AZ/RU rejimində yalnız mətn sahələri (data-translatable) göstərilir, şəkil və digər sahələr gizlədilir.
//  - "Save" / "Create" bir dəfə basılır: əsas forma ingilis mətnlə göndərilir, AZ və RU mətnləri gizli sahədə (__translations) gedir,
//    server (Helpers/SaveTranslationsFilter.cs) əməliyyat uğurlu olandan sonra onları yazır.
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
  const stashKey = "lang-edit:" + location.pathname;

  // Səhifə açılanda saxlanmış ingilis mətnlər: mövcud tərcümələrin açarı bunlardır (yaratmada hamısı boşdur)
  const saved = fields.map((field) => field.value);
  const isEdit = saved.some((value) => value.trim() !== "");
  let english = saved.slice();                   // ingilis mətnin cari (yazılmış) halı
  const drafts = { az: null, ru: null };         // dil açılmayıbsa null: o dilə toxunulmur
  let current = "en";

  fields.forEach((field) => {
    field.dataset.placeholder = field.placeholder || "";
    const wrapper = field.closest(".field");
    if (wrapper) wrapper.classList.add("has-translatable");
  });

  function say(message, isError) {
    if (!status) return;
    status.textContent = message || "";
    status.classList.toggle("is-error", !!isError);
  }

  function values() {
    return fields.map((field) => field.value);
  }

  function keep() {
    if (current === "en") english = values();
    else drafts[current] = values();
  }

  function show(list, placeholders) {
    fields.forEach((field, i) => {
      field.value = list[i] || "";
      field.placeholder = placeholders ? english[i] : field.dataset.placeholder;
    });
  }

  function mark(language) {
    buttons.forEach((button) => button.classList.toggle("is-on", button.dataset.lang === language));
    form.classList.toggle("is-translating", language !== "en");
  }

  async function lookup(language) {
    if (!isEdit) return saved.map(() => "");
    const data = new URLSearchParams();
    if (tokenInput) data.append("__RequestVerificationToken", tokenInput.value);
    data.append("language", language);
    saved.forEach((key) => data.append("keys", key));
    const response = await fetch("/Admin/ContentTranslations/Lookup", {
      method: "POST",
      credentials: "same-origin",
      headers: { "Content-Type": "application/x-www-form-urlencoded", "X-Requested-With": "XMLHttpRequest" },
      body: data.toString()
    });
    if (!response.ok) throw new Error("lookup");
    const dictionary = await response.json();
    return saved.map((key) => dictionary[key] || "");
  }

  async function open(language) {
    keep();
    if (language === "en") {
      show(english, false);
      current = "en";
      mark("en");
      say("");
      return;
    }

    if (drafts[language] === null) {
      say(t("Loading..."));
      try {
        drafts[language] = await lookup(language);
      } catch (error) {
        say(t("The translations can't be loaded right now. Please try again in a moment."), true);
        // Əvvəlki rejimin dəyərləri yerində qalır
        show(current === "en" ? english : drafts[current], current !== "en");
        return;
      }
    }
    show(drafts[language], true);
    current = language;
    mark(language);
    say(t("Editing the {0} translation. The English text is shown as a hint. Leave a field empty to show the English text.", names[language]));
  }

  buttons.forEach((button) => {
    button.addEventListener("click", () => {
      if (button.dataset.lang !== current) open(button.dataset.lang);
    });
  });

  // Göndərməzdən əvvəl: əsas forma ingilis mətnlə gedir, AZ/RU mətnləri gizli sahədə
  form.addEventListener("submit", () => {
    keep();
    const payload = {};
    ["az", "ru"].forEach((language) => {
      if (drafts[language] === null) return;
      payload[language] = {};
      fields.forEach((field, i) => {
        if (field.name) payload[language][field.name] = drafts[language][i];
      });
    });

    // Server xəta qaytarsa (məs. boş məcburi sahə) yazılanlar itməsin deyə müvəqqəti saxlanılır
    try {
      sessionStorage.setItem(stashKey, JSON.stringify({ english: english, az: drafts.az, ru: drafts.ru }));
    } catch (error) { /* saxlama bağlıdırsa vacib deyil */ }

    let hidden = form.querySelector("input[name='__translations']");
    if (!hidden) {
      hidden = document.createElement("input");
      hidden.type = "hidden";
      hidden.name = "__translations";
      form.appendChild(hidden);
    }
    hidden.value = Object.keys(payload).length ? JSON.stringify(payload) : "";

    // Gizli sahələrdəki AZ/RU mətni əsas formaya düşməsin
    show(english, false);
  }, true);

  // Səhifə xəta ilə yenidən açılıbsa (validasiya) əvvəl yazılanlar qaytarılır, əks halda müvəqqəti saxlanan silinir
  try {
    const raw = sessionStorage.getItem(stashKey);
    const hasError = document.querySelector(".field-error:not(:empty), .flash-error");
    if (raw && hasError) {
      const stash = JSON.parse(raw);
      if (Array.isArray(stash.english) && stash.english.length === fields.length) english = stash.english;
      ["az", "ru"].forEach((language) => {
        if (Array.isArray(stash[language]) && stash[language].length === fields.length) drafts[language] = stash[language];
      });
      show(english, false);
      if (drafts.az || drafts.ru) say(t("Your AZ and RU texts are kept. Fix the error and save again."));
    }
    if (raw && !hasError) sessionStorage.removeItem(stashKey);
  } catch (error) { /* vacib deyil */ }
})();
