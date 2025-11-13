function initVisitorDetailsDashboard(data) {

    console.log("Visitor Details Data Received:", data);

    // (1) الألوان
    const colorPalette = [
        '#2a9d8f', // (Returned - Green)
        '#e76f51', // (Overdue - Red)
        '#e9c46a'  // (Borrowed - Yellow)
    ];
    // (إعادة ترتيب الألوان عشان تطابق الحالات)
    const statusColors = data.statusPieChart.labels.map(label => {
        if (label === 'Returned') return colorPalette[0];
        if (label === 'Overdue') return colorPalette[1];
        if (label === 'Borrowed') return colorPalette[2];
        return '#ccc'; // (لون افتراضي لو فيه حالة رابعة)
    });

    // (2) تنسيق Plotly
    const plotlyBaseLayout = {
        paper_bgcolor: 'rgba(0,0,0,0)',
        plot_bgcolor: 'rgba(0,0,0,0)',
        font: { family: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif', color: '#495057' },
        margin: { t: 0, b: 0, l: 0, r: 0 },
        height: 140 // (ارتفاع أقل للـ KPI)
    };

    // --- (أ) رسم الـ KPIs (Plotly) ---
    function drawIndicators() {
        var dataBooks = [{
            type: "indicator", mode: "number", value: Number(data.totalBorrowings || 0),
            title: { text: "Total Borrowings", font: { size: 16, color: "#6c757d" } },
            number: { font: { size: 48, color: '#4bc1d2', family: "Arial" } }
        }];
        Plotly.newPlot('kpiTotalBorrowings', dataBooks, plotlyBaseLayout, { responsive: true });

        var dataFines = [{
            type: "indicator", mode: "number", value: Number(data.totalFines || 0),
            title: { text: "Total Overdue Fines", font: { size: 16, color: "#6c757d" } },
            number: {
                font: { size: 48, color: (data.totalFines > 0 ? '#dc3545' : '#198754'), family: "Arial" },
                prefix: "EGP "
            }
        }];
        Plotly.newPlot('kpiTotalFines', dataFines, plotlyBaseLayout, { responsive: true });
    }

    // --- (ب) رسم الـ Pie Chart (Google Charts - Status) ---
    function drawStatusPieChart() {
        const chartDiv = document.getElementById('statusPieChart');
        if (!data.statusPieChart || !Array.isArray(data.statusPieChart.labels) || data.statusPieChart.labels.length === 0) {
            chartDiv.innerHTML = "<p class='text-muted text-center p-5'>No borrowing history found.</p>";
            return;
        }

        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(function () {
            const dataArray = [['Status', 'Book Count']];
            data.statusPieChart.labels.forEach((label, index) => {
                dataArray.push([String(label), Number(data.statusPieChart.data[index] || 0)]);
            });
            const dataTable = google.visualization.arrayToDataTable(dataArray);
            const options = {
                is3D: true,
                backgroundColor: 'transparent',
                colors: statusColors, // (استخدام الألوان المخصصة)
                pieSliceText: 'percentage',
                pieSliceBorderColor: 'transparent',
                legend: { position: 'bottom', textStyle: { color: '#495057', fontSize: 12 } },
                chartArea: { left: 10, top: 10, width: '90%', height: '80%' },
                animation: {
                    startup: true,
                    duration: 1000,
                    easing: 'out',
                }
            };
            const chart = new google.visualization.PieChart(chartDiv);
            chart.draw(dataTable, options);
        });
    }

    // --- (ج) التنفيذ ---
    drawIndicators();
    drawStatusPieChart();
}