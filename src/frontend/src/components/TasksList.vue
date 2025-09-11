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
              :is-editing="editingTasks.has(task.id)"
              :edit-value="editValues[task.id] || task.title"
              @toggle="handleToggle"
              @edit="handleEdit"
              @save-edit="handleSaveEdit"
              @cancel-edit="handleCancelEdit"
              @update-edit-value="handleUpdateEditValue"
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
    
    <!-- Delete Confirmation Modal -->
    <DeleteModal
      :visible="showDeleteModal"
      :message="deleteMessage"
      confirm-text="Delete"
      cancel-text="Cancel"
      @confirm="confirmDelete"
      @cancel="cancelDelete"
    />
  </div>
</template>

<script setup>
import { ref, computed, onMounted, nextTick } from 'vue'
import { getTasks, createTask, updateTask, deleteTask } from '../services/api.js'
import TaskItem from './TaskItem.vue'
import DeleteModal from './DeleteModal.vue'

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
const editingTasks = ref(new Set()) // Track which tasks are currently being edited
const editValues = ref({}) // Store edit input values for each task

// Delete modal state
const showDeleteModal = ref(false)
const taskToDelete = ref(null)
const isDeleting = ref(false)

// Computed properties for task separation
const activeTasks = computed(() => 
  allTasks.value.filter(task => !task.isCompleted)
)

const completedTasks = computed(() => 
  allTasks.value.filter(task => task.isCompleted)
)

const deleteMessage = computed(() => {
  try {
    if (!taskToDelete.value || !taskToDelete.value.title) {
      return ''
    }
    return `Are you sure you want to delete "${taskToDelete.value.title}"?`
  } catch (error) {
    console.warn('Error computing deleteMessage:', error)
    return ''
  }
})

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
    isAddingTask.value = false
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
  const task = allTasks.value.find(t => t.id === taskId)
  if (!task || task.isCompleted) {
    return // Don't allow editing completed tasks
  }
  
  // Enter edit mode
  editingTasks.value.add(taskId)
  editValues.value[taskId] = task.title
}

const handleUpdateEditValue = (taskId, value) => {
  editValues.value[taskId] = value
}

const handleSaveEdit = async (taskId, value = null) => {
  const task = allTasks.value.find(t => t.id === taskId)
  if (!task) return
  
  // Use the passed value if available, otherwise fall back to reactive state
  const newTitle = (value || editValues.value[taskId] || '').trim()
  
  // If title is empty, treat as cancel (like pressing Escape)
  if (!newTitle) {
    handleCancelEdit(taskId)
    return
  }
  
  // Client validation for non-empty titles
  const validationError = validateTaskTitle(newTitle)
  if (validationError) {
    showGlobalError(validationError)
    return
  }
  
  // If title hasn't changed, just cancel edit
  if (newTitle === task.title) {
    handleCancelEdit(taskId)
    return
  }
  
  // Disable the row while saving
  editingTasks.value.add(taskId)
  
  try {
    // Call API to update task title
    await updateTask({ 
      id: taskId, 
      title: newTitle 
    })
    
    // On success, refresh tasks to get updated data from server
    const tasks = await getTasks()
    allTasks.value = tasks || []
    
  } catch (error) {
    console.error('Failed to update task:', error)
    showGlobalError(error.message || 'Failed to update task. Please try again.')
  } finally {
    // Exit edit mode
    editingTasks.value.delete(taskId)
    delete editValues.value[taskId]
  }
}

const handleCancelEdit = (taskId) => {
  // Exit edit mode and revert changes
  editingTasks.value.delete(taskId)
  delete editValues.value[taskId]
}

const handleDelete = (taskId) => {
  const task = allTasks.value.find(t => t.id === taskId)
  if (!task) return
  
  taskToDelete.value = task
  showDeleteModal.value = true
}

const confirmDelete = async () => {
  if (!taskToDelete.value || isDeleting.value) return
  
  try {
    isDeleting.value = true
    clearGlobalError()
    
    // Call API to delete task
    await deleteTask(taskToDelete.value.id)
    
    // Refresh tasks to get updated list from server
    const tasks = await getTasks()
    allTasks.value = tasks || []
    
  } catch (error) {
    console.error('Failed to delete task:', error)
    showGlobalError(error.message || 'Failed to delete task. Please try again.')
  } finally {
    isDeleting.value = false
    showDeleteModal.value = false
    taskToDelete.value = null
  }
}

const cancelDelete = () => {
  showDeleteModal.value = false
  taskToDelete.value = null
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
