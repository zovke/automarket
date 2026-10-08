// Canlı fiyat hesaplayıcı.
// Formdaki tarih / şube / ekstra alanları değiştikçe /api/quote uç noktasına istek atar
// ve sonucu kalem kalem gösterir. Hesabın kendisi sunucuda (Services/PricingService.cs) yapılır;
// burada sadece sonuç ekrana yazılır → fiyat kuralı tek bir yerde kalır.
//
// Kullanım (HTML):
//   <form data-quote-form data-vehicle-id="5">
//     <input name="Start"> <input name="End"> <select name="PickupBranchId"> <select name="ReturnBranchId">
//     <input type="checkbox" name="ExtraIds" value="1"> ...
//     <ul data-quote-breakdown></ul>   <div data-quote-errors></div>   <button data-quote-submit>
//   </form>

(function () {
    const form = document.querySelector("[data-quote-form]");
    if (!form) return;

    const breakdown = form.querySelector("[data-quote-breakdown]");
    const errors = form.querySelector("[data-quote-errors]");
    const submit = form.querySelector("[data-quote-submit]");
    const totals = form.querySelectorAll("[data-quote-total]");
    let timer, lastRequest = 0;

    const value = name => form.querySelector(`[name="${name}"]`)?.value;

    async function refresh() {
        const body = {
            vehicleId: Number(form.dataset.vehicleId),
            start: value("Start"),
            end: value("End"),
            pickupBranchId: Number(value("PickupBranchId")),
            returnBranchId: Number(value("ReturnBranchId")),
            extraIds: [...form.querySelectorAll('[name="ExtraIds"]:checked')].map(x => Number(x.value)),
        };
        if (!body.start || !body.end) return;

        const requestId = ++lastRequest;
        breakdown?.classList.add("quote-loading");
        try {
            const res = await fetch("/api/quote", {
                method: "POST",
                headers: { "Content-Type": "application/json" },
                body: JSON.stringify(body),
            });
            const data = await res.json();
            if (requestId !== lastRequest) return;   // daha yeni bir istek varsa eskisini yok say
            render(data);
        } catch {
            showErrors(["Fiyat hesaplanamadı. Bağlantınızı kontrol edin."]);
        } finally {
            breakdown?.classList.remove("quote-loading");
        }
    }

    function render(data) {
        showErrors(data.errors || []);
        if (submit) submit.disabled = !data.isAvailable || submit.dataset.locked === "true";
        if (!data.quote) return;

        const q = data.quote, t = data.text;
        totals.forEach(el => el.textContent = t.total);

        if (!breakdown) return;
        const rows = [[`${q.days} gün × ${money(q.dailyPrice)}`, t.baseTotal]];
        if (q.discountAmount > 0) rows.push([`Uzun dönem indirimi (%${Math.round(q.discountRate * 100)})`, "− " + t.discount, "discount"]);
        q.extraLines.forEach(x => rows.push([x.name, money(x.total)]));
        if (q.oneWayFee > 0) rows.push(["Farklı şubeye iade", t.oneWayFee]);
        rows.push(["Toplam", t.total, "total"]);

        breakdown.replaceChildren(...rows.map(([label, amount, cls]) => {
            const li = document.createElement("li");
            if (cls) li.className = cls;
            li.innerHTML = `<span></span><span class="tabular"></span>`;
            li.children[0].textContent = label;
            li.children[1].textContent = amount;
            return li;
        }));

        const deposit = form.querySelector("[data-quote-deposit]");
        if (deposit) deposit.textContent = t.deposit;
    }

    function showErrors(list) {
        if (!errors) return;
        errors.replaceChildren(...list.map(msg => {
            const div = document.createElement("div");
            div.className = "alert alert-warning py-2 px-3 small mb-2";
            div.textContent = msg;
            return div;
        }));
    }

    const money = n => new Intl.NumberFormat("tr-TR", { maximumFractionDigits: 0 }).format(n) + " ₺";

    form.addEventListener("change", () => { clearTimeout(timer); timer = setTimeout(refresh, 150); });
    refresh();
})();
