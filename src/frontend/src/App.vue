<template>
  <div id="app">
    <!-- Global Error Banner -->
    <ErrorBanner
      v-if="errorMessage"
      :message="errorMessage"
      :auto-dismiss="true"
      :duration="5000"
      @dismiss="clearError"
    />
    
    <!-- Main Content -->
    <div :class="{ 'pt-16': errorMessage }">
      <router-view />
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted, onUnmounted } from 'vue'
import ErrorBanner from './components/ErrorBanner.vue'
import { eventBus } from './services/eventBus.js'

const errorMessage = ref('')

let unsubscribeError = null

const clearError = () => {
  errorMessage.value = ''
}

onMounted(() => {
  // Subscribe to error events
  unsubscribeError = eventBus.on('show-error', ({ message }) => {
    errorMessage.value = message
  })
})

onUnmounted(() => {
  // Clean up event listeners
  if (unsubscribeError) {
    unsubscribeError()
  }
})
</script>

<style>
#app {
  min-height: 100vh;
  background-color: #f8fafc;
}
</style>
