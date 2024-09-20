const inputField = document.getElementById('input');
const outputDiv = document.getElementById('output');

const siteUrl = new URLSearchParams(window.location.search).get('site');
console.log("Terminal for: ", siteUrl);

function runCommand(command) {
    // if command is clear, clear the output div
    if (command === 'clear') {
        outputDiv.innerHTML = '';
        return;
    }
    else if (command === 'exit') {
        window.close();
        return;
    }
    
    // send a post request to the server
    fetch('/command', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ "command":command, "siteUrl":siteUrl })
    })
        .then(response => response.json())
        .then(data => {
            appendOutput(data.output);
        });
    
}

function appendOutput(text) {
    outputDiv.innerHTML += `<div>${text}</div>`;
}

inputField.addEventListener('keydown', function(event) {
    if (event.key === 'Enter') {
        const command = inputField.value.trim();
        if (command !== '') {
            appendOutput(`<span class="prompt">$</span> ${command}`);
            runCommand(command);
            inputField.value = '';
        }
    }
});
