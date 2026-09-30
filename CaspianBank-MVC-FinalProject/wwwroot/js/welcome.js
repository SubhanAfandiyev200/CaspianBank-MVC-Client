(function () {
  const form = document.getElementById("welcome-form");
  const emailInput = document.getElementById("email");

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const email = emailInput.value.trim();
    BankUI.setFieldError(emailInput, "");
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      BankUI.setFieldError(emailInput, "Enter a valid email address.");
      return;
    }
    const button = form.querySelector("button[type=submit]");
    BankUI.setBusy(button, true);
    // TODO: POST /api/account/identify  { email }  -> { exists: boolean }
    await BankUI.request("/api/account/identify", {
      method: "POST",
      body: JSON.stringify({ email })
    });
    CaspianStore.setFlow({ email });
    const state = CaspianStore.get();
    const known = email.toLowerCase() === CaspianDemo.knownEmail.toLowerCase()
      || email.toLowerCase() === (state.profile?.email || "").toLowerCase();
    location.href = known ? "login.html" : "otp.html";
  });
})();
