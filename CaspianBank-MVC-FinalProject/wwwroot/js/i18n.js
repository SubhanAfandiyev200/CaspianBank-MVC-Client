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

  // Tarix: "07 may 2027". Brauzerin az üçün ay adları yoxdur ("M05" qaytarır), ona görə qısa ay adları burada yazılıb
  const months = {
    az: ["yan", "fev", "mar", "apr", "may", "iyn", "iyl", "avq", "sen", "okt", "noy", "dek"],
    ru: ["янв", "фев", "мар", "апр", "мая", "июн", "июл", "авг", "сен", "окт", "ноя", "дек"],
    en: ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"]
  };

  window.cbDate = function (date, withDay) {
    const lang = months[document.documentElement.lang] ? document.documentElement.lang : "en";
    const name = months[lang][date.getMonth()];
    if (withDay === false) return name + " " + date.getFullYear();
    return String(date.getDate()).padStart(2, "0") + " " + name + " " + date.getFullYear();
  };
})();

// Serverdən "07 Nov 2026" kimi ingilis qısa ay adı ilə gələn tarixlər seçilmiş dildə göstərilir (az, ru).
// Yalnız gündən sonra gələn ay adı dəyişir ("5 May", "07 Nov 2026"), başqa mətnə toxunulmur
(function () {
  const lang = document.documentElement.lang;
  const names = {
    az: ["yan", "fev", "mar", "apr", "may", "iyn", "iyl", "avq", "sen", "okt", "noy", "dek"],
    ru: ["янв", "фев", "мар", "апр", "мая", "июн", "июл", "авг", "сен", "окт", "ноя", "дек"]
  }[lang];
  if (!names) return;

  const english = ["Jan", "Feb", "Mar", "Apr", "May", "Jun", "Jul", "Aug", "Sep", "Oct", "Nov", "Dec"];
  const pattern = /(\b\d{1,2})\s(Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b/g;
  const probe = /\b\d{1,2}\s(?:Jan|Feb|Mar|Apr|May|Jun|Jul|Aug|Sep|Oct|Nov|Dec)\b/;

  function run() {
    const walker = document.createTreeWalker(document.body, NodeFilter.SHOW_TEXT, {
      acceptNode: function (node) {
        const parent = node.parentElement;
        if (!parent || /^(SCRIPT|STYLE|TEXTAREA|INPUT)$/.test(parent.tagName)) return NodeFilter.FILTER_REJECT;
        return probe.test(node.nodeValue) ? NodeFilter.FILTER_ACCEPT : NodeFilter.FILTER_SKIP;
      }
    });
    const nodes = [];
    while (walker.nextNode()) nodes.push(walker.currentNode);
    nodes.forEach(function (node) {
      pattern.lastIndex = 0;
      node.nodeValue = node.nodeValue.replace(pattern, function (all, day, month) {
        return day + " " + names[english.indexOf(month)];
      });
    });
  }

  if (document.readyState === "loading") document.addEventListener("DOMContentLoaded", run);
  else run();
})();
