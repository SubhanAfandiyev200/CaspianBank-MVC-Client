"use strict";

// Ortaq təsdiq pəncərəsi (SweetAlert tipli): həm müştəri, həm admin səhifələri istifadə edir.
// data-confirm-text olan düymə basılanda əvvəl soruşulur; "Yes" olarsa forma həmin düymə ilə göndərilir.
(function () {
  // data-confirm-text olan düymə basılanda əvvəl "Are you sure?" soruşulur; "Yes" olarsa forma həmin düymə ilə göndərilir
  function confirmDialog(options) {
    return new Promise((resolve) => {
      const wrap = document.createElement("div");
      wrap.className = "swal-backdrop";
      wrap.innerHTML =
        '<div class="swal" role="alertdialog" aria-modal="true" aria-labelledby="swal-title" aria-describedby="swal-text">'
        + '<div class="swal-icon" aria-hidden="true">?</div>'
        + '<h2 id="swal-title"></h2>'
        + '<p id="swal-text"></p>'
        + '<div class="swal-actions">'
        + '<button type="button" class="swal-btn swal-cancel"></button>'
        + '<button type="button" class="swal-btn swal-confirm"></button>'
        + '</div></div>';
      wrap.querySelector("#swal-title").textContent = options.title;
      wrap.querySelector("#swal-text").textContent = options.text;
      const cancel = wrap.querySelector(".swal-cancel");
      const confirm = wrap.querySelector(".swal-confirm");
      cancel.textContent = options.no;
      confirm.textContent = options.yes;
      if (options.danger) confirm.classList.add("swal-danger");
      document.body.appendChild(wrap);
      document.body.classList.add("modal-open");
      confirm.focus();

      function close(result) {
        document.removeEventListener("keydown", onKey);
        wrap.classList.add("is-closing");
        setTimeout(() => {
          wrap.remove();
          if (!document.querySelector(".modal:not([hidden])")) document.body.classList.remove("modal-open");
        }, 160);
        resolve(result);
      }
      function onKey(event) {
        if (event.key === "Escape") close(false);
      }
      document.addEventListener("keydown", onKey);
      cancel.addEventListener("click", () => close(false));
      confirm.addEventListener("click", () => close(true));
      wrap.addEventListener("click", (event) => { if (event.target === wrap) close(false); });
    });
  }

  document.addEventListener("click", async (event) => {
    const button = event.target.closest("[data-confirm-text]");
    if (!button || button.dataset.confirmed === "1") return;
    event.preventDefault();
    const form = button.form;
    const ok = await confirmDialog({
      title: button.getAttribute("data-confirm-title") || "Are you sure?",
      text: button.getAttribute("data-confirm-text") || "",
      yes: button.getAttribute("data-confirm-yes") || "Yes",
      no: button.getAttribute("data-confirm-no") || "Cancel",
      danger: button.hasAttribute("data-confirm-danger")
    });
    if (ok && form) {
      button.dataset.confirmed = "1";
      form.requestSubmit(button);
    }
  });

  // Təsdiq düyməsi iki dəfə basılmasın (server də eyni sorğunu təkrar köçürmür, amma düymə dərhal söndürülür)
  document.querySelectorAll("form").forEach((form) => {
    form.addEventListener("submit", (event) => {
      const submitter = event.submitter;
      if (submitter && submitter.hasAttribute("data-once")) setTimeout(() => { submitter.disabled = true; }, 0);
    });
  });

})();
