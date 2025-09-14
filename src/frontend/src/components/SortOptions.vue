<template>
  <div class="sort-options-container">
    <!-- Sort Label -->
    <span class="sort-label">Sort:</span>
    
    <!-- Custom Sort By Dropdown -->
    <div class="custom-dropdown" @click="toggleDropdown" :class="{ 'dropdown-open': isDropdownOpen }">
      <div class="dropdown-selected">
        <span>{{ getSelectedLabel() }}</span>
        <svg class="dropdown-icon" width="16" height="16" viewBox="0 0 24 24" fill="none" stroke="currentColor">
          <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M4 6h16M4 12h16M4 18h16" />
        </svg>
      </div>
      
      <div v-if="isDropdownOpen" class="dropdown-options">
        <div 
          v-for="option in sortOptions" 
          :key="option.value"
          @click="selectOption(option.value)"
          class="dropdown-option"
        >
          {{ option.label }}
        </div>
      </div>
    </div>
    
    <!-- Sort Order Toggle Button -->
    <button
      @click="toggleSortOrder"
      class="sort-toggle-btn"
      :title="sortOrder === 'asc' ? 'Ascending' : 'Descending'"
    >
      <svg 
        v-if="sortOrder === 'asc'" 
        class="w-4 h-4" 
        fill="none" 
        stroke="currentColor" 
        viewBox="0 0 24 24"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M5 15l7-7 7 7" />
      </svg>
      <svg 
        v-else 
        class="w-4 h-4" 
        fill="none" 
        stroke="currentColor" 
        viewBox="0 0 24 24"
      >
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 9l-7 7-7-7" />
      </svg>
    </button>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

// Emits
const emit = defineEmits(['sort-change'])

// Reactive state
const sortBy = ref('dateCreated')
const sortOrder = ref('asc') // 'asc' = Oldest First, 'desc' = Newest First
const isDropdownOpen = ref(false)

// Sort options data
const sortOptions = [
  { value: 'dateCreated', label: 'Date Created' },
  { value: 'dateModified', label: 'Date Modified' },
  { value: 'title', label: 'Title' }
]

// Load saved preferences on mount
onMounted(() => {
  loadSortPreferences()
})

// Load sort preferences from localStorage
const loadSortPreferences = () => {
  try {
    const saved = localStorage.getItem('userPreferences')
    if (saved) {
      const preferences = JSON.parse(saved)
      if (preferences.sorting) {
        sortBy.value = preferences.sorting.sortBy || 'dateCreated'
        sortOrder.value = preferences.sorting.sortOrder || 'asc'
      }
    }
  } catch (error) {
    console.warn('Failed to load sort preferences:', error)
  }
}

// Close dropdown when clicking outside
const closeDropdownOnOutsideClick = (event) => {
  if (!event.target.closest('.custom-dropdown')) {
    isDropdownOpen.value = false
  }
}

// Add event listener for outside clicks
onMounted(() => {
  document.addEventListener('click', closeDropdownOnOutsideClick)
})

onUnmounted(() => {
  document.removeEventListener('click', closeDropdownOnOutsideClick)
})

// Save sort preferences to localStorage
const saveSortPreferences = () => {
  try {
    const preferences = {
      sorting: {
        sortBy: sortBy.value,
        sortOrder: sortOrder.value
      }
    }
    localStorage.setItem('userPreferences', JSON.stringify(preferences))
  } catch (error) {
    console.warn('Failed to save sort preferences:', error)
  }
}

// Custom dropdown methods
const toggleDropdown = () => {
  isDropdownOpen.value = !isDropdownOpen.value
}

const selectOption = (value) => {
  sortBy.value = value
  isDropdownOpen.value = false
  emitSortChange()
}

const getSelectedLabel = () => {
  const option = sortOptions.find(opt => opt.value === sortBy.value)
  return option ? option.label : 'Date Created'
}

// Toggle sort order
const toggleSortOrder = () => {
  sortOrder.value = sortOrder.value === 'asc' ? 'desc' : 'asc'
  emitSortChange()
}

// Emit sort change event
const emitSortChange = () => {
  saveSortPreferences()
  emit('sort-change', {
    sortBy: sortBy.value,
    sortOrder: sortOrder.value
  })
}
</script>
