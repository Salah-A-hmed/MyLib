function initAdminDashboard(data) {

    console.log("Admin Dashboard Data Received:", data);

    const colorPalette = ['#2a9d8f', '#e76f51', '#e9c46a', '#8338ec', '#3a86ff', '#f4a261', '#264653', '#a7c957', '#6a040f', '#0077b6'];
    const themeColor = '#4bc1d2';

    const plotlyBaseLayout = {
        paper_bgcolor: 'rgba(0,0,0,0)', plot_bgcolor: 'rgba(0,0,0,0)',
        font: { family: '-apple-system, BlinkMacSystemFont, "Segoe UI", Roboto, sans-serif', color: '#495057' },
        margin: { t: 30, b: 50, l: 150, r: 20 },
        xaxis: { gridcolor: '#e9ecef' }, yaxis: { gridcolor: '#e9ecef', automargin: true }
    };

    // 1. Pie Chart (User Collections)
    function drawGooglePieChart() {
        const chartDiv = document.getElementById('pieUserCollections');
        if (!data.userCollectionChart || data.userCollectionChart.labels.length === 0) {
            chartDiv.innerHTML = "<p class='text-muted text-center p-5'>No users have created collections yet.</p>";
            return;
        }

        google.charts.load('current', { 'packages': ['corechart'] });
        google.charts.setOnLoadCallback(function () {
            const dataArray = [['User', 'Collection Count']];
            data.userCollectionChart.labels.forEach((label, index) => {
                dataArray.push([label, data.userCollectionChart.data[index]]);
            });
            const dataTable = google.visualization.arrayToDataTable(dataArray);
            const options = {
                is3D: true, backgroundColor: 'transparent', colors: colorPalette,
                pieSliceText: 'percentage', pieSliceBorderColor: 'transparent',
                legend: { position: 'bottom' },
                chartArea: { left: 20, top: 20, width: '90%', height: '80%' },
                animation: { startup: true, duration: 1000, easing: 'out' }
            };
            const chart = new google.visualization.PieChart(chartDiv);

            // (اللينك لـ User Dashboard)
            google.visualization.events.addListener(chart, 'select', function () {
                var selection = chart.getSelection();
                if (selection.length > 0) {
                    var rowIndex = selection[0].row;
                    var userId = data.userCollectionChart.ids[rowIndex]; // (استخدام الـ ID)
                    window.location.href = '/Admin/ViewUserDashboard/' + userId;
                }
            });

            chart.draw(dataTable, options);
        });
    }

    // 2. Bar Chart (User Books)
    const barUserBooksDiv = document.getElementById('barUserBooks');
    if (data.userBookCountChart.labels.length > 0) {
        var trace = {
            x: data.userBookCountChart.data,
            y: data.userBookCountChart.labels,
            type: 'bar', orientation: 'h', marker: { color: themeColor }
        };
        Plotly.newPlot(barUserBooksDiv, [trace], { ...plotlyBaseLayout, yaxis: { automargin: true } }, { responsive: true });
    } else {
        barUserBooksDiv.innerHTML = "<p class='text-muted text-center p-5'>No users have added books yet.</p>";
    }

    // 3. Line Chart (Platform Books)
    const lineBooksDiv = document.getElementById('linePlatformBooks');
    if (data.platformBooksTimeline.labels.length > 0) {
        const dateObjects = data.platformBooksTimeline.labels.map(lbl => new Date(lbl + "-01"));
        var trace = {
            x: dateObjects, y: data.platformBooksTimeline.data,
            type: 'scatter', mode: 'lines+markers', fill: 'tozeroy',
            marker: { color: themeColor }
        };
        Plotly.newPlot(lineBooksDiv, [trace], { ...plotlyBaseLayout, xaxis: { type: 'date', tickformat: '%Y-%m', automargin: true }, margin: { l: 50 } }, { responsive: true });
    } else {
        lineBooksDiv.innerHTML = "<p class='text-muted text-center p-5'>No book additions found.</p>";
    }

    // 4. Line Chart (Platform Collections)
    const lineCollectionsDiv = document.getElementById('linePlatformCollections');
    if (data.platformCollectionsTimeline.labels.length > 0) {
        const dateObjects = data.platformCollectionsTimeline.labels.map(lbl => new Date(lbl + "-01"));
        var trace = {
            x: dateObjects, y: data.platformCollectionsTimeline.data,
            type: 'scatter', mode: 'lines+markers', fill: 'tozeroy',
            marker: { color: colorPalette[1] } // (لون مختلف)
        };
        Plotly.newPlot(lineCollectionsDiv, [trace], { ...plotlyBaseLayout, xaxis: { type: 'date', tickformat: '%Y-%m', automargin: true }, margin: { l: 50 } }, { responsive: true });
    } else {
        lineCollectionsDiv.innerHTML = "<p class='text-muted text-center p-5'>No collection additions found.</p>";
    }

    // --- التنفيذ ---
    drawGooglePieChart();
}