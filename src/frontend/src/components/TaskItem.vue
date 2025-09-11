<template>
  <div 
    :class="[
      'flex items-center p-4 border border-gray-200 rounded-lg transition-colors',
      disabled 
        ? 'bg-gray-100 opacity-60' 
        : 'hover:bg-gray-50'
    ]"
  >
    <!-- Checkbox on left -->
    <div class="flex items-center gap-3 flex-1">
      <input
        type="checkbox"
        :checked="task.isCompleted"
        :disabled="disabled"
        @click="!disabled && $emit('toggle', task.id)"
        :class="[
          'w-4 h-4 text-blue-600 bg-gray-100 border-gray-300 rounded focus:ring-blue-500 focus:ring-2',
          disabled ? 'cursor-not-allowed' : 'cursor-pointer'
        ]"
      />
      
      <!-- Title text (strikethrough if completed) -->
      <span
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
      @click="!disabled && $emit('delete', task.id)"
      :disabled="disabled"
      :class="[
        'p-2 rounded-md transition-colors',
        disabled 
          ? 'text-gray-300 cursor-not-allowed' 
          : 'text-gray-400 hover:text-red-500 hover:bg-red-50'
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
  }
})

// Emits
const emit = defineEmits(['toggle', 'edit', 'delete'])

// Methods
const handleTitleClick = () => {
  // Only emit edit event if task is not completed
  if (!props.task.isCompleted) {
    emit('edit', props.task.id)
  }
}
</script>
