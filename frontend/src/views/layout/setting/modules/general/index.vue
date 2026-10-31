<template>
  <div class="preference-panel">
    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:monitor-cog" />
        <h3>{{ '通用' }}</h3>
      </div>
      <div class="preference-group">
        <PreferenceItem
          :label="'页面水印'"
          :description="'在工作区内容上显示身份标识水印'"
        >
          <el-switch v-model="config.showWatermark" />
        </PreferenceItem>
        <PreferenceItem
          :label="'动态页面标题'"
          :description="'在浏览器标题中显示当前页面名称'"
        >
          <el-switch v-model="config.dynamicTitle" />
        </PreferenceItem>
      </div>
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:sparkles" />
        <h3>{{ '页面切换动画' }}</h3>
      </div>
      <TransitionPreview v-model="config.transitionType" :active="active" />
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:database-backup" />
        <h3>{{ '设置维护' }}</h3>
      </div>
      <div class="maintenance-actions">
        <el-button
          :icon="createIconifyIcon('lucide:file-output')"
          @click="handleExportConfig"
        >
          {{ '导出备份' }}
        </el-button>
        <el-upload
          :auto-upload="false"
          :show-file-list="false"
          accept=".json,application/json"
          @change="handleImportConfig"
        >
          <el-button :icon="createIconifyIcon('lucide:file-input')">
            {{ '导入备份' }}
          </el-button>
        </el-upload>
      </div>
    </section>

    <section class="preference-section">
      <div class="preference-section-heading">
        <AppIcon icon="lucide:info" />
        <h3>{{ '项目说明' }}</h3>
      </div>
      <div class="project-info">
        <img :src="setting.appLogo" alt="" />
        <div>
          <strong>{{ setting.appName }}</strong>
          <p>
            {{
              '基于 .NET 10 与 Vue 3 的企业后台管理解决方案，适用于权限控制、系统管理与通用业务中台场景。'
            }}
          </p>
          <div class="project-links">
            <a
              href="https://github.com/xianhc/ape-volo-admin"
              target="_blank"
              rel="noopener noreferrer"
              >{{ 'GitHub 仓库' }}</a
            >
            <a
              href="https://gitee.com/xianhc/ape-volo-admin"
              target="_blank"
              rel="noopener noreferrer"
              >{{ 'Gitee 仓库' }}</a
            >
            <a
              href="https://www.apevolo.com/"
              target="_blank"
              rel="noopener noreferrer"
              >{{ '系统官网' }}</a
            >
          </div>
        </div>
      </div>
    </section>
  </div>
</template>

<script setup lang="ts">
  /**
   * 通用设置页。
   */
  import AppIcon from '@/components/AppIcon.vue'
  import { createIconifyIcon } from '@/components/icons'
  import { ElMessage } from 'element-plus'
  import type { UploadFile } from 'element-plus'
  import { storeToRefs } from 'pinia'
  import { useAppStore } from '@/pinia'
  import setting from '@/setting'
  import { parsePreferences } from '@/services/appearance/preferences'
  import TransitionPreview from '../../components/TransitionPreview.vue'
  import PreferenceItem from '../../components/PreferenceItem.vue'
  defineProps<{
    /** 日志队列中当前活动的消息数量。 */
    active: boolean
  }>()

  defineOptions({
    name: 'GeneralSettings'
  })

  const appStore = useAppStore()
  const { config } = storeToRefs(appStore)
  /**
   * 导出当前有效配置。
   */
  const handleExportConfig = () => {
    const configData = JSON.stringify(config.value, null, 2)
    const blob = new Blob([configData], { type: 'application/json' })
    const url = URL.createObjectURL(blob)

    const link = document.createElement('a')
    link.href = url
    link.download = `ape-volo-admin-settings-${
      new Date().toISOString().split('T')[0]
    }.json`
    document.body.appendChild(link)
    link.click()
    document.body.removeChild(link)
    URL.revokeObjectURL(url)

    ElMessage.success('设置备份已导出')
  }

  /**
   * 读取并校验用户导入的配置。
   * @param file 当前上传、预览或写入的文件。
   */
  const handleImportConfig = (file: UploadFile) => {
    if (!file.raw) {
      ElMessage.error('备份文件格式不正确')
      return
    }
    const reader = new FileReader()
    reader.onload = (e: ProgressEvent<FileReader>) => {
      try {
        if (typeof e.target?.result !== 'string')
          throw new Error('Invalid file')
        const importedConfig = parsePreferences(e.target.result, config.value)
        appStore.replaceConfig(importedConfig)

        ElMessage.success('设置备份已导入')
      } catch {
        ElMessage.error('备份文件格式不正确')
      }
    }
    reader.onerror = () => ElMessage.error('备份文件格式不正确')
    reader.readAsText(file.raw)
  }
</script>

<style scoped>
  .maintenance-actions {
    display: grid;
    grid-template-columns: 1fr 1fr;
    gap: 8px;
  }
  .maintenance-actions :deep(.el-button),
  .maintenance-actions :deep(.el-upload) {
    width: 100%;
    margin: 0;
  }
  .project-info {
    display: flex;
    gap: 12px;
    padding: 12px;
    border: 1px solid var(--app-border);
    border-radius: var(--ape-volo-radius);
  }
  .project-info img {
    flex: none;
    width: 44px;
    height: 44px;
    object-fit: contain;
  }
  .project-info strong {
    color: var(--app-text-primary);
    font-size: 13px;
  }
  .project-info p {
    margin: 4px 0 8px;
    color: var(--app-text-secondary);
    font-size: 11px;
    line-height: 1.5;
  }
  .project-links {
    display: flex;
    flex-wrap: wrap;
    gap: 10px;
  }
  .project-links a {
    color: var(--el-color-primary);
    font-size: 11px;
    font-weight: 600;
  }
  .preference-readonly {
    max-width: 160px;
    overflow: hidden;
    color: var(--app-text-secondary);
    font-size: 12px;
    text-overflow: ellipsis;
    white-space: nowrap;
  }
</style>
