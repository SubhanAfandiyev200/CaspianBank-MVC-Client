(function () {
  const ticketId = new URLSearchParams(location.search).get("id") || "";
  if (!ticketId) return;
  document.getElementById("ticket-list")?.setAttribute("hidden", "");
  document.getElementById("ticket-detail")?.removeAttribute("hidden");
  const thread = document.getElementById("ticket-thread");
  const form = document.getElementById("reply-form");

  function render() {
    const ticket = CaspianStore.get().tickets.find((item) => item.id === ticketId);
    if (!ticket) {
      document.getElementById("ticket-subject").textContent = "Ticket not found";
      form.hidden = true;
      thread.innerHTML = `<div class="empty"><h3>No such message</h3><p>It may have been removed.</p></div>`;
      return;
    }
    document.getElementById("ticket-subject").textContent = ticket.subject;
    document.getElementById("ticket-meta").textContent = ticket.name + " · " + ticket.email + " · " + BankUI.formatDateTime(ticket.created);
    thread.innerHTML = `
      <div class="spread"><h2>Original message</h2>${BankUI.badge(ticket.status)}</div>
      <div class="bubble mt-5">
        <p class="meta">${BankUI.esc(ticket.name)} · ${BankUI.formatDateTime(ticket.created)}</p>
        <p>${BankUI.esc(ticket.message)}</p>
      </div>
      ${ticket.reply ? `<div class="bubble bubble-staff mt-5">
        <p class="meta">Caspian Support · ${BankUI.formatDateTime(ticket.repliedAt)}</p>
        <p>${BankUI.esc(ticket.reply)}</p>
      </div>` : ""}`;
  }

  form.addEventListener("submit", async (event) => {
    event.preventDefault();
    const reply = document.getElementById("reply");
    BankUI.setFieldError(reply, "");
    if (reply.value.trim().length < 2) {
      BankUI.setFieldError(reply, "Write a reply before sending.");
      return;
    }
    const button = form.querySelector("button[type=submit]");
    BankUI.setBusy(button, true);
    // TODO: POST /api/admin/tickets/{id}/reply
    await BankUI.request("/api/admin/tickets/" + ticketId + "/reply", {
      method: "POST",
      body: JSON.stringify({ reply: reply.value.trim() })
    });
    CaspianStore.update((state) => {
      const ticket = state.tickets.find((item) => item.id === ticketId);
      if (!ticket) return;
      ticket.reply = reply.value.trim();
      ticket.repliedAt = new Date().toISOString();
      ticket.status = "Answered";
      CaspianStore.logAudit(state, "Support reply", "Replied to “" + ticket.subject + "”");
      if (state.session && state.session.email.toLowerCase() === ticket.email.toLowerCase()) {
        CaspianStore.pushNote(state, {
          type: "support",
          title: "Support replied",
          body: "We answered “" + ticket.subject + "”."
        });
      }
    });
    reply.value = "";
    render();
    BankUI.setBusy(button, false);
    BankUI.toast("Reply sent.");
  });

  render();
})();
