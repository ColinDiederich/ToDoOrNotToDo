/**
 * API service for task management
 * Provides native fetch wrappers for all task-related endpoints
 */

const API_BASE_URL = 'http://localhost:5280/api';

/**
 * Standardized error class for API errors
 */
class ApiError extends Error {
  constructor(code, message, details = null) {
    super(message);
    this.name = 'ApiError';
    this.code = code;
    this.details = details;
  }
}

/**
 * Handles API response and error parsing
 * @param {Response} response - The fetch response object
 * @returns {Promise<any>} Parsed JSON data or null for 204 responses
 * @throws {ApiError} Standardized error for non-ok responses
 */
async function handleResponse(response) {
  // Handle 204 No Content responses
  if (response.status === 204) {
    return null;
  }

  // Parse response body
  let data;
  try {
    data = await response.json();
  } catch (error) {
    throw new ApiError('PARSE_ERROR', 'Failed to parse response as JSON', { originalError: error.message });
  }

  // If response is ok, return the data
  if (response.ok) {
    return data;
  }

  // Handle error responses
  const errorInfo = data?.error || {};
  throw new ApiError(
    errorInfo.code || 'UNKNOWN_ERROR',
    errorInfo.message || `HTTP ${response.status}: ${response.statusText}`,
    errorInfo.details || null
  );
}

/**
 * Makes a fetch request with common configuration
 * @param {string} endpoint - The API endpoint
 * @param {Object} options - Fetch options
 * @returns {Promise<any>} Parsed response data
 */
async function apiRequest(endpoint, options = {}) {
  const url = `${API_BASE_URL}${endpoint}`;
  
  const defaultOptions = {
    headers: {
      'Content-Type': 'application/json',
      ...options.headers,
    },
  };

  const response = await fetch(url, { ...defaultOptions, ...options });
  return handleResponse(response);
}

/**
 * Gets all tasks
 * @returns {Promise<Array>} Array of task objects
 */
export async function getTasks() {
  return apiRequest('/tasks');
}

/**
 * Creates a new task
 * @param {string} title - The task title
 * @returns {Promise<Object>} Created task object
 */
export async function createTask(title) {
  return apiRequest('/tasks', {
    method: 'POST',
    body: JSON.stringify({ title }),
  });
}

/**
 * Updates an existing task
 * @param {Object} taskData - Task update data
 * @param {number} taskData.id - Task ID (required)
 * @param {string} [taskData.title] - New task title (optional)
 * @param {boolean} [taskData.isCompleted] - Task completion status (optional)
 * @returns {Promise<Object>} Updated task object
 */
export async function updateTask({ id, title, isCompleted }) {
  return apiRequest('/tasks', {
    method: 'PATCH',
    body: JSON.stringify({ id, title, isCompleted }),
  });
}

/**
 * Deletes a task by ID
 * @param {number} id - Task ID to delete
 * @returns {Promise<null>} Always returns null for successful deletion
 */
export async function deleteTask(id) {
  return apiRequest(`/tasks/${id}`, {
    method: 'DELETE',
  });
}

// Export the ApiError class for external use
export { ApiError };
