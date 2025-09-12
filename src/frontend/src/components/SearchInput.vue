<template>
  <div class="search-container">
    <div class="search-input-wrapper">
      <div class="search-icon">
        <svg width="20" height="20" viewBox="0 0 24 24" fill="none" xmlns="http://www.w3.org/2000/svg">
          <path d="M21 21L16.514 16.506L21 21ZM19 10.5C19 15.194 15.194 19 10.5 19C5.806 19 2 15.194 2 10.5C2 5.806 5.806 2 10.5 2C15.194 2 19 5.806 19 10.5Z" stroke="currentColor" stroke-width="2" stroke-linecap="round" stroke-linejoin="round"/>
        </svg>
      </div>
      <input
        v-model="searchQuery"
        @input="onSearchInput"
        placeholder="Search tasks..."
        class="search-input"
        maxlength="100"
      />
      <button
        v-if="searchQuery"
        @click="clearSearch"
        class="search-clear-btn"
        type="button"
        aria-label="Clear search"
      >
        <span class="text-lg">×</span>
      </button>
    </div>
    <div v-if="searchQuery" class="search-results-info">
      <span class="text-sm text-gray-600">
        {{ getSearchResultsText() }}
      </span>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, watch } from 'vue'

// Props
const props = defineProps({
  allTasks: {
    type: Array,
    default: () => []
  },
  activeTasks: {
    type: Array,
    default: () => []
  },
  completedTasks: {
    type: Array,
    default: () => []
  }
})

// Emits
const emit = defineEmits(['search-change'])

// Reactive state
const searchQuery = ref('')

// Computed properties for search filtering
const filteredTasks = computed(() => {
  if (!searchQuery.value.trim()) {
    return props.allTasks
  }
  
  const query = searchQuery.value.toLowerCase().trim()
  return props.allTasks.filter(task => 
    task.title.toLowerCase().includes(query)
  )
})

const filteredActiveTasks = computed(() => 
  filteredTasks.value.filter(task => !task.isCompleted)
)

const filteredCompletedTasks = computed(() => 
  filteredTasks.value.filter(task => task.isCompleted)
)

// Search functionality
const onSearchInput = () => {
  // Emit the search change event with filtered results
  emit('search-change', {
    query: searchQuery.value,
    filteredTasks: filteredTasks.value,
    filteredActiveTasks: filteredActiveTasks.value,
    filteredCompletedTasks: filteredCompletedTasks.value
  })
}

const clearSearch = () => {
  searchQuery.value = ''
  onSearchInput() // Trigger the search change event
}

const getSearchResultsText = () => {
  const totalResults = filteredTasks.value.length
  const activeResults = filteredActiveTasks.value.length
  const completedResults = filteredCompletedTasks.value.length
  
  if (totalResults === 0) {
    return `No tasks found for "${searchQuery.value}"`
  }
  
  let resultText = `Found ${totalResults} task${totalResults === 1 ? '' : 's'}`
  if (activeResults > 0 && completedResults > 0) {
    resultText += ` (${activeResults} active, ${completedResults} completed)`
  } else if (activeResults > 0) {
    resultText += ` (${activeResults} active)`
  } else if (completedResults > 0) {
    resultText += ` (${completedResults} completed)`
  }
  
  return resultText
}

// Watch for changes in allTasks to update search results
watch(() => props.allTasks, () => {
  onSearchInput()
}, { deep: true })
</script>