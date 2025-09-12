<template>
  <div class="tasks-container">
    <!-- Tasks with Active/Completed sections -->
    <div class="tasks-content" :class="{ 'tasks-content-disabled': isCreatingTask || isDeleting }">
      <!-- No tasks message -->
      <div v-if="allTasks.length === 0" class="text-no-tasks">
        No tasks found.
      </div>
      <!-- No search results message -->
      <div v-else-if="searchQuery && sortedFilteredTasks.length === 0" class="text-no-tasks">
        No tasks found for "{{ searchQuery }}".
      </div>
      <!-- Active Tasks -->
      <div v-else class="tasks-section mb-6">
        <TransitionGroup
          name="task-slide"
          tag="div"
          class="tasks-section"
        >
          <TaskItem
            v-for="task in sortedActiveTasks"
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
            :value="newTaskTitle"
            @keydown.enter="saveNewTask"
            @keydown.esc="cancelAddingTask"
            @blur="saveNewTask"
            @input="updateNewTaskTitle"
            :disabled="isCreatingTask || isDeleting"
            placeholder="Enter task title..."
            class="input-task"
            maxlength="500"
          />
        </div>
      </div>
      
      <!-- Completed Tasks -->
      <div v-if="sortedCompletedTasks.length > 0" class="tasks-section-completed">
        <TransitionGroup
          name="task-slide"
          tag="div"
          class="tasks-section"
        >
          <TaskItem
            v-for="task in sortedCompletedTasks"
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
import { ref, watch, nextTick, computed } from 'vue'
import { updateTask, deleteTask } from '../services/api.js'
import { showError } from '../services/eventBus.js'
import { celebrateTaskCompletion, playUncheckSound } from '../services/effects.js'
import TaskItem from './TaskItem.vue'
import DeleteModal from './DeleteModal.vue'

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
  },
  searchQuery: {
    type: String,
    default: ''
  },
  filteredTasks: {
    type: Array,
    default: () => []
  },
  filteredActiveTasks: {
    type: Array,
    default: () => []
  },
  filteredCompletedTasks: {
    type: Array,
    default: () => []
  },
  loading: {
    type: Boolean,
    default: false
  },
  isCreatingTask: {
    type: Boolean,
    default: false
  },
  isDeleting: {
    type: Boolean,
    default: false
  },
  togglingTasks: {
    type: Set,
    default: () => new Set()
  },
  editingTasks: {
    type: Set,
    default: () => new Set()
  },
  editValues: {
    type: Object,
    default: () => ({})
  },
  isAddingTask: {
    type: Boolean,
    default: false
  },
  newTaskTitle: {
    type: String,
    default: ''
  },
  sortBy: {
    type: String,
    default: 'dateCreated'
  },
  sortOrder: {
    type: String,
    default: 'asc'
  }
})

// Emits
const emit = defineEmits([
  'refresh-tasks',
  'create-task'
])

// Refs
const taskInput = ref(null)

// Task modification state
const togglingTasks = ref(new Set())
const editingTasks = ref(new Set())
const editValues = ref({})
const isAddingTask = ref(false)
const newTaskTitle = ref('')
const isCreatingTask = ref(false)

// Delete modal state
const showDeleteModal = ref(false)
const taskToDelete = ref(null)
const isDeleting = ref(false)

// Computed properties
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

// Task modification logic
const handleToggle = async (taskId) => {
  // Find the task to get current completion status
  const task = props.allTasks.find(t => t.id === taskId)
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
    
    // Emit refresh event to parent
    emit('refresh-tasks')
    
    // Play appropriate sound based on action
    if (!task.isCompleted) {
      // Task was completed - play celebration sound
      celebrateTaskCompletion()
    } else {
      // Task was unchecked - play uncheck sound
      playUncheckSound()
    }
    
  } catch (error) {
    console.error('Failed to toggle task:', error)
    showError(error.message || 'Failed to toggle task. Please try again.')
  } finally {
    // Re-enable the row's controls
    togglingTasks.value.delete(taskId)
  }
}

const handleEdit = (taskId) => {
  const task = props.allTasks.find(t => t.id === taskId)
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
  const task = props.allTasks.find(t => t.id === taskId)
  if (!task) return
  
  // Use the passed value if available, otherwise fall back to reactive state
  const newTitle = (value || editValues.value[taskId] || '').trim()
  
  // If title is empty, treat as cancel (like pressing Escape)
  if (!newTitle) {
    handleCancelEdit(taskId)
    return
  }
  
  // Client validation for non-empty titles
  if (newTitle.length > 500) {
    showError('Task title must be 500 characters or less')
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
    
    // Emit refresh event to parent
    emit('refresh-tasks')
    
  } catch (error) {
    console.error('Failed to update task:', error)
    showError(error.message || 'Failed to update task. Please try again.')
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
  const task = props.allTasks.find(t => t.id === taskId)
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
    
    // Emit refresh event to parent
    emit('refresh-tasks')
    
  } catch (error) {
    console.error('Failed to delete task:', error)
    showError(error.message || 'Failed to delete task. Please try again.')
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

const startAddingTask = () => {
  isAddingTask.value = true
  newTaskTitle.value = ''
  nextTick(() => {
    taskInput.value?.focus()
  })
}

const saveNewTask = async () => {
  if (isCreatingTask.value) return
  
  const validationError = validateTaskTitle(newTaskTitle.value)
  if (validationError) {
    isAddingTask.value = false
    return
  }
  
  try {
    isCreatingTask.value = true
    
    // Emit to parent to create task
    emit('create-task', newTaskTitle.value.trim())
    
    // Reset input state
    isAddingTask.value = false
    newTaskTitle.value = ''
    
  } catch (error) {
    console.error('Failed to create task:', error)
    showError(error.message || 'Failed to create task. Please try again.')
  } finally {
    isCreatingTask.value = false
  }
}

const cancelAddingTask = () => {
  isAddingTask.value = false
  newTaskTitle.value = ''
}

const updateNewTaskTitle = (event) => {
  newTaskTitle.value = event.target.value
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

// Sort function
const sortTasks = (tasks, sortBy, sortOrder) => {
  if (!tasks || tasks.length === 0) return tasks
  
  return [...tasks].sort((a, b) => {
    let comparison = 0
    
    switch (sortBy) {
      case 'title':
        comparison = a.title.localeCompare(b.title)
        break
      case 'dateCreated':
        comparison = new Date(a.createdAt) - new Date(b.createdAt)
        break
      case 'dateModified':
        comparison = new Date(a.updatedAt) - new Date(b.updatedAt)
        break
      default:
        comparison = new Date(a.createdAt) - new Date(b.createdAt)
    }
    
    return sortOrder === 'asc' ? comparison : -comparison
  })
}

// Computed properties for sorted tasks
const sortedActiveTasks = computed(() => {
  // Use filtered tasks when search is active, otherwise use regular active tasks
  const tasksToSort = props.searchQuery && props.filteredActiveTasks.length > 0 
    ? props.filteredActiveTasks 
    : props.activeTasks
  return sortTasks(tasksToSort, props.sortBy, props.sortOrder)
})

const sortedCompletedTasks = computed(() => {
  // Use filtered tasks when search is active, otherwise use regular completed tasks
  const tasksToSort = props.searchQuery && props.filteredCompletedTasks.length > 0 
    ? props.filteredCompletedTasks 
    : props.completedTasks
  return sortTasks(tasksToSort, props.sortBy, props.sortOrder)
})

const sortedFilteredTasks = computed(() => {
  return sortTasks(props.filteredTasks, props.sortBy, props.sortOrder)
})

const sortedFilteredActiveTasks = computed(() => {
  return sortTasks(props.filteredActiveTasks, props.sortBy, props.sortOrder)
})

const sortedFilteredCompletedTasks = computed(() => {
  return sortTasks(props.filteredCompletedTasks, props.sortBy, props.sortOrder)
})

// Watch for isAddingTask to focus the input
watch(() => props.isAddingTask, async (isAdding) => {
  if (isAdding) {
    await nextTick()
    taskInput.value?.focus()
  }
})
</script>