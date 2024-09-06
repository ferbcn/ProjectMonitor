const mySpinner = document.getElementById('mySpinner');

async function fetchData() {
    mySpinner.style.display = 'block';
    const response = await fetch('/dashboard-data');
    const data = await response.json();
    const tableBody = document.getElementById('data-table');
    tableBody.innerHTML = ''; // Clear existing table data
    data.forEach(site => {
        const row = document.createElement('tr');
        
        row.innerHTML = `
            <td>${site.name}</td>
            <td><a href="https://${site.url}" target="_blank">${site.url}</a></td>
            <td>${site.up ? 'Up' : 'Down'}</td>
            <td>${site.downloadMillis}</td>
        `;
        console.log(site.color);
        row.style.backgroundColor = convertColor(site.color);
            
        mySpinner.style.display = 'none';
        tableBody.appendChild(row);
    });
}

document.addEventListener('DOMContentLoaded', () => {
    fetchData();
});

document.getElementById('refresh').addEventListener('click', () => {
    fetchData();
});

// Convert the color object to a valid CSS color string
function convertColor(color) {
    return `rgba(${color.r}, ${color.g}, ${color.b}, ${color.a / 255})`;
}