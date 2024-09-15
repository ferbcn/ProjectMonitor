const mySpinner = document.getElementById('mySpinner');
const tableBody = document.getElementById('table-body');
const refreshBtn = document.getElementById('refresh'); // Get the refreshButton
const streamBtn = document.getElementById('stream-toggle'); // Get the streamButton
const animSwitch = document.getElementById('switch'); // Get the animation switch

let eventSource;

// Function to initialize the EventSource connection
function initializeEventSource() {
    eventSource = new EventSource('/api-stream');

    eventSource.onmessage = (event) => {
        // Update the HTML page with the streamed data
        const sse_data = event.data;
        if (sse_data === 'DONE') {
            console.log("ALL DONE!");
            // close the connection
            eventSource.close();
            console.log("Closing connection...");
            return;
        }
        const site = JSON.parse(sse_data);
        console.log("Data received:", sse_data);
        
        // check if row with id site.url exists
        if (document.getElementById(site.url)) {
            // delete row
            // tableBody.removeChild(document.getElementById(site.url));
            
            // update the row
            const row = document.getElementById(site.url);
            row.children[2].innerText = site.up ? 'Up' : 'Down';
            row.children[3].innerText = site.pingMillis;
            row.children[4].innerText = site.downloadMillis;
            row.style.backgroundColor = site.colorHex;
            
            row.classList.add('highlight');
            setTimeout(() => row.classList.remove('highlight'), 1000);
            return;
        }

        const row = document.createElement('li');
        row.id = site.url;
        row.classList.add('table-row');
        row.innerHTML = `
            <div class="col col-1" data-label="Name">${site.name}</div>
            <div class="col col-2" data-label="URL"><a href="https://${site.url}" target="_blank">${site.url}</a></div>
            <div class="col col-3" data-label="Status">${site.up ? 'Up' : 'Down'}</div>
            <div class="col col-4" data-label="Ping (ms)">${site.pingMillis}</div>
            <div class="col col-5" data-label="Load (ms)">${site.downloadMillis}</div>
            <div class="col col-6" data-label="Tools">
                <div class="tool" ><a href="https://${site.url}" target="_blank">&#128279;</a></div>
            </div>
        `;
        row.style.backgroundColor = site.colorHex;
        tableBody.appendChild(row);

        mySpinner.style.display = 'none';
    };

    eventSource.onerror = () => {
        console.error('Error occurred while connecting to the SSE endpoint. Reconnecting...');
        eventSource.close();
        setTimeout(initializeEventSource, 3000); // Reconnect after 3 seconds
    };

    eventSource.onopen = () => {
        console.log('Connected to the SSE endpoint');
    };

    eventSource.onclose = () => {
        console.log('Disconnected from the SSE endpoint. Reconnecting...');
        // setTimeout(initializeEventSource, 3000); // Reconnect after 3 seconds
    };
}

// Initialize the EventSource connection
initializeEventSource();

refreshBtn.addEventListener('click', () => {
    // Clear the table
    tableBody.innerHTML = '';

    // Reconnect to the SSE endpoint
    eventSource.close();
    initializeEventSource();

    // Show the spinner
    mySpinner.style.display = 'block';
});

// streamBtn.addEventListener('click', () => {
//     if (!streamOn) {
//         tableBody.innerHTML = '';
//         streamOn = true;
//         initializeEventSource();
//         streamBtn.innerText = 'Stop Stream';
//     } else {
//         streamOn = false;
//         eventSource.close();
//         streamBtn.innerText = 'Start Stream';
//     }
// });

animSwitch.addEventListener('change', () => {
    const canvas = document.getElementById('canvas');
    if (animSwitch.checked) {
        canvas.style.display = 'block';
    } else {
        canvas.style.display = 'none';
    }
});