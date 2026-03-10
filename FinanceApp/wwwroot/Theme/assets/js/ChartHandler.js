function renderMyChart(canvasId, chartData, chartLabels, type, name) {
    
    const generatePastelColors = (count) => {
        return Array.from({ length: count }, (_, i) => {
            const hue = (i * 360) / count;
            return `hsl(${hue}, 75%, 85%)`;
        });
    };

    const bordercolor = type == 'line' ? 'rgba(54, 162, 235, 1)' : '#fff';

    const ctx = document.getElementById(canvasId).getContext('2d');

    new Chart(ctx, {
        type: type, 
        data: {
            labels: chartLabels,
            datasets: [{
                label: name,
                data: chartData,
                backgroundColor: generatePastelColors(chartData.length),
                borderColor: bordercolor,
                borderWidth: 1
            }]
        },
        options: {
            responsive: true
        }
    });
}