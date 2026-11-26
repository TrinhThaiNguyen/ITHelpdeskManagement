document.addEventListener('DOMContentLoaded', function () {
    // ---------- Date range filter ----------
    (function () {
        const form = document.getElementById('filterForm');

        if (!form) return;

        const dateFields = document.getElementById('dateFields');
        const fromDate = document.getElementById('fromDate');
        const toDate = document.getElementById('toDate');
        const radios = form.querySelectorAll('input[name="range"]');

        function selectedRange() {
            const checked = form.querySelector('input[name="range"]:checked');
            return checked ? checked.value : 'Today';
        }

        function sync() {
            if (!dateFields || !fromDate || !toDate) return;

            const isCustom = selectedRange() === 'Custom';

            dateFields.classList.toggle('show', isCustom);
            fromDate.disabled = !isCustom;
            toDate.disabled = !isCustom;
        }

        radios.forEach(function (radio) {
            radio.addEventListener('change', function () {
                sync();

                // Quick ranges apply immediately; Custom waits for the Apply button.
                if (selectedRange() !== 'Custom') {
                    form.submit();
                }
            });
        });

        sync();
    })();

    // ---------- Data from the server ----------
    const dataElement = document.getElementById('dashboardData');

    if (!dataElement || typeof Chart === 'undefined') return;

    let dashboardData;

    try {
        dashboardData = JSON.parse(dataElement.textContent);
    } catch (error) {
        console.error('Dashboard data could not be parsed.', error);
        return;
    }

    const {
        priorityRaw = [],
        priorityValues = [],
        statusRaw = [],
        statusValues = [],
        trendLabels = [],
        trendValues = [],
        departmentLabels = [],
        departmentValues = [],
        reopenTrendLabels = [],
        reopenPendingValues = [],
        reopenApprovedValues = [],
        reopenRejectedValues = [],
        slaPriorityData = [],
        slaTrendLabels = [],
        slaResponseTrend = [],
        slaResolutionTrend = []
    } = dashboardData;

    // ---------- Enum -> label (works for numbers and for strings) ----------
    const priorityNames = { 1: 'Low', 2: 'Medium', 3: 'High', 4: 'Critical' };
    const statusNames = { 1: 'New', 2: 'Open', 3: 'In Progress', 4: 'Pending', 5: 'Resolved', 6: 'Closed' };

    function priorityName(value) {
        return typeof value === 'number' ? (priorityNames[value] ?? 'Unknown') : String(value);
    }

    function statusName(value) {
        return typeof value === 'number' ? (statusNames[value] ?? 'Unknown') : String(value);
    }

    const priorityLabels = priorityRaw.map(priorityName);
    const statusLabels = statusRaw.map(statusName);

    // ---------- Theme (matches the Login / Dashboard palette) ----------
    const palette = {
        indigo: '#4f46e5',
        violet: '#7c3aed',
        sky: '#0ea5e9',
        green: '#22c55e',
        amber: '#f59e0b',
        red: '#ef4444',
        pink: '#ec4899',
        slate: '#94a3b8'
    };

    const fallbackColors = [
        palette.indigo, palette.violet, palette.sky,
        palette.green, palette.amber, palette.red,
        palette.pink, palette.slate
    ];

    // Known label -> color; unknown labels fall back to the palette by index
    const semanticColors = {
        low: palette.green,
        medium: palette.sky,
        normal: palette.sky,
        high: palette.amber,
        urgent: palette.red,
        critical: palette.red,
        new: palette.sky,
        open: palette.indigo,
        assigned: palette.violet,
        inprogress: palette.amber,
        onhold: palette.slate,
        pending: palette.slate,
        resolved: palette.green,
        closed: palette.violet,
        cancelled: palette.red,
        rejected: palette.red
    };

    function colorsFor(labels) {
        return labels.map(function (label, i) {
            const key = String(label).toLowerCase().replace(/[\s_-]/g, '');
            return semanticColors[key] || fallbackColors[i % fallbackColors.length];
        });
    }

    Chart.defaults.font.family = "Inter, system-ui, -apple-system, 'Segoe UI', Roboto, Arial, sans-serif";
    Chart.defaults.font.size = 12;
    Chart.defaults.color = '#6b7280';
    Chart.defaults.plugins.legend.labels.usePointStyle = true;
    Chart.defaults.plugins.legend.labels.pointStyle = 'circle';
    Chart.defaults.plugins.legend.labels.boxWidth = 8;
    Chart.defaults.plugins.legend.labels.padding = 16;
    Chart.defaults.plugins.tooltip.backgroundColor = '#111827';
    Chart.defaults.plugins.tooltip.padding = 12;
    Chart.defaults.plugins.tooltip.cornerRadius = 10;
    Chart.defaults.plugins.tooltip.titleFont = { weight: '600' };
    Chart.defaults.plugins.tooltip.boxPadding = 4;

    const gridColor = 'rgba(17, 24, 39, 0.06)';

    function verticalScales() {
        return {
            x: { grid: { display: false }, border: { display: false } },
            y: {
                beginAtZero: true,
                border: { display: false },
                grid: { color: gridColor },
                ticks: { precision: 0 }
            }
        };
    }

    function lineDataset(label, data, color, fillColor) {
        return {
            label: label,
            data: data,
            borderColor: color,
            backgroundColor: fillColor,
            borderWidth: 2.5,
            tension: 0.4,
            fill: true,
            pointRadius: 2,
            pointHoverRadius: 6,
            pointBackgroundColor: '#ffffff',
            pointBorderColor: color,
            pointBorderWidth: 2
        };
    }

    function hasPositive(values) {
        return values.some(function (value) {
            return Number(value) > 0;
        });
    }

    // ---------- Chart helpers (loading + empty state) ----------
    function hideChartLoading(chartId) {
        const loadingElement = document.getElementById(chartId + 'Loading');

        if (loadingElement) {
            loadingElement.style.display = 'none';
        }
    }

    // Returns true when the chart can be drawn, false when the empty state is shown.
    function prepareChart(chartId, hasData) {
        const canvas = document.getElementById(chartId);

        if (!canvas) {
            return false;
        }

        hideChartLoading(chartId);

        const wrapper = canvas.closest('.chart-wrapper');
        const existingEmpty = wrapper ? wrapper.querySelector('.chart-empty') : null;

        if (hasData) {
            canvas.style.display = '';

            if (existingEmpty) {
                existingEmpty.remove();
            }

            return true;
        }

        canvas.style.display = 'none';

        if (wrapper && !existingEmpty) {
            const emptyElement = document.createElement('div');
            emptyElement.className = 'chart-empty';
            emptyElement.textContent = 'No data available.';
            wrapper.appendChild(emptyElement);
        }

        return false;
    }

    function renderChart(chartId, hasData, config) {
        if (!prepareChart(chartId, hasData)) {
            return;
        }

        new Chart(document.getElementById(chartId), config);
    }

    // ---------- Priority (doughnut) ----------
    renderChart('priorityChart', hasPositive(priorityValues), {
        type: 'doughnut',
        data: {
            labels: priorityLabels,
            datasets: [{
                data: priorityValues,
                backgroundColor: colorsFor(priorityLabels),
                borderColor: '#ffffff',
                borderWidth: 3,
                hoverOffset: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            cutout: '68%',
            plugins: { legend: { position: 'bottom' } }
        }
    });

    // ---------- Status (pie) ----------
    renderChart('statusChart', hasPositive(statusValues), {
        type: 'pie',
        data: {
            labels: statusLabels,
            datasets: [{
                data: statusValues,
                backgroundColor: colorsFor(statusLabels),
                borderColor: '#ffffff',
                borderWidth: 3,
                hoverOffset: 8
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: 'bottom' } }
        }
    });

    // ---------- Ticket trend (line with gradient area) ----------
    renderChart('trendChart', hasPositive(trendValues), {
        type: 'line',
        data: {
            labels: trendLabels,
            datasets: [{
                label: 'Created Tickets',
                data: trendValues,
                borderColor: palette.indigo,
                borderWidth: 3,
                tension: 0.4,
                fill: true,
                backgroundColor: function (context) {
                    const chart = context.chart;
                    const area = chart.chartArea;

                    if (!area) {
                        return 'rgba(79, 70, 229, 0.12)';
                    }

                    const gradient = chart.ctx.createLinearGradient(0, area.top, 0, area.bottom);
                    gradient.addColorStop(0, 'rgba(79, 70, 229, 0.28)');
                    gradient.addColorStop(1, 'rgba(124, 58, 237, 0)');

                    return gradient;
                },
                pointRadius: 3,
                pointHoverRadius: 6,
                pointBackgroundColor: '#ffffff',
                pointBorderColor: palette.indigo,
                pointBorderWidth: 2
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            interaction: { mode: 'index', intersect: false },
            plugins: { legend: { display: false } },
            scales: verticalScales()
        }
    });

    // ---------- Department (horizontal bar) ----------
    renderChart('departmentChart', hasPositive(departmentValues), {
        type: 'bar',
        data: {
            labels: departmentLabels,
            datasets: [{
                label: 'Total Tickets',
                data: departmentValues,
                backgroundColor: 'rgba(99, 102, 241, 0.85)',
                hoverBackgroundColor: palette.violet,
                borderRadius: 8,
                borderSkipped: false,
                maxBarThickness: 28
            }]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            indexAxis: 'y',
            plugins: { legend: { display: false } },
            scales: {
                x: {
                    beginAtZero: true,
                    border: { display: false },
                    grid: { color: gridColor },
                    ticks: { precision: 0 }
                },
                y: { grid: { display: false }, border: { display: false } }
            }
        }
    });

    // ---------- Reopen request trends ----------
    renderChart(
        'reopenTrendChart',
        hasPositive([...reopenPendingValues, ...reopenApprovedValues, ...reopenRejectedValues]),
        {
            type: 'line',
            data: {
                labels: reopenTrendLabels,
                datasets: [
                    lineDataset('Pending', reopenPendingValues, palette.amber, 'rgba(245, 158, 11, 0.08)'),
                    lineDataset('Approved', reopenApprovedValues, palette.green, 'rgba(34, 197, 94, 0.08)'),
                    lineDataset('Rejected', reopenRejectedValues, palette.red, 'rgba(239, 68, 68, 0.08)')
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: { legend: { position: 'bottom' } },
                scales: verticalScales()
            }
        }
    );

    // ---------- SLA breaches by priority ----------
    const hasSlaPriorityData = slaPriorityData.some(function (item) {
        return Number(item.responseBreached) > 0 || Number(item.resolutionBreached) > 0;
    });

    renderChart('slaPriorityChart', hasSlaPriorityData, {
        type: 'bar',
        data: {
            labels: slaPriorityData.map(function (x) { return priorityName(x.priority); }),
            datasets: [
                {
                    label: 'Response Breached',
                    data: slaPriorityData.map(function (x) { return x.responseBreached; }),
                    backgroundColor: palette.amber,
                    borderRadius: 8,
                    borderSkipped: false,
                    maxBarThickness: 32
                },
                {
                    label: 'Resolution Breached',
                    data: slaPriorityData.map(function (x) { return x.resolutionBreached; }),
                    backgroundColor: palette.red,
                    borderRadius: 8,
                    borderSkipped: false,
                    maxBarThickness: 32
                }
            ]
        },
        options: {
            responsive: true,
            maintainAspectRatio: false,
            plugins: { legend: { position: 'bottom' } },
            scales: verticalScales()
        }
    });

    // ---------- SLA breach trends ----------
    renderChart(
        'slaTrendChart',
        hasPositive([...slaResponseTrend, ...slaResolutionTrend]),
        {
            type: 'line',
            data: {
                labels: slaTrendLabels,
                datasets: [
                    lineDataset('Response Breached', slaResponseTrend, palette.amber, 'rgba(245, 158, 11, 0.08)'),
                    lineDataset('Resolution Breached', slaResolutionTrend, palette.red, 'rgba(239, 68, 68, 0.08)')
                ]
            },
            options: {
                responsive: true,
                maintainAspectRatio: false,
                interaction: { mode: 'index', intersect: false },
                plugins: { legend: { position: 'bottom' } },
                scales: verticalScales()
            }
        }
    );
});