// Yönetim paneli grafikleri (Chart.js).
// Veriler sayfadaki <script type="application/json" id="chart-data"> bloğundan okunur.
// Bu blok çalıştırılmadığı için güvenlik politikasına (CSP) takılmaz.
//   Dashboard: { monthly: [{Label, Value}], categories: [{Label, Value}] }   → Services/ReportService.cs
//   Raporlar:  { branches: [{Branch, RentalCount, Revenue}] }
(function () {
    const el = document.getElementById("chart-data");
    if (!el || typeof Chart === "undefined") return;
    const data = JSON.parse(el.textContent);

    const css = name => getComputedStyle(document.documentElement).getPropertyValue(name).trim();
    const money = v => new Intl.NumberFormat("tr-TR").format(v) + " ₺";
    const thousands = v => (v / 1000) + "K";
    let charts = [];

    function draw() {
        charts.forEach(c => c.destroy());
        charts = [];
        const brand = css("--brand"), grid = css("--border");
        Chart.defaults.font.family = "Inter, system-ui, sans-serif";
        Chart.defaults.color = css("--muted");

        const revenue = document.getElementById("revenueChart");
        if (revenue && data.monthly) {
            charts.push(new Chart(revenue, {
                type: "bar",
                data: {
                    labels: data.monthly.map(x => x.Label),
                    datasets: [{ data: data.monthly.map(x => x.Value), backgroundColor: brand, borderRadius: 6, maxBarThickness: 36 }]
                },
                options: {
                    maintainAspectRatio: false,
                    plugins: { legend: { display: false }, tooltip: { callbacks: { label: c => money(c.raw) } } },
                    scales: {
                        x: { grid: { display: false } },
                        y: { grid: { color: grid }, border: { display: false }, ticks: { callback: thousands } }
                    }
                }
            }));
        }

        const category = document.getElementById("categoryChart");
        if (category && data.categories) {
            charts.push(new Chart(category, {
                type: "doughnut",
                data: {
                    labels: data.categories.map(x => x.Label),
                    datasets: [{ data: data.categories.map(x => x.Value), backgroundColor: [brand, "#0891b2", "#f59e0b", "#8b5cf6", "#94a3b8"], borderWidth: 0 }]
                },
                options: { maintainAspectRatio: false, cutout: "68%", plugins: { legend: { position: "bottom", labels: { usePointStyle: true, padding: 14 } } } }
            }));
        }

        const branch = document.getElementById("branchChart");
        if (branch && data.branches) {
            charts.push(new Chart(branch, {
                type: "bar",
                data: {
                    labels: data.branches.map(b => b.Branch),
                    datasets: [{ data: data.branches.map(b => b.Revenue), backgroundColor: brand, borderRadius: 6 }]
                },
                options: {
                    indexAxis: "y", maintainAspectRatio: false,
                    plugins: { legend: { display: false }, tooltip: { callbacks: { label: c => money(c.raw) } } },
                    scales: { x: { grid: { color: grid }, ticks: { callback: thousands } }, y: { grid: { display: false } } }
                }
            }));
        }
    }

    draw();
    document.addEventListener("themechange", draw);   // tema değişince renkleri yenile
})();
