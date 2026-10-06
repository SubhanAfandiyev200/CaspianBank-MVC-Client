/* OTP səhifəsi: 6 xanalı kod sahəsi və "yenidən göndər" geri sayımı */
(function () {
  const root = document.querySelector("[data-otp]");
  const form = document.getElementById("otp-form");
  if (!root || !form) return;

  const inputs = [...root.querySelectorAll("input")];
  const hidden = document.getElementById("otp-code");

  const land = (input) => {
    input.classList.remove("is-typed");
    if (!input.value) return;
    void input.offsetWidth;
    input.classList.add("is-typed");
  };

  const clearError = () => {
    root.classList.remove("is-invalid", "is-shake");
    inputs.forEach((el) => el.removeAttribute("aria-invalid"));
  };

  const shakeError = () => {
    root.classList.add("is-invalid");
    inputs.forEach((el) => {
      el.classList.remove("is-typed");
      el.setAttribute("aria-invalid", "true");
    });
    root.classList.remove("is-shake");
    void root.offsetWidth;
    root.classList.add("is-shake");
    window.setTimeout(() => root.classList.remove("is-shake"), 850);
  };

  const existing = (hidden?.value || "").replace(/\D/g, "").slice(0, inputs.length);
  if (existing) {
    [...existing].forEach((ch, i) => { if (inputs[i]) inputs[i].value = ch; });
  }

  if (root.classList.contains("is-invalid") || document.querySelector(".validation-summary-errors")) {
    window.requestAnimationFrame(shakeError);
  }

  inputs.forEach((input, index) => {
    input.addEventListener("input", () => {
      if (root.classList.contains("is-invalid")) clearError();
      input.value = input.value.replace(/\D/g, "").slice(-1);
      land(input);
      if (input.value && inputs[index + 1]) inputs[index + 1].focus();
    });
    input.addEventListener("keydown", (event) => {
      if (event.key === "Backspace" && !input.value && inputs[index - 1]) inputs[index - 1].focus();
      if (event.key === "ArrowLeft" && inputs[index - 1]) inputs[index - 1].focus();
      if (event.key === "ArrowRight" && inputs[index + 1]) inputs[index + 1].focus();
    });
    input.addEventListener("paste", (event) => {
      const text = (event.clipboardData.getData("text") || "").replace(/\D/g, "").slice(0, inputs.length);
      if (!text) return;
      event.preventDefault();
      if (root.classList.contains("is-invalid")) clearError();
      [...text].forEach((ch, i) => { if (inputs[i]) inputs[i].value = ch; });
      text.split("").forEach((_, i) => {
        window.setTimeout(() => { if (inputs[i]) land(inputs[i]); }, i * 70);
      });
      inputs[Math.min(text.length, inputs.length) - 1].focus();
    });
  });

  // Göndərməzdən əvvəl xanalar birləşib gizli sahəyə yazılır
  form.addEventListener("submit", (event) => {
    hidden.value = inputs.map((input) => input.value).join("");
    if (!/^\d{6}$/.test(hidden.value)) {
      event.preventDefault();
      shakeError();
    }
  });

  inputs[0].focus();

  // "Resend code" düyməsi 60 saniyə gözləyir (serverdəki gözləmə ilə eyni)
  const resend = document.getElementById("resend");
  if (resend) {
    let left = Number(resend.dataset.wait || 60);
    const paint = () => {
      if (left <= 0) {
        resend.disabled = false;
        resend.textContent = "Resend code";
        return;
      }
      resend.disabled = true;
      resend.textContent = "Resend code in 0:" + String(left).padStart(2, "0");
    };
    paint();
    const timer = setInterval(() => {
      left -= 1;
      paint();
      if (left <= 0) clearInterval(timer);
    }, 1000);
  }
})();
