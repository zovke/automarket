// Kayıtlı tema tercihini (yoksa işletim sisteminin tercihini) sayfa çizilmeden önce uygular.
// <head> içinde, defer'siz yüklenir. Tema butonu: site.js
(function () {
    let theme = null;
    try { theme = localStorage.getItem("theme"); } catch { /* gizli sekme vb. */ }
    if (!theme) theme = matchMedia("(prefers-color-scheme: dark)").matches ? "dark" : "light";
    document.documentElement.setAttribute("data-bs-theme", theme);
})();
