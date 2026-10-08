// 3 adımlı rezervasyon sihirbazı (Views/Reservations/Create.cshtml).
// Aynı form içindeki [data-step] bölümlerini sırayla gösterir; [data-step-go] butonları adım değiştirir.
(function () {
    const form = document.getElementById("wizard");
    if (!form) return;

    const panes = form.querySelectorAll("[data-step]");
    const indicators = document.querySelectorAll("[data-step-indicator]");

    function go(step) {
        panes.forEach(p => p.classList.toggle("active", Number(p.dataset.step) === step));
        indicators.forEach(i => {
            const n = Number(i.dataset.stepIndicator);
            i.classList.toggle("active", n === step);
            i.classList.toggle("done", n < step);
        });
        window.scrollTo({ top: 0, behavior: "smooth" });
    }

    form.querySelectorAll("[data-step-go]").forEach(b => b.addEventListener("click", () => go(Number(b.dataset.stepGo))));
    go(Number(form.dataset.startStep || 1));
})();
