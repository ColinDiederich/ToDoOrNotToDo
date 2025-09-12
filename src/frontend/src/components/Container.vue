<template>
  <div class="container-main">
    <div class="content-wrapper">
      <h1 class="text-title mb-8">To Do or Not To Do</h1>
      
      <!-- Search Input -->
      <SearchInput
        :all-tasks="allTasks"
        :active-tasks="activeTasks"
        :completed-tasks="completedTasks"
        @search-change="handleSearchChange"
      />
      
      <!-- Loading Spinner -->
      <div v-if="loading" class="loading-container">
        <div class="loading-spinner"></div>
      </div>

      <!-- Tasks List -->
      <TasksList
        v-else
        :all-tasks="allTasks"
        :active-tasks="activeTasks"
        :completed-tasks="completedTasks"
        :search-query="searchQuery"
        :filtered-tasks="filteredTasks"
        :filtered-active-tasks="filteredActiveTasks"
        :filtered-completed-tasks="filteredCompletedTasks"
        @refresh-tasks="refreshTasks"
        @create-task="handleCreateTask"
      />
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { getTasks, createTask } from '../services/api.js'
import { showError } from '../services/eventBus.js'
import SearchInput from './SearchInput.vue'
import TasksList from './TasksList.vue'

// Reactive state
const loading = ref(true)
const allTasks = ref([])
const isCreatingTask = ref(false)

// Search state
const searchQuery = ref('')
const filteredTasks = ref([])
const filteredActiveTasks = ref([])
const filteredCompletedTasks = ref([])


// Computed properties for task separation
const activeTasks = computed(() => 
  filteredActiveTasks.value.length > 0 ? filteredActiveTasks.value : allTasks.value.filter(task => !task.isCompleted)
)

const completedTasks = computed(() => 
  filteredCompletedTasks.value.length > 0 ? filteredCompletedTasks.value : allTasks.value.filter(task => task.isCompleted)
)

// Global error handling using event bus
const showGlobalError = (message) => {
  showError(message)
}

// Search functionality
const handleSearchChange = (searchData) => {
  searchQuery.value = searchData.query
  filteredTasks.value = searchData.filteredTasks
  filteredActiveTasks.value = searchData.filteredActiveTasks
  filteredCompletedTasks.value = searchData.filteredCompletedTasks
}

// Handle task creation from TasksList
const handleCreateTask = async (title) => {
  try {
    isCreatingTask.value = true
    
    const newTask = await createTask(title)
    
    // Add the new task to the list
    allTasks.value.push(newTask)
    
  } catch (error) {
    console.error('Failed to create task:', error)
    showGlobalError(error.message || 'Failed to create task. Please try again.')
  } finally {
    isCreatingTask.value = false
  }
}

// Refresh tasks from server
const refreshTasks = async () => {
  try {
    const tasks = await getTasks()
    allTasks.value = tasks || []
  } catch (error) {
    console.error('Failed to refresh tasks:', error)
    showGlobalError('Failed to refresh tasks. Please try again.')
  }
}

// Load tasks on component mount
onMounted(async () => {
  try {
    loading.value = true
    const tasks = await getTasks()
    allTasks.value = tasks || []
  } catch (error) {
    console.error('Failed to load tasks:', error)
    allTasks.value = []
    showGlobalError('Failed to load tasks. Please refresh the page.')
  } finally {
    loading.value = false
  }
})
</script>