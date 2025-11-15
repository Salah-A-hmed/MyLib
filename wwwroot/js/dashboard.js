function initCollectionDashboard(dashboardData) {

    console.log("Dashboard Data Received:", dashboardData);

    // 1. رينج الألوان الموسع
    const colorPalette = [
        '#2a9d8f', '#e76f51', '#e9c46a', '#8338ec', '#3a86ff',
        '#f4a261', '#264653', '#a7c957', '#6a040f', '#0077b6'
    ];
    const themeColor = '#4bc1d2';
    const valueColor = 'rgba(25, 135, 84, 0.6)';

    // 2. تنسيق Plotly
    const plotlyBaseLayout = {
        paper_bgcolor: 'rgba(0,0,0,0)',
        plot_bgcolor: 'rgba(0,0,0,0)',
        font: {
            family: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif',
            color: '#495057'
        },
        margin: { t: 30, b: 50, l: 150, r: 20 },
        xaxis: { gridcolor: '#e9ecef' },
        yaxis: { gridcolor: '#e9ecef' }
    };

    // (جديد) دالة بتحدد المسار الصح (هل إحنا أدمن ولا يوزر عادي)
    function getRedirectUrl(id) {
        // Make pathname comparison case-insensitive
        const path = (window.location && window.location.pathname) ? window.location.pathname.toLowerCase() : '';
        if (path.startsWith("/admin")) {
            // لو إحنا في صفحة الأدمن، وديه لصفحة تفاصيل الكولكشن بتاعة الأدمن
            return `/Admin/ViewCollectionDetails/${id}`;
        } else {
            // لو إحنا في صفحتنا العادية، وديه لصفحة التفاصيل العادية
            return `/Collections/Details/${id}`;
        }
    }

    // --- (أ) رسم الـ Pie Chart (Google Charts) ---
    function drawGooglePieChart() {
        const chartDiv = document.getElementById('pieBookCountChart');
        if (!dashboardData.collectionBookCountChart || dashboardData.collectionBookCountChart.labels.length === 0) {
            chartDiv.innerHTML = "<p class='text-muted text-center p-5'>No collection data to display.</p>";
            return;
        }

        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(function () {
            const dataArray = [['Collection', 'Book Count']];
            dashboardData.collectionBookCountChart.labels.forEach((label, index) => {
                dataArray.push([label, dashboardData.collectionBookCountChart.data[index]]);
            });
            const data = google.visualization.arrayToDataTable(dataArray);
            const options = {
                is3D: true,
                backgroundColor: 'transparent',
                colors: colorPalette,
                pieSliceText: 'percentage',
                pieSliceBorderColor: 'transparent',
                legend: { position: 'bottom', textStyle: { color: '#495057', fontSize: 12 } },
                chartArea: { left: 20, top: 20, width: '90%', height: '80%' }
            };
            const chart = new google.visualization.PieChart(chartDiv);

            // (جديد) إضافة event listener (ملاحظة: جوجل شارت لا تدعم "dblclick" بسهولة، سنستخدم "click")
            google.visualization.events.addListener(chart, 'select', function () {
                try {
                    var selection = chart.getSelection();
                    if (selection && selection.length > 0) {
                        var rowIndex = selection[0].row;
                        var collectionId = dashboardData.collectionBookCountChart.ids[rowIndex];
                        if (collectionId !== undefined && collectionId !== null) {
                            window.location.href = getRedirectUrl(collectionId);
                        }
                    }
                } catch (e) {
                    console.error("Error handling pie chart selection:", e);
                }
            });

            chart.draw(data, options);
        });
    }

    // --- (ب) رسم الـ KPIs (Plotly) ---
    function drawPlotlyIndicators() {
        const kpiLayout = { ...plotlyBaseLayout, margin: { t: 0, b: 0, l: 0, r: 0 }, height: 150 };

        var dataBooks = [{
            type: "indicator", mode: "number", value: dashboardData.totalUniqueBooks,
            title: { text: "Total Unique Books", font: { size: 16, color: "#6c757d" } },
            number: { font: { size: 48, color: themeColor, family: "Arial" }, suffix: " Books" }
        }];
        Plotly.newPlot('kpiTotalBooks', dataBooks, kpiLayout, { responsive: true });

        var dataCopies = [{
            type: "indicator", mode: "number", value: dashboardData.totalCopies,
            title: { text: "Total Copies (All Books)", font: { size: 16, color: "#6c757d" } },
            number: { font: { size: 48, color: themeColor, family: "Arial" }, suffix: " Copies" }
        }];
        Plotly.newPlot('kpiTotalCopies', dataCopies, kpiLayout, { responsive: true });
    }

    // --- (ج) رسم الـ Bar Charts (Plotly) ---
    function drawPlotlyBarCharts() {
        const barLayout = { ...plotlyBaseLayout, yaxis: { automargin: true } };

        // 1. Bar Chart (Copies)
        const copiesChartDiv = document.getElementById('barCopiesChart');
        if (dashboardData.collectionCopiesChart.labels.length > 0) {
            var traceCopies = {
                x: dashboardData.collectionCopiesChart.data,
                y: dashboardData.collectionCopiesChart.labels,
                type: 'bar', orientation: 'h', marker: { color: themeColor }
            };
            Plotly.newPlot(copiesChartDiv, [traceCopies], barLayout, { responsive: true });

            // (جديد) إضافة dblclick listener
            copiesChartDiv.on('plotly_doubleclick', function (data) {
                try {
                    if (data && data.points && data.points.length > 0) {
                        var pointIndex = data.points[0].pointNumber;
                        var collectionId = dashboardData.collectionCopiesChart.ids[pointIndex];
                        if (collectionId !== undefined && collectionId !== null) {
                            window.location.href = getRedirectUrl(collectionId);
                        }
                    }
                } catch (e) {
                    console.error("Error handling copies chart doubleclick:", e);
                }
            });
        } else {
            copiesChartDiv.innerHTML = "<p class='text-muted text-center p-5'>No data to display.</p>";
        }

        // 2. Bar Chart (Value)
        const valueChartDiv = document.getElementById('barValueChart');
        if (dashboardData.collectionValueChart.labels.length > 0) {
            var traceValue = {
                x: dashboardData.collectionValueChart.data,
                y: dashboardData.collectionValueChart.labels,
                type: 'bar', orientation: 'h', marker: { color: valueColor },
                text: dashboardData.collectionValueChart.data.map(v => `EGP ${v.toLocaleString('en-EG')}`),
                textposition: 'auto', hoverinfo: 'y+text'
            };
            Plotly.newPlot(valueChartDiv, [traceValue], barLayout, { responsive: true });

            // (جديد) إضافة dblclick listener
            valueChartDiv.on('plotly_doubleclick', function (data) {
                try {
                    if (data && data.points && data.points.length > 0) {
                        var pointIndex = data.points[0].pointNumber;
                        var collectionId = dashboardData.collectionValueChart.ids[pointIndex];
                        if (collectionId !== undefined && collectionId !== null) {
                            window.location.href = getRedirectUrl(collectionId);
                        }
                    }
                } catch (e) {
                    console.error("Error handling value chart doubleclick:", e);
                }
            });
        } else {
            valueChartDiv.innerHTML = "<p class='text-muted text-center p-5'>No pricing data available.</p>";
        }
    }

    // --- (د) رسم الـ Column Charts (Plotly) ---
    function drawPlotlyColumnCharts() {
        const columnLayout = { ...plotlyBaseLayout, barmode: 'stack', xaxis: { automargin: true } };

        // 1. Column Chart (Monthly)
        const monthlyChartDiv = document.getElementById('colMonthlyChart');
        if (dashboardData.monthlyAddedChart.series.length > 0) {
            const monthlyTraces = dashboardData.monthlyAddedChart.series.map((series, index) => ({
                x: dashboardData.monthlyAddedChart.labels,
                y: series.data,
                name: series.name,
                type: 'bar',
                marker: { color: colorPalette[index % colorPalette.length] }
            }));
            Plotly.newPlot(monthlyChartDiv, monthlyTraces, columnLayout, { responsive: true });
            // (الـ Charts المكدسة مش هينفع نعملها click-through)
        } else {
            monthlyChartDiv.innerHTML = "<p class='text-muted text-center p-5'>No books added this year yet.</p>";
        }

        // 2. Column Chart (Yearly)
        const yearlyChartDiv = document.getElementById('colYearlyChart');
        if (dashboardData.yearlyAddedChart.series.length > 0) {
            const yearlyTraces = dashboardData.yearlyAddedChart.series.map((series, index) => ({
                x: dashboardData.yearlyAddedChart.labels,
                y: series.data,
                name: series.name,
                type: 'bar',
                marker: { color: colorPalette[index % colorPalette.length] }
            }));
            Plotly.newPlot(yearlyChartDiv, yearlyTraces, columnLayout, { responsive: true });
        } else {
            yearlyChartDiv.innerHTML = "<p class='text-muted text-center p-5'>No books added in previous years.</p>";
        }
    }

    // --- (هـ) التنفيذ ---
    drawGooglePieChart();
    drawPlotlyIndicators();
    drawPlotlyBarCharts();
    drawPlotlyColumnCharts();
}