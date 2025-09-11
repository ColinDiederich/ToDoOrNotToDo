<template>
  <div
    v-if="isVisible"
    class="fixed top-0 left-0 right-0 z-50 bg-red-600 text-white px-4 py-3 shadow-lg"
  >
    <div class="container mx-auto flex items-center justify-between">
      <div class="flex items-center">
        <svg class="w-5 h-5 mr-2" fill="currentColor" viewBox="0 0 20 20">
          <path fill-rule="evenodd" d="M10 18a8 8 0 100-16 8 8 0 000 16zM8.707 7.293a1 1 0 00-1.414 1.414L8.586 10l-1.293 1.293a1 1 0 101.414 1.414L10 11.414l1.293 1.293a1 1 0 001.414-1.414L11.414 10l1.293-1.293a1 1 0 00-1.414-1.414L10 8.586 8.707 7.293z" clip-rule="evenodd" />
        </svg>
        <span class="font-medium">{{ message }}</span>
      </div>
      <button
        @click="dismiss"
        class="ml-4 text-white hover:text-red-200 focus:outline-none focus:text-red-200 transition-colors"
        aria-label="Dismiss error"
      >
        <svg class="w-5 h-5" fill="currentColor" viewBox="0 0 20 20">
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
