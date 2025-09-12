import confetti from 'canvas-confetti'

// Play party blower sound from MP3 file
const playPartyBlowerSound = () => {
  try {
    // Create audio element and play the MP3 file
    const audio = new Audio('/party-blower.mp3')
    audio.volume = 0.7 // Set a reasonable volume
    audio.play().catch(error => {
      console.warn('Could not play party blower sound:', error)
    })
  } catch (error) {
    console.warn('Could not play party blower sound:', error)
  }
}

// Create confetti animation from bottom corners
const shootConfetti = () => {
  // Left side confetti - more prominent
  confetti({
    particleCount: 100,
    spread: 120,
    origin: { x: 0, y: 1 },
    colors: ['#8B5CF6', '#A855F7', '#C084FC', '#DDD6FE', '#F3E8FF', '#F59E0B', '#EF4444', '#10B981'],
    shapes: ['circle', 'square', 'star'],
    gravity: 0.8,
    drift: 0.7,
    ticks: 100,
    scalar: 1.2,
    startVelocity: 90
  })
  
  // Right side confetti - more prominent
  confetti({
    particleCount: 100,
    spread: 120,
    origin: { x: 1, y: 1 },
    colors: ['#8B5CF6', '#A855F7', '#C084FC', '#DDD6FE', '#F3E8FF', '#F59E0B', '#EF4444', '#10B981'],
    shapes: ['circle', 'square', 'star'],
    gravity: 0.8,
    drift: -0.7,
    ticks: 100,
    scalar: 1.2,
    startVelocity: 90
  })
}

// Play delete confirmation sound from MP3 file
const playDeleteSound = () => {
  try {
    // Create audio element and play the MP3 file
    // You can replace 'delete-sound.mp3' with your provided MP3 filename
    const audio = new Audio('/delete-sound.mp3')
    audio.volume = 0.6 // Slightly lower volume for delete sound
    audio.play().catch(error => {
      console.warn('Could not play delete sound:', error)
    })
  } catch (error) {
    console.warn('Could not play delete sound:', error)
  }
}

// Main celebration function
export const celebrateTaskCompletion = () => {
  // Play party blower sound
  playPartyBlowerSound()
  
  // Shoot confetti
  shootConfetti()
}

// Play uncheck sound from MP3 file
const playUncheckSoundInternal = () => {
  try {
    // Create audio element and play the MP3 file
    const audio = new Audio('/uncheck-sound.mp3')
    audio.volume = 0.5 // Lower volume for uncheck sound (more subtle)
    audio.play().catch(error => {
      console.warn('Could not play uncheck sound:', error)
    })
  } catch (error) {
    console.warn('Could not play uncheck sound:', error)
  }
}

// Delete confirmation sound function
export const playDeleteConfirmationSound = () => {
  playDeleteSound()
}

// Uncheck sound function
export const playUncheckSound = () => {
  playUncheckSoundInternal()
}
