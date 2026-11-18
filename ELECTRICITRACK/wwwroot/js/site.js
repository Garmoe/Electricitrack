(() => {
    const money = value => new Intl.NumberFormat("en-PH", {
        minimumFractionDigits: 2,
        maximumFractionDigits: 2
    }).format(value);
    const decimal = value => new Intl.NumberFormat("en-PH", {
        minimumFractionDigits: 1,
        maximumFractionDigits: 1
    }).format(value);

    const applianceForm = document.querySelector("#appliance-form");
    if (applianceForm) {
        const power = applianceForm.querySelector('[name="PowerWatts"]');
        const hours = applianceForm.querySelector('[name="HoursPerDay"]');
        const days = applianceForm.querySelector('[name="DaysPerMonth"]');
        const rate = Number(window.electricitrackRate || 0);
        const kwhOutput = applianceForm.querySelector("[data-monthly-kwh]");
        const costOutput = applianceForm.querySelector("[data-monthly-cost]");
        const updateEstimate = () => {
            const monthlyKwh = Math.max(0, Number(power.value || 0) / 1000 *
                Number(hours.value || 0) * Number(days.value || 0));
            kwhOutput.textContent = decimal(monthlyKwh);
            costOutput.textContent = money(monthlyKwh * rate);
        };
        applianceForm.querySelectorAll("[data-estimate-input]").forEach(input =>
            input.addEventListener("input", updateEstimate));
        updateEstimate();
    }

    const simulation = document.querySelector("#simulation-form");
    if (!simulation) return;

    const rate = Number(document.querySelector("#simulation-rate").value);
    const budget = Number(document.querySelector("#simulation-budget").value);
    const rows = [...simulation.querySelectorAll("[data-adjustment-row]")];
    const guidance = document.querySelector("[data-budget-guidance]");
    const guidanceId = Number(guidance.dataset.guidanceId || 0);
    const currentKwh = rows.reduce((total, row) =>
        total + Number(row.dataset.watts) / 1000 *
        Number(row.dataset.currentHours) * Number(row.dataset.days), 0);
    const currentCost = currentKwh * rate;

    const updateSimulation = () => {
        let adjustedKwh = 0;
        let changedCount = 0;
        const rowValues = rows.map(row => {
            const input = row.querySelector(".hours-input");
            let hours = Number(input.value);
            if (!Number.isFinite(hours)) hours = 0;
            hours = Math.min(24, Math.max(0, hours));
            input.value = hours.toString();
            const kwh = Number(row.dataset.watts) / 1000 * hours * Number(row.dataset.days);
            const initialKwh = Number(row.dataset.watts) / 1000 *
                Number(row.dataset.currentHours) * Number(row.dataset.days);
            adjustedKwh += kwh;
            if (hours !== Number(row.dataset.currentHours)) changedCount++;
            const saving = (initialKwh - kwh) * rate;
            const savingOutput = row.querySelector("[data-row-saving]");
            savingOutput.textContent = saving > .005 ? `₱${money(saving)}` : "—";
            savingOutput.classList.toggle("accent-text", saving > .005);
            savingOutput.classList.toggle("muted", saving <= .005);
            return { row, kwh };
        });

        const adjustedCost = adjustedKwh * rate;
        const savedCost = Math.max(0, currentCost - adjustedCost);
        const savedKwh = Math.max(0, currentKwh - adjustedKwh);
        const reduction = currentCost > 0 ? savedCost / currentCost * 100 : 0;
        const set = (selector, value) => {
            const target = document.querySelector(selector);
            if (target) target.textContent = value;
        };
        set("[data-current-cost]", money(currentCost));
        set("[data-current-kwh]", decimal(currentKwh));
        set("[data-adjusted-cost]", money(adjustedCost));
        set("[data-adjusted-kwh]", decimal(adjustedKwh));
        set("[data-savings]", money(savedCost));
        set("[data-kwh-saved]", decimal(savedKwh));
        set("[data-reduction]", decimal(reduction));
        set("[data-adjusted-count]", changedCount);
        set("[data-budget-value]", money(budget));
        set("[data-budget-estimate]", money(adjustedCost));
        set("[data-current-bar-label]", decimal(currentKwh));
        set("[data-adjusted-bar-label]", decimal(adjustedKwh));
        document.querySelector(".current-fill").style.width = currentKwh > 0 ? "100%" : "0%";
        document.querySelector(".adjusted-fill").style.width =
            currentKwh > 0 ? `${Math.min(100, adjustedKwh / currentKwh * 100)}%` : "0%";

        const message = document.querySelector("[data-budget-message]");
        if (adjustedCost <= budget) {
            message.textContent = `Within budget by ₱${money(budget - adjustedCost)}.`;
            message.classList.remove("over-budget");
        } else {
            message.textContent = `Over budget by ₱${money(adjustedCost - budget)}.`;
            message.classList.add("over-budget");
        }

        const guidanceRow = rowValues.find(item =>
            Number(item.row.querySelector('[name$=".Id"]').value) === guidanceId);
        if (guidanceRow) {
            const otherCost = rowValues.filter(item => item !== guidanceRow)
                .reduce((total, item) => total + item.kwh * rate, 0);
            const remainingBudget = Math.max(0, budget - otherCost);
            const hourlyMonthlyCost = Number(guidanceRow.row.dataset.watts) / 1000 *
                Number(guidanceRow.row.dataset.days) * rate;
            const recommended = hourlyMonthlyCost > 0
                ? Math.min(24, remainingBudget / hourlyMonthlyCost)
                : 0;
            set("[data-recommended-hours]", (Math.round(recommended * 10) / 10).toString());
        }
    };

    simulation.querySelectorAll("[data-adjustment-row]").forEach(row => {
        const input = row.querySelector(".hours-input");
        row.querySelectorAll("[data-step]").forEach(button => {
            button.addEventListener("click", () => {
                const next = Number(input.value || 0) + Number(button.dataset.step);
                input.value = (Math.round(Math.min(24, Math.max(0, next)) * 2) / 2).toString();
                updateSimulation();
            });
        });
        input.addEventListener("input", updateSimulation);
    });

    updateSimulation();
})();
