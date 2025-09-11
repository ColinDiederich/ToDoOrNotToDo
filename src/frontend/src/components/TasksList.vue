<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <h1 class="text-3xl font-bold text-gray-800 mb-8">Tasks</h1>
      
      <!-- Global Error Banner -->
      <div v-if="globalError" class="mb-4 p-4 bg-red-100 border border-red-400 text-red-700 rounded-lg">
        {{ globalError }}
        <button @click="clearGlobalError" class="ml-2 text-red-500 hover:text-red-700 font-bold">&times;</button>
      </div>
      
      <!-- Loading Spinner -->
      <div v-if="loading" class="flex justify-center items-center py-12">
        <div class="animate-spin rounded-full h-12 w-12 border-b-2 border-blue-600"></div>
      </div>

      <!-- Tasks List -->
      <div v-else class="card">
        <h2 class="text-xl font-semibold text-gray-700 mb-4">Task List</h2>
        
        <!-- No tasks message -->
        <div v-if="allTasks.length === 0" class="text-gray-500 text-center py-8">
          No tasks found.
        </div>
        
        <!-- Tasks with Active/Completed sections -->
        <div v-else>
          <!-- Active Tasks -->
          <div v-if="activeTasks.length > 0" class="space-y-3 mb-6">
            <TaskItem
              v-for="task in activeTasks"
              :key="task.id"
              :task="task"
              :disabled="togglingTasks.has(task.id)"
              @toggle="handleToggle"
              @edit="handleEdit"
              @delete="handleDelete"
            />
          </div>

          <!-- Add New Task Button/Input -->
          <div class="mb-6">
            <!-- Plus Button -->
            <button
              v-if="!isAddingTask"
              @click="startAddingTask"
              :disabled="isCreatingTask"
              class="w-full p-3 border-2 border-dashed border-gray-300 rounded-lg text-gray-500 hover:border-blue-400 hover:text-blue-500 transition-colors disabled:opacity-50 disabled:cursor-not-allowed"
            >
              <span class="text-xl">+</span> Add new task
            </button>
            
            <!-- Text Input -->
            <div v-else class="border border-gray-300 rounded-lg p-3 bg-white">
              <input
                ref="taskInput"
                v-model="newTaskTitle"
                @keydown.enter="saveNewTask"
                @keydown.esc="cancelAddingTask"
                @blur="saveNewTask"
                :disabled="isCreatingTask"
                placeholder="Enter task title..."
                class="w-full border-none outline-none text-gray-700 placeholder-gray-400 disabled:opacity-50"
                maxlength="100"
              />
            </div>
          </div>

          <!-- Divider between Active and Completed -->
          <div v-if="activeTasks.length > 0 && completedTasks.length > 0" class="border-t border-gray-200 my-6"></div>

          <!-- Completed Tasks -->
          <div v-if="completedTasks.length > 0" class="space-y-3">
            <TaskItem
              v-for="task in completedTasks"
              :key="task.id"
              :task="task"
              :disabled="togglingTasks.has(task.id)"
              @toggle="handleToggle"
              @edit="handleEdit"
              @delete="handleDelete"
            />
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import { getTasks, createTask, updateTask } from '../services/api.js'
import TaskItem from './TaskItem.vue'

// Reactive state
const loading = ref(true)
const allTasks = ref([])
const globalError = ref('')
const isAddingTask = ref(false)
const isCreatingTask = ref(false)
const newTaskTitle = ref('')
const taskInput = ref(null)
const isCancelling = ref(false)
const togglingTasks = ref(new Set()) // Track which tasks are currently being toggled

// Computed properties for task separation
const activeTasks = computed(() => 
  allTasks.value.filter(task => !task.isCompleted)
)

const completedTasks = computed(() => 
  allTasks.value.filter(task => task.isCompleted)
)

// Global error handling
const showGlobalError = (message) => {
  globalError.value = message
  // Auto-clear error after 5 seconds
  setTimeout(() => {
    globalError.value = ''
  }, 5000)
}

const clearGlobalError = () => {
  globalError.value = ''
}

// Task input validation
const validateTaskTitle = (title) => {
  const trimmed = title.trim()
  if (!trimmed) {
    return 'Task title cannot be empty'
  }
  if (trimmed.length > 100) {
    return 'Task title must be 100 characters or less'
  }
  return null
}

// Add new task functionality
const startAddingTask = () => {
  isAddingTask.value = true
  newTaskTitle.value = ''
  nextTick(() => {
    taskInput.value?.focus()
  })
}

const cancelAddingTask = () => {
  isCancelling.value = true
  isAddingTask.value = false
  newTaskTitle.value = ''
  clearGlobalError()
  // Reset the cancelling flag after a brief delay to allow blur event to complete
  nextTick(() => {
    isCancelling.value = false
  })
}

const saveNewTask = async () => {
  if (isCreatingTask.value || isCancelling.value) return
  
  const validationError = validateTaskTitle(newTaskTitle.value)
  if (validationError) {
    showGlobalError(validationError)
    return
  }
  
  try {
    isCreatingTask.value = true
    clearGlobalError()
    
    const newTask = await createTask(newTaskTitle.value.trim())
    
    // Add the new task to the list
    allTasks.value.push(newTask)
    
    // Reset input state
    isAddingTask.value = false
    newTaskTitle.value = ''
    
  } catch (error) {
    console.error('Failed to create task:', error)
    showGlobalError(error.message || 'Failed to create task. Please try again.')
  } finally {
    isCreatingTask.value = false
  }
}

// Event handlers
const handleToggle = async (taskId) => {
  // Find the task to get current completion status
  const task = allTasks.value.find(t => t.id === taskId)
  if (!task) {
    console.error('Task not found:', taskId)
    return
  }

  // Immediately disable the toggled row's controls
  togglingTasks.value.add(taskId)

  try {
    // Call API to toggle task completion
    await updateTask({ 
      id: taskId, 
      isCompleted: !task.isCompleted 
    })
    
    // On success, refresh tasks to get updated data from server
    const tasks = await getTasks()
    allTasks.value = tasks || []
    
  } catch (error) {
    console.error('Failed to toggle task:', error)
    showGlobalError(error.message || 'Failed to toggle task. Please try again.')
  } finally {
    // Re-enable the row's controls
    togglingTasks.value.delete(taskId)
  }
}

const handleEdit = (taskId) => {
  console.log('Edit task:', taskId)
  // TODO: Implement edit functionality
}

const handleDelete = (taskId) => {
  console.log('Delete task:', taskId)
  // TODO: Implement API call to delete task
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
