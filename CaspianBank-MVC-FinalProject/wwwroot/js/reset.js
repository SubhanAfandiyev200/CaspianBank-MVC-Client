(function () {
  const params = new URLSearchParams(location.search);
  const token = params.get("token") || "";
  const email = params.get("email") || CaspianStore.getFlow()?.email || "";
  const resetStep = document.getElementById("reset-step");
  if (!token || !document.getElementById("reset-form")) return;
  if (resetStep) resetStep.hidden = false;
  const forgotStep = document.getElementById("forgot-step");
  const forgotSent = document.getElementById("forgot-sent");
  if (forgotStep) forgotStep.hidden = true;
  if (forgotSent) forgotSent.hidden = true;
  if (email) {
    document.getElementById("reset-lead").textContent = "Set a new password for " + BankUI.maskEmail(email) + ".";
  }

  const form = document.getElementById("reset-form");
  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    BankUI.clearErrors(form);
    const password = document.getElementById("password");
    const confirm = document.getElementById("confirm");
    let ok = true;
    if (!/^(?=.*[A-Za-z])(?=.*\d).{8,}$/.test(password.value)) {
      BankUI.setFieldError(password, "Use at least 8 characters, with a letter and a number.");
      ok = false;
    }
    if (password.value !== confirm.value) {
      BankUI.setFieldError(confirm, "Passwords do not match.");
      ok = false;
    }
    if (!ok) return;
    const button = form.querySelector("button[type=submit]");
    BankUI.setBusy(button, true);
    // TODO: POST /api/account/reset-password  { token, email, password }
    await BankUI.request("/api/account/reset-password", {
      method: "POST",
      body: JSON.stringify({ token, email })
    });
    const state = CaspianStore.get();
    if (state.session && (!email || state.session.email.toLowerCase() === email.toLowerCase())) {
      CaspianStore.update((s) => { s.session.credential = password.value; });
    } else if (state.profile && email && state.profile.email.toLowerCase() === email.toLowerCase()) {
      CaspianStore.update((s) => {
        if (!s.session) {
          s.session = {
            email: s.profile.email,
            name: s.profile.firstName + " " + s.profile.lastName,
            mode: s.profile.email.toLowerCase() === CaspianDemo.knownEmail.toLowerCase() ? "existing" : "new"
          };
        }
        s.session.credential = password.value;
      });
    }
    BankUI.toast("Your password has been updated.");
    location.href = "login.html";
  });
})();
