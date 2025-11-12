function initCollectionDetailsDashboard(data) {

    console.log("Collection Details Data Received:", data);

    // 1. الألوان (نفس الباليت)
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
        yaxis: { gridcolor: '#e9ecef', automargin: true }
    };

    // helper: safe access + coercion
    function toStringArray(arr) {
        if (!Array.isArray(arr)) return [];
        return arr.map(a => a === null || a === undefined ? "" : String(a));
    }
    function toNumberArray(arr) {
        if (!Array.isArray(arr)) return [];
        return arr.map(n => {
            const v = Number(n);
            return Number.isNaN(v) ? 0 : v;
        });
    }

    // --- (أ) رسم الـ KPIs (Plotly) ---
    function drawIndicators() {
        const kpiLayout = { ...plotlyBaseLayout, margin: { t: 0, b: 0, l: 0, r: 0 }, height: 150 };
        var dataBooks = [{
            type: "indicator", mode: "number", value: Number(data.totalUniqueBooks || 0),
            title: { text: "Unique Books in this Collection", font: { size: 16, color: "#6c757d" } },
            number: { font: { size: 48, color: themeColor, family: "Arial" }, suffix: " Books" }
        }];
        Plotly.newPlot('kpiTotalBooks', dataBooks, kpiLayout, { responsive: true });

        var dataCopies = [{
            type: "indicator", mode: "number", value: Number(data.totalCopies || 0),
            title: { text: "Total Copies in this Collection", font: { size: 16, color: "#6c757d" } },
            number: { font: { size: 48, color: themeColor, family: "Arial" }, suffix: " Copies" }
        }];
        Plotly.newPlot('kpiTotalCopies', dataCopies, kpiLayout, { responsive: true });
    }

    // --- (ب) رسم الـ Pie Chart (Google Charts - Status) ---
    function drawStatusPieChart() {
        const chartDiv = document.getElementById('statusPieChart');
        if (!data.statusPieChart || !Array.isArray(data.statusPieChart.labels) || data.statusPieChart.labels.length === 0) {
            chartDiv.innerHTML = "<p class='text-muted text-center p-5'>No books found to analyze status.</p>";
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
                colors: colorPalette,
                pieSliceText: 'percentage',
                pieSliceBorderColor: 'transparent',
                legend: { position: 'bottom', textStyle: { color: '#495057', fontSize: 12 } },
                chartArea: { left: 10, top: 10, width: '90%', height: '80%' }
            };
            const chart = new google.visualization.PieChart(chartDiv);
            chart.draw(dataTable, options);
        });
    }

    // --- (ج) رسم الـ Bar Charts (Plotly) ---
    function drawBarCharts() {
        const barLayout = { ...plotlyBaseLayout, yaxis: { automargin: true } };

        // 1. Top 10 Copies
        const copiesChartDiv = document.getElementById('copiesBarChart');
        if (data.copiesBarChart.labels.length > 0) {
            var traceCopies = {
                x: data.copiesBarChart.data,
                y: data.copiesBarChart.labels,
                type: 'bar', orientation: 'h', marker: { color: themeColor }
            };
            Plotly.newPlot(copiesChartDiv, [traceCopies], barLayout, { responsive: true });
        } else {
            copiesChartDiv.innerHTML = "<p class='text-muted text-center p-5'>No copies tracked in this collection.</p>";
        }

        // 2. Top 10 Price
        const priceChartDiv = document.getElementById('priceBarChart');
        if (data.priceBarChart.labels.length > 0) {
            var traceValue = {
                x: data.priceBarChart.data,
                y: data.priceBarChart.labels,
                type: 'bar', orientation: 'h', marker: { color: valueColor },
                text: data.priceBarChart.data.map(v => `EGP ${v.toLocaleString('en-EG')}`),
                textposition: 'auto', hoverinfo: 'y+text'
            };
            Plotly.newPlot(priceChartDiv, [traceValue], barLayout, { responsive: true });
        } else {
            priceChartDiv.innerHTML = "<p class='text-muted text-center p-5'>No pricing data found for books in this collection.</p>";
        }
    }

    // --- (د) رسم الـ Column Charts (Plotly) ---
    function drawColumnCharts() {
        // 1. Rating Histogram
        const ratingChartDiv = document.getElementById('ratingHistogramChart');
        const ratingLabels = toStringArray(data?.ratingHistogram?.labels);
        const ratingData = toNumberArray(data?.ratingHistogram?.data);
        console.log("ratingLabels:", ratingLabels, "ratingData:", ratingData);

        if (ratingData.some(d => d > 0) && ratingLabels.length === ratingData.length && ratingLabels.length > 0) {
            var traceRating = {
                x: ratingLabels,
                y: ratingData,
                type: 'bar',
                marker: { color: colorPalette[2] }
            };
            // force categorical x axis so labels show as categories
            const layout = { ...plotlyBaseLayout, xaxis: { type: 'category', automargin: true }, margin: { l: 50, b: 100 } };
            Plotly.newPlot(ratingChartDiv, [traceRating], layout, { responsive: true });
        } else if (ratingData.some(d => d > 0)) {
            ratingChartDiv.innerHTML = "<p class='text-muted text-center p-5'>Ratings exist but labels/data mismatch.</p>";
        } else {
            ratingChartDiv.innerHTML = "<p class='text-muted text-center p-5'>No ratings available for books in this collection.</p>";
        }

        // 2. Date Timeline (convert 'yyyy-MM' labels to real Date objects)
        const dateChartDiv = document.getElementById('dateTimelineChart');
        const dateLabels = toStringArray(data?.dateAddedTimeline?.labels);
        const dateData = toNumberArray(data?.dateAddedTimeline?.data);
        console.log("dateLabels:", dateLabels, "dateData:", dateData);

        if (dateLabels.length > 0 && dateData.length === dateLabels.length) {
            // convert "yyyy-MM" to Date at start of month
            const dateObjects = dateLabels.map(lbl => {
                // lbl expected "yyyy-MM" or "yyyy-MM-dd" - try safe parse
                try {
                    return new Date(lbl + "-01");
                } catch (e) {
                    return new Date(lbl);
                }
            });
            var traceDate = {
                x: dateObjects,
                y: dateData,
                type: 'scatter',
                mode: 'lines+markers',
                fill: 'tozeroy',
                marker: { color: themeColor },
                hovertemplate: '%{x|%Y-%m}<br>%{y}<extra></extra>'
            };
            const layout = { ...plotlyBaseLayout, xaxis: { type: 'date', tickformat: '%Y-%m', automargin: true }, margin: { l: 50 } };
            Plotly.newPlot(dateChartDiv, [traceDate], layout, { responsive: true });
        } else if (dateData.some(d => d > 0)) {
            dateChartDiv.innerHTML = "<p class='text-muted text-center p-5'>Date data present but labels are missing or mismatched.</p>";
        } else {
            dateChartDiv.innerHTML = "<p class='text-muted text-center p-5'>No date information found for books in this collection.</p>";
        }
    }

    // --- (هـ) التنفيذ ---
    drawIndicators();
    drawStatusPieChart();
    drawBarCharts();
    drawColumnCharts();
}