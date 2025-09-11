<template>
  <div class="container-main">
    <div class="content-wrapper">
      <h1 class="text-title mb-8">To Do or Not To Do</h1>
      
      <!-- Loading Spinner -->
      <div v-if="loading" class="loading-container">
        <div class="loading-spinner"></div>
      </div>

      <!-- Tasks List -->
      <div v-else class="tasks-container">
        <!-- Tasks with Active/Completed sections -->
        <div class="tasks-content" :class="{ 'tasks-content-disabled': isCreatingTask || isDeleting }">
          <!-- No tasks message -->
          <div v-if="allTasks.length === 0" class="text-no-tasks">
            No tasks found.
          </div>
          <!-- Active Tasks -->
          <div v-else class="tasks-section mb-6">
            <TransitionGroup
              name="task-slide"
              tag="div"
              class="tasks-section"
            >
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
            </TransitionGroup>
          </div>

          <!-- Add New Task Button/Input -->
          <div class="add-task-container">
            <!-- Plus Button -->
            <button
              v-if="!isAddingTask"
              @click="startAddingTask"
              :disabled="isCreatingTask || isDeleting"
              class="btn-add-task"
            >
              <span class="text-2xl">+</span> Add new task
            </button>
            
            <!-- Text Input -->
            <div v-else class="add-task-input-container">
              <input
                ref="taskInput"
                v-model="newTaskTitle"
                @keydown.enter="saveNewTask"
                @keydown.esc="cancelAddingTask"
                @blur="saveNewTask"
                :disabled="isCreatingTask || isDeleting"
                placeholder="Enter task title..."
                class="input-task"
                maxlength="500"
              />
            </div>
          </div>
          
          <!-- Completed Tasks -->
          <div v-if="completedTasks.length > 0" class="tasks-section-completed">
            <TransitionGroup
              name="task-slide"
              tag="div"
              class="tasks-section"
            >
              <TaskItem
                v-for="task in completedTasks"
                :key="task.id"
                :task="task"
                :disabled="togglingTasks.has(task.id)"
                @toggle="handleToggle"
                @edit="handleEdit"
                @delete="handleDelete"
              />
            </TransitionGroup>
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
import { showError } from '../services/eventBus.js'
import TaskItem from './TaskItem.vue'
import DeleteModal from './DeleteModal.vue'

// Reactive state
const loading = ref(true)
const allTasks = ref([])
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
    return `Are you sure you want to delete: "${taskToDelete.value.title}"?`
  } catch (error) {
    console.warn('Error computing deleteMessage:', error)
    return ''
  }
})

// Global error handling using event bus
const showGlobalError = (message) => {
  showError(message)
}

// Task input validation
const validateTaskTitle = (title) => {
  const trimmed = title.trim()
  if (!trimmed) {
    return 'Task title cannot be empty'
  }
  if (trimmed.length > 500) {
    return 'Task title must be 500 characters or less'
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


<style scoped>
.no-caret {
  caret-color: transparent;
}

/* Task sliding animations */
.task-slide-enter-active,
.task-slide-leave-active {
  transition: all 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
}

.task-slide-enter-from {
  opacity: 0;
  transform: translateX(-30px) scale(0.95);
}

.task-slide-leave-to {
  opacity: 0;
  transform: translateX(30px) scale(0.95);
}

.task-slide-move {
  transition: transform 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
}

/* Ensure smooth transitions for task items */
.task-slide-enter-active .flex,
.task-slide-leave-active .flex {
  transition: all 0.4s cubic-bezier(0.25, 0.8, 0.25, 1);
}
</style>