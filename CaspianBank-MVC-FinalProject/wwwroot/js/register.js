(function () {
  const flow = CaspianStore.getFlow() || {};
  if (!flow.email) return;

  const form = document.getElementById("register-form");
  const email = document.getElementById("email");
  const phone = document.getElementById("phone");
  const first = document.getElementById("first-name");
  const last = document.getElementById("last-name");
  const dob = document.getElementById("dob");
  const password = document.getElementById("password");
  const confirm = document.getElementById("confirm");

  email.value = flow.email;
  dob.max = new Date().toISOString().slice(0, 10);

  function ageFrom(value) {
    const born = new Date(value + "T00:00:00");
    const now = new Date();
    let age = now.getFullYear() - born.getFullYear();
    const month = now.getMonth() - born.getMonth();
    if (month < 0 || (month === 0 && now.getDate() < born.getDate())) age -= 1;
    return age;
  }

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    BankUI.clearErrors(form);
    let ok = true;
    const phoneValue = BankUI.canonicalPhone(phone.value);

    if (!phoneValue) {
      BankUI.setFieldError(phone, "Enter a valid Azerbaijan mobile number.");
      ok = false;
    }
    if (!first.value.trim()) {
      BankUI.setFieldError(first, "Enter your first name.");
      ok = false;
    }
    if (!last.value.trim()) {
      BankUI.setFieldError(last, "Enter your last name.");
      ok = false;
    }
    if (!dob.value) {
      BankUI.setFieldError(dob, "Enter your date of birth.");
      ok = false;
    } else if (ageFrom(dob.value) < 18) {
      BankUI.setFieldError(dob, "You must be 18 or older to open an account.");
      ok = false;
    }
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
    const profile = {
      firstName: first.value.trim(),
      lastName: last.value.trim(),
      email: flow.email,
      phone: phoneValue,
      dob: dob.value,
      fin: "",
      registered: new Date().toISOString().slice(0, 10)
    };
    // TODO: POST /api/account/register
    await BankUI.request("/api/account/register", {
      method: "POST",
      body: JSON.stringify({ email: profile.email, phone: profile.phone, firstName: profile.firstName, lastName: profile.lastName, dob: profile.dob })
    });
    CaspianStore.establish("new", profile);
    CaspianStore.update((state) => {
      state.session.credential = password.value;
      state.users.unshift({
        id: BankUI.uid("u"),
        name: profile.firstName + " " + profile.lastName,
        email: profile.email,
        phone: profile.phone,
        registered: profile.registered,
        status: "Active",
        sent: 0,
        received: 0,
        spent: 0
      });
    });
    location.href = "app.html";
  });
})();
