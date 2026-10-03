(function () {
  const form = document.getElementById("otp-form");
  const root = document.querySelector("[data-otp]");
  const hidden = document.getElementById("otp-code");
  const resend = document.getElementById("resend");
  if (!form || !root) return;

  const inputs = [...root.querySelectorAll("input")];

  function readCode() {
    return inputs.map((input) => input.value.replace(/\D/g, "")).join("");
  }

  function syncHidden() {
    if (hidden) hidden.value = readCode();
  }

  function land(input) {
    input.classList.remove("is-typed");
    if (!input.value) return;
    void input.offsetWidth;
    input.classList.add("is-typed");
  }

  function clearError() {
    if (!root.classList.contains("is-error")) return;
    root.classList.remove("is-error");
    inputs.forEach((input) => input.classList.remove("is-typed"));
  }

  function playError() {
    root.classList.remove("is-error");
    inputs.forEach((input) => input.classList.remove("is-typed"));
    void root.offsetWidth;
    root.classList.add("is-error");
  }

  inputs.forEach((input, index) => {
    input.addEventListener("input", () => {
      input.value = input.value.replace(/\D/g, "").slice(-1);
      clearError();
      land(input);
      syncHidden();
      if (input.value && inputs[index + 1]) inputs[index + 1].focus();
    });
    input.addEventListener("keydown", (event) => {
      if (event.key === "Backspace" && !input.value && inputs[index - 1]) {
        inputs[index - 1].focus();
      }
      if (event.key === "ArrowLeft" && inputs[index - 1]) inputs[index - 1].focus();
      if (event.key === "ArrowRight" && inputs[index + 1]) inputs[index + 1].focus();
    });
    input.addEventListener("paste", (event) => {
      const text = (event.clipboardData.getData("text") || "").replace(/\D/g, "").slice(0, inputs.length);
      if (!text) return;
      event.preventDefault();
      clearError();
      [...text].forEach((ch, i) => {
        if (inputs[i]) inputs[i].value = ch;
      });
      text.split("").forEach((_, i) => {
        window.setTimeout(() => {
          if (inputs[i]) land(inputs[i]);
        }, i * 70);
      });
      syncHidden();
      inputs[Math.min(text.length, inputs.length) - 1].focus();
    });
  });

  form.addEventListener("submit", () => {
    syncHidden();
  });

  if (root.classList.contains("is-error")) playError();

  if (resend) {
    let left = 60;
    let timer = null;

    function paint() {
      if (left <= 0) {
        resend.disabled = false;
        resend.textContent = "Resend code";
        clearInterval(timer);
        timer = null;
        return;
      }
      resend.disabled = true;
      resend.textContent = "Resend code in 0:" + String(left).padStart(2, "0");
    }

    paint();
    timer = setInterval(() => {
      left -= 1;
      paint();
    }, 1000);
  }

  const firstEmpty = inputs.find((input) => !input.value);
  (firstEmpty || inputs[0]).focus();
})();
