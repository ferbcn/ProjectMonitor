const inputField = document.getElementById('input');
const outputDiv = document.getElementById('output');


const commands = {
    help: 'Available commands: help, about, clear',
    about: 'Terminal Simulator v1.0. Created using HTML, CSS, and JavaScript.',
    clear: ''
};

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
        body: JSON.stringify({ command })
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
