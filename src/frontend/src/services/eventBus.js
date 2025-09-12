/**
 * Simple event bus for global application events
 * Used for managing global error messages and other cross-component communication
 */

class EventBus {
  constructor() {
    this.events = {}
  }

  /**
   * Subscribe to an event
   * @param {string} event - Event name
   * @param {Function} callback - Callback function
   * @returns {Function} Unsubscribe function
   */
  on(event, callback) {
    if (!this.events[event]) {
      this.events[event] = []
    }
    this.events[event].push(callback)

    // Return unsubscribe function
    return () => {
      this.events[event] = this.events[event].filter(cb => cb !== callback)
    }
  }

  /**
   * Emit an event
   * @param {string} event - Event name
   * @param {any} data - Event data
   */
  emit(event, data) {
    if (this.events[event]) {
      this.events[event].forEach(callback => {
        try {
          callback(data)
        } catch (error) {
          console.error(`Error in event callback for ${event}:`, error)
        }
      })
    }
  }

  /**
   * Remove all listeners for an event
   * @param {string} event - Event name
   */
  off(event) {
    if (this.events[event]) {
      delete this.events[event]
    }
  }

  /**
   * Remove all listeners
   */
  removeAllListeners() {
    this.events = {}
  }
}

// Create and export a singleton instance
export const eventBus = new EventBus()

// Convenience methods for common events
export const showError = (message, options = {}) => {
  eventBus.emit('show-error', { message, ...options })
}

