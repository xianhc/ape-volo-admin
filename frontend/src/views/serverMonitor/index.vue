<template>
  <div class="app-container">
    <div>
      <el-card class="m-4">
        <div class="text-gray-600 dark:text-gray-100 text-base">
          <span> 系统：{{ data.sys.os }} </span>
          <span> IP：{{ data.sys.ip }} </span>
          <span> 系统已持续运行：{{ data.sys.day }} </span>
        </div>
      </el-card>
      <el-card class="m-4">
        <template #header>
          <span class="text-gray-600 dark:text-gray-100 text-base"> 状态 </span>
        </template>
        <el-row :gutter="10">
          <el-col :xs="24" :sm="12" :md="6">
            <div class="title">磁盘使用率</div>
            <el-tooltip placement="top-end">
              <template #content>
                <div style="font-size: 12px">
                  <div style="padding: 3px">总量：{{ data.disk.total }}</div>
                  <div style="padding: 3px">
                    空闲：{{ data.disk.available }}
                  </div>
                </div>
              </template>
              <div class="content">
                <el-progress
                  type="dashboard"
                  :percentage="parseFloat(data.disk.usageRate || '0')"
                />
              </div>
            </el-tooltip>
            <div class="footer">
              {{ data.disk.used }} / {{ data.disk.total }}
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="title">内存使用率</div>
            <el-tooltip placement="top-end">
              <template #content>
                <div style="font-size: 12px">
                  <div style="padding: 3px">总量：{{ data.memory.total }}</div>
                  <div style="padding: 3px">已使用：{{ data.memory.used }}</div>
                  <div style="padding: 3px">
                    空闲：{{ data.memory.available }}
                  </div>
                </div>
              </template>
              <div class="content">
                <el-progress
                  type="dashboard"
                  :percentage="parseFloat(data.memory.usageRate || '0')"
                />
              </div>
            </el-tooltip>
            <div class="footer">
              {{ data.memory.used }} / {{ data.memory.total }}
            </div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="title">CPU使用率</div>
            <el-tooltip placement="top-end">
              <template #content>
                <div style="font-size: 12px">
                  <div style="padding: 3px">{{ data.cpu.name }}</div>
                  <div style="padding: 3px">{{ data.cpu.package }}</div>
                  <div style="padding: 3px">{{ data.cpu.core }}</div>
                  <div style="padding: 3px">{{ data.cpu.logic }}</div>
                </div>
              </template>
              <div class="content">
                <el-progress
                  type="dashboard"
                  :percentage="parseFloat(data.cpu.used || '0')"
                />
              </div>
            </el-tooltip>
            <div class="footer">{{ data.cpu.coreNumber }} 核心</div>
          </el-col>
          <el-col :xs="24" :sm="12" :md="6">
            <div class="title">交换区使用率</div>
            <el-tooltip placement="top-end">
              <template #content>
                <div style="font-size: 12px">
                  <div style="padding: 3px">
                    总量：{{ data.swap.total || '无' }}
                  </div>
                  <div style="padding: 3px">
                    已使用：{{ data.swap.used || '无' }}
                  </div>
                  <div style="padding: 3px">
                    空闲：{{ data.swap.available || '无' }}
                  </div>
                </div>
              </template>
              <div class="content">
                <el-progress
                  type="dashboard"
                  :percentage="parseFloat(data.swap.usageRate || '0') || 0"
                />
              </div>
            </el-tooltip>
            <div class="footer">
              {{ data.swap.used || '无' }} / {{ data.swap.total || '无' }}
            </div>
          </el-col>
        </el-row>
      </el-card>
      <el-row :gutter="10">
        <el-col :xs="24" :sm="12">
          <el-card class="m-4">
            <template #header>
              <span class="text-gray-600 dark:text-gray-100 text-base">
                CPU使用率监控
              </span>
            </template>
            <div class="h-[240px]">
              <e-charts :options="cpuInfo" height="240px" />
            </div>
          </el-card>
        </el-col>
        <el-col :xs="24" :sm="12">
          <el-card class="m-4">
            <template #header>
              <span class="text-gray-600 dark:text-gray-100 text-base">
                内存使用率监控
              </span>
            </template>
            <div class="h-[240px]">
              <e-charts :options="memoryInfo" height="240px" />
            </div>
          </el-card>
        </el-col>
      </el-row>
    </div>
  </div>
</template>

<script setup lang="ts">
  import { ref, reactive, onMounted, onBeforeUnmount, defineAsyncComponent } from 'vue'
  import { ElCard, ElRow, ElCol, ElTooltip, ElProgress } from 'element-plus'
  import { initData } from '@/api/data'
  const ECharts = defineAsyncComponent(() => import('@/components/Echarts.vue'))

  interface SysInfo {
    os?: string
    ip?: string
    day?: string
  }

  interface DiskInfo {
    total?: string
    available?: string
    usageRate?: string
    used?: string
  }

  interface MemoryInfo {
    total?: string
    used?: string
    available?: string
    usageRate?: string
  }

  interface CpuInfo {
    name?: string
    package?: string
    core?: string
    logic?: string
    used?: string
    coreNumber?: number
  }

  interface SwapInfo {
    total?: string
    used?: string
    available?: string
    usageRate?: string
  }

  interface ServerData {
    sys: SysInfo
    disk: DiskInfo
    memory: MemoryInfo
    cpu: CpuInfo
    swap: SwapInfo
    time?: string
  }

  const monitor = ref<ReturnType<typeof setInterval> | null>(null)
  const url = '/server-resources/get-list'
  const data: ServerData = reactive({
    sys: {},
    disk: {},
    memory: {},
    cpu: {},
    swap: {}
  })

  const cpuInfo = reactive({
    grid: { top: 40, bottom: 40, left: 40, right: 20 },
    tooltip: { trigger: 'axis' },
    xAxis: { type: 'category', boundaryGap: false, data: [] as string[] },
    yAxis: { type: 'value', min: 0, max: 100, interval: 20 },
    series: [
      {
        data: [] as number[],
        type: 'line',
        areaStyle: { color: 'rgb(32, 160, 255)' },
        itemStyle: { color: '#6fbae1', lineStyle: { color: '#6fbae1' } }
      }
    ]
  })

  const memoryInfo = reactive({
    grid: { top: 40, bottom: 40, left: 40, right: 20 },
    tooltip: { trigger: 'axis' },
    xAxis: { type: 'category', boundaryGap: false, data: [] as string[] },
    yAxis: { type: 'value', min: 0, max: 100, interval: 20 },
    series: [
      {
        data: [] as number[],
        type: 'line',
        areaStyle: { color: 'rgb(32, 160, 255)' },
        itemStyle: { color: '#6fbae1', lineStyle: { color: '#6fbae1' } }
      }
    ]
  })

  // 保证响应式赋值
  function fillReactive(target: any, src: any) {
    for (const key in target) {
      if (typeof target[key] === 'object' && target[key] !== null && src[key]) {
        Object.assign(target[key], src[key])
      } else if (src[key] !== undefined) {
        target[key] = src[key]
      }
    }
  }

  function updateCharts(res: Partial<ServerData>) {
    if (cpuInfo.xAxis.data.length >= 8) {
      cpuInfo.xAxis.data.shift()
      memoryInfo.xAxis.data.shift()
      cpuInfo.series[0].data.shift()
      memoryInfo.series[0].data.shift()
    }
    cpuInfo.xAxis.data.push(res.time || '')
    memoryInfo.xAxis.data.push(res.time || '')
    cpuInfo.series[0].data.push(parseFloat(res.cpu?.used || '0') || 0)
    memoryInfo.series[0].data.push(
      parseFloat(res.memory?.usageRate || '0') || 0
    )
  }

  function init() {
    initData(url, {})
      .then((res) => {
        const realData = res.data ? res.data : res
        fillReactive(data, realData)
        updateCharts(realData)
      })
      .catch((err) => {
        // 可以弹窗提示
        console.error('接口请求失败', err)
      })
      .finally(() => {})
  }

  onMounted(() => {
    init()
    monitor.value = setInterval(() => {
      init()
    }, 30000)
  })

  onBeforeUnmount(() => {
    if (monitor.value) {
      clearInterval(monitor.value)
    }
  })
</script>

<style scoped lang="scss">
  .title {
    text-align: center;
    font-size: 15px;
    font-weight: 500;
    color: #999;
    margin-bottom: 16px;
  }
  .footer {
    text-align: center;
    font-size: 15px;
    font-weight: 500;
    color: #999;
    margin-top: -5px;
    margin-bottom: 10px;
  }
  .content {
    text-align: center;
    margin-top: 5px;
    margin-bottom: 5px;
  }
</style>
