<template>
  <div 
    :class="[
      'task-item-base',
      disabled 
        ? 'task-item-disabled' 
        : isEditing
          ? 'task-item-editing'
          : 'task-item-interactive'
    ]"
  >
    <!-- Checkbox on left -->
    <div class="task-content">
      <input
        type="checkbox"
        :checked="task.isCompleted"
        :disabled="disabled"
        @click="!disabled && !isEditing && $emit('toggle', task.id)"
        :class="[
          'task-checkbox',
          disabled ? 'task-checkbox-disabled' : 'task-checkbox-interactive'
        ]"
      />
      
      <!-- Clickable area for editing - entire space between checkbox and trash icon -->
      <div 
        v-if="!isEditing"
        @click="!disabled && !task.isCompleted && handleTitleClick()"
        :class="[
          'task-title-area',
          disabled 
            ? 'task-title-area-disabled' 
            : task.isCompleted 
              ? 'task-title-area-completed' 
              : ''
        ]"
      >
        <span
          :class="[
            'select-none overflow-hidden text-wrap-anywhere',
            disabled 
              ? 'text-task-disabled' 
              : task.isCompleted 
                ? 'text-task-completed' 
                : 'text-task-active'
          ]"
        >
          {{ task.title }}
        </span>
      </div>
      
      <!-- Edit input -->
      <div v-else class="task-edit-area">
        <textarea
          :value="editValue"
          @keydown.enter="handleEnter"
          @keydown.esc="handleEscape"
          @blur="handleBlur"
          @focus="handleFocus"
          @input="autoResize"
          class="textarea-edit"
          maxlength="500"
          ref="editInput"
          rows="1"
        />
      </div>
    </div>
    
    <!-- Trash icon on right -->
    <button
      @click="!disabled && !isEditing && $emit('delete', task.id)"
      :disabled="disabled"
      :class="[
        'task-delete-btn',
        (disabled)
          ? 'task-delete-btn-disabled' 
          : 'task-delete-btn-interactive'
      ]"
      :title="disabled ? 'Task is being updated...' : 'Delete task'"
    >
      <svg class="w-5 h-5" fill="none" stroke="currentColor" viewBox="0 0 24 24">
        <path stroke-linecap="round" stroke-linejoin="round" stroke-width="2" d="M19 7l-.867 12.142A2 2 0 0116.138 21H7.862a2 2 0 01-1.995-1.858L5 7m5 4v6m4-6v6m1-10V4a1 1 0 00-1-1h-4a1 1 0 00-1 1v3M4 7h16" />
      </svg>
    </button>
  </div>
</template>

<script setup>
import { ref, watch, nextTick } from 'vue'

// Props
const props = defineProps({
  task: {
    type: Object,
    required: true,
    validator: (value) => {
      return value && 
             typeof value.id !== 'undefined' && 
             typeof value.title === 'string' && 
             typeof value.isCompleted === 'boolean'
    }
  },
  disabled: {
    type: Boolean,
    default: false
  },
  isEditing: {
    type: Boolean,
    default: false
  },
  editValue: {
    type: String,
    default: ''
  }
})

// Emits
const emit = defineEmits(['toggle', 'edit', 'delete', 'save-edit', 'cancel-edit', 'update-edit-value'])

// Refs
const editInput = ref(null)
const isHandlingKeypress = ref(false)

// Methods
const handleTitleClick = () => {
  // Only emit edit event if task is not completed
  if (!props.task.isCompleted) {
    emit('edit', props.task.id)
  }
}

const handleEnter = (event) => {
  // If Shift is pressed, allow line break (default behavior)
  if (event.shiftKey) {
    return
  }
  
  // If Enter without Shift, save the task
  event.preventDefault()
  // Set flag to prevent blur event from interfering
  isHandlingKeypress.value = true
  // Capture the current input value and pass it directly to save
  const currentValue = event.target.value || ''
  emit('update-edit-value', props.task.id, currentValue)
  // Pass the value directly to avoid timing issues
  emit('save-edit', props.task.id, currentValue)
}

const handleEscape = () => {
  // Set flag to prevent blur event from interfering
  isHandlingKeypress.value = true
  // Cancel the edit
  emit('cancel-edit', props.task.id)
}

const handleBlur = () => {
  // Skip blur handling if we're already handling Enter key or Escape key
  if (isHandlingKeypress.value) {
    isHandlingKeypress.value = false
    return
  }
  
  // Capture the current input value directly to avoid timing issues
  const currentValue = editInput.value?.value || ''
  emit('update-edit-value', props.task.id, currentValue)
  // Pass the value directly to avoid timing issues
  emit('save-edit', props.task.id, currentValue)
}

const handleFocus = () => {
  // Focus handler - no special logic needed
}

// Auto-resize textarea based on content
const autoResize = () => {
  const textarea = editInput.value
  if (textarea) {
    textarea.style.height = 'auto'
    textarea.style.height = textarea.scrollHeight + 'px'
  }
}

// Auto-focus edit input when entering edit mode
watch(() => props.isEditing, async (isEditing) => {
  if (isEditing) {
    await nextTick()
    editInput.value?.focus()
    // Position cursor at the end of the text instead of selecting all
    const input = editInput.value
    if (input) {
      const length = input.value.length
      input.setSelectionRange(length, length)
      // Auto-resize the textarea to fit content
      autoResize()
    }
  }
})
</script>
