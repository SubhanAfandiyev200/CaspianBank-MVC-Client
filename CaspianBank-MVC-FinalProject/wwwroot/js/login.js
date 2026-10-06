(function () {
  const flow = CaspianStore.getFlow();
  const state = CaspianStore.get();
  const emailInput = document.getElementById("email");
  const password = document.getElementById("password");
  emailInput.value = flow?.email || state.profile?.email || CaspianDemo.knownEmail;

  document.getElementById("login-form").addEventListener("submit", async (event) => {
    event.preventDefault();
    const form = event.target;
    BankUI.clearErrors(form);
    const email = emailInput.value.trim();
    let ok = true;
    if (!/^[^\s@]+@[^\s@]+\.[^\s@]+$/.test(email)) {
      BankUI.setFieldError(emailInput, "Enter a valid email address.");
      ok = false;
    }
    if (!/^(?=.*[A-Za-z])(?=.*\d).{8,}$/.test(password.value)) {
      BankUI.setFieldError(password, "Password must be at least 8 characters and include a letter and a number.");
      ok = false;
    }
    if (!ok) return;

    const button = form.querySelector("button[type=submit]");
    BankUI.setBusy(button, true);
    // TODO: POST /api/account/login  { email, password }
    await BankUI.request("/api/account/login", {
      method: "POST",
      body: JSON.stringify({ email })
    });

    const current = CaspianStore.get();
    const isLeyla = email.toLowerCase() === CaspianDemo.knownEmail.toLowerCase();
    const isReturning = current.profile
      && current.profile.email.toLowerCase() === email.toLowerCase()
      && !isLeyla;

    if (!isLeyla && !isReturning) {
      BankUI.setBusy(button, false);
      BankUI.setFieldError(password, "We couldn't sign in with those details.");
      return;
    }

    if (isLeyla) CaspianStore.establish("existing");
    else {
      CaspianStore.update((s) => {
        s.session = {
          email: s.profile.email,
          name: s.profile.firstName + " " + s.profile.lastName,
          mode: "new"
        };
        CaspianStore.logAudit(s, "Sign-in", "Web banking session opened");
      });
    }
    CaspianStore.update((s) => { s.session.credential = password.value; });
    location.href = "app.html";
  });
})();
