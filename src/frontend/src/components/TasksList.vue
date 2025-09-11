<template>
  <div class="container mx-auto px-4 py-8">
    <div class="max-w-4xl mx-auto">
      <h1 class="text-3xl font-bold text-gray-800 mb-8">Tasks</h1>
      
      <div class="card mb-6">
        <h2 class="text-xl font-semibold text-gray-700 mb-4">Add New Task</h2>
        <div class="flex gap-4">
          <input 
            v-model="newTaskTitle" 
            type="text" 
            placeholder="Enter task title..." 
            class="input flex-1"
            @keyup.enter="addTask"
          />
          <button @click="addTask" class="btn-primary">Add Task</button>
        </div>
      </div>

      <div class="card">
        <h2 class="text-xl font-semibold text-gray-700 mb-4">Task List</h2>
        <div v-if="tasks.length === 0" class="text-gray-500 text-center py-8">
          No tasks yet. Add one above!
        </div>
        <div v-else class="space-y-3">
          <div 
            v-for="task in tasks" 
            :key="task.id"
            class="flex items-center justify-between p-4 border border-gray-200 rounded-lg hover:bg-gray-50"
          >
            <div class="flex items-center gap-3">
              <input 
                v-model="task.completed" 
                type="checkbox" 
                class="w-4 h-4 text-blue-600 rounded focus:ring-blue-500"
              />
              <span 
                :class="task.completed ? 'line-through text-gray-500' : 'text-gray-800'"
                class="text-lg"
              >
                {{ task.title }}
              </span>
            </div>
            <button 
              @click="deleteTask(task.id)"
              class="text-red-500 hover:text-red-700 font-medium"
            >
              Delete
            </button>
          </div>
        </div>
      </div>

      <div class="mt-6 text-center">
        <p class="text-sm text-gray-500">
          This is a placeholder TasksList component. Backend integration coming soon!
        </p>
      </div>
    </div>
  </div>
</template>

<script setup>
import { ref, onMounted } from 'vue'

const newTaskTitle = ref('')
const tasks = ref([])

const addTask = () => {
  if (newTaskTitle.value.trim()) {
    tasks.value.push({
      id: Date.now(),
      title: newTaskTitle.value.trim(),
      completed: false
    })
    newTaskTitle.value = ''
  }
}

const deleteTask = (taskId) => {
  tasks.value = tasks.value.filter(task => task.id !== taskId)
}

// Test API connection on component mount
onMounted(async () => {
  try {
    const response = await fetch('/api/health')
    if (response.ok) {
      console.log('✅ API connection successful')
    } else {
      console.warn('⚠️ API connection failed:', response.status)
    }
  } catch (error) {
    console.error('❌ API connection error:', error)
  }
})
</script>
