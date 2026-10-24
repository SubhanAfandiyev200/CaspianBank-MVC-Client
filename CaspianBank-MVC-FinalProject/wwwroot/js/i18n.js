"use strict";

// Səhifə skriptlərinin mətnləri üçün tərcümə: cbt("English text {0}", arg). Lüğət layout-dakı window.I18N-dən gəlir
// (açarlar "js:" ilə başlayır). Tərcümə yoxdursa ingilis mətn göstərilir, ona görə səhifə heç vaxt sınmır.
(function () {
  window.cbt = function (text) {
    const table = window.I18N || {};
    let result = typeof text === "string" && Object.prototype.hasOwnProperty.call(table, "js:" + text) && table["js:" + text] ? table["js:" + text] : text;
    for (let i = 1; i < arguments.length; i++) {
      result = String(result).split("{" + (i - 1) + "}").join(String(arguments[i]));
    }
    return result;
  };
})();
