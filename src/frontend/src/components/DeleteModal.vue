<template>
  <div
    v-if="visible"
    class="modal-backdrop"
  >
    <!-- Backdrop -->
    <div 
      class="modal-overlay"
      @click="handleBackdropClick"
    ></div>
    
    <!-- Modal -->
    <div
      class="modal-container"
      @click.stop
    >
      <!-- Header -->
      <div class="modal-header">
        <h3 class="modal-title">Confirm Delete</h3>
      </div>
      
      <!-- Content -->
      <div class="modal-content">
        <p class="modal-message">{{ message }}</p>
      </div>
      
      <!-- Actions -->
      <div class="modal-actions">
        <button
          @click="handleCancel"
          class="btn-modal-cancel"
        >
          {{ cancelText }}
        </button>
        <button
          @click="handleConfirm"
          class="btn-modal-confirm"
        >
          {{ confirmText }}
        </button>
      </div>
    </div>
  </div>
</template>

<script setup>
import { onMounted, onUnmounted } from 'vue'

// Props
const props = defineProps({
  visible: {
    type: Boolean,
    default: false
  },
  message: {
    type: String,
    default: 'Are you sure you want to delete this item?'
  },
  confirmText: {
    type: String,
    default: 'Delete'
  },
  cancelText: {
    type: String,
    default: 'Cancel'
  }
})

// Emits
const emit = defineEmits(['confirm', 'cancel'])

// Methods
const handleConfirm = () => {
  emit('confirm')
}

const handleCancel = () => {
  emit('cancel')
}

const handleBackdropClick = () => {
  handleCancel()
}

const handleKeydown = (event) => {
  if (!props.visible) return
  
  if (event.key === 'Escape') {
    handleCancel()
  } else if (event.key === 'Enter') {
    handleConfirm()
  }
}

// Keyboard event listeners
onMounted(() => {
  document.addEventListener('keydown', handleKeydown)
})

onUnmounted(() => {
  document.removeEventListener('keydown', handleKeydown)
})
</script>
