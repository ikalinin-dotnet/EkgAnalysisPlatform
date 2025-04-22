window.drawEkgChart = (canvasId, dataPoints) => {
    const canvas = document.getElementById(canvasId);
    const ctx = canvas.getContext('2d');
    
    // Clear canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);
    
    // Set line style
    ctx.strokeStyle = '#00AA00';
    ctx.lineWidth = 2;
    
    // Calculate scales
    const width = canvas.width;
    const height = canvas.height;
    const xScale = width / dataPoints.length;
    const yMax = Math.max(...dataPoints);
    const yMin = Math.min(...dataPoints);
    const yRange = yMax - yMin;
    const yScale = height / (yRange * 1.2); // Add 20% padding
    const yOffset = height / 2;
    
    // Draw EKG line
    ctx.beginPath();
    ctx.moveTo(0, height - (dataPoints[0] - yMin) * yScale - yOffset);
    
    for (let i = 1; i < dataPoints.length; i++) {
        const x = i * xScale;
        const y = height - (dataPoints[i] - yMin) * yScale - yOffset;
        ctx.lineTo(x, y);
    }
    
    ctx.stroke();
};