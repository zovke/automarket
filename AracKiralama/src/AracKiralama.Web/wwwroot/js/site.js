// Sitenin genel davranışları. Her sayfada yüklenir.

// ---- Açık / koyu tema ----
// Tercih localStorage'da saklanır; <head> içindeki küçük script sayfa açılırken uygular (yanıp sönmeyi önler).
document.querySelectorAll("[data-theme-toggle]").forEach(btn => {
    btn.addEventListener("click", () => {
        const next = document.documentElement.getAttribute("data-bs-theme") === "dark" ? "light" : "dark";
        document.documentElement.setAttribute("data-bs-theme", next);
        try { localStorage.setItem("theme", next); } catch { /* gizli sekme vb. */ }
        document.dispatchEvent(new CustomEvent("themechange", { detail: next }));
    });
});

// ---- Bildirimler (TempData → _Toast.cshtml) ----
document.querySelectorAll(".toast").forEach(el => bootstrap.Toast.getOrCreateInstance(el, { delay: 5000 }).show());

// ---- Onay isteyen butonlar ----
// Kullanım: <button data-confirm="Emin misiniz?">Sil</button>
document.addEventListener("click", e => {
    const el = e.target.closest("[data-confirm]");
    if (el && !confirm(el.getAttribute("data-confirm"))) {
        e.preventDefault();
        e.stopPropagation();
    }
});

// ---- Değişince otomatik gönderilen filtre formları ----
// Kullanım: <form data-auto-submit> ... </form>
document.querySelectorAll("form[data-auto-submit]").forEach(form => {
    form.addEventListener("change", e => {
        if (e.target.matches("input[type=text], input[type=search]")) return;
        form.requestSubmit();
    });
});

// ---- Tarih alanları: iade tarihi alıştan önce olamasın ----
document.querySelectorAll("[data-date-pair]").forEach(group => {
    const start = group.querySelector("[data-date-start]");
    const end = group.querySelector("[data-date-end]");
    if (!start || !end) return;
    const sync = () => {
        end.min = start.value;
        if (start.value && end.value && end.value <= start.value) {
            const d = new Date(start.value);
            d.setDate(d.getDate() + 3);
            end.value = toLocalInput(d);
            end.dispatchEvent(new Event("change", { bubbles: true }));
        }
    };
    start.addEventListener("change", sync);
    sync();
});

function toLocalInput(d) {
    const pad = n => String(n).padStart(2, "0");
    return `${d.getFullYear()}-${pad(d.getMonth() + 1)}-${pad(d.getDate())}T${pad(d.getHours())}:${pad(d.getMinutes())}`;
}

// Tema butonundaki ikon: koyu temada güneş, açık temada ay
function syncThemeIcon() {
    const dark = document.documentElement.getAttribute("data-bs-theme") === "dark";
    document.querySelectorAll("[data-theme-toggle] i").forEach(i => i.className = dark ? "bi bi-sun" : "bi bi-moon-stars");
}
syncThemeIcon();
document.addEventListener("themechange", syncThemeIcon);
