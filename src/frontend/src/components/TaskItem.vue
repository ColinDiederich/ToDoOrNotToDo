<template>
  <div 
    :class="[
      'flex items-center p-4 border border-gray-200 rounded-lg transition-colors',
      disabled 
        ? 'bg-gray-100 opacity-60' 
        : isEditing
          ? 'bg-blue-50 border-blue-300'
          : 'hover:bg-gray-50'
    ]"
  >
    <!-- Checkbox on left -->
    <div class="flex items-center gap-3 flex-1">
      <input
        type="checkbox"
        :checked="task.isCompleted"
        :disabled="disabled || isEditing"
        @click="!disabled && !isEditing && $emit('toggle', task.id)"
        :class="[
          'w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 focus:ring-2',
          (disabled || isEditing) ? 'cursor-not-allowed' : 'cursor-pointer'
        ]"
      />
      
      <!-- Title text or edit input -->
      <div v-if="isEditing" class="flex-1">
        <input
          :value="editValue"
          @keydown.enter="handleEnter"
          @keydown.esc="handleEscape"
          @blur="handleBlur"
          @focus="handleFocus"
          class="w-full text-lg border-none outline-none bg-transparent text-gray-800"
          maxlength="100"
          ref="editInput"
        />
      </div>
      <span
        v-else
        @click="!disabled && handleTitleClick()"
        :class="[
          'text-lg select-none',
          disabled 
            ? 'cursor-not-allowed text-gray-400' 
            : task.isCompleted 
              ? 'line-through text-gray-500 cursor-pointer' 
              : 'text-gray-800 hover:text-blue-600 cursor-pointer'
        ]"
      >
        {{ task.title }}
      </span>
    </div>
    
    <!-- Trash icon on right -->
    <button
      @click="!disabled && !isEditing && $emit('delete', task.id)"
      :disabled="disabled || isEditing"
      :class="[
        'p-2 rounded-md transition-colors',
        (disabled || isEditing)
          ? 'text-gray-300 cursor-not-allowed' 
          : 'text-gray-400 hover:text-red-500 hover:bg-red-50'
      ]"
      :title="disabled ? 'Task is being updated...' : isEditing ? 'Cannot delete while editing' : 'Delete task'"
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
    }
  }
})
</script>
