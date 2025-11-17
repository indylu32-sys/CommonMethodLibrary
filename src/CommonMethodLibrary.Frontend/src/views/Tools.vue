<template>
  <div class="tools-page">
    <el-row :gutter="20">
      <!-- 字符串工具 -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <span style="font-weight: bold;">字符串工具演示</span>
          </template>
          <el-input
            v-model="stringInput"
            placeholder="请输入字符串"
            @change="testString"
          />
          <el-button type="primary" @click="testString" style="margin-top: 10px;" :loading="stringLoading">
            测试
          </el-button>
          <div v-if="stringResult" style="margin-top: 15px;">
            <el-descriptions :column="1" border size="small">
              <el-descriptions-item label="原始值">{{ stringResult.original }}</el-descriptions-item>
              <el-descriptions-item label="驼峰命名">{{ stringResult.camelCase }}</el-descriptions-item>
              <el-descriptions-item label="帕斯卡命名">{{ stringResult.pascalCase }}</el-descriptions-item>
              <el-descriptions-item label="蛇形命名">{{ stringResult.snakeCase }}</el-descriptions-item>
              <el-descriptions-item label="截断">{{ stringResult.truncated }}</el-descriptions-item>
              <el-descriptions-item label="随机字符串">{{ stringResult.randomString }}</el-descriptions-item>
              <el-descriptions-item label="手机号掩码">{{ stringResult.maskedPhone }}</el-descriptions-item>
            </el-descriptions>
          </div>
        </el-card>
      </el-col>

      <!-- 日期时间工具 -->
      <el-col :span="12">
        <el-card>
          <template #header>
            <span style="font-weight: bold;">日期时间工具演示</span>
          </template>
          <el-button type="primary" @click="testDateTime" :loading="dateTimeLoading">
            测试
          </el-button>
          <div v-if="dateTimeResult" style="margin-top: 15px;">
            <el-descriptions :column="1" border size="small">
              <el-descriptions-item label="当前时间戳">{{ dateTimeResult.currentTimestamp }}</el-descriptions-item>
              <el-descriptions-item label="当前时间戳(毫秒)">{{ dateTimeResult.currentTimestampMs }}</el-descriptions-item>
              <el-descriptions-item label="友好时间">{{ dateTimeResult.friendlyTime }}</el-descriptions-item>
              <el-descriptions-item label="是否工作日">{{ dateTimeResult.isWorkday ? '是' : '否' }}</el-descriptions-item>
              <el-descriptions-item label="本周开始">{{ dateTimeResult.startOfWeek }}</el-descriptions-item>
              <el-descriptions-item label="本月开始">{{ dateTimeResult.startOfMonth }}</el-descriptions-item>
              <el-descriptions-item label="本月结束">{{ dateTimeResult.endOfMonth }}</el-descriptions-item>
              <el-descriptions-item label="年龄计算">{{ dateTimeResult.age }}岁</el-descriptions-item>
            </el-descriptions>
          </div>
        </el-card>
      </el-col>

      <!-- 验证工具 -->
      <el-col :span="12" style="margin-top: 20px;">
        <el-card>
          <template #header>
            <span style="font-weight: bold;">验证工具演示</span>
          </template>
          <el-form :model="validationForm" label-width="100px">
            <el-form-item label="邮箱">
              <el-input v-model="validationForm.email" placeholder="test@example.com" />
            </el-form-item>
            <el-form-item label="手机号">
              <el-input v-model="validationForm.phone" placeholder="13800138000" />
            </el-form-item>
            <el-form-item label="URL">
              <el-input v-model="validationForm.url" placeholder="https://example.com" />
            </el-form-item>
            <el-form-item label="IP地址">
              <el-input v-model="validationForm.ipAddress" placeholder="192.168.1.1" />
            </el-form-item>
            <el-form-item label="密码">
              <el-input v-model="validationForm.password" placeholder="Password123!" />
            </el-form-item>
            <el-form-item>
              <el-button type="primary" @click="testValidation" :loading="validationLoading">
                验证
              </el-button>
            </el-form-item>
          </el-form>
          <div v-if="validationResult" style="margin-top: 15px;">
            <el-descriptions :column="2" border size="small">
              <el-descriptions-item label="邮箱">
                <el-tag :type="validationResult.isValidEmail ? 'success' : 'danger'">
                  {{ validationResult.isValidEmail ? '有效' : '无效' }}
                </el-tag>
              </el-descriptions-item>
              <el-descriptions-item label="手机号">
                <el-tag :type="validationResult.isValidPhone ? 'success' : 'danger'">
                  {{ validationResult.isValidPhone ? '有效' : '无效' }}
                </el-tag>
              </el-descriptions-item>
              <el-descriptions-item label="URL">
                <el-tag :type="validationResult.isValidUrl ? 'success' : 'danger'">
                  {{ validationResult.isValidUrl ? '有效' : '无效' }}
                </el-tag>
              </el-descriptions-item>
              <el-descriptions-item label="IP地址">
                <el-tag :type="validationResult.isValidIP ? 'success' : 'danger'">
                  {{ validationResult.isValidIP ? '有效' : '无效' }}
                </el-tag>
              </el-descriptions-item>
              <el-descriptions-item label="密码强度">
                <el-tag :type="validationResult.isStrongPassword ? 'success' : 'danger'">
                  {{ validationResult.isStrongPassword ? '强' : '弱' }}
                </el-tag>
              </el-descriptions-item>
            </el-descriptions>
          </div>
        </el-card>
      </el-col>

      <!-- 加密工具 -->
      <el-col :span="12" style="margin-top: 20px;">
        <el-card>
          <template #header>
            <span style="font-weight: bold;">加密工具演示</span>
          </template>
          <el-input
            v-model="encryptionInput"
            placeholder="请输入要加密的文本"
          />
          <el-button type="primary" @click="testEncryption" style="margin-top: 10px;" :loading="encryptionLoading">
            加密
          </el-button>
          <div v-if="encryptionResult" style="margin-top: 15px;">
            <el-descriptions :column="1" border size="small">
              <el-descriptions-item label="原始文本">{{ encryptionResult.original }}</el-descriptions-item>
              <el-descriptions-item label="MD5">
                <el-text type="info" size="small" truncated>{{ encryptionResult.md5 }}</el-text>
              </el-descriptions-item>
              <el-descriptions-item label="SHA256">
                <el-text type="info" size="small" truncated>{{ encryptionResult.sha256 }}</el-text>
              </el-descriptions-item>
              <el-descriptions-item label="GUID">{{ encryptionResult.guid }}</el-descriptions-item>
              <el-descriptions-item label="随机密钥">{{ encryptionResult.randomKey }}</el-descriptions-item>
            </el-descriptions>
          </div>
        </el-card>
      </el-col>

      <!-- 缓存工具 -->
      <el-col :span="24" style="margin-top: 20px;">
        <el-card>
          <template #header>
            <span style="font-weight: bold;">缓存工具演示</span>
          </template>
          <el-row :gutter="20">
            <el-col :span="12">
              <h4>设置缓存</h4>
              <el-form :model="cacheForm" label-width="100px">
                <el-form-item label="键">
                  <el-input v-model="cacheForm.key" placeholder="cache_key" />
                </el-form-item>
                <el-form-item label="值">
                  <el-input v-model="cacheForm.value" placeholder="cache_value" />
                </el-form-item>
                <el-form-item label="过期时间">
                  <el-input-number v-model="cacheForm.expirationMinutes" :min="1" :max="60" />
                  <span style="margin-left: 10px;">分钟</span>
                </el-form-item>
                <el-form-item>
                  <el-button type="primary" @click="setCacheValue" :loading="cacheSetLoading">
                    设置
                  </el-button>
                </el-form-item>
              </el-form>
            </el-col>
            <el-col :span="12">
              <h4>获取缓存</h4>
              <el-input v-model="cacheKey" placeholder="请输入缓存键">
                <template #append>
                  <el-button @click="getCacheValue" :loading="cacheGetLoading">获取</el-button>
                </template>
              </el-input>
              <div v-if="cacheValue" style="margin-top: 15px;">
                <el-alert :title="`缓存值: ${cacheValue}`" type="success" :closable="false" />
              </div>
              <div style="margin-top: 15px;">
                <el-button @click="getAllCacheKeys" :loading="cacheKeysLoading">
                  查看所有缓存键
                </el-button>
              </div>
              <div v-if="cacheKeys.length > 0" style="margin-top: 10px;">
                <el-tag v-for="key in cacheKeys" :key="key" style="margin: 5px;">{{ key }}</el-tag>
              </div>
            </el-col>
          </el-row>
        </el-card>
      </el-col>
    </el-row>
  </div>
</template>

<script setup>
import { ref, reactive } from 'vue'
import { ElMessage } from 'element-plus'
import { toolsApi } from '@/api/tools'

// 字符串工具
const stringInput = ref('HelloWorld')
const stringResult = ref(null)
const stringLoading = ref(false)

const testString = async () => {
  stringLoading.value = true
  try {
    const res = await toolsApi.stringDemo(stringInput.value)
    stringResult.value = res.data
  } catch (error) {
    ElMessage.error('测试失败')
  } finally {
    stringLoading.value = false
  }
}

// 日期时间工具
const dateTimeResult = ref(null)
const dateTimeLoading = ref(false)

const testDateTime = async () => {
  dateTimeLoading.value = true
  try {
    const res = await toolsApi.dateTimeDemo()
    dateTimeResult.value = res.data
  } catch (error) {
    ElMessage.error('测试失败')
  } finally {
    dateTimeLoading.value = false
  }
}

// 验证工具
const validationForm = reactive({
  email: 'test@example.com',
  phone: '13800138000',
  url: 'https://example.com',
  ipAddress: '192.168.1.1',
  password: 'Password123!'
})
const validationResult = ref(null)
const validationLoading = ref(false)

const testValidation = async () => {
  validationLoading.value = true
  try {
    const res = await toolsApi.validationDemo(validationForm)
    validationResult.value = res.data
  } catch (error) {
    ElMessage.error('验证失败')
  } finally {
    validationLoading.value = false
  }
}

// 加密工具
const encryptionInput = ref('Hello, World!')
const encryptionResult = ref(null)
const encryptionLoading = ref(false)

const testEncryption = async () => {
  encryptionLoading.value = true
  try {
    const res = await toolsApi.encryptionDemo({ text: encryptionInput.value })
    encryptionResult.value = res.data
  } catch (error) {
    ElMessage.error('加密失败')
  } finally {
    encryptionLoading.value = false
  }
}

// 缓存工具
const cacheForm = reactive({
  key: 'test_key',
  value: 'test_value',
  expirationMinutes: 5
})
const cacheKey = ref('')
const cacheValue = ref('')
const cacheKeys = ref([])
const cacheSetLoading = ref(false)
const cacheGetLoading = ref(false)
const cacheKeysLoading = ref(false)

const setCacheValue = async () => {
  cacheSetLoading.value = true
  try {
    await toolsApi.cacheSet(cacheForm)
    ElMessage.success('缓存设置成功')
  } catch (error) {
    ElMessage.error('设置失败')
  } finally {
    cacheSetLoading.value = false
  }
}

const getCacheValue = async () => {
  if (!cacheKey.value) {
    ElMessage.warning('请输入缓存键')
    return
  }
  cacheGetLoading.value = true
  try {
    const res = await toolsApi.cacheGet(cacheKey.value)
    cacheValue.value = JSON.stringify(res.data.value)
    ElMessage.success('获取成功')
  } catch (error) {
    cacheValue.value = ''
  } finally {
    cacheGetLoading.value = false
  }
}

const getAllCacheKeys = async () => {
  cacheKeysLoading.value = true
  try {
    const res = await toolsApi.cacheKeys()
    cacheKeys.value = res.data.keys
  } catch (error) {
    ElMessage.error('获取失败')
  } finally {
    cacheKeysLoading.value = false
  }
}
</script>

<style scoped>
.tools-page {
  padding: 20px;
}
</style>
