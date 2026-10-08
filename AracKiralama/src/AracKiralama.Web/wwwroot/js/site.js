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

// ---- Bilgi balonları (doluluk takvimindeki bloklar vb.) ----
// Kullanım: <a title="..." data-bs-toggle="tooltip">
document.querySelectorAll('[data-bs-toggle="tooltip"]').forEach(el =>
    new bootstrap.Tooltip(el, { customClass: "small", placement: "top" }));

// ---- Giriş sayfasındaki demo hesap butonları ----
// Kullanım: <button data-demo="admin@rota.com"> → e-posta alanını doldurur
document.querySelectorAll("[data-demo]").forEach(b => b.addEventListener("click", () => {
    const email = document.getElementById("Email");
    if (email) email.value = b.dataset.demo;
    document.getElementById("Password")?.focus();
}));

// ---- Görsel yüklemeden önce önizleme ----
// Kullanım: <input type="file" data-image-preview="#preview">
document.querySelectorAll("[data-image-preview]").forEach(input => input.addEventListener("change", () => {
    const file = input.files[0];
    const img = document.querySelector(input.dataset.imagePreview);
    if (file && img) img.src = URL.createObjectURL(file);
}));

// ---- İkon adı yazıldıkça önizleme ----
// Kullanım: <input data-icon-preview="#iconPreview">
document.querySelectorAll("[data-icon-preview]").forEach(input => input.addEventListener("input", () => {
    const icon = document.querySelector(input.dataset.iconPreview);
    if (icon) icon.className = "bi " + input.value;
}));
