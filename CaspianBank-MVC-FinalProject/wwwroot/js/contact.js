(function () {
  const form = document.getElementById("contact-form");
  const list = document.getElementById("ticket-list");
  const as = document.getElementById("contact-as");

  function render() {
    const state = CaspianStore.get();
    if (!state.session) {
      as.textContent = "Sign in to write to support and see earlier messages.";
      form.querySelector("button[type=submit]").disabled = true;
      list.innerHTML = BankUI.emptyHtml("Sign in required", "Your message history is available after you sign in.", '<a class="btn btn-primary" href="login.html">Sign in</a>');
      return;
    }
    as.textContent = "Sending as " + state.session.name + ".";
    form.querySelector("button[type=submit]").disabled = false;
    const tickets = state.tickets.filter((ticket) => ticket.email.toLowerCase() === state.session.email.toLowerCase());
    list.innerHTML = tickets.length ? tickets.map((ticket) => `
      <article class="thread">
        <div class="spread">
          <h3>${BankUI.esc(ticket.subject)}</h3>
          ${BankUI.badge(ticket.status)}
        </div>
        <div class="bubble">
          <p class="meta">You · ${BankUI.formatDateTime(ticket.created)}</p>
          <p>${BankUI.esc(ticket.message)}</p>
        </div>
        ${ticket.reply ? `<div class="bubble bubble-staff">
          <p class="meta">Caspian Support · ${BankUI.formatDateTime(ticket.repliedAt)}</p>
          <p>${BankUI.esc(ticket.reply)}</p>
        </div>` : ""}
      </article>`).join("") : `<div class="empty"><h3>No messages yet</h3><p>When you write to us, the thread will stay here.</p></div>`;
  }

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const state = CaspianStore.get();
    if (!state.session) return;
    const subject = document.getElementById("subject");
    const message = document.getElementById("message");
    BankUI.clearErrors(form);
    let ok = true;
    if (!subject.value.trim()) { BankUI.setFieldError(subject, "Enter a subject."); ok = false; }
    if (message.value.trim().length < 10) { BankUI.setFieldError(message, "Write a short message, at least 10 characters."); ok = false; }
    if (!ok) return;
    const button = document.getElementById("contact-submit");
    BankUI.setBusy(button, true);
    // TODO: POST /api/support/tickets
    await BankUI.request("/api/support/tickets", {
      method: "POST",
      body: JSON.stringify({ subject: subject.value.trim(), message: message.value.trim() })
    });
    CaspianStore.update((next) => {
      next.tickets.unshift({
        id: BankUI.uid("t"),
        userId: next.users.find((user) => user.email.toLowerCase() === next.profile.email.toLowerCase())?.id || "",
        name: next.session.name,
        email: next.session.email,
        subject: subject.value.trim(),
        message: message.value.trim(),
        status: "Pending",
        created: new Date().toISOString(),
        reply: "",
        repliedAt: null
      });
      CaspianStore.logAudit(next, "Support reply", "Customer opened “" + subject.value.trim() + "”");
    });
    form.reset();
    render();
    BankUI.setBusy(button, false);
    BankUI.toast("Message sent. We'll reply on this page.");
  });

  render();
})();
