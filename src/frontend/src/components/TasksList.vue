<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <h1 class="text-3xl font-bold text-gray-800 mb-8">Tasks</h1>
      
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
            <div 
              v-for="task in activeTasks" 
              :key="task.id"
              class="flex items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              <div class="flex items-center gap-3">
                <div class="w-4 h-4 border-2 border-gray-300 rounded"></div>
                <span class="text-gray-800 text-lg">
                  {{ task.title }}
                </span>
              </div>
            </div>
          </div>

          <!-- Divider between Active and Completed -->
          <div v-if="activeTasks.length > 0 && completedTasks.length > 0" class="border-t border-gray-200 my-6"></div>

          <!-- Completed Tasks -->
          <div v-if="completedTasks.length > 0" class="space-y-3">
            <div 
              v-for="task in completedTasks" 
              :key="task.id"
              class="flex items-center p-4 border border-gray-200 rounded-lg hover:bg-gray-50"
            >
              <div class="flex items-center gap-3">
                <div class="w-4 h-4 border-2 border-gray-300 rounded bg-gray-300"></div>
                <span class="line-through text-gray-500 text-lg">
                  {{ task.title }}
                </span>
              </div>
            </div>
          </div>
        </div>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, computed, onMounted } from 'vue'
import { getTasks } from '../services/api.js'

// Reactive state
const loading = ref(true)
const allTasks = ref([])

// Computed properties for task separation
const activeTasks = computed(() => 
  allTasks.value.filter(task => !task.isCompleted)
)

const completedTasks = computed(() => 
  allTasks.value.filter(task => task.isCompleted)
)

// Load tasks on component mount
onMounted(async () => {
  try {
    loading.value = true
    const tasks = await getTasks()
    allTasks.value = tasks || []
  } catch (error) {
    console.error('Failed to load tasks:', error)
    allTasks.value = []
  } finally {
    loading.value = false
  }
})
</script>
