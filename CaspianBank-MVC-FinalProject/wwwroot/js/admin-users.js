(function () {
  const body = document.getElementById("user-body");
  const query = document.getElementById("q");
  const status = document.getElementById("status");

  function render() {
    const q = query.value.trim().toLowerCase();
    const rows = CaspianStore.get().users.filter((user) => {
      if (status.value !== "all" && user.status !== status.value) return false;
      if (!q) return true;
      return [user.name, user.email, user.phone].join(" ").toLowerCase().includes(q);
    });
    body.innerHTML = rows.length ? rows.map((user) => `<tr>
      <td data-value="${BankUI.esc(user.name)}">${BankUI.esc(user.name)}</td>
      <td data-value="${BankUI.esc(user.email)}">${BankUI.esc(user.email)}</td>
      <td data-value="${BankUI.esc(user.phone)}">${BankUI.esc(user.phone)}</td>
      <td data-value="${BankUI.esc(user.registered)}">${BankUI.formatDate(user.registered + "T12:00:00")}</td>
      <td data-value="${BankUI.esc(user.status)}">${BankUI.badge(user.status)}</td>
      <td class="actions">
        <a class="btn btn-secondary btn-sm" href="admin-users.html?id=${user.id}">View</a>
        <button type="button" class="btn btn-danger btn-sm" data-freeze="${user.id}">${user.status === "Restricted" ? "Unfreeze" : "Restrict"}</button>
      </td>
    </tr>`).join("") : `<tr class="empty-row"><td colspan="6"><div class="empty"><h3>No users</h3><p>Nothing matches this search.</p></div></td></tr>`;
  }

  body.addEventListener("click", async (event) => {
    return;
    const button = event.target.closest("[data-freeze]");
    if (!button) return;
    const id = button.getAttribute("data-freeze");
    const user = CaspianStore.get().users.find((item) => item.id === id);
    if (!user) return;
    const restrict = user.status !== "Restricted";
    const yes = await BankUI.confirm({
      title: restrict ? "Restrict this account?" : "Restore this account?",
      message: restrict
        ? user.name + " will be unable to move money until the account is restored."
        : user.name + " will be able to transact again.",
      confirmText: restrict ? "Restrict account" : "Unfreeze",
      danger: restrict
    });
    if (!yes) return;
    // TODO: POST /api/admin/users/{id}/status
    await BankUI.request("/api/admin/users/" + id + "/status", { method: "POST", body: JSON.stringify({ status: restrict ? "Restricted" : "Active" }) });
    CaspianStore.update((state) => {
      const target = state.users.find((item) => item.id === id);
      target.status = restrict ? "Restricted" : "Active";
      CaspianStore.logAudit(state, restrict ? "Account restricted" : "Account restored", target.name);
    });
    render();
    BankUI.toast(restrict ? "Account restricted." : "Account restored.");
  });

  query.addEventListener("input", render);
  status.addEventListener("change", render);
  render();
  BankUI.enhanceTable(document.getElementById("user-table"));
})();
