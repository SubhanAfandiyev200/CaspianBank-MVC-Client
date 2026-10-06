/* Login və Register səhifələri üçün ortaq sahələr: telefon maskası və email-in səhifələr arasında daşınması */
(function () {
  const EMAIL_KEY = "caspian.authEmail";

  // Telefon: 0 ilə başlayırsa 10 rəqəm (3-3-2-2), 0-suz olarsa 9 rəqəm (2-3-2-2)
  function formatPhone(raw) {
    let digits = String(raw || "").replace(/\D/g, "");
    const withZero = digits.startsWith("0");
    digits = digits.slice(0, withZero ? 10 : 9);
    const groups = withZero ? [3, 3, 2, 2] : [2, 3, 2, 2];
    const parts = [];
    let pos = 0;
    for (const size of groups) {
      if (pos >= digits.length) break;
      parts.push(digits.slice(pos, pos + size));
      pos += size;
    }
    return parts.join(" ");
  }

  function bindPhone(input) {
    input.addEventListener("input", () => {
      const caret = input.selectionStart ?? input.value.length;
      const digitsBefore = input.value.slice(0, caret).replace(/\D/g, "").length;
      const next = formatPhone(input.value);
      if (input.value === next) return;
      input.value = next;
      let pos = next.length;
      if (digitsBefore === 0) pos = 0;
      else {
        let seen = 0;
        for (let i = 0; i < next.length; i += 1) {
          if (/\d/.test(next[i])) seen += 1;
          if (seen >= digitsBefore) { pos = i + 1; break; }
        }
      }
      input.setSelectionRange(pos, pos);
    });
  }

  function saveEmail(value) {
    try { sessionStorage.setItem(EMAIL_KEY, value); } catch (e) { /* sessionStorage bağlı ola bilər */ }
  }

  function loadEmail() {
    try { return sessionStorage.getItem(EMAIL_KEY) || ""; } catch (e) { return ""; }
  }

  function bindEmailCarry() {
    const email = document.getElementById("email");
    if (!email) return;

    if (!email.value) email.value = loadEmail();

    // Linkə basanda yazılmış email növbəti səhifəyə keçsin (URL-ə yazılmır)
    document.querySelectorAll("[data-carry-email]").forEach((link) => {
      link.addEventListener("click", () => saveEmail(email.value.trim()));
    });
    email.addEventListener("input", () => saveEmail(email.value.trim()));
  }

  // Şifrə sahəsində "göstər/gizlət" göz ikonu (qapaq vurma + üstündən xətt çəkilmə animasiyası)
  const EYE_SVG =
    '<svg viewBox="0 0 24 24" width="20" height="20" fill="none" stroke="currentColor" stroke-width="1.6" stroke-linecap="round" stroke-linejoin="round" aria-hidden="true">' +
    '<path class="eye-shape" d="M2 12s3.5-6 10-6 10 6 10 6-3.5 6-10 6S2 12 2 12z"/>' +
    '<circle class="eye-pupil" cx="12" cy="12" r="3"/>' +
    '<path class="eye-slash" d="M4 4l16 16" pathLength="1"/>' +
    "</svg>";

  function mountPasswordToggles() {
    document.querySelectorAll("[data-password]").forEach((input) => {
      if (input.dataset.ready === "1") return;
      input.dataset.ready = "1";
      const wrap = document.createElement("div");
      wrap.className = "input-wrap";
      input.parentNode.insertBefore(wrap, input);
      wrap.appendChild(input);

      const btn = document.createElement("button");
      btn.type = "button";
      btn.className = "icon-btn pw-eye";
      btn.setAttribute("aria-label", "Show password");
      btn.setAttribute("aria-pressed", "false");
      btn.innerHTML = EYE_SVG;
      wrap.appendChild(btn);

      const svg = btn.querySelector("svg");
      svg.addEventListener("animationend", () => svg.classList.remove("is-blinking"));

      btn.addEventListener("click", () => {
        const show = input.type === "password";
        input.type = show ? "text" : "password";
        btn.classList.toggle("is-on", show);
        btn.setAttribute("aria-label", show ? "Hide password" : "Show password");
        btn.setAttribute("aria-pressed", show ? "true" : "false");
        svg.classList.remove("is-blinking");
        void svg.getBoundingClientRect(); // animasiyanı yenidən başlatmaq üçün
        svg.classList.add("is-blinking");
      });
    });
  }

  function init() {
    const phone = document.getElementById("phone");
    if (phone) bindPhone(phone);
    bindEmailCarry();
    mountPasswordToggles();
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", init);
  else init();
})();
