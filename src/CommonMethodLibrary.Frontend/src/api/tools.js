import request from './request'

export const toolsApi = {
  // 字符串工具演示
  stringDemo(input) {
    return request.get('/ToolsDemo/string', { params: { input } })
  },

  // 日期时间工具演示
  dateTimeDemo() {
    return request.get('/ToolsDemo/datetime')
  },

  // 验证工具演示
  validationDemo(data) {
    return request.post('/ToolsDemo/validation', data)
  },

  // JSON工具演示
  jsonDemo(data) {
    return request.post('/ToolsDemo/json', data)
  },

  // 加密工具演示
  encryptionDemo(data) {
    return request.post('/ToolsDemo/encryption', data)
  },

  // 文件工具演示
  fileDemo() {
    return request.get('/ToolsDemo/file')
  },

  // 缓存工具演示 - 设置
  cacheSet(data) {
    return request.post('/ToolsDemo/cache/set', data)
  },

  // 缓存工具演示 - 获取
  cacheGet(key) {
    return request.get(`/ToolsDemo/cache/get/${key}`)
  },

  // 缓存工具演示 - 获取所有键
  cacheKeys() {
    return request.get('/ToolsDemo/cache/keys')
  }
}
