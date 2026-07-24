(function () {
    "use strict";
    const root = document.getElementById("operations-dashboard");
    if (!root) return;
    let shippingChart;
    const currency = new Intl.NumberFormat(undefined, { minimumFractionDigits: 2, maximumFractionDigits: 2 });
    const emptyRow = (columns) => `<tr><td colspan="${columns}" class="text-center text-muted">無資料</td></tr>`;
    const text = (value) => String(value ?? "").replace(/[&<>"']/g, character => ({ "&": "&amp;", "<": "&lt;", ">": "&gt;", '"': "&quot;", "'": "&#39;" })[character]);
    function render(data) {
        ["todayOrderCount", "monthlyOrderCount"].forEach(key => root.querySelector(`[data-dashboard="${key}"]`).textContent = data[key] ?? 0);
        ["todayRevenue", "monthlyRevenue"].forEach(key => root.querySelector(`[data-dashboard="${key}"]`).textContent = currency.format(data[key] ?? 0));
        const lowStock = root.querySelector('[data-dashboard-table="lowStockProducts"]');
        lowStock.innerHTML = data.lowStockProducts?.length ? data.lowStockProducts.map(x => `<tr><td>${text(x.productName)}</td><td>${x.unitsInStock}</td><td>${x.reorderLevel}</td></tr>`).join("") : emptyRow(3);
        const customers = root.querySelector('[data-dashboard-table="topCustomers"]');
        customers.innerHTML = data.topCustomers?.length ? data.topCustomers.map(x => `<tr><td>${text(x.companyName)}</td><td>${x.orderCount}</td><td>${currency.format(x.revenue)}</td></tr>`).join("") : emptyRow(3);
        if (shippingChart) shippingChart.destroy();
        shippingChart = new Chart(document.getElementById("shipping-status-chart"), { type: "doughnut", data: { labels: (data.shippingStatuses || []).map(x => ({ 0: "已出貨", 1: "待出貨", 2: "已逾期" })[x.status]), datasets: [{ data: (data.shippingStatuses || []).map(x => x.count), backgroundColor: ["#198754", "#0d6efd", "#dc3545"] }] }, options: { responsive: true, maintainAspectRatio: false } });
    }
    async function refresh() {
        try { const response = await fetch(root.dataset.dashboardUrl, { credentials: "same-origin", headers: { Accept: "application/json" } }); if (!response.ok) throw new Error("Dashboard refresh failed"); render(await response.json()); }
        catch { toastr.error("儀表板資料更新失敗，請稍後再試。"); }
    }
    render(JSON.parse(document.getElementById("operations-dashboard-data").textContent));
    window.setInterval(refresh, 600000);
})();
