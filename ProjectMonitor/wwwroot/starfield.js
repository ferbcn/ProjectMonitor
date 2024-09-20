const animSwitch = document.getElementById('switch'); // Get the animation switch


// Get the canvas element
const canvas = document.getElementById('canvas');
const ctx = canvas.getContext('2d');

// Set the canvas dimensions
canvas.width = window.innerWidth;
canvas.height = window.innerHeight;

// Define the starfield properties
const numStars = 1000;
const starSpeed = 0.1;

let speedMultiplier = 1;

// Create an array to store the star positions and velocities
const stars = [];

// Initialize the star positions and velocities
for (let i = 0; i < numStars; i++) {
    const star = {
        size: 0,
        x: 0,
        y: 0,
        z: 0,
        vx: 0,
        vy: 0,
        vz: 0,
        acceleration: 0,
        velocity: 0,
    }
    resetStar(star);
    stars.push(star);
}


// Update star position and velocity
function updateStar(star) {
    star.velocity += star.acceleration * 10;
    // Update the star's position
    star.x += star.vx * star.velocity;
    star.y += star.vy * star.velocity;
    star.z += star.vz * star.velocity;
    // const newSize = Math.min(20, starSize * (1000 / star.z));
    star.size += star.acceleration/10;
    return star;
}

function resetStar(star) {
    star.x = canvas.width / 2 + (Math.random() * 50 - 25);
    star.y = canvas.height / 4 + (Math.random() * 50 - 25);
    star.z = Math.random() * 100;
    star.vx = Math.random() * 2 - 1;
    star.vy = Math.random() * 2 - 1;
    star.vz = -starSpeed;
    star.velocity = Math.random();
    star.size = Math.random();
    return star;
}


// Main animation loop
function animate() {
    
    // Clear the canvas
    ctx.clearRect(0, 0, canvas.width, canvas.height);

    // Draw the stars
    for (let i = 0; i < numStars; i++) {
        let star = stars[i];
        
        star = updateStar(star);
        
        // Draw the star
        ctx.beginPath();
        ctx.arc(star.x, star.y, star.size, 0, 2 * Math.PI);
        ctx.fillStyle = 'white';
        ctx.fill();

        // Check if the star has moved off the screen
        if (star.x < 0 || star.x > canvas.width || star.y < 0 || star.y > canvas.height) {
            star = resetStar(star);
        }
    }

    // Request the next frame
    requestAnimationFrame(animate);
}

// Start the animation
animate();

// on window resize update the canvas dimensions
window.addEventListener('resize', () => {
    canvas.width = window.innerWidth;
    canvas.height = window.innerHeight;
});

animSwitch.addEventListener('change', () => {
    const canvas = document.getElementById('canvas');
    if (animSwitch.checked) {
        canvas.style.display = 'block';
    } else {
        canvas.style.display = 'none';
    }
});