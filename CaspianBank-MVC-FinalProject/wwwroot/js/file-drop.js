// Şəkil seçmə sahəsi: [data-file-drop] içindəki input üçün önizləmə, fayl adı/ölçüsü və sürüklə-burax
(function () {
  "use strict";

  var MAX_BYTES = 2 * 1024 * 1024;
  var TYPES = ["image/png", "image/jpeg", "image/webp"];

  document.querySelectorAll("[data-file-drop]").forEach(function (zone) {
    var input = zone.querySelector("input[type=file]");
    var preview = zone.querySelector("[data-file-preview]");
    var title = zone.querySelector("[data-file-title]");
    var hint = zone.querySelector("[data-file-hint]");
    if (!input || !preview || !title || !hint) return;

    var startPreview = preview.innerHTML;
    var startTitle = title.textContent;
    var startHint = hint.textContent;

    function reset(message) {
      input.value = "";
      preview.innerHTML = startPreview;
      title.textContent = startTitle;
      hint.textContent = message || startHint;
      zone.classList.toggle("is-error", !!message);
      zone.classList.remove("has-file");
    }

    function show(file) {
      if (TYPES.indexOf(file.type) === -1) {
        reset("Only PNG, JPEG or WebP images are allowed.");
        return;
      }
      if (file.size > MAX_BYTES) {
        reset("The image must be up to 2 MB.");
        return;
      }

      var image = document.createElement("img");
      image.alt = "";
      image.src = URL.createObjectURL(file);
      preview.innerHTML = "";
      preview.appendChild(image);

      title.textContent = file.name;
      hint.textContent = Math.max(1, Math.round(file.size / 1024)) + " KB · click to change";
      zone.classList.remove("is-error");
      zone.classList.add("has-file");
    }

    input.addEventListener("change", function () {
      if (input.files && input.files[0]) show(input.files[0]);
      else reset();
    });

    ["dragenter", "dragover"].forEach(function (name) {
      zone.addEventListener(name, function (event) {
        event.preventDefault();
        zone.classList.add("is-over");
      });
    });
    ["dragleave", "drop"].forEach(function (name) {
      zone.addEventListener(name, function () {
        zone.classList.remove("is-over");
      });
    });
    zone.addEventListener("drop", function (event) {
      event.preventDefault();
      if (event.dataTransfer && event.dataTransfer.files && event.dataTransfer.files.length) {
        input.files = event.dataTransfer.files;
        input.dispatchEvent(new Event("change"));
      }
    });
  });
})();
