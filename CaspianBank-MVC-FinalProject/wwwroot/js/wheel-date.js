/* Təkər tipli tarix seçici (script.js-dən ayrılıb, yalnız auth səhifələri üçün) */
(function () {
  const MONTHS = ["January", "February", "March", "April", "May", "June", "July", "August", "September", "October", "November", "December"];
  const MONTHS_SHORT = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sept", "Oct", "Nov", "Dec"];
  const WHEEL_ITEM = 36;

  function esc(value) {
    return String(value ?? "").replace(/[&<>"']/g, (ch) => ({
      "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;"
    }[ch]));
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

  function enhanceDates() {
    document.querySelectorAll('input[type="date"]').forEach(bindWheelDate);
    document.addEventListener("click", (event) => {
      document.querySelectorAll(".wheel-date.is-open").forEach((wrap) => {
        if (wrap.contains(event.target) || wrap._wheelPop?.contains(event.target)) return;
        wrap._closeWheel?.(true);
      });
    });
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", enhanceDates);
  else enhanceDates();
})();
