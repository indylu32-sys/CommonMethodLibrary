<template>
  <div class="about-page">
    <el-card>
      <template #header>
        <span style="font-size: 18px; font-weight: bold;">关于项目</span>
      </template>

      <el-descriptions :column="1" border>
        <el-descriptions-item label="项目名称">Common Method Library</el-descriptions-item>
        <el-descriptions-item label="版本">1.0.0</el-descriptions-item>
        <el-descriptions-item label="描述">
          一个完整的.NET Core演示项目，包含常用工具类库和最佳实践
        </el-descriptions-item>
      </el-descriptions>

      <el-divider />

      <h3>后端技术栈</h3>
      <ul style="line-height: 2;">
        <li>ASP.NET Core 8.0 - Web框架</li>
        <li>Entity Framework Core - ORM框架</li>
        <li>Serilog - 日志框架</li>
        <li>Swagger/OpenAPI - API文档</li>
        <li>xUnit - 单元测试框架</li>
        <li>Moq - Mock框架</li>
      </ul>

      <el-divider />

      <h3>前端技术栈</h3>
      <ul style="line-height: 2;">
        <li>Vue 3 - 渐进式JavaScript框架</li>
        <li>Vite - 下一代前端构建工具</li>
        <li>Element Plus - Vue 3 组件库</li>
        <li>Vue Router - 官方路由管理器</li>
        <li>Pinia - 状态管理库</li>
        <li>Axios - HTTP客户端</li>
      </ul>

      <el-divider />

      <h3>核心功能</h3>
      <el-row :gutter="20">
        <el-col :span="12">
          <h4>工具类库</h4>
          <ul style="line-height: 2;">
            <li>StringHelper - 字符串处理</li>
            <li>DateTimeHelper - 日期时间处理</li>
            <li>ValidationHelper - 数据验证</li>
            <li>JsonHelper - JSON处理</li>
            <li>EncryptionHelper - 加密工具</li>
            <li>FileHelper - 文件处理</li>
            <li>HttpHelper - HTTP请求</li>
            <li>CacheHelper - 缓存管理</li>
          </ul>
        </el-col>
        <el-col :span="12">
          <h4>架构特性</h4>
          <ul style="line-height: 2;">
            <li>RESTful API设计</li>
            <li>依赖注入（DI）</li>
            <li>中间件管道</li>
            <li>全局异常处理</li>
            <li>请求日志记录</li>
            <li>速率限制</li>
            <li>模型验证</li>
            <li>后台服务</li>
          </ul>
        </el-col>
      </el-row>

      <el-divider />

      <h3>项目结构</h3>
      <pre style="background: #f5f7fa; padding: 15px; border-radius: 4px; overflow-x: auto;">
CommonMethodLibrary/
├── src/
│   ├── CommonMethodLibrary.Core/          # 核心工具类库
│   │   ├── Helpers/                       # 工具类
│   │   └── Attributes/                    # 验证特性
│   ├── CommonMethodLibrary.WebAPI/        # Web API项目
│   │   ├── Controllers/                   # 控制器
│   │   ├── Models/                        # 模型
│   │   ├── Services/                      # 服务层
│   │   ├── Data/                          # 数据访问层
│   │   ├── Middleware/                    # 中间件
│   │   ├── Filters/                       # 过滤器
│   │   └── HostedServices/                # 后台服务
│   └── CommonMethodLibrary.Frontend/     # Vue3前端项目
│       ├── src/
│       │   ├── views/                     # 页面组件
│       │   ├── api/                       # API请求
│       │   └── router/                    # 路由配置
│       └── package.json
├── tests/
│   └── CommonMethodLibrary.Tests/        # 单元测试项目
└── CommonMethodLibrary.sln               # 解决方案文件
      </pre>

      <el-divider />

      <h3>快速开始</h3>
      <el-steps :active="3" finish-status="success">
        <el-step title="安装.NET SDK" description="安装.NET 8.0 SDK" />
        <el-step title="安装Node.js" description="安装Node.js 18+" />
        <el-step title="运行后端" description="dotnet run" />
        <el-step title="运行前端" description="npm run dev" />
      </el-steps>

      <el-divider />

      <div style="text-align: center; margin-top: 30px;">
        <el-tag size="large" type="success">演示项目</el-tag>
        <el-tag size="large" type="info" style="margin-left: 10px;">学习用途</el-tag>
        <el-tag size="large" type="warning" style="margin-left: 10px;">最佳实践</el-tag>
      </div>
    </el-card>
  </div>
</template>

<script setup>
</script>

<style scoped>
.about-page {
  padding: 20px;
}

h3 {
  color: #303133;
  margin: 20px 0 15px 0;
}

h4 {
  color: #606266;
  margin: 15px 0 10px 0;
}

ul {
  padding-left: 20px;
}

pre {
  font-size: 12px;
  line-height: 1.6;
}
</style>
