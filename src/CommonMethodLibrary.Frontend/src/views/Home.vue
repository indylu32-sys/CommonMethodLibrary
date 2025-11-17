<template>
  <div class="home">
    <el-card class="welcome-card">
      <template #header>
        <div class="card-header">
          <el-icon :size="24"><Monitor /></el-icon>
          <span style="margin-left: 10px; font-size: 20px; font-weight: bold;">欢迎使用 Common Method Library</span>
        </div>
      </template>

      <el-row :gutter="20">
        <el-col :span="24">
          <h2>项目简介</h2>
          <p style="line-height: 1.8; font-size: 14px; color: #606266;">
            这是一个完整的 .NET Core + Vue3 演示项目，展示了如何构建标准的企业级应用程序。
            本项目包含了常用的工具类库、RESTful API、以及现代化的前端界面。
          </p>
        </el-col>
      </el-row>

      <el-divider />

      <el-row :gutter="20">
        <el-col :xs="24" :sm="12" :md="8">
          <el-card shadow="hover" class="feature-card">
            <el-icon :size="40" color="#409EFF"><User /></el-icon>
            <h3>用户管理</h3>
            <p>完整的CRUD操作示例</p>
            <el-button type="primary" size="small" @click="$router.push('/users')">查看</el-button>
          </el-card>
        </el-col>

        <el-col :xs="24" :sm="12" :md="8">
          <el-card shadow="hover" class="feature-card">
            <el-icon :size="40" color="#67C23A"><Tools /></el-icon>
            <h3>工具演示</h3>
            <p>各种工具类的使用示例</p>
            <el-button type="success" size="small" @click="$router.push('/tools')">查看</el-button>
          </el-card>
        </el-col>

        <el-col :xs="24" :sm="12" :md="8">
          <el-card shadow="hover" class="feature-card">
            <el-icon :size="40" color="#E6A23C"><Document /></el-icon>
            <h3>API文档</h3>
            <p>Swagger API文档</p>
            <el-button type="warning" size="small" @click="openSwagger">查看</el-button>
          </el-card>
        </el-col>
      </el-row>

      <el-divider />

      <el-row :gutter="20">
        <el-col :span="24">
          <h3>技术栈</h3>
          <div style="margin-top: 20px;">
            <el-tag size="large" style="margin: 5px;">ASP.NET Core 8.0</el-tag>
            <el-tag size="large" type="success" style="margin: 5px;">Vue 3</el-tag>
            <el-tag size="large" type="info" style="margin: 5px;">Vite</el-tag>
            <el-tag size="large" type="warning" style="margin: 5px;">Element Plus</el-tag>
            <el-tag size="large" type="danger" style="margin: 5px;">Entity Framework Core</el-tag>
            <el-tag size="large" style="margin: 5px;">Serilog</el-tag>
            <el-tag size="large" type="success" style="margin: 5px;">xUnit</el-tag>
            <el-tag size="large" type="info" style="margin: 5px;">Swagger</el-tag>
          </div>
        </el-col>
      </el-row>

      <el-divider />

      <el-row :gutter="20">
        <el-col :span="24">
          <h3>功能特性</h3>
          <el-row :gutter="10" style="margin-top: 15px;">
            <el-col :xs="24" :sm="12">
              <ul style="line-height: 2;">
                <li><el-icon><Check /></el-icon> RESTful API 设计</li>
                <li><el-icon><Check /></el-icon> 依赖注入模式</li>
                <li><el-icon><Check /></el-icon> 中间件管道</li>
                <li><el-icon><Check /></el-icon> 全局异常处理</li>
              </ul>
            </el-col>
            <el-col :xs="24" :sm="12">
              <ul style="line-height: 2;">
                <li><el-icon><Check /></el-icon> 后台服务（IHostedService）</li>
                <li><el-icon><Check /></el-icon> 模型验证</li>
                <li><el-icon><Check /></el-icon> 单元测试</li>
                <li><el-icon><Check /></el-icon> API文档（Swagger）</li>
              </ul>
            </el-col>
          </el-row>
        </el-col>
      </el-row>
    </el-card>
  </div>
</template>

<script setup>
const openSwagger = () => {
  window.open('http://localhost:5000', '_blank')
}
</script>

<style scoped>
.home {
  padding: 20px;
}

.welcome-card {
  margin-bottom: 20px;
}

.card-header {
  display: flex;
  align-items: center;
}

.feature-card {
  text-align: center;
  padding: 20px;
  margin-bottom: 20px;
}

.feature-card h3 {
  margin: 15px 0 10px 0;
  color: #303133;
}

.feature-card p {
  color: #606266;
  margin-bottom: 15px;
}

ul {
  list-style: none;
  padding: 0;
}

ul li {
  display: flex;
  align-items: center;
  gap: 8px;
}
</style>
