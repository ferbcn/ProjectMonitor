const mySpinner = document.getElementById('mySpinner');

async function fetchData() {
    mySpinner.style.display = 'block';
    const response = await fetch('/dashboard-data');
    const data = await response.json();
    
    const tableBody = document.getElementById('table-body');
    tableBody.innerHTML = '';
    
    data.forEach(site => {
        const row = document.createElement('li');
        row.classList.add('table-row');
        
        row.innerHTML = `
                <div class="col col-1" data-label="Name">${site.name}</div>
                <div class="col col-2" data-label="URL"><a href="https://${site.url}" target="_blank">${site.url}</a></div>
                <div class="col col-3" data-label="Status">${site.up ? 'Up' : 'Down'}</div>
                <div class="col col-4" data-label="Time (ms)">${site.downloadMillis}</div>
        `;
        
        // console.log(site.color);
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
    return `rgba(${color.r}, ${color.g}, ${color.b}, ${color.a/255})`;
}
