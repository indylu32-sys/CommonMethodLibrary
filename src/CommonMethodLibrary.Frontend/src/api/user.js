import request from './request'

export const userApi = {
  // 获取所有用户
  getAllUsers() {
    return request.get('/users')
  },

  // 分页获取用户
  getPagedUsers(params) {
    return request.get('/users/paged', { params })
  },

  // 根据ID获取用户
  getUserById(id) {
    return request.get(`/users/${id}`)
  },

  // 创建用户
  createUser(data) {
    return request.post('/users', data)
  },

  // 更新用户
  updateUser(id, data) {
    return request.put(`/users/${id}`, data)
  },

  // 删除用户
  deleteUser(id) {
    return request.delete(`/users/${id}`)
  }
}
