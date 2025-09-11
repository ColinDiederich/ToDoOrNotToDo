<template>
  <div
    v-if="isVisible"
    class="error-banner"
  >
    <div class="error-banner-content">
      <div class="error-banner-message">
        <svg class="error-banner-icon" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
        </svg>
        <span>{{ message }}</span>
      </div>
      <button
        @click="dismiss"
        class="error-banner-close"
        aria-label="Dismiss error"
      >
        <svg class="error-banner-close-icon" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M4.293 4.293a1 1 0 011.414 0L10 8.586l4.293-4.293a1 1 0 111.414 1.414L11.414 10l4.293 4.293a1 1 0 01-1.414 1.414L10 11.414l-4.293 4.293a1 1 0 01-1.414-1.414L8.586 10 4.293 5.707a1 1 0 010-1.414z" clip-rule="evenodd" />
        </svg>
      </button>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'

const props = defineProps({
  message: {
    type: String,
    required: true
  },
  autoDismiss: {
    type: Boolean,
    default: true
  },
  duration: {
    type: Number,
    default: 5000
  }
})

const emit = defineEmits(['dismiss'])

const isVisible = ref(true)
let autoDismissTimer = null

const dismiss = () => {
  isVisible.value = false
  emit('dismiss')
}

onMounted(() => {
  if (props.autoDismiss) {
    autoDismissTimer = setTimeout(() => {
      dismiss()
    }, props.duration)
  }
})

onUnmounted(() => {
  if (autoDismissTimer) {
    clearTimeout(autoDismissTimer)
  }
})
</script>
