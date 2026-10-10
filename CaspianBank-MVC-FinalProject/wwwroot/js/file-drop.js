// Fayl seçmə sahəsi: [data-file-drop] içindəki input üçün önizləmə, fayl adı/ölçüsü və sürüklə-burax.
// Şəkil sahələri üçün heç nə yazmaq lazım deyil (PNG, JPEG, WebP, 2 MB). Video sahəsi üçün zona üzərində:
// data-kind="video" data-types="video/mp4,video/webm" data-max-bytes="52428800" data-type-error="..." data-size-error="..."
(function () {
  "use strict";

  var IMAGE_TYPES = ["image/png", "image/jpeg", "image/webp"];
  var IMAGE_MAX_BYTES = 2 * 1024 * 1024;

  function formatSize(bytes) {
    if (bytes >= 1024 * 1024) return (bytes / (1024 * 1024)).toFixed(1) + " MB";
    return Math.max(1, Math.round(bytes / 1024)) + " KB";
  }

  document.querySelectorAll("[data-file-drop]").forEach(function (zone) {
    var input = zone.querySelector("input[type=file]");
    var preview = zone.querySelector("[data-file-preview]");
    var title = zone.querySelector("[data-file-title]");
    var hint = zone.querySelector("[data-file-hint]");
    if (!input || !preview || !title || !hint) return;

    var isVideo = zone.getAttribute("data-kind") === "video";
    var types = zone.hasAttribute("data-types") ? zone.getAttribute("data-types").split(",") : IMAGE_TYPES;
    var maxBytes = zone.hasAttribute("data-max-bytes") ? Number(zone.getAttribute("data-max-bytes")) : IMAGE_MAX_BYTES;
    var typeError = zone.getAttribute("data-type-error") || "Only PNG, JPEG or WebP images are allowed.";
    var sizeError = zone.getAttribute("data-size-error") || "The image must be up to 2 MB.";

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
      if (types.indexOf(file.type) === -1) {
        reset(typeError);
        return;
      }
      if (file.size > maxBytes) {
        reset(sizeError);
        return;
      }

      // Şəkil üçün şəkil, video üçün ilk kadr (video yüklənmir, yalnız metadata oxunur)
      var media = document.createElement(isVideo ? "video" : "img");
      if (isVideo) {
        media.muted = true;
        media.preload = "metadata";
        media.src = URL.createObjectURL(file) + "#t=0.5";
      } else {
        media.alt = "";
        media.src = URL.createObjectURL(file);
      }
      preview.innerHTML = "";
      preview.appendChild(media);

      title.textContent = file.name;
      hint.textContent = formatSize(file.size) + " · click to change";
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
