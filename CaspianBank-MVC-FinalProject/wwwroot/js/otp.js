(function () {
  const flow = CaspianStore.getFlow() || {};
  if (!flow.email) return;

  document.getElementById("otp-lead").textContent = "We sent a code to " + BankUI.maskEmail(flow.email) + ".";
  const resend = document.getElementById("resend");
  let left = 45;
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

  function startCountdown() {
    clearInterval(timer);
    left = 45;
    paint();
    timer = setInterval(() => {
      left -= 1;
      paint();
    }, 1000);
  }

  startCountdown();

  resend.addEventListener("click", async () => {
    // TODO: POST /api/account/resend-otp  { email }
    await BankUI.request("/api/account/resend-otp", {
      method: "POST",
      body: JSON.stringify({ email: flow.email })
    });
    BankUI.toast("A new code is on its way.");
    startCountdown();
  });

  document.getElementById("otp-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    const code = BankUI.readOtp(document.querySelector("[data-otp]"));
    if (code.length !== 6) {
      BankUI.toast("Enter the 6-digit code.", "error");
      return;
    }
    const button = document.getElementById("otp-submit");
    BankUI.setBusy(button, true);
    // TODO: POST /api/account/verify-otp  { email, code }
    await BankUI.request("/api/account/verify-otp", {
      method: "POST",
      body: JSON.stringify({ email: flow.email, code })
    });
    location.href = "register.html";
  });
})();
