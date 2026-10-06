(function () {
  const flow = CaspianStore.getFlow();
  const emailInput = document.getElementById("email");
  if (flow?.email) emailInput.value = flow.email;

  document.getElementById("forgot-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    const email = emailInput.value.trim();
    BankUI.setFieldError(emailInput, "");
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      BankUI.setFieldError(emailInput, "Enter a valid email address.");
      return;
    }
    const button = event.target.querySelector("button[type=submit]");
    BankUI.setBusy(button, true);
    // TODO: POST /api/account/forgot-password  { email }
    await BankUI.request("/api/account/forgot-password", {
      method: "POST",
      body: JSON.stringify({ email })
    });
    const token = "reset-" + Math.random().toString(36).slice(2, 10);
    document.getElementById("forgot-sent-copy").textContent = "If an account exists for " + BankUI.maskEmail(email) + ", a reset link is on its way. It expires in 30 minutes.";
    document.getElementById("forgot-continue").href = "forgot-password.html?token=" + encodeURIComponent(token) + "&email=" + encodeURIComponent(email);
    document.getElementById("forgot-step").hidden = true;
    document.getElementById("forgot-sent").hidden = false;
  });
})();
