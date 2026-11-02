"use strict";

// Açıq yazışma səhifəsi (müştəri: /App/Support/Ticket/{id}, işçi: /Admin/Tickets/Detail/{id}) canlı yenilənir:
// bu yazışma haqqında bildiriş SignalR ilə gələn kimi söhbət hissəsi səhifəni yeniləmədən dəyişir.
(function () {
  const log = document.querySelector(".chat-log");
  if (!log) return;

  let refreshing = false;

  document.addEventListener("caspian:notification", async (event) => {
    const link = String((event.detail && event.detail.linkUrl) || "").toLowerCase();
    if (!link || link !== location.pathname.toLowerCase() || refreshing) return;

    refreshing = true;
    try {
      const response = await fetch(location.href, { credentials: "same-origin", cache: "no-store" });
      if (!response.ok) return;
      const html = await response.text();
      const fresh = new DOMParser().parseFromString(html, "text/html").querySelector(".chat-log");
      if (fresh) {
        log.innerHTML = fresh.innerHTML;
        window.scrollTo({ top: document.body.scrollHeight, behavior: "smooth" });
      }
    } catch (error) { /* növbəti bildirişdə yenidən cəhd olunur */ } finally {
      refreshing = false;
    }
  });
})();

// Yazışma səhifəsindəki cavab sahəsində Enter göndərir, Shift+Enter yeni sətir yazır (söhbət pəncərəsi kimi)
(function () {
  document.querySelectorAll("textarea[data-enter-submit]").forEach((area) => {
    area.addEventListener("keydown", (event) => {
      if (event.key !== "Enter" || event.shiftKey || event.isComposing) return;
      event.preventDefault();
      if (area.value.trim() === "") return;
      const form = area.closest("form");
      if (form) {
        const button = form.querySelector("[type=submit]");
        if (form.requestSubmit) form.requestSubmit(button); else form.submit();
      }
    });
  });
})();
