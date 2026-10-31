<!--
  个人中心「行为记录」面板。
  展示当前用户的接口调用日志，支持分页与单条详情抽屉。
-->
<template>
  <div class="py-6">
    <div class="ape-volo-table">
      <el-table v-loading="tableLoading" :data="tableData" style="width: 100%" row-key="id">
        <el-table-column prop="creationTime" label="创建时间" />
        <el-table-column prop="executionDuration" label="请求耗时">
          <template #default="scope">
            <el-tag v-if="scope.row.executionDuration <= 200" type="success">
              {{ scope.row.executionDuration }}ms
            </el-tag>
            <el-tag v-else-if="scope.row.executionDuration <= 500">
              {{ scope.row.executionDuration }}ms
            </el-tag>
            <el-tag v-else-if="scope.row.executionDuration <= 1000" type="warning">
              {{ scope.row.executionDuration }}ms
            </el-tag>
            <el-tag v-else type="danger">{{ scope.row.executionDuration }}ms</el-tag>
          </template>
        </el-table-column>
        <el-table-column prop="requestUrl" label="请求地址" />
        <el-table-column prop="description" label="描述" />
        <el-table-column prop="method" label="请求方式" />
        <el-table-column prop="requestIp" label="Ip地址" />
        <el-table-column prop="ipAddress" label="归属地" />
        <el-table-column :min-width="appStore.operateMinWith" label="操作">
          <template #default="scope">
            <el-button icon="view" type="text" @click="showDetail(scope.row)">详情</el-button>
          </template>
        </el-table-column>
      </el-table>
      <el-pagination
        :current-page="pageIndex"
        :page-size="pageSize"
        :total="total"
        layout="total, prev, pager, next, jumper"
        @current-change="handleCurrentChange"
      />
    </div>
  </div>
  <el-drawer v-model="detailVisible" direction="rtl" :size="appStore.drawerSize">
    <template #header>
      <div class="flex justify-between items-center">
        <span class="text-lg">{{ currentRow.description }}</span>
      </div>
    </template>
    <div class="p-2">
      <div class="ape-volo-bg rounded-lg shadow-md p-3">
        <div class="flex items-center mb-2">
          <div class="flex gap-2">
            <span
              class="w-3.5 h-3.5 rounded-full inline-block border border-[#e0443e] bg-[#ff5f56] shadow"
            ></span>
            <span
              class="w-3.5 h-3.5 rounded-full inline-block border border-[#dea123] bg-[#ffbd2e] shadow"
            ></span>
            <span
              class="w-3.5 h-3.5 rounded-full inline-block border border-[#13a10e] bg-[#27c93f] shadow"
            ></span>
          </div>
        </div>
        <div class="grid gap-y-2 gap-x-4" style="grid-template-columns: 88px 1fr">
          <span class="text-emerald-500 min-w-[88px]">操作人：</span
          ><span class="break-all">{{ currentRow.creatorName }}</span>
          <span class="text-emerald-500 min-w-[88px]">请求时间：</span
          ><span class="break-all">{{ currentRow.creationTime }}</span>
          <span class="text-emerald-500 min-w-[88px]">请求地址：</span
          ><span class="break-all">{{ currentRow.requestUrl }}</span>
          <span class="text-emerald-500 min-w-[88px]">请求方式：</span
          ><span class="break-all">{{ currentRow.method }}</span>
          <span class="text-emerald-500 min-w-[88px]">Ip地址：</span
          ><span class="break-all">{{ currentRow.requestIp }}</span>
          <span class="text-emerald-500 min-w-[88px]">Ip归属地：</span
          ><span class="break-all">{{ currentRow.ipAddress }}</span>
          <span class="text-emerald-500 min-w-[88px]">耗时：</span
          ><span class="break-all">{{ currentRow.executionDuration }}</span>
          <span class="text-emerald-500 min-w-[88px]">操作系统：</span
          ><span class="break-all">{{ currentRow.operatingSystem }}</span>
          <span class="text-emerald-500 min-w-[88px]">设备类型：</span
          ><span class="break-all">{{ currentRow.deviceType }}</span>
          <span class="text-emerald-500 min-w-[88px]">浏览器：</span
          ><span class="break-all">{{ currentRow.browserName }}</span>
          <span class="text-emerald-500 min-w-[88px]">版本号：</span
          ><span class="break-all">{{ currentRow.version }}</span>
          <span class="text-emerald-500 min-w-[88px]">代理信息：</span
          ><span class="break-all">{{ currentRow.userAgent }}</span>
          <span class="text-emerald-500 min-w-[88px]">请求参数：</span>
          <pre class="break-all rounded">{{ formatJson(currentRow.requestParameters) }}</pre>
          <span class="text-emerald-500 min-w-[88px]">响应结果：</span>
          <pre class="break-all rounded">{{ formatJson(currentRow.responseData) }}</pre>
        </div>
      </div>
    </div>
  </el-drawer>
</template>

<script setup lang="ts">
  import { ref } from 'vue'
  import { useAppStore } from '@/pinia'
  import { getCurrent, type OperateLog } from '@/api/log/operateLog'

  defineOptions({
    name: 'PersonalCenterOperateLog'
  })

  const appStore = useAppStore()
  const tableLoading = ref(false)
  const pageIndex = ref(1)
  const pageSize = ref(10)
  const total = ref(0)
  const tableData = ref<OperateLog[]>([])
  const detailVisible = ref(false)
  const currentRow = ref<Partial<OperateLog>>({})

  /** 按当前分页参数拉取行为记录。 */
  const fetchPage = async (): Promise<void> => {
    const res = await getCurrent({ pageIndex: pageIndex.value, pageSize: pageSize.value })
    tableData.value = res.data.items
    total.value = res.data.totalCount
  }

  /** 加载行为记录，供父组件在切换到该标签页时调用。 */
  const load = async (): Promise<void> => {
    tableLoading.value = true
    try {
      await fetchPage()
    } finally {
      tableLoading.value = false
    }
  }

  /** 切换页码后重新拉取。 */
  const handleCurrentChange = (val: number): void => {
    pageIndex.value = val
    void load()
  }

  /** 打开单条记录的详情抽屉。 */
  const showDetail = (row: OperateLog): void => {
    currentRow.value = { ...row }
    detailVisible.value = true
  }

  /** 格式化请求参数与响应体，解析失败时原样返回。 */
  const formatJson = (value: unknown): string => {
    if (!value) return ''
    try {
      const obj = typeof value === 'string' ? JSON.parse(value) : value
      return JSON.stringify(obj, null, 2)
    } catch {
      return String(value)
    }
  }

  defineExpose({ load })
</script>
