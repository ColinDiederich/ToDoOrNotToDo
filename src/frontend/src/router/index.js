import { createRouter, createWebHistory } from 'vue-router'
import Container from '../components/Container.vue'

const routes = [
  {
    path: '/',
    name: 'Container',
    component: Container
  }
]

const router = createRouter({
  history: createWebHistory(),
  routes
})

export default router
