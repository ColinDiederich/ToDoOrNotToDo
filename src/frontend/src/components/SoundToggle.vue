<template>
  <div class="sound-toggle-container">
    <button
      @click="toggleSound"
      class="sound-toggle-btn"
      :class="{ 'sound-enabled': soundEnabled }"
      :title="soundEnabled ? 'Disable sound effects' : 'Enable sound effects'"
    >
      <svg 
        v-if="soundEnabled" 
        class="sound-icon" 
        width="20" 
        height="20" 
        viewBox="0 0 24 24" 
        fill="none" 
        stroke="currentColor"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M15.536 8.464a5 5 0 010 7.072m2.828-9.9a9 9 0 010 12.728M5.586 15H4a1 1 0 01-1-1v-4a1 1 0 011-1h1.586l4.707-4.707C10.923 4.663 12 5.109 12 6v12c0 .891-1.077 1.337-1.707.707L5.586 15z" />
      </svg>
      <svg 
        v-else 
        class="sound-icon" 
        width="20" 
        height="20" 
        viewBox="0 0 24 24" 
        fill="none" 
        stroke="currentColor"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5.586 15H4a1 1 0 01-1-1v-4a1 1 0 011-1h1.586l4.707-4.707C10.923 4.663 12 5.109 12 6v12c0 .891-1.077 1.337-1.707.707L5.586 15z" />
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M17 14l2-2m0 0l2-2m-2 2l-2-2m2 2l2 2" />
      </svg>
    </button>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

// Emits
const emit = defineEmits(['sound-toggle'])

// Reactive state
const soundEnabled = ref(true)

// Load sound preference on mount
onMounted(() => {
  loadSoundPreference()
})

// Load sound preference from localStorage
const loadSoundPreference = () => {
  try {
    const saved = localStorage.getItem('userPreferences')
    if (saved) {
      const preferences = JSON.parse(saved)
      if (preferences.sound !== undefined) {
        soundEnabled.value = preferences.sound
      }
    }
  } catch (error) {
    console.warn('Failed to load sound preference:', error)
  }
}

// Save sound preference to localStorage
const saveSoundPreference = () => {
  try {
    const existing = localStorage.getItem('userPreferences')
    let preferences = {}
    
    if (existing) {
      preferences = JSON.parse(existing)
    }
    
    preferences.sound = soundEnabled.value
    localStorage.setItem('userPreferences', JSON.stringify(preferences))
  } catch (error) {
    console.warn('Failed to save sound preference:', error)
  }
}

// Toggle sound effects
const toggleSound = () => {
  soundEnabled.value = !soundEnabled.value
  saveSoundPreference()
  emit('sound-toggle', soundEnabled.value)
}
</script>
