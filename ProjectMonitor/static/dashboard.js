async function fetchData() {
    const response = await fetch('/dashboard-data');
    const data = await response.json();
    const tableBody = document.getElementById('data-table');
    tableBody.innerHTML = ''; // Clear existing table data
    data.forEach(site => {
        const row = document.createElement('tr');
        row.style.backgroundColor = 'darkgreen';
        
        row.innerHTML = `
            <td>${site.name}</td>
            <td><a href="https://${site.url}" target="_blank">${site.url}</a></td>
            <td>${site.up ? 'Up' : 'Down'}</td>
            <td>${site.ping_time}</td>
            <td>${site.downloadMillis}</td>
        `;
        if (site.downloadMillis > 99) {
            row.style.backgroundColor = 'orange';
        }
        
        if (!site.up) {
            row.style.backgroundColor = 'firebrick';
        } 
        
        tableBody.appendChild(row);
    });
}

document.addEventListener('DOMContentLoaded', () => {
    fetchData();
});

document.getElementById('refresh').addEventListener('click', () => {
    fetchData();
});
