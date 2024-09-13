const mySpinner = document.getElementById('mySpinner');

// Create an EventSource object to connect to the SSE endpoint
const eventSource = new EventSource('api-stream/');


// Define a function to handle the incoming events
eventSource.onmessage = (event) => {
    // Update the HTML page with the streamed data
    const sse_data = event.data;
    // parse the JSON string to object
    const site = JSON.parse(sse_data);
    // console.log("Data received:", sse_data);
    
    const tableBody = document.getElementById('table-body');

    const row = document.createElement('li');
    row.id = site.url;
    row.classList.add('table-row');
    row.innerHTML = `
            <div class="col col-1" data-label="Name">${site.name}</div>
            <div class="col col-2" data-label="URL"><a href="https://${site.url}" target="_blank">${site.url}</a></div>
            <div class="col col-3" data-label="Status">${site.up ? 'Up' : 'Down'}</div>
            <div class="col col-4" data-label="Time (ms)">${site.downloadMillis}</div>
            <div class="col col-5" data-label="Tools">
                <div class="tool" ><a href="https://${site.url}" target="_blank">&#128279;</a></div>
            </div>
    `;
    console.log("Color: ",site.color);
    row.style.backgroundColor = convertColor(site.color);
    tableBody.appendChild(row);

    mySpinner.style.display = 'none';
};

// Define a function to handle errors
eventSource.onerror = () => {
    console.error('Error occurred while connecting to the SSE endpoint');
};

// Define a function to handle the connection being closed
eventSource.onopen = () => {
    console.log('Connected to the SSE endpoint');
};

// Define a function to handle the connection being closed
eventSource.onclose = () => {
    console.log('Disconnected from the SSE endpoint');
};


// Convert the color object to a valid CSS color string
function convertColor(color) {
    const colorString = `rgba(${color.R}, ${color.G}, ${color.B}, ${color.A/255})`;
    console.log("Color String: ",colorString);
    return colorString;
}
