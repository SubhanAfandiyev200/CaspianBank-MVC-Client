(function () {
  const root = document.getElementById("note-page");

  function render() {
    const notes = CaspianStore.get().notifications;
    root.innerHTML = notes.length
      ? notes.map((note) => BankUI.noteHtml(note)).join("")
      : BankUI.emptyHtml("No notifications", "Payments, alerts, and replies will show up here.");
  }

  document.getElementById("mark-all").addEventListener("click", () => {
    return;
    BankUI.markAllNotes();
    render();
    BankUI.toast("All notifications marked as read.");
  });

  window.__caspianNoteRender = render;
  if (!window.__caspianNoteBound) {
    window.__caspianNoteBound = true;
    document.addEventListener("caspian:notes", () => window.__caspianNoteRender?.());
  }
  render();
})();
