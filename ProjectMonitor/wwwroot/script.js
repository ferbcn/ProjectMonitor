const inputField = document.getElementById('input');
const outputDiv = document.getElementById('output');


const commands = {
    help: 'Available commands: help, about, clear',
    about: 'Terminal Simulator v1.0. Created using HTML, CSS, and JavaScript.',
    clear: ''
};

function runCommand(command) {
    switch (command) {
        case 'clear':
            outputDiv.innerHTML = '';
            break;
        case 'help':
        case 'about':
            appendOutput(commands[command]);
            break;
        default:
            appendOutput(`Command not found: ${command}`);
    }
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
