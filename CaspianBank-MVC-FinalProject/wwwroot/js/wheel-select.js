"use strict";

// Təkər tipli seçim (select[data-wheel]): doğum tarixi seçicisi ilə eyni görünüş. Əsl <select> gizli qalır və formada o göndərilir.
// Üstündən çevirmək, kliklə seçmək, yuxarı/aşağı ox düymələri, Done və ya Enter ilə təsdiq, Esc ilə bağlamaq.
(function () {
  const ITEM = 36;
  const chevron = '<svg class="pick-chevron" width="16" height="16" viewBox="0 0 16 16" fill="none" stroke="currentColor" stroke-width="1.6" aria-hidden="true"><path d="M4 6l4 4 4-4"/></svg>';

  function esc(value) {
    return String(value ?? "").replace(/[&<>"']/g, (ch) => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" }[ch]));
  }

  function closeAll(except) {
    document.querySelectorAll(".wheel-date.is-open").forEach((wrap) => {
      if (wrap !== except) wrap._closeWheel?.();
    });
  }

  function bind(select) {
    if (select.dataset.wheelBound || select.closest(".wheel-date")) return;
    select.dataset.wheelBound = "1";

    const wrap = document.createElement("div");
    wrap.className = "wheel-date wheel-select";
    select.parentNode.insertBefore(wrap, select);
    wrap.appendChild(select);
    select.classList.add("wheel-native");
    select.tabIndex = -1;
    select.setAttribute("aria-hidden", "true");

    const button = document.createElement("button");
    button.type = "button";
    button.className = "input wheel-date-btn wheel-select-btn";
    button.setAttribute("aria-haspopup", "listbox");
    button.setAttribute("aria-expanded", "false");
    button.disabled = select.disabled;
    wrap.appendChild(button);

    const pop = document.createElement("div");
    pop.className = "wheel-pop wheel-select-pop";
    pop.hidden = true;
    pop.innerHTML =
      '<div class="wheel-frame is-single"><div class="wheel-highlight" aria-hidden="true"></div>'
      + '<div class="wheel" tabindex="0" role="listbox"></div></div>'
      + '<button type="button" class="btn btn-primary btn-block wheel-done">Done</button>';
    document.body.appendChild(pop);
    const wheel = pop.querySelector(".wheel");
    const done = pop.querySelector(".wheel-done");

    let draft = 0;
    let syncing = false;
    let timer = 0;

    function options() {
      return [...select.options];
    }

    function syncButton() {
      const current = select.options[select.selectedIndex];
      button.innerHTML = '<span class="pick-label">' + esc(current ? current.text : "") + "</span>" + chevron;
    }

    function paint() {
      const list = options();
      draft = Math.max(0, select.selectedIndex);
      wheel.innerHTML = '<div class="wheel-pad"></div>' + list.map((option, i) =>
        '<div class="wheel-item' + (i === draft ? " is-active" : "") + '" role="option" data-i="' + i + '">' + esc(option.text) + "</div>"
      ).join("") + '<div class="wheel-pad"></div>';
      syncing = true;
      wheel.scrollTop = draft * ITEM;
      requestAnimationFrame(() => { syncing = false; });
    }

    function place() {
      const rect = button.getBoundingClientRect();
      const width = Math.max(rect.width, 220);
      const height = pop.offsetHeight || 260;
      let top = rect.bottom + 6;
      if (top + height > window.innerHeight - 8 && rect.top - height - 6 > 8) top = rect.top - height - 6;
      pop.style.width = width + "px";
      pop.style.left = Math.min(Math.max(8, rect.left), window.innerWidth - width - 8) + "px";
      pop.style.top = top + "px";
    }

    function open() {
      if (select.disabled) return;
      closeAll(wrap);
      paint();
      pop.hidden = false;
      place();
      wrap.classList.add("is-open");
      button.setAttribute("aria-expanded", "true");
      wheel.focus({ preventScroll: true });
    }

    function close() {
      pop.hidden = true;
      wrap.classList.remove("is-open");
      button.setAttribute("aria-expanded", "false");
    }
    wrap._closeWheel = close;

    function commit() {
      if (draft !== select.selectedIndex) {
        select.selectedIndex = draft;
        select.dispatchEvent(new Event("change", { bubbles: true }));
      }
      syncButton();
      close();
      button.focus({ preventScroll: true });
    }

    function readScroll() {
      const index = Math.min(options().length - 1, Math.max(0, Math.round(wheel.scrollTop / ITEM)));
      draft = index;
      wheel.querySelectorAll(".wheel-item").forEach((el, i) => el.classList.toggle("is-active", i === index));
    }

    wheel.addEventListener("scroll", () => {
      if (syncing) return;
      window.clearTimeout(timer);
      timer = window.setTimeout(readScroll, 60);
    });
    wheel.addEventListener("click", (event) => {
      const item = event.target.closest(".wheel-item");
      if (!item) return;
      wheel.scrollTo({ top: Number(item.dataset.i) * ITEM, behavior: "smooth" });
      draft = Number(item.dataset.i);
      wheel.querySelectorAll(".wheel-item").forEach((el, i) => el.classList.toggle("is-active", i === draft));
    });
    wheel.addEventListener("dblclick", (event) => {
      if (event.target.closest(".wheel-item")) commit();
    });
    wheel.addEventListener("keydown", (event) => {
      if (event.key === "ArrowUp" || event.key === "ArrowDown") {
        event.preventDefault();
        wheel.scrollTo({ top: wheel.scrollTop + (event.key === "ArrowDown" ? ITEM : -ITEM), behavior: "smooth" });
      } else if (event.key === "Enter") {
        event.preventDefault();
        readScroll();
        commit();
      }
    });
    done.addEventListener("click", () => { readScroll(); commit(); });
    button.addEventListener("click", () => (pop.hidden ? open() : close()));
    document.addEventListener("click", (event) => {
      if (!pop.hidden && !pop.contains(event.target) && !wrap.contains(event.target)) close();
    });
    document.addEventListener("keydown", (event) => { if (event.key === "Escape" && !pop.hidden) close(); });
    window.addEventListener("resize", () => { if (!pop.hidden) place(); });
    window.addEventListener("scroll", (event) => { if (!pop.hidden && !pop.contains(event.target)) place(); }, true);

    select.addEventListener("change", syncButton);
    syncButton();
  }

  document.querySelectorAll("select[data-wheel]").forEach(bind);
})();
